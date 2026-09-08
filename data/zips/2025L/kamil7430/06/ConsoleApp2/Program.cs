using System;
using System.Linq;
using System.Text;
using ASD;
using System.Collections.Generic;
using ASD.Graphs;
using System.Reflection.Metadata.Ecma335;

namespace ASD
{
    public class Lab06Stage1TestCase : TestCase
    {
        protected readonly DiGraph<int> G;
        protected readonly int[] waitTime;
        protected readonly int s;
        protected readonly string description;
        protected readonly int expectedLength;
        protected (int t, int l, int[] path) answer;

        public Lab06Stage1TestCase(DiGraph<int> G, int[] waitTime, int s, int expectedLength, int timeLimit, string description) : base(timeLimit, null, description)
        {
            this.description = description;
            this.G = G;
            this.waitTime = waitTime;
            this.s = s;
            this.expectedLength = expectedLength;
        }
        protected override void PerformTestCase(object prototypeObject)
        {
            answer = ((Lab06)prototypeObject).Stage1(G, waitTime, s);
        }

        protected override (Result resultCode, string message) VerifyTestCase(object settings)
        {
            (var code, var message) = _VerifyTestCase(settings);
            return (code, $"{message} [{Description}]");
        }

        protected (Result resultCode, string message) _VerifyTestCase(object settings)
        {
            string goodPathLength = "Długość trasy OK, ale";
            if (answer.l != expectedLength)
                return (Result.WrongResult, $"Zwrócona długość wycieczki ({answer.l}) jest inna od oczekiwanej ({expectedLength})");
            if (answer.path == null || answer.path.Length == 0)
                return (Result.WrongResult, $"{goodPathLength} zwrócona ścieżka jest pusta");
            if (answer.path[0] != s)
                return (Result.WrongResult, $"{goodPathLength} ścieżka nie zaczyna się w punkcie początkowym");
            if (answer.path[answer.path.Length - 1] != answer.t)
                return (Result.WrongResult, $"{goodPathLength} ścieżka nie kończy się w zwróconym punkcie końcowym {answer.t}");

            int pathLength = 0;
            for (int i = 0; i < answer.path.Length - 1; ++i)
            {
                if (!G.HasEdge(answer.path[i], answer.path[i + 1]))
                    return (Result.WrongResult, $"{goodPathLength} między sąsiednimi wierzchołkami na ścieżce ({answer.path[i]}, {answer.path[i + 1]}) nie ma krawędzi");
                pathLength += G.GetEdgeWeight(answer.path[i], answer.path[i + 1]);
                if (i + 1 != answer.path.Length - 1)
                    pathLength += waitTime[answer.path[i + 1]];
            }
            if (pathLength != expectedLength)
                return (Result.WrongResult, $"{goodPathLength} czas przejścia zwróconą ścieżką ({pathLength}) różni się od oczekiwanego ({expectedLength})");

            return OkResult("OK");
        }

        public (Result resultCode, string message) OkResult(string message) =>
            (TimeLimit < PerformanceTime ? Result.LowEfficiency : Result.Success,
            $"{message} {PerformanceTime.ToString("#0.00")}s");
    }

    public class Lab06Stage2TestCase : TestCase
    {
        protected readonly DiGraph<int> G;
        protected readonly Graph<int> C;
        protected readonly int[] waitTime;
        protected readonly int s, t;
        protected readonly string description;
        protected readonly int? expectedLength, expectedCost;
        protected (int l, int c, int[] path)? answer;

        public Lab06Stage2TestCase(DiGraph<int> G, Graph<int> C, int[] waitTime, int s, int t, int? expectedLength, int? expectedCost, int timeLimit, string description) : base(timeLimit, null, description)
        {
            this.description = description;
            this.G = G;
            this.C = C;
            this.waitTime = waitTime;
            this.s = s;
            this.t = t;
            if ((expectedLength == null && expectedCost != null) || (expectedLength != null && expectedCost == null))
                throw new ArgumentException();
            this.expectedLength = expectedLength;
            this.expectedCost = expectedCost;
        }

