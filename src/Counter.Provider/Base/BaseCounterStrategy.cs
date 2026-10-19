using CilInjection.Core.Abstractions;

namespace Counter.Provider.Base;

public class BaseCounterStrategy : BaseInjectionStrategyWithRuntime<ICounterHandle>
{
    public BaseCounterStrategy(ICounterWeaver weaver) : base(CounterMetadata.ResourceName, weaver)
    {
        var counterFieldRef = ResolveRuntimeField(CounterMetadata.TypeName, CounterMetadata.FieldName);
        weaver.Initialize(counterFieldRef);
    }

    protected override ICounterHandle CreateHandleInstance()
    {
        return new BaseCounterHandle();
    }
}