using System;
using System.Diagnostics;

namespace ASD
{
    class CrossoutChecker
    {
        /// <summary>
        /// Sprawdza, czy podana lista wzorców zawiera wzorzec x
        /// </summary>
        /// <param name="patterns">Lista wzorców</param>
        /// <param name="x">Jedyny znak szukanego wzorca</param>
        /// <returns></returns>
        bool comparePattern(char[][] patterns, char x)
        {
            foreach (char[] pat in patterns)
            {
                if (pat.Length == 1 && pat[0] == x)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Sprawdza, czy podana lista wzorców zawiera wzorzec xy
        /// </summary>
        /// <param name="patterns">Lista wzorców</param>
        /// <param name="x">Pierwszy znak szukanego wzorca</param>
        /// <param name="y">Drugi znak szukanego wzorca</param>
        /// <returns></returns>
        bool comparePattern(char[][] patterns, char x, char y)
        {
            foreach (char[] pat in patterns)
            {
                if (pat.GetLength(0) == 2 && pat[0] == x && pat[1] == y)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Metoda sprawdza, czy podany ciąg znaków można sprowadzić do ciągu pustego przez skreślanie zadanych wzorców.
        /// Zakładamy, że każdy wzorzec składa się z jednego lub dwóch znaków!
        /// </summary>
        /// <param name="sequence">Ciąg znaków</param>
        /// <param name="patterns">Lista wzorców</param>
        /// <param name="crossoutsNumber">Minimalna liczba skreśleń gwarantująca sukces lub int.MaxValue, jeżeli się nie da</param>
        /// <returns></returns>
        public bool Erasable(char[] sequence, char[][] patterns, out int crossoutsNumber)
        {
            return DajTabeleSkreslen(sequence, patterns, out crossoutsNumber, out var fragmenty, out var skreslenia);
        }

        /// <summary>
        /// Metoda sprawdza, jaka jest minimalna długość ciągu, który można uzyskać z podanego poprzez skreślanie zadanych wzorców.
        /// Zakładamy, że każdy wzorzec składa się z jednego lub dwóch znaków!
        /// </summary>
        /// <param name="sequence">Ciąg znaków</param>
        /// <param name="patterns">Lista wzorców</param>
        /// <returns></returns>
        public int MinimumRemainder(char[] sequence, char[][] patterns)
        {
            DajTabeleSkreslen(sequence, patterns, out var crossoutsNumber, out var fragmenty, out var skreslenia);
            int minimalna = sequence.Length, n = sequence.Length;
            Dictionary<int, int> doSkreslenia = [];
            for (int i = 0; i < n; i++)
                for (int j = i; j < n; j++)
                    if (fragmenty[i, j])
                        doSkreslenia[i] = j;
            foreach (var ind in doSkreslenia)
            {
                int nieskreslone = ind.Key, j = ind.Value + 1;
                while (j < n)
                {
                    if (doSkreslenia.ContainsKey(j))
                        j = doSkreslenia[j] + 1;
                    else
                    {
                        nieskreslone++;
                        j++;
                    }
                }
                if (nieskreslone < minimalna)
                    minimalna = nieskreslone;
            }
            return minimalna;
        }

        // można dopisać metody pomocnicze
        public bool DajTabeleSkreslen(char[] sequence, char[][] patterns, out int crossoutsNumber, out bool[,] fragmenty, out int?[,] skreslenia)
        {
            int n = sequence.Length;
            fragmenty = new bool[n,n];
            skreslenia = new int?[n,n];
            for (int i = 0; i < n; i++)
            {
                fragmenty[i, i] = comparePattern(patterns, sequence[i]);
                if (fragmenty[i, i])
                    skreslenia[i, i] = 1;
            }
            for (int i = 0; i < n - 1; i++)
            {
                fragmenty[i, i + 1] = comparePattern(patterns, sequence[i], sequence[i + 1]);
                if (fragmenty[i, i + 1])
                    skreslenia[i, i + 1] = 1;
            }
            for (int offset = 2; offset < n; offset++)
            {
                // długość rozważanego przedziału
                for (int i = 0; i < n - offset; i++)
                {
                    //fragment [i..i+offset]
                    for (int j = 0; j < offset; j++)
                    {
                        if (fragmenty[i, i + j] && fragmenty[i + j + 1, i + offset])
                        {
                            fragmenty[i, i + offset] = true;
                            int? c = skreslenia[i, i + j] + skreslenia[i + j + 1, i + offset];
                            if (c != null && c < (skreslenia[i, i + offset] == null ? int.MaxValue : skreslenia[i, i + offset]))
                                skreslenia[i, i + offset] = c;
                        }
                    }
                    if (fragmenty[i, i + offset - 2] && comparePattern(patterns, sequence[i + offset - 1], sequence[i + offset]))
                    {
                        fragmenty[i, i + offset] = true;
                        int? c = skreslenia[i, i + offset - 2] + 1;
                        if (c != null && c < (skreslenia[i, i + offset] == null ? int.MaxValue : skreslenia[i, i + offset]))
                            skreslenia[i, i + offset] = c;
                    } 
                    if(fragmenty[i, i + offset - 1] && comparePattern(patterns, sequence[i + offset]))
                    {
                        fragmenty[i, i + offset] = true;
                        int? c = skreslenia[i, i + offset - 1] + 1;
                        if (c != null && c < (skreslenia[i, i + offset] == null ? int.MaxValue : skreslenia[i, i + offset]))
                            skreslenia[i, i + offset] = c;
                    }
                    if (fragmenty[i + 1, i + offset - 1] && comparePattern(patterns, sequence[i], sequence[i + offset]))
                    {
                        fragmenty[i, i + offset] = true;
                        int? c = skreslenia[i + 1, i + offset - 1] + 1;
                        if (c != null && c < (skreslenia[i, i + offset] == null ? int.MaxValue : skreslenia[i, i + offset]))
                            skreslenia[i, i + offset] = c;
                    }
                    if (fragmenty[i + 1, i + offset] && comparePattern(patterns, sequence[i]))
                    {
                        fragmenty[i, i + offset] = true;
                        int? c = skreslenia[i + 1, i + offset] + 1;
                        if (c != null && c < (skreslenia[i, i + offset] == null ? int.MaxValue : skreslenia[i, i + offset]))
                            skreslenia[i, i + offset] = c;
                    }
                    if (fragmenty[i + 2, i + offset] && comparePattern(patterns, sequence[i], sequence[i + 1]))
                    {
                        fragmenty[i, i + offset] = true;
                        int? c = skreslenia[i + 2, i + offset] + 1;
                        if (c != null && c < (skreslenia[i, i + offset] == null ? int.MaxValue : skreslenia[i, i + offset]))
                            skreslenia[i, i + offset] = c;
                    }
                }
            }
            if (!fragmenty[0, n - 1])
                crossoutsNumber = int.MaxValue;
            else
                crossoutsNumber = skreslenia[0, n - 1].Value;
            return fragmenty[0, n - 1];
        }
    }
}
