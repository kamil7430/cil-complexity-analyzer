using Mono.Cecil.Cil;
using Mono.Cecil.Cil;
using CilInjection.Core.Abstractions;
using Counter.Provider.Base;

public static class InstructionCostAnalyzer
{
    internal static IEnumerable<Instruction> GetCostInstructions(IReadOnlyInstructionContext context)
    {
        var opCode = context.OpCode;

        // --- LINIOWY DYNAMICZNY (Rozmiar znany tylko w Runtime na stosie) ---
        if (opCode == OpCodes.Newarr)
        {
            return GenerateLinearDynamicCost();
        }
        // TODO w przyszłości: cpblk, initblk (tutaj rozmiar jest głębiej na stosie)

        // --- LINIOWY STATYCZNY (Rozmiar znany teraz z Operandu) ---
        if (opCode == OpCodes.Ldstr && context.Operand is string str)
        {
            long stringCost = 1L + str.Length; 
            return GenerateConstantCost(stringCost);
        }

        // --- CZAS STAŁY (Domyślny) ---
        return GenerateConstantCost(1L);
    }

    /// <summary>
    /// Generuje instrukcje wrzucające stałą wartość (Int64) na stos CIL.
    /// </summary>
    private static IEnumerable<Instruction> GenerateConstantCost(long cost)
    {
        // Dla Int64 używamy Ldc_I8
        yield return Instruction.Create(OpCodes.Ldc_I8, cost);
    }

    /// <summary>
    /// Generuje instrukcje pobierające wartość ze szczytu stosu w locie (Runtime).
    /// </summary>
    private static IEnumerable<Instruction> GenerateLinearDynamicCost()
    {
        // Klonujemy wartość ze szczytu stosu (np. rozmiar tablicy podawany do newarr jako Int32)
        yield return Instruction.Create(OpCodes.Dup);
        
        // Ponieważ nasz licznik jest typu Int64, a rozmiar tablicy to Int32/Native Int,
        // musimy dokonać konwersji, żeby instrukcja Add zadziałała poprawnie.
        yield return Instruction.Create(OpCodes.Conv_I8);

    }
}