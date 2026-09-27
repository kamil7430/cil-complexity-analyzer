namespace CilComplexityAnalyzer.LibCilInjection.Tests.GraphFunctions;

using System.Collections.Generic;
using ASD.Graphs;


public class GraphSearcher
{

    public int[] BreadthFirstSearch(Graph G, int startVertex)
    {
        int n = G.VertexCount;
        bool[] visited = new bool[n];
        Queue<int> queue = new Queue<int>();
        List<int> visitedOrder = new List<int>();

        visited[startVertex] = true;
        queue.Enqueue(startVertex);

        while (queue.Count > 0)
        {
            int current = queue.Dequeue();
            visitedOrder.Add(current);

            foreach (int neighbor in G.OutNeighbors(current))
            {
                if (!visited[neighbor])
                {
                    visited[neighbor] = true;
                    queue.Enqueue(neighbor);
                }
            }
        }

        return visitedOrder.ToArray();
    }
    
    
}

public class HeavyGraphOperations
{
    public int AllPathsDFS(Graph G, int current, int target, bool[] visited)
    {
        if (current == target) return 1;

        visited[current] = true;
        int count = 0;

        foreach (int neighbor in G.OutNeighbors(current))
        {
            if (!visited[neighbor])
            {
                count += AllPathsDFS(G, neighbor, target, visited);
            }
        }

        visited[current] = false;
        return count;
    }
}