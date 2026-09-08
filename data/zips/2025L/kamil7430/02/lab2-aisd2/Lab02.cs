using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ASD
{
    public class Lab02 : MarshalByRefObject
    {
        /// <summary>
        /// Etap 1 - Wyznaczenie ścieżki (seam) o minimalnym sumarycznym score.
        /// Ścieżka przebiega od górnego do dolnego wiersza obrazu.
        /// </summary>
        /// <param name="S">macierz score o wymiarach H x W, gdzie S[i, j] reprezentuje "ważność" piksela w wierszu i i kolumnie j</param>
        /// <returns>
        /// (int cost, (int, int)[] seam) - 
        /// cost: minimalny łączny koszt ścieżki (suma wartości pikseli);
        /// seam: tablica pozycji pikseli (włącznie z pikselem z pierwszego i ostatniego wiersza) tworzących ścieżkę.
        /// </returns>
        public (int cost, (int i, int j)[] seam) Stage1(int[,] S)
        {
            int H = S.GetLength(0);
            int W = S.GetLength(1);

            int[,] cost = new int[H, W];

            for (int i = 0; i < W; i++)
            {
                cost[0, i] = S[0, i];
            }
            for (int i = 1; i < H; i++)
            {
                for (int j = 0; j < W; j++)
                {
                    int newCost = int.MaxValue;
                    if(j>0)
                    {
                        if (cost[i - 1, j - 1] + S[i, j] < newCost)
                            newCost = cost[i - 1, j - 1] + S[i, j];
                    }
                    if (cost[i - 1, j] + S[i, j] < newCost)
                        newCost = cost[i - 1, j] + S[i, j];
                    if(j<W-1)
                    {
                        if (cost[i - 1, j + 1] + S[i, j] < newCost)
                            newCost = cost[i - 1, j + 1] + S[i, j];
                    }
                    cost[i,j] = newCost;
                }
            }
            int minimum = cost[H - 1, 0], indeks = 0;
            for(int  i = 0; i < W;i++)
            {
                if (minimum > cost[H - 1, i])
                {
                    minimum = cost[H - 1, i];
                    indeks = i;
                }
            }

            //od konca
            (int, int)[] sciezka = new (int, int)[H];
            sciezka[H - 1] = (H - 1, indeks);
            for (int i = H - 2; i >= 0; i--)
            {
                int j = sciezka[i + 1].Item2;
                int nowyIndeks = j;
                int newCost = cost[i, j];
                if (j > 0)
                {
                    if (cost[i, j - 1] < newCost)
                    {
                        newCost = cost[i, j - 1];
                        nowyIndeks = j - 1;
                    }
                }
                if (j < W - 1)
                {
                    if (cost[i, j + 1] < newCost)
                    { 
                        newCost = cost[i, j + 1];
                        nowyIndeks = j + 1;
                    }
                }
                sciezka[i] = (i, nowyIndeks);
            }

            return (minimum, sciezka);
        }

        /// <summary>
        /// Etap 2 - Wyznaczenie ścieżki (seam) o minimalnym sumarycznym score z uwzględnieniem kary za zmianę kierunku.
        /// Przy każdym przejściu, gdy kierunek ruchu różni się od poprzedniego, do łącznego kosztu dodawana jest kara K.
        /// Pierwszy krok (z pierwszego wiersza) nie podlega karze.
        /// </summary>
        /// <param name="S">macierz score o wymiarach H x W</param>
        /// <param name="K">kara za zmianę kierunku (K >= 1)</param>
        /// <returns>
        /// (int cost, (int, int)[] seam) - 
        /// cost: minimalny łączny koszt ścieżki (suma wartości pikseli oraz naliczonych kar);
        /// seam: tablica pozycji pikseli tworzących ścieżkę.
        /// </returns>
        public (int cost, (int i, int j)[] seam) Stage2(int[,] S, int K)
        {
            int H = S.GetLength(0);
            int W = S.GetLength(1);

            (int L, int G, int P)[,] cost = new (int, int, int)[H, W];
            // tupla zawierajaca koszt dojscia od lewej-gory, z gory i z prawej-gory
            
            for (int i = 0; i < W; i++)
            { // pierwszy wiersz
                cost[0, i] = (S[0, i], S[0, i], S[0, i]);
            }
            
            // drugi wiersz
            cost[1, 0] = (int.MaxValue - K, S[0, 0] + S[1, 0], S[0, 1] + S[1, 0]);
            for (int i = 1; i < W - 1; i++)
            { 
                cost[1, i] = (S[0, i - 1] + S[1, i], S[0, i] + S[1, i], S[0, i + 1] + S[1, i]);
            }
            cost[1, W - 1] = (S[0, W - 2] + S[1, W - 1], S[0, W - 1] + S[1, W - 1], int.MaxValue - K);

            // kolejne wiersze
            for (int i = 2; i < H; i++)
            {
                for (int j = 0; j < W; j++)
                {
                    // lewa-góra
                    if (j == 0)
                        cost[i, j].L = int.MaxValue - K;
                    else
                    {
                        int newMin = cost[i - 1, j - 1].L;
                        if (cost[i - 1, j - 1].G + K < newMin)
                            newMin = cost[i - 1, j - 1].G + K;
                        if (cost[i - 1, j - 1].P + K < newMin)
                            newMin = cost[i - 1, j - 1].P + K;
                        cost[i, j].L = newMin + S[i, j];
                    }
                    // góra
                    {
                        int newMin = cost[i - 1, j].L + K;
                        if (cost[i - 1, j].G < newMin)
                            newMin = cost[i - 1, j].G;
                        if (cost[i - 1, j].P + K < newMin)
                            newMin = cost[i - 1, j].P + K;
                        cost[i, j].G = newMin + S[i, j];
                    }
                    // prawa-góra
                    if (j == W - 1)
                        cost[i, j].P = int.MaxValue - K;
                    else
                    {
                        int newMin = cost[i - 1, j + 1].L + K;
                        if (cost[i - 1, j + 1].G + K < newMin)
                            newMin = cost[i - 1, j + 1].G + K;
                        if (cost[i - 1, j + 1].P < newMin)
                            newMin = cost[i - 1, j + 1].P;
                        cost[i, j].P = newMin + S[i, j];
                    }
                }
            }
            
            // minimum
            int minimum = int.MaxValue, indeks = -1;
            for (int i = 0; i < W; i++)
            {
                int newMin = Math.Min(Math.Min(cost[H - 1, i].L, cost[H - 1, i].G), cost[H - 1, i].P);
                if (newMin < minimum)
                {
                    minimum = newMin;
                    indeks = i;
                }
            }
            
            // wyznaczanie ścieżki
            (int, int)[] sciezka = new (int, int)[H];
            sciezka[H - 1] = (H - 1, indeks);
            char? poprz = null;
            for (int i = H - 2; i >= 0; i--)
            {
                int j = sciezka[i + 1].Item2;
                int min = cost[i + 1, j].L;
                char kier = 'L';
                if (poprz != null)
                {
                    switch (poprz)
                    {
                        case 'L':
                            min = cost[i + 1, j].L - K;
                            break;
                        case 'G':
                            min = cost[i + 1, j].G - K;
                            kier = 'G';
                            break;
                        case 'P':
                            min = cost[i + 1, j].P - K;
                            kier = 'P';
                            break;
                    }
                }
                if (cost[i + 1, j].L < min)
                {
                    min = cost[i + 1, j].L;
                    kier = 'L';
                }
                if (cost[i + 1, j].G < min)
                {
                    min = cost[i + 1, j].G;
                    kier = 'G';
                }
                if (cost[i + 1, j].P < min)
                {
                    min = cost[i + 1, j].P;
                    kier = 'P';
                }
                sciezka[i].Item1 = i;
                poprz = kier;
                switch (kier)
                {
                    case 'L':
                        sciezka[i].Item2 = j - 1;
                        break;
                    case 'G':
                        sciezka[i].Item2 = j;
                        break;
                    case 'P':
                        sciezka[i].Item2 = j + 1;
                        break;
                }
            }
            
            return (minimum, sciezka);
        }
    }
}

