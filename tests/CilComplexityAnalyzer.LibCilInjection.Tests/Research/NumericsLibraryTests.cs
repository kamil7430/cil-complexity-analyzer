using System.Diagnostics;
using System.Numerics;
using CilComplexityAnalyzer.LibCilInjection.Tests.Common;
using CilInjection.Core;
using Counter.Provider.InstructionCounter;

namespace CilComplexityAnalyzer.LibCilInjection.Tests.Research;

// Test udowadniający, że nie da się skompilować na własnym urządzeniu biblioteki w taki sposób jakbyśmy chcieli ponieważ 
// jest problem z zabezpieczeniem modyfikowalności bibliotek nawet jak weźmiemy ściągnięte biblioteki które mamy w .nuget

[TestClass]
public class NumericsLibraryTests
{
    [TestMethod]
    public void SystemNumerics_RealCilInjectionAndExecution_ShouldSucceed()
    {
        string assemblyPath = typeof(BigInteger).Assembly.Location;
        byte[] originalBytes = File.ReadAllBytes(assemblyPath);

        var strategy = new InstructionCounterStrategy();
        var manager = new CilInjectionManager(strategy);

        byte[] instrumentedBytes = manager.Transform(originalBytes);

        Assert.IsNotNull(instrumentedBytes);
        Assert.IsTrue(instrumentedBytes.Length > originalBytes.Length, 
            "Zainstrumentowana biblioteka powinna być większa o wstrzyknąłem instrukcje licznika.");

        Console.WriteLine($"[REAL INJECTION SUCCESS] System.Numerics.dll ztransformowany. Oryginał: {originalBytes.Length} B, Po wstrzyknięciu: {instrumentedBytes.Length} B.");

        using var sandbox = InstrumentedSandbox.Create(
            instrumentedBytes, 
            nameof(SystemNumerics_RealCilInjectionAndExecution_ShouldSucceed), 
            strategy);

        var bigIntType = sandbox.Assembly.GetType("System.Numerics.BigInteger")!;
        var parseMethod = bigIntType.GetMethod("Parse", new[] { typeof(string) })!;

        sandbox.Counter.ResetCounter();
        object? result = parseMethod.Invoke(null, new object[] { "12345678901234567890" });
        long executedInstructions = sandbox.Counter.GetCounter();

        Console.WriteLine($"[EXECUTION] System.Numerics.BigInteger.Parse wykonało: {executedInstructions} instrukcji CIL.");

        Assert.IsNotNull(result);
        Assert.IsTrue(executedInstructions > 0, "Licznik instrukcji CIL dla zainstrumentowanej biblioteki zwrócił 0.");
    }

    [TestMethod]
    public void SystemDiagnosticsDebug_RealCilInjectionAndExecution_ShouldSucceed()
    {
        string assemblyPath = typeof(Debug).Assembly.Location;
        byte[] originalBytes = File.ReadAllBytes(assemblyPath);

        var strategy = new InstructionCounterStrategy();
        var manager = new CilInjectionManager(strategy);

        byte[] instrumentedBytes = manager.Transform(originalBytes);
        Console.WriteLine($"[REAL INJECTION SUCCESS] System.Diagnostics.Debug.dll ztransformowany. Rozmiar: {instrumentedBytes.Length} B.");

        using var sandbox = InstrumentedSandbox.Create(
            instrumentedBytes, 
            nameof(SystemDiagnosticsDebug_RealCilInjectionAndExecution_ShouldSucceed), 
            strategy);

        var debugType = sandbox.Assembly.GetType("System.Diagnostics.Debug")!;
        var assertMethod = debugType.GetMethod("Assert", new[] { typeof(bool) })!;

        sandbox.Counter.ResetCounter();
        assertMethod.Invoke(null, new object[] { true });
        long executedInstructions = sandbox.Counter.GetCounter();

        Console.WriteLine($"[EXECUTION] System.Diagnostics.Debug.Assert wykonało: {executedInstructions} instrukcji CIL.");

        Assert.IsTrue(executedInstructions > 0, "Licznik dla System.Diagnostics.Debug zwrócił 0.");
    }
}