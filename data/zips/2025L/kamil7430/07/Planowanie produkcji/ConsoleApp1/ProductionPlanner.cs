using System;
using System.Linq;
using ASD.Graphs;

namespace ASD
{
    public class ProductionPlanner : MarshalByRefObject
    {
        /// <summary>
        /// Flaga pozwalająca na włączenie wypisywania szczegółów skonstruowanego planu na konsolę.
        /// Wartość <code>true</code> spoeoduje wypisanie planu.
        /// </summary>
        public bool ShowDebug { get; } = false;

        /// <summary>
        /// Część 1. zadania - zaplanowanie produkcji telewizorów dla pojedynczego kontrahenta.
        /// </summary>
        /// <remarks>
        /// Do przeprowadzenia testów wyznaczających maksymalną produkcję i zysk wymagane jest jedynie zwrócenie obiektu <see cref="PlanData"/>.
        /// Testy weryfikujące plan wymagają przypisania tablicy z planem do parametru wyjściowego <see cref="weeklyPlan"/>.
        /// </remarks>
        /// <param name="production">
        /// Tablica obiektów zawierających informacje o produkcji fabryki w kolejnych tygodniach.
        /// Wartości pola <see cref="PlanData.Quantity"/> oznaczają limit produkcji w danym tygodniu,
        /// a pola <see cref="PlanData.Value"/> - koszt produkcji jednej sztuki.
        /// </param>
        /// <param name="sales">
        /// Tablica obiektów zawierających informacje o sprzedaży w kolejnych tygodniach.
        /// Wartości pola <see cref="PlanData.Quantity"/> oznaczają maksymalną sprzedaż w danym tygodniu,
        /// a pola <see cref="PlanData.Value"/> - cenę sprzedaży jednej sztuki.
        /// </param>
        /// <param name="storageInfo">
        /// Obiekt zawierający informacje o magazynie.
        /// Wartość pola <see cref="PlanData.Quantity"/> oznacza pojemność magazynu,
        /// a pola <see cref="PlanData.Value"/> - koszt przechowania jednego telewizora w magazynie przez jeden tydzień.
        /// </param>
        /// <param name="weeklyPlan">
        /// Parametr wyjściowy, przez który powinien zostać zwrócony szczegółowy plan sprzedaży.
        /// </param>
        /// <returns>
        /// Obiekt <see cref="PlanData"/> opisujący wyznaczony plan.
        /// W polu <see cref="PlanData.Quantity"/> powinna znaleźć się maksymalna liczba wyprodukowanych telewizorów,
        /// a w polu <see cref="PlanData.Value"/> - wyznaczony maksymalny zysk fabryki.
        /// </returns>
        public PlanData CreateSimplePlan(PlanData[] production, PlanData[] sales, PlanData storageInfo,
            out SimpleWeeklyPlan[] weeklyPlan)
        {
            int tyg = production.Length;
            int n = 4 * tyg + 2;
            int s = n - 2, t = n - 1;
            NetworkWithCosts<int, double> graf = new(n);

            // fabryka -> tygodnie produkcji
            for (int i = 0; i < tyg; i++)
                graf.AddEdge(s, i, production[i].Quantity, production[i].Value);

            // tygodnie produkcji -> ...
            for (int i = 0; i < tyg; i++)
            {
                graf.AddEdge(i, 2 * tyg + i, sales[i].Quantity, -sales[i].Value);
                if (i < tyg - 1)
                    graf.AddEdge(i, 3 * tyg + i, storageInfo.Quantity, storageInfo.Value);
            }
            
            // magazynIn -> magazynOut
            for (int i = 0; i < tyg - 1; i++)
                graf.AddEdge(3 * tyg + i, i + tyg, storageInfo.Quantity, 0);

            // magazynOut -> ...
            for (int i = 0; i < tyg - 1; i++)
            {
                graf.AddEdge(i + tyg, 2 * tyg + i + 1, sales[i + 1].Quantity, -sales[i + 1].Value);
                graf.AddEdge(i + tyg, i + 3 * tyg + 1, storageInfo.Quantity, storageInfo.Value);
            }
            
            // odbiory -> klient
            for (int i = 0; i < tyg; i++)
                graf.AddEdge(2 * tyg + i, t, sales[i].Quantity, 0);

            var przeplyw = Flows.MinCostMaxFlow(graf, s, t);
            
            weeklyPlan = new SimpleWeeklyPlan[tyg];
            for (int i = 0; i < tyg; i++)
            {
                try {weeklyPlan[i].UnitsProduced = przeplyw.Item3.GetEdgeWeight(s, i);} catch {}
                try {weeklyPlan[i].UnitsSold = przeplyw.Item3.GetEdgeWeight(2 * tyg + i, t);} catch {}
                if (i < tyg - 1)
                    try {weeklyPlan[i].UnitsStored = przeplyw.Item3.GetEdgeWeight(3 * tyg + i, i + tyg);} catch {}
            }
            
            return new PlanData
            {
                Value = -przeplyw.Item2,
                Quantity = przeplyw.Item1
            };
        }

