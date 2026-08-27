namespace CilComplexityAnalyzer.TestFramework.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class TestCaseAttribute(string? description = null) : Attribute
{
    public string? Description { get; } = description;
}