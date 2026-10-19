using System;
using ASD;
using ASD.Graphs;
using ASD.Graphs.Testing;
using CilComplexityAnalyzer.Contract;
using CilComplexityAnalyzer.Contract.Attributes;

namespace CilComplexityAnalyzer.Facade.Tests;

[StudentSolution(typeof(Lab03))]
public partial class Lab03Evaluation : TestSuite
{
    public override TestSuiteSettings? Settings()
        => new TestSuiteSettings
        {
            Containerized = false,
        };

    public class Case0_Reverse_Gesty : TestCase
    {
        private DiGraph _g = null!;
        private DiGraph _original = null!;
        private DiGraph? _result;

        public override int TestNumber() => 0;

        public override TestCaseSettings Settings() => new TestCaseSettings();

        public override void Arrange()
        {
            _g = GraphHelper.ReverseGraph(0);
            _original = (DiGraph)_g.Clone();
        }

        public override void Act()
        {
            var studentLab = new Lab03();
            _result = studentLab.Lab03Reverse(_g);
        }

        public override void Assert()
        {
            CheckReverse(this, _g, _original, _result);
            base.Assert();
        }
    }

    public class Case1_Reverse_RzadkiDuzy : TestCase
    {
        private DiGraph _g = null!;
        private DiGraph _original = null!;
        private DiGraph? _result;

        public override int TestNumber() => 1;

        public override TestCaseSettings Settings() => new TestCaseSettings();

        public override void Arrange()
        {
            _g = GraphHelper.ReverseGraph(1);
            _original = (DiGraph)_g.Clone();
        }

        public override void Act()
        {
            var studentLab = new Lab03();
            _result = studentLab.Lab03Reverse(_g);
        }

        public override void Assert()
        {
            CheckReverse(this, _g, _original, _result);
            base.Assert();
        }
    }

    public class Case2_Reverse_Macierz : TestCase
    {
        private DiGraph _g = null!;
        private DiGraph _original = null!;
        private DiGraph? _result;

        public override int TestNumber() => 2;

        public override TestCaseSettings Settings() => new TestCaseSettings();

        public override void Arrange()
        {
            _g = GraphHelper.ReverseGraph(2);
            _original = (DiGraph)_g.Clone();
        }

        public override void Act()
        {
            var studentLab = new Lab03();
            _result = studentLab.Lab03Reverse(_g);
        }

        public override void Assert()
        {
            CheckReverse(this, _g, _original, _result);
            base.Assert();
        }
    }

    public class Case3_Reverse_MacierzRzadka : TestCase
    {
        private DiGraph _g = null!;
        private DiGraph _original = null!;
        private DiGraph? _result;

        public override int TestNumber() => 3;

        public override TestCaseSettings Settings() => new TestCaseSettings();

        public override void Arrange()
        {
            _g = GraphHelper.ReverseGraph(3);
            _original = (DiGraph)_g.Clone();
        }

        public override void Act()
        {
            var studentLab = new Lab03();
            _result = studentLab.Lab03Reverse(_g);
        }

        public override void Assert()
        {
            CheckReverse(this, _g, _original, _result);
            base.Assert();
        }
    }

    public class Case4_Reverse_BezKrawedzi : TestCase
    {
        private DiGraph _g = null!;
        private DiGraph _original = null!;
        private DiGraph? _result;

        public override int TestNumber() => 4;

        public override TestCaseSettings Settings() => new TestCaseSettings();

        public override void Arrange()
        {
            _g = GraphHelper.ReverseGraph(4);
            _original = (DiGraph)_g.Clone();
        }

        public override void Act()
        {
            var studentLab = new Lab03();
            _result = studentLab.Lab03Reverse(_g);
        }

        public override void Assert()
        {
            CheckReverse(this, _g, _original, _result);
            base.Assert();
        }
    }

    public class Case5_Bipartite_Dwudzielny : TestCase
    {
        private Graph _g = null!;
        private Graph _original = null!;
        private bool _expected;
        private bool _result;
        private int[]? _part;

        public override int TestNumber() => 5;

        public override TestCaseSettings Settings() => new TestCaseSettings();

        public override void Arrange()
        {
            _g = GraphHelper.BipartiteGraph(0);
            _original = (Graph)_g.Clone();
            _expected = true;
        }

        public override void Act()
        {
            var studentLab = new Lab03();
            _result = studentLab.Lab03IsBipartite(_g, out _part);
        }

