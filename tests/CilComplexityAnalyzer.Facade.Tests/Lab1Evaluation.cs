using CilComplexityAnalyzer.TestExecutor.Contract;
using CilComplexityAnalyzer.Facade.Tests.Submissions;

namespace CilComplexityAnalyzer.Facade.Tests;

[TestSuite]
[StudentSolution(typeof(Student1Solution))]
public partial class Lab01Evaluation : TestSuite
{
    public class Case1 : TestCase
    {
        public override TestCaseSettings Settings()  { /* ... */ }
        public override void Arrange() { /* ... */ }
        public override void Act() { /* ... */ }
        public override void Assert() { /* ... */ }
    }
    
    public class Case2 : TestCase
    {
        // private Graph _graph = new();
        public override TestCaseSettings Settings()  { /* ... */ }
        public override void Arrange()
        {
            // ...
            // _graph = new Graph();
            // ...
        }
        public override void Act() { /* ... */ }
        public override void Assert() { /* ... */ }
    }
    
    // ...
    
    public class CaseN : TestCase
    {
        public override TestCaseSettings Settings()  { /* ... */ }
        public override void Arrange() { /* ... */ }
        public override void Act() { /* ... */ }
        public override void Assert() { /* ... */ }
    }
}

// generated tests
/*
    public async Task CaseN() 
    {
        _executor.BeginExecution();
        var result = await _executor.GetResult(N);
        Assert.Is(...);
    }
*/