        protected override void PerformTestCase(object prototypeObject)
        {
            answer = ((Lab06)prototypeObject).Stage2(G, C, waitTime, s, t);
        }

        protected override (Result resultCode, string message) VerifyTestCase(object settings)
        {
            (var code, var message) = _VerifyTestCase(settings);
            return (code, $"{message} [{Description}]");
        }

        protected (Result resultCode, string message) _VerifyTestCase(object settings)
        {
            string goodPathLength = "Długość i koszt trasy OK, ale";
            if (answer == null && expectedLength != null)
                return (Result.WrongResult, $"Zwrócono brak trasy, podczas gdy trasa istnieje");
            if (answer != null && expectedLength == null)
                return (Result.WrongResult, $"Zwrócono jakąś trasę, podczas gdy trasa nie istnieje");
            if (answer == null && expectedLength == null)
                return OkResult("OK");
            if (answer!.Value.c != expectedCost)
                return (Result.WrongResult, $"Zwrócony koszt wycieczki ({answer!.Value.c}) jest inny od oczekiwanego ({expectedCost})");
            if (answer!.Value.l != expectedLength)
                return (Result.WrongResult, $"Koszt OK, ale zwrócona długość wycieczki ({answer!.Value.l}) jest inna od oczekiwanej ({expectedLength})");
            if (answer!.Value.path == null || answer!.Value.path.Length == 0)
                return (Result.WrongResult, $"{goodPathLength} zwrócona ścieżka jest pusta");

            var path = answer!.Value.path;
            if (path[0] != s)
                return (Result.WrongResult, $"{goodPathLength} ścieżka nie zaczyna się w punkcie początkowym");
            if (path[path.Length - 1] != t)
                return (Result.WrongResult, $"{goodPathLength} ścieżka nie kończy się w zwróconym punkcie końcowym {t}");

            int pathLength = 0;
            int pathCost = 0;
            for (int i = 0; i < path.Length - 1; ++i)
            {
                if (!G.HasEdge(path[i], path[i + 1]))
                    return (Result.WrongResult, $"{goodPathLength} między sąsiednimi wierzchołkami na ścieżce ({path[i]}, {path[i + 1]}) nie ma krawędzi");
                pathLength += G.GetEdgeWeight(path[i], path[i + 1]);
                if (i + 1 != path.Length - 1)
                    pathLength += waitTime[path[i + 1]];
                pathCost += C.GetEdgeWeight(path[i], path[i + 1]);
            }
            if (pathCost != expectedCost)
                return (Result.WrongResult, $"{goodPathLength} koszt przejścia zwróconą ścieżką ({pathCost}) różni się od oczekiwanego ({expectedCost})");
            if (pathLength != expectedLength)
                return (Result.WrongResult, $"{goodPathLength} czas przejścia zwróconą ścieżką ({pathLength}) różni się od oczekiwanego ({expectedLength})");

            return OkResult("OK");
        }

        public (Result resultCode, string message) OkResult(string message) =>
            (TimeLimit < PerformanceTime ? Result.LowEfficiency : Result.Success,
            $"{message} {PerformanceTime.ToString("#0.00")}s");
    }


    public class Lab06Tests : TestModule
    {
        TestSet Stage1 = new TestSet(prototypeObject: new Lab06(), description: "Etap I", settings: true);
        TestSet Stage2 = new TestSet(prototypeObject: new Lab06(), description: "Etap II", settings: true);

        public override void PrepareTestSets()
        {
            TestSets["Etap I"] = Stage1;
            TestSets["Etap II"] = Stage2;

            PrepareTests();
        }

