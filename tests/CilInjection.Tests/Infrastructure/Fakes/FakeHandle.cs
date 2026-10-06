using CilInjection.Core.Abstractions;

namespace CilInjection.Tests.Infrastructure.Fakes;

public interface IFakeHandle : IHandle
{
}

internal class BaseCounterHandle : BaseRuntimeHandle, IFakeHandle
{
    protected override void OnBound()
    {
    }
}