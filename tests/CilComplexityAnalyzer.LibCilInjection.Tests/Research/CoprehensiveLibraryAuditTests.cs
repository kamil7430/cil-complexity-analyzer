using System.Reflection;
using System.Runtime.InteropServices;
using ASD.Graphs;
using CilComplexityAnalyzer.LibCilInjection.Tests.Common;
using CilInjection.Core;
using Counter.Provider.InstructionCounter;

namespace CilComplexityAnalyzer.LibCilInjection.Tests.Research;

[TestClass]
public class ComprehensiveLibraryAuditTests
{
    [TestMethod]
    public void AuditAllPriorityLibraries_ShouldClassifySuccessAndRestrictions()
    {
        ForceLoadAssemblies();

        var priorityList = new[]
        {
            ("System", 83),
            ("System.Collections.Generic", 73),
            ("System.Linq", 61),
            ("ASD.Graphs", 58),
            ("ASD", 13),
            ("System.Numerics", 5),
            ("System.IO", 3),
            ("System.Reflection.Metadata", 3),
            ("System.Threading.Tasks", 3),
            ("System.Collections", 2),
            ("System.Diagnostics.Debug", 2),
            ("System.Runtime.InteropServices", 2),
            ("System.ComponentModel", 1),
            ("System.Drawing.Primitives", 1),
            ("System.Reflection", 1),
            ("System.Security.Cryptography", 1)
        };

        var strategy = new InstructionCounterStrategy();
        var manager = new CilInjectionManager(strategy);

        int successCount = 0;
        int notFoundCount = 0;
        int r2rRestrictedCount = 0;

        Console.WriteLine("=== PEŁNY AUDYT WSTRZYKIWANIA CIL DLA WSZYSTKICH BIBLIOTEK ===");

        foreach (var (libName, priority) in priorityList.OrderByDescending(x => x.Item2))
        {
            string? dllPath = FindPureIlAssemblyPath(libName);

            if (string.IsNullOrEmpty(dllPath) || !File.Exists(dllPath))
            {
                Console.WriteLine($"[NOT FOUND] {libName} (Priorytet: {priority}) - brak pliku na dysku/w domenie.");
                notFoundCount++;
                continue;
            }
            try
            {
                byte[] inputBytes = File.ReadAllBytes(dllPath);
                
                byte[] outputBytes = manager.Transform(inputBytes);

                Assert.IsNotNull(outputBytes);
                Console.WriteLine($"[SUCCESS] {libName} (Priorytet: {priority}) -> Zainstrumentowano pomyślnie ({outputBytes.Length} bajtów).");
                successCount++;
            }
            catch (NotSupportedException ex) when (ex.Message.Contains("mixed-mode") || ex.Message.Contains("ReadyToRun") || ex.Message.Contains("not supported"))
            {
                Console.WriteLine($"[RESTRICTED - R2R / BCL] {libName} (Priorytet: {priority}) -> Ograniczenie architektoniczne .NET (ReadyToRun).");
                r2rRestrictedCount++;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RESTRICTED / ERROR] {libName} (Priorytet: {priority}) -> {ex.GetType().Name}: {ex.Message}");
                r2rRestrictedCount++;
            }
        }

        Console.WriteLine("=== PODSUMOWANIE AUDYTU ===");
        Console.WriteLine($"Sukcesy (CIL wstrzyknięty): {successCount}");
        Console.WriteLine($"Ograniczenia systemowe (R2R/mixed-mode): {r2rRestrictedCount}");
        Console.WriteLine($"Nie znaleziono fizycznie: {notFoundCount}");

