using ASD.Graphs;
using CilComplexityAnalyzer.LibCilInjection.Tests.Common;
using Counter.Provider.InstructionCounter;

namespace CilComplexityAnalyzer.LibCilInjection.Tests.FixedTests.Graphsdll;

[TestClass]
public class GraphExactInstructionTests
{
    [TestMethod]
    public void Graph_AddEdge_ShouldProduceExactAndMeasurableInstructionCount()
    {
        byte[] graphsBytes = File.ReadAllBytes(typeof(Graph).Assembly.Location);

        var strategy = new InstructionCounterStrategy();

        using var sandbox = InstrumentedSandbox.Create(
            graphsBytes, 
            nameof(Graph_AddEdge_ShouldProduceExactAndMeasurableInstructionCount), 
            strategy);

        var graphType = sandbox.Assembly.GetType("ASD.Graphs.Graph")!;
        
        var graphInstance = Activator.CreateInstance(graphType, new object[] { 2 })!;
        var addEdgeMethod = graphType.GetMethod("AddEdge", new[] { typeof(int), typeof(int) })!;

        sandbox.Counter.ResetCounter();
        addEdgeMethod.Invoke(graphInstance, new object[] { 0, 1 });
        
        long exactInstructions = sandbox.Counter.GetCounter();

        Console.WriteLine($"[DETERMINISTIC TEST] Liczba instrukcji CIL dla AddEdge(0, 1): {exactInstructions}");

        Assert.IsTrue(exactInstructions > 0, "Licznik CIL zwrócił 0 — oznacza to, że kod wewnątrz Graphs.dll nie został zainstrumentowany lub metoda nie wykonała żadnych zliczanych instrukcji.");
        
        //Assert.IsTrue(exactInstructions > 10, $"Zarejestrowana liczba instrukcji ({exactInstructions}) jest zbyt niska, co sugeruje problem z wstrzykiwaniem.");
        Assert.AreEqual(158, exactInstructions, "Liczba instrukcji CIL dla AddEdge uległa zmianie!");
    }
}