using ASD;
using ASD.Graphs;
using System;
using System.Collections.Generic;

namespace Lab06
{
    public class HeroesSolver : MarshalByRefObject
    {
        /// <summary>
        /// Etap 1 - stwierdzenie, czy rozwiązanie istnieje
        /// </summary>
        /// <param name="g">graf przedstawiający mapę</param>
        /// <param name="keymasterTents">tablica krotek zawierająca pozycje namiotów klucznika - pierwsza liczba to kolor klucznika, druga to numer skrzyżowania</param>
        /// <param name="borderGates">tablica krotek zawierająca pozycje bram granicznych - pierwsza liczba to kolor bramy, dwie pozostałe to numery skrzyżowań na drodze między którymi znajduje się brama</param>
        /// <param name="p">ilość występujących kolorów (występujące kolory to 1,2,...,p)</param>
        /// <returns>bool - wartość true jeśli rozwiązanie istnieje i false wpp.</returns>
        public bool Lab06Stage1(Graph<int> g, (int color, int city)[] keymasterTents, (int color, int cityA, int cityB)[] borderGates, int p)
        {
            int n = g.VertexCount - 1; // wierzchołek 0 nie występuje w zadaniu

            var namioty = new List<int>?[n + 1];
            foreach (var keymasterTent in keymasterTents)
            {
                namioty[keymasterTent.city] ??= [];
                namioty[keymasterTent.city].Add(keymasterTent.color);
            }

            var bramy = new Dictionary<int, List<int>>?[n + 1];
            foreach (var borderGate in borderGates)
            {
                bramy[borderGate.cityA] ??= [];
                bramy[borderGate.cityB] ??= [];
                if (!bramy[borderGate.cityA].ContainsKey(borderGate.cityB))
                    bramy[borderGate.cityA][borderGate.cityB] = [borderGate.color];
                else 
                    bramy[borderGate.cityA][borderGate.cityB].Add(borderGate.color);
                if (!bramy[borderGate.cityB].ContainsKey(borderGate.cityA))
                    bramy[borderGate.cityB][borderGate.cityA] = [borderGate.color];
                else
                    bramy[borderGate.cityB][borderGate.cityA].Add(borderGate.color);
            }

            var klucze = new bool[p + 1];
            
            Queue<int> queue = new();
            var visited = new bool[n + 1];
            queue.Enqueue(1);
            visited[1] = true;
            
            if(namioty[1] != null)
                foreach (var color in namioty[1])
                    klucze[color] = true;

            while (queue.Count > 0)
            {
                var u = queue.Dequeue();
                if (u == n)
                    return true;
                foreach (var v in g.OutNeighbors(u))
                {
                    if (!visited[v])
                    {
                        bool skip = false;
                        if(bramy[u] != null && bramy[u].ContainsKey(v))
                            foreach (var color in bramy[u][v])
                                if (!klucze[color])
                                {
                                    skip = true;
                                    break;
                                }
                        if (skip)
                            continue;
                        bool reset = false;
                        if(namioty[v] != null)
                            foreach (var color in namioty[v])
                                if (!klucze[color])
                                {
                                    reset = true;
                                    klucze[color] = true;
                                }
                        queue.Enqueue(v);
                        visited[v] = true;
                        if (reset)
                        {
                            queue.Clear();
                            queue.Enqueue(v);
                            visited = new bool[n + 1];
                            visited[v] = true;
                        }
                    }
                }
            }
            
            return false;
        }

        /// <summary>
        /// Etap 2 - stwierdzenie, czy rozwiązanie istnieje
        /// </summary>
        /// <param name="g">graf przedstawiający mapę</param>
        /// <param name="keymasterTents">tablica krotek zawierająca pozycje namiotów klucznika - pierwsza liczba to kolor klucznika, druga to numer skrzyżowania</param>
        /// <param name="borderGates">tablica krotek zawierająca pozycje bram granicznych - pierwsza liczba to kolor bramy, dwie pozostałe to numery skrzyżowań na drodze między którymi znajduje się brama</param>
        /// <param name="p">ilość występujących kolorów (występujące kolory to 1,2,...,p)</param>
        /// <returns>krotka (bool solutionExists, int solutionLength) - solutionExists ma wartość true jeśli rozwiązanie istnieje i false wpp. SolutionLenth zawiera długość optymalnej trasy ze skrzyżowania 1 do n</returns>
        public (bool solutionExists, int solutionLength) Lab06Stage2(Graph<int> g, (int color, int city)[] keymasterTents, (int color, int cityA, int cityB)[] borderGates, int p)
        {
            int n = g.VertexCount - 1; // wierzchołek 0 nie występuje w zadaniu

            if(!Lab06Stage1(g, keymasterTents, borderGates, p))
                return (false, 0);

            var namioty = new List<int>?[n + 1];
            foreach (var keymasterTent in keymasterTents)
            {
                namioty[keymasterTent.city] ??= [];
                namioty[keymasterTent.city].Add(keymasterTent.color);
            }

            var bramy = new Dictionary<int, List<int>>?[n + 1];
            foreach (var borderGate in borderGates)
            {
                bramy[borderGate.cityA] ??= [];
                bramy[borderGate.cityB] ??= [];
                if (!bramy[borderGate.cityA].ContainsKey(borderGate.cityB))
                    bramy[borderGate.cityA][borderGate.cityB] = [borderGate.color];
                else 
                    bramy[borderGate.cityA][borderGate.cityB].Add(borderGate.color);
                if (!bramy[borderGate.cityB].ContainsKey(borderGate.cityA))
                    bramy[borderGate.cityB][borderGate.cityA] = [borderGate.color];
                else
                    bramy[borderGate.cityB][borderGate.cityA].Add(borderGate.color);
            }
            
            
            
            return (true, 1);
        }
    }
}
