using Counter.RunTime;

namespace Counter.Provider.Base;

internal static class CounterMetadata
{
    public static readonly string ResourceName = "Counter.Runtime";
    public static readonly string TypeName = typeof(GlobalCounterContainer).FullName!;
    public static readonly string FieldName = nameof(GlobalCounterContainer.Counter);
    public static readonly string GetMethodName = nameof(GlobalCounterContainer.GetCounter);
    public static readonly string ResetMethodName = nameof(GlobalCounterContainer.ResetCounter);
} 