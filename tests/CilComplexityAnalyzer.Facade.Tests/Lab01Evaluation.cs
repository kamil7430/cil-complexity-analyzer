using CilComplexityAnalyzer.Contract;
using CilComplexityAnalyzer.Contract.CaseAttributes;
using CilComplexityAnalyzer.Contract.SuiteAttributes;
using CilComplexityAnalyzer.Facade.Tests.Submissions;

namespace CilComplexityAnalyzer.Facade.Tests;

[TestSuite]
[Containerized(true)]
[StudentSolution(typeof(Student1Solution))]
public partial class Lab01Evaluation : TestSuite
{
    [TestCase]
    [InstructionCap(1_000_000)]
    [TimeoutMs(30_000)]
    [MemoryMb(512)]
    public partial class Case1 : TestCase
    {
        private int _liczba = 3;

        public override void Arrange()
        {
            _liczba = 5;
        }

        public override void Act()
        {
            _liczba *= _liczba;
        }

        public override void Assert()
        {
            IsTrue(_liczba > 0, "liczba większa od zera");
            IsTrue(_liczba == 25, "liczba równa 25");
        }
    }
    
    [TestCase]
    [InstructionCap(1_000_000)]
    [TimeoutMs(30_000)]
    [MemoryMb(512)]
    public partial class Case2 : TestCase
    {
        public override void Arrange() { }
        
        public override void Act() { }

        public override void Assert()
        {
            IsTrue(0 < 1, "0 < 1");
            IsTrue(1 < 1, "1 < 1");
            IsTrue(2 < 1, "2 < 1");
        }
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