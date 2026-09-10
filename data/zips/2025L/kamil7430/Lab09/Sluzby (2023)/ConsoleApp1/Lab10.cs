using ASD.Graphs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lab10
{
    public class Lab10Solution : MarshalByRefObject
    {
        /// <summary>
        /// Wariant 1: Znajdź najtańszy zbiór wierzchołków grafu G 
        /// rozdzielający wszystkie pary wierzchołków z listy fanclubs 
        /// </summary>
        /// <param name="G">Graf prosty</param>
        /// <param name="fanclubs">Lista wierzchołków, które należy rozdzielić</param>
        /// <param name="cost">cost[v] to koszt użycia wierzchołka v; koszty są nieujemne</param>
        /// <param name="maxBudget">Górne ograniczenie na koszt rozwiązania</param>
        /// <returns></returns>
        public List<int> FindSeparatingSet(Graph G, List<int> fanclubs, int[] cost, int maxBudget)
        {
            int n = G.VertexCount;
            LinkedList<int> S = [];
            int costS = 0;
            LinkedList<int> bestS = [];
            int costBestS = int.MaxValue;

            GenerateSubsets(0);

            return bestS.ToList();

            bool DFS()
            {
                foreach (var fc in fanclubs)
                {
                    if (S.Contains(fc))
                        continue;
                    var stack = new Stack<int>();
                    var visited = new bool[n];
                    stack.Push(fc);
                    visited[fc] = true;
                    while (stack.Count > 0)
                    {
                        int v = stack.Pop();
                        foreach (var nei in G.OutNeighbors(v))
                        {
                            if (!visited[nei] && !S.Contains(nei))
                            {
                                if (nei != fc && fanclubs.Contains(nei))
                                    return false;
                                visited[nei] = true;
                                stack.Push(nei);
                            }
                        }
                    }
                }
                return true;
            }

            void GenerateSubsets(int k)
            {
                if (costS >= costBestS)
                    return;
                if (DFS())
                {
                    bestS = new(S);
                    costBestS = costS;
                }
                //PrintList(S);
                for (int m = k; m < n; m++)
                {
                    S.AddLast(m);
                    costS += cost[m];
                    if (costS <= maxBudget)
                        GenerateSubsets(m + 1);
                    costS -= cost[m];
                    S.RemoveLast();
                }
            }
        }

        private void PrintList(LinkedList<int> list)
        {
            foreach(var i in list)
                Console.Write($"{i} ");
            Console.WriteLine();
        }

        /// <summary>
        /// Wariant 2: Znajdź najtańszy spójny zbiór wierzchołków grafu G 
        /// rozdzielający wszystkie pary wierzchołków z listy fanclubs 
        /// </summary>
        /// <param name="G">Graf prosty</param>
        /// <param name="fanclubs">Lista wierzchołków, które należy rozdzielić</param>
        /// <param name="cost">cost[v] to koszt użycia wierzchołka v; koszty są nieujemne</param>
        /// <param name="maxBudget">Górne ograniczenie na koszt rozwiązania</param>
        /// <returns></returns>
        public List<int> FindConnectedSeparatingSet(Graph G, List<int> fanclubs, int[] cost, int maxBudget)
        {
            int n = G.VertexCount;
            LinkedList<int> S = [];
            int costS = 0;
            LinkedList<int> bestS = [];
            int costBestS = int.MaxValue;

            GenerateSubsets(0);

            return costBestS == int.MaxValue ? null! : bestS.ToList();

            bool PoliceStationsConnected()
            {
                if (S.Count == 0)
                    return false;
                var queue = new Queue<int>();
                var visited = new bool[n];
                queue.Enqueue(S.First.Value);
                visited[S.First.Value] = true;
                while (queue.Count > 0)
                {
                    int v = queue.Dequeue();
                    foreach (var nei in G.OutNeighbors(v))
                    {
                        if (S.Contains(nei) && !visited[nei])
                        {
                            visited[nei] = true;
                            queue.Enqueue(nei);
                        }
                    }
                }
                foreach (var i in S)
                    if (!visited[i])
                        return false;
                return true;
            }

            bool DFS()
            {
                foreach (var fc in fanclubs)
                {
                    if (S.Contains(fc))
                        continue;
                    var stack = new Stack<int>();
                    var visited = new bool[n];
                    stack.Push(fc);
                    visited[fc] = true;
                    while (stack.Count > 0)
                    {
                        int v = stack.Pop();
                        foreach (var nei in G.OutNeighbors(v))
                        {
                            if (!visited[nei] && !S.Contains(nei))
                            {
                                if (nei != fc && fanclubs.Contains(nei))
                                    return false;
                                visited[nei] = true;
                                stack.Push(nei);
                            }
                        }
                    }
                }
                return true;
            }

            void GenerateSubsets(int k)
            {
                if (costS >= costBestS)
                    return;
                if (PoliceStationsConnected() && DFS())
                {
                    bestS = new(S);
                    costBestS = costS;
                }
                //PrintList(S);
                for (int m = k; m < n; m++)
                //foreach(var i in S)
                //foreach(var m in G.OutNeighbors(i))
                {
                    //if (m <= k)
                    //    continue;
                    S.AddLast(m);
                    costS += cost[m];
                    if (costS <= maxBudget)
                        GenerateSubsets(m + 1);
                    costS -= cost[m];
                    S.RemoveLast();
                }
            }
        }
    }
}