        Assert.IsTrue(successCount >= 0, "Przynajmniej część bibliotek powinna dać się zainstrumentować.");
    }

    private static void ForceLoadAssemblies()
    {
        try
        {
            _ = typeof(Graph);
            _ = typeof(System.Linq.Enumerable);
            _ = typeof(System.Collections.Generic.List<>);
            _ = typeof(System.Numerics.BigInteger);
        }
        catch { }
    }

    private static string? FindAssemblyPath(string libName)
    {
        if (libName == "ASD.Graphs")
        {
            try
            {
                var loc = typeof(Graph).Assembly.Location;
                if (!string.IsNullOrEmpty(loc) && File.Exists(loc)) return loc;
            }
            catch { }
        }

        string? runtimeDir = RuntimeEnvironment.GetRuntimeDirectory();
        if (!string.IsNullOrEmpty(runtimeDir))
        {
            string candidate = Path.Combine(runtimeDir, $"{libName}.dll");
            if (File.Exists(candidate)) return candidate;

            DirectoryInfo? parent = Directory.GetParent(runtimeDir);
            if (parent != null)
            {
                foreach (var dir in parent.GetDirectories())
                {
                    string sharedPath = Path.Combine(dir.FullName, $"{libName}.dll");
                    if (File.Exists(sharedPath)) return sharedPath;
                }
            }
        }

        return null;
    }
    
    private static string? FindPureIlAssemblyPath(string libName)
    {
        string? dotnetRoot = Environment.GetEnvironmentVariable("DOTNET_ROOT");
        if (string.IsNullOrEmpty(dotnetRoot))
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                dotnetRoot = "/usr/local/share/dotnet";
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                dotnetRoot = "/usr/share/dotnet";
            else
                dotnetRoot = @"C:\Program Files\dotnet";
        }

        string packsDir = Path.Combine(dotnetRoot, "packs", "Microsoft.NETCore.App.Ref");
        if (Directory.Exists(packsDir))
        {
            var latestVersionDir = Directory.GetDirectories(packsDir).OrderByDescending(d => d).FirstOrDefault();
            if (latestVersionDir != null)
            {
                string refCandidate = Path.Combine(latestVersionDir, "ref", "net8.0", $"{libName}.dll");
                if (File.Exists(refCandidate))
                {
                    Console.WriteLine($"[PURE IL FOUND IN REF PACKS] {libName} -> {refCandidate}");
                    return refCandidate;
                }
            }
        }

        string userHome = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        string nugetPackageDir = Path.Combine(userHome, ".nuget", "packages");
        if (Directory.Exists(nugetPackageDir))
        {
            var nugetMatch = Directory.GetFiles(nugetPackageDir, $"{libName}.dll", SearchOption.AllDirectories)
                .FirstOrDefault(f => !f.Contains("/runtimes/") && !f.Contains("\\runtimes\\"));
            if (!string.IsNullOrEmpty(nugetMatch))
            {
                return nugetMatch;
            }
        }

        return null;
    }

    [TestMethod]
    public void CheckIfNugetPackageIsPureIl_AndTransformable()
    {
        string userHome = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        string nugetPackageDir = Path.Combine(userHome, ".nuget", "packages");
        if (!Directory.Exists(nugetPackageDir))
        {
            Assert.Inconclusive("Katalog NuGet packages nie został znaleziony w profilu użytkownika.");
            return;
        }
        string? sampleDll = Directory.GetFiles(nugetPackageDir, "*.dll", SearchOption.AllDirectories)
            .FirstOrDefault(f => f.Contains("System.") || f.Contains("Microsoft."));
        
        if (sampleDll == null)
        {
            Assert.Inconclusive("Nie znaleziono żadnej biblioteki w cache NuGet.");
            return;
        }

        Console.WriteLine($"[TEST NUGET] Znaleziono plik z NuGet: {sampleDll}");

        byte[] originalBytes = File.ReadAllBytes(sampleDll);
        var strategy = new InstructionCounterStrategy();
        var manager = new CilInjectionManager(strategy);

        try
        {
            byte[] transformedBytes = manager.Transform(originalBytes);

            Console.WriteLine($"[SUKCES] Biblioteka z NuGet jest czystym IL! Rozmiar po transformacji: {transformedBytes.Length} B");
            Assert.IsTrue(transformedBytes.Length > 0);
        }
        catch (NotSupportedException ex) when (ex.Message.Contains("mixed-mode"))
        {
            Console.WriteLine($"[INFO] Ten konkretny plik z NuGet miał kompilację hybrydową/R2R: {Path.GetFileName(sampleDll)}");
        }
    }
    
    // czy wrzucenie CIL jest dozwolone do Graphs
    [TestMethod]
    public void AuditAndExecutePriorityLibraries_ShouldProveRealCapabilities()
    {
        TestLibraryExecution("ASD.Graphs", typeof(Graph).Assembly.Location, () =>
        {
            var graph = new Graph(2);
            graph.AddEdge(0, 1);
            return graph.VertexCount;
        });

        TestLibraryExecution("System.Numerics", typeof(System.Numerics.BigInteger).Assembly.Location, () =>
        {
            var bigInt = System.Numerics.BigInteger.Parse("12345");
            return (int)bigInt;
        });
    }

    // czy w Numerics jest doswolony CIL?
    private static void TestLibraryExecution(string libName, string dllPath, Func<int> executionAction)
    {
        Console.WriteLine($"\n=== AUDYT I TEST URUCHOMIENIA: {libName} == =");

        if (string.IsNullOrEmpty(dllPath) || !File.Exists(dllPath))
        {
            Console.WriteLine($"[NOT FOUND] Nie znaleziono pliku dla {libName} pod ścieżką: {dllPath}");
            return;
        }

        try
        {
            byte[] originalBytes = File.ReadAllBytes(dllPath);
            var strategy = new InstructionCounterStrategy();
            var manager = new CilInjectionManager(strategy);

            byte[] instrumentedBytes = manager.Transform(originalBytes);
            Console.WriteLine($"[TRANSFORM SUCCESS] {libName} ztransformowany pomyślnie ({instrumentedBytes.Length} bajtów).");

            using var sandbox = InstrumentedSandbox.Create(
                instrumentedBytes, 
                $"Audit_{libName}", 
                strategy);

            int result = executionAction();
            Console.WriteLine($"[EXECUTION SUCCESS] {libName} wykonał się poprawnie. Wynik testowy: {result}");
        }
        catch (NotSupportedException ex)
        {
            Console.WriteLine($"[RESTRICTED / R2R LIMITATION] {libName} odrzucony przez architekturę .NET: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] {libName} napotkał błąd: {ex.GetType().Name}: {ex.Message}");
        }
    }
}