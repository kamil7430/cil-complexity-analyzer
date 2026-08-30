namespace CilComplexityAnalyzer.TestGenerator.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class MethodToInvokeAttribute(string methodName) : Attribute
{
    public string? MethodName { get; } = methodName;
}