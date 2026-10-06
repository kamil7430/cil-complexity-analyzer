using CilInjection.Core.Abstractions;

namespace CilInjection.Tests.Infrastructure.Fakes;

using System;
using CilInjection.Core;
using Mono.Cecil;

public class FakeInjectionStrategy0 : BaseInjectionStrategy
{
    public FakeInjectionStrategy0(Type runtimeMarkerType) 
        : base(new FakeWeaver())
    {
        RuntimeMarkerType = runtimeMarkerType ?? throw new ArgumentNullException(nameof(runtimeMarkerType));
    }
}

public class FakeStrategyWithRuntime(string runtimeName, IWeaver weaver) : BaseInjectionStrategyWithRuntime<IFakeHandle>(runtimeName, weaver)


    protected override ICounterHandle CreateHandleInstance()
    {
        return new BaseCounterHandle();
    }
}

