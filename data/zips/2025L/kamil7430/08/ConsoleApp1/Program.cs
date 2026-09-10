using System;
using System.Linq;
using System.Text;
using ASD;
using System.Collections.Generic;
using ASD.Graphs;
using System.Reflection.Metadata.Ecma335;

namespace ASD
{
    public class Lab08Stage1TestCase : TestCase
    {
        int[,] P;
        (int row, int col)[] M;
        protected readonly string description;
        protected (int s, int[] S) answer;
        int[,] solutions;

        public Lab08Stage1TestCase(int[,] P, (int row, int col)[] M, int[,] solutions, int timeLimit, string description) : base(timeLimit, null, description)
        {
            this.description = description;
            this.P = P;
            this.M = M;
            this.solutions = solutions;
        }
        protected override void PerformTestCase(object prototypeObject)
        {
            answer = ((Lab08)prototypeObject).Stage1(this.P, this.M);
            Array.Sort(answer.S);
        }

        protected override (Result resultCode, string message) VerifyTestCase(object settings)
        {
            int[,] solutions = this.solutions;

            if (solutions.Length == 0)
            {
                if (answer.s == 0)
                {
                    return OkResult("OK");
                }
                return (Result.WrongResult, $"Rozwiązanie nie istnieje, a zwrócono s > 0, {this.description}");
            }

            if (answer.s != solutions.GetLength(1))
            {
                return (Result.WrongResult, $"Zwrócono savedNum={answer.s}, powinno być {solutions.GetLength(1)}, {this.description}");
            }

            if (answer.S.GetLength(0) != solutions.GetLength(1))
            {
                return (Result.WrongResult, $"savedNum OK, ale Saved ma rozmiar {answer.S.GetLength(0)}, powinno być {solutions.GetLength(1)}, ${this.description}");
            }

            bool found = false;
            for (int i = 0; i < solutions.GetLength(0); ++i)
            {
                bool ok = true;
                for (int j = 0; j < solutions.GetLength(1); ++j)
                {
                    if (solutions[i, j] != answer.S[j])
                    {
                        ok = false;
                        break;
                    }
                }
                if (ok)
                {
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                return (Result.WrongResult, $"Liczba maszyn OK, ale zwrócono złe maszyny, {this.description}");
            }

            foreach (int i in answer.S)
            {
                if (this.P[this.M[i].row, this.M[i].col] == 0)
                {
                    return (Result.WrongResult, $"Zwrócono maszynę, dla której wartość w tablicy P wynosi 0, {this.description}");
                }
            }

            return OkResult("OK");
        }

        public (Result resultCode, string message) OkResult(string message) =>
            (TimeLimit < PerformanceTime ? Result.LowEfficiency : Result.Success,
            $"{message} {PerformanceTime.ToString("#0.00")}s");
    }

    public class Lab08Stage2TestCase : TestCase
    {
        int[,] P;
        (int row, int col)[] M;
        int[] W;
        int k;
        int bestCost;
        List<List<int>> solutions;
        protected readonly string description;
        protected (int d, int[] S) answer;

        public Lab08Stage2TestCase(int[,] P, (int row, int col)[] M, int[] W, int k, int bestCost, List<List<int>> solutions, int timeLimit, string description) : base(timeLimit, null, description)
        {
            this.description = description;
            this.P = P;
            this.M = M;
            this.W = W;
            this.k = k;
            this.bestCost = bestCost;
            this.solutions = solutions;
            // this.expectedLength = expectedLength;
        }
        protected override void PerformTestCase(object prototypeObject)
        {
            answer = ((int d, int[] S))((Lab08)prototypeObject).Stage2(this.P, this.M, this.W, this.k);
            Array.Sort(answer.S);
        }

        protected override (Result resultCode, string message) VerifyTestCase(object settings)
        {
            List<List<int>> solutions = this.solutions;

            if (answer.d < 0)
            {
                return (Result.WrongResult, $"Zwrócono ujemny zysk {answer.d}, {this.description}");
            }

            /*
            if(solutions.Count == 0)
            {
                if(answer.S.Length == 0)
                {
                    return OkResult("Ok");
                }
                return (Result.WrongResult, "Rozwiązanie nie istnieje, a zwrócono |S| > 0 maszyn");
            }
            */

            if (answer.d != this.bestCost)
            {
                return (Result.WrongResult, $"Zwrócono zły zysk. Jest {answer.d}, powinno być {this.bestCost}, {this.description}");
            }

            bool found = false;
            foreach (List<int> solution in solutions)
            {
                if (solution.Count != answer.S.Length)
                {
                    continue;
                }
                bool ok = true;
                int i = 0;
                foreach (int m in solution)
                {
                    if (m != answer.S[i])
                    {
                        ok = false;
                        break;
                    }
                    ++i;
                }
                if (ok)
                {
                    found = true;
                    break;
                }
            }

            foreach (int i in answer.S)
            {
                if (this.P[this.M[i].row, this.M[i].col] == 0)
                {
                    return (Result.WrongResult, $"Zwrócono maszynę, dla której wartość w tablicy P wynosi 0, {this.description}");
                }
            }

            if (!found)
            {
                return (Result.WrongResult, $"Koszt OK, ale zwrócono złe maszyny, {this.description}");
            }


            return OkResult("OK");
        }

        public (Result resultCode, string message) OkResult(string message) =>
            (TimeLimit < PerformanceTime ? Result.LowEfficiency : Result.Success,
            $"{message} {PerformanceTime.ToString("#0.00")}s");
    }


    public class Lab06Tests : TestModule
    {
        TestSet Stage1 = new TestSet(prototypeObject: new Lab08(), description: "Etap I", settings: true);
        TestSet Stage2 = new TestSet(prototypeObject: new Lab08(), description: "Etap II", settings: true);

        public override void PrepareTestSets()
        {
            TestSets["Etap I"] = Stage1;
            TestSets["Etap II"] = Stage2;

            PrepareTests();
        }

        void PrepareTests()
        {
            // ===== STAGE I =====
            {
                int[,] P = new int[,]
                {
                    {2, 0, 1, 0},
                    {2, 3, 1, 1},
                    {1, 1, 0, 1}
                };
                (int row, int col)[] M = new (int row, int col)[] { (0, 2), (2, 0), (2, 1), (2, 3) };
                int[,] solutions =
                {
                    {0, 1, 2},
                    {0, 1, 3},
                    {0, 2, 3},
                    {1, 2, 3}
                };
                Stage1.TestCases.Add(new Lab08Stage1TestCase(P, M, solutions, 1, "Przykład 1 z treści zadania"));
            }
            {
                int[,] P = new int[,]
                {
                    {1, 1, 0, 0},
                    {1, 1, 1, 1},
                    {1, 1, 0, 1}
                };
                (int row, int col)[] M = new (int row, int col)[] { (2, 1), (2, 3) };
                int[,] solutions =
                {
                    {0, 1}
                };
                Stage1.TestCases.Add(new Lab08Stage1TestCase(P, M, solutions, 1, "Przykład 2 z treści zadania"));
            }
            {
                int[,] P = new int[,]
                {
                    {1, 0, 1, 0 },
                    {1, 0, 1, 0 },
                    {1, 0, 1, 0 },
                    {1, 0, 1, 0 },
                    {1, 0, 1, 0 }
                };
                (int row, int col)[] M = new (int row, int col)[] { (0, 1), (3, 1), (3, 3), (4, 3) };
                int[,] solutions = { };
                Stage1.TestCases.Add(new Lab08Stage1TestCase(P, M, solutions, 1, "Brak rozwiązań"));

            }
            {
                int[,] P = new int[,]
                {
                    {3},
                    {4 },
                    {3 },
                    {100000 },
                    {5 },
                    {10 }
                };
                (int row, int col)[] M = new (int row, int col)[] { (0, 0), (2, 0), (3, 0), (4, 0) };
                int[,] solutions = {
                    {0, 1, 2 },
                    {0, 1, 3 },
                    {0, 2, 3 },
                    {1, 2, 3 }
                };
                Stage1.TestCases.Add(new Lab08Stage1TestCase(P, M, solutions, 1, "Edge case, jedna kolumna"));
            }
            {
                int[,] P = new int[,]
                {
                    {4, 2, 0, 1, 0 }
                };
                (int row, int col)[] M = new (int row, int col)[] { (0, 0), (0, 1), (0, 3), (0, 4) };
                int[,] solutions =
                {
                    {0, 1, 2 }
                };
                Stage1.TestCases.Add(new Lab08Stage1TestCase(P, M, solutions, 1, "Edge case, jeden wiersz"));
            }
            {
                int[,] P = null;
                (int row, int col)[] M = null;
                (P, M) = randomStageI(20, 20, 10, 3, 42);
                int[,] solutions = { { 0, 8 }, { 2, 8 }, { 3, 8 }, { 6, 8 } };
                // Utils.generateAllSolutions(P, M, "out.txt");
                Stage1.TestCases.Add(new Lab08Stage1TestCase(P, M, solutions, 1, "Test losowy 1"));
            }
            {
                int[,] P = null;
                (int row, int col)[] M = null;
                (P, M) = randomStageI(50, 60, 15, 5, 123);
                int[,] solutions = { { 0, 1, 2, 4, 5, 6, 7, 8, 9, 11, 12, 13, 14 } };
                // Utils.generateAllSolutions(P, M, "out.txt");
                Stage1.TestCases.Add(new Lab08Stage1TestCase(P, M, solutions, 1, "Test losowy 2"));
            }
            {
                int[,] P = null;
                (int row, int col)[] M = null;
                (P, M) = randomStageI(60, 50, 15, 3, 321);
                int[,] solutions = {
                    { 1, 2, 5, 6, 8, 10 },
                    { 1, 2, 5, 6, 8, 13 },
                    { 1, 2, 5, 8, 10, 13 },
                    { 1, 5, 6, 8, 10, 13},
                    { 2, 5, 6, 8, 10, 13}
                };
                // Utils.generateAllSolutions(P, M, "out.txt");
                Stage1.TestCases.Add(new Lab08Stage1TestCase(P, M, solutions, 1, "Test losowy 3"));
            }

            (int[,] P, (int row, int col)[] M) randomStageI(int h, int w, int numMachines, int maxP, int seed)
            {
                Random random = new Random(seed);
                int[,] P = new int[h, w];
                for (int r = 0; r < h; ++r)
                {
                    for (int c = 0; c < w; ++c)
                    {
                        P[r, c] = random.Next(0, maxP);
                    }
                }
                (int row, int col)[] M = GenerateDistinctPairs(h, w, numMachines, random);
                return (P, M);
            }


            (int row, int col)[] GenerateDistinctPairs(int h, int w, int n, Random random)
            {
                var distinctPairs = new HashSet<(int row, int col)>();

                while (distinctPairs.Count < n)
                {
                    int x = random.Next(0, h);
                    int y = random.Next(0, w);

                    distinctPairs.Add((x, y));
                }

                return distinctPairs.ToArray();
            }
            // ===== STAGE I  =====
            // ===== STAGE II =====
            {
                int[,] P = new int[,]
                {
                    {1, 1, 0, 0},
                    {1, 1, 1, 1},
                    {1, 1, 0, 1}
                };
                (int row, int col)[] M = new (int row, int col)[] { (2, 1), (2, 3) };
                int[] W = new int[] { 1000, 400 };
                int k = 100;
                int bestCost = 700;
                List<List<int>> solutions = new List<List<int>>()
                {
                    new List<int>(){0}
                };
                Stage2.TestCases.Add(new Lab08Stage2TestCase(P, M, W, k, bestCost, solutions, 1, "Przykład z treści zadania"));
            }
            {
                int[,] P = new int[,]
                {
                    {1, 1, 0, 0},
                    {1, 1, 1, 1},
                    {1, 1, 0, 1}
                };
                (int row, int col)[] M = new (int row, int col)[] { (2, 1), (2, 3) };
                int[] W = new int[] { 1000, 5000 };
                int k = 100;
                int bestCost = 5100;
                List<List<int>> solutions = new List<List<int>>()
                {
                    new List<int>(){0, 1}
                };
                Stage2.TestCases.Add(new Lab08Stage2TestCase(P, M, W, k, bestCost, solutions, 1, "Przykład z treści zadania, żółta maszyna warta 5000"));
            }
            {
                int[,] P = new int[,]
                {
                    {5, 5, 5, 5, 5},
                    {5, 5, 5, 5, 5},
                    {5, 5, 5, 5, 5},
                    {5, 5, 5, 5, 5},
                    {5, 5, 5, 5, 5}
                };
                (int row, int col)[] M = new (int row, int col)[] { (0, 4), (1, 1), (3, 4) };
                int[] W = new int[] { 10, 20, 30 };
                int k = 50;
                int bestCost = 0;
                List<List<int>> solutions = new List<List<int>>() { new List<int>() };
                Stage2.TestCases.Add(new Lab08Stage2TestCase(P, M, W, k, bestCost, solutions, 1, "Nie opłaca się ratować żadnej maszyny"));
            }
            {
                int[,] P = new int[,] {
                    {4, 0, 2, 2},
                    {4, 3, 0, 3 },
                    {4, 2, 0, 0},
                    {2, 0, 2, 3}
                };
                (int row, int col)[] M = new (int row, int col)[] { (2, 0), (1, 2), (3, 1), (0, 2), (2, 2) };
                int[] W = new int[] { 4, 4, 1, 2, 5 };
                int k = 1;
                // (P, M, W, k) = randomStageII(4, 4, 5, 4, 5, 5, 768);
                List<List<int>> solutions = new List<List<int>> { new List<int>() { 0, 3 } };
                int bestCost = 2;
                // Utils.generateAllStage2Solutions(P, M, W, k, "out2.txt");
                Stage2.TestCases.Add(new Lab08Stage2TestCase(P, M, W, k, bestCost, solutions, 1, "Test losowy 1"));
            }
            {
                int[,] P = null;
                (int row, int col)[] M = null;
                int[] W = null;
                int k = -1;
                (P, M, W, k) = randomStageII(6, 5, 5, 7, 5, 5, 768);
                List<List<int>> solutions = new List<List<int>> {
                    new List<int>() {}, new List<int>() {1}
                };
                int bestCost = 0;
                // Utils.generateAllStage2Solutions(P, M, W, k, "out2.txt");
                Stage2.TestCases.Add(new Lab08Stage2TestCase(P, M, W, k, bestCost, solutions, 1, "Test losowy 2"));
            }
            {
                int[,] P = new int[,] {
                    { 5, 0, 3, 2, 5, 4, 0, 4},
                    { 5, 2, 0, 0, 3, 0, 3, 3},
                    { 4, 0, 2, 4, 5, 1, 4, 1},
                    { 5, 2, 1, 3, 4, 3, 4, 4},
                    { 0, 1, 4, 0, 1, 4, 5, 0},
                    { 0, 0, 3, 1, 4, 0, 3, 2},
                    { 1, 4, 3, 5, 4, 5, 5, 4},
                    { 2, 1, 3, 2, 2, 2, 0, 2}
                };
                (int row, int col)[] M = new(int row, int col)[] {
                    (6, 2),
                    (7, 2),
                    (1, 4),
                    (3, 7),
                    (2, 3),
                    (4, 6),
                    (5, 4),
                    (6, 5),
                    (2, 7),
                    (7, 6)
                };
                int[] W = new int[] { 5, 1, 2, 1, 4, 5, 2, 3, 3, 4 };
                int k = 1;
                // (P, M, W, k) = randomStageII(8, 8, 10, 5, 5, 3, 768);
                List<List<int>> solutions = new List<List<int>> {
                    new List<int>() {}, new List<int>() {2}, new List<int>(){4},
                    new List<int>() {2, 4}, new List<int>() {8}, new List<int>(){2, 8},
                    new List<int>() {4, 8}, new List<int>() {2, 4, 8}
                };
                int bestCost = 0;
                // Utils.generateAllStage2Solutions(P, M, W, k, "out2.txt");
                Stage2.TestCases.Add(new Lab08Stage2TestCase(P, M, W, k, bestCost, solutions, 1, "Test losowy 2"));
            }
            {
                var (P, M, W, k) = randomStageII(30, 10, 12, 8, 100, 5, 948);
                List<List<int>> solutions = new List<List<int>>() { new List<int>() {1, 8}, new List<int>() { 0, 1, 8 } };
                int bestCost = 85;
                // Utils.generateAllStage2Solutions(P, M, W, k, "out2.txt");
                Stage2.TestCases.Add(new Lab08Stage2TestCase(P, M, W, k, bestCost, solutions, 10, "Duży test losowy 1"));
            }
            {
                var (P, M, W, k) = randomStageII(16, 18, 10, 8, 1000, 100, 948);
                List<List<int>> solutions = new List<List<int>>() { new List<int>() { 0, 1, 4, 5, 6, 7, 9 } };
                int bestCost = 3446;
                // Utils.generateAllStage2Solutions(P, M, W, k, "out3.txt");
                Stage2.TestCases.Add(new Lab08Stage2TestCase(P, M, W, k, bestCost, solutions, 5, "Duży test losowy 2"));
            }

            (int[,] P, (int row, int col)[] M, int[] W, int k) randomStageII(int h, int w, int numMachines, int maxP, int maxW, int maxK, int seed)
            {
                Random random = new Random(seed);
                int[,] P = new int[h, w];
                for (int r = 0; r < h; ++r)
                {
                    for (int c = 0; c < w; ++c)
                    {
                        // P[r, c] = random.Next(0, maxP+1);
                        P[r, c] = random.Next(0, maxP + 1);
                    }
                }
                (int row, int col)[] M = GenerateDistinctPairs(h, w, numMachines, random);
                int[] W = new int[numMachines];
                for(int i = 0; i < numMachines; ++i)
                {
                    W[i] = random.Next(1, maxW+1);
                }
                int k = random.Next(1, maxK+1);
                return (P, M, W, k);
            }
            // ===== STAGE II =====

        }

    }

    class Program
    {
        static void Main(string[] args)
        {
            var tests = new Lab06Tests();
            tests.PrepareTestSets();
            foreach (var ts in tests.TestSets)
            {
                ts.Value.PerformTests(verbose: true, checkTimeLimit: false);
            }
        }
    }
}