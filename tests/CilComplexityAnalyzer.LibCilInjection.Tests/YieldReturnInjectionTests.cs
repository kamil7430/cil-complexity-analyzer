using System;
using System.Collections.Generic;
using System.Linq;
using CilComplexityAnalyzer.LibCilInjection.Tests.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CilComplexityAnalyzer.LibCilInjection.Tests;

[TestClass]
public class YieldReturnInjectionTests
{
    private const string IteratorSource = """
        namespace DummyNamespace;

        public class RangeYielder
        {
            public System.Collections.Generic.IEnumerable<int> Range(int from, int to)
            {
                for (int i = from; i <= to; i++)
                    yield return i * i;
            }
        }
        """;

    [TestMethod]
    public void Injector_ShouldAccuratelyTrackInstructions_ForYieldReturnStateMachine()
    {
        byte[] assemblyBytes = DynamicSourceCompiler.CompileSource(IteratorSource);

        using var sandbox = InstrumentedSandbox.Create(
            assemblyBytes, nameof(Injector_ShouldAccuratelyTrackInstructions_ForYieldReturnStateMachine));

        var yielderType = sandbox.Assembly.GetType("DummyNamespace.RangeYielder")!;
        var instance = Activator.CreateInstance(yielderType)!;
        var rangeMethod = yielderType.GetMethod("Range")!;

        sandbox.Counter.ResetCounter();

        var enumerable5 = (IEnumerable<int>)rangeMethod.Invoke(instance, new object[] { 1, 5 })!;
        var result5 = enumerable5.ToList();

        long countFor5Items = sandbox.Counter.GetCounter();

        CollectionAssert.AreEqual(new List<int> { 1, 4, 9, 16, 25 }, result5);
        Assert.IsTrue(countFor5Items > 0, $"Licznik CIL powinien wynieść > 0, a wyniósł {countFor5Items}");

        sandbox.Counter.ResetCounter();

        var enumerable2 = (IEnumerable<int>)rangeMethod.Invoke(instance, new object[] { 1, 2 })!;
        var result2 = enumerable2.ToList();

        long countFor2Items = sandbox.Counter.GetCounter();

        CollectionAssert.AreEqual(new List<int> { 1, 4 }, result2);

        Assert.IsTrue(countFor5Items > countFor2Items,
            $"Liczba instrukcji dla 5 elementów ({countFor5Items}) powinna być większa niż dla 2 elementów ({countFor2Items})");
    }
}