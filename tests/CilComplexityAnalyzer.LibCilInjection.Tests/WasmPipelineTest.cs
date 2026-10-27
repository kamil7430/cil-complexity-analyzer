using System.Diagnostics;
using CilComplexityAnalyzer.TestExecutor;

namespace CilComplexityAnalyzer.LibCilInjection.Tests;

[TestClass]
public class WasmPipelineTest
{
    [TestMethod]
    public void InstrumentAndRunWasmApp()
    {
        string testsDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
        string appBundlePath = Path.Combine(testsDir, "StudentWasmApp", "bin", "Debug", "net10.0", "wasi-wasm", "AppBundle");

        Assert.IsTrue(Directory.Exists(appBundlePath), $"Nie znaleziono AppBundle pod ścieżką: {appBundlePath}. Zbuduj najpierw StudentWasmApp!");

        string managedDir = Path.Combine(appBundlePath, "managed");
        string studentDll = Path.Combine(managedDir, "StudentWasmApp.dll");
        string linqDll = Path.Combine(managedDir, "System.Linq.dll");

        Assert.IsTrue(File.Exists(studentDll), $"Brak StudentWasmApp.dll w {managedDir}");
        Assert.IsTrue(File.Exists(linqDll), $"Brak System.Linq.dll w {managedDir}");

        byte[] studentBytes = File.ReadAllBytes(studentDll);
        byte[] linqBytes = File.ReadAllBytes(linqDll);

        byte[] instrumentedStudent = CilInstructionInjector.InjectCilToAssemblyBytes(studentBytes);
        byte[] instrumentedLinq = CilInstructionInjector.InjectCilToAssemblyBytes(linqBytes);

        //File.WriteAllBytes(studentDll, studentBytes);
        //File.WriteAllBytes(linqDll, linqBytes);

        File.WriteAllBytes(studentDll, instrumentedStudent);
        File.WriteAllBytes(linqDll, instrumentedLinq);
        
        Console.WriteLine("[SUCCESS] Nadpisano pliki w AppBundle/managed/ zinstrumentowanym kodem CIL!");

        var startInfo = new ProcessStartInfo
        {
            FileName = "wasmtime",
            Arguments = "run -S http --dir . -- dotnet.wasm StudentWasmApp",
            WorkingDirectory = appBundlePath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(startInfo);
        Assert.IsNotNull(process, "Nie udało się uruchomić wasmtime.");

        process.WaitForExit(10000); // 10 sekund timeoutu

        string output = process.StandardOutput.ReadToEnd();
        string error = process.StandardError.ReadToEnd();

        Console.WriteLine("=== [WASM OUTPUT] ===");
        Console.WriteLine(output);

        if (!string.IsNullOrEmpty(error))
        {
            Console.WriteLine("=== [WASM ERROR] ===");
            Console.WriteLine(error);
        }

        Assert.IsTrue(output.Contains("[INSTRUCTION_COUNTER_RESULT]="), "Brak wyniku licznika w wyjściu z Wasmtime!");
    }

    [TestMethod]
    public void InstrumentAndRunWasmApp_Collections()
    {
        string testsDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
        string appBundlePath = Path.Combine(testsDir, "StudentWasmApp", "bin", "Debug", "net10.0", "wasi-wasm", "AppBundle");

        string managedDir = Path.Combine(appBundlePath, "managed");
        string studentDll = Path.Combine(managedDir, "StudentWasmApp.dll");
        string collectionsDll = Path.Combine(managedDir, "System.Collections.dll");
        string coreLibDll = Path.Combine(managedDir, "System.Private.CoreLib.dll");

        byte[] studentBytes = File.ReadAllBytes(studentDll);
        byte[] collectionsBytes = File.ReadAllBytes(collectionsDll);
        byte[] coreLibBytes = File.ReadAllBytes(coreLibDll);

        byte[] instrumentedStudent = CilInstructionInjector.InjectCilToAssemblyBytes(studentBytes);
        byte[] instrumentedCollections = CilInstructionInjector.InjectCilToAssemblyBytes(collectionsBytes);
        byte[] instrumentedCoreLib = CilInstructionInjector.InjectCilToAssemblyBytes(coreLibBytes);

        File.WriteAllBytes(studentDll, instrumentedStudent);
        File.WriteAllBytes(collectionsDll, instrumentedCollections);
        File.WriteAllBytes(coreLibDll, instrumentedCoreLib);

        var startInfo = new ProcessStartInfo
        {
            FileName = "wasmtime",
            Arguments = "run -S http --dir . -- dotnet.wasm StudentWasmApp collections", // wazne
            WorkingDirectory = appBundlePath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(startInfo);
        process!.WaitForExit(10000);

        string output = process.StandardOutput.ReadToEnd();
        Console.WriteLine("=== [WASM COLLECTIONS OUTPUT] ===");
        Console.WriteLine(output);

        Assert.IsTrue(output.Contains("[INSTRUCTION_COUNTER_RESULT]="));
    }   
    private static byte[] InjectLinq(byte[] bytes) => CilInstructionInjector.InjectCilToAssemblyBytes(bytes);
}