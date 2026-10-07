using CilInjection.Core.Abstractions;

namespace CilInjection.Tests.Infrastructure.Fakes;

public class FakeStrategyWithRuntime(byte[] runtimeBytes, IWeaver weaver) : BaseInjectionStrategyWithRuntime<IFakeHandle>(runtimeBytes, weaver)
{
    protected override IFakeHandle CreateHandleInstance()
    {
        return new FakeHandle();
    }
}

