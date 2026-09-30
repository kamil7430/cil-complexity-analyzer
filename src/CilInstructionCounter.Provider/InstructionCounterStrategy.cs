using System.Diagnostics.Metrics;
using CilInjection.Core.Abstractions;
using System.Runtime.Loader;
using Counter.RunTime;
using Mono.Cecil;

namespace CilInstructionCounter;

public class CounterStrategy : BaseInjectionStrategyWithRuntime
{
    private static readonly string RuntimeDllPath = Path.Combine(AppContext.BaseDirectory, "Counter.RunTime.dll");

    private static readonly Type ContainerType = typeof(GlobalCounterContainer);
    private static readonly Field CounterField = (GlobalCounterContainer.Counter);
    
    public CounterStrategy(ICounterWeaver weaver) : base(RuntimeDllPath, weaver)
    {
        var counterFieldRef = ResolveRuntimeField(
            originalTypeName: CounterMetadata.TypeName, 
            fieldName: CounterMetadata.FieldName
        );
        weaver.Initialize(counterFieldRef);
    }
    
    public ICounterHandle BuildHandle(AssemblyLoadContext context)
    {
        return new CounterHandle(context );
    }
}