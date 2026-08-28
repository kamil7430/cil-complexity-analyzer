using CilComplexityAnalyzer.TestExecutor.Contract;
using CilComplexityAnalyzer.TestGenerator.Attributes;

namespace CilComplexityAnalyzer.TestGenerator.Tests;


[IsContainerized]
[Name("")] // opcjonalne
public class Lab01Evaluation : TestSuiteBase // nazwa koncepcyjna
{
    [CaseSettings(InstructionCap = 1_000_000)]
    public class Case1 : TestCaseBase
    {
        public override void Arrange() { /* ... */ }
        public override void Assert() { /* ... */ }
    }
    
    [CaseSettings(InstructionCap = 10_000_000)]
    public class Case2 : TestCaseBase
    {
        //private Graph _graph = new();
        public override void Arrange() { /* ... */ }
        public override void Assert() { /* ... */ }
    }

    
    [CaseSettings(InstructionCap = 20_000_000)]
    public class CaseN : TestCaseBase
    {
        public override void Arrange() { /* ... */ }
        public override void Assert() { /* ... */ }
    }
}