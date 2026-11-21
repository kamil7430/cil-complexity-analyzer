namespace CilComplexityAnalyzer.Contract.CaseAttributes;

[AttributeUsage(AttributeTargets.Class)]
public class TimeoutMsAttribute(long timeoutMs) : Attribute
{
    public long TimeoutMs { get; } = timeoutMs;
}