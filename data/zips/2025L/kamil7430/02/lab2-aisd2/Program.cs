using System;
using System.Linq;
using System.Text;
using ASD;
using System.Collections.Generic;

namespace ASD
{
    public abstract class Lab02TestCase : TestCase
    {
        protected readonly int H;
        protected readonly int W;
        protected readonly int[,] S;
        protected readonly int expectedCost;
        protected (int returnedCost, (int i, int j)[] seam) result;

        protected Lab02TestCase(int H, int W, int[,] S, int expectedCost, int timeLimit, string description)
            : base(timeLimit, null, description)
        {
            this.H = H;
            this.W = W;
            this.S = S;
            this.expectedCost = expectedCost;
        }

        protected override (Result resultCode, string message) VerifyTestCase(object settings)
        {
            var (code, msg) = CheckDeclaredCost();
            return (code, $"{msg} [{this.Description}]");
        }

        /// <summary>
        /// Oblicza podstawowy koszt ścieżki – sumę wartości pikseli według macierzy S.
        /// </summary>
        protected int ComputeBasicCost((int i, int j)[] seam)
        {
            int cost = 0;
            foreach (var pos in seam)
                cost += S[pos.i, pos.j];
            return cost;
        }

        /// <summary>
        /// Domyślna implementacja naliczania dodatkowej kary – w Etapie 1 nie naliczamy żadnej kary.
        /// </summary>
        protected virtual int ExtraPenalty()
        {
            return 0;
        }

        /// <summary>
        /// Sprawdza, czy zwrócony przez implementację studencką koszt ścieżki jest poprawnie obliczony.
        /// Walidacja obejmuje:
        /// - czy ścieżka nie jest null,
        /// - czy zawiera dokładnie H elementów,
        /// - czy zaczyna się w wierszu 0 i kończy w wierszu H-1,
        /// - czy kolejne kroki są poprawne (różnica wierszy = 1, różnica kolumn = -1, 0 lub 1),
        /// - porównanie sumy wartości pikseli (z macierzy S) oraz dodatkowej kary (ExtraPenalty) z deklarowanym kosztem.
        /// </summary>
        protected (Result resultCode, string message) CheckDeclaredCost()
        {
            if (result.returnedCost != this.expectedCost)
                return (Result.WrongResult, $"Zwrócono wartość kosztu {result.returnedCost}, a powinno być {this.expectedCost}");

            if (result.seam == null)
                return (Result.WrongResult, "Zwrócono null zamiast ścieżki");

            if (result.seam.Length != H)
                return (Result.WrongResult, $"Zwrócona ścieżka ma długość {result.seam.Length}, powinna mieć {H}");

            if (result.seam[0].i != 0)
                return (Result.WrongResult, "Ścieżka nie zaczyna się w wierszu 0");
            if (result.seam[result.seam.Length - 1].i != H - 1)
                return (Result.WrongResult, "Ścieżka nie kończy się w ostatnim wierszu");

            (int i, int j) pos = result.seam[0];
            for (int idx = 1; idx < result.seam.Length; idx++)
            {
                var curr = result.seam[idx];
                if (curr.i - pos.i != 1)
                    return (Result.WrongResult, $"Różnica wierszy między krokami nie wynosi 1: {pos} -> {curr}");
                int dj = curr.j - pos.j;
                if (dj < -1 || dj > 1)
                    return (Result.WrongResult, $"Różnica kolumn między krokami powinna wynosić -1, 0 lub 1: {pos} -> {curr}");
                if (curr.j < 0 || curr.j >= W)
                    return (Result.WrongResult, $"Krok wychodzi poza planszę: {curr}");
                pos = curr;
            }

            int basicCost = ComputeBasicCost(result.seam);
            int penalty = ExtraPenalty();
            int totalCost = basicCost + penalty;
            if (totalCost != result.returnedCost)
                return (Result.WrongResult, $"Obliczony koszt (suma wartości pikseli + kara {penalty}) wynosi {totalCost}, a zwrócony koszt to {result.returnedCost}");
            return OkResult("OK");
        }

        public (Result resultCode, string message) OkResult(string message) =>
            (TimeLimit < PerformanceTime ? Result.LowEfficiency : Result.Success,
            $"{message} {PerformanceTime.ToString("#0.00")}s");
    }

