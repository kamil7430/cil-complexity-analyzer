using CilInjection.Core.Abstractions;
using Counter.Provider.Base;
using Mono.Cecil.Cil;

namespace Counter.Provider.InstructionCostCounter;

public class InstructionCostCounterWeaver : BaseCounterWeaver
{
    protected override void OnInject(IMethodInjectionContext methodInjectionContext, IMetadataContext metadataContext)
    {
        ArgumentNullException.ThrowIfNull(methodInjectionContext);
        ArgumentNullException.ThrowIfNull(metadataContext);

        var counterFieldRef = metadataContext.ImportField(CounterFieldInfo);

        foreach (var instructionContext in methodInjectionContext.Contexts)
        {
            var instructionsToInject = CostInjectionBuilder.BuildIncrementInstructions(instructionContext.Target, counterFieldRef);
            
            instructionContext.AddBefore(instructionsToInject);
        }
    }
}