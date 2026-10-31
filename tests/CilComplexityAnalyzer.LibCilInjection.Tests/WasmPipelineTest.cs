using System.Diagnostics;
using CilComplexityAnalyzer.TestExecutor;

namespace CilComplexityAnalyzer.LibCilInjection.Tests;

[TestClass]
[DoNotParallelize]
public class WasmPipelineTest
{
    [TestMethod]
    public void InstrumentAndRunWasmApp()
    {
        RunWasmTest(null, "System.Linq.dll");
    }

    [TestMethod]
    public void InstrumentAndRunWasmApp_Collections()
    {
        RunWasmTest("collections","System.Private.CoreLib.dll", "System.Collections.dll");
    }  
    
    private void RunWasmTest(string? cliArgument, params string[] additionalAssembliesToInstrument)
    {
        string testsDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
        string appBundlePath = Path.Combine(testsDir, "StudentWasmApp", "bin", "Debug", "net10.0", "wasi-wasm", "AppBundle");

        Assert.IsTrue(Directory.Exists(appBundlePath), $"Nie znaleziono AppBundle pod ścieżką: {appBundlePath}. Zbuduj najpierw StudentWasmApp!");

        string managedDir = Path.Combine(appBundlePath, "managed");
        string studentDll = Path.Combine(managedDir, "StudentWasmApp.dll");

        Assert.IsTrue(File.Exists(studentDll), $"Brak StudentWasmApp.dll w {managedDir}");

        byte[] studentBytes = File.ReadAllBytes(studentDll);
        byte[] instrumentedStudent = CilInstructionInjector.InjectCilToAssemblyBytes(studentBytes, true);
        File.WriteAllBytes(studentDll, instrumentedStudent);

        foreach (var assemblyName in additionalAssembliesToInstrument)
        {
            string dllPath = Path.Combine(managedDir, assemblyName);
            if (File.Exists(dllPath))
            {
                byte[] bytes = File.ReadAllBytes(dllPath);
                byte[] instrumentedBytes = CilInstructionInjector.InjectCilToAssemblyBytes(bytes, false);
                File.WriteAllBytes(dllPath, instrumentedBytes);
            }
        }

        Console.WriteLine("[SUCCESS] Nadpisano pliki w AppBundle/managed/ zinstrumentowanym kodem CIL!");

        string args = string.IsNullOrEmpty(cliArgument)
            ? "run -S http --dir . -- dotnet.wasm StudentWasmApp"
            : $"run -S http --dir . -- dotnet.wasm StudentWasmApp {cliArgument}";

        var startInfo = new ProcessStartInfo
        {
            FileName = "wasmtime",
            Arguments = args,
            WorkingDirectory = appBundlePath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process();
        process.StartInfo = startInfo;

        process.Start();
        
        var outputTask = process.StandardOutput.ReadToEndAsync();
        var errorTask = process.StandardError.ReadToEndAsync();

        bool exited = process.WaitForExit(10000);
        if (!exited)
        {
            process.Kill();
            Assert.Fail("Proces wasmtime przekoczyl zalozone 10s czasu ");
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

        Assert.IsTrue(output.Contains("[INSTRUCTION_COUNTER_RESULT]="), "Brak wyniku licznika w wyjściu z Wasmtime!");
    }
}