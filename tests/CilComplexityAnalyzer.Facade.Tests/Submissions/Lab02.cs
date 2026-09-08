using System;
//using System.Runtime.Intrinsics.Arm;

namespace NewTester.Lab02.Submissions
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
            int[,] dp = new int[H, W];

            (int,int)[,] position = new (int,int)[H, W];

            (int, int)[] seam = new (int, int)[H];


            for(int j = 0; j<W; j++)
            {
                dp[0, j] = S[0, j];
                position[0, j] = (0, j);
            }



            //pomysl prostszy: dodajmey koszt wymagany czyli S[i, j]
            //i robimy deafaut z jako góre i potem lecmy lewo albo prawo jeśli się da
            for (int i=1; i<H; i++)
            {
                for(int j=0; j<W; j++)
                {
                    dp[i, j] = S[i, j] + dp[i - 1, j];
                    position[i, j] = (i - 1, j);
                    if (j > 0 && S[i, j] + dp[i-1, j-1] < dp[i, j])
                    {
                        dp[i,j] = S[i, j] + dp[i -1, j - 1];
                        position[i, j] = (i - 1, j-1);
                    }
                    if (j < W-1 && S[i, j] + dp[i-1, j+1] < dp[i, j])
                    {
                        dp[i, j] = S[i, j] + dp[i - 1, j + 1];
                        position[i, j] = (i - 1, j + 1);
                    }
                }
            }

            //wyznaczam index dla dla najmniejszej sciezki:
            int index = 0;
            int odp = int.MaxValue;
            for(int j=0; j<W; j++)
            {
                if(odp > dp[H-1, j])
                {
                    odp = dp[H-1, j];
                    index = j;
                }
            }
            seam[H - 1] = (H - 1, index);
            //znacznie prosciej niz wczeniej bo bez if'ow!!! 
            // zdobywamy teras sciezke: fajny pomsyl -> jak mamy wsm elementy to
            //pomzemy zmienic index na obecny j taki jaki tam jest zapisany
            int previousI, previousJ;
            for (int i = H - 1; i > 0; i--)
            {
                (previousI, previousJ) = position[i, index];
                seam[i - 1] = (previousI, previousJ);
                index = previousJ;
            }
            return (odp, seam);
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

        //Pomsył: Robimy to samo, tylko bierzemy jeszcze pod uwage skad idziemy ( jako 3 wymiar)
        // odpowiednio 0 oznacza przyjscie z lewej, 1 oznacza przyjscie z gory, 2 oznacza przyjscie w prawej
        public (int cost, (int i, int j)[] seam) Stage2(int[,] S, int K)
        {
            int H = S.GetLength(0);
            int W = S.GetLength(1);
            int[,,] dp = new int[H, W, 3];

            (int, int)[] seam = new (int, int)[H];
            (int, int, int)[,,] prev = new (int, int, int)[H, W, 3];


            //uzupelnijmy gorna warste
            for (int j = 0; j < W; j++)
            {
                for (int dim = 0; dim < 3; dim++)
                {
                    dp[0, j, dim] = S[0, j];
                    prev[0, j, dim] = (0, j, 0);
                }
            }

            //ustawiamy pozostale wartosic na nieskonczonosc ( najwazniejsze jest aby
            //brzegowe wartosci byly inf ( oczywiscei -1000000 w razie czego )
            for (int i = 1; i < H; i++)
            {
                for (int j = 0; j < W; j++)
                {
                    for (int dim = 0; dim < 3; dim++)
                    {
                        dp[i, j, dim] = int.MaxValue - 1000000;
                    }
                }
            }

            //uzupelnijmu poziom 1 ( czyli drugi od gory) osobno bo bez Kar jest
            // nie da sie zrobic kary gdy nie wiadomo skad przyszlismy wczesniej
            // Zlozonosc O(W*3*3) = O(W)
            // trzeba wyIf'owac przypadki niemozliwe czyli przyjscie od lewej gdzie nie ma niczego z lewej
            // ( dlatego dp w wartosciach ((j == 0) && (dim == 0)) oraz ((j == W - 1) && (dim == 2))) powinny 
            // zostac rowne inf ( lub int.MaxValue))
            for (int j = 0; j < W; j++)
            {
                for (int dim = 0; dim < 3; dim++)
                {
                    for(int dimWczes = 0; dimWczes<3; dimWczes++)
                    {
                        if (!((j == 0) && (dim == 0)) && !((j == W - 1) && (dim == 2)))
                        {
                            if (dp[0, j + dim - 1, dimWczes] + S[1, j] < dp[1, j, dim])
                            {
                                dp[1, j, dim] = dp[0, j + dim - 1, dimWczes] + S[1, j];
                                prev[1, j, dim] = (0, j + dim -1, dimWczes);
                            }
                        }
                    }
                }
            }

            //Prosze wzrocic uwage, ze zlozonsc jest O(H*W*3*3) czyli O(O*W)
            // pisze to poniewaz cztery pętle wyglądają groźnie, ale sa one tylko dla wygody i przejrzystosci kodu
            // ( możnaby HardCodowo wpisać 9 if'ów ale to by było za dużo kodu)
            // Pisze w razie czego jakbym za tydzien aby pozniej sie szybko wybronic
            // gdyby Pan pytal, a jak pod presja nie wiedzialmyb co powiedziec
            for(int i=2; i<H; i++)
            {
                for(int j=0; j<W; j++)
                {
                    for(int dim = 0; dim < 3; dim++)
                    {
                        for(int dimWczes = 0; dimWczes<3; dimWczes++)
                        {
                            if (!((j == 0) && (dim == 0)) && !((j == W - 1) && (dim == 2)))
                            {
                                int kara = (dimWczes == dim) ? 0 : K;
                                if (dp[i-1, j+dim-1, dimWczes] + kara + S[i, j] < dp[i, j, dim])
                                {
                                    dp[i, j, dim] = dp[i - 1, j + dim - 1, dimWczes] + kara + S[i, j];
                                    prev[i, j, dim] = (i - 1, j + dim -1, dimWczes);
                                }
                            }
                        }
                    }
                }
            }

            //wyznaczam index dla dla najmniejszej sciezki:
            int indexJ = 0;
            int indexDim = 0;
            int odp = int.MaxValue;
            for (int j = 0; j < W; j++)
            {
                for(int dim = 0; dim<3; dim++)
                {
                    if (dp[H - 1, j, dim] < odp)
                    {
                        odp = dp[H - 1, j, dim];
                        indexJ = j;
                        indexDim = dim;
                    }
                }
            }

            //odtwarzaie sciezki
            //Analogicznie do poprzedniego etapu
            int tempI = H - 1,
                tempJ = indexJ,
                tempDim = indexDim;
            seam[H-1] = (tempI, tempJ);

            int prevI, prevJ, prevDim;
            for (int i = H - 1; i > 0; i--)
            {
                (prevI, prevJ, prevDim) = prev[tempI, tempJ, tempDim]; 
                seam[i-1] = (prevI, prevJ);
                tempI = prevI;
                tempJ = prevJ;
                tempDim = prevDim;
            }
            return (odp, seam);
        }
    }
}
