using System;
using System.Linq;
using CilComplexityAnalyzer.LibCilInjection.Tests.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CilComplexityAnalyzer.LibCilInjection.Tests;

[TestClass]
public class CollectionsAndLinqBoundaryTests
{
    private const int SmallN = 500;
    private const int LargeN = 4000;

    private const string DriverSource = """
        namespace Drivers;

        using System.Collections.Generic;
        using System.Linq;

        public static class BoundaryDriver
        {
            private static List<int> _list;
            private static int[] _array;

            public static void Prepare(int n)
            {
                var list = new List<int>(n);
                for (int i = 0; i < n; i++)
                    list.Add(i);
                _list = list;
                _array = list.ToArray();
            }

            // Cała praca widoczna w instrumentowanym kodzie — brak wywołań do BCL wewnątrz pętli,
            // tylko indeksowanie tablicy. To jest nasz punkt odniesienia ("w pełni widoczne").
            public static long RunManualArrayLoop()
            {
                long sum = 0;
                for (int i = 0; i < _array.Length; i++)
                {
                    if (_array[i] % 2 == 0)
                        sum += _array[i] * 2;
                }
                return sum;
            }

            // foreach po List<int> — pętla jest w naszym kodzie, ale MoveNext()/get_Current()
            // enumeratora-structa to wywołania do System.Private.CoreLib (nieinstrumentowane wewnątrz).
            // Liczba wywołań powinna mimo to rosnąć liniowo z n.
            public static long RunForeachOverList()
            {
                long sum = 0;
                foreach (int x in _list)
                {
                    if (x % 2 == 0)
                        sum += x * 2;
                }
                return sum;
            }

            // Łańcuch LINQ z lambdami studenta. Where/Select nie wykonują nic przy wywołaniu
            // (budują tylko iteratory), a mechanika pętli (MoveNext) siedzi w System.Linq.dll.
            // ALE: kompilator kompiluje ciała lambd (x => x % 2 == 0, x => x * 2) jako metody
            // W TYM SAMYM assembly co BoundaryDriver, więc każdy callback z iteratora do lambdy
            // WCHODZI do instrumentowanego kodu. Oczekiwanie: mimo ukrytej pętli, koszt i tak
            // rośnie liniowo, bo callback dzieje się raz na element.
            public static long RunLinqChain()
                => _list.Where(x => x % 2 == 0).Select(x => (long)x * 2).Sum();

            // Operacja terminalna BEZ ŻADNEGO delegatu studenta — Sum() na wprost liście int.
            // Tu nie ma żadnego callbacku do instrumentowanego kodu: cała suma liczy się
            // wewnątrz System.Linq.dll, iterując po List<int>.Enumerator z System.Private.CoreLib.
            // To jest prawdziwy test na "całkowicie znikającą pętlę".
            public static long RunLinqSumWithoutDelegate() => _list.Sum();
        }
        """;

    private static readonly Lazy<byte[]> DriverBytes = new(() =>
        DynamicSourceCompiler.CompileSource(DriverSource));

    private readonly record struct Costs(long Manual, long Foreach, long Linq, long LinqNoDelegate);

    private static double Exponent(long costSmall, long costLarge, int nSmall, int nLarge)
        => Math.Log((double)costLarge / costSmall) / Math.Log((double)nLarge / nSmall);

