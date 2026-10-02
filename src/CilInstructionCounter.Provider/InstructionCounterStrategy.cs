using System.Diagnostics.Metrics;
using CilInjection.Core.Abstractions;
using System.Runtime.Loader;
using Counter.RunTime;
using Mono.Cecil;

namespace CilInstructionCounter;

public class CounterStrategy : BaseInjectionStrategyWithRuntime
{
    private static readonly string RuntimeDllPath = Path.Combine(AppContext.BaseDirectory, "Counter.RunTime.dll");
    
    public CounterStrategy(ICounterWeaver weaver) : base(RuntimeDllPath, weaver)
    {
        var counterFieldRef = ResolveRuntimeField(CounterMetadata.TypeName, CounterMetadata.FieldName);
        weaver.Initialize(counterFieldRef);
    }
    
    public ICounterHandle BuildHandle(AssemblyLoadContext context)
    {
        return new CounterHandle(context );
    }
}