namespace CilInstructionCounter;

using System.Reflection;
using CilInjection.Core.Abstractions;
using Mono.Cecil.Cil;

public class InstructionCounterWeaver : BaseCounterWeaver
{
    protected override void OnInject(IMethodInjectionContext methodInjectionContext, IMetadataContext metadataContext)
    {
        ArgumentNullException.ThrowIfNull(methodInjectionContext);
        ArgumentNullException.ThrowIfNull(metadataContext);

        var counterFieldRef = metadataContext.ImportField(CounterFieldInfo);

        foreach (var instructionContext in methodInjectionContext.Contexts)
        {
            instructionContext.AddBefore(
            [
                Instruction.Create(OpCodes.Ldsfld, counterFieldRef),
                Instruction.Create(OpCodes.Ldc_I8, 1L),
                Instruction.Create(OpCodes.Add),
                Instruction.Create(OpCodes.Stsfld, counterFieldRef)
            ]);
        }
    }
}