        public override void Assert()
        {
            CheckBipartite(this, _g, _original, _expected, _result, _part);
            base.Assert();
        }
    }

    public class Case6_Bipartite_LosowyRzadki : TestCase
    {
        private Graph _g = null!;
        private Graph _original = null!;
        private bool _expected;
        private bool _result;
        private int[]? _part;

        public override int TestNumber() => 6;

        public override TestCaseSettings Settings() => new TestCaseSettings();

        public override void Arrange()
        {
            _g = GraphHelper.BipartiteGraph(1);
            _original = (Graph)_g.Clone();
            _expected = false;
        }

        public override void Act()
        {
            var studentLab = new Lab03();
            _result = studentLab.Lab03IsBipartite(_g, out _part);
        }

        public override void Assert()
        {
            CheckBipartite(this, _g, _original, _expected, _result, _part);
            base.Assert();
        }
    }

    public class Case7_Bipartite_LosowyGesty : TestCase
    {
        private Graph _g = null!;
        private Graph _original = null!;
        private bool _expected;
        private bool _result;
        private int[]? _part;

        public override int TestNumber() => 7;

        public override TestCaseSettings Settings() => new TestCaseSettings();

        public override void Arrange()
        {
            _g = GraphHelper.BipartiteGraph(2);
            _original = (Graph)_g.Clone();
            _expected = false;
        }

        public override void Act()
        {
            var studentLab = new Lab03();
            _result = studentLab.Lab03IsBipartite(_g, out _part);
        }

        public override void Assert()
        {
            CheckBipartite(this, _g, _original, _expected, _result, _part);
            base.Assert();
        }
    }

    public class Case8_Bipartite_CyklNieparzysty : TestCase
    {
        private Graph _g = null!;
        private Graph _original = null!;
        private bool _expected;
        private bool _result;
        private int[]? _part;

        public override int TestNumber() => 8;

        public override TestCaseSettings Settings() => new TestCaseSettings();

        public override void Arrange()
        {
            _g = GraphHelper.BipartiteGraph(3);
            _original = (Graph)_g.Clone();
            _expected = false;
        }

        public override void Act()
        {
            var studentLab = new Lab03();
            _result = studentLab.Lab03IsBipartite(_g, out _part);
        }

        public override void Assert()
        {
            CheckBipartite(this, _g, _original, _expected, _result, _part);
            base.Assert();
        }
    }

    public class Case9_Bipartite_BezKrawedzi : TestCase
    {
        private Graph _g = null!;
        private Graph _original = null!;
        private bool _expected;
        private bool _result;
        private int[]? _part;

        public override int TestNumber() => 9;

        public override TestCaseSettings Settings() => new TestCaseSettings();

        public override void Arrange()
        {
            _g = GraphHelper.BipartiteGraph(4);
            _original = (Graph)_g.Clone();
            _expected = true;
        }

        public override void Act()
        {
            var studentLab = new Lab03();
            _result = studentLab.Lab03IsBipartite(_g, out _part);
        }

        public override void Assert()
        {
            CheckBipartite(this, _g, _original, _expected, _result, _part);
            base.Assert();
        }
    }

    public class Case10_Kruskal_Maly : TestCase
    {
        private Graph<int> _g = null!;
        private Graph<int> _original = null!;
        private int _expectedWeight;
        private Graph<int>? _result;
        private int _weight;

        public override int TestNumber() => 10;

        public override TestCaseSettings Settings() => new TestCaseSettings();

        public override void Arrange()
        {
            _g = GraphHelper.KruskalGraph(0);
            _original = (Graph<int>)_g.Clone();
            _expectedWeight = -133;
        }

        public override void Act()
        {
            var studentLab = new Lab03();
            _result = studentLab.Lab03Kruskal(_g, out _weight);
        }

        public override void Assert()
        {
            CheckKruskal(this, _g, _original, _expectedWeight, _result, _weight);
            base.Assert();
        }
    }

    public class Case11_Kruskal_RzadkiDuzy : TestCase
    {
        private Graph<int> _g = null!;
        private Graph<int> _original = null!;
        private int _expectedWeight;
        private Graph<int>? _result;
        private int _weight;

        public override int TestNumber() => 11;

        public override TestCaseSettings Settings() => new TestCaseSettings();

        public override void Arrange()
        {
            _g = GraphHelper.KruskalGraph(1);
            _original = (Graph<int>)_g.Clone();
            _expectedWeight = -76248;
        }

