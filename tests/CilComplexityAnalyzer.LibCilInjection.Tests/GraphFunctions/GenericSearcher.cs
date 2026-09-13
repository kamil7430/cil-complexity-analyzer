namespace CilComplexityAnalyzer.LibCilInjection.Tests.GraphFunctions;

public class GenericSearcher<T> where T : notnull
{
    public List<T> PerformGenericSearch(Dictionary<T, List<T>> adjacencyList, T startVertex)
    {
        var visited = new List<T>();
        var queue = new Queue<T>();

        visited.Add(startVertex);
        queue.Enqueue(startVertex);

        while (queue.Count > 0)
        {
            T current = queue.Dequeue();

            if (adjacencyList.TryGetValue(current, out var neighbors))
            {
                foreach (T neighbor in neighbors)
                {
                    if (!visited.Contains(neighbor))
                    {
                        visited.Add(neighbor);
                        queue.Enqueue(neighbor);
                    }
                }
            }
        }

        return visited;
    }
}