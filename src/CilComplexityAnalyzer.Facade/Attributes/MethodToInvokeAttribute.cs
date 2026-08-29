using System;

namespace CilComplexityAnalyzer.TestGenerator.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class MethodToInvokeAttribute : Attribute
{
    public string? MethodName { get; }

    public MethodToInvokeAttribute(string methodName)
    {
        if (string.IsNullOrEmpty(methodName))
            throw new ArgumentException(
                "Method name cannot be null or empty " + methodName);
        MethodName = methodName;
    } 
}