        public override void Act()
        {
            var studentLab = new Lab03();
            _result = studentLab.Lab03Kruskal(_g, out _weight);
        }

        public override void Assert()
        {
            CheckKruskal(this, _g, _original, _expectedWeight, _result, _weight);
            base.Assert();
        }
    }

    public class Case12_Kruskal_Jednorodny : TestCase
    {
        private Graph<int> _g = null!;
        private Graph<int> _original = null!;
        private int _expectedWeight;
        private Graph<int>? _result;
        private int _weight;

        public override int TestNumber() => 12;

        public override TestCaseSettings Settings() => new TestCaseSettings();

        public override void Arrange()
        {
            _g = GraphHelper.KruskalGraph(2);
            _original = (Graph<int>)_g.Clone();
            _expectedWeight = 9;
        }

        public override void Act()
        {
            var studentLab = new Lab03();
            _result = studentLab.Lab03Kruskal(_g, out _weight);
        }

        public override void Assert()
        {
            CheckKruskal(this, _g, _original, _expectedWeight, _result, _weight);
            base.Assert();
        }
    }

    public class Case13_Kruskal_Cykl : TestCase
    {
        private Graph<int> _g = null!;
        private Graph<int> _original = null!;
        private int _expectedWeight;
        private Graph<int>? _result;
        private int _weight;

        public override int TestNumber() => 13;

        public override TestCaseSettings Settings() => new TestCaseSettings();

        public override void Arrange()
        {
            _g = GraphHelper.KruskalGraph(3);
            _original = (Graph<int>)_g.Clone();
            _expectedWeight = 20462;
        }

        public override void Act()
        {
            var studentLab = new Lab03();
            _result = studentLab.Lab03Kruskal(_g, out _weight);
        }

        public override void Assert()
        {
            CheckKruskal(this, _g, _original, _expectedWeight, _result, _weight);
            base.Assert();
        }
    }

    public class Case14_Kruskal_BezKrawedzi : TestCase
    {
        private Graph<int> _g = null!;
        private Graph<int> _original = null!;
        private int _expectedWeight;
        private Graph<int>? _result;
        private int _weight;

        public override int TestNumber() => 14;

        public override TestCaseSettings Settings() => new TestCaseSettings();

        public override void Arrange()
        {
            _g = GraphHelper.KruskalGraph(4);
            _original = (Graph<int>)_g.Clone();
            _expectedWeight = 0;
        }

        public override void Act()
        {
            var studentLab = new Lab03();
            _result = studentLab.Lab03Kruskal(_g, out _weight);
        }

        public override void Assert()
        {
            CheckKruskal(this, _g, _original, _expectedWeight, _result, _weight);
            base.Assert();
        }
    }

    public class Case15_Acyclic_Drzewo : TestCase
    {
        private Graph _g = null!;
        private Graph _original = null!;
        private bool _expected;
        private bool _result;

        public override int TestNumber() => 15;

        public override TestCaseSettings Settings() => new TestCaseSettings();

        public override void Arrange()
        {
            _g = GraphHelper.AcyclicGraph(0);
            _original = (Graph)_g.Clone();
            _expected = true;
        }

        public override void Act()
        {
            var studentLab = new Lab03();
            _result = studentLab.Lab03IsUndirectedAcyclic(_g);
        }

        public override void Assert()
        {
            CheckAcyclic(this, _g, _original, _expected, _result);
            base.Assert();
        }
    }

    public class Case16_Acyclic_LosowyRzadki : TestCase
    {
        private Graph _g = null!;
        private Graph _original = null!;
        private bool _expected;
        private bool _result;

        public override int TestNumber() => 16;

        public override TestCaseSettings Settings() => new TestCaseSettings();

        public override void Arrange()
        {
            _g = GraphHelper.AcyclicGraph(1);
            _original = (Graph)_g.Clone();
            _expected = false;
        }

        public override void Act()
        {
            var studentLab = new Lab03();
            _result = studentLab.Lab03IsUndirectedAcyclic(_g);
        }

        public override void Assert()
        {
            CheckAcyclic(this, _g, _original, _expected, _result);
            base.Assert();
        }
    }

    public class Case17_Acyclic_LosowyGesty : TestCase
    {
        private Graph _g = null!;
        private Graph _original = null!;
        private bool _expected;
        private bool _result;

        public override int TestNumber() => 17;

        public override TestCaseSettings Settings() => new TestCaseSettings();

