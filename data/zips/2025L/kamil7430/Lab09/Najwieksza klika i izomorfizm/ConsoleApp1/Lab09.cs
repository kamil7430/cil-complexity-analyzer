
using System.Collections.Generic;
using System.Linq;
using ASD.Graphs;

/// <summary>
/// Klasa rozszerzająca klasę Graph o rozwiązania problemów największej kliki i izomorfizmu grafów metodą pełnego przeglądu (backtracking)
/// </summary>
public static class Lab10GraphExtender
{
    /// <summary>
    /// Wyznacza największą klikę w grafie i jej rozmiar metodą pełnego przeglądu (backtracking)
    /// </summary>
    /// <param name="g">Badany graf</param>
    /// <param name="clique">Wierzchołki znalezionej największej kliki - parametr wyjściowy</param>
    /// <returns>Rozmiar największej kliki</returns>
    /// <remarks>
    /// Nie wolno modyfikować badanego grafu.
    /// </remarks>
    public static int MaxClique(this Graph g, out int[] clique)
    {
        int n = g.VertexCount;
        HashSet<int> S = [];
        HashSet<int> bestS = [];

        MaxCliqueRec(0);

        void MaxCliqueRec(int k)
        {
            HashSet<int> C = [];
            for (int i = k; i < n; i++)
            {
                bool toAdd = true;
                foreach (var v in S)
                    if (!g.HasEdge(i, v))
                    {
                        toAdd = false;
                        break;
                    }
                if (toAdd)
                    C.Add(i);
            }

            if (C.Count + S.Count <= bestS.Count)
                return;

            if (S.Count > bestS.Count)
                bestS = new(S);

            foreach (var m in C)
            {
                S.Add(m);
                MaxCliqueRec(m + 1);
                S.Remove(m);
            }
        }

        clique = bestS.ToArray();
        return bestS.Count;
    }

    /// <summary>
    /// Bada izomorfizm grafów metodą pełnego przeglądu (backtracking)
    /// </summary>
    /// <param name="g">Pierwszy badany graf</param>
    /// <param name="h">Drugi badany graf</param>
    /// <param name="map">Mapowanie wierzchołków grafu h na wierzchołki grafu g (jeśli grafy nie są izomorficzne to null) - parametr wyjściowy</param>
    /// <returns>Informacja, czy grafy g i h są izomorficzne</returns>
    /// <remarks>
    /// 1) Uwzględniamy wagi krawędzi
    /// 3) Nie wolno modyfikować badanych grafów.
    /// </remarks>
    public static bool IsomorphismTest(this Graph<int> g, Graph<int> h, out int[] map)
    {
        map = null;
        int n = g.VertexCount;
        if (g.VertexCount != h.VertexCount || g.EdgeCount != h.EdgeCount)
            return false;

        bool finished = false;
        var used = new bool[n];
        var perm = new int[n];
        GeneratePermutations(0);

        bool SprawdzNowyWierzcholek(int v)
        {
            for (int i = 0; i < v; i++)
                if (g.HasEdge(i, v))
                {
                    if (!h.HasEdge(perm[i], perm[v]))
                        return false;
                    if (g.GetEdgeWeight(i, v) != h.GetEdgeWeight(perm[i], perm[v]))
                        return false;
                }
                else
                {
                    if (h.HasEdge(perm[i], perm[v]))
                        return false;
                }

            return true;
        }

        void GeneratePermutations(int k)
        {
            if (finished)
                return;
            if (!SprawdzNowyWierzcholek(k - 1))
                return;
            if (k == n)
            {
                finished = true;
                return;
            }
            for (int m = 0; m < n; m++)
            {
                if (used[m])
                    continue;
                used[m] = true;
                perm[k] = m;
                GeneratePermutations(k + 1);
                if (finished)
                    return;
                used[m] = false;
            }
        }

        if (finished)
        {
            map = new int[n];
            for (int i = 0; i < n; i++)
            {
                map[perm[i]] = i;
            }
        }
        return finished;
    }

}

