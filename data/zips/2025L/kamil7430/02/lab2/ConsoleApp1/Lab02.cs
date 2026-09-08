using System;
using System.Collections.Generic;
using System.Text;

namespace Lab02
{
    public class PatternMatching : MarshalByRefObject
    {
        /// <summary>
        /// Etap 1 - wyznaczenie trasy, zgodnie z którą robot przemieści się z pozycji poczatkowej (0,0) na pozycję docelową (-n-1, m-1)
        /// </summary>
        /// <param name="n">wysokość prostokąta</param>
        /// <param name="m">szerokość prostokąta</param>
        /// <param name="obstacles">tablica ze współrzędnymi przeszkód</param>
        /// <returns>krotka (bool result, string path) - result ma wartość true jeżeli trasa istnieje, false wpp., path to wynikowa trasa</returns>
        public (bool result, string path) Lab02Stage1(int n, int m, (int, int)[] obstacles)
        {
            var komorki = new string?[n, m];
            int j = 1;
            komorki[0, 0] = "";
            while (j < m)
            {
                if (obstacles.Contains((0, j)))
                {
                    for (int k = j; k < m; k++)
                        komorki[0, k] = null;
                    break;
                }
                komorki[0, j] = komorki[0, j - 1] + "R";
                j++;
            }
            int i = 1;
            while (i < n)
            {
                if (obstacles.Contains((i, 0)))
                {
                    for (int k = i; k < n; k++)
                        komorki[k, 0] = null;
                    break;
                }
                komorki[i, 0] = komorki[i - 1, 0] + "D";
                i++;
            }

            for (i = 1; i < n; i++)
            {
                for (j = 1; j < m; j++)
                {
                    if (obstacles.Contains((i, j)))
                        komorki[i, j] = null;
                    else if (komorki[i - 1, j] != null)
                        komorki[i, j] = komorki[i - 1, j] + "D";
                    else if (komorki[i, j - 1] != null)
                        komorki[i, j] = komorki[i, j - 1] + "R";
                    else
                        komorki[i, j] = null;
                }
            }
            if (komorki[n - 1, m - 1] == null)
                return (false, "");
            return (true, (string)komorki[n - 1, m - 1]!);
        }

        /// <summary>
        /// Etap 2 - wyznaczenie trasy realizującej zadany wzorzec, zgodnie z którą robot przemieści się z pozycji poczatkowej (0,0) na pozycję docelową (-n-1, m-1)
        /// </summary>
        /// <param name="n">wysokość prostokąta</param>
        /// <param name="m">szerokość prostokąta</param>
        /// <param name="pattern">zadany wzorzec</param>
        /// <param name="obstacles">tablica ze współrzędnymi przeszkód</param>
        /// <returns>krotka (bool result, string path) - result ma wartość true jeżeli trasa istnieje, false wpp., path to wynikowa trasa</returns>
        public (bool result, string path) Lab02Stage2(int n, int m, string pattern, (int, int)[] obstacles)
        {
            int p = pattern.Length;
            var sciezki = new string?[n, m, p+1];
            sciezki[0, 0, 0] = "";
            int k = 1;
            while (k <= p && (pattern[k - 1] == '*' || pattern[k - 1] == '?'))
            {
                sciezki[0, 0, k] = "";
                k++;
            }
            while (k <= p)
            {
                sciezki[0, 0, k] = null;
                k++;
            }
            for (int j = 1; j < m; j++)
            {
                if (obstacles.Contains((0, j)))
                    for (k = 0; k <= p; k++)
                        sciezki[0, j, k] = null;
                else
                {
                    if (sciezki[0, j - 1, 0] != null)
                        sciezki[0, j, 0] = sciezki[0, j - 1, 0] + "R";
                    for (k = 1; k <= p; k++)
                    {
                        if (sciezki[0, j - 1, k - 1] != null &&
                            (pattern[k - 1] == 'R' || pattern[k - 1] == '*' || pattern[k - 1] == '?'))
                            sciezki[0, j, k] = sciezki[0, j - 1, k - 1] + "R";
                    }
                }
            }
            for (int i = 1; i < n; i++)
            {
                if (obstacles.Contains((i, 0)))
                    for (k = 0; k <= p; k++)
                        sciezki[i, 0, k] = null;
                else
                {
                    if (sciezki[i - 1, 0, 0] != null)
                        sciezki[i, 0, 0] = sciezki[i - 1, 0, 0] + "D";
                    for (k = 1; k <= p; k++)
                    {
                        if (sciezki[i - 1, 0, k - 1] != null &&
                            (pattern[k - 1] == 'D' || pattern[k - 1] == '*' || pattern[k - 1] == '?'))
                            sciezki[i, 0, k] = sciezki[i - 1, 0, k - 1] + "D";
                    }
                }
            }

            for (int i = 1; i < n; i++)
            {
                for (int j = 1; j < m; j++)
                {
                    if (obstacles.Contains((i, j)))
                        for (k = 0; k <= p; k++)
                            sciezki[i, j, k] = null;
                    else
                    {
                        if (sciezki[i - 1, j, 0] != null)
                            sciezki[i, j, 0] = sciezki[i - 1, j, 0] + "D";
                        else if (sciezki[i, j - 1, 0] != null)
                            sciezki[i, j, 0] = sciezki[i, j - 1, 0] + "R";
                        for (k = 1; k <= p; k++)
                        {
                            if (sciezki[i - 1, j, k - 1] != null &&
                                (pattern[k - 1] == 'D' || pattern[k - 1] == '*' || pattern[k - 1] == '?'))
                                sciezki[i, j, k] = sciezki[i - 1, j, k - 1] + "D";
                            else if (sciezki[i, j - 1, k - 1] != null && 
                                     (pattern[k - 1] == 'R' || pattern[k - 1] == '*' || pattern[k - 1] == '?'))
                                sciezki[i, j, k] = sciezki[i, j - 1, k - 1] + "R";
                        }
                    }
                }
            }

            if (sciezki[n - 1, m - 1, p - 1] is null)
                return (false, "");
            return (true, (string)sciezki[n - 1, m - 1, p - 1]!);
        }
    }
}