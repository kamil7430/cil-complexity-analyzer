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
            path = "";

            return -1;
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
            path = "";
            return -1;
        }
    }
}