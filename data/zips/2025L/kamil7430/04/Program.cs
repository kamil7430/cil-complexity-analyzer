using System;
using System.Linq;
using System.Text;
using ASD;
using System.Collections.Generic;
using ASD.Graphs;

namespace ASD
{
    public abstract class Lab04TestCase : TestCase
    {
        protected readonly int stage;
        protected readonly Graph g;
        protected readonly int n;
        protected readonly int m;
        protected readonly int K;
        protected readonly string description;
        protected readonly int expectedNumberOfInfectedServices;
        protected readonly int[] expectedListOfInfectedServices;
        protected (int numberOfInfectedServices, int[] infectedServices) result;

        protected Lab04TestCase(
            Graph g,
            int n,
            int m,
            int K,
            int expectedNumberOfInfectedServices,
            int[] expectedListOfInfectedServices,
            int timeLimit,
            string description)
            : base(timeLimit, null, description)
        {
            this.description = description;
            this.g = g;
            this.n = n;
            this.m = m;
            this.K = K;
            this.expectedNumberOfInfectedServices = expectedNumberOfInfectedServices;
            this.expectedListOfInfectedServices = expectedListOfInfectedServices;
            Array.Sort(this.expectedListOfInfectedServices);
        }

        protected abstract int getStage();

        protected Exception getException(string exceptionMessage)
        {
            return new ArgumentException($"[Stage {getStage()}][{description}] {exceptionMessage}");
        }

        protected override (Result resultCode, string message) VerifyTestCase(object settings)
        {
            var (code, msg) = CheckDeclaredSolution();
            return (code, $"{msg} [{description}]");
        }