// public (int cost, (int i, int j)[] seam) Stage2(int[,] S, int K)
//         {
//             int H = S.GetLength(0);
//             int W = S.GetLength(1);
//
//             int[,] cost = new int[H, W];
//             List<char>[,] kier = new List<char>[H, W];
//             // tablica kierunków, z których przyszliśmy do danego pola
//             // 'L' - z lewej góry, 'G' - z góry, 'R' - z prawej góry
//
//             int i;
//             for (i = 0; i < W; i++)
//             {
//                 cost[0, i] = S[0, i];
//                 kier[0, i] = ['L', 'G', 'R'];
//             }
//             i = 1; // pierwszy ruch nie podlega karze
//             {
//                 for (int j = 0; j < W; j++)
//                 {
//                     int newCost = int.MaxValue;
//                     if (j > 0)
//                     {
//                         if (cost[i - 1, j - 1] + S[i, j] < newCost)
//                         { 
//                             newCost = cost[i - 1, j - 1] + S[i, j];
//                             kier[i, j] = ['L'];
//                         }
//                         else if (cost[i - 1, j - 1] + S[i, j] == newCost)
//                         {
//                             kier[i, j].Add('L');
//                         }
//                     }
//                     if (cost[i - 1, j] + S[i, j] < newCost)
//                     {
//                         newCost = cost[i - 1, j] + S[i, j];
//                         kier[i, j] = ['G'];
//                     }
//                     else if (cost[i - 1, j] + S[i, j] == newCost)
//                     {
//                         kier[i, j].Add('G');
//                     }
//                     if (j < W - 1)
//                     {
//                         if (cost[i - 1, j + 1] + S[i, j] < newCost)
//                         {
//                             newCost = cost[i - 1, j + 1] + S[i, j];
//                             kier[i, j] = ['R'];
//                         }
//                         else if (cost[i - 1, j + 1] + S[i, j] == newCost)
//                         {
//                             newCost = cost[i - 1, j + 1] + S[i, j];
//                             kier[i, j].Add('R');
//                         }
//                     }
//                     cost[i, j] = newCost;
//                 }
//             }
//
//             for (i = 2; i < H; i++)
//             {
//                 for (int j = 0; j < W; j++)
//                 {
//                     int newCost = int.MaxValue;
//                     if (j > 0)
//                     {
//                         if (cost[i - 1, j - 1] + S[i, j] + (kier[i - 1, j - 1].Contains('L') ? 0 : K) < newCost)
//                         {
//                             newCost = cost[i - 1, j - 1] + S[i, j] + (kier[i - 1, j - 1].Contains('L') ? 0 : K);
//                             kier[i, j] = ['L'];
//                         }
//                         else if (cost[i - 1, j - 1] + S[i, j] + (kier[i - 1, j - 1].Contains('L') ? 0 : K) == newCost)
//                         {
//                             kier[i, j].Add('L');
//                         }
//                     }
//                     if (cost[i - 1, j] + S[i, j] + (kier[i - 1, j].Contains('G') ? 0 : K) < newCost)
//                     {
//                         newCost = cost[i - 1, j] + S[i, j] + (kier[i - 1, j].Contains('G') ? 0 : K);
//                         kier[i, j] = ['G'];
//                     }
//                     else if (cost[i - 1, j] + S[i, j] + (kier[i - 1, j].Contains('G') ? 0 : K) == newCost)
//                     {
//                         kier[i, j].Add('G');
//                     }
//                     if (j < W - 1)
//                     {
//                         if (cost[i - 1, j + 1] + S[i, j] + (kier[i - 1, j + 1].Contains('R') ? 0 : K) < newCost)
//                         {
//                             newCost = cost[i - 1, j + 1] + S[i, j] + (kier[i - 1, j + 1].Contains('R') ? 0 : K);
//                             kier[i, j] = ['R'];
//                         }
//                         else if (cost[i - 1, j + 1] + S[i, j] + (kier[i - 1, j + 1].Contains('R') ? 0 : K) == newCost)
//                         {
//                             kier[i, j].Add('R');
//                         }
//                     }
//                     cost[i, j] = newCost;
//                 }
//             }
//
//             int minimum = cost[H - 1, 0], indeks = 0;
//             for (i = 0; i < W; i++)
//             {
//                 if (minimum > cost[H - 1, i])
//                 {
//                     minimum = cost[H - 1, i];
//                     indeks = i;
//                 }
//             }
//
//             (int, int)[] sciezka = new (int, int)[H];
//             sciezka[H - 1] = (H - 1, indeks);
//             for (i = H - 2; i >= 1; i--)
//             {
//                 int j = sciezka[i + 1].Item2;
//                 int nowyIndeks = j;
//                 int newCost = cost[i, j];
//                 newCost += kier[i - 1, j].Contains('G') ? 0 : K;
//                 if (j > 0)
//                 {
//                     if (cost[i, j - 1] + (kier[i - 1, j - 1].Contains('L') ? 0 : K) < newCost)
//                     {
//                         newCost = cost[i, j - 1] + (kier[i - 1, j - 1].Contains('L') ? 0 : K);
//                         nowyIndeks = j - 1;
//                     }
//                 }
//                 if (j < W - 1)
//                 {
//                     if (cost[i, j + 1] + (kier[i - 1, j + 1].Contains('R') ? 0 : K) < newCost)
//                     {
//                         newCost = cost[i, j + 1] + (kier[i - 1, j + 1].Contains('R') ? 0 : K);
//                         nowyIndeks = j + 1;
//                     }
//                 }
//                 sciezka[i] = (i, nowyIndeks);
//             }
//
//             return (minimum, sciezka);
//         }
//     }

