using CilComplexityAnalyzer.TestGenerator.Attributes;

namespace CilComplexityAnalyzer.TestGenerator.Tests;


[SolutionList(
    ids: new[] {"Student1", "Student2"},
    paths: new[] {"Submissions/student1.cs",  "Submissions/student1.cs"})]
[MethodToInvoke("Solve")]
[TestCase("Case1", new object[] {1, 2}, new object[]{3} )]
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
[TestCase("case4",new object[]{2, 3}, new object[]{5})]
[TestCase("smiecznyCase",[2, 3], [5])]
public partial class SampleMathTests
{
}