using CilComplexityAnalyzer.TestExecutor.Contract;
using CilComplexityAnalyzer.Facade.Attributes;

namespace CilComplexityAnalyzer.Facade.Attributes.Tests;


[IsContainerized]
[Name("")] // opcjonalne
//[MethodToInvoke("SolveProblem")]
public class Lab01Evaluation : TestSuite // nazwa koncepcyjna
{
    [CaseSettings(InstructionCap = 1_000_000)]
    public class Case1 : TestCase
    {
        public override void Arrange() { /* ... */ }
        public override void Assert() { /* ... */ }
    }
    
    [CaseSettings(InstructionCap = 10_000_000, TimeoutMs = 30_000)]
    public class Case2 : TestCase
    {
        //private Graph _graph = new();
        public override void Arrange() { /* ... */ }
        public override void Assert() { /* ... */ }
    }

    
    [CaseSettings(InstructionCap = 20_000_000, TimeoutMs = 30_000, MemoryMb = 1024)]
    public class CaseN : TestCase
    {
        public override void Arrange() { /* ... */ }
        public override void Assert() { /* ... */ }
    }
}