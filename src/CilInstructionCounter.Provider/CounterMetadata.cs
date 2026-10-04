using Counter.RunTime;

namespace CilInstructionCounter;

internal class CounterMetadata
{
    public static readonly string StaticTypeName = typeof(GlobalCounterContainer).FullName!;
    public static readonly string FieldName = nameof(GlobalCounterContainer.Counter);
    public static readonly string GetMethodName = nameof(GlobalCounterContainer.GetCounter);
    public static readonly string ResetMethodName = nameof(GlobalCounterContainer.ResetCounter);
} 