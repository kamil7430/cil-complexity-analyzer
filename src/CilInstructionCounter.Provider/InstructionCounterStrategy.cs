using CilInjection.Core.Abstractions;
using System.Runtime.Loader;
using CilInstructionCounter.RunTime;

namespace CilInstructionCounter;

public class CounterStrategy(IWeaver weaver) : BaseInjectionStrategyWithRuntime(RuntimeDllPath, weaver)
{
    private static readonly string RuntimeDllPath = Path.Combine(AppContext.BaseDirectory, "Counter.RunTime.dll");
    
    public ICounterHandle BuildHandle(AssemblyLoadContext context)
    {
        return new CounterHandle(context );
    }
}