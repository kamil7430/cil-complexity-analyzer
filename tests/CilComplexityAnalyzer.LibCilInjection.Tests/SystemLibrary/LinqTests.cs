using System.Diagnostics;
using System.Reflection;
using CilComplexityAnalyzer.LibCilInjection.Tests.Common;
using Counter.Provider.InstructionCounter;

namespace CilComplexityAnalyzer.LibCilInjection.Tests.SystemLibrary;

[TestClass]
public class LinqTests
{
    [TestMethod]
    public void Linq_Count_ShouldProduceMeasurableCilCount()
    {
        // byte[] assemblyBytes = DynamicSourceCompiler.CompileSource(System.Linq, optimize: true);
        var originalAssemblyBytes = File.ReadAllBytes(typeof(Enumerable).Assembly.Location);
        var processInfo = new ProcessStartInfo
        {
            FileName = "C:\\Users\\kamil\\.nuget\\packages\\ilasm\\9.4.0\\bin\\Win.x64\\ildasm.exe"
        }
        
        var strategy = new InstructionCounterStrategy();

        using var sandbox = InstrumentedSandbox.Create(
            assemblyBytes,
            nameof(Linq_Count_ShouldProduceMeasurableCilCount),
            strategy
        );

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