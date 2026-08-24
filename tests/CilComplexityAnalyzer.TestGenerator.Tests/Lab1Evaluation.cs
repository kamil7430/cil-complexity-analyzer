using CilComplexityAnalyzer.TestGenerator.Attributes;
namespace CilComplexityAnalyzer.TestGenerator.Tests;


[SolutionList(
    ids: new[] {"Student1", "Student2"},
    paths: new[] {"Submissions/student1.cs",  "Submissions/student1.cs"})]
[MethodToInvoke("Solve")]
[TestCase("Case1", new object[] {1, 2}, new object[]{3} )]
[TestCase("Case2", new object[] {2, 2}, new object[]{4})]
[TestCase("Case3", new object[] {3, 3}, new object[]{6})]
[TestCase("Case4", new object[] {3, 3}, new object[]{6})]
[TestCase("Case5", new object[] {3, 3}, new object[]{6})]
[TestCase("Case6", new object[] {3, 3}, new object[]{6})]
[TestCase("Case7", new object[] {3, 3}, new object[]{6})]
[TestCase("Case8", new object[] {3, 3}, new object[]{6})]
public partial class Lab1Evaluation
{
}

[SolutionList(
    ids: new[] {"Student1", "Student2"},
    paths: new[] {"Submissions/student1.cs",  "Submissions/student1.cs"})]
[MethodToInvoke("Solve")]
[TestCase("case1",new object[]{2, 3}, new object[]{5})]
[TestCase("case2",new object[]{2, 3}, new object[]{5})]
[TestCase("case3",new object[]{2, 3}, new object[]{5})]
public partial class SampleMathTests
{
}