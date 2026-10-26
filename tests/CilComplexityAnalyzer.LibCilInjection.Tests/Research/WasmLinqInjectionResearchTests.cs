using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using CilComplexityAnalyzer.TestExecutor;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CilComplexityAnalyzer.LibCilInjection.Tests.Research;

[TestClass]
public class WasmAndLinqInjectionResearchTests
{
    [TestMethod]
    public void Test_InjectCilToSystemLinq_AndExecuteQuery()
    {
        string? dotnetRoot = Environment.GetEnvironmentVariable("DOTNET_ROOT");
        if (string.IsNullOrEmpty(dotnetRoot))
        {
            if (Directory.Exists("/usr/local/share/dotnet"))
                dotnetRoot = "/usr/local/share/dotnet";
            else if (Directory.Exists("/opt/homebrew/share/dotnet"))
                dotnetRoot = "/opt/homebrew/share/dotnet";
            else
                dotnetRoot = "/usr/local/share/dotnet";
        }

        string packsDir = Path.Combine(dotnetRoot, "packs", "Microsoft.NETCore.App.Ref");
        string? targetDll = null;

        if (Directory.Exists(packsDir))
        {
            var latestVersionDir = Directory.GetDirectories(packsDir).OrderByDescending(d => d).FirstOrDefault();
            if (latestVersionDir != null)
            {
                string candidate = Path.Combine(latestVersionDir, "ref", "net10.0", "System.Linq.dll");
                if (File.Exists(candidate))
                {
                    targetDll = candidate;
                }
                else
                {
                    var refDir = Path.Combine(latestVersionDir, "ref");
                    if (Directory.Exists(refDir))
                    {
                        var subDirs = Directory.GetDirectories(refDir);
                        var net10SubDir = subDirs.FirstOrDefault(s => s.Contains("net10.0")) ?? subDirs.FirstOrDefault();
                        if (net10SubDir != null)
                        {
                            string altCandidate = Path.Combine(net10SubDir, "System.Linq.dll");
                            if (File.Exists(altCandidate)) targetDll = altCandidate;
                        }
                    }
                }
            }
        }

        if (string.IsNullOrEmpty(targetDll))
        {
            string userHome = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            targetDll = Directory.GetFiles(Path.Combine(userHome, ".nuget", "packages"), "System.Linq.dll", SearchOption.AllDirectories)
                .FirstOrDefault(f => !f.Contains("/runtimes/") && !f.Contains("\\runtimes\\"));
        }

        Assert.IsNotNull(targetDll, "Nie odnaleziono czystego pliku System.Linq.dll na macOS.");
        Console.WriteLine($"[RESEARCH TARGET] Znaleziono plik na macOS: {targetDll}");

        byte[] originalBytes = File.ReadAllBytes(targetDll);

        byte[] instrumentedBytes;
        try
        {
            instrumentedBytes = CilInstructionInjector.InjectCilToAssemblyBytes(originalBytes);
            Console.WriteLine($"[SUCCESS] Twój wstrzykiwacz pomyślnie przetworzył System.Linq na macOS! Rozmiar po transformacji: {instrumentedBytes.Length} B");
        }
        catch (Exception ex)
        {
            Assert.Fail($"Wstrzykiwanie CIL przez CilInstructionInjector nie powiodło się: {ex.GetType().Name}: {ex.Message}");
            return;
        }

        Assert.IsNotNull(instrumentedBytes);
        Assert.IsTrue(instrumentedBytes.Length > 0);
        
        var context = new System.Runtime.Loader.AssemblyLoadContext("ResearchContext", isCollectible: true);
        using var ms = new MemoryStream(instrumentedBytes);
        var loadedAssembly = context.LoadFromStream(ms);

        var containerType = loadedAssembly.GetType("<GlobalCounterContainer>");
        Assert.IsNotNull(containerType, "Nie znaleziono wstrzykniętej klasy <GlobalCounterContainer>.");
        
        var resetMethod = containerType.GetMethod("ResetInstructionCount");
        var getCountMethod = containerType.GetMethod("GetInstructionCount");
        Assert.IsNotNull(resetMethod);
        Assert.IsNotNull(getCountMethod);

        resetMethod.Invoke(null, null);

        var enumerableType = loadedAssembly.GetType("System.Linq.Enumerable");
        Assert.IsNotNull(enumerableType, "Nie znaleziono typu Enumerable.");
        
        var rangeMethod = enumerableType.GetMethod("Range");
        Assert.IsNotNull(rangeMethod, "Nie znaleziono metody Range.");

        Console.WriteLine("[EXECUTE] Próba wywołania Enumerable.Range(1, 10) z Twojego pliku...");
        try
        {
            rangeMethod.Invoke(null, new object[] { 1, 10 });
            Console.WriteLine("[INFO] Wywołanie zakończone bez wyjątku.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[INFO] Wywołanie przerwał błąd (to normalne w plikach .Ref!): {ex.InnerException?.GetType().Name} - {ex.InnerException?.Message}");
        }

        long executedInstructions = (long)getCountMethod.Invoke(null, null);
        Console.WriteLine($"[EXECUTION RESULT] Liczba zarejestrowanych instrukcji w System.Linq: {executedInstructions}");

        context.Unload();
    }
}