    private static Costs Measure(int n)
    {
        using var sandbox = InstrumentedSandbox.Create(DriverBytes.Value, $"bcl_boundary_{n}_{Guid.NewGuid():N}");

        var driver = sandbox.Assembly.GetType("Drivers.BoundaryDriver")!;
        driver.GetMethod("Prepare")!.Invoke(null, new object[] { n });

        long expected = 0;
        for (int i = 0; i < n; i++)
            if (i % 2 == 0) expected += (long)i * 2;

        sandbox.Counter.ResetCounter();
        long manualResult = (long)driver.GetMethod("RunManualArrayLoop")!.Invoke(null, null)!;
        long manualCost = sandbox.Counter.GetCounter();
        Assert.AreEqual(expected, manualResult, "RunManualArrayLoop dał błędny wynik.");

        sandbox.Counter.ResetCounter();
        long foreachResult = (long)driver.GetMethod("RunForeachOverList")!.Invoke(null, null)!;
        long foreachCost = sandbox.Counter.GetCounter();
        Assert.AreEqual(expected, foreachResult, "RunForeachOverList dał błędny wynik.");

        sandbox.Counter.ResetCounter();
        long linqResult = (long)driver.GetMethod("RunLinqChain")!.Invoke(null, null)!;
        long linqCost = sandbox.Counter.GetCounter();
        Assert.AreEqual(expected, linqResult, "RunLinqChain dał błędny wynik.");

        long expectedPlainSum = 0;
        for (int i = 0; i < n; i++) expectedPlainSum += i;

        sandbox.Counter.ResetCounter();
        long linqNoDelegateResult = (long)driver.GetMethod("RunLinqSumWithoutDelegate")!.Invoke(null, null)!;
        long linqNoDelegateCost = sandbox.Counter.GetCounter();
        Assert.AreEqual(expectedPlainSum, linqNoDelegateResult, "RunLinqSumWithoutDelegate dał błędny wynik.");

        return new Costs(manualCost, foreachCost, linqCost, linqNoDelegateCost);
    }

    [TestMethod]
    public void ManualArrayLoop_ShouldScaleLinearlyWithN()
    {
        var small = Measure(SmallN);
        var large = Measure(LargeN);

        double k = Exponent(small.Manual, large.Manual, SmallN, LargeN);
        Console.WriteLine($"manual: small={small.Manual}, large={large.Manual}, k={k:F2}");

        Assert.IsTrue(k > 0.8 && k < 1.2, $"Ręczna pętla po tablicy powinna skalować się ~liniowo, wyszło k={k:F2}.");
    }

    [TestMethod]
    public void ForeachOverList_GrowthExponent_ShouldStillBeLinear()
    {
        var small = Measure(SmallN);
        var large = Measure(LargeN);

        double kManual = Exponent(small.Manual, large.Manual, SmallN, LargeN);
        double kForeach = Exponent(small.Foreach, large.Foreach, SmallN, LargeN);

        Console.WriteLine($"foreach: small={small.Foreach}, large={large.Foreach}, k={kForeach:F2} (manual k={kManual:F2})");

        Assert.IsTrue(kForeach > 0.7,
            $"foreach po List<int> ma wykładnik k={kForeach:F2}. Jeśli <0.7, pętla po liście jest " +
            "liczona jako prawie płaska względem n — trzeba to jawnie odnotować jako ograniczenie.");
    }

    [TestMethod]
    public void LinqChainWithStudentLambdas_ShouldStillScaleLinearly()
    {
        var small = Measure(SmallN);
        var large = Measure(LargeN);

        double kManual = Exponent(small.Manual, large.Manual, SmallN, LargeN);
        double kLinq = Exponent(small.Linq, large.Linq, SmallN, LargeN);

        Console.WriteLine($"linq (z lambdami): small={small.Linq}, large={large.Linq}, k={kLinq:F2} (manual k={kManual:F2})");

        Assert.IsTrue(Math.Abs(kLinq - kManual) < 0.3,
            $"Łańcuch LINQ z lambdami studenta ma wykładnik k={kLinq:F2}, ręczna pętla k={kManual:F2} — " +
            "oczekiwano zbliżonych wartości, bo callbacki do lambd studenta są instrumentowane.");
    }

    [TestMethod]
    public void LinqTerminalOperationWithoutDelegate_ShouldExposeFlatCost()
    {
        var small = Measure(SmallN);
        var large = Measure(LargeN);

        double kManual = Exponent(small.Manual, large.Manual, SmallN, LargeN);
        double kLinqNoDelegate = Exponent(small.LinqNoDelegate, large.LinqNoDelegate, SmallN, LargeN);

        Console.WriteLine($"linq bez delegatu: small={small.LinqNoDelegate}, large={large.LinqNoDelegate}, " +
            $"k={kLinqNoDelegate:F2} (manual k={kManual:F2})");

        Assert.IsTrue(kLinqNoDelegate < kManual - 0.3,
            $"Sum() bez selektora ma wykładnik k={kLinqNoDelegate:F2}, ręczna pętla k={kManual:F2} — " +
            "oczekiwano wyraźnie niższego wykładnika, bo cała iteracja dzieje się poza instrumentowanym kodem.");
    }
}