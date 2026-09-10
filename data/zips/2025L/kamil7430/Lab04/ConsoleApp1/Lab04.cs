using System;
using ASD.Graphs;
using System.Collections.Generic;

namespace ASD
{
    public class Lab04 : MarshalByRefObject
    {
        /// <summary>
        /// Etap 1 - Szukanie mozliwych do odwiedzenia miast z grafu skierowanego
        /// przy zalozeniu, ze pociagi odjezdzaja co godzine.
        /// </summary>
        /// <param name="graph">Graf skierowany przedstawiający siatke pociagow</param>
        /// <param name="miastoStartowe">Numer miasta z ktorego zaczyna sie podroz pociagiem</param>
        /// <param name="K">Godzina o ktorej musi zakonczyc sie nasza podroz</param>
        /// <returns>Tablica numerow miast ktore mozna odwiedzic. Posortowana rosnaco.</returns>
        public int[] Lab04Stage1(DiGraph graph, int miastoStartowe, int K)
        {
            int n = graph.VertexCount;
            var odwiedzone = new int?[n];
            int ilosc = 1;
            Queue<int> queue = new();
            odwiedzone[miastoStartowe] = 8;
            queue.Enqueue(miastoStartowe);
            while(queue.Count > 0)
            {
                var v = queue.Dequeue();
                if(odwiedzone[v] != K)
                    foreach (var neigh in graph.OutNeighbors(v))
                    {
                        if (odwiedzone[neigh] == null)
                        {
                            odwiedzone[neigh] = odwiedzone[v] + 1;
                            ilosc++;
                            queue.Enqueue(neigh);
                        }
                    }
            }
            var doZwrotu = new int[ilosc];
            int j = 0;
            for (int i = 0; i < n; i++)
            {
                if (odwiedzone[i] != null)
                {
                    doZwrotu[j] = i;
                    j++;
                }
            }
            return doZwrotu;
        }

        /// <summary>
        /// Etap 2 - Szukanie mozliwych do odwiedzenia miast z grafu skierowanego.
        /// Waga krawedzi oznacza, ze pociag rusza o tej godzinie
        /// </summary>
        /// <param name="graph">Wazony graf skierowany przedstawiający siatke pociagow</param>
        /// <param name="miastoStartowe">Numer miasta z ktorego zaczyna sie podroz pociagiem</param>
        /// <param name="K">Godzina o ktorej musi zakonczyc sie nasza podroz</param>
        /// <returns>Tablica numerow miast ktore mozna odwiedzic. Posortowana rosnaco.</returns>
        public int[] Lab04Stage2(DiGraph<int> graph, int miastoStartowe, int K)
        {
            int n = graph.VertexCount;
            var godzDotarcia = new int?[n];
            godzDotarcia[miastoStartowe] = 8;
            Queue<int> queue = new();
            queue.Enqueue(miastoStartowe);
            while(queue.Count > 0)
            {
                int v = queue.Dequeue();
                if (godzDotarcia[v] >= K)
                    continue;
                foreach (var edge in graph.OutEdges(v))
                {
                    if (godzDotarcia[edge.From] <= edge.Weight)
                        if (godzDotarcia[edge.To] is null || godzDotarcia[edge.To] > edge.Weight + 1)
                        {
                            godzDotarcia[edge.To] = edge.Weight + 1;
                            queue.Enqueue(edge.To);
                        }
                }
            }
            List<int> doZwrotu = [];
            for (int i = 0; i < n; i++)
                if (godzDotarcia[i] <= K)
                    doZwrotu.Add(i);
            return doZwrotu.ToArray();
        }
    }
}
