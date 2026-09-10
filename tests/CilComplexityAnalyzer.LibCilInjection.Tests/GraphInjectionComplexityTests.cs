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

            FieldInfo counterField =
                counterContainer.GetField("__InstructionCounter", BindingFlags.Public | BindingFlags.Static)
                ?? throw new InvalidOperationException("Pole __InstructionCounter nie zostało odnalezione!");

            Type searcherType = instrumentedAssembly.GetType(typeof(GraphSearcher).FullName!)!;
            object searcherInstance = Activator.CreateInstance(searcherType)!;
            MethodInfo bfsMethod = searcherType.GetMethod("BreadthFirstSearch")!;

            Graph graph = new Graph(4);
            graph.AddEdge(0, 1);
            graph.AddEdge(0, 2);
            graph.AddEdge(1, 3);

            counterField.SetValue(null, 0L);

            object? result = bfsMethod.Invoke(searcherInstance, new object[] { graph, 0 });

            long executedInstructions = (long)counterField.GetValue(null)!;

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
}