using System;
using System.Collections.Generic;
using System.Text;

namespace ASD
{
    public class LZ77 : MarshalByRefObject
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
        /// Odkodowywanie napisu zakodowanego algorytmem LZ77. Dane kodowanie jest poprawne (nie trzeba tego sprawdzać).
        /// </summary>
        public string Decode(List<EncodingTriple> encoding)
        {
            if (encoding.Count <= 0)
                return "";

            int n = encoding.Count;
            foreach (var enc in encoding)
                n += enc.c;

            StringBuilder builder = new(n);

            builder.Append(encoding[0].s);
            foreach (var enc in encoding.Skip(1))
            {
                int start = builder.Length - enc.p - 1; // -1??
                for (int i = 0; i < enc.c; i++)
                    builder.Append(builder[start + i]);

                builder.Append(enc.s);
            }

            return builder.ToString();
        }

        /// <summary>
        /// Kodowanie napisu s algorytmem LZ77
        /// </summary>
        /// <returns></returns>
        public List<EncodingTriple> Encode(string s, int maxP)
        {
            var enc = new List<EncodingTriple>();

            int j = 0;
            while (j < s.Length)
            {
                int w_start = Math.Max(j - maxP, 0); // w \in <j-p_max-1, j)

                int maxPrefixLength = 0;
                int best_needle_start = 0;
                int best_w_start = 0;

                for (int w_start_pointer = w_start; w_start_pointer < j; w_start_pointer++)
                {
                    for (int needle_start_pointer = w_start_pointer + 1;
                        needle_start_pointer < s.Length; needle_start_pointer++)
                    {
                        int i;
                        for (i = 0; i < s.Length - needle_start_pointer; i++) // w?
                        {
                            if (s[w_start_pointer + i] != s[needle_start_pointer + i])
                            {
                                if (i > maxPrefixLength)
                                {
                                    maxPrefixLength = i;
                                    best_w_start = w_start_pointer;
                                    best_needle_start = needle_start_pointer;
                                }
                                break;
                            }
                        }
                        if (--i > maxPrefixLength)
                        {
                            maxPrefixLength = i;
                            best_w_start = w_start_pointer;
                            best_needle_start = needle_start_pointer;
                        }
                    }
                }

                enc.Add(new EncodingTriple
                {
                    p = maxPrefixLength <= 0 ? 0 : j - best_w_start,
                    c = maxPrefixLength,
                    s = s[best_needle_start + maxPrefixLength]
                });

                j += maxPrefixLength + 1; //TODO
            }

            return enc;
        }
    }

    [Serializable]
    public struct EncodingTriple
    {
        public int p, c;
        public char s;

        public EncodingTriple(int p, int c, char s)
        {
            this.p = p;
            this.c = c;
            this.s = s;
        }
    }
}

//for (int length = 1; length < Math.Min(maxP + 1, s.Length - j); length++)
//{
//    for (int w_pointer = w_start; w_pointer < j; w_pointer++)
//    {
//        bool isBad = false;
//        for (int i = 0; i < length; i++)
//        {
//            if (s[w_start + i] != s[j + i])
//            {
//                isBad = true;
//                break;
//            }
//        }
//        if(!isBad)
//    }
//}