using ASD.Graphs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ASD
{
    public class Lab04 : MarshalByRefObject
    {
        /// <summary>
        /// Etap 1 - Wyznaczenie liczby oraz listy zainfekowanych serwisów po upływie K dni.
        /// Algorytm analizuje propagację infekcji w grafie i zwraca wszystkie dotknięte nią serwisy.
        /// </summary>
        /// <param name="G">Graf reprezentujący infrastrukturę serwisów.</param>
        /// <param name="K">Liczba dni propagacji infekcji.</param>
        /// <param name="s">Indeks początkowo zainfekowanego serwisu.</param>
        /// <returns>
        /// (int numberOfInfectedServices, int[] listOfInfectedServices) - 
        /// numberOfInfectedServices: liczba zainfekowanych serwisów,
        /// listOfInfectedServices: tablica zawierająca numery zainfekowanych serwisów w kolejności rosnącej.
        /// </returns>
        public (int numberOfInfectedServices, int[] listOfInfectedServices) Stage1(Graph G, int K, int s)
        {
            int n = G.VertexCount;
            int m = G.EdgeCount;

            var zakazony = new int?[n];
            Queue<int> queue = new Queue<int>();
            queue.Enqueue(s);
            zakazony[s] = 1;
            while (queue.Count > 0)
            {
                var v = queue.Dequeue();
                foreach (var neigh in G.OutNeighbors(v))
                {
                    if ((zakazony[neigh] is null && zakazony[v] + 1 <= K) 
                        || (zakazony[neigh] != null && zakazony[v] + 1 < zakazony[neigh]))
                    {
                        zakazony[neigh] = zakazony[v] + 1;
                        queue.Enqueue(neigh);
                    }
                }
            }
            List<int> lista_zakazonych = new List<int>();
            for (int i = 0; i < n; i++)
                if (zakazony[i] != null)
                    lista_zakazonych.Add(i);
            return (lista_zakazonych.Count, lista_zakazonych.ToArray());
        }

        /// <summary>
        /// Etap 2 - Wyznaczenie liczby oraz listy zainfekowanych serwisów przy uwzględnieniu wyłączeń.
        /// Algorytm analizuje propagację infekcji z możliwością wcześniejszego wyłączania serwisów.
        /// </summary>
        /// <param name="G">Graf reprezentujący infrastrukturę serwisów.</param>
        /// <param name="K">Liczba dni propagacji infekcji.</param>
        /// <param name="s">Tablica początkowo zainfekowanych serwisów.</param>
        /// <param name="serviceTurnoffDay">Tablica zawierająca dzień, w którym dany serwis został wyłączony (K + 1 oznacza brak wyłączenia).</param>
        /// <returns>
        /// (int numberOfInfectedServices, int[] listOfInfectedServices) - 
        /// numberOfInfectedServices: liczba zainfekowanych serwisów,
        /// listOfInfectedServices: tablica zawierająca numery zainfekowanych serwisów w kolejności rosnącej.
        /// </returns>
        public (int numberOfInfectedServices, int[] listOfInfectedServices) Stage2(Graph G, int K, int[] s, int[] serviceTurnoffDay)
        {
            int n = G.VertexCount;
            int m = G.EdgeCount;
            int p = s.Length;

            var zakazony = new int?[n];
            Queue<int> queue = new Queue<int>();
            foreach (var v in s)
            {
                queue.Enqueue(v);
                zakazony[v] = 1;
            }
            while (queue.Count > 0)
            {
                var v = queue.Dequeue();
                foreach (var neigh in G.OutNeighbors(v))
                {
                    if ((zakazony[neigh] is null && zakazony[v] + 1 <= K)
                        || (zakazony[neigh] != null && zakazony[v] + 1 < zakazony[neigh]))
                    {
                        if (zakazony[v] + 1 < serviceTurnoffDay[v] 
                            && zakazony[v] + 1 < serviceTurnoffDay[neigh])
                        {
                            zakazony[neigh] = zakazony[v] + 1;
                            queue.Enqueue(neigh); 
                        }
                    }
                }
            }

            List<int> lista_zakazonych = new List<int>();
            for (int i = 0; i < n; i++)
                if (zakazony[i] != null)
                    lista_zakazonych.Add(i);
            return (lista_zakazonych.Count, lista_zakazonych.ToArray());
        }

        /// <summary>
        /// Etap 3 - Wyznaczenie liczby oraz listy zainfekowanych serwisów z możliwością ponownego włączenia wyłączonych serwisów.
        /// Algorytm analizuje propagację infekcji uwzględniając serwisy, które mogą być ponownie uruchamiane po określonym czasie.
        /// </summary>
        /// <param name="G">Graf reprezentujący infrastrukturę serwisów.</param>
        /// <param name="K">Liczba dni propagacji infekcji.</param>
        /// <param name="s">Tablica początkowo zainfekowanych serwisów.</param>
        /// <param name="serviceTurnoffDay">Tablica zawierająca dzień, w którym dany serwis został wyłączony (K + 1 oznacza brak wyłączenia).</param>
        /// <param name="serviceTurnonDay">Tablica zawierająca dzień, w którym dany serwis został ponownie włączony.</param>
        /// <returns>
        /// (int numberOfInfectedServices, int[] listOfInfectedServices) - 
        /// numberOfInfectedServices: liczba zainfekowanych serwisów,
        /// listOfInfectedServices: tablica zawierająca numery zainfekowanych serwisów w kolejności rosnącej.
        /// </returns>
        public (int numberOfInfectedServices, int[] listOfInfectedServices) Stage3(Graph G, int K, int[] s, int[] serviceTurnoffDay, int[] serviceTurnonDay)
        {
            int n = G.VertexCount;
            int m = G.EdgeCount;
            int p = s.Length;

            var zakazony = new int?[n];
            Queue<int> queue = new Queue<int>();
            foreach (var v in s)
            {
                queue.Enqueue(v);
                if (serviceTurnoffDay[v] == 1)
                    zakazony[v] = serviceTurnonDay[v];
                else
                    zakazony[v] = 1;
            }
            while (queue.Count > 0)
            {
                var v = queue.Dequeue();
                foreach (var neigh in G.OutNeighbors(v))
                {
                    int dzienZakazenia = (int)zakazony[v] + 1;
                    if ((zakazony[neigh] is null && dzienZakazenia <= K)
                        || (zakazony[neigh] != null && dzienZakazenia < zakazony[neigh]))
                    {
                        if (dzienZakazenia >= serviceTurnoffDay[v] && dzienZakazenia <= serviceTurnonDay[v])
                            dzienZakazenia = serviceTurnonDay[v] + 1;
                        if (dzienZakazenia >= serviceTurnoffDay[neigh] &&
                                 dzienZakazenia <= serviceTurnonDay[neigh])
                            dzienZakazenia = serviceTurnonDay[neigh] + 1;
                        if (dzienZakazenia >= serviceTurnoffDay[v] && dzienZakazenia <= serviceTurnonDay[v])
                            dzienZakazenia = serviceTurnonDay[v] + 1;
                        if (dzienZakazenia < K + 1 && !(zakazony[neigh] != null && dzienZakazenia < zakazony[neigh]))
                        {
                            zakazony[neigh] = dzienZakazenia;
                            queue.Enqueue(neigh);
                        }
                    }
                }
            }

            List<int> lista_zakazonych = new List<int>();
            for (int i = 0; i < n; i++)
                if (zakazony[i] != null)
                    lista_zakazonych.Add(i);
            return (lista_zakazonych.Count, lista_zakazonych.ToArray());
        }
    }
}
