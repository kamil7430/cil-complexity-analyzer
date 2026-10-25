using System;
using CilInjection.Core.Abstractions;
using CilInjection.Tests.Infrastructure.Fakes;

namespace CilInjection.Tests.Infrastructure.Fakes;

internal class TestableBaseStrategy : BaseInjectionStrategyWithRuntime<IFakeHandle>
{
    public TestableBaseStrategy(byte[] baseDllBytes, IWeaver? weaver = null) 
        : base(baseDllBytes, weaver ?? new FakeWeaver())
    {
    }

    protected override IFakeHandle CreateHandleInstance()
    {
        return new FakeHandle();
    }
}