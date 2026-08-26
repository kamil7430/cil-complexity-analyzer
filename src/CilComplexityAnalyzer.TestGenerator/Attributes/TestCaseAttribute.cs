using System;

namespace CilComplexityAnalyzer.TestGenerator.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class TestCaseAttribute : Attribute
{
    public string Name { get; }
    public object[] Input { get; }
    public object[] Output { get; }

    public TestCaseAttribute(string name, object[] input, object[] output)
    {
        Name = name;
        Input = input;
        Output = output;
    }
}