namespace CilInjecting.Tests.Infrastructure.Fakes;

using System;
using CilInjection.Core;
using Mono.Cecil;

public class FakeInjectionStrategy : BaseInjectionStrategy
{
    public override Type RuntimeMarkerType { get; }

    public FakeInjectionStrategy(Type runtimeMarkerType) 
        : base(new FakeWeaver())
    {
        RuntimeMarkerType = runtimeMarkerType ?? throw new ArgumentNullException(nameof(runtimeMarkerType));
    }
}