    public class Stage1TestCase : Lab02TestCase
    {
        public Stage1TestCase(int H, int W, int[,] S, int expectedCost, int timeLimit, string description)
            : base(H, W, S, expectedCost, timeLimit, description)
        { }

        protected override void PerformTestCase(object prototypeObject)
        {
            // Zakładamy, że studenci implementują klasę Lab02 z metodą Stage1, która przyjmuje tylko macierz S.
            result = ((Lab02)prototypeObject).Stage1(S);
        }
        // W Etapie 1 domyślna implementacja ExtraPenalty (zwracająca 0) jest wystarczająca.
    }

    public class Stage2TestCase : Lab02TestCase
    {
        private readonly int K;
        public Stage2TestCase(int H, int W, int[,] S, int K, int expectedCost, int timeLimit, string description)
            : base(H, W, S, expectedCost, timeLimit, description)
        {
            this.K = K;
        }

        protected override void PerformTestCase(object prototypeObject)
        {
            // Zakładamy, że studenci implementują klasę Lab02 z metodą Stage2, która przyjmuje tylko macierz S oraz parametr K.
            result = ((Lab02)prototypeObject).Stage2(S, K);
        }

        protected override int ExtraPenalty()
        {
            // W Etapie 2 naliczamy karę za zmianę kierunku.
            int penalty = 0;
            var seam = result.seam;
            if (seam.Length < 2)
                return penalty;
            int prevDj = seam[1].j - seam[0].j;
            for (int idx = 2; idx < seam.Length; idx++)
            {
                int currDj = seam[idx].j - seam[idx - 1].j;
                if (currDj != prevDj)
                    penalty += K;
                prevDj = currDj;
            }
            return penalty;
        }
    }

    public class Lab02Tests : TestModule
    {
        TestSet Stage1 = new TestSet(prototypeObject: new Lab02(), description: "Etap 1", settings: true);
        TestSet Stage2 = new TestSet(prototypeObject: new Lab02(), description: "Etap 2", settings: true);

        public override void PrepareTestSets()
        {
            TestSets["Stage1"] = Stage1;
            TestSets["Stage2"] = Stage2;
            Prepare();
        }

        private void addStage1(Stage1TestCase testCase)
        {
            Stage1.TestCases.Add(testCase);
        }

        private void addStage2(Stage2TestCase testCase)
        {
            Stage2.TestCases.Add(testCase);
        }

        // Funkcja tworząca macierz jednorodną o szerokości W i wysokości H = 2*W, wypełnioną wartością 'value'
        private int[,] CreateHomogeneousMatrix(int W, int value)
        {
            int H = 2 * W;
            int[,] matrix = new int[H, W];
            for (int i = 0; i < H; i++)
            {
                for (int j = 0; j < W; j++)
                {
                    matrix[i, j] = value;
                }
            }
            return matrix;
        }

        // Funkcja tworząca macierz gradientową o szerokości W i wysokości H = 2*W.
        // Każdy element macierzy obliczany jest jako: baseValue + (i * stepRow) + (j * stepCol).
        private int[,] CreateGradientMatrix(int W, int baseValue, int stepRow, int stepCol)
        {
            int H = 2 * W;
            int[,] matrix = new int[H, W];
            for (int i = 0; i < H; i++)
            {
                for (int j = 0; j < W; j++)
                {
                    matrix[i, j] = baseValue + i * stepRow + j * stepCol;
                }
            }
            return matrix;
        }

