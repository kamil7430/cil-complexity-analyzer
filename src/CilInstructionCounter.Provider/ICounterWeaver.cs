

using Mono.Cecil;

namespace CilInstructionCounter;

using CilInjection.Core.Abstractions;

public interface ICounterWeaver : IWeaver
{
    ICounterWeaver Initialize(FieldReference counterFieldInfo);
}

public class BaseCounterWeaver : BaseWeaver, ICounterWeaver
{
    private FieldReference? _counterFieldReference;

    protected FieldReference CounterFieldInfo => 
        _counterFieldReference ?? throw new InvalidOperationException("Weaver not initialized.");
    
    public ICounterWeaver Initialize(FieldReference counterFieldInfo)
    {
        ArgumentNullException.ThrowIfNull(counterFieldInfo);
        _counterFieldReference = counterFieldInfo;
        return this;
    }
}