using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using CilComplexityAnalyzer.TestExecutor;

namespace CilComplexityAnalyzer.LibCilInjection.Tests;

[TestClass]
[DoNotParallelize]
public class WasmPipelineTest
{
    private const string CoreLib = "System.Private.CoreLib.dll";

    private static readonly TimeSpan WasmTimeout = TimeSpan.FromSeconds(120);

    private static readonly string[] ExtraRoots = { "System.Array", "System.SpanHelpers" };

    [TestMethod]
    public void InstrumentAndRunWasmApp()
        => RunWasmTest(null, expectedResults: 4, CoreLib, "System.Linq.dll");

    [TestMethod]
    public void InstrumentAndRunWasmApp_Collections()
        => RunWasmTest("collections", expectedResults: 3, CoreLib, "System.Collections.dll");

    [TestMethod]
    public void ComplexityScaling_Collections()
    {
        string output = RunWasmTest("scaling", expectedResults: 0, CoreLib, "System.Collections.dll");

        var data = Regex.Matches(output, @"\[SCALING\] (\w+) n=(\d+) count=(\d+)")
            .GroupBy(m => m.Groups[1].Value)
            .ToDictionary(
                g => g.Key,
                g => g.Select(m => (n: int.Parse(m.Groups[2].Value), count: long.Parse(m.Groups[3].Value))).ToList());

        Assert.AreEqual(5, data.Count, "Oczekiwano 5 algorytmów w wyjściu");

        foreach (var (name, points) in data)
            Console.WriteLine($"[SCALING] {name}: {string.Join(", ", points.Select(p => $"n={p.n}->{p.count}"))}");

        foreach (var (name, points) in data)
        {
            var at200 = points.Where(p => p.n == 200).Select(p => p.count).ToList();
            Assert.AreEqual(2, at200.Count, $"{name}: brak dwóch pomiarów dla n=200");
            Assert.AreEqual(at200[0], at200[1], $"{name}: wynik nie jest deterministyczny");
        }

        double Ratio(string name) =>
            (double)data[name].First(p => p.n == 800).count / data[name].First(p => p.n == 200).count;

        double contains = Ratio("ListContains"), loop = Ratio("ListLoop"), hash = Ratio("HashSetContains");
        Console.WriteLine($"[RATIO 200->800] ListContains={contains:F2}, ListLoop={loop:F2}, HashSetContains={hash:F2}");

        Assert.IsTrue(contains is > 3.0 and < 4.5, $"List.Contains nie wygląda na O(n): ×{contains:F2}");
        Assert.IsTrue(loop is > 3.0 and < 4.5, $"Pętla po liście nie wygląda na O(n): ×{loop:F2}");
        Assert.IsTrue(hash < 1.5, $"HashSet.Contains nie wygląda na O(1): ×{hash:F2}");
        
        double dictLookup = Ratio("DictionaryLookup"), dictFill = Ratio("DictionaryFill");
        Console.WriteLine($"[RATIO 200->800] DictionaryLookup={dictLookup:F2}, DictionaryFill={dictFill:F2}");

        Assert.IsTrue(dictLookup < 1.1, $"Dictionary.TryGetValue nie wygląda na O(1): ×{dictLookup:F2}");
        Assert.IsTrue(dictFill is > 3.0 and < 4.5, $"Wypełnianie Dictionary nie wygląda na O(n): ×{dictFill:F2}");
    }
    
