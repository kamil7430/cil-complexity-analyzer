using ASD.Graphs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ASD
{
    public class Lab06 : MarshalByRefObject
    {
        /// <summary>Etap I</summary>
        /// <param name="G">Graf opisujący połączenia szlakami turystycznymi z podanym czasem przejścia krawędzi w wadze.</param>
        /// <param name="waitTime">Czas oczekiwania Studenta-Podróżnika w danym wierzchołku.</param>
        /// <param name="s">Wierzchołek startowy (początek trasy).</param>
        /// <returns>Pierwszy element krotki to wierzchołek końcowy szukanej trasy. Drugi element to długość trasy w minutach. Trzeci element to droga będąca rozwiązaniem: sekwencja odwiedzanych wierzchołków (zawierająca zarówno wierzchołek początkowy, jak i końcowy).</returns>
        public (int t, int l, int[] path) Stage1(DiGraph<int> G, int[] waitTime, int s)
        {
            int n = G.VertexCount;
            DiGraph<int> pomoc = new DiGraph<int>(n);

            for (int i = 0; i < n; i++)
                foreach (var e in G.OutEdges(i))
                    pomoc.AddEdge(i, e.To, waitTime[i] + e.Weight);

            var dijkstra = Paths.Dijkstra(pomoc, s);
            int maximum = -1, maxInd = -1;
            bool reachable = false;
            for (int i = 0; i < n; i++)
                if (i != s && dijkstra.Reachable(s, i))
                    if (maximum < dijkstra.GetDistance(s, i))
                    {
                        reachable = true;
                        maximum = dijkstra.GetDistance(s, i);
                        maxInd = i;
                    }

            if (!reachable)
                return (s, 0, new[] {s});

            return (maxInd, maximum - waitTime[s], dijkstra.GetPath(s, maxInd));
        }

        /// <summary>Etap II</summary>
        /// <param name="G">Graf opisujący połączenia szlakami turystycznymi z podanym czasem przejścia krawędzi w wadze.</param>
        /// <param name="C">Graf opisujący koszty przejścia krawędziami w grafie G.</param>
        /// <param name="waitTime">Czas oczekiwania Studenta-Podróżnika w danym wierzchołku.</param>
        /// <param name="s">Wierzchołek startowy (początek trasy).</param>
        /// <param name="t">Wierzchołek końcowy (koniec trasy).</param>
        /// <returns>Pierwszy element krotki to długość trasy w minutach. Drugi element to koszt przebycia trasy w złotych. Trzeci element to droga będąca rozwiązaniem: sekwencja odwiedzanych wierzchołków (zawierająca zarówno wierzchołek początkowy, jak i końcowy). Jeśli szukana trasa nie istnieje, funkcja zwraca `null`.</returns>
        public (int l, int c, int[] path)? Stage2(DiGraph<int> G, Graph<int> C, int[] waitTime, int s, int t)
        {
            int n = G.VertexCount;
            DiGraph<int> pomoc_koszty = new DiGraph<int>(n);

            for (int i = 0; i < n; i++)
                foreach (var v in G.OutNeighbors(i))
                    pomoc_koszty.AddEdge(i, v, C.GetEdgeWeight(i, v));

            var koszty_sciezki = Paths.Dijkstra(pomoc_koszty, s);
            if (!koszty_sciezki.Reachable(s, t))
                return null;

            int koszt = koszty_sciezki.GetDistance(s, t);
            
            if (koszt == 0)
                return (s, 0, new[] {s});

            var pomoc_G = new DiGraph<long>(n);

            for (int i = 0; i < n; i++)
                foreach (var e in G.OutEdges(i))
                {
                    long waga = C.GetEdgeWeight(i, e.To);
                    waga <<= 32;
                    waga += waitTime[i] + e.Weight;
                    pomoc_G.AddEdge(i, e.To, waga);
                }

            var sciezka = Paths.Dijkstra(pomoc_G, s);
            var dystans = (sciezka.GetDistance(s, t) - waitTime[s]) & int.MaxValue;
            var tablica_sciezka = sciezka.GetPath(s, t);
            
            return ((int)dystans, koszt, tablica_sciezka);

            // int k = -waitTime[s];
            // for (int i = 0; i < sci.Length - 1; i++)
            //     k += pomoc_G.GetEdgeWeight(sci[i], sci[i + 1]);
            //Console.WriteLine(k);

            // var queue = new SafePriorityQueue<int, int>();
            // var cost = new int[n];
            // for (int i = 0; i < n; i++)
            // {
            //     if (i != s)
            //         cost[i] = int.MaxValue / 2;
            //     queue.Insert(i, cost[i]);
            // }
            //
            // var final = new DiGraph<int>(n);
            // while (queue.Count > 0)
            // {
            //     var u = queue.Extract();
            //     if (cost[u] > koszt)
            //         continue;
            //     foreach (var e in pomoc_G.OutEdges(u))
            //     {
            //         if (cost[u] + C.GetEdgeWeight(u, e.To) <= koszt)
            //             final.AddEdge(u, e.To, e.Weight);
            //         if (cost[u] + C.GetEdgeWeight(u, e.To) < cost[e.To])
            //         {
            //             cost[e.To] = cost[u] + C.GetEdgeWeight(u, e.To);
            //             queue.UpdatePriority(e.To, cost[e.To]);
            //         }
            //     }
            // }
            //
            // var sciezka = Paths.Dijkstra(final, s);
            // var dystans = sciezka.GetDistance(s, t) - waitTime[s];
            // var tablica_sciezka = sciezka.GetPath(s, t);
            //
            // return (dystans, koszt, tablica_sciezka);

            // var queue = new SafePriorityQueue<int, int>();
            // var odleglosc = new int[n];
            // var cost = new int[n];
            // var prevs = new int?[n];
            // for (int i = 0; i < n; i++)
            // {
            //     if (i == s)
            //         odleglosc[i] = 0;
            //     else
            //     {
            //         odleglosc[i] = int.MaxValue / 2;
            //         cost[i] = koszt;
            //     }
            //     queue.Insert(i, odleglosc[i]);
            // }
            //
            // while (queue.Count > 0)
            // {
            //     var u = queue.Extract();
            //     foreach (var e in pomoc_G.OutEdges(u))
            //     {
            //         if (cost[u] + C.GetEdgeWeight(u, e.To) <= koszt)
            //             if (odleglosc[e.To] > odleglosc[u] + e.Weight)
            //             {
            //                 odleglosc[e.To] = odleglosc[u] + e.Weight;
            //                 cost[e.To] = cost[u] + C.GetEdgeWeight(u, e.To);
            //                 prevs[e.To] = u;
            //                 queue.UpdatePriority(e.To, odleglosc[e.To]);
            //             }
            //     }
            // }
            //
            // var sciezka = new List<int>();
            // int index = t;
            // while (index != s)
            // {
            //     sciezka.Add(index);
            //     index = prevs[index].Value;
            // }
            // sciezka.Add(s);
            // sciezka.Reverse();

            // return (odleglosc[t] - waitTime[s], koszt, sciezka.ToArray());
        }
    }
}