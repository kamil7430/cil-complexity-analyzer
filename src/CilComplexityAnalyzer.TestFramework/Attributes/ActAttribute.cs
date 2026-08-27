namespace CilComplexityAnalyzer.TestFramework.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class ActAttribute : Attribute
{
    public long MaxOps { get; } = 0;
    public long MaxMem { get; } = 0;
    public double TimeoutMultiplier { get; } = 0;
    public double Points { get; }

    public ActAttribute(long maxOps = -1, long maxMem = -1, double timeoutMultiplier = 1.0, double points = 0.0)
    {
        MaxOps = maxOps;
        MaxMem = maxMem;
        TimeoutMultiplier = timeoutMultiplier;
        Points = points;
    }
}