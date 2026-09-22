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
}