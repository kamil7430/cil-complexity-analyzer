using ASD.Graphs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace ASD
{
	public class Lab06 : MarshalByRefObject
	{
		/// <summary>Etap 1</summary>
		/// <param name="k">Liczba kolorów (równa liczbie wierzchołków w c)</param>
		/// <param name="c">Graf opisujący możliwe przejścia między kolorami. Waga to wysiłek.</param>
		/// <param name="g">Graf opisujący drogi w mieście. Waga to kolor drogi.</param>
		/// <param name="target">Wierzchołek docelowy (dom Grzesia).</param>
		/// <param name="start">Wierzchołek startowy (wejście z lasu).</param>
		/// <returns>Pierwszy element pary to informacja, czy rozwiązanie istnieje. Drugi element pary, to droga będąca rozwiązaniem: sekwencja odwiedzanych wierzchołków (pierwszy musi być start, ostatni target). W przypadku, gdy nie ma rozwiązania, ma być tablica o długości 0.</returns>
		public (bool possible, int[] path) Stage1(
			int k, DiGraph<int> c, Graph<int> g, int target, int start
		)
		{
			var n = g.VertexCount;
			var visited = new bool[n, k];
			var prev = new (int vertex, int color)?[n, k];
			Queue<(int vertex, int inColor)?> queue = [];
			var canGo = new bool[k, k];

			for (int i = 0; i < k; i++)
				foreach (var neigh in c.OutNeighbors(i))
					canGo[i, neigh] = true;

            foreach (var edge in g.OutEdges(start))
			{
				queue.Enqueue((edge.To, edge.Weight));
                prev[edge.To, edge.Weight] = (start, -1);
            }

			while(queue.Count > 0)
			{
				(int u, int kolor) = queue.Dequeue().Value;
				visited[u, kolor] = true;
				foreach (var edge in g.OutEdges(u))
					if (!visited[edge.To, edge.Weight])
						if (kolor == edge.Weight || canGo[kolor, edge.Weight])
						{
							queue.Enqueue((edge.To, edge.Weight));
							prev[edge.To, edge.Weight] = (u, kolor);
						}
            }

			bool trafil = false;
			for (int i = 0; i < k; i++)
				if (visited[target, i])
				{
					trafil = true;
					break;
				}

			if(!trafil)
				return (false, []);

			var path = new List<int>();
			int startPrevColor = -1;
			for (int i = 0; i < k; i++)
				if (prev[target, i] != null)
					startPrevColor = i;
			int vert = target;
			while (vert != start)
			{
				path.Add(vert);
				(vert, startPrevColor) = prev[vert, startPrevColor].Value;
			}
			path.Add(start);
			path.Reverse();
			return (true, path.ToArray());
        }

		/// <summary>Drugi etap</summary>
		/// <param name="k">Liczba kolorów (równa liczbie wierzchołków w c)</param>
		/// <param name="c">Graf opisujący możliwe przejścia między kolorami. Waga to wysiłek.</param>
		/// <param name="g">Graf opisujący drogi w mieście. Waga to kolor drogi.</param>
		/// <param name="target">Wierzchołek docelowy (dom Grzesia).</param>
		/// <param name="starts">Wierzchołki startowe (wejścia z lasu).</param>
		/// <returns>Pierwszy element pary to koszt najlepszego rozwiązania lub null, gdy rozwiązanie nie istnieje. Drugi element pary, tak jak w etapie 1, to droga będąca rozwiązaniem: sekwencja odwiedzanych wierzchołków (pierwszy musi być start, ostatni target). W przypadku, gdy nie ma rozwiązania, ma być tablica o długości 0.</returns>
		public (int? cost, int[] path) Stage2(
			int k, DiGraph<int> c, Graph<int> g, int target, int[] starts
		)
		{
			int n = g.VertexCount;
			foreach (int start in starts)
				if (!Stage1(k, c, g, target, start).possible)
					return (null, []);

			SafePriorityQueue<int, (int vertex, int prevColor)> queue = new();
			var odleglosc = new int[n, k];
            var prev = new (int vertex, int color)?[n, k];

			for (int i = 0; i < n; i++)
				for (int j = 0; j < k; j++)
				{
					odleglosc[i, j] = int.MaxValue / 2;
					queue.Insert((i, j), odleglosc[i, j]);
				}

            var canGo = new int?[k, k];
			for (int i = 0; i < k; i++)
			{
				canGo[i, i] = 0;
				foreach (var edge in c.OutEdges(i))
					canGo[edge.To, i] = edge.Weight;
			}

            foreach (var edge in g.OutEdges(target))
            {
				odleglosc[edge.To, edge.Weight] = 1;
				queue.UpdatePriority((edge.To, edge.Weight), 1);
                prev[edge.To, edge.Weight] = (target, -1);
            }

            while (queue.Count > 0)
			{
				(int u, int prevColor) = queue.Extract();
				foreach (var edge in g.OutEdges(u))
				{
					if (canGo[prevColor, edge.Weight] == null)
						continue;
					int nowyKoszt = odleglosc[u, prevColor] + 1 + canGo[prevColor, edge.Weight].Value;
					if (odleglosc[edge.To, edge.Weight] > nowyKoszt)
					{
						odleglosc[edge.To, edge.Weight] = nowyKoszt;
						if(queue.Contains((edge.To, edge.Weight)))
							queue.UpdatePriority((edge.To, edge.Weight), nowyKoszt);
                        prev[edge.To, edge.Weight] = (u, prevColor);
                    }
				}
            }

			int minimum = int.MaxValue, minIndex = -1;
			foreach (int start in starts)
				for (int i = 0; i < k; i++) 
					if (odleglosc[start,i]<minimum)
					{
						minimum = odleglosc[start, i];
						minIndex = start;
					}

            var path = new List<int>();
			int startPrevColor = k - 1;
			for (int i = 0; i < k; i++)
				if (odleglosc[minIndex, i] < odleglosc[minIndex, startPrevColor])
					startPrevColor = i;
			int vert = minIndex;
            while (vert != target)
            {
                path.Add(vert);
                (vert, startPrevColor) = prev[vert, startPrevColor].Value;
            }
            path.Add(target);

            return (minimum, path.ToArray());
		}
	}
}
