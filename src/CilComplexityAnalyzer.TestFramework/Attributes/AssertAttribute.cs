namespace CilComplexityAnalyzer.TestFramework.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class AssertAttribute : Attribute
{
    public string? CustomMessage { get; }
    public AssertAttribute(string? customMessage = null) => CustomMessage = customMessage;
}