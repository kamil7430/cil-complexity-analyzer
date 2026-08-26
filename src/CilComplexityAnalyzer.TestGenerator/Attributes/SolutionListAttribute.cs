using System;

namespace CilComplexityAnalyzer.TestGenerator.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class SolutionListAttribute(string[] ids, string[] paths) : Attribute
{
    public string[] Ids { get; } = ids;
    public string[] Paths { get; } = paths;
}