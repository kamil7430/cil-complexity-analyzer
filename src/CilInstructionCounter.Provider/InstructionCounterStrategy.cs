using System.Diagnostics.Metrics;
using CilInjection.Core.Abstractions;
using System.Runtime.Loader;
using Counter.RunTime;
using Mono.Cecil;

namespace CilInstructionCounter;

public class BaseCounterStrategy : BaseInjectionStrategyWithRuntime<ICounterHandle>
{
    private static readonly string RuntimeDllPath = Path.Combine(AppContext.BaseDirectory, "Counter.RunTime.dll");
    
    public BaseCounterStrategy(ICounterWeaver weaver) : base(RuntimeDllPath, weaver)
    {
        var counterFieldRef = ResolveRuntimeField(CounterMetadata.TypeName, CounterMetadata.FieldName);
        weaver.Initialize(counterFieldRef);
    }

    protected override ICounterHandle CreateHandleInstance()
    {
        return new BaseCounterHandle();
    }
}