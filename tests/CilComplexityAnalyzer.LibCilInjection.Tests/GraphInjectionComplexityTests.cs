using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using ASD.Graphs;
using CilComplexityAnalyzer.LibCilInjection.Tests.Common;
using CilComplexityAnalyzer.LibCilInjection.Tests.GraphFunctions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CilComplexityAnalyzer.LibCilInjection.Tests;

[TestClass]
public class GraphInjectionComplexityTests
{
    [TestMethod]
    public void Injector_ShouldAccuratelyTrackCilInstructionsForGraphSearcher()
    {
        Assembly originalAssembly = typeof(GraphSearcher).Assembly;
        byte[] originalBytes = File.ReadAllBytes(originalAssembly.Location);

        using var sandbox = InstrumentedSandbox.Create(
            originalBytes, nameof(Injector_ShouldAccuratelyTrackCilInstructionsForGraphSearcher));

        var searcherType = sandbox.Assembly.GetType(typeof(GraphSearcher).FullName!)!;
        var searcherInstance = Activator.CreateInstance(searcherType)!;
        var bfsMethod = searcherType.GetMethod("BreadthFirstSearch")!;

        var graph = new Graph(4);
        graph.AddEdge(0, 1);
        graph.AddEdge(0, 2);
        graph.AddEdge(1, 3);

        sandbox.Counter.ResetCounter();

        object? result = bfsMethod.Invoke(searcherInstance, new object[] { graph, 0 });

        long executedInstructions = sandbox.Counter.GetCounter();

        Assert.IsNotNull(result);
        var visitedOrder = (int[])result;
        CollectionAssert.AreEqual(new[] { 0, 1, 2, 3 }, visitedOrder);

        Assert.IsTrue(executedInstructions > 0,
            $"Licznik CIL powinien zarejestrować dodatnią liczbę instrukcji, a wyniósł: {executedInstructions}");
        Assert.IsTrue(executedInstructions > 50,
            $"Zarejestrowana liczba instrukcji CIL ({executedInstructions}) jest podejrzanie niska.");
    }

    [TestMethod]
    public void Injector_ShouldTrackInstructions_ForGenericClassesAndMethods()
    {
        Assembly originalAssembly = typeof(GraphSearcher).Assembly;
        byte[] originalBytes = File.ReadAllBytes(originalAssembly.Location);

        using var sandbox = InstrumentedSandbox.Create(
            originalBytes, nameof(Injector_ShouldTrackInstructions_ForGenericClassesAndMethods));

        var openSearcherType = sandbox.Assembly.GetType(typeof(GenericSearcher<>).FullName!)!;
        var closedSearcherType = openSearcherType.MakeGenericType(typeof(string));
        var searcherInstance = Activator.CreateInstance(closedSearcherType)!;
        var searchMethod = closedSearcherType.GetMethod("PerformGenericSearch")!;

        var graphData = new Dictionary<string, List<string>>
        {
            { "A", new List<string> { "B", "C" } },
            { "B", new List<string> { "D" } },
            { "C", new List<string> { "D" } },
            { "D", new List<string>() }
        };

        sandbox.Counter.ResetCounter();

        object? result = searchMethod.Invoke(searcherInstance, new object[] { graphData, "A" });

        long executedInstructions = sandbox.Counter.GetCounter();

        Assert.IsNotNull(result);
        var visitedNodes = (List<string>)result;
        CollectionAssert.AreEqual(new List<string> { "A", "B", "C", "D" }, visitedNodes);

        Assert.IsTrue(executedInstructions > 0,
            $"Licznik CIL powinien zarejestrować dodatnią liczbę instrukcji, a wyniósł: {executedInstructions}");
        Assert.IsTrue(executedInstructions > 100,
            $"Zarejestrowana liczba instrukcji CIL ({executedInstructions}) jest za niska dla przeszukiwania grafu.");
    }
}