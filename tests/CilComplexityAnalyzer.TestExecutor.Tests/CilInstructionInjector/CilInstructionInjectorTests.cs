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
    
    [TestMethod]
    public void InjectCilToStudentSolution_WhenExecuted_IncrementsInstructionCounterCorrectly()
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
        var assembly = System.Reflection.Assembly.Load(testSuite.StudentSolutionAssemblyBytes!);
        var calculatorType = assembly.GetTypes()
            .FirstOrDefault(t => t.Name == "Calculator");
        Assert.IsNotNull(calculatorType, "Nie odnaleziono typu Calculator w załadowanym assembly.");
        var calculatorInstance = Activator.CreateInstance(calculatorType)!;
        var addMethod = calculatorType.GetMethod("Add")!;
        var result = (int)addMethod.Invoke(calculatorInstance, new object[] { 2, 3 })!;
        var containerType = assembly.GetTypes()
            .FirstOrDefault(t => t.Name == "<GlobalCounterContainer>");
        Assert.IsNotNull(containerType, "Nie odnaleziono typu <GlobalCounterContainer> w załadowanym assembly.");
        var counterField = containerType.GetField("__InstructionCounter", 
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)!;
        var instructionCount = (long)counterField.GetValue(null)!;

        // Assert
        Assert.AreEqual(5, result, "Metoda Add powinna zwrócić poprawny wynik logiczny (2 + 3 = 5).");
        Assert.IsTrue(instructionCount > 0, $"Licznik instrukcji powinien być większy od 0, a wynosił: {instructionCount}");
    }
}