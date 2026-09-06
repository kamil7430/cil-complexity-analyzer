using Microsoft.CodeAnalysis.Operations;

namespace CilComplexityAnalyzer.Facade.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class MethodToInvokeAttribute(string methodName) : Attribute
{
    public string MethodName { get; } = methodName ?? throw new ArgumentNullException(nameof(methodName));
}