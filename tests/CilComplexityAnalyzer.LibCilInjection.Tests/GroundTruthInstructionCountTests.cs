using System;
using System.Linq;
using CilComplexityAnalyzer.LibCilInjection.Tests.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CilComplexityAnalyzer.LibCilInjection.Tests;

[TestClass]
public class GroundTruthInstructionCountTests
{
    private static string BuildStraightLineSource(int repetitions)
    {
        var body = string.Join("\n", Enumerable.Repeat("x = x + 1;", repetitions));
        return $$"""
            namespace DummyNamespace;

            public static class StraightLine
            {
                public static int Run()
                {
                    int x = 0;
                    {{body}}
                    return x;
                }
            }
            """;
    }

    private static long MeasureCost(int repetitions)
    {
        byte[] bytes = DynamicSourceCompiler.CompileSource(BuildStraightLineSource(repetitions));
        using var sandbox = InstrumentedSandbox.Create(bytes, $"ground_truth_{repetitions}_{Guid.NewGuid():N}");

        var type = sandbox.Assembly.GetType("DummyNamespace.StraightLine")!;
        var run = type.GetMethod("Run")!;

        sandbox.Counter.ResetCounter();
        run.Invoke(null, null);
        return sandbox.Counter.GetCounter();
    }

    [TestMethod]
    public void StraightLineCost_ShouldGrowByExactlyConstantAmountPerStatement()
    {
        long c10 = MeasureCost(10);
        long c20 = MeasureCost(20);
        long c40 = MeasureCost(40);

        long diffLowToMid = c20 - c10;
        long diffMidToHigh = c40 - c20;

        Console.WriteLine($"c10={c10}, c20={c20}, c40={c40}, diff(10->20)={diffLowToMid}, diff(20->40)={diffMidToHigh}");

        Assert.AreEqual(diffLowToMid, diffMidToHigh / 2,
            $"Koszt na powtórzenie nie jest stały: (c20-c10)={diffLowToMid} vs (c40-c20)/2={diffMidToHigh / 2.0:F1}. " +
            "Sugeruje to, że licznik nie liczy liniowo instrukcji w kodzie bez pętli/rozgałęzień.");

        Assert.AreEqual(0, diffLowToMid % 10, "Koszt 10 powtórzeń powinien dzielić się przez 10 bez reszty.");
        long costPerStatement = diffLowToMid / 10;
        Assert.IsTrue(costPerStatement > 0, "Każde powtórzenie 'x = x + 1;' powinno mieć niezerowy koszt.");

        Console.WriteLine($"koszt na jedno 'x = x + 1;' = {costPerStatement} instrukcji CIL");
    }

    [TestMethod]
    public void EmptyMethod_ShouldHaveNonZeroDeterministicBaselineCost()
    {
        long cost1 = MeasureCost(0);
        long cost2 = MeasureCost(0);

        Assert.AreEqual(cost1, cost2,
            "Dwa pomiary tej samej (bez treści) metody dały różny koszt — licznik nie jest deterministyczny.");
        Assert.IsTrue(cost1 > 0, $"Koszt metody bez treści wyszedł {cost1} — instrumentacja nie weszła do środka.");

        Console.WriteLine($"koszt bazowy (bez treści) = {cost1}");
    }
}