        void PrepareTests()
        {
            {
                DiGraph<int> G = new DiGraph<int>(6);
                Graph<int> C = new Graph<int>(6);
                int[] waitTime = new int[] { 10, 40, 5, 10, 30, 30 };
                int s = 0;
                int t = 4;
                AddUndirectedEdge(G, C, 0, 1, 125, 5);
                AddUndirectedEdge(G, C, 0, 2, 75, 0);
                AddUndirectedEdge(G, C, 1, 3, 25, 5);
                AddUndirectedEdge(G, C, 3, 4, 15, 0);
                AddUndirectedEdge(G, C, 2, 3, 100, 20);
                AddUndirectedEdge(G, C, 2, 5, 120, 10);
                AddUndirectedEdge(G, C, 3, 5, 125, 0);

                Stage1.TestCases.Add(new Lab06Stage1TestCase(G, waitTime, s, 205, 1, "Przykład z treści zadania"));
                Stage2.TestCases.Add(new Lab06Stage2TestCase(G, C, waitTime, s, t, 215, 10, 2, "Przykład z treści zadania"));
            }
            {
                DiGraph<int> G = new DiGraph<int>(4);
                Graph<int> C = new Graph<int>(4);
                int[] waitTime = new int[] { 5, 4, 3, 2 };
                int s = 0;

                AddDirectedEdge(G, C, 0, 1, 0, 1);
                AddDirectedEdge(G, C, 0, 2, 1, 0);
                AddDirectedEdge(G, C, 0, 3, 2, 0);

                Stage1.TestCases.Add(new Lab06Stage1TestCase(G, waitTime, s, 2, 1, "Skierowana gwiazda 1"));
            }
            {
                DiGraph<int> G = new DiGraph<int>(4);
                Graph<int> C = new Graph<int>(4);
                int[] waitTime = new int[] { 5, 4, 3, 2 };
                int s = 0;

                AddDirectedEdge(G, C, 1, 0, 0, 1);
                AddDirectedEdge(G, C, 2, 0, 1, 0);
                AddDirectedEdge(G, C, 3, 0, 2, 0);

                Stage1.TestCases.Add(new Lab06Stage1TestCase(G, waitTime, s, 0, 1, "Skierowana gwiazda 2"));
            }
            {
                DiGraph<int> G = new DiGraph<int>(100);
                Graph<int> C = new Graph<int>(100);
                int[] waitTime = new int[100];
                for (int i = 0; i < 100; ++i)
                    waitTime[i] = 100;
                int s = 0;
                int t1 = 0, t2 = 99;

                Stage1.TestCases.Add(new Lab06Stage1TestCase(G, waitTime, s, 0, 1, "Graf pusty"));
                Stage2.TestCases.Add(new Lab06Stage2TestCase(G, C, waitTime, s, t1, 0, 0, 1, "Graf pusty, istnieje jednowierzchołkowa ścieżka"));
                Stage2.TestCases.Add(new Lab06Stage2TestCase(G, C, waitTime, s, t2, null, null, 1, "Graf pusty, ścieżka nie istnieje"));
            }
            {
                DiGraph<int> G = new DiGraph<int>(10);
                Graph<int> C = new Graph<int>(10);
                int[] waitTime = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
                int s = 0, t = 9;
                for (int i = 0; i < 10; ++i)
                    AddDirectedEdge(G, C, i, (i + 1) % 10, 10 - i, 1);

                Stage1.TestCases.Add(new Lab06Stage1TestCase(G, waitTime, s, 98, 1, "Cykl skierowany"));
                Stage2.TestCases.Add(new Lab06Stage2TestCase(G, C, waitTime, s, t, 98, 9, 1, "Cykl skierowany"));
            }
            {
                DiGraph<int> G = new DiGraph<int>(10);
                Graph<int> C = new Graph<int>(10);
                int[] waitTime = new int[10];
                int s = 0, t = 9;
                for (int i = 0; i < 9; ++i)
                    AddUndirectedEdge(G, C, i, i + 1, 1, 1);
                AddUndirectedEdge(G, C, 9, 0, 100000, 8);

                Stage1.TestCases.Add(new Lab06Stage1TestCase(G, waitTime, s, 9, 1, "Cykl z dużym czasem przejścia na jednej krawędzi"));
                Stage2.TestCases.Add(new Lab06Stage2TestCase(G, C, waitTime, s, t, 100000, 8, 1, "Cykl z dużym czasem przejścia na taniej krawędzi"));

                var C2 = (Graph<int>)C.Clone();
                C2.SetEdgeWeight(9, 0, 9);
                Stage2.TestCases.Add(new Lab06Stage2TestCase(G, C2, waitTime, s, t, 9, 9, 1, "Cykl, dwie ścieżki o równym koszcie"));
            }
            {
                DiGraph<int> G = new DiGraph<int>(500);
                Graph<int> C = new Graph<int>(500);
                int s = 0, t = 499;

                Random random = new Random(12345);
                int[] waitTime = new int[500];
                for (int i = 0; i < 500; ++i)
                    waitTime[i] = random.Next(500000);

                for (int i = 0; i < 499; ++i)
                    AddUndirectedEdge(G, C, i, i + 1, random.Next(500000), random.Next(500000));

                Stage1.TestCases.Add(new Lab06Stage1TestCase(G, waitTime, s, 247650084, 2, "Ścieżka nieskierowana o dużych wagach"));
                Stage2.TestCases.Add(new Lab06Stage2TestCase(G, C, waitTime, s, t, 247650084, 123462357, 2, "Ścieżka nieskierowana o dużych wagach"));
            }
            {
                int n = 5000, m = 2 * n;
                (var G, var C, var waitTime) = GenerateRandomExample(n, m, 1, 200, 0, 50, 1111);
                int s = 0, t = 4004;
                Stage1.TestCases.Add(new Lab06Stage1TestCase(G, waitTime, s, 4018, 3, $"Graf losowy n={n}"));
                Stage2.TestCases.Add(new Lab06Stage2TestCase(G, C, waitTime, s, t, null, null, 6, $"Graf losowy n={n}"));
            }
            {
                int n = 10000, m = 4 * n;
                (var G, var C, var waitTime) = GenerateRandomExample(n, m, 1, 200, 0, 50, 2137);
                int s = 0, t = 999;
                Stage1.TestCases.Add(new Lab06Stage1TestCase(G, waitTime, s, 1940, 5, $"Graf losowy n={n}"));
                Stage2.TestCases.Add(new Lab06Stage2TestCase(G, C, waitTime, s, t, 2739, 115, 10, $"Graf losowy n={n}"));
            }
        }

