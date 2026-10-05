using System.Diagnostics.Metrics;
using CilInjection.Core.Abstractions;
using System.Runtime.Loader;
using Counter.RunTime;
using Mono.Cecil;

namespace CilInstructionCounter;

public class BaseCounterStrategy : BaseInjectionStrategyWithRuntime
{
    private static readonly string RuntimeDllPath = Path.Combine(AppContext.BaseDirectory, "Counter.RunTime.dll");
    
    public BaseCounterStrategy(ICounterWeaver weaver) : base(RuntimeDllPath, weaver)
    {
        var counterFieldRef = ResolveRuntimeField(CounterMetadata.TypeName, CounterMetadata.FieldName);
        weaver.Initialize(counterFieldRef);
    }
    
    public ICounterHandle CreateHandle(AssemblyLoadContext context)
    {
        return CreateHandle(context, () => new CounterHandle());
    }
}