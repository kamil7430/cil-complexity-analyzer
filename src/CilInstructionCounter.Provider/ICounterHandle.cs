using System.Reflection;
using System.Runtime.Loader;
using CilInjection.Core.Abstractions;
using CilInstructionCounter;


namespace CilInstructionCounter;

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