        public override void Arrange()
        {
            _g = GraphHelper.AcyclicGraph(2);
            _original = (Graph)_g.Clone();
            _expected = false;
        }

        public override void Act()
        {
            var studentLab = new Lab03();
            _result = studentLab.Lab03IsUndirectedAcyclic(_g);
        }

        public override void Assert()
        {
            CheckAcyclic(this, _g, _original, _expected, _result);
            base.Assert();
        }
    }

    public class Case18_Acyclic_DlugiCykl : TestCase
    {
        private Graph _g = null!;
        private Graph _original = null!;
        private bool _expected;
        private bool _result;

        public override int TestNumber() => 18;

        public override TestCaseSettings Settings() => new TestCaseSettings();

        public override void Arrange()
        {
            _g = GraphHelper.AcyclicGraph(3);
            _original = (Graph)_g.Clone();
            _expected = false;
        }

        public override void Act()
        {
            var studentLab = new Lab03();
            _result = studentLab.Lab03IsUndirectedAcyclic(_g);
        }

        public override void Assert()
        {
            CheckAcyclic(this, _g, _original, _expected, _result);
            base.Assert();
        }
    }

    public class Case19_Acyclic_BezKrawedzi : TestCase
    {
        private Graph _g = null!;
        private Graph _original = null!;
        private bool _expected;
        private bool _result;

        public override int TestNumber() => 19;

        public override TestCaseSettings Settings() => new TestCaseSettings();

        public override void Arrange()
        {
            _g = GraphHelper.AcyclicGraph(4);
            _original = (Graph)_g.Clone();
            _expected = true;
        }

        public override void Act()
        {
            var studentLab = new Lab03();
            _result = studentLab.Lab03IsUndirectedAcyclic(_g);
        }

        public override void Assert()
        {
            CheckAcyclic(this, _g, _original, _expected, _result);
            base.Assert();
        }
    }

    private static void CheckReverse(
        TestCase testCase,
        DiGraph g,
        DiGraph original,
        DiGraph? result)
    {
        testCase.IsTrue(result != null, "Zwrócono null zamiast grafu");
        if (result == null)
            return;

        testCase.IsTrue(result.Representation.GetType() == g.Representation.GetType(),
            "Zwrócony graf ma niepoprawną reprezentację");

        testCase.IsTrue(g.Equals(original), "Graf wejściowy został zmodyfikowany");

        testCase.IsTrue(result.VertexCount == original.VertexCount,
            $"Zwrócony graf ma {result.VertexCount} wierzchołków, powinien mieć {original.VertexCount}");
        if (result.VertexCount != original.VertexCount)
            return;

        for (int v = 0; v < result.VertexCount; v++)
        {
            foreach (int u in result.OutNeighbors(v))
                testCase.IsTrue(original.HasEdge(u, v),
                    $"Zwrócony graf zawiera krawędź {v}->{u}, której odwrotność nie istnieje w grafie wejściowym");

            foreach (int u in original.OutNeighbors(v))
                testCase.IsTrue(result.HasEdge(u, v),
                    $"W zwróconym grafie brakuje krawędzi {u}->{v}");
        }
    }

    private static void CheckBipartite(
        TestCase testCase,
        Graph g,
        Graph original,
        bool expected,
        bool result,
        int[]? part)
    {
        testCase.IsTrue(g.Equals(original), "Graf wejściowy został zmodyfikowany");

        testCase.IsTrue(result == expected,
            $"Zwrócono {result}, a powinno być {expected}");

        if (result)
        {
            string? error = CheckPartition(g, part);
            testCase.IsTrue(error == null, error ?? "");
        }
        else
        {
            testCase.IsTrue(part == null, "Dla grafu niedwudzielnego part powinno być null");
        }
    }

    private static string? CheckPartition(Graph g, int[]? part)
    {
        if (part == null)
            return "Zwrócono null zamiast podziału wierzchołków";

        if (part.Length != g.VertexCount)
            return $"Podział ma długość {part.Length}, powinien mieć {g.VertexCount}";

        for (int v = 0; v < g.VertexCount; v++)
            if (part[v] != 1 && part[v] != 2)
                return $"Wierzchołek {v} ma nieprawidłowy numer części: {part[v]} (dozwolone 1 lub 2)";

        for (int v = 0; v < g.VertexCount; v++)
            foreach (int u in g.OutNeighbors(v))
                if (part[u] == part[v])
                    return $"Krawędź {v}-{u} łączy wierzchołki z tej samej części";

        return null;
    }