        /// <summary>
        /// Część 2. zadania - zaplanowanie produkcji telewizorów dla wielu kontrahentów.
        /// </summary>
        /// <remarks>
        /// Do przeprowadzenia testów wyznaczających produkcję dającą maksymalny zysk wymagane jest jedynie zwrócenie obiektu <see cref="PlanData"/>.
        /// Testy weryfikujące plan wymagają przypisania tablicy z planem do parametru wyjściowego <see cref="weeklyPlan"/>.
        /// </remarks>
        /// <param name="production">
        /// Tablica obiektów zawierających informacje o produkcji fabryki w kolejnych tygodniach.
        /// Wartość pola <see cref="PlanData.Quantity"/> oznacza limit produkcji w danym tygodniu,
        /// a pola <see cref="PlanData.Value"/> - koszt produkcji jednej sztuki.
        /// </param>
        /// <param name="sales">
        /// Dwuwymiarowa tablica obiektów zawierających informacje o sprzedaży w kolejnych tygodniach.
        /// Pierwszy wymiar tablicy jest równy liczbie kontrahentów, zaś drugi - liczbie tygodni w planie.
        /// Wartości pola <see cref="PlanData.Quantity"/> oznaczają maksymalną sprzedaż w danym tygodniu,
        /// a pola <see cref="PlanData.Value"/> - cenę sprzedaży jednej sztuki.
        /// Każdy wiersz tablicy odpowiada jednemu kontrachentowi.
        /// </param>
        /// <param name="storageInfo">
        /// Obiekt zawierający informacje o magazynie.
        /// Wartość pola <see cref="PlanData.Quantity"/> oznacza pojemność magazynu,
        /// a pola <see cref="PlanData.Value"/> - koszt przechowania jednego telewizora w magazynie przez jeden tydzień.
        /// </param>
        /// <param name="weeklyPlan">
        /// Parametr wyjściowy, przez który powinien zostać zwrócony szczegółowy plan sprzedaży.
        /// </param>
        /// <returns>
        /// Obiekt <see cref="PlanData"/> opisujący wyznaczony plan.
        /// W polu <see cref="PlanData.Quantity"/> powinna znaleźć się optymalna liczba wyprodukowanych telewizorów,
        /// a w polu <see cref="PlanData.Value"/> - wyznaczony maksymalny zysk fabryki.
        /// </returns>
        public PlanData CreateComplexPlan(PlanData[] production, PlanData[,] sales, PlanData storageInfo,
            out WeeklyPlan[] weeklyPlan)
        {
            int T = production.Length;
            int k = sales.GetLength(0);
            int n = 4 * T + k + k * T + 2;
            NetworkWithCosts<int, double> siec = new(n);

            int s = n - 2, t = n - 1;
            Func<int, int> Produkcja = i => i;
            Func<int, int> Smietnik = i => T + i;
            Func<int, int> MagazynIn = i => 2 * T + i;
            Func<int, int> MagazynOut = i => 3 * T + i;
            Func<int, int, int> TygKontr = (i, kontr) => (4 + kontr) * T + i;
            Func<int, int> Kontrahent = i => s - k + i;
            
            // fabryka -> produkcja
            for (int i = 0; i < T; i++)
                siec.AddEdge(s, Produkcja(i), production[i].Quantity, production[i].Value);
            
            // produkcja -> ...
            for (int i = 0; i < T; i++)
            {
                siec.AddEdge(Produkcja(i), Smietnik(i), int.MaxValue, -production[i].Value); // TODO: przemyśleć
                if (i < T - 1)
                    siec.AddEdge(Produkcja(i), MagazynIn(i), int.MaxValue, 0);
                for (int j = 0; j < k; j++)
                    siec.AddEdge(Produkcja(i), TygKontr(i, j), int.MaxValue, 0);
            }
            
            // magazynIn -> magazynOut
            for (int i = 0; i < T - 1; i++)
                siec.AddEdge(MagazynIn(i), MagazynOut(i), storageInfo.Quantity, storageInfo.Value);
            
            // magazynOut -> ...
            for (int i = 0; i < T - 1; i++)
            {
                if (i < T - 2)
                    siec.AddEdge(MagazynOut(i), MagazynIn(i + 1), int.MaxValue, 0);
                for (int j = 0; j < k; j++)
                    siec.AddEdge(MagazynOut(i), TygKontr(i + 1, j), int.MaxValue, 0);
            }
            
            // tygKontr -> kontrahent
            for (int i = 0; i < T; i++)
                for (int j = 0; j < k; j++)
                    siec.AddEdge(TygKontr(i, j), Kontrahent(j), sales[j, i].Quantity, -sales[j, i].Value);
            
            // kontrahent -> ujście
            for (int j = 0; j < k; j++)
                siec.AddEdge(Kontrahent(j), t, int.MaxValue, 0);
            
            // smietnik -> ujscie
            for (int i = 0; i < T; i++)
                siec.AddEdge(Smietnik(i), t, int.MaxValue, 0);

            var przeplyw = Flows.MinCostMaxFlow(siec, s, t);
            var flow = przeplyw.Item3;
            var quantity = przeplyw.Item1;
            for (int i = 0; i < T; i++)
                try {quantity -= flow.GetEdgeWeight(Produkcja(i), Smietnik(i));} catch {}

            weeklyPlan = new WeeklyPlan[T];
            for (int i = 0; i < T; i++)
            {
                try {
                    weeklyPlan[i].UnitsProduced = flow.GetEdgeWeight(s, Produkcja(i));
                    try { weeklyPlan[i].UnitsProduced -= flow.GetEdgeWeight(Produkcja(i), Smietnik(i)); } catch { }
                } catch{}
                try {weeklyPlan[i].UnitsStored = flow.GetEdgeWeight(MagazynIn(i), MagazynOut(i));} catch { }
                weeklyPlan[i].UnitsSold = new int[k];
                for (int j = 0; j < k; j++)
                {
                    try{weeklyPlan[i].UnitsSold[j] = flow.GetEdgeWeight(TygKontr(i, j), Kontrahent(j));} catch{}
                }
            }
            
            return new PlanData
            {
                Value = -przeplyw.Item2,
                Quantity = quantity
            };
        }
    }
}