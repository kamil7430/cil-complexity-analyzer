using CilInjection.Core.Abstractions;

namespace Counter.Provider.Base;

public interface ICounterHandle : IHandle
{
    long GetCounter();
    void ResetCounter();
}

internal class BaseCounterHandle : BaseRuntimeHandle, ICounterHandle
{
    private Func<long>? _getCounter;
    private Action? _resetCounter;

    protected override void OnBound()
    {
        _getCounter = BindMethod<Func<long>>(CounterMetadata.TypeName, CounterMetadata.GetMethodName);
        _resetCounter = BindMethod<Action>(CounterMetadata.TypeName, CounterMetadata.ResetMethodName);
    }
    
    public long GetCounter() => _getCounter?.Invoke() ?? 0;
    public void ResetCounter() => _resetCounter();
}