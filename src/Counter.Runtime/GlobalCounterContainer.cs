namespace Counter.RunTime;

/// <summary>
/// Centralny kontener pamięci służący do zliczania.
/// </summary>
public static class GlobalCounterContainer
{
    /// <summary>
    /// Statyczny licznik wykonanych instrukcji/kosztu.
    /// Wstrzyknięty kod CIL wykonuje na nim bezpośrednie operacje ldsfld / stsfld.
    /// </summary>
    public static long Counter;

    /// <summary>
    /// Pobiera aktualną wartość.
    /// </summary>
    public static long GetCounter()
    {
        return Counter;
    }
 
    /// <summary>
    /// Zeruje stan licznika.
    /// </summary>
    public static void ResetCounter()
    {
        Counter = 0L;
    }
}