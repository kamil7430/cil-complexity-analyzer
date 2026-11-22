namespace CilComplexityAnalyzer.Contract.SuiteAttributes;

[AttributeUsage(AttributeTargets.Class)]
public class ContainerizedAttribute(bool containerized) : Attribute
{
    public bool Containerized { get; } = containerized;
}