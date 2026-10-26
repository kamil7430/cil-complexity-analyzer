using Mono.Cecil.Cil;
using CilInjection.Core.Abstractions;
using Counter.Provider.Base;
using Mono.Cecil;

namespace Counter.Provider.InstructionCostCounter;

public static class CostInjectionBuilder
{
    public static IEnumerable<Instruction> BuildIncrementInstructions(IReadOnlyInstructionContext context, FieldReference counterFieldRef)
    {
        // Załadowanie obecnej wartości licznika na stos
        yield return Instruction.Create(OpCodes.Ldsfld, counterFieldRef);

        // Wrzucenie na stos obliczonego KOSZT (jako Int64)
        var costCalculationInstructions = InstructionCostAnalyzer.GetCostInstructions(context);
        foreach (var inst in costCalculationInstructions)
        {
            yield return inst;
        }

        yield return Instruction.Create(OpCodes.Add);
        yield return Instruction.Create(OpCodes.Stsfld, counterFieldRef);
    }
}