        // Funkcja tworząca macierz z barierą.
        // Plansza ma wymiary: H = 2*W + 1 (wiersze) oraz W (kolumny).
        // Wartości:
        // - wartosc_duza: przypisywana na pierwszej (j==0) i ostatniej kolumnie (j==W-1) we wszystkich wierszach,
        // - wartosc_sciana: przypisywana w drugiej (j==1) i przedostatniej kolumnie (j==W-2) dla wierszy 1..H-2,
        //   ORAZ w środkowym wierszu (i == W) dla kolumn od 1 do W-2,
        // - wartosc_mala: w pozostałych polach.
        private int[,] CreateBarrierMatrix(int W, int wartosc_sciana, int wartosc_duza, int wartosc_mala)
        {
            int H = 2 * W + 1;
            if (wartosc_duza * H >= wartosc_mala * (H - 1) + wartosc_sciana)
            {
                throw new ArgumentException("Nieprawidłowe parametry dla macierzy bariery: wartosc_duza * H musi być większe niż wartosc_mala * (H-1) + wartosc_sciana.");
            }

            int[,] matrix = new int[H, W];
            for (int i = 0; i < H; i++)
            {
                for (int j = 0; j < W; j++)
                {
                    // Najpierw: pierwsza i ostatnia kolumna mają wartosc_duza.
                    if (j == 0 || j == W - 1)
                    {
                        matrix[i, j] = wartosc_duza;
                    }
                    else if ((i == 0 || i == H - 1) && (j == 1 || j == W - 2))
                    {
                        matrix[i, j] = wartosc_duza;
                    }
                    // Następnie: dla wierszy poza pierwszym i ostatnim, w kolumnach 1 i W-2 przypisujemy wartosc_sciana.
                    else if ((i != 0 && i != H - 1) && (j == 1 || j == W - 2))
                    {
                        matrix[i, j] = wartosc_sciana;
                    }
                    // Dodatkowo: w środkowym wierszu (i == W), poza pierwszą i ostatnią kolumną, przypisujemy wartosc_sciana.
                    else if (i == W && (j != 0 && j != W - 1))
                    {
                        matrix[i, j] = wartosc_sciana;
                    }
                    else
                    {
                        matrix[i, j] = wartosc_mala;
                    }
                }
            }
            return matrix;
        }

        // Funkcja generująca losową macierz o wymiarach H x W,
        // gdzie każdy element jest losowany jednostajnie z przedziału [los_min, los_max]
        private int[,] CreateRandomMatrix(int W, int H, int los_min, int los_max, Random rand)
        {
            int[,] matrix = new int[H, W];
            for (int i = 0; i < H; i++)
            {
                for (int j = 0; j < W; j++)
                {
                    matrix[i, j] = rand.Next(los_min, los_max + 1);
                }
            }
            return matrix;
        }

