namespace CilInstructionCounter;

using CilInjection.Core.Extensions;
using RunTime;
using Mono.Cecil;
using Mono.Cecil.Cil;

internal static class CilInjector
{
    public static void InjectCounterIncrementation(this ModuleDefinition module)
    {
        // 1. Bezpieczny import statycznego pola (sam wykrywa, że to long / Int64)
        var counterFieldRef = module.ImportStaticField(
            typeof(GlobalCounterContainer), 
            nameof(GlobalCounterContainer.InstructionCounter));

        // 2. Iniekcja CIL we wszystkich metodach modułu
        module.InjectInEveryMethodAtInstructionLevel((il, targetInstr) => new[]
        {
            il.Create(OpCodes.Ldsfld, counterFieldRef),
            il.Create(OpCodes.Ldc_I8, 1L),
            il.Create(OpCodes.Add),
            il.Create(OpCodes.Stsfld, counterFieldRef)
        });
    }
}