using System.Linq;

namespace ASD
{
    using ASD.Graphs;
    using System;
    using System.Collections.Generic;

    public class Lab06 : System.MarshalByRefObject
    {
        public List<int> WidePath(DiGraph<int> G, int start, int end)
        {
            int n = G.VertexCount;
            var najszersze = new int[n];
            var prev = new int?[n];
            SafePriorityQueue<int, int> queue = new();
            for (int i = 0; i < n; i++)
            {
                if (i == start)
                    najszersze[i] = int.MaxValue;
                else
                    najszersze[i] = 0;
                queue.Insert(i, -najszersze[i]);
            }

            while(queue.Count > 0)
            {
                var u = queue.Extract();
                foreach (var edge in G.OutEdges(u))
                {
                    if (Math.Min(edge.Weight, najszersze[u]) > najszersze[edge.To])
                    {
                        najszersze[edge.To] = Math.Min(edge.Weight, najszersze[u]);
                        prev[edge.To] = u;
                        queue.UpdatePriority(edge.To, -najszersze[edge.To]);
                    }
                }
            }

            if (prev[end] is null)
                return [];

            List<int> result = new();
            int index = end;
            while (index != start)
            {
                result.Add(index);
                index = prev[index].Value;
            }
            result.Add(start);
            result.Reverse();

            return result;
        }

        public List<int> WeightedWidePath(DiGraph<int> G, int start, int end, int[] weights, int maxWeight)
        {
            int n = G.VertexCount;
            SortedSet<int> edgeWeights = new();
            for (int i = 0; i < n; i++)
                foreach (var edge in G.OutEdges(i))
                    edgeWeights.Add(edge.Weight);

            int maxDifference = int.MinValue;
            List<int> bestPath = [];

            foreach (var minWei in edgeWeights)
            {
                var najmniejszy = new int[n];
                var prev = new int?[n];
                System.Collections.Generic.PriorityQueue<int, int> queue = new();
                for (int i = 0; i < n; i++)
                {
                    if (i == start)
                        najmniejszy[i] = weights[i];
                    else
                        najmniejszy[i] = int.MaxValue;
                    queue.Enqueue(i, najmniejszy[i]);
                }

                while (queue.Count > 0)
                {
                    queue.TryDequeue(out var u, out var priority);
                    if (u == end)
                        break;
                    if (priority != najmniejszy[u])
                        continue;
                    foreach (var edge in G.OutEdges(u))
                    {
                        if (edge.Weight >= minWei && najmniejszy[u] + weights[edge.To] < najmniejszy[edge.To])
                        {
                            najmniejszy[edge.To] = najmniejszy[u] + weights[edge.To];
                            prev[edge.To] = u;
                            queue.Enqueue(edge.To, najmniejszy[edge.To]);
                        }
                    }
                }

                List<int> result = new();
                int index = end;
                int sumaWag = 0;
                bool failure = false;
                while (index != start && result.Count <= n)
                {
                    result.Add(index);
                    sumaWag += weights[index];
                    try
                    {
                        index = prev[index].Value;
                    }
                    catch(InvalidOperationException)
                    {
                        failure = true;
                        break;
                    }
                }
                if (failure || result.Count >= n)
                    continue;
                result.Add(start);
                sumaWag += weights[start];
                result.Reverse();

                if (minWei - sumaWag > maxDifference)
                {
                    maxDifference = minWei - sumaWag;
                    bestPath = result;
                }
            }
            return bestPath;
        }
    }
}