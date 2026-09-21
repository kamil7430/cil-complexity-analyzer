using System;
using System.IO;
using ASD.Graphs;
using CilComplexityAnalyzer.LibCilInjection.Tests.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CilComplexityAnalyzer.LibCilInjection.Tests;

/// <summary>
/// Porównuje koszt (liczbę wykonanych instrukcji CIL) własnej implementacji BFS
/// z kosztem BFS-a z Graphs.dll (g.BFS().SearchFrom(...)).
///
/// Zasada: graf budowany jest WEWNĄTRZ sandboxa (typy ASD.* z sandboxa i z hosta to różne typy —
/// nie da się ich podawać przez Invoke). Przez granicę refleksji przechodzą tylko int-y.
/// Graphs.dll jest instrumentowany razem z kodem testowym — inaczej wywołanie biblioteki
/// zliczyłoby się jako 0 instrukcji i porównanie byłoby bez sensu.
/// </summary>
[TestClass]
public class GraphLibraryComparisonTests
{
    // Progi dobrane na podstawie zmierzonych wartości (licznik jest deterministyczny, więc nie ma szumu):
    //   stosunek kosztu własny/lib  ~0.54-0.58  -> dopuszczamy [0.25, 1.25]
    //   wykładnik skalowania k      lib = own = 1.00, slow ~1.96
    private const double MinCostRatio = 0.25;        // poniżej: podejrzanie tanio (np. bug pomijający pracę)
    private const double MaxCostRatio = 1.25;        // powyżej: własny BFS wyraźnie droższy od bibliotecznego
    private const double ExponentTolerance = 0.1;    // własny może mieć wykładnik k co najwyżej o 0.1 większy niż lib
    private const double MinExponentGap = 0.5;       // kontrola negatywna: kwadratowy musi mieć k większy o >= 0.5

    private const int SmallN = 500;
    private const int LargeN = 1000;

    private const string DriverSource = """
        namespace Drivers;

        using System.Collections.Generic;
        using ASD.Graphs;

        public static class BfsDriver
        {
            private static Graph G;
            private static int _arcs;

            // Liczba łuków (każda krawędź nieskierowana = 2 łuki). Tyle zdarzeń zwraca SearchFrom.
            public static int ExpectedLibraryEdges() => _arcs;

            // Nie jest mierzone (licznik resetowany po Prepare).
            // shape 0 = ścieżka 0-1-2-...; shape 1 = ścieżka + deterministyczne "skróty" (rzadki graf spójny)
            public static void Prepare(int n, int shape)
            {
                var g = new Graph(n);
                for (int i = 0; i + 1 < n; i++)
                    g.AddEdge(i, i + 1);

                if (shape == 1)
                {
                    for (int i = 0; i < n; i++)
                    {
                        int j = (int)(((long)i * 7 + 3) % n);
                        if (i != j && !g.HasEdge(i, j))
                            g.AddEdge(i, j);
                    }
                }
                G = g;

                int arcs = 0;
                for (int i = 0; i < n; i++)
                    arcs += g.Degree(i);
                _arcs = arcs;
            }

            // RunOwn/RunSlow zwracają odwiedzone wierzchołki - 1.
            // RunLibrary zwraca liczbę krawędzi wyemitowanych przez SearchFrom — biblioteka emituje
            // KAŻDY łuk (w grafie nieskierowanym: 2 na krawędź), a nie tylko krawędzie drzewa BFS.

            public static int RunLibrary()
            {
                int edges = 0;
                foreach (var e in G.BFS().SearchFrom(0))
                    edges++;
                return edges;
            }

            public static int RunOwn() => OwnBfs(G, 0).Length - 1;

            public static int RunSlow() => SlowBfs(G, 0).Length - 1;

            // "Rozwiązanie studenta" — poprawne, liniowe.
            public static int[] OwnBfs(Graph g, int start)
            {
                bool[] visited = new bool[g.VertexCount];
                var queue = new Queue<int>();
                var order = new List<int>();

                visited[start] = true;
                queue.Enqueue(start);

                while (queue.Count > 0)
                {
                    int cur = queue.Dequeue();
                    order.Add(cur);
                    foreach (int nb in g.OutNeighbors(cur))
                    {
                        if (!visited[nb])
                        {
                            visited[nb] = true;
                            queue.Enqueue(nb);
                        }
                    }
                }
                return order.ToArray();
            }

            // Kontrola negatywna: wynik poprawny, ale O(V^2) — "visited" jako ręcznie przeszukiwana lista.
            // (Ręczna pętla, a nie List.Contains, bo Contains siedzi w BCL i przy braku instrumentacji BCL
            //  liczyłby się jako jedna instrukcja call.)
            public static int[] SlowBfs(Graph g, int start)
            {
                var order = new List<int>();
                var queue = new Queue<int>();
                order.Add(start);
                queue.Enqueue(start);

                while (queue.Count > 0)
                {
                    int cur = queue.Dequeue();
                    foreach (int nb in g.OutNeighbors(cur))
                    {
                        bool seen = false;
                        for (int k = 0; k < order.Count; k++)
                        {
                            if (order[k] == nb) { seen = true; break; }
                        }
                        if (!seen)
                        {
                            order.Add(nb);
                            queue.Enqueue(nb);
                        }
                    }
                }
                return order.ToArray();
            }
        }
        """;

