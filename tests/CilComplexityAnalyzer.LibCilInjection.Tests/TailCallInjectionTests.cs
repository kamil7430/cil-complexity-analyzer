using CilComplexityAnalyzer.LibCilInjection.Tests.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CilComplexityAnalyzer.LibCilInjection.Tests;

[TestClass]
public class TailCallInjectionTests
{
    private const string TailCallSource = """
                                          namespace DummyNamespace;

                                          public static class TailRecursive
                                          {
                                              public static long SumTo(long n, long acc = 0)
                                              {
                                                  if (n == 0) return acc;
                                                  return SumTo(n - 1, acc + n); // kompilator MOŻE (nie musi) wyemitować tail.
                                              }
                                          }
                                          """;

    [TestMethod]
    public void Injector_ShouldPreserveTailCallSemanticsAndTrackInstructions()
    {
        byte[] assemblyBytes = DynamicSourceCompiler.CompileSource(TailCallSource, optimize: true);

        using var sandbox = InstrumentedSandbox.Create(
            assemblyBytes, nameof(Injector_ShouldPreserveTailCallSemanticsAndTrackInstructions));

        var tailType = sandbox.Assembly.GetType("DummyNamespace.TailRecursive")!;
        var sumToMethod = tailType.GetMethod("SumTo")!;

        sandbox.Counter.ResetCounter();

        var result = (long)sumToMethod.Invoke(null, new object[] { 100L, 0L })!;

        Assert.AreEqual(5050L, result);

        long executedInstructions = sandbox.Counter.GetCounter();
        Assert.IsTrue(executedInstructions > 0,
            $"Licznik CIL powinien zarejestrować instrukcje w rekurencji ogonowej, a wyniósł: {executedInstructions}");
        Assert.IsTrue(executedInstructions > 500,
            $"Liczba zarejestrowanych instrukcji ({executedInstructions}) jest za mała dla 100 wywołań rekurencyjnych.");
    }
}