//public (int cost, (int i, int j)[] seam) Stage2(int[,] S, int K)
//{
//    int H = S.GetLength(0);
//    int W = S.GetLength(1);

//    int[,] cost = new int[H, W];
//    char[,] kier = new char[H, W];
//    // tablica kierunków, z których przyszliśmy do danego pola
//    // 'L' - z lewej góry, 'G' - z góry, 'R' - z prawej góry

//    int i;
//    for (i = 0; i < W; i++)
//    {
//        cost[0, i] = S[0, i];
//    }
//    i = 1; // pierwszy ruch nie podlega karze
//    {
//        for (int j = 0; j < W; j++)
//        {
//            int newCost = int.MaxValue;
//            if (j > 0)
//            {
//                if (cost[i - 1, j - 1] + S[i, j] < newCost)
//                {
//                    newCost = cost[i - 1, j - 1] + S[i, j];
//                    kier[i, j] = 'L';
//                }
//            }
//            if (cost[i - 1, j] + S[i, j] < newCost)
//            {
//                newCost = cost[i - 1, j] + S[i, j];
//                kier[i, j] = 'G';
//            }
//            if (j < W - 1)
//            {
//                if (cost[i - 1, j + 1] + S[i, j] < newCost)
//                {
//                    newCost = cost[i - 1, j + 1] + S[i, j];
//                    kier[i, j] = 'R';
//                }
//            }
//            cost[i, j] = newCost;
//        }
//    }