    private static readonly Lazy<byte[]> DriverBytes = new(() =>
    {
        _ = typeof(Graph).Assembly;
        return DynamicSourceCompiler.CompileSource(DriverSource);
    });

    private static readonly Lazy<byte[]> GraphsBytes = new(() =>
        File.ReadAllBytes(typeof(Graph).Assembly.Location));

    private readonly record struct Costs(long Library, long Own, long Slow);

    private static double Exponent(long costSmall, long costLarge, int nSmall, int nLarge)
        => Math.Log((double)costLarge / costSmall) / Math.Log((double)nLarge / nSmall);

    private static Costs Measure(int n, int shape)
    {
        using var sandbox = InstrumentedSandbox.Create(
            DriverBytes.Value,
            $"bfs_cmp_{n}_{shape}_{Guid.NewGuid():N}",
            GraphsBytes.Value);

        var driver = sandbox.Assembly.GetType("Drivers.BfsDriver")!;
        var prepare = driver.GetMethod("Prepare")!;

        prepare.Invoke(null, new object[] { n, shape });

        (int edges, long cost) Run(string method)
        {
            sandbox.Counter.ResetCounter();
            int edges = (int)driver.GetMethod(method)!.Invoke(null, null)!;
            return (edges, sandbox.Counter.GetCounter());
        }

        var lib = Run("RunLibrary");
        var own = Run("RunOwn");
        var slow = Run("RunSlow");

        int expectedLibraryEdges = (int)driver.GetMethod("ExpectedLibraryEdges")!.Invoke(null, null)!;
        Assert.AreEqual(expectedLibraryEdges, lib.edges, "BFS z biblioteki wyemitował inną liczbę krawędzi niż liczba łuków grafu.");
        Assert.AreEqual(n - 1, own.edges, "Własny BFS nie odwiedził całego grafu.");
        Assert.AreEqual(n - 1, slow.edges, "Wolny BFS nie odwiedził całego grafu.");

        Assert.IsTrue(lib.cost > n,
            $"Koszt BFS z biblioteki ({lib.cost}) jest podejrzanie niski dla n={n} — " +
            "czy Graphs.dll na pewno został zainstrumentowany i załadowany do sandboxa?");

        return new Costs(lib.cost, own.cost, slow.cost);
    }


    [TestMethod]
    [DataRow(0, DisplayName = "ścieżka")]
    [DataRow(1, DisplayName = "rzadki graf ze skrótami")]
    public void OwnBfs_ShouldCostComparableToLibraryBfs(int shape)
    {
        var c = Measure(SmallN, shape);

        double ratio = (double)c.Own / c.Library;
        Console.WriteLine($"shape={shape}: biblioteka={c.Library}, własny={c.Own}, stosunek={ratio:F2}");

        Assert.IsTrue(ratio >= MinCostRatio && ratio <= MaxCostRatio,
            $"Własny BFS wykonał {c.Own} instrukcji, biblioteczny {c.Library} " +
            $"(x{ratio:F2}, dozwolone [{MinCostRatio}, {MaxCostRatio}]).");
    }

    [TestMethod]
    [DataRow(0, DisplayName = "ścieżka")]
    [DataRow(1, DisplayName = "rzadki graf ze skrótami")]
    public void OwnBfs_ShouldScaleLikeLibraryBfs(int shape)
    {
        var small = Measure(SmallN, shape);
        var large = Measure(LargeN, shape);

        double kLib = Exponent(small.Library, large.Library, SmallN, LargeN);
        double kOwn = Exponent(small.Own, large.Own, SmallN, LargeN);
        Console.WriteLine($"shape={shape}: k lib={kLib:F2}, k own={kOwn:F2}");

        Assert.IsTrue(kOwn <= kLib + ExponentTolerance,
            $"Własny BFS skaluje się gorzej niż biblioteczny: k_own={kOwn:F2} vs k_lib={kLib:F2} (tolerancja {ExponentTolerance}).");
    }

    [TestMethod]
    public void QuadraticBfs_ShouldBeDetectedAsGrowingFasterThanLibraryBfs()
    {
        var small = Measure(SmallN, shape: 0);
        var large = Measure(LargeN, shape: 0);

        double kLib = Exponent(small.Library, large.Library, SmallN, LargeN);
        double kSlow = Exponent(small.Slow, large.Slow, SmallN, LargeN);
        Console.WriteLine($"k lib={kLib:F2}, k slow={kSlow:F2}");

        Assert.IsTrue(kSlow >= kLib + MinExponentGap,
            $"Kwadratowy BFS nie został odróżniony od bibliotecznego: k_slow={kSlow:F2} vs k_lib={kLib:F2} " +
            $"(wymagana różnica >= {MinExponentGap}).");
    }
}