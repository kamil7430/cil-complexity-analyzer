using Microsoft.VisualStudio.TestTools.UnitTesting;
using ASD.Graphs;
using CilComplexityAnalyzer.LibCilInjection.Tests.GraphFunctions;

namespace CilComplexityAnalyzer.LibCilInjection.Tests;

[TestClass]
public class GraphOperationsTests
{
    [TestMethod]
    public void BFS_ShouldCountVisitedEdgesAndVerticesCorrectly()
    {
        //Arrange
        Graph graph = new Graph(5);

        graph.AddEdge(0, 1);
        graph.AddEdge(0, 2);
        graph.AddEdge(1, 3);

        var searcher = new GraphSearcher();
        // Act
        int[] result = searcher.BreadthFirstSearch(graph, 0);

        // Assert
        CollectionAssert.AreEqual(new int[] { 0, 1, 2, 3 }, result);
    }
}