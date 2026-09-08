using Microsoft.VisualStudio.TestTools.UnitTesting;
using CilComplexityAnalyzer.TestExecutor.Tests.CilInstructionInjector.Infrastructure;
using CilComplexityAnalyzer.TestExecutor.Tests.CilInstructionInjector.Assertions;

namespace CilComplexityAnalyzer.TestExecutor.Tests.CilInstructionInjector;

[TestClass]
public class CilInstructionInjectorTests
{
    [TestMethod]
    public void InjectCilToStudentSolution_WhenAssemblyBytesIsNull_ThrowsInvalidOperationException()
    {
        // Arrange
        var testSuite = CilTestSuiteBuilder.Create()
            .WithoutStudentAssembly()
            .Build();

        // Act & Assert
        testSuite.ShouldThrowInvalidOperationExceptionWhenInjecting();
    }

    [TestMethod]
    public void InjectCilToStudentSolution_CreatesGlobalCounterContainerAndField()
    {
        // Arrange
        var testSuite = CilTestSuiteBuilder.Create()
            .WithStudentCode(@"
                namespace StudentSolution;
                public class Calculator {
                    public int Add(int a, int b) => a + b;
                }")
            .Build();

        // Act
        testSuite.InjectCilToStudentSolution();

        // Assert
        testSuite.ShouldContainGlobalCounterContainer()
            .ShouldHaveStaticLongCounterField();
    }
    
    [TestMethod]
    public void InjectCilToStudentSolution_InjectsCounterInstructionSequence()
    {
        // Arrange
        var testSuite = CilTestSuiteBuilder.Create()
            .WithStudentCode(@"
                namespace StudentSolution;
                public class Calculator
                {
                    public int Add(int a, int b)
                    {
                        return a + b;
                    }
                }")
            .Build();

        // Act
        testSuite.InjectCilToStudentSolution();

        // Assert
        testSuite.ShouldHaveInstructionSeqeunceInjected(
            className: "Calculator", 
            methodName: "Add"
        );
    }
}