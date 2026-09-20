using System;
using System.Collections.Generic;
using CilComplexityAnalyzer.LibCilInjection.Tests.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CilComplexityAnalyzer.LibCilInjection.Tests;

[TestClass]
public class StructEnumeratorInjectionTests
{
    private const string SourceCode = """
        namespace DummyNamespace;

        public class ListSummer
        {
            public int SumList(System.Collections.Generic.List<int> values)
            {
                int sum = 0;
                foreach (var v in values) // generuje constrained.callvirt na List<int>.Enumerator (struct!)
                {
                    sum += v;
                }
                return sum;
            }

            public int SumDictionaryValues(System.Collections.Generic.Dictionary<string, int> map)
            {
                int sum = 0;
                foreach (var kvp in map) // Dictionary<K,V>.Enumerator też jest structem
                {
                    sum += kvp.Value;
                }
                return sum;
            }
        }
        """;

    [TestMethod]
    public void Injector_ShouldNotThrow_ForForeachOverListStructEnumerator()
    {
        byte[] assemblyBytes = DynamicSourceCompiler.CompileSource(SourceCode);

        using var sandbox = InstrumentedSandbox.Create(
            assemblyBytes, nameof(Injector_ShouldNotThrow_ForForeachOverListStructEnumerator));

        var summerType = sandbox.Assembly.GetType("DummyNamespace.ListSummer")!;
        var instance = Activator.CreateInstance(summerType)!;
        var sumListMethod = summerType.GetMethod("SumList")!;

        sandbox.Counter.ResetCounter();

        var list = new List<int> { 1, 2, 3 };
        var result = sumListMethod.Invoke(instance, new object[] { list });

        Assert.AreEqual(6, result);
        Assert.IsTrue(sandbox.Counter.GetCounter() > 0);
    }

    [TestMethod]
    public void Injector_ShouldNotThrow_ForForeachOverDictionaryStructEnumerator()
    {
        byte[] assemblyBytes = DynamicSourceCompiler.CompileSource(SourceCode);

        using var sandbox = InstrumentedSandbox.Create(
            assemblyBytes, nameof(Injector_ShouldNotThrow_ForForeachOverDictionaryStructEnumerator));

        var summerType = sandbox.Assembly.GetType("DummyNamespace.ListSummer")!;
        var instance = Activator.CreateInstance(summerType)!;
        var sumDictMethod = summerType.GetMethod("SumDictionaryValues")!;

        sandbox.Counter.ResetCounter();

        var dictionary = new Dictionary<string, int>
        {
            { "A", 10 },
            { "B", 20 },
            { "C", 30 }
        };

        var result = sumDictMethod.Invoke(instance, new object[] { dictionary });

        Assert.AreEqual(60, result);
        Assert.IsTrue(sandbox.Counter.GetCounter() > 0);
    }
}