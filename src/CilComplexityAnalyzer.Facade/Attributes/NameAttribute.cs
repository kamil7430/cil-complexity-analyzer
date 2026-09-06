namespace CilComplexityAnalyzer.Facade.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class NameAttribute(string value = "") : Attribute
{
    public string Value { get; } = value;
}