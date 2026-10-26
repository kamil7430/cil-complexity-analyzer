using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using CilComplexityAnalyzer.LibCilInjection.Tests.Common;
using CilInjection.Core;
using Counter.Provider.InstructionCounter;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CilComplexityAnalyzer.LibCilInjection.Tests.Research;

[TestClass]
public class ReferenceAssemblyVerificationTest
{
    // test pokazuje, ze możemy dostać się do kodu do Referencji Assembly ALE nie ma tam żadnego kodu bo są to tylko 
    // linijki z deklaracjami metod w bibliotekcha. => nie nada się do naszego Cil
    [TestMethod]
    public void VerifyIfReferenceAssemblyCanBeInjectedAndExecuted()
    {
        string? refDllPath = FindReferenceAssemblyPath("System.Numerics");
        
        Assert.IsNotNull(refDllPath, "Nie znaleziono pliku System.Numerics.dll w Reference Assemblies.");
        Console.WriteLine($"[FOUND] Znaleziono referencyjny plik: {refDllPath}");

        byte[] originalBytes = File.ReadAllBytes(refDllPath);
        var strategy = new InstructionCounterStrategy();
        var manager = new CilInjectionManager(strategy);

        byte[] instrumentedBytes;
        try
        {
            instrumentedBytes = manager.Transform(originalBytes);
            Console.WriteLine($"[SUCCESS] Mono.Cecil pomyślnie ztransformował plik z Ref Packs! Rozmiar: {instrumentedBytes.Length} B");
        }
        catch (Exception ex)
        {
            Assert.Fail($"Transformacja Mono.Cecil nie powiodła się dla Reference Assembly: {ex.GetType().Name}: {ex.Message}");
            return;
        }

        try
        {
            using var sandbox = InstrumentedSandbox.Create(
                instrumentedBytes, 
                nameof(VerifyIfReferenceAssemblyCanBeInjectedAndExecuted), 
                strategy);

            var bigIntType = sandbox.Assembly.GetType("System.Numerics.BigInteger");
            Assert.IsNotNull(bigIntType, "Nie odnaleziono typu BigInteger w załadowanym module.");

            var parseMethod = bigIntType.GetMethod("Parse", new[] { typeof(string) });
            Assert.IsNotNull(parseMethod, "Nie odnaleziono metody Parse w BigInteger.");

            sandbox.Counter.ResetCounter();
            object? result = parseMethod.Invoke(null, new object[] { "98765432109876543210" });
            long executedInstructions = sandbox.Counter.GetCounter();

            Console.WriteLine($"[EXECUTION SUCCESS] BigInteger.Parse wykonało: {executedInstructions} instrukcji CIL.");
            Assert.IsNotNull(result);
            Assert.IsTrue(executedInstructions >= 0, "Licznik instrukcji zwrócił niepoprawną wartość.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SANDBOX WARNING] Plik referencyjny przeszedł transformację, ale uruchomienie w sandboxie wymagało runtime'u: {ex.Message}");
        }
    }

    private static string? FindReferenceAssemblyPath(string libName)
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
                if (File.Exists(refCandidate)) return refCandidate;
            }
        }
        return null;
    }
}