        void AddDirectedEdge(DiGraph<int> G, Graph<int> C, int from, int to, int weight, int cost)
        {
            G.AddEdge(from, to, weight);
            C.AddEdge(from, to, cost);
        }

        void AddUndirectedEdge(DiGraph<int> G, Graph<int> C, int from, int to, int weight, int cost)
        {
            G.AddEdge(from, to, weight);
            G.AddEdge(to, from, weight);
            C.AddEdge(from, to, cost);
        }

        (DiGraph<int> G, Graph<int> C, int[] waitTime) GenerateRandomExample(int n, int m, int minTime, int maxTime, int minCost, int maxCost, int seed, bool directed = false)
        {
            DiGraph<int> G = new DiGraph<int>(n);
            Graph<int> C = new Graph<int>(n);
            int[] waitTime = new int[n];

            Random random = new Random(seed);

            for (int i = 0; i < n; ++i)
                waitTime[i] = random.Next(minTime, maxTime);

            for (int i = 0; i < m; ++i)
            {
                int from = random.Next(n), to = random.Next(n);
                while (from == to)
                    to = random.Next(n);
                if (directed)
                    AddDirectedEdge(G, C, from, to, random.Next(minTime, maxTime), random.Next(minCost, maxCost));
                else
                    AddDirectedEdge(G, C, from, to, random.Next(minTime, maxTime), random.Next(minCost, maxCost));
            }

            return (G, C, waitTime);
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
