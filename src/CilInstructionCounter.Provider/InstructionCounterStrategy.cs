using CilInjection.Core.Abstractions;

namespace CilInstructionCounter;

public class BaseCounterStrategy : BaseInjectionStrategyWithRuntime<ICounterHandle>
{
    private static readonly string ResourceName = "Counter.RunTime.dll";
    
    public BaseCounterStrategy(ICounterWeaver weaver) : base(ResourceName, weaver)
    {
        var counterFieldRef = ResolveRuntimeField(CounterMetadata.TypeName, CounterMetadata.FieldName);
        weaver.Initialize(counterFieldRef);
    }

    protected override ICounterHandle CreateHandleInstance()
    {
        return new BaseCounterHandle();
    }
}