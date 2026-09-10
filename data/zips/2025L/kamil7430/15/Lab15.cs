using System;
using ASD.Graphs;
using ASD;
using System.Collections.Generic;
using System.Linq;

namespace ASD
{

    public class Lab15 : System.MarshalByRefObject
    {
        private void DoSomethingWithProvinces(ref Graph G1, Action<(int, int)?, Stack<(int, int)>> doSth)
        {
            int n = G1.VertexCount;
            var visitTime = new int?[n];
            var lowTime = new int?[n];
            var visited = new bool[n];
            var blockStack = new Stack<(int u, int v)>();

            for (int i = 0; i < n; i++)
                if (!visited[i])
                {
                    DFS(ref G1, i, null, 1);

                    if (blockStack.Count > 2)
                        doSth(null, blockStack);
                    else
                        blockStack.Clear();
                }

            void DFS(ref Graph G, int u, int? prev, int time)
            {
                visited[u] = true;
                visitTime[u] = lowTime[u] = time;

                int children = 0;

                foreach (var v in G.OutNeighbors(u))
                {
                    if (v == prev)
                        continue;

                    if (!visited[v])
                    {
                        children++;
                        blockStack.Push((u, v));
                        DFS(ref G, v, u, time + 1);

                        lowTime[u] = Math.Min(lowTime[u].Value, lowTime[v].Value);

                        if ((prev != null && lowTime[v].Value >= visitTime[u].Value)
                            || (prev is null && children > 1))
                        {
                            doSth((u, v), blockStack);
                        }
                    }
                    else if (visitTime[v].Value < visitTime[u].Value)
                    {
                        lowTime[u] = Math.Min(lowTime[u].Value, visitTime[v].Value);
                        blockStack.Push((u, v));
                    }
                }
            }
        }
        
        /// <summary>
        /// Etap 1: Rozmiar najliczniejszej krainy w zadanym grafie (2.5p)
        /// 
        /// Przez krainę rozumiemy maksymalny zbiór wierzchołków, z których
        /// każde dwa należą do jakiegoś cyklu (równoważnie: najliczniejszy
        /// zbiór wierzchołków G indukujący podgraf 2-spójny wierzchołkowo).
        /// 
        /// Uwaga: Z powyższej definicji wynika, że zbiór pusty jest krainą, 
        /// a zbiór jednoelementowy nie.
        /// </summary>
        /// <param name="G">Graf prosty</param>
        /// <returns>Rozmiar największej bańki</returns>
        public int MaxProvinceSize(Graph G)
        {
            int n = G.VertexCount;
            var visited = new int[n];
            int maxProvince = -1;

            int i = 1;
            DoSomethingWithProvinces(ref G, 
                (tuple, stack) => // stack jest referencją
                {
                    int vertices = 0;

                    while (stack.Count > 0)
                    {
                        var (u, v) = stack.Extract();
                        if (visited[u] != i)
                        {
                            visited[u] = i;
                            vertices++;
                        }
                        if (visited[v] != i)
                        {
                            visited[v] = i;
                            vertices++;
                        }

                        if (tuple != null && (u, v).Equals(tuple.Value))
                            break;
                    }

                    if (vertices > maxProvince)
                        maxProvince = vertices;
                    i++;
                });

            return maxProvince <= 2 ? 0 : maxProvince;
        }

        /// <summary>
        /// Etap 2: Wierzchołek znajdujący się w największej liczbie krain (2.5p)
        /// 
        /// Funkcja zwraca wierzchołek znajdujący się w największej liczbie krain.
        /// 
        /// W przypadku remisu należy zwrócić wierzchołek o mniejszym numerze.
        /// </summary>
        /// <param name="G"></param>
        /// <returns></returns>
        public int VertexInMostProvinces(Graph G)
        {
            int n = G.VertexCount;
            var visited = new int[n];
            var provinces = new int[n];

            int i = 1;
            DoSomethingWithProvinces(ref G, 
                (tuple, stack) =>
                {
                    if (tuple != null)
                    {
                        var top1 = stack.Extract();
                        if (tuple.Value.Equals(top1))
                            return;
                        var top2 = stack.Extract();
                        if (tuple.Value.Equals(top2))
                            return;
                        stack.Push(top2);
                        stack.Push(top1);
                    }
                    
                    // Console.WriteLine("\n---------");
                    while (stack.Count > 0)
                    {
                        var (u, v) = stack.Extract();
                        // Console.Write($"({u},{v});");
                        if (visited[u] != i)
                        {
                            visited[u] = i;
                            provinces[u]++;
                        }
                        if (visited[v] != i)
                        {
                            visited[v] = i;
                            provinces[v]++;
                        }

                        if (tuple != null && (u, v).Equals(tuple.Value))
                            break;
                    }
                    
                    i++;
                });

            int maxIndex = 0;
            for (int j = 1; j < n; j++)
                if (provinces[j] > provinces[maxIndex])
                    maxIndex = j;
            
            return maxIndex;
        }
    }
}

