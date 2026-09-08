using System.Text;
using CilComplexityAnalyzer.Facade.Attributes;
using CilComplexityAnalyzer.TestExecutor.Contract;
using CilComplexityAnalyzer.Facade.Tests.Submissions;

namespace CilComplexityAnalyzer.Facade.Tests;

//[TestSuite]
[StudentSolution(typeof(Student1Solution))]
public partial class Lab01Evaluation : TestSuite
{
    
    public class Case1 : TestCase
    {
        public override int TestNumber() => 0;

        public override TestCaseSettings Settings() => new  TestCaseSettings();
        public override void Arrange() {  }
        public override void Act() {  }
        public override void Assert() {  }
    }
    
    public class Case2 : TestCase
    {
        public override int TestNumber() => 1;

        public override TestCaseSettings Settings()  => new  TestCaseSettings();
        public override void Arrange() { }
        public override void Act() {  }
        public override void Assert() {  }
    }

    public override TestSuiteSettings? Settings()
    {
        return new TestSuiteSettings
        {
            Containerized = false
        };
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