        protected (Result resultCode, string message) CheckDeclaredSolution()
        {
            if (result.numberOfInfectedServices != expectedNumberOfInfectedServices)
            {
                return (Result.WrongResult, $"Zwrócono niepoprawną ilość zainfekowanych serwisów: zwrócono {result.numberOfInfectedServices}, oczekiwano {expectedNumberOfInfectedServices}");
            }
            if (result.infectedServices == null)
            {
                return (Result.WrongResult, "Zwrócono null zamiast listy zainfekowanych serwisów");
            }
            if (result.infectedServices.Length != expectedNumberOfInfectedServices)
            {
                return (Result.WrongResult, $"Długość listy zainfekowanych serwisów ({result.infectedServices.Length}) nie zgadza się ze zwróconą liczbą takowych serwisów ({result.numberOfInfectedServices})");
            }
            if (result.infectedServices.Distinct().ToArray().Length != result.infectedServices.Length)
            {
                return (Result.WrongResult, "Powtórzenia w liście zainfekowanych serwisów");
            }
            Array.Sort(result.infectedServices);
            if (!result.infectedServices.SequenceEqual(expectedListOfInfectedServices))
            {
                return (Result.WrongResult, "Niepoprawna lista zainfekowanych serwisów");
            }
            var res = OkResult("OK");
            if (res.resultCode == Result.Success || res.resultCode == Result.LowEfficiency)
            {
                if (PerformanceTime > TimeLimit)
                {
                    return (Result.WrongResult, $"Podany wynik jest poprawny ale przekroczono limit czasowy {TimeLimit.ToString("#0.00")}s (Twoj czas: {PerformanceTime.ToString("#0.00")}s)");
                }
            }
            return res;
        }

        public (Result resultCode, string message) OkResult(string message) =>
            (TimeLimit < PerformanceTime ? Result.LowEfficiency : Result.Success,
            $"{message} {PerformanceTime.ToString("#0.00")}s");
    }

    public class Stage1TestCase : Lab04TestCase
    {
        protected readonly int s;
        public Stage1TestCase(
            int s,
            Graph G,
            int n,
            int m,
            int K,
            int expectedNumberOfInfectedServices,
            int[] expectedListOfInfectedServices,
            int timeLimit,
            string description)
            : base(G, n, m, K, expectedNumberOfInfectedServices, expectedListOfInfectedServices, timeLimit, description)
        {
            this.s = s;
            if (expectedListOfInfectedServices.Length != expectedNumberOfInfectedServices)
            {
                throw getException("expectedListOfInfectedServices.Length != expectedNumberOfInfectedServices");
            }
        }

        protected override int getStage()
        {
            return 1;
        }

        protected override void PerformTestCase(object prototypeObject)
        {
            result = ((Lab04)prototypeObject).Stage1((Graph)g.Clone(), K, s);
        }
    }

    public class Stage2TestCase : Lab04TestCase
    {
        private readonly int[] s;
        private readonly int[] serviceTurnoffDay;
        public Stage2TestCase(
            int[] s,
            int[] serviceTurnoffDay,
            Graph G,
            int n,
            int m,
            int K,
            int expectedNumberOfInfectedServices,
            int[] expectedListOfInfectedServices,
            int timeLimit,
            string description)
            : base(G, n, m, K, expectedNumberOfInfectedServices, expectedListOfInfectedServices, timeLimit, description)
        {
            this.s = s;
            this.serviceTurnoffDay = serviceTurnoffDay;
            if (expectedListOfInfectedServices.Length != expectedNumberOfInfectedServices)
            {
                throw getException("expectedListOfInfectedServices.Length != expectedNumberOfInfectedServices");
            }
            if (serviceTurnoffDay.Length != n)
            {
                throw getException("serviceTurnoffDay.Length != n");
            }
            for (int i = 0; i < n; i++)
            {
                if (serviceTurnoffDay[i] < 0 || serviceTurnoffDay[i] > K + 1)
                {
                    throw getException($"serviceTurnoffDay[i] < 0 || serviceTurnoffDay[i] > K + 1, [i={i}]");
                }
            }
        }

        protected override int getStage()
        {
            return 2;
        }

        protected override void PerformTestCase(object prototypeObject)
        {
            result = ((Lab04)prototypeObject).Stage2((Graph)g.Clone(), K, (int[])s.Clone(), (int[])serviceTurnoffDay.Clone());
        }
    }

    public class Stage3TestCase : Lab04TestCase
    {
        private readonly int[] s;
        private readonly int[] serviceTurnoffDay;
        private readonly int[] serviceTurnonDay;
        public Stage3TestCase(
            int[] s,
            int[] serviceTurnoffDay,
            int[] serviceTurnonDay,
            Graph G,
            int n,
            int m,
            int K,
            int expectedNumberOfInfectedServices,
            int[] expectedListOfInfectedServices,
            int timeLimit,
            string description)
            : base(G, n, m, K, expectedNumberOfInfectedServices, expectedListOfInfectedServices, timeLimit, description)
        {
            this.s = s;
            this.serviceTurnoffDay = serviceTurnoffDay;
            this.serviceTurnonDay = serviceTurnonDay;
            if (expectedListOfInfectedServices.Length != expectedNumberOfInfectedServices)
            {
                throw getException($"expectedListOfInfectedServices.Length != expectedNumberOfInfectedServices");
            }
            if (serviceTurnoffDay.Length != n)
            {
                throw getException($"serviceTurnoffDay.Length != n");
            }
            if (serviceTurnonDay.Length != n)
            {
                throw getException($"serviceTurnonDay.Length != n");
            }
            for (int i = 0; i < n; i++)
            {
                if (serviceTurnoffDay[i] < 0 || serviceTurnoffDay[i] > K + 1)
                {
                    throw getException($"serviceTurnoffDay[i] < 0 || serviceTurnoffDay[i] > K + 1, [i={i}]");
                }
                if (serviceTurnoffDay[i] > serviceTurnonDay[i])
                {
                    throw getException($"serviceTurnoffDay[i] > serviceTurnonDay[i], [i={i}]");
                }

            }
        }

        protected override int getStage()
        {
            return 3;
        }

        protected override void PerformTestCase(object prototypeObject)
        {
            result = ((Lab04)prototypeObject).Stage3((Graph)g.Clone(), K, (int[])s.Clone(), (int[])serviceTurnoffDay.Clone(), (int[])serviceTurnonDay.Clone());
        }
    }

    public class Lab04Tests : TestModule
    {
        TestSet Stage1 = new TestSet(prototypeObject: new Lab04(), description: "Etap 1", settings: true);
        TestSet Stage2 = new TestSet(prototypeObject: new Lab04(), description: "Etap 2", settings: true);
        TestSet Stage3 = new TestSet(prototypeObject: new Lab04(), description: "Etap 3", settings: true);

        TestSet Stage1Home = new TestSet(prototypeObject: new Lab04(), description: "Etap 1", settings: true);
        TestSet Stage2Home = new TestSet(prototypeObject: new Lab04(), description: "Etap 2", settings: true);
        TestSet Stage3Home = new TestSet(prototypeObject: new Lab04(), description: "Etap 3", settings: true);


        public override void PrepareTestSets()
        {
            TestSets["Stage1"] = Stage1;
            TestSets["Stage2"] = Stage2;
            TestSets["Stage3"] = Stage3;
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

        private void addStage3(Stage3TestCase testCase)
        {
            Stage3.TestCases.Add(testCase);
        }

        private void Prepare()
        {
            test0();
            test1_0();
            test1_1();
            test1();
            test2();
            test3();
            test4();
            test5();
            test6();
        }

        private void test0()
        {
            Graph g = new Graph(1);
            int K = 1;
            addStage1(new Stage1TestCase(0, g, 1, g.EdgeCount, K, 1, new int[] { 0 }, 1, "Test zerowy"));
            addStage2(new Stage2TestCase(new int[] { 0 }, new int[] { K+1 }, g, 1, g.EdgeCount, K, 1, new int[] { 0 }, 1, "Test zerowy"));
        }

        private void test1_0()
        {
            int K = 2;
            int n = 3;
            Graph g = new Graph(n);
            g.AddEdge(0, 1);
            g.AddEdge(1, 2);
            addStage2(new Stage2TestCase(new int[] { 0 }, new int[] { K + 1, 2, K + 1 }, g, n, g.EdgeCount, K, 1, new int[] { 0 }, 1, "Test linia v1"));
            addStage2(new Stage2TestCase(new int[] { 1 }, new int[] { 1, 1, 1 }, g, n, g.EdgeCount, K, 1, new int[] { 1 }, 1, "Test linia v2"));
            addStage2(new Stage2TestCase(new int[] { 0, 1, 2 }, new int[] { 1, 1, 1 }, g, n, g.EdgeCount, K, 3, new int[] { 0, 1, 2 }, 1, "Test linia v3"));
            addStage2(new Stage2TestCase(new int[] { 0 }, new int[] { 2, K + 1, K + 1 }, g, n, g.EdgeCount, K, 1, new int[] { 0 }, 1, "Test AdamChoj"));
			K = 1;
            addStage3(new Stage3TestCase(new int[] { 0 }, new int[] { 1, K + 1, K + 1 }, new int[] { 1, K + 1, K + 1 }, g, n, g.EdgeCount, K, 1, new int[] { 0 }, 1, "Test linia v1"));
            K = 2;
            addStage3(new Stage3TestCase(new int[] { 0 }, new int[] { 1, K + 1, K + 1 }, new int[] { 1, K + 1, K + 1 }, g, n, g.EdgeCount, K, 2, new int[] { 0, 1 }, 1, "Test linia v2"));
            addStage3(new Stage3TestCase(new int[] { 0 }, new int[] { 2, K + 1, K + 1 }, new int[] { 2, K + 1, K + 1 }, g, n, g.EdgeCount, K, 1, new int[] { 0 }, 1, "Test linia v3"));
            K = 3;
            addStage3(new Stage3TestCase(new int[] { 0 }, new int[] { K + 1, 1, K + 1 }, new int[] { K + 1, 2, K + 1 }, g, n, g.EdgeCount, K, 2, new int[] { 0, 1 }, 1, "Test linia v4"));
            addStage3(new Stage3TestCase(new int[] { 0, 2 }, new int[] { 1, 1, K + 1 }, new int[] { 1, 2, K + 1 }, g, n, g.EdgeCount, K, 3, new int[] { 0, 1, 2 }, 1, "Test linia v5"));
        }

        private void test1_1()
        {
            int K;
            int n = 4;
            Graph g = new Graph(n);
            g.AddEdge(0, 1);
            g.AddEdge(1, 2);
            g.AddEdge(2, 3);
            K = 3;
            addStage3(new Stage3TestCase(new int[] { 0 }, new int[] { 1, 1, K + 1, K + 1 }, new int[] { 1, 2, K + 1, K + 1 }, g, n, g.EdgeCount, K, 2, new int[] { 0, 1 }, 1, "Test propagacji v1"));
            K = 4;
            addStage3(new Stage3TestCase(new int[] { 0 }, new int[] { 1, 1, K + 1, K + 1 }, new int[] { 1, 2, K + 1, K + 1 }, g, n, g.EdgeCount, K, 3, new int[] { 0, 1, 2 }, 1, "Test propagacji v2"));
            K = 5;
            addStage3(new Stage3TestCase(new int[] { 0 }, new int[] { 1, 1, K + 1, K + 1 }, new int[] { 1, 2, K + 1, K + 1 }, g, n, g.EdgeCount, K, 4, new int[] { 0, 1, 2, 3 }, 1, "Test propagacji v3"));

            K = 3;
            addStage3(new Stage3TestCase(new int[] { 0 }, new int[] { 1, 2, K + 1, K + 1 }, new int[] { 1, 2, K + 1, K + 1 }, g, n, g.EdgeCount, K, 2, new int[] { 0, 1 }, 1, "Test propagacji v4"));
            K = 4;
            addStage3(new Stage3TestCase(new int[] { 0 }, new int[] { 1, 2, K + 1, K + 1 }, new int[] { 1, 2, K + 1, K + 1 }, g, n, g.EdgeCount, K, 3, new int[] { 0, 1, 2 }, 1, "Test propagacji v5"));
        }

        private void test1()
        {
            int n = 7;
            Graph g = new Graph(n);
            g.AddEdge(0, 1);
            g.AddEdge(1, 2);
            g.AddEdge(2, 3);
            g.AddEdge(3, 0);
            g.AddEdge(0, 4);
            g.AddEdge(4, 5);
            g.AddEdge(5, 6);
            int K = 10;
            int z = K + 1;
            addStage1(new Stage1TestCase(0, g, n, g.EdgeCount, 3, 6, new int[] { 1, 2, 3, 4, 5, 0 }, 1, "Test z treści zadania"));
            addStage2(new Stage2TestCase(new int[] { 0 }, new int[] { z, z, 3, z, z, 3, z }, g, n, g.EdgeCount, 10, 4, new int[] { 0, 1, 3, 4 }, 1, "Graf jak w treści zadania"));
            K = 4;
            z = K + 1;
            addStage3(new Stage3TestCase(new int[] { 0 }, new int[] { z, z, z, z, 1, z, z }, new int[] { z, z, z, z, 3, z, z }, g, n, g.EdgeCount, 4, 5, new int[] { 0, 1, 2, 3, 4 }, 1, "Graf jak w treści zadania v1"));
        }

        private void test2()
        {
            int n = 5;
            int K = 300000;
            Graph g = new Graph(n);
            g.AddEdge(0, 1);
            g.AddEdge(1, 2);
            g.AddEdge(2, 3);
            g.AddEdge(3, 4);
            g.AddEdge(4, 0);
            addStage1(new Stage1TestCase(3, g, n, g.EdgeCount, K, 5, new int[] { 1, 2, 3, 4, 0 }, 1, "Test duże K"));
            addStage2(new Stage2TestCase(new int[] { 3 }, Enumerable.Range(0, n).Select(i => K + 1).ToArray(), g, n, g.EdgeCount, K, 5, new int[] { 1, 2, 3, 4, 0 }, 1, "Test duże K"));
        }

        private void test3()
        {
            int n = 10000;
            Graph g = new Graph(n);
            for (int i = 1; i < n - 1; i++)
            {
                g.AddEdge(i, i + 1);
                g.AddEdge(i, n - i - 1);
            }
            addStage1(new Stage1TestCase(0, g, n, g.EdgeCount, n, 1, new int[] { 0 }, 2, "Test duże n i K"));
            addStage2(new Stage2TestCase(Enumerable.Range(0, n).ToArray(), Enumerable.Repeat(1, n).ToArray(), g, n, g.EdgeCount, n, n, Enumerable.Range(0, n).ToArray(), 2, "Test duże n i K"));
            g = new Graph(n);
            for (int i = 0; i < n - 1; i++)
            {
                g.AddEdge(i, i + 1);
                g.AddEdge(i, n - i - 1);
            }
            addStage3(new Stage3TestCase(new int[] { 0 }, Enumerable.Range(1, n).ToArray(), Enumerable.Range(2, n).ToArray(), g, n, g.EdgeCount, n, n, Enumerable.Range(0, n).ToArray(), 2, "Test duże n i K"));
        }

        private void test4()
        {
            int n = 50;
            Graph g = new Graph(n);
            for (int i = 0; i < n; i++)
            {
                for (int j = i + 1; j < n; j++)
                {
                    g.AddEdge(i, j);
                }
            }
            addStage1(new Stage1TestCase(25, g, n, g.EdgeCount, 1, 1, new int[] { 25 }, 10, "Test klika K50"));
            addStage1(new Stage1TestCase(25, g, n, g.EdgeCount, 2, 50, Enumerable.Range(0, 50).ToArray(), 10, "Test klika K50 v2"));
        }

        private void test5()
        {
            int n = 25;
            int K = 4;
            Graph g = new Graph(n);
            g.AddEdge(0, 1);
            g.AddEdge(1, 6);
            g.AddEdge(6, 14);
            g.AddEdge(14, 15);
            g.AddEdge(1, 5);
            g.AddEdge(5, 13);
            g.AddEdge(13, 15);
            g.AddEdge(0, 2);
            g.AddEdge(2, 7);
            g.AddEdge(7, 16);
            g.AddEdge(16, 18);
            g.AddEdge(2, 8);
            g.AddEdge(8, 17);
            g.AddEdge(17, 18);
            g.AddEdge(0, 4);
            g.AddEdge(4, 9);
            g.AddEdge(9, 19);
            g.AddEdge(19, 21);
            g.AddEdge(4, 10);
            g.AddEdge(10, 20);
            g.AddEdge(20, 21);
            g.AddEdge(0, 3);
            g.AddEdge(3, 11);
            g.AddEdge(11, 22);
            g.AddEdge(22, 24);
            g.AddEdge(3, 12);
            g.AddEdge(12, 23);
            g.AddEdge(23, 24);
            addStage1(new Stage1TestCase(0, g, n, g.EdgeCount, K, 21, Enumerable.Range(0, 25)
                            .Where(x => x != 15 && x != 18 && x != 21 && x != 24).ToArray(), 10, "Test wiatrak"));
            addStage2(new Stage2TestCase(new int[] { 0 }, Enumerable.Range(0, n).Select(x => x == 3 || x == 4 || x == 1 ? 2 : K + 1).ToArray(), g, n, g.EdgeCount, 4, 6, new int[] { 0, 2, 7, 8, 16, 17 }, 10, "Test wiatrak"));
        }

        void test6()
        {
            Graph h = new Graph(2);
            h.AddEdge(0, 1);
            addStage3(new Stage3TestCase(new int[] { 0 }, new int[] { 4, 1 }, new int[] { 10, 7 }, h, 2, 1, 9, 1, new int[] { 0 }, 1, "Test szefa"));
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var tests = new Lab04Tests();
            tests.PrepareTestSets();
            foreach (var ts in tests.TestSets)
            {
                ts.Value.PerformTests(verbose: true, checkTimeLimit: false);
            }
        }
    }
}