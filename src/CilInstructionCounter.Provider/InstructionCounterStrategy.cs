namespace CilInstructionCounter;

using System.Runtime.Loader;
using CilInjection.Core;
using RunTime;

public class InstructionCounterStrategy(IWeaver weaver) : BaseInjectionStrategy<ICounterHandle>(weaver)
{
    public override Type RuntimeMarkerType => typeof(GlobalCounterContainer);

    public override ICounterHandle BuildHandle(AssemblyLoadContext context)
    {
        return new CounterHandle(context );
    }
}