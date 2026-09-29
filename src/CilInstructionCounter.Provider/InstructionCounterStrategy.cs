namespace CilInstructionCounter;

using System.Runtime.Loader;
using CilInjection.Core;
using RunTime;

public class InstructionCounterStrategy : BaseInjectionStrategy<ICounterHandle>
{
    public override Type RuntimeMarkerType => typeof(GlobalCounterContainer);

    public InstructionCounterStrategy() 
        : base(new InstructionCounterWeaver()) // Przekazanie wewnętrznego weavera
    {
    }
    
    public override ICounterHandle BuildHandle(AssemblyLoadContext context)
    {
        return new CounterHandle(context );
    }
}