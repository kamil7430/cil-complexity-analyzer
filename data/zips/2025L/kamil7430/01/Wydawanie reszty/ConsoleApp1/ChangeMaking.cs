
using System;

namespace ASD
{

    class ChangeMaking
    {

        /// <summary>
        /// Metoda wyznacza rozwiązanie problemu wydawania reszty przy pomocy minimalnej liczby monet
        /// bez ograniczeń na liczbę monet danego rodzaju
        /// </summary>
        /// <param name="amount">Kwota reszty do wydania</param>
        /// <param name="coins">Dostępne nominały monet</param>
        /// <param name="change">Liczby monet danego nominału użytych przy wydawaniu reszty</param>
        /// <returns>Minimalna liczba monet potrzebnych do wydania reszty</returns>
        /// <remarks>
        /// coins[i]  - nominał monety i-tego rodzaju
        /// change[i] - liczba monet i-tego rodzaju (nominału) użyta w rozwiązaniu
        /// Jeśli dostepnymi monetami nie da się wydać danej kwoty to change = null,
        /// a metoda również zwraca null
        ///
        /// Wskazówka/wymaganie:
        /// Dodatkowa uzyta pamięć powinna (musi) być proporcjonalna do wartości amount ( czyli rzędu o(amount) )
        /// </remarks>
        public int? NoLimitsDynamic(int amount, int[] coins, out int[] change)
        {
            int?[] T = new int?[amount + 1];
            int[] P = new int[amount + 1];
            T[0] = 0;
            for (int i = 1; i <= amount; i++)
            {
                T[i] = null;
                for (int j = 0; j < coins.Length; j++)
                {
                    if (i - coins[j] >= 0)
                    {
                        int? c = 1 + T[i - coins[j]];
                        if (c != null && c < (T[i] == null ? int.MaxValue : T[i]))
                        {
                            T[i] = c;
                            P[i] = j;
                        }
                    }
                }
            }

            if (T[amount] == null)
            {
                change = null;
                return null;
            }

            change = new int[coins.Length];
            int kk = amount;
            while (kk > 0)
            {
                change[P[kk]]++;
                kk -= coins[P[kk]];
            }
            return T[amount];
        }

        /// <summary>
        /// Metoda wyznacza rozwiązanie problemu wydawania reszty przy pomocy minimalnej liczby monet
        /// z uwzględnieniem ograniczeń na liczbę monet danego rodzaju
        /// </summary>
        /// <param name="amount">Kwota reszty do wydania</param>
        /// <param name="coins">Dostępne nominały monet</param>
        /// <param name="limits">Liczba dostępnych monet danego nomimału</param>
        /// <param name="change">Liczby monet danego nominału użytych przy wydawaniu reszty</param>
        /// <returns>Minimalna liczba monet potrzebnych do wydania reszty</returns>
        /// <remarks>
        /// coins[i]  - nominał monety i-tego rodzaju
        /// limits[i] - dostepna liczba monet i-tego rodzaju (nominału)
        /// change[i] - liczba monet i-tego rodzaju (nominału) użyta w rozwiązaniu
        /// Jeśli dostepnymi monetami nie da się wydać danej kwoty to change = null,
        /// a metoda również zwraca null
        ///
        /// Wskazówka/wymaganie:
        /// Dodatkowa uzyta pamięć powinna (musi) być proporcjonalna do wartości iloczynu amount*(liczba rodzajów monet)
        /// ( czyli rzędu o(amount*(liczba rodzajów monet)) )
        /// </remarks>
        public int? Dynamic(int amount, int[] coins, int[] limits, out int[] change)
        {
            int?[,] T = new int?[coins.Length, amount + 1];
            int[,] P = new int[coins.Length, amount + 1];
            for (int i = 0; i < coins.Length; i++)
            {
                for (int j = 0; j <= amount; j++)
                {
                    T[i, j] = null;
                    if (i == 0)
                    {
                        if (j % coins[0] == 0 && j / coins[0] <= limits[0])
                            T[0, j] = P[0, j] = j / coins[0];
                    }
                    else
                    {
                        T[i, j] = T[i - 1, j];
                        for (int k = 1; k <= limits[i]; k++)
                        {
                            int index = j - k * coins[i];
                            if (index >= 0)
                            {
                                int? c = T[i - 1, index] + k;
                                if (c != null && c < (T[i, j] == null ? int.MaxValue : T[i, j]))
                                {
                                    T[i, j] = c;
                                    P[i, j] = k;
                                }
                            }
                        }
                    }
                }
            }

            if (T[coins.Length - 1, amount] == null)
            {
                change = null;
                return null;
            }
            
            change = new int[coins.Length];
            int kk = amount;
            for (int i = coins.Length - 1; i >= 0; i--)
            {
                change[i] += P[i, kk];
                kk -= P[i, kk] * coins[i];
            }
            return T[coins.Length - 1, amount];
        }

    }

}
