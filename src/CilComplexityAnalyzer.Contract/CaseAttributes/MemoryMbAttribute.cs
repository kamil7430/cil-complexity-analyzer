namespace CilComplexityAnalyzer.Contract.CaseAttributes;

[AttributeUsage(AttributeTargets.Class)]
public class MemoryMbAttribute(long memoryMb) : Attribute
{
    public long MemoryMb { get; } = memoryMb;
}