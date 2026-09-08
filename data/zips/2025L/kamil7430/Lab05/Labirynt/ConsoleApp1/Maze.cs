using ASD.Graphs;
using System;
using System.Linq;
using System.Text;

namespace ASD
{
    public class Maze : MarshalByRefObject
    {

        /// <summary>
        /// Wersje zadania I oraz II
        /// Zwraca najkrótszy możliwy czas przejścia przez labirynt bez dynamitów lub z dowolną ich liczbą
        /// </summary>
        /// <param name="maze">labirynt</param>
        /// <param name="withDynamite">informacja, czy dostępne są dynamity 
        /// Wersja I zadania -> withDynamites = false, Wersja II zadania -> withDynamites = true</param>
        /// <param name="path">zwracana ścieżka</param>
        /// <param name="t">czas zburzenia ściany (dotyczy tylko wersji II)</param> 
        public int FindShortestPath(char[,] maze, bool withDynamite, out string path, int t = 0)
        {
            int x = maze.GetLength(1), y = maze.GetLength(0);
            int s = -1, d = -1;
            DiGraph<int> graph = new(maze.Length);
            var GraphIndex = (int i, int j) => i * x + j;
            for (int i = 0; i < y; i++)
                for (int j = 0; j < x; j++)
                    if (maze[i, j] != 'X')
                    {
                        if (maze[i, j] == 'S')
                            s = GraphIndex(i, j);
                        if (maze[i, j] == 'E')
                            d = GraphIndex(i, j);
                        if (i > 0)
                        {
                            graph.AddEdge(GraphIndex(i - 1, j), GraphIndex(i, j), 1);
                        }
                        if (i < y - 1)
                        {
                            graph.AddEdge(GraphIndex(i + 1, j), GraphIndex(i, j), 1);
                        }
                        if (j > 0)
                        {
                            graph.AddEdge(GraphIndex(i, j - 1), GraphIndex(i, j), 1);
                        }
                        if (j < x - 1)
                        {
                            graph.AddEdge(GraphIndex(i, j + 1), GraphIndex(i, j), 1);
                        }
                    }
                    else if (withDynamite)
                    {
                        if (i > 0)
                            graph.AddEdge(GraphIndex(i - 1, j), GraphIndex(i, j), t);
                        if (i < y - 1)
                            graph.AddEdge(GraphIndex(i + 1, j), GraphIndex(i, j), t);
                        if (j > 0)
                            graph.AddEdge(GraphIndex(i, j - 1), GraphIndex(i, j), t);
                        if (j < x - 1)
                            graph.AddEdge(GraphIndex(i, j + 1), GraphIndex(i, j), t);
                    }
            
            var pathInfo = Paths.Dijkstra(graph, s);
            if (!pathInfo.Reachable(s, d))
            {
                path = "";
                return -1;
            }
            StringBuilder builder = new();
            var intPath = pathInfo.GetPath(s, d);
            for (int i = 1; i < intPath.Length; i++)
            {
                var roznica = intPath[i] - intPath[i - 1];
                if (roznica == 1)
                    builder.Append("E");
                if (roznica == -1)
                    builder.Append("W");
                if (roznica == x)
                    builder.Append("S");
                if (roznica == -x)
                    builder.Append("N");
            }

            path = builder.ToString();
            return pathInfo.GetDistance(s, d);
        }

        /// <summary>
        /// Wersja III i IV zadania
        /// Zwraca najkrótszy możliwy czas przejścia przez labirynt z użyciem co najwyżej k lasek dynamitu
        /// </summary>
        /// <param name="maze">labirynt</param>
        /// <param name="k">liczba dostępnych lasek dynamitu, dla wersji III k=1</param>
        /// <param name="path">zwracana ścieżka</param>
        /// <param name="t">czas zburzenia ściany</param>
        public int FindShortestPathWithKDynamites(char[,] maze, int k, out string path, int t)
        {
            int x = maze.GetLength(1), y = maze.GetLength(0);
            int s = -1, d = -1;
            int dynamites = k;
            k++;
            DiGraph<int> graph = new(maze.Length * k);
            var GraphIndex = (int i, int j, int h) => (i * x + j) + (h * maze.Length);
            for (int h = 0; h < k; h++)
                for (int i = 0; i < y; i++)
                    for (int j = 0; j < x; j++)
                        if (maze[i, j] != 'X')
                        {
                            if (maze[i, j] == 'S' && h == 0)
                                s = GraphIndex(i, j, h);
                            if (maze[i, j] == 'E' && h == 0)
                                d = GraphIndex(i, j, h);
                            if (i > 0)
                            {
                                graph.AddEdge(GraphIndex(i - 1, j, h), GraphIndex(i, j, h), 1);
                            }
                            if (i < y - 1)
                            {
                                graph.AddEdge(GraphIndex(i + 1, j, h), GraphIndex(i, j, h), 1);
                            }
                            if (j > 0)
                            {
                                graph.AddEdge(GraphIndex(i, j - 1, h), GraphIndex(i, j, h), 1);
                            }
                            if (j < x - 1)
                            {
                                graph.AddEdge(GraphIndex(i, j + 1, h), GraphIndex(i, j, h), 1);
                            }
                        }
                        else if (h > 0)
                        {
                            if (i > 0)
                                graph.AddEdge(GraphIndex(i - 1, j, h - 1), GraphIndex(i, j, h), t);
                            if (i < y - 1)
                                graph.AddEdge(GraphIndex(i + 1, j, h - 1), GraphIndex(i, j, h), t);
                            if (j > 0)
                                graph.AddEdge(GraphIndex(i, j - 1, h - 1), GraphIndex(i, j, h), t);
                            if (j < x - 1)
                                graph.AddEdge(GraphIndex(i, j + 1, h - 1), GraphIndex(i, j, h), t);
                        }

            var pathInfo = Paths.Dijkstra(graph, s);
            int minimum = int.MaxValue, indexMin = int.MaxValue;
            for (int h = 0; h < k; h++)
                if (pathInfo.Reachable(s, d + h * maze.Length) &&
                    pathInfo.GetDistance(s, d + h * maze.Length) < minimum)
                {
                    minimum = pathInfo.GetDistance(s, d + h * maze.Length);
                    indexMin = d + h * maze.Length;
                }
                    
            if(minimum == int.MaxValue)
            {
                path = "";
                return -1;
            }

            StringBuilder builder = new();
            var intPath = pathInfo.GetPath(s, indexMin);
            for (int i = 1; i < intPath.Length; i++)
            {
                var roznica = intPath[i] % maze.Length - intPath[i - 1] % maze.Length;
                if (roznica == 1)
                    builder.Append("E");
                if (roznica == -1)
                    builder.Append("W");
                if (roznica == x)
                    builder.Append("S");
                if (roznica == -x)
                    builder.Append("N");
            }

            path = builder.ToString();
            return minimum;
        }
    }
}