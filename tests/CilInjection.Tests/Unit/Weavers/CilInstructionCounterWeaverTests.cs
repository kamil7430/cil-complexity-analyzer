namespace CilInstructionCounter.Tests;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Assertions;
using CilInjecting.Tests.Infrastructure.Compilers;

[TestClass]
public class InstructionCounterWeaverTests
{
    [TestMethod]
    public void Inject_ShouldPrependCounterSequence_BeforeEveryInstructionInModule()
    {
        // Arrange
        using var originalModule = TestAssemblyGenerator.CreateDefaultModule();
        using var targetModule = TestAssemblyGenerator.CreateDefaultModule();
        
        var weaver = new InstructionCounterWeaver();

        // Act
        weaver.Inject(targetModule);

        // Assert
        targetModule.ShouldHaveInjectedCounterSequenceComparedTo(originalModule);
    }
    
    [TestMethod]
    public void Inject_ShouldRetargetBranchTargets_ToStartOfInjectedSequence()
    {
        const string sourceCode = @"
        namespace TestTarget;

        public class BranchClass
        {
            public int EvaluateCondition(bool condition)
            {
                if (condition)
                {
                    return 10;
                }
                return 20;
            }

            public string EvaluateSwitch(int option)
            {
                return option switch
                {
                    1 => ""One"",
                    2 => ""Two"",
                    _ => ""Other""
                };
            }
        }";
        
        using var originalModule = TestAssemblyGenerator.CompileToModule(sourceCode);
        using var targetModule = TestAssemblyGenerator.CompileToModule(sourceCode);

        var weaver = new InstructionCounterWeaver();

        // Act
        weaver.Inject(targetModule);

        // Assert
        targetModule.ShouldHaveRetargetedBranchTargetsComparedTo(originalModule);
    }
}