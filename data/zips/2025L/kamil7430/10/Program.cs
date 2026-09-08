namespace ASD
{
    using System;
    using ASD.Graphs;
    using ASD.Graphs.Testing;

    public class Lab10Stage1TestCase : TestCase
    {
        Graph G;
        int[] color;
        int expectedLength;
        int[] answer;
        public Lab10Stage1TestCase(Graph G, int[] color, int expectedLength, double timeLimit, string description) : base(timeLimit, null, description)
        {
            this.G = (Graph)G.Clone();
            this.color = (int[])color.Clone();
            this.expectedLength = expectedLength;
        }

        protected override void PerformTestCase(object prototypeObject)
        {
            answer = ((Lab10)prototypeObject).FindLongestRepetition((Graph)G.Clone(), (int[])color.Clone());
            //if (answer != null)
            //    Console.WriteLine(answer.Length);
        }

        protected override (Result resultCode, string message) VerifyTestCase(object settings)
        {
            (Result resultCode, string message) tempFunction()
            {
                if ((answer == null) && expectedLength > 0)
                    return (Result.WrongResult, "Nie znaleziono powtórzenia");
                if (answer != null)
                {
                    if (answer.Length % 2 != 0)
                        return (Result.WrongResult, "Zwrócono tablicę nieparzystej długości");
                    if (answer.Length > G.VertexCount)
                        return (Result.WrongResult, "Zwrócono tablicę dłuższą niż liczba wierzchołków grafu");
                    int l = answer.Length / 2;
                    for (int i = 0; i < l; i++)
                        if (color[answer[i]] != color[answer[i + l]])
                            return (Result.WrongResult, "Zwrócona tablica nie zawiera powtórzenia");
                    bool[] used = new bool[G.VertexCount];
                    if (answer.Length > 0)
                        used[answer[0]] = true;
                    for (int i = 1; i < answer.Length; i++)
                    {
                        if (!G.HasEdge(answer[i - 1], answer[i]))
                            return (Result.WrongResult, "Zwrócona ścieżka przechodzi po nieistniejącej krawędzi");
                        if (used[answer[i]])
                            return (Result.WrongResult, "Zwrócona ścieżka odwiedza ten sam wierzchołek więcej niż raz");
                        used[answer[i]] = true;
                    }

                    if (answer.Length < expectedLength)
                        return (Result.WrongResult, "Zwrócono za krótkie powtórzenie");
                }
                if (answer != null && expectedLength < answer.Length)
                    return (Result.WrongResult, $"Błąd w testach (znaleziono powtórzenie długości {answer.Length}, którego nie powinno być), proszę o kontakt z koordynatorem przedmiotu");

                return (Result.Success, "OK");
            }

            (Result resultCode, string message) = tempFunction();
            message += $" ({base.Description})";
            return (resultCode, message);
        }
    }

    public class Lab10Tests : TestModule
    {
        TestSet Stage1small = new TestSet(prototypeObject: new Lab10(), description: "Etap I, małe testy", settings: true);
        TestSet Stage1medium = new TestSet(prototypeObject: new Lab10(), description: "Etap II, średnie testy", settings: true);
        TestSet Stage1large = new TestSet(prototypeObject: new Lab10(), description: "Etap III, duże testy", settings: true);

        public override void PrepareTestSets()
        {
            TestSets["Etap I małe"] = Stage1small;
            TestSets["Etap II średnie"] = Stage1medium;
            TestSets["Etap III duże"] = Stage1large;
            PrepareSmallTests();
            PrepareMediumTests();
            PrepareLargeTests();
        }

        void PrepareSmallTests()
        {

            Graph G2 = new Graph(1);
            int[] c1 = { 1 };
            Stage1small.TestCases.Add(new Lab10Stage1TestCase(G2, c1, 0, 1, "Pojedynczy wierzcholek"));

            Graph Gse = new Graph(2);
            Gse.AddEdge(0, 1);
            int[] cse1 = { 1, 2 };
            int[] cse2 = { 1, 1 };
            Stage1small.TestCases.Add(new Lab10Stage1TestCase(Gse, cse1, 0, 1, "Krawedz bez powtorzenia"));
            Stage1small.TestCases.Add(new Lab10Stage1TestCase(Gse, cse2, 2, 1, "Krawedz z powtorzeniem"));

            Graph G = new Graph(10);
            for (int i = 0; i < 9; i++)
                G.AddEdge(i, i + 1);
            int[] col1 = { 1, 2, 3, 2, 1, 3, 2, 3, 1, 2 };
            int[] col2 = { 2, 3, 2, 1, 3, 2, 1, 2, 3, 1 };
            Stage1small.TestCases.Add(new Lab10Stage1TestCase(G, col1, 0, 1, "Sciezka bez powtorzen"));
            Stage1small.TestCases.Add(new Lab10Stage1TestCase(G, col2, 6, 1, "Sciezka z powtorzeniem"));


            int[] col3 = { 1, 2, 3, 4, 1, 5, 4, 3, 2, 5 };
            for (int i = 0; i < 8; i++)
                G.AddEdge(i, i + 2);
            Stage1small.TestCases.Add(new Lab10Stage1TestCase(G, col3, 0, 1, "Kwadrat sciezki bez powtorzen"));

            int[] col4 = { 1, 2, 3, 4, 8, 1, 9, 2, 3, 4 };
            Stage1small.TestCases.Add(new Lab10Stage1TestCase(G, col4, 8, 1, "Kwadrat sciezki z powtorzeniem"));

            Graph GP = new Graph(10);
            GP.AddEdge(0, 1);
            GP.AddEdge(0, 2);
            GP.AddEdge(0, 4);
            GP.AddEdge(1, 3);
            GP.AddEdge(1, 7);
            GP.AddEdge(2, 6);
            GP.AddEdge(2, 8);
            GP.AddEdge(3, 5);
            GP.AddEdge(3, 8);
            GP.AddEdge(4, 5);
            GP.AddEdge(4, 9);
            GP.AddEdge(5, 6);
            GP.AddEdge(6, 7);
            GP.AddEdge(7, 9);
            GP.AddEdge(8, 9);
            int[] colp = { 3, 2, 4, 3, 2, 1, 5, 1, 6, 4 };
            int[] colnp = { 3, 2, 4, 3, 6, 7, 6, 1, 5, 4 };
            Stage1small.TestCases.Add(new Lab10Stage1TestCase(GP, colp, 6, 1, "Petersen z powtorzeniem"));
            Stage1small.TestCases.Add(new Lab10Stage1TestCase(GP, colnp, 0, 1, "Petersen bez powtorzenia"));


            RandomGraphGenerator rgg = new RandomGraphGenerator(10);
            Random rand = new Random(10);

            int[] wyniks = { 6, 4, 2, 0, 2, 0, 2, 2, 2 };
            for (int i = 0; i < 5; i++)
            {
                Graph Gr = rgg.Graph(18, 0.2);
                int[] col = new int[Gr.VertexCount];
                for (int j = 0; j < Gr.VertexCount; j++)
                    col[j] = rand.Next(23);
                Stage1small.TestCases.Add(new Lab10Stage1TestCase(Gr, col, wyniks[i], 1, "Maly test losowy " + i.ToString()));
            }
            for (int i = 5; i < 9; i++)
            {
                Graph Gr = rgg.Graph(18, 0.15);
                int[] col = new int[Gr.VertexCount];
                for (int j = 0; j < Gr.VertexCount; j++)
                    col[j] = rand.Next(23);
                Stage1small.TestCases.Add(new Lab10Stage1TestCase(Gr, col, wyniks[i], 1, "Maly test losowy " + i.ToString()));
            }
        }

        void PrepareMediumTests()
        {
            int n = 13;
            Graph clique = new Graph(n);
            int[] col = new int[n];
            for (int i = 0; i < n; i++)
                for (int j = i + 1; j < n; j++)
                    clique.AddEdge(i, j);
            for (int i = 0; i < n; i++)
                col[i] = i;
            Stage1medium.TestCases.Add(new Lab10Stage1TestCase(clique, col, 0, 1, "Klika bez powtorzen"));

            Graph p2;
            int[] col2;
            makePathSquareTest(8, out p2, out col2);
            Stage1medium.TestCases.Add(new Lab10Stage1TestCase(p2, col2, 16, 1, "Kwadrat sciezki z powtorzeniem"));

            Random rand = new Random(1343);
            Graph p22;
            int[] col22;
            makeRandomPermutation(p2, col2, out p22, out col22, 1343);
            Stage1medium.TestCases.Add(new Lab10Stage1TestCase(p22, col22, 16, 1, "Kwadrat sciezki z powtorzeniem, permutacja"));

            int[] wyniks = { 0, 4, 0, 8, 18, 12, 12, 16 };
            RandomGraphGenerator rgg = new RandomGraphGenerator(15);
            for (int i = 0; i < 4; i++)
            {
                Graph G;
                int[] coll;
                makeSmartRandomTest(rgg, 27, 0.12, 25, out G, out coll, 13);
                Stage1medium.TestCases.Add(new Lab10Stage1TestCase(G, coll, wyniks[i], 1, "Średni test losowy " + i.ToString()));
            }
            for (int i = 4; i < 8; i++)
            {
                Graph G;
                int[] coll;
                makeSmartRandomTest(rgg, 27, 0.12, 5, out G, out coll, 13);
                Stage1medium.TestCases.Add(new Lab10Stage1TestCase(G, coll, wyniks[i], 1, "Średni test losowy " + i.ToString()));
            }
        }

        void PrepareLargeTests()
        {
            int n = 150;
            Graph clique = new Graph(n);
            int[] col = new int[n];
            for (int i = 0; i < n; i++)
                for (int j = i + 1; j < n; j++)
                    clique.AddEdge(i, j);
            for (int i = 0; i < n; i++)
                col[i] = i;
            Stage1large.TestCases.Add(new Lab10Stage1TestCase(clique, col, 0, 1, "Klika bez powtorzen"));

            Graph p2;
            int[] col2;
            makePathSquareTest(80, out p2, out col2);
            Stage1large.TestCases.Add(new Lab10Stage1TestCase(p2, col2, 160, 1, "Kwadrat sciezki z powtorzeniem"));
            Graph p22;
            int[] col22;
            makeRandomPermutation(p2, col2, out p22, out col22, 1343);
            Stage1large.TestCases.Add(new Lab10Stage1TestCase(p22, col22, 160, 1, "Kwadrat sciezki z powtorzeniem, permutacja"));

            int[] wyniks = { 4, 0, 0, 6, 42, 38, 30, 48 };
            RandomGraphGenerator rgg = new RandomGraphGenerator(16);
            for (int i = 0; i < 4; i++)
            {
                Graph G;
                int[] coll;
                makeSmartRandomTest(rgg, 500, 0.02, 480, out G, out coll, 15);
                Stage1large.TestCases.Add(new Lab10Stage1TestCase(G, coll, wyniks[i], 1, "Duży test losowy " + i.ToString()));
            }
            for (int i = 4; i < 8; i++)
            {
                Graph G;
                int[] coll;
                makeSmartRandomTest(rgg, 500, 0.02, 90, out G, out coll, 15);
                Stage1large.TestCases.Add(new Lab10Stage1TestCase(G, coll, wyniks[i], 6, "Duży test losowy " + i.ToString()));
            }
        }

        static void makePathSquareTest(int n, out Graph G, out int[] col)
        {
            G = new Graph(4 * n);
            for (int i = 0; i < 4 * n - 1; i++)
                G.AddEdge(i, i + 1);
            for (int i = 0; i < 4 * n - 2; i++)
                G.AddEdge(i, i + 2);
            col = new int[4 * n];
            for (int i = 0; i < n; i++)
            {
                col[2 * i] = col[2 * n + 2 * i] = 2 * i;
                col[2 * i + 1] = 2 * i + 1;
                col[2 * n + 2 * i + 1] = 2 * n + 2 * i + 1;
            }
        }

        static void makeSmartRandomTest(RandomGraphGenerator rgg, int n, double density, int colors, out Graph G, out int[] col, int seed = 10)
        {
            Random rand = new Random(seed);
            G = rgg.Graph(n, density);
            col = new int[n];
            int tries = 0;
            for (int i = 0; i < n; i++, tries++)
            {
                col[i] = rand.Next(colors);
                foreach (int v in G.OutNeighbors(i))
                    if (v < i && col[v] == col[i])
                        i--;
                if (tries > 5 * n * colors * Math.Log10(colors))
                {
                    tries = 0;
                    colors++;
                }
            }
        }

        static void makeRandomPermutation(IGraph G, int[] c, out Graph Gout, out int[] cout, int seed = 10)
        {
            Random rand = new Random(seed);
            int[] perm = new int[G.VertexCount];
            perm[0] = 0;
            for (int i = 1; i < G.VertexCount; i++)
            {
                int posi = rand.Next(i + 1);
                for (int j = i; j >= posi + 1; j--)
                    perm[j] = perm[j - 1];
                perm[posi] = i;
            }
            Gout = new Graph(G.VertexCount);
            cout = new int[c.GetLength(0)];
            for (int i = 0; i < G.VertexCount; i++)
            {
                cout[perm[i]] = c[i];
                foreach (int v in G.OutNeighbors(i))
                    Gout.AddEdge(perm[i], perm[v]);
            }
        }

        public override double ScoreResult()
        {
            return 2.5;
        }
    }

    class Program
    {

        static int Fibonacci(int n)
        {
            if (n == 0 || n == 1)
                return 1;
            else
                return Fibonacci(n-1) + Fibonacci(n-2);
        }

        public static void Main()
        {
            var tests = new Lab10Tests();
            tests.PrepareTestSets();


            DateTime t1 = DateTime.Now;
            int z = Fibonacci(39);
            DateTime t2 = DateTime.Now;

            foreach (var ts in tests.TestSets)
            {
                DateTime t3 = DateTime.Now;
                ts.Value.PerformTests(verbose: true, checkTimeLimit: false);
                DateTime t4 = DateTime.Now;

                Console.WriteLine($"Unormowany czas działania: {(t4 - t3).TotalMilliseconds / (t2 - t1).TotalMilliseconds * 0.45: 0.00}s");
                Console.WriteLine();
            }



        }

    }


}
