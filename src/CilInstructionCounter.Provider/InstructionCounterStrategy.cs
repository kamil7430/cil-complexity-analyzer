using System.Runtime.Loader;

namespace CilInstructionCounter;

using Mono.Cecil;
using CilInjection.Core;
using RunTime;

public class InstructionCounterStrategy : BaseInjectionStrategy<ICounterHandle>
{
    protected override Type RuntimeMarkerType => typeof(GlobalCounterContainer);

    public override void Inject(ModuleDefinition module)
    {
        module.InjectCounterIncrementation();
    }

    public override ICounterHandle BuildHandle(AssemblyLoadContext context)
    {
        return new CounterHandle(context );
    }
}