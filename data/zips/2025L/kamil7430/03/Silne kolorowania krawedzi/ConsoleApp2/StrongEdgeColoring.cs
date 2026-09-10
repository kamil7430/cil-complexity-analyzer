
namespace ASD
{
    using ASD.Graphs;
    using System.Collections.Generic;

    public class Lab03 : System.MarshalByRefObject
    {
        // Część I
        // Funkcja zwracajaca kwadrat danego grafu.
        // Kwadratem grafu nazywamy graf o takim samym zbiorze wierzchołków jak graf pierwotny, w którym wierzchołki
        // połączone sa krawędzią jeśli w grafie pierwotnym były polączone krawędzia bądź ścieżką złożoną z 2 krawędzi
        // (ale pętli, czyli krawędzi o początku i końcu w tym samym wierzchołku, nie dodajemy!).
        public Graph Square(Graph graph)
        {
            int n = graph.VertexCount;
            Graph g = (Graph)graph.Clone();
            for (int i = 0; i < n; i++)
                foreach (var neigh1 in graph.OutNeighbors(i))
                    foreach (var neigh2 in graph.OutNeighbors(neigh1))
                        if (i != neigh2)
                            g.AddEdge(i, neigh2);
            return g;
        }

        // Część II
        // Funkcja zwracająca Graf krawędziowy danego grafu.
        // Wierzchołki grafu krawędziwego odpowiadają krawędziom grafu pierwotnego, wierzcholki grafu krawędziwego
        // połączone sa krawędzią jeśli w grafie pierwotnym z krawędzi odpowiadającej pierwszemu z nich można przejść
        // na krawędź odpowiadającą drugiemu z nich przez wspólny wierzchołek.

        // Tablicę names tworzymy i wypełniamy według następującej zasady.
        // Każdemu wierzchołkowi grafu krawędziowego odpowiada element tablicy names (o indeksie równym numerowi wierzchołka)
        // zawierający informację z jakiej krawędzi grafu pierwotnego wierzchołek ten powstał.
        // Np.dla wierzchołka powstałego z krawedzi <0,1> do tablicy zapisujemy krotke (0, 1) - przyda się w dalszych etapach
        public Graph LineGraph(Graph graph, out (int x, int y)[] names)
        {
            int n = graph.VertexCount, Ln = graph.EdgeCount;
            Graph LG = new(Ln, graph.Representation);
            names = new (int x, int y)[Ln];
            int j = 0;
            for (int i = 0; i < n; i++)
                foreach (var neigh in graph.OutNeighbors(i).Where(x => x > i))
                {
                    names[j] = (i, neigh);
                    j++;
                }
            for (int i = 0; i < Ln; i++)
            {
                var v = names[i];
                for (int k = i + 1; k < Ln; k++)
                {
                    var t = names[k];
                    if (v.x == t.x || v.x == t.y || v.y == t.x || v.y == t.y)
                        LG.AddEdge(i, k);
                }
            }
            return LG;
        }

        // Część III
        // Funkcja znajdujaca poprawne kolorowanie wierzchołków danego grafu nieskierowanego.
        // Kolorowanie wierzchołków jest poprawne, gdy każde dwa sąsiadujące wierzchołki mają różne kolory
        // Funkcja ma szukać kolorowania według następujacego algorytmu zachłannego:

        // Dla wszystkich wierzchołków v (od 0 do n-1)
        // pokoloruj wierzcholek v kolorem o najmniejszym możliwym numerze(czyli takim, na który nie są pomalowani jego sąsiedzi)
        // Kolory numerujemy począwszy od 0.

        // UWAGA: Podany opis wyznacza kolorowanie jednoznacznie, jakiekolwiek inne kolorowanie, nawet jeśli spełnia formalnie
        // definicję kolorowania poprawnego, na potrzeby tego zadania będzie uznane za błędne.

        // Funkcja zwraca liczbę użytych kolorów (czyli najwyższy numer użytego koloru + 1),
        // a w tablicy colors zapamiętuje kolory poszczególnych wierzchołkow.
        public int VertexColoring(Graph graph, out int[] colors)
        {
            int n = graph.VertexCount;
            colors = new int[n];
            colors[0] = 0;
            int maxColor = 0;
            for (int i = 1; i < n; i++)
            {
                HashSet<int> occupied = new();
                foreach (var neigh in graph.OutNeighbors(i).Where(x => x < i))
                    occupied.Add(colors[neigh]);
                int j;
                for(j = 0; j<=maxColor;j++)
                    if (!occupied.Contains(j))
                        break;
                colors[i] = j;
                if (j > maxColor)
                    maxColor = j;
            }
            return maxColor + 1;
        }

        // Funkcja znajduje silne kolorowanie krawędzi danego grafu.
        // Silne kolorowanie krawędzi grafu jest poprawne gdy każde dwie krawędzie, które są ze sobą sąsiednie
        // (czyli można przejść z jednej na drugą przez wspólny wierzchołek)
        // albo są połączone inną krawędzią(czyli można przejść z jednej na drugą przez ową inną krawędź), mają różne kolory.

        // Należy zwrocić nowy graf, który będzie miał strukturę identyczną jak zadany graf,
        // ale w wagach krawędzi zostaną zapisane przydzielone kolory.

        // Wskazówka - to bardzo proste.Należy wykorzystać wszystkie poprzednie funkcje.
        // Zastanowić się co możemy powiedzieć o kolorowaniu wierzchołków kwadratu grafu krawędziowego?
        // Jak się to ma do silnego kolorowania krawędzi grafu pierwotnego?
        public int StrongEdgeColoring(Graph graph, out Graph<int> coloredGraph)
        {
            Graph L2G = Square(LineGraph(graph, out var names));
            int maxColor = VertexColoring(L2G, out var colors);
            coloredGraph = new(graph.VertexCount, graph.Representation);
            for (int i = 0; i < colors.Length; i++)
                coloredGraph.AddEdge(names[i].x, names[i].y, colors[i]);
            return maxColor;
        }

    }

}
