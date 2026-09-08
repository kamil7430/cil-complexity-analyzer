using ASD.Graphs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASD
{
    public static class FlowExtender
    {
        private static Edge<double>[] MinPrzekroj(ref DiGraph<double> graf, ref DiGraph<double> przeplyw, int s, int t)
        {
            int n = graf.VertexCount;

            DiGraph<double> rezydualny = new(graf.VertexCount, graf.Representation);
            for (int i = 0; i < n; i++)
                foreach (var grafEdge in graf.OutEdges(i))
                {
                    var wagaPrzeplywOd = przeplyw.HasEdge(i, grafEdge.To) ? przeplyw.GetEdgeWeight(i, grafEdge.To) : 0;
                    var wagaPrzeplywDo = przeplyw.HasEdge(grafEdge.To ,i) ? przeplyw.GetEdgeWeight(grafEdge.To, i) : 0;
                    var waga = grafEdge.Weight - wagaPrzeplywOd + wagaPrzeplywDo;
                    if (waga > 0)
                        rezydualny.AddEdge(i, grafEdge.To, waga);
                }

            var visited = new bool[n];
            visited[s] = true;
            foreach (var edge in rezydualny.BFS().SearchFrom(s))
            {
                visited[edge.To] = true;
            }

            List<Edge<double>> edges = [];
            for (int i = 0; i < n; i++)
                if (visited[i])
                    foreach (var edge in graf.OutEdges(i))
                        if (!visited[edge.To])
                            edges.Add(edge);

            return edges.ToArray();
        }

        /// <summary>
        /// Metod wylicza minimalny s-t-przekrój.
        /// </summary>
        /// <param name="undirectedGraph">Nieskierowany graf</param>
        /// <param name="s">wierzchołek źródłowy</param>
        /// <param name="t">wierzchołek docelowy</param>
        /// <param name="minCut">minimalny przekrój</param>
        /// <returns>wartość przekroju</returns>
        public static double MinCut(this Graph<double> undirectedGraph, int s, int t, out Edge<double>[] minCut)
        {
            int n = undirectedGraph.VertexCount;
            DiGraph<double> graph = new(undirectedGraph.VertexCount, undirectedGraph.Representation);
            for (int i = 0; i < n; i++)
                foreach (var edge in undirectedGraph.OutEdges(i))
                    graph.AddEdge(i, edge.To, edge.Weight);

            (var flowValue, var f) = Flows.FordFulkerson(graph, s, t);
            minCut = MinPrzekroj(ref graph, ref f, s, t);
            return flowValue;
        }

        /// <summary>
        /// Metada liczy spójność krawędziową grafu oraz minimalny zbiór rozcinający.
        /// </summary>
        /// <param name="undirectedGraph">nieskierowany graf</param>
        /// <param name="cutingSet">zbiór krawędzi rozcinających</param>
        /// <returns>spójność krawędziowa</returns>
        public static int EdgeConnectivity(this Graph<double> undirectedGraph, out Edge<double>[] cutingSet)
        {
            int n = undirectedGraph.VertexCount;
            DiGraph<double> graph = new(undirectedGraph.VertexCount, undirectedGraph.Representation);
            for (int i = 0; i < n; i++)
                foreach (var edge in undirectedGraph.OutEdges(i))
                    graph.AddEdge(i, edge.To, edge.Weight);

            double minimumCutSetWeight = double.MaxValue;
            DiGraph<double>? minFlow = null;
            int minInd = -1;
            for (int i = 1; i < n; i++)
            {
                (var flowValue, var f) = Flows.FordFulkerson(graph, 0, i);
                if (flowValue < minimumCutSetWeight)
                {
                    minimumCutSetWeight = flowValue;
                    minFlow = f;
                    minInd = i;
                }
            }

            cutingSet = MinPrzekroj(ref graph, ref minFlow, 0, minInd);
            return (int)minimumCutSetWeight;
        }
        
    }
}