    private string RunWasmTest(string? cliArgument, int expectedResults, params string[] librariesToInstrument)
    {
        string testsDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
        string appBundlePath = Path.Combine(testsDir, "StudentWasmApp", "bin", "Debug", "net10.0", "wasi-wasm", "AppBundle");

        Assert.IsTrue(Directory.Exists(appBundlePath), $"Nie znaleziono AppBundle: {appBundlePath}. Zbuduj najpierw StudentWasmApp!");

        string workDir = Path.Combine(Path.GetTempPath(), "cil-wasm-run", Guid.NewGuid().ToString("N"));
        CopyDirectory(appBundlePath, workDir);
        bool success = false;

        try
        {
            string managedDir = Path.Combine(workDir, "managed");
            string studentDll = Path.Combine(managedDir, "StudentWasmApp.dll");
            Assert.IsTrue(File.Exists(studentDll), $"Brak StudentWasmApp.dll w {managedDir}");

            File.WriteAllBytes(studentDll, CilInstructionInjector.InjectCilToAssemblyBytes(File.ReadAllBytes(studentDll)));

            foreach (var lib in librariesToInstrument)
            {
                string path = Path.Combine(managedDir, lib);
                Assert.IsTrue(File.Exists(path), $"Brak {lib} w {managedDir}");
                File.WriteAllBytes(path, InstrumentCached(File.ReadAllBytes(path)));
            }

            Console.WriteLine($"[SUCCESS] Zinstrumentowana kopia bundla: {workDir}");

            string args = string.IsNullOrEmpty(cliArgument)
                ? "run -S http --dir . -- dotnet.wasm StudentWasmApp"
                : $"run -S http --dir . -- dotnet.wasm StudentWasmApp {cliArgument}";

            var startInfo = new ProcessStartInfo
            {
                FileName = "wasmtime",
                Arguments = args,
                WorkingDirectory = workDir,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo)!;
            var outputTask = process.StandardOutput.ReadToEndAsync();
            var errorTask = process.StandardError.ReadToEndAsync();

            if (!process.WaitForExit((int)WasmTimeout.TotalMilliseconds))
            {
                process.Kill(entireProcessTree: true);
                Assert.Fail($"wasmtime przekroczył {WasmTimeout.TotalSeconds}s");
            }

            string output = outputTask.GetAwaiter().GetResult();
            string error = errorTask.GetAwaiter().GetResult();

            Console.WriteLine("=== [WASM OUTPUT] ===");
            Console.WriteLine(output);
            if (!string.IsNullOrEmpty(error))
            {
                Console.WriteLine("=== [WASM ERROR] ===");
                Console.WriteLine(error);
            }

            Assert.IsFalse(error.Contains("call stack exhausted"), "Wasm: call stack exhausted (zapętlenie w CoreLib?)");
            Assert.IsFalse(output.Contains("Unhandled") || error.Contains("Unhandled"), "Nieobsłużony wyjątek w aplikacji WASM");

            var counts = Regex.Matches(output, @"\[INSTRUCTION_COUNTER_RESULT\]=(\d+)")
                              .Select(m => long.Parse(m.Groups[1].Value))
                              .ToList();

            Assert.AreEqual(expectedResults, counts.Count, "Niepoprawna liczba wyników licznika");
            Assert.IsTrue(counts.All(c => c > 0), $"Licznik zwrócił 0: [{string.Join(", ", counts)}]");

            Console.WriteLine($"[RESULTS] {string.Join(", ", counts)}");
            success = true;
            return output;
        }
        finally
        {
            if (success) TryDelete(workDir);
            else Console.WriteLine($"[DEBUG] Katalog zachowany do analizy: {workDir}");
        }
    }

    private static byte[] InstrumentCached(byte[] originalBytes)
    {
        string cacheDir = Path.Combine(Path.GetTempPath(), "cil-instrumented-cache");
        Directory.CreateDirectory(cacheDir);

        string injectorId = typeof(CilInstructionInjector).Assembly.ManifestModule.ModuleVersionId.ToString("N");
        string parameters = string.Join(",", ExtraRoots);

        var hash = SHA256.HashData(
            originalBytes
                .Concat(System.Text.Encoding.UTF8.GetBytes(injectorId + "|" + parameters))
                .ToArray());

        string cached = Path.Combine(cacheDir, $"{Convert.ToHexString(hash)}.dll");

        if (File.Exists(cached))
        {
            Console.WriteLine($"[CACHE] trafienie: {Path.GetFileName(cached)}");
            return File.ReadAllBytes(cached);
        }

        var instrumented = CilInstructionInjector.InjectCilToAssemblyBytes(originalBytes, extraRoots: ExtraRoots);
        File.WriteAllBytes(cached, instrumented);
        return instrumented;
    }

    private static void CopyDirectory(string source, string target)
    {
        Directory.CreateDirectory(target);
        foreach (var dir in Directory.GetDirectories(source, "*", SearchOption.AllDirectories))
            Directory.CreateDirectory(dir.Replace(source, target));
        foreach (var file in Directory.GetFiles(source, "*", SearchOption.AllDirectories))
            File.Copy(file, file.Replace(source, target), overwrite: true);
    }

    private static void TryDelete(string dir)
    {
        try { Directory.Delete(dir, recursive: true); } catch { /* nieistotne */ }
    }
}