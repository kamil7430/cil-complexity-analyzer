using CilInjection.Core.Abstractions;
using System.Runtime.Loader;
using CilInstructionCounter.RunTime;

namespace CilInstructionCounter;

public class InstructionCounterStrategy(IWeaver weaver) : BaseInjectionStrategy(weaver)
{
    private static readonly Type ContainerType = typeof(GlobalCounterContainer);
    private static readonly string FieldName = nameof(GlobalCounterContainer.InstructionCounter);
    
    protected override Type RuntimeMarkerType => ContainerType;
    
    public ICounterHandle BuildHandle(AssemblyLoadContext context)
    {
        return new CounterHandle(context );
    }
}