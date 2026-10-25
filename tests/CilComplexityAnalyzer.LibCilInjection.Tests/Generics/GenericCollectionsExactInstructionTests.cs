using CilComplexityAnalyzer.LibCilInjection.Tests.Common;
using Counter.Provider.InstructionCounter;

namespace CilComplexityAnalyzer.LibCilInjection.Tests.Generics;

[TestClass]
public class GenericCollectionsExactInstructionTests
{
    [TestMethod]
    public void GenericCollections_ListAndDictionary_ShouldProduceMeasurableCilCount()
    {
        string genericCollectionsSource = """
            using System;

            namespace SimulatedCollections;

            public class GenericList<T>
            {
                private T[] _items = new T[4];
                private int _size = 0;

                public int Count => _size;

                public void Add(T item)
                {
                    if (_size == _items.Length)
                    {
                        var newArray = new T[_items.Length * 2];
                        Array.Copy(_items, newArray, _size);
                        _items = newArray;
                    }
                    _items[_size++] = item;
                }
            }

            public class GenericDictionary<TKey, TValue> where TKey : notnull
            {
                private struct Entry
                {
                    public TKey Key;
                    public TValue Value;
                    public bool Used;
                }

                private Entry[] _entries = new Entry[4];
                private int _count = 0;

                public int Count => _count;

                public void Add(TKey key, TValue value)
                {
                    if (_count >= _entries.Length)
                    {
                        var newEntries = new Entry[_entries.Length * 2];
                        Array.Copy(_entries, newEntries, _count);
                        _entries = newEntries;
                    }

                    _entries[_count] = new Entry { Key = key, Value = value, Used = true };
                    _count++;
                }
            }

            public static class CollectionsRunner
            {
                public static int RunListTest(int n)
                {
                    var list = new GenericList<int>();
                    for (int i = 0; i < n; i++)
                    {
                        list.Add(i);
                    }
                    return list.Count;
                }

                public static int RunDictionaryTest(int n)
                {
                    var dict = new GenericDictionary<string, int>();
                    for (int i = 0; i < n; i++)
                    {
                        dict.Add("key_" + i, i);
                    }
                    return dict.Count;
                }
            }
            """;

        byte[] assemblyBytes = DynamicSourceCompiler.CompileSource(genericCollectionsSource, optimize: true);
        var strategy = new InstructionCounterStrategy();

        using var sandbox = InstrumentedSandbox.Create(
            assemblyBytes,
            nameof(GenericCollections_ListAndDictionary_ShouldProduceMeasurableCilCount),
            strategy);

        var runnerType = sandbox.Assembly.GetType("SimulatedCollections.CollectionsRunner")!;
        
        var listMethod = runnerType.GetMethod("RunListTest")!;
        var dictMethod = runnerType.GetMethod("RunDictionaryTest")!;

        sandbox.Counter.ResetCounter();
        int listResult = (int)listMethod.Invoke(null, new object[] { 50 })!;
        long listCost = sandbox.Counter.GetCounter();

        sandbox.Counter.ResetCounter();
        int dictResult = (int)dictMethod.Invoke(null, new object[] { 50 })!;
        long dictCost = sandbox.Counter.GetCounter();

        Console.WriteLine($"[GENERIC COLLECTIONS] GenericList<int> (50 elementów): {listCost} instrukcji CIL.");
        Console.WriteLine($"[GENERIC COLLECTIONS] GenericDictionary<string, int> (50 elementów): {dictCost} instrukcji CIL.");

        Assert.AreEqual(50, listResult);
        Assert.AreEqual(50, dictResult);
        Assert.IsTrue(listCost > 0, "Licznik dla generycznej listy zwrócił 0.");
        Assert.IsTrue(dictCost > 0, "Licznik dla generycznego słownika zwrócił 0.");
        Assert.AreEqual(1639, listCost, "Liczba instrukcji CIL dla AddEdge uległa zmianie!");
        Assert.AreEqual(2339, dictCost, "Liczba instrukcji CIL dla AddEdge uległa zmianie!");
    }
    
    [TestMethod]
    public void Benchmark_1_CustomGenericList()
    {
        string source = """
            using System;

            namespace BenchmarkProject;

            public class GenericList<T>
            {
                private T[] _items = new T[4];
                private int _size = 0;
                public int Count => _size;

                public void Add(T item)
                {
                    if (_size == _items.Length)
                    {
                        var newArray = new T[_items.Length * 2];
                        Array.Copy(_items, newArray, _size);
                        _items = newArray;
                    }
                    _items[_size++] = item;
                }

                public T Get(int index) => _items[index];
            }

            public static class AlgorithmRunner
            {
                public static int Run(int n)
                {
                    var list = new GenericList<int>();
                    for (int i = 0; i < n; i++) list.Add(i);
                    int sum = 0;
                    for (int i = 0; i < list.Count; i++) sum += list.Get(i);
                    return sum;
                }
            }
            """;

        long cost = RunAndGetCost(source, nameof(Benchmark_1_CustomGenericList), 50);
        Console.WriteLine($"[BENCHMARK] 1. Własna GenericList<T> (50 el.): {cost} instrukcji CIL.");
        Assert.IsTrue(cost > 0);
    }

    [TestMethod]
    public void Benchmark_2_StandardBclList()
    {
        string source = """
            using System.Collections.Generic;

            namespace BenchmarkProject;

            public static class AlgorithmRunner
            {
                public static int Run(int n)
                {
                    var list = new List<int>();
                    for (int i = 0; i < n; i++) list.Add(i);
                    int sum = 0;
                    for (int i = 0; i < list.Count; i++) sum += list[i];
                    return sum;
                }
            }
            """;

        long cost = RunAndGetCost(source, nameof(Benchmark_2_StandardBclList), 50);
        Console.WriteLine($"[BENCHMARK] 2. Standardowa List<int> BCL (50 el.): {cost} instrukcji CIL.");
        Assert.IsTrue(cost > 0);
    }

    [TestMethod]
    public void Benchmark_3_RawArrayNonGeneric()
    {
        string source = """
            using System;

            namespace BenchmarkProject;

            public static class AlgorithmRunner
            {
                public static int Run(int n)
                {
                    var arr = new int[n];
                    for (int i = 0; i < n; i++) arr[i] = i;
                    int sum = 0;
                    for (int i = 0; i < arr.Length; i++) sum += arr[i];
                    return sum;
                }
            }
            """;

        long cost = RunAndGetCost(source, nameof(Benchmark_3_RawArrayNonGeneric), 50);
        Console.WriteLine($"[BENCHMARK] 3. Zwykła tablica int[] (50 el.): {cost} instrukcji CIL.");
        Assert.IsTrue(cost > 0);
    }

    private static long RunAndGetCost(string source, string testName, int n)
    {
        byte[] bytes = DynamicSourceCompiler.CompileSource(source, optimize: true);
        var strategy = new InstructionCounterStrategy();

        using var sandbox = InstrumentedSandbox.Create(bytes, testName, strategy);
        var runnerType = sandbox.Assembly.GetType("BenchmarkProject.AlgorithmRunner")!;
        var method = runnerType.GetMethod("Run")!;

        sandbox.Counter.ResetCounter();
        method.Invoke(null, new object[] { n });
        return sandbox.Counter.GetCounter();
    }
}