        private void Prepare()
        {
            // Przykładowy test z treści zadania
            int[,] S = new int[5, 5]
            {
                {3, 2, 1, 3, 7},
                {6, 1, 8, 2, 7},
                {5, 9, 3, 9, 9},
                {4, 4, 1, 5, 6},
                {7, 2, 3, 8, 1}
            };
            // Etap 1: zgodnie z przykładem, spodziewany koszt = 8
            addStage1(new Stage1TestCase(H: 5, W: 5, S: S, expectedCost: 8, timeLimit: 1, description: "Przykład z zadania: Etap 1"));
            // Etap 2: dla K = 2, zgodnie z przykładem, spodziewany koszt = 13
            addStage2(new Stage2TestCase(H: 5, W: 5, S: S, K: 2, expectedCost: 13, timeLimit: 1, description: "Przykład z zadania: Etap 2"));

            // Test minimalny
            int[,] Smin = new int[2, 2]
            {
                {1, 2},
                {3, 4}
            };
            // Etap 1: minimalny test, spodziewany koszt = 4
            addStage1(new Stage1TestCase(H: 2, W: 2, S: Smin, expectedCost: 4, timeLimit: 1, description: "Test minimalny"));
            // Etap 2: dla K = 2, spodziewany koszt = 4 (ponieważ przy jednym kroku nie ma zmiany kierunku)
            addStage2(new Stage2TestCase(H: 2, W: 2, S: Smin, K: 2, expectedCost: 4, timeLimit: 1, description: "Test minimalny"));

            // Test wredny 1
            int[,] S_wredny1 = new int[5, 5]
            {
                {1, 999, 999, 999, 1},
                {999, 1, 999, 1, 999},
                {999, 999, 1, 999, 999},
                {999, 1, 999, 1, 999},
                {2, 999, 999, 999, 1}
            };
            addStage2(new Stage2TestCase(H: 5, W: 5, S: S_wredny1, K: 2, expectedCost: 5, timeLimit: 1, description: "Test wredny 1"));

            // Test wredny 2
            int[,] S_wredny2 = new int[5, 5]
            {
                {1, 999, 999, 999, 1},
                {999, 1, 999, 1, 999},
                {999, 999, 1, 999, 999},
                {999, 1, 999, 1, 999},
                {1, 999, 999, 999, 2}
            };
            addStage2(new Stage2TestCase(H: 5, W: 5, S: S_wredny2, K: 2, expectedCost: 5, timeLimit: 1, description: "Test wredny 2"));


            // Test jednorodny
            int W_hom = 100;
            int H_hom = 2 * W_hom;
            int[,] Shom = CreateHomogeneousMatrix(W_hom, 5);
            int expectedCostHom = H_hom * 5;
            addStage1(new Stage1TestCase(H: H_hom, W: W_hom, S: Shom, expectedCost: expectedCostHom, timeLimit: 1, description: "Test jednorodny"));
            addStage2(new Stage2TestCase(H: H_hom, W: W_hom, S: Shom, K: 2, expectedCost: expectedCostHom, timeLimit: 1, description: "Test jednorodny"));

            // Test gradientowy
            int W_grad = 500;
            int H_grad = 2 * W_grad;
            int baseValue = 100;
            int stepRow = 1;
            int stepCol = 2;
            int[,] Sgrad = CreateGradientMatrix(W_grad, baseValue, stepRow, stepCol);
            // Przyjmujemy, że najtańsza ścieżka to pozostanie w kolumnie 0:
            int expectedCostGrad = baseValue * H_grad + stepRow * (H_grad - 1) * H_grad / 2;
            addStage1(new Stage1TestCase(H: H_grad, W: W_grad, S: Sgrad, expectedCost: expectedCostGrad, timeLimit: 1, description: "Test gradientowy"));
            addStage2(new Stage2TestCase(H: H_grad, W: W_grad, S: Sgrad, K: 2, expectedCost: expectedCostGrad, timeLimit: 5, description: "Test gradientowy"));

            // Test z barierą
            int W_bar = 100;
            int H_bar = 2 * W_bar + 1;
            int wartosc_duza = 100;
            int wartosc_sciana = 600000;
            int wartosc_mala = 1;
            int[,] S_bar = CreateBarrierMatrix(W_bar, wartosc_sciana, wartosc_duza, wartosc_mala);
            int expectedCostBarrier = H_bar * wartosc_duza;
            addStage1(new Stage1TestCase(H: H_bar, W: W_bar, S: S_bar, expectedCost: expectedCostBarrier, timeLimit: 1, description: "Test z barierą"));
            addStage2(new Stage2TestCase(H: H_bar, W: W_bar, S: S_bar, K: 2, expectedCost: expectedCostBarrier, timeLimit: 1, description: "Test z barierą"));

            // Testy losowe
            Random rand = new Random(2025);
            int los_min = 1, los_max = 10;
            int K_random = 3; // przykładowa kara

            // Test 1: macierz 10x15
            {
                int W_rand = 10;
                int H_rand = 15;
                int[,] S_rand = CreateRandomMatrix(W_rand, H_rand, los_min, los_max, rand);
                addStage1(new Stage1TestCase(H: H_rand, W: W_rand, S: S_rand, expectedCost: 37, timeLimit: 1, description: "Test losowy: 10x15"));
                addStage2(new Stage2TestCase(H: H_rand, W: W_rand, S: S_rand, K: K_random, expectedCost: 58, timeLimit: 1, description: "Test losowy: 10x15"));
            }

            // Test 2: macierz 160x240
            {
                int W_rand = 10 * 16;
                int H_rand = 15 * 16;
                int[,] S_rand = CreateRandomMatrix(W_rand, H_rand, los_min, los_max, rand);
                addStage1(new Stage1TestCase(H: H_rand, W: W_rand, S: S_rand, expectedCost: 557, timeLimit: 1, description: "Test losowy: 160x240"));
                addStage2(new Stage2TestCase(H: H_rand, W: W_rand, S: S_rand, K: K_random, expectedCost: 815, timeLimit: 1, description: "Test losowy: 160x240"));
            }

            // Test 3: macierz 1280x1920
            {
                int W_rand = 10 * 16 * 8;
                int H_rand = 15 * 16 * 8;
                int[,] S_rand = CreateRandomMatrix(W_rand, H_rand, los_min, los_max, rand);
                addStage1(new Stage1TestCase(H: H_rand, W: W_rand, S: S_rand, expectedCost: 4409, timeLimit: 5, description: "Test losowy: 1280x1920"));
                addStage2(new Stage2TestCase(H: H_rand, W: W_rand, S: S_rand, K: K_random, expectedCost: 6623, timeLimit: 10, description: "Test losowy: 1280x1920"));
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var tests = new Lab02Tests();
            tests.PrepareTestSets();
            foreach (var ts in tests.TestSets)
            {
                ts.Value.PerformTests(verbose: true, checkTimeLimit: false);
            }
        }
    }
}
