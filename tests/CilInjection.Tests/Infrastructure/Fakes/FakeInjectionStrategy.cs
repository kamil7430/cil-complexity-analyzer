using CilInjection.Core.Abstractions;

namespace CilInjection.Tests.Infrastructure.Fakes;

using System;
using CilInjection.Core;
using Mono.Cecil;

public class FakeStrategyWithRuntime(string runtimeName, IWeaver weaver) : BaseInjectionStrategyWithRuntime<IFakeHandle>(runtimeName, weaver)
{
    protected override IFakeHandle CreateHandleInstance()
    {
        return new FakeHandle();
    }
}

