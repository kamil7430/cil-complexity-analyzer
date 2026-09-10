using System;
using ASD.Graphs;
using ASD;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace ASD
{

    public class Lab03GraphFunctions : System.MarshalByRefObject
    {

        // Część 1
        // Wyznaczanie odwrotności grafu
        //   0.5 pkt
        // Odwrotność grafu to graf skierowany o wszystkich krawędziach przeciwnie skierowanych niż w grafie pierwotnym
        // Parametry:
        //   g - graf wejściowy
        // Wynik:
        //   odwrotność grafu
        // Uwagi:
        //   1) Graf wejściowy pozostaje niezmieniony
        //   2) Graf wynikowy musi być w takiej samej reprezentacji jak wejściowy
        public DiGraph Lab03Reverse(DiGraph g)
        {
            DiGraph G = new(g.VertexCount, g.Representation);
            for (int i = 0; i < g.VertexCount; i++)
            {
                foreach (var outNeighbor in g.OutNeighbors(i))
                {
                    G.AddEdge(outNeighbor, i);
                }
            }
            return G;
        }

        // Część 2
        // Badanie czy graf jest dwudzielny
        //   0.5 pkt
        // Graf dwudzielny to graf nieskierowany, którego wierzchołki można podzielić na dwa rozłączne zbiory
        // takie, że dla każdej krawędzi jej końce należą do róźnych zbiorów
        // Parametry:
        //   g - badany graf
        //   vert - tablica opisująca podział zbioru wierzchołków na podzbiory w następujący sposób
        //          vert[i] == 1 oznacza, że wierzchołek i należy do pierwszego podzbioru
        //          vert[i] == 2 oznacza, że wierzchołek i należy do drugiego podzbioru
        // Wynik:
        //   true jeśli graf jest dwudzielny, false jeśli graf nie jest dwudzielny (w tym przypadku parametr vert ma mieć wartość null)
        // Uwagi:
        //   1) Graf wejściowy pozostaje niezmieniony
        //   2) Podział wierzchołków może nie być jednoznaczny - znaleźć dowolny
        //   3) Pamiętać, że każdy z wierzchołków musi być przyporządkowany do któregoś ze zbiorów
        //   4) Metoda ma mieć taki sam rząd złożoności jak zwykłe przeszukiwanie (za większą będą kary!)
        public bool Lab03IsBipartite(Graph g, out int[] vert)
        {
            int n = g.VertexCount;
            var visited = new bool[n];
            vert = new int[n];
            Queue<int> queue = new();
            for (int i = 0; i < n; i++)
            {
                if (visited[i])
                    continue;
                queue.Enqueue(i);
                visited[i] = true;
                vert[i] = 1;
                while (queue.Count > 0)
                {
                    int v = queue.Dequeue();
                    int color = (vert[v] == 1) ? 2 : 1;
                    foreach (var outNeighbor in g.OutNeighbors(v))
                    {
                        if (vert[outNeighbor] != 0 && vert[outNeighbor] != color)
                        {
                            vert = null;
                            return false;
                        }
                        vert[outNeighbor] = color;
                        if (!visited[outNeighbor])
                        {
                            visited[outNeighbor] = true;
                            queue.Enqueue(outNeighbor);
                        }
                    }
                }
            }
            return true;
        }

        // Część 3
        // Wyznaczanie minimalnego drzewa rozpinającego algorytmem Kruskala
        //   1 pkt
        // Schemat algorytmu Kruskala
        //   1) wrzucić wszystkie krawędzie do "wspólnego worka"
        //   2) wyciągać z "worka" krawędzie w kolejności wzrastających wag
        //      - jeśli krawędź można dodać do drzewa to dodawać, jeśli nie można to ignorować
        //      - punkt 2 powtarzać aż do skonstruowania drzewa (lub wyczerpania krawędzi)
        // Parametry:
        //   g - graf wejściowy
        //   mstw - waga skonstruowanego drzewa (lasu)
        // Wynik:
        //   skonstruowane minimalne drzewo rozpinające (albo las)
        // Uwagi:
        //   1) Graf wejściowy pozostaje niezmieniony
        //   2) Wykorzystać klasę UnionFind z biblioteki Graph
        //   3) Jeśli graf g jest niespójny to metoda wyznacza las rozpinający
        //   4) Graf wynikowy (drzewo) musi być w takiej samej reprezentacji jak wejściowy
        public Graph<int> Lab03Kruskal(Graph<int> g, out int mstw)
        {
            int n = g.VertexCount;
            PriorityQueue<int, Edge<int>> pq = new();
            UnionFind uf = new(n);
            Graph<int> G = new(n, g.Representation);
            mstw = 0;
            foreach (var edge in g.BFS().SearchAll())
                pq.Insert(edge, edge.Weight);
            while (pq.Count > 0)
            {
                var edge = pq.Extract();
                if (uf.Find(edge.From) != uf.Find(edge.To))
                {
                    uf.Union(edge.From, edge.To);
                    G.AddEdge(edge.From, edge.To);
                    mstw += edge.Weight;
                }
            }
            return G;
        }

        // Część 4
        // Badanie czy graf nieskierowany jest acykliczny
        //   0.5 pkt
        // Parametry:
        //   g - badany graf
        // Wynik:
        //   true jeśli graf jest acykliczny, false jeśli graf nie jest acykliczny
        // Uwagi:
        //   1) Graf wejściowy pozostaje niezmieniony
        //   2) Najpierw pomysleć jaki, prosty do sprawdzenia, warunek spełnia acykliczny graf nieskierowany
        //      Zakodowanie tego sprawdzenia nie powinno zająć więcej niż kilka linii!
        //      Zadanie jest bardzo łatwe (jeśli wydaje się trudne - poszukać prostszego sposobu, a nie walczyć z trudnym!)
        public bool Lab03IsUndirectedAcyclic(Graph g)
        {
            // n = m + c
            int n = g.VertexCount, m = g.EdgeCount, c = 0;
            var visited = new bool[n];
            for (int i = 0; i < n; i++)
            {
                if (visited[i])
                    continue;
                visited[i] = true;
                c++;
                foreach (var edge in g.BFS().SearchFrom(i))
                {
                    visited[edge.To] = true;
                }
            }
            return n == m + c;
        }
        
    }

}
