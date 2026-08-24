using System;
namespace CilComplexityAnalyzer.TestGenerator.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class SolutionListAttribute : Attribute
{
    public string[] Ids { get; }
    public string[] Paths { get; }
    public SolutionListAttribute(string[] ids, string[] paths)
    {
        Ids = ids;
        Paths = paths;
    }
}