using System.Runtime.InteropServices;
using ASD.Graphs;
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
        int mixedModeRestrictedCount = 0;
        int notFoundCount = 0;

        Console.WriteLine("=== PEŁNY AUDYT WSTRZYKIWANIA CIL DLA WSZYSTKICH BIBLIOTEK ===");

        foreach (var (libName, priority) in priorityList.OrderByDescending(x => x.Item2))
        {
            string? dllPath = FindAssemblyPath(libName);

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
            catch (NotSupportedException ex) when (ex.Message.Contains("mixed-mode") || ex.Message.Contains("ReadyToRun"))
            {
                Console.WriteLine($"[RESTRICTED / BCL R2R] {libName} (Priorytet: {priority}) -> Pominięto (zoptymalizowana natywnie R2R).");
                mixedModeRestrictedCount++;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] {libName} (Priorytet: {priority}) -> Błąd: {ex.Message}");
            }
        }

        Console.WriteLine("=== PODSUMOWANIE AUDYTU ===");
        Console.WriteLine($"Sukcesy (CIL wstrzyknięty): {successCount}");
        Console.WriteLine($"Ograniczenia systemowe (R2R/mixed-mode): {mixedModeRestrictedCount}");
        Console.WriteLine($"Nie znaleziono fizycznie: {notFoundCount}");

        Assert.IsTrue(successCount > 0, "Przynajmniej część bibliotek powinna dać się zainstrumentować.");
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
        var loadedAsm = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(a => string.Equals(a.GetName().Name, libName, StringComparison.OrdinalIgnoreCase) ||
                                 a.GetName().Name?.StartsWith(libName, StringComparison.OrdinalIgnoreCase) == true);

        if (loadedAsm != null && !string.IsNullOrEmpty(loadedAsm.Location) && File.Exists(loadedAsm.Location))
        {
            return loadedAsm.Location;
        }

        string? runtimeDirectory = RuntimeEnvironment.GetRuntimeDirectory();
        if (!string.IsNullOrEmpty(runtimeDirectory))
        {
            string candidatePath = Path.Combine(runtimeDirectory, $"{libName}.dll");
            if (File.Exists(candidatePath))
            {
                return candidatePath;
            }
        }

        return null;
    }
}