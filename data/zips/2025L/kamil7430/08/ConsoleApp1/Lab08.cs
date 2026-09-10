using ASD.Graphs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace ASD
{
    public class Lab08 : MarshalByRefObject
    {
        /// <summary>Etap I</summary>
        /// <param name="P">Tablica która dla każdego pola zawiera informacje, ile maszyn moze lacznie wyjechac z tego pola</param>
        /// <param name="MachinePos">Tablica zawierajaca informacje o poczatkowym polozeniu maszyn</param>
        /// <returns>Pierwszy element kroki to liczba uratowanych maszyn, drugi to tablica indeksów tych maszyn</returns>
        public (int savedNum, int[] Saved) Stage1(int[,] P, (int row, int col)[] MachinePos)
        {
			int h = P.GetLength(0);
            int w = P.GetLength(1);

            int N = h * w * 2 + 2;
            int n = h * w;
            int s = N - 2, t = N - 1;
            // in: <0, hw-1>, out: <hw, 2hw-1>
            DiGraph<int> g = new DiGraph<int>(N);

            Func<int, int, int> V = (int i, int j) => i * w + j;
            int INF = int.MaxValue / 2;

            for (int i = 0; i < h; i++)
            for (int j = 0; j < w; j++)
            {
                if (i < h - 1 && i != 0)
                    g.AddEdge(V(i, j) + n, V(i + 1, j), INF);
                if (i > 0)
                    g.AddEdge(V(i, j) + n, V(i - 1, j), INF);
                if (j < w - 1 && i != 0)
                    g.AddEdge(V(i, j) + n, V(i, j + 1), INF);
                if (j > 0 && i != 0)
                    g.AddEdge(V(i, j) + n, V(i, j - 1), INF);
                if (i == 0)
                    g.AddEdge(V(i, j) + n, t, INF);
                if (P[i, j] > 0)
                    g.AddEdge(V(i, j), V(i, j) + n, P[i, j]);
            }

            foreach (var machine in MachinePos)
                g.AddEdge(s, V(machine.row, machine.col), 1);

            var (flow, f) = Flows.FordFulkerson(g, s, t);

            List<int> saved = new List<int>();
            for (int i = 0; i < MachinePos.Length; i++)
            {
                var machine = MachinePos[i];
                if (f.HasEdge(s, machine.row * w + machine.col)
                    && f.GetEdgeWeight(s, machine.row * w + machine.col) > 0)
                    saved.Add(i);
            }

            return (flow, saved.ToArray());
        }

        /// <summary>Etap II</summary>
        /// <param name="P">Tablica która dla każdego pola zawiera informacje, ile maszyn moze lacznie wyjechac z tego pola</param>
        /// <param name="MachinePos">Tablica zawierajaca informacje o poczatkowym polozeniu maszyn</param>
        /// <param name="MachineValue">Tablica zawierajaca informacje o wartosci maszyn</param>
        /// <param name="moveCost">Koszt jednego ruchu</param>
        /// <returns>Pierwszy element kroki to najwiekszy mozliwy zysk, drugi to tablica indeksow maszyn, ktorych wyprowadzenie maksymalizuje zysk</returns>
        public (int bestProfit, int[] Saved) Stage2(int[,] P, (int row, int col)[] MachinePos, int[] MachineValue, int moveCost)
        {
			int h = P.GetLength(0);
            int w = P.GetLength(1);

            int N = h * w * 2 + 2;
            int n = h * w;
            int s = N - 2, t = N - 1;
            // in: <0, hw-1>, out: <hw, 2hw-1>
            NetworkWithCosts<int, int> g = new NetworkWithCosts<int, int>(N);

            int MAXCAP = MachinePos.Length;

            for (int i = 0; i < h; i++) 
            for (int j = 0; j < w; j++)
            {
                if (P[i, j] == 0)
                    continue;
                int pos = i * w + j;
                if (i > 0)
                {
                    g.AddEdge(pos + n, pos - w, MAXCAP, 0);
                    if (i < h - 1)
                        g.AddEdge(pos + n, pos + w, MAXCAP, 0);
                    if (j > 0)
                        g.AddEdge(pos + n, pos - 1, MAXCAP, 0);
                    if (j < w - 1)
                        g.AddEdge(pos + n, pos + 1, MAXCAP, 0);
                }
                else
                    g.AddEdge(pos + n, t, MAXCAP, 0);
                g.AddEdge(pos, pos + n, P[i, j], moveCost);
            }

            for (int i = 0; i < MachinePos.Length; i++)
            {
                var (row, col) = MachinePos[i];
                if (row * moveCost >= MachineValue[i] || P[row, col] <= 0)
                    continue;
                g.AddEdge(s, w * row + col, 1, -MachineValue[i]);
                g.AddEdge(w * row + col, t, 1, MachineValue[i]);
            }

            var (_, cost, flow) = Flows.MinCostMaxFlow(g, s, t);

            var uratowane = new LinkedList<int>();
            for (int i = 0; i < MachinePos.Length; i++)
            {
                var (row, col) = MachinePos[i];
                if (flow.HasEdge(s, row * w + col) && !flow.HasEdge(row * w + col, t))
                    uratowane.AddLast(i);
            }

            return (-cost, uratowane.ToArray());
        }
    }
}