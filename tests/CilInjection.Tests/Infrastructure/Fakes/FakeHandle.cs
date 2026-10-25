using CilInjection.Core.Abstractions;

namespace CilInjection.Tests.Infrastructure.Fakes;

public interface IFakeHandle : IHandle
{
}

internal class FakeHandle : BaseRuntimeHandle, IFakeHandle
{
    protected override void OnBound()
    {
    }
}