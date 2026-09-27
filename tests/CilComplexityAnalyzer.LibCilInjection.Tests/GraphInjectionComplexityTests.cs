using System.Reflection;
using System.Runtime.Loader;
using ASD.Graphs;
using CilComplexityAnalyzer.LibCilInjection.Tests.GraphFunctions;
using CilComplexityAnalyzer.TestExecutor;

namespace CilComplexityAnalyzer.LibCilInjection.Tests;

[TestClass]
public class GraphInjectionComplexityTests
{
    [TestMethod]
    public void Injector_ShouldAccuratelyTrackCilInstructionsForGraphSearcher()
    {
        Assembly originalAssembly = typeof(GraphSearcher).Assembly;
        byte[] originalBytes = File.ReadAllBytes(originalAssembly.Location);

        byte[] instrumentedBytes = CilInstructionInjector.InjectCilToAssemblyBytes(originalBytes);

        var alc = new AssemblyLoadContext("TestContext", isCollectible: true);

        try
        {

            using var ms = new MemoryStream(instrumentedBytes);
            Assembly instrumentedAssembly = alc.LoadFromStream(ms);

            Type counterContainer = instrumentedAssembly.GetType("<GlobalCounterContainer>")
                                    ?? throw new InvalidOperationException(
                                        "Klasa <GlobalCounterContainer> nie została odnaleziona!");

            MethodInfo resetMethod = counterContainer.GetMethod(
                 "ResetInstructionCount", BindingFlags.Public | BindingFlags.Static) 
                 ?? throw new InvalidOperationException("Method ResetInstructionCount nie zostala odnaleziona!");
            MethodInfo getMethod = counterContainer.GetMethod(
                "GetInstructionCount", BindingFlags.Public | BindingFlags.Static)
                ?? throw new InvalidOperationException("<etoda GetInstructionCount nie została odnaleziona!");

            Type searcherType = instrumentedAssembly.GetType(typeof(GraphSearcher).FullName!)!;
            object searcherInstance = Activator.CreateInstance(searcherType)!;
            MethodInfo bfsMethod = searcherType.GetMethod("BreadthFirstSearch")!;

            Graph graph = new Graph(4);
            graph.AddEdge(0, 1);
            graph.AddEdge(0, 2);
            graph.AddEdge(1, 3);

            resetMethod.Invoke(null, null);

            object? result = bfsMethod.Invoke(searcherInstance, new object[] { graph, 0 });

            long executedInstructions = (long)getMethod.Invoke(null, null)!;

            Assert.IsNotNull(result);
            int[] visitedOrder = (int[])result;
            CollectionAssert.AreEqual(new int[] { 0, 1, 2, 3 }, visitedOrder);

            Assert.IsTrue(executedInstructions > 0, $"Licznik CIL powinien zarejestrować dodatnią liczbę instrukcji, a wyniósł: {executedInstructions}");
            Assert.IsTrue(executedInstructions > 50, $"Zarejestrowana liczba instrukcji CIL ({executedInstructions}) jest podejrzanie niska.");
        }
        finally
        {
            alc.Unload();
        }
    }

    [TestMethod]
    public void Injector_ShouldTrackInstructions_ForGenericClassesAndMethods()
    {
        Assembly originalAssembly = typeof(GraphSearcher).Assembly;
        byte[] originalBytes = File.ReadAllBytes(originalAssembly.Location);
        byte[] instrumentedBytes = CilInstructionInjector.InjectCilToAssemblyBytes(originalBytes);
        var alc = new AssemblyLoadContext("GenericTestContext", isCollectible: true);

        try
        {
            using var ms = new MemoryStream(instrumentedBytes);
            Assembly instrumentedAssembly = alc.LoadFromStream(ms);

            Type counterContainer = instrumentedAssembly.GetType("<GlobalCounterContainer>") 
                                    ?? throw new InvalidOperationException("Klasa <GlobalCounterContainer nie została odnaleziona!");
            
            MethodInfo resetMethod = counterContainer.GetMethod(
                "ResetInstructionCount", BindingFlags.Public | BindingFlags.Static)
                ?? throw new InvalidOperationException("Metoda ResetInstructionCount nie została odnaleziona!");
            
            MethodInfo getMethod = counterContainer.GetMethod(
                "GetInstructionCount", BindingFlags.Public | BindingFlags.Static)
                ?? throw new InvalidOperationException("Metoda GetInstructionCount nie zostala odnaleziona!");

            Type openSearcherType = instrumentedAssembly.GetType(typeof(GenericSearcher<>).FullName!)!;
            Type closedSearcherType = openSearcherType.MakeGenericType(typeof(string));

            object searcherInstance = Activator.CreateInstance(closedSearcherType)!;
            MethodInfo searchMethod = closedSearcherType.GetMethod("PerformGenericSearch")!;

            var graphData = new Dictionary<string, List<string>>
            {
                { "A", new List<string> { "B", "C" } },
                { "B", new List<string> { "D" } },
                { "C", new List<string> { "D" } },
                { "D", new List<string>() }
            };

            resetMethod.Invoke(null, null);

            object? result = searchMethod.Invoke(searcherInstance, new object[] { graphData, "A" });

            long executedInstructions = (long)getMethod.Invoke(null, null)!;
            Assert.IsNotNull(result);
            List<string> visitedNodes = (List<string>)result;
            
            CollectionAssert.AreEqual(new List<string> { "A", "B", "C", "D" }, visitedNodes);
            
            Assert.IsTrue(executedInstructions > 0, 
                $"Licznik CIL powinien zarejestrować dodatnią liczbę instrukcji, a wyniósł: {executedInstructions}");

            Assert.IsTrue(executedInstructions > 100, 
                $"Zarejestrowana liczba instrukcji CIL ({executedInstructions}) jest za niska dla przeszukiwania grafu.");
        }
        finally
        {
            alc.Unload();
        }
    }
}