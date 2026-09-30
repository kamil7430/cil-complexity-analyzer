using CilInjection.Core.Abstractions;
using CilInstructionCounter.RunTime;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace CilInstructionCounter;

public class InstructionCounterWeaver(Type containerType, string fieldName) : BaseWeaver
{
    private readonly Type _containerType = containerType ?? throw new ArgumentNullException(nameof(containerType));
    private readonly string _fieldName = fieldName ?? throw new ArgumentNullException(nameof(fieldName));
    
    protected override void OnInject(IMethodInjectionContext methodInjectionContext, IMetadataContext metadataContext)
    {
        ArgumentNullException.ThrowIfNull(methodInjectionContext);
        ArgumentNullException.ThrowIfNull(metadataContext);

        var counterFieldRef = metadataContext.ImportField(_containerType, _fieldName);

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