using System;
using System.Text;

namespace Lab15
{
    public static class stringExtender
    {
        private static int[] ComputeP(string x)
        {
            int m = x.Length;
            var P = new int[m + 1];
            int t = 0;

            for (int j = 2; j <= m; j++)
            {
                while (t > 0 && x[t] != x[j - 1])
                    t = P[t];
                if (x[t] == x[j - 1])
                    t++;
                P[j] = t;
            }

            return P;
        }

        /// <summary>
        /// Metoda zwraca okres słowa s, tzn. najmniejszą dodatnią liczbę p taką, że s[i]=s[i+p] dla każdego i od 0 do |s|-p-1.
        /// 
        /// Metoda musi działać w czasie O(|s|)
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        static public int Period(this string s)
        {
            var P = ComputeP(s);
            return s.Length - P[s.Length];
        }

        /// <summary>
        /// Metoda wyznacza największą potęgę zawartą w słowie s.
        /// 
        /// Jeżeli x jest słowem, wówczas przez k-tą potęgę słowa x rozumiemy k-krotne powtórzenie słowa x
        /// (na przykład xyzxyzxyz to trzecia potęga słowa xyz).
        /// 
        /// Należy zwrócić największe k takie, że k-ta potęga jakiegoś słowa jest zawarta w s jako spójny podciąg.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="startIndex">Pierwszy indeks fragmentu zawierającego znalezioną potęgę</param>
        /// <param name="endIndex">Pierwszy indeks po fragmencie zawierającym znalezioną potęgę</param>
        /// <returns></returns>
        static public int MaxPower(this string s, out int startIndex, out int endIndex)
        {
            int n = s.Length;
            int maxPower = 0;
            startIndex = 0;
            endIndex = 1;

            if (n <= 2)
            {
                if (n == 2 && s[0] == s[1])
                {
                    startIndex = 0;
                    endIndex = 2;
                    return 2;
                }
                startIndex = 0;
                endIndex = 1;
                return 1;
            }

            for (int start = 0; start < n; start++)
            {
                var P = ComputeP(s.Substring(start));

                for (int end = 1; end < P.Length; end++)
                {
                    int p = end - P[end];
                    if (P[end] % p == 0 && P[end] / p > maxPower)
                    {
                        maxPower = P[end] / p;
                        startIndex = start;
                        endIndex = startIndex + P[end] + p;
                    }
                }
            }
            maxPower++;

            //Console.WriteLine(s);
            //Console.WriteLine($"{maxPower}, {startIndex}, {endIndex}");

            return maxPower;
        }
    }
}