// var visited = new bool[n];
// var stack = new Stack<int>();
//
// for (int i = 0; i < n; i++)
// {
//     if (visited[i] && G.Degree(i) <= 1)
//         continue;
//
//     //visited[i] = true;
//     stack.Push(i);
//     int actualLand = 0;
//
//     while (stack.Count > 0)
//     {
//         int v = stack.Extract();
//         actualLand++;
//         foreach (var neigh in G.OutNeighbors(v))
//         {
//             if (!visited[neigh])
//             {
//                 visited[neigh] = true;
//                 stack.Push(neigh);
//             }
//         }
//     }
//
//     if (actualLand > maxLandSize)
//         maxLandSize = actualLand;
// }

// int FindBridges(int? prev, int v, int number)
// {
//     numbering[v] = number;
//     int low = number;
//     
//     foreach (var neigh in G.OutNeighbors(v))
//     {
//         if (neigh == prev)
//             continue;
//         if (numbering[neigh] == null)
//         {
//             int tmp = FindBridges(v, neigh, number + 1);
//             if (tmp < low)
//                 low = tmp;
//         }
//         else if (numbering[neigh].Value < low)
//             low = numbering[neigh].Value;
//     }
//
//     actualLandSize++;
//     lows[v] = low;
//     if (low == number && prev != null)
//     {
//         edgesToRemove.AddLast((prev.Value, v));
//         if (actualLandSize > maxLandSize)
//             maxLandSize = actualLandSize;
//         actualLandSize = 1;
//     }
//
//     return low;
// }

// void DFS(int v)
// {
//     if (isArticulationPoint[v])
//     {
//         if (articulationPointCounted[v] != actualStartingVertex)
//         {
//             actualLandSize++;
//             articulationPointCounted[v] = actualStartingVertex;
//         }
//     }
//     else if (!visited[v])
//     {
//         visited[v] = true;
//         actualLandSize++;
//         foreach (var u in G.OutNeighbors(v))
//         {
//             if (!visited[u])
//                 DFS(u);
//         }
//     }
// }

// // Przejście DFS-em i znalezienie największej krainy.
// int maxLandSize = 0;
// int actualLandSize = 0;
//
// var visited = new bool[n];
// var articulationPointCounted = new int?[n];
// int actualStartingVertex = 0;
// int count = 0;
//
// for (actualStartingVertex = 0; actualStartingVertex < n; actualStartingVertex++)
//     if (!visited[actualStartingVertex] && !isArticulationPoint[actualStartingVertex])
//     {
//         actualLandSize = 0;
//         DFS(actualStartingVertex);
//         if (actualLandSize > maxLandSize)
//             maxLandSize = actualLandSize;
//         count++;
//     }

// Graph G = (Graph)G_original.Clone();
// int n = G.VertexCount;
//
// var D = new int?[n];
// var isArticulationPoint = new bool[n];
//
// int start;
//
// // Znajdowanie punktów artykulacji
// for (start = 0; start < n; start++)
//     if (D[start] == null)
//     {
//         int nc = 0;
//         D[start] = 1;
//         
//         foreach (var u in G.OutNeighbors(start))
//         {
//             if (D[u] == null)
//             {
//                 nc++;
//                 FindArticulationPoints(u, start, 2);
//             }
//         }
//         
//         if (nc > 1)
//             isArticulationPoint[start] = true;
//     }
//
// return maxLandSize <= 2 ? 0 : maxLandSize;
//
// int FindArticulationPoints(int v, int vf, int number)
// {
//     D[v] = number;
//     int low = number;
//     bool test = false;
//
//     foreach (var u in G.OutNeighbors(v))
//     {
//         if (u == vf)
//             continue;
//         
//         if (D[u] == null)
//         {
//             int temp = FindArticulationPoints(u, v, number + 1);
//
//             if (temp < low)
//                 low = temp;
//             if (temp >= D[v])
//                 test = true;
//         }
//         
//         else if (D[u] < low)
//         {
//             low = D[u].Value;
//         }
//     }
//
//     if (test)
//         isArticulationPoint[v] = true;
//     
//     return low;
// }