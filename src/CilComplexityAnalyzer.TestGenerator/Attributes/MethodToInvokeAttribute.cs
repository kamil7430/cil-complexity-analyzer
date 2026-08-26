using System;

namespace CilComplexityAnalyzer.TestGenerator.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class MethodToInvokeAttribute : Attribute
{
    public string? MethodName { get; } = null;
    public MethodToInvokeAttribute(string methodName) => MethodName = methodName;
}