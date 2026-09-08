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
        public int[] FindLongestRepetition(Graph G, int[] color)
        {
            int n = G.VertexCount;
            List<int> S = [];
            List<int> bestS = [];
            var visited = new bool[n];
            
            int maxCol = -1;
            foreach (int c in color)
                if (maxCol < c)
                    maxCol = c;
            var maxColors = new int[maxCol + 1];
            foreach (var c in color)
                maxColors[c]++;
            var colorsVisited = new int[maxCol + 1];

            for (int i = 0; i < n; i++)
            {
                S.Add(i);
                visited[i] = true;
                colorsVisited[color[i]] = 1;
                Paths(i);
                colorsVisited[color[i]] = 0;
                visited[i] = false;
                S.RemoveAt(S.Count - 1);
            }
    
            return bestS.ToArray();

            bool CheckPath()
            {
                int k = S.Count / 2;
                for (int i = 0; i < k; i++)
                {
                    if (color[S[i]] != color[S[i + k]])
                        return false;
                }
                return true;
            }

            bool ToAbandon(int v)
            {
                if (S.Count > 1 && maxColors[color[v]] <= 1)
                    return true;
                
                return false;
            }

            void Paths(int v)
            {
                if (ToAbandon(v))
                    return;
                if (S.Count > bestS.Count && S.Count % 2 == 0)
                {
                    if (CheckPath())
                    {
                        bestS = new(S);
                    }
                }
                foreach (int neigh in G.OutNeighbors(v))
                {
                    if (!visited[neigh])
                    {
                        S.Add(neigh);
                        visited[neigh] = true;
                        colorsVisited[color[neigh]]++;
                        Paths(neigh);
                        colorsVisited[color[neigh]]--;
                        visited[neigh] = false;
                        S.RemoveAt(S.Count - 1);
                    }
                }
            }
        }
    }

}


