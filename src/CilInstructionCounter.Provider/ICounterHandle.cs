using System.Reflection;
using System.Runtime.Loader;
using CilInjection.Core.Abstractions;
using CilInstructionCounter;


namespace CilInstructionCounter;

public interface ICounterHandle
{
    long GetCounter();
    void ResetCounter();
}

internal class CounterHandle : BaseRuntimeHandle, ICounterHandle
{
    private readonly Func<long> _getCounter;
    private readonly Action _resetCounter;

    public CounterHandle(AssemblyLoadContext context) 
        : base(context, CounterMetadata.TypeName, OtherTypeMetadata.TypeName) // Możesz podać wiele typów po przecinku!
    {
        _getCounter = BindMethod<Func<long>>(CounterMetadata.TypeName, CounterMetadata.GetMethodName);
        _resetCounter = BindMethod<Action>(CounterMetadata.TypeName, CounterMetadata.ResetMethodName);
    }

    public long GetCounter() => _getCounter();
    public void ResetCounter() => _resetCounter();
}