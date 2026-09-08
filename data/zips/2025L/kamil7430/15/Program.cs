using System;
using ASD.Graphs;
using ASD.Graphs.Testing;
using System.Collections.Generic;

namespace ASD
{

    public class Lab15Part1TestCase : TestCase
    {
        protected readonly int expectedResult;
        protected int result;

        protected readonly Graph G;

        public Lab15Part1TestCase(Graph G, int maxBubbleSize, double timeLimit, string description) : base(timeLimit, null, description)
        {
            this.G = G;
            this.expectedResult = maxBubbleSize;
        }

        protected override void PerformTestCase(object prototypeObject)
        {
            result = ((Lab15)prototypeObject).MaxProvinceSize(G);
        }

        protected override (Result resultCode, string message) VerifyTestCase(object settings)
        {
            var (code, msg) = checkSolution(result);
            return (code, $"{msg} [{this.Description}]");
        }

        private (Result resultCode, string message) checkSolution(int returnedResult)
        {
            if (expectedResult != returnedResult)
            {
                return (Result.WrongResult, $"Zwrócono {returnedResult}, powinno być {expectedResult}");
            }

            return (Result.Success, "OK ");
        }
    }


    public class Lab15Part2TestCase : Lab15Part1TestCase
    {

        public Lab15Part2TestCase(Graph G, int vertexInMostBubbles, double timeLimit, string description) : base(G, vertexInMostBubbles, timeLimit, description)
        {
        }

        protected override void PerformTestCase(object prototypeObject)
        {
            result = ((Lab15)prototypeObject).VertexInMostProvinces(G);
        }
    }

    public class Lab06Tests : TestModule
    {
        TestSet Stage1 = new TestSet(prototypeObject: new Lab15(), description: "Etap 1, rozmiar największej bańki", settings: true);
        TestSet Stage2 = new TestSet(prototypeObject: new Lab15(), description: "Etap 2, wierzchołek w największej liczbie baniek", settings: true);

        public override void PrepareTestSets()
        {
            TestSets["Stage1"] = Stage1;
            TestSets["Stage2"] = Stage2;

            prepare();
        }

        private void addStage1(Lab15Part1TestCase s1TestCase)
        {
            Stage1.TestCases.Add(s1TestCase);
        }

        private void addStage2(Lab15Part2TestCase s2TestCase)
        {
            Stage2.TestCases.Add(s2TestCase);
        }

        private void prepare()
        {
            Graph triangle = new Graph(@"
0:1 2
1:0 2
2:0 1
");
            addStage1(new Lab15Part1TestCase(triangle, 3, 1.0, "Trójkąt"));
            addStage2(new Lab15Part2TestCase(triangle, 0, 1.0, "Trójkąt"));

            Graph twoCycles = new Graph(@"
0:1 4
1:0 2
2:1 3
3:2 4
4:0 3 5 7
5:4 6
6:5 7
7:6 4
");
            addStage1(new Lab15Part1TestCase(twoCycles, 5, 1.0, "Dwa cykle ze wspólnym wierzchołkiem"));
            addStage2(new Lab15Part2TestCase(twoCycles, 4, 1.0, "Dwa cykle ze wspólnym wierzchołkiem"));

            Graph K5 = new Graph(@"
0: 1 2 3 4
1: 0 2 3 4
2: 0 1 3 4
3: 0 1 2 4
4: 0 1 2 3
");
            addStage1(new Lab15Part1TestCase(K5, 5, 1.0, "Graf pełny 5 wierzchołków"));
            addStage2(new Lab15Part2TestCase(K5, 0, 1.0, "Graf pełny 5 wierzchołków"));

            Graph T5 = new Graph(@"
0:1
1:0 2
2:1 3 4
3:2
4:2
");
            addStage1(new Lab15Part1TestCase(T5, 0, 1.0, "Drzewo 5 wierzchołków"));
            addStage2(new Lab15Part2TestCase(T5, 0, 1.0, "Drzewo 5 wierzchołków"));

            Graph dumbbell = new Graph(@"
0:4 5
1:2 4
2:1 3
3:2 4
4:0 1 3
5:0 6 8
6:5 7
7:6 8
8:7 6
");
            addStage1(new Lab15Part1TestCase(dumbbell, 4, 1.0, "Dwa cykle połączone scieżką"));
            addStage2(new Lab15Part2TestCase(dumbbell, 1, 1.0, "Dwa cykle połączone ścieżką"));

            Graph disjointTriangles = new Graph(@"
0:1 2
1:0 2
2:0 1
3:4 5
4:3 5
5:3 4
6:7 8
7:6 8
8:6 7
");

            addStage1(new Lab15Part1TestCase(disjointTriangles, 3, 1.0, "Trzy rozłączne trójkąty"));
            addStage2(new Lab15Part2TestCase(disjointTriangles, 0, 1.0, "Trzy rozłączne trójkąty"));

            Graph cactus = new Graph(@"
0:1 4
1:2 3
2:1 3
3:1 2
4:0 5 7
5:4 6
6:5 7 8
7:4 6
8:6 9 17
9:8 10
10:9 11 15 16 21
11:10 12
12:11 13
13:12 14
14:13 15
15:10 14
16:10 17
17:8 16
18:10 19
19:18 20
20:19 21
21:10 20
");
            addStage1(new Lab15Part1TestCase(cactus, 6, 1.0, "Kaktus"));
            addStage2(new Lab15Part2TestCase(cactus, 10, 1.0, "Kaktus"));

            Graph strange = new Graph(@"
0:1 4
1:0 2
2:1 3 5
3:2 4 5 6 8
4:0 3
5:2 3 6 7 14 20 22 23
6:3 7
7:5 6
8:3 9 10 11
9:8 10
10:8 9 11
11:8 10 12 13
12:11 13
13:11 12
14:5 15 21
15:14 16
16:15 17 21
17:16 18
18:17 19
19:18 20 21
20:5 19
21:14 16 19
22:5 23
23:5 22
");

            addStage1(new Lab15Part1TestCase(strange, 9, 1.0, "Złośliwy graf"));
            addStage2(new Lab15Part2TestCase(strange, 5, 1.0, "Złośliwy graf"));

            Graph strange2 = new Graph(@"
0:14 16 19
1:21 2
2:1 3 5
3:2 4 5 6 8
4:21 3
5:2 3 6 7 14 20 22 23
6:3 7
7:5 6
8:3 9 10 11
9:8 10
10:8 9 11
11:8 10 12 13
12:11 13
13:11 12
14:5 15 0
15:14 16
16:15 17 0
17:16 18
18:17 19
19:18 20 0
20:5 19
21:1 4
22:5 23
23:5 22
");

            addStage1(new Lab15Part1TestCase(strange2, 9, 1.0, "Złośliwy graf + zamiana 0<->21"));
            addStage2(new Lab15Part2TestCase(strange2, 5, 1.0, "Złośliwy graf + zamiana 0<->21"));

            int[] answerStage1 = { 9, 3, 7 };
            int[] answerStage2 = { 4, 6, 1 };

            RandomGraphGenerator rgg = new RandomGraphGenerator(1);
            for (int i = 0; i < 3; i++)
            {
                Graph G = rgg.Graph(25, 0.08);
                addStage1(new Lab15Part1TestCase(G, answerStage1[i], 1, $"Mały losowy graf {i}"));
                addStage2(new Lab15Part2TestCase(G, answerStage2[i], 1, $"Mały losowy graf {i}"));
            }
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
                ts.Value.PerformTests(verbose: true, checkTimeLimit: true);
            }
        }
    }
}