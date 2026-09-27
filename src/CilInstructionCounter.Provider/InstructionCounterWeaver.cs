using CilInjection.Core;

namespace CilInstructionCounter;

using Mono.Cecil;

public class InstructionCounterWeaver : IWeaver
{
    public void Inject(ModuleDefinition module)
    {
        module.InjectCounterIncrementation();
    }
}