//    for (i = 2; i < H; i++)
//    {
//        for (int j = 0; j < W; j++)
//        {
//            int newCost = int.MaxValue;
//            if (j > 0)
//            {
//                if (cost[i - 1, j - 1] + S[i, j] + (kier[i - 1, j - 1] == 'L' ? 0 : K) < newCost)
//                {
//                    newCost = cost[i - 1, j - 1] + S[i, j] + (kier[i - 1, j - 1] == 'L' ? 0 : K);
//                    kier[i, j] = 'L';
//                }
//            }
//            if (cost[i - 1, j] + S[i, j] + (kier[i - 1, j] == 'G' ? 0 : K) < newCost)
//            {
//                newCost = cost[i - 1, j] + S[i, j] + (kier[i - 1, j] == 'G' ? 0 : K);
//                kier[i, j] = 'G';
//            }
//            if (j < W - 1)
//            {
//                if (cost[i - 1, j + 1] + S[i, j] + (kier[i - 1, j + 1] == 'R' ? 0 : K) < newCost)
//                {
//                    newCost = cost[i - 1, j + 1] + S[i, j] + (kier[i - 1, j + 1] == 'R' ? 0 : K);
//                    kier[i, j] = 'R';
//                }
//            }
//            cost[i, j] = newCost;
//        }
//    }

//    int minimum = cost[H - 1, 0], indeks = 0;
//    for (i = 0; i < W; i++)
//    {
//        if (minimum > cost[H - 1, i])
//        {
//            minimum = cost[H - 1, i];
//            indeks = i;
//        }
//    }

//    (int, int)[] sciezka = new (int, int)[H];
//    sciezka[H - 1] = (H - 1, indeks);
//    for (i = H - 2; i >= 0; i--)
//    {
//        int j = sciezka[i + 1].Item2;
//        int nowyIndeks = j;
//        int newCost = cost[i, j];
//        if (j > 0)
//        {
//            if (cost[i, j - 1] < newCost)
//            {
//                newCost = cost[i, j - 1];
//                nowyIndeks = j - 1;
//            }
//        }
//        if (j < W - 1)
//        {
//            if (cost[i, j + 1] < newCost)
//            {
//                newCost = cost[i, j + 1];
//                nowyIndeks = j + 1;
//            }
//        }
//        sciezka[i] = (i, nowyIndeks);
//    }

//    return (minimum, sciezka);
//}
//    }
//}