    private static void CheckKruskal(
        TestCase testCase,
        Graph<int> g,
        Graph<int> original,
        int expectedWeight,
        Graph<int>? result,
        int weight)
    {
        testCase.IsTrue(result != null, "Zwrócono null zamiast drzewa");
        if (result == null)
            return;

        testCase.IsTrue(!result.Directed, "Zwrócony graf jest skierowany");
        testCase.IsTrue(result.GetType() == g.GetType(), "Zwrócony graf ma niepoprawny typ/reprezentację");
        testCase.IsTrue(g.Equals(original), "Graf wejściowy został zmodyfikowany");

        testCase.IsTrue(weight == expectedWeight,
            $"Zwrócono wagę {weight}, a powinno być {expectedWeight}");

        testCase.IsTrue(result.VertexCount == g.VertexCount,
            $"Zwrócone drzewo ma {result.VertexCount} wierzchołków, powinno mieć {g.VertexCount}");
        if (result.VertexCount != g.VertexCount)
            return;

        int edges = 0;
        int treeWeight = 0;
        bool edgesExist = true;
        for (int v = 0; v < result.VertexCount; v++)
        {
            foreach (int u in result.OutNeighbors(v))
            {
                if (!g.HasEdge(v, u))
                {
                    edgesExist = false;
                    continue;
                }

                if (u < v)
                {
                    edges++;
                    treeWeight += g.GetEdgeWeight(v, u);
                }
            }
        }

        testCase.IsTrue(edgesExist, "Zwrócone drzewo zawiera krawędź, której nie ma w grafie wejściowym");
        testCase.IsTrue(edges <= g.VertexCount - 1,
            $"Zwrócone drzewo ma {edges} krawędzi, a maksymalnie może mieć {g.VertexCount - 1}");
        testCase.IsTrue(treeWeight == weight,
            $"Suma wag krawędzi zwróconego drzewa wynosi {treeWeight}, a zwrócona waga to {weight}");
    }

    private static void CheckAcyclic(
        TestCase testCase,
        Graph g,
        Graph original,
        bool expected,
        bool result)
    {
        testCase.IsTrue(g.Equals(original), "Graf wejściowy został zmodyfikowany");

        testCase.IsTrue(result == expected,
            $"Zwrócono {result}, a powinno być {expected}");
    }
}

public static class GraphHelper
{
    private const int Seed = 12345;

    public static DiGraph ReverseGraph(int index)
    {
        var rgg = new RandomGraphGenerator(Seed);
        var g = new DiGraph[5];

        g[0] = rgg.DiGraph(10, 0.7);
        g[1] = rgg.DiGraph(100, 0.1);
        rgg.SetRepresentation(new MatrixGraphRepresentation());
        g[2] = rgg.DiGraph(10, 0.5);
        g[3] = rgg.DiGraph(30, 0.1);
        g[4] = new DiGraph(50000);

        return g[index];
    }

    public static Graph BipartiteGraph(int index)
    {
        var rgg = new RandomGraphGenerator(Seed);
        var g = new Graph[5];

        g[0] = rgg.BipartiteGraph(4, 3, 0.4);
        g[1] = rgg.Graph(100, 0.1);
        g[2] = rgg.Graph(10, 0.5);
        g[3] = GraphExamples.Cycle(50001);
        g[4] = new Graph(50000);

        return g[index];
    }

    public static Graph<int> KruskalGraph(int index)
    {
        var rgg = new RandomGraphGenerator(Seed);
        var g = new Graph<int>[5];

        g[0] = rgg.WeightedGraph(5, 0.7, -99, 99);
        g[1] = rgg.WeightedGraph(100, 0.1, -999, 999);
        g[2] = rgg.WeightedGraph(10, 0.5, 1, 1);
        g[3] = rgg.AssignWeights(GraphExamples.Cycle(50000), -99, 99);
        g[4] = new Graph<int>(50000);

        return g[index];
    }

    public static Graph AcyclicGraph(int index)
    {
        var rgg = new RandomGraphGenerator(Seed);
        var g = new Graph[5];

        g[0] = rgg.Tree(7);
        g[1] = rgg.Graph(100, 0.1);
        g[2] = rgg.Graph(10, 0.5);
        g[3] = GraphExamples.Cycle(50000);
        g[4] = new Graph(50000);

        return g[index];
    }
}