using System;
using System.Diagnostics;
using System.IO;
using CilComplexityAnalyzer.TestExecutor;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CilComplexityAnalyzer.LibCilInjection.Tests.Research;

[TestClass]
public class WasmPipelineE2ETests
{
    [TestMethod]
    public void WasmAppBundle_InjectSystemLinq_ShouldSucceedAndExecute()
    {
        string testsDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
        string originalAppBundlePath = Path.Combine(testsDir, "StudentWasmApp", "bin", "Debug", "net10.0", "wasi-wasm", "AppBundle");

        Assert.IsTrue(Directory.Exists(originalAppBundlePath), $"Brakuje folderu AppBundle. Zbuduj najpierw StudentWasmApp! Szukano w: {originalAppBundlePath}");

        string tempAppBundlePath = Path.Combine(Path.GetTempPath(), $"WasmTest_{Guid.NewGuid():N}");
        CopyDirectory(originalAppBundlePath, tempAppBundlePath);
        
        string managedDir = Path.Combine(tempAppBundlePath, "managed");
        string studentDllPath = Path.Combine(managedDir, "StudentWasmApp.dll");
        string linqDllPath = Path.Combine(managedDir, "System.Linq.dll");

        Assert.IsTrue(File.Exists(studentDllPath), "Brakuje StudentWasmApp.dll w zarządzanym folderze WASM.");
        Assert.IsTrue(File.Exists(linqDllPath), "Brakuje System.Linq.dll w zarządzanym folderze WASM.");
        
        byte[] studentOriginalBytes = File.ReadAllBytes(studentDllPath);
        byte[] linqOriginalBytes = File.ReadAllBytes(linqDllPath);

        byte[] studentInstrumentedBytes;
        byte[] linqInstrumentedBytes;

        try
        {
            //TODO zaminiec na faktyczny kod nowego CilInjectora
            studentInstrumentedBytes = CilInstructionInjector.InjectCilToAssemblyBytes(studentOriginalBytes, true);
            linqInstrumentedBytes = CilInstructionInjector.InjectCilToAssemblyBytes(linqOriginalBytes, false);
        }
        catch (Exception ex)
        {
            Assert.Fail($"Transformacja CIL niespodziewanie rzuciła błąd: {ex}");
            return;
        }

        File.WriteAllBytes(studentDllPath, studentInstrumentedBytes);
        File.WriteAllBytes(linqDllPath, linqInstrumentedBytes);

        Console.WriteLine($"[SUCCESS] StudentWasmApp.dll zinstrumentowany: {studentOriginalBytes.Length}B -> {studentInstrumentedBytes.Length}B");
        Console.WriteLine($"[SUCCESS] System.Linq.dll zinstrumentowany: {linqOriginalBytes.Length}B -> {linqInstrumentedBytes.Length}B");
        
        Console.WriteLine($"[WASM HOST] Uruchamianie zewnętrznego hosta Wasmtime na zmodyfikowanych plikach...");
        
        var startInfo = new ProcessStartInfo
        {
            FileName = "wasmtime",
            Arguments = "run -S http --dir . -- dotnet.wasm StudentWasmApp",
            WorkingDirectory = tempAppBundlePath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(startInfo);
        Assert.IsNotNull(process, "Nie udało się uruchomić wasmtime. Upewnij się, że jest dodany do PATH.");

        process.WaitForExit(10000); //dlugo 
        
        string output = process.StandardOutput.ReadToEnd();
        string error = process.StandardError.ReadToEnd();

        Console.WriteLine("[WASM OUTPUT]");
        Console.WriteLine(output);
        
        if (!string.IsNullOrEmpty(error))
        {
            Console.WriteLine("[WASM ERROR LOGS]");
            Console.WriteLine(error);
        }

        Assert.IsTrue(output.Contains("Znaleziono 50 liczb parzystych. Suma: 2550"), "Maszyna WebAssembly nie zwróciła poprawnego wyniku z LINQ!");
        Assert.AreEqual(0, process.ExitCode, "Proces wasmtime zakończył się błędem!");
        
        Directory.Delete(tempAppBundlePath, true);
    }

    private static void CopyDirectory(string sourceDir, string targetDir)
    {
        Directory.CreateDirectory(targetDir);
        foreach (var file in Directory.GetFiles(sourceDir))
            File.Copy(file, Path.Combine(targetDir, Path.GetFileName(file)));
        foreach (var dir in Directory.GetDirectories(sourceDir))
            CopyDirectory(dir, Path.Combine(targetDir, Path.GetFileName(dir)));
    }
}