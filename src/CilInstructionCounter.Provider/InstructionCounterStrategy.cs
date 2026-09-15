

namespace CilInstructionCounter;

using Mono.Cecil;
using CilInjection.Core;
using CilInjection.Core.Extensions;
using RunTime;

public class InstructionCounterStrategy : IInjectionStrategy
{
    public void Inject(ModuleDefinition module)
    {
        // Logika Mono.Cecil...
    }

    public void LoadRuntime(System.Runtime.Loader.AssemblyLoadContext context)
    {      
        context.LoadRuntimeFromType(typeof(GlobalCounterContainer));
    }

    public object CreateHandle(System.Runtime.Loader.AssemblyLoadContext context)
    {
        throw new NotImplementedException();
    }
}