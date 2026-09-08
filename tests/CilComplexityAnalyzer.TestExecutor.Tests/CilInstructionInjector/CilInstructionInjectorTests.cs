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
        testSuite.ShouldHaveInstructionSequenceInjected(
            className: "Calculator", 
            methodName: "Add"
        );
    }
    
    [TestMethod]
    public void InjectCilToStudentSolution_RedirectsBranchTargetsToInjectedCounters()
    {
        // Arrange
        const string studentCode = @"
        namespace StudentSolution;
        public class Calculator
        {
            public int Max(int a, int b)
            {
                if (a > b)
                {
                    return a;
                }
                return b;
            }
        }";

        var originalTestSuite = CilTestSuiteBuilder.Create()
            .WithStudentCode(studentCode)
            .Build();

        var testSuite = CilTestSuiteBuilder.Create()
            .WithStudentCode(studentCode)
            .Build();

        // Act
        testSuite.InjectCilToStudentSolution();

        // Assert
        testSuite.ShouldRedirectBranchTargetsToCorrectInjectedCounter(
            originalTestSuite: originalTestSuite,
            className: "Calculator",
            methodName: "Max"
        );
    }
}