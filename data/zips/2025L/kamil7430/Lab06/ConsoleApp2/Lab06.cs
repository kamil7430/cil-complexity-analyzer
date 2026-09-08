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
            DiGraph<int> pomoc = new(n);

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
                return (s, 0, [s]);

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
            DiGraph<int> pomoc_koszty = new(n);

            for (int i = 0; i < n; i++)
                foreach (var v in G.OutNeighbors(i))
                    pomoc_koszty.AddEdge(i, v, C.GetEdgeWeight(i, v));

            var koszty_sciezki = Paths.Dijkstra(pomoc_koszty, s);
            if (!koszty_sciezki.Reachable(s, t))
                return null;

            int koszt = koszty_sciezki.GetDistance(s, t);

            return (-1, koszt, []);
        }
    }
}