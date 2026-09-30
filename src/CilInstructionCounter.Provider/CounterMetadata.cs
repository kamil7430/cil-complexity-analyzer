using Counter.RunTime;

namespace CilInstructionCounter;

internal static class CounterMetadata
{
    public const string TypeName = nameof(GlobalCounterContainer);
    public const string FieldName = nameof(GlobalCounterContainer.Counter);
    public const string GetMethodName = nameof(GlobalCounterContainer.GetCounter);
    public const string ResetMethodName = nameof(GlobalCounterContainer.ResetCounter);
}