using ASD.Graphs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ASD
{
    public class Lab10 : MarshalByRefObject
    {
        /// <summary>
        /// Szukanie najdłuższego powtórzenia w zadanym kolorowaniu grafu.
        /// </summary>
        /// <param name="G">Graf prosty</param>
        /// <param name="color">Kolorowanie wierzchołków G (color[v] to kolor wierzchołka v)</param>
        /// <returns>Ścieżka, na której występuje powtórzenie (numery kolejnych wierzchołków)</returns>
        /// <remarks>W przypadku braku powtórzeń należy zwrócić null lub tablicę o długości 0</remarks>
        public int[] FindLongestRepetition(Graph H, int[] color)
        {
            int n = H.VertexCount;
            Graph G = new Graph(n, new ListGraphRepresentation());
            for (int i = 0; i < n; i++)
                for (int j = i + 1; j < n; j++)
                    if (H.HasEdge(i, j))
                        G.AddEdge(i, j);

            var doPrzejrzenia = new List<(int, int)>();
            var uzyte = new bool[n];

            for (int i = 0; i < n; i++)
                for (int j = i + 1; j < n; j++)
                    if (color[i] == color[j])
                        doPrzejrzenia.Add((i, j));

            var sciezka1 = new List<int>();
            var sciezka2 = new List<int>();
            var najlepsza = new List<int>();

            foreach (var tupla in doPrzejrzenia)
            {
                sciezka1.Add(tupla.Item1);
                uzyte[tupla.Item1] = true;
                sciezka2.Add(tupla.Item2);
                uzyte[tupla.Item2] = true;
                BadajSciezki(tupla.Item1, tupla.Item2);
                uzyte[tupla.Item2] = false;
                sciezka2.RemoveAt(sciezka2.Count - 1);
                uzyte[tupla.Item1] = false;
                sciezka1.RemoveAt(sciezka1.Count - 1);
            }

            return najlepsza.ToArray();

            void BadajSciezki(int u, int v)
            {
                foreach (int uNei in G.OutNeighbors(u))
                {
                    if (uNei == sciezka2[0] && sciezka1.Count > najlepsza.Count / 2)
                    {
                        najlepsza = new List<int>(sciezka1);
                        najlepsza.AddRange(sciezka2);
                    }
                    
                    foreach (int vNei in G.OutNeighbors(v))
                    {
                        if (vNei == sciezka1[0] && sciezka1.Count > najlepsza.Count / 2)
                        {
                            najlepsza = new List<int>(sciezka2);
                            najlepsza.AddRange(sciezka1);
                        }
                        if (uzyte[uNei] || uzyte[vNei])
                            continue;
                        if (uNei != vNei && color[uNei] == color[vNei])
                        {
                            sciezka1.Add(uNei);
                            uzyte[uNei] = true;
                            sciezka2.Add(vNei);
                            uzyte[vNei] = true;
                            BadajSciezki(uNei, vNei);
                            uzyte[vNei] = false;
                            sciezka2.RemoveAt(sciezka2.Count - 1);
                            uzyte[uNei] = false;
                            sciezka1.RemoveAt(sciezka1.Count - 1);
                        }
                    }
                }
            }
        }
    }

}


