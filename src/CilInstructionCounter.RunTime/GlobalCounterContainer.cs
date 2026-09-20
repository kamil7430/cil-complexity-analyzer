namespace CilInstructionCounter.RunTime;

/// <summary>
/// Centralny kontener pamięci dla zliczania instrukcji CIL.
/// Wszystkie zinstrumentowane pliki .dll wchodzące w skład testu
/// modyfikują bezpośrednio pole tej klasy.
/// </summary>
public static class GlobalCounterContainer
{
    /// <summary>
    /// Statyczny licznik wykonanych instrukcji/kosztu.
    /// Wstrzyknięty kod CIL wykonuje na nim bezpośrednie operacje ldsfld / stsfld.
    /// </summary>
    public static long InstructionCounter;

    /// <summary>
    /// Pobiera aktualną wartość zliczonych instrukcji.
    /// </summary>
    public static long GetCounter()
    {
        return InstructionCounter;
    }
 
    /// <summary>
    /// Zeruje stan licznika przed rozpoczęciem nowego przypadku testowego.
    /// </summary>
    public static void ResetCounter()
    {
        InstructionCounter = 0L;
    }
}