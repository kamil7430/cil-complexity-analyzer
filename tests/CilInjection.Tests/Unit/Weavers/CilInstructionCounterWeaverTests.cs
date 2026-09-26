namespace CilInstructionCounter.Tests;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Assertions;
using CilInjecting.Tests.Infrastructure.Extensions;
using CilInjecting.Tests.Infrastructure.Loaders;
using CilInstructionCounter.RunTime;

[TestClass]
public class InstructionCounterWeaverTests
{
    [TestMethod]
    [DataRow("Basic/SimpleCalculator.cs")]
    [DataRow("Basic/EmptyMethods.cs")]
    [DataRow("ControlFlow/ConditionalBranches.cs")]
    [DataRow("Exceptions/TryCatchFinally.cs")]
    public void Inject_ShouldPrependCounterSequence_BeforeEveryInstructionInModule(string relativeFilePath)
    {
        // Arrange
        using var originalModule = TestTargetLoader.CompileSourceToModule(relativeFilePath);
        using var targetModule = TestTargetLoader.CompileSourceToModule(relativeFilePath);
        
        var weaver = new InstructionCounterWeaver();

        // Act
        weaver.Inject(targetModule);

        // Assert
        targetModule.ShouldHaveInjectedCounterSequenceComparedTo(originalModule);
    }
    
    [TestMethod]
    [DataRow("ControlFlow/ConditionalBranches.cs")]
    [DataRow("ControlFlow/LoopControlFlow.cs")]
    [DataRow("ControlFlow/ShortBranchExpansion.cs")]
    [DataRow("ControlFlow/SwitchStatements.cs")]
    public void Inject_ShouldRetargetBranchTargets_ToStartOfInjectedSequence(string relativeFilePath)
    {
        // Arrange
        using var originalModule = TestTargetLoader.CompileSourceToModule(relativeFilePath);
        using var targetModule = TestTargetLoader.CompileSourceToModule(relativeFilePath);

        var weaver = new InstructionCounterWeaver();

        // Act
        weaver.Inject(targetModule);

        // Assert
        targetModule.ShouldHaveRetargetedBranchTargetsComparedTo(originalModule);
    }
    
    [TestMethod]
    [DataRow("Exceptions/TryCatchFinally.cs")]
    [DataRow("Exceptions/MultipleCatches.cs")]
    [DataRow("Exceptions/ExceptionFilters.cs")]
    [DataRow("Exceptions/NestedExceptions.cs")]
    public void Inject_ShouldRetargetExceptionHandlers_ToStartOfInjectedSequence(string relativeFilePath)
    {
        // Arrange
        using var originalModule = TestTargetLoader.CompileSourceToModule(relativeFilePath);
        using var targetModule = TestTargetLoader.CompileSourceToModule(relativeFilePath);

        var weaver = new InstructionCounterWeaver();

        // Act
        weaver.Inject(targetModule);

        // Assert
        targetModule.ShouldHaveRetargetedExceptionHandlersComparedTo(originalModule);
    }
    
    [TestMethod]
    public void Inject_WhenExecutedInRuntime_IncrementsGlobalCounterCorrectly()
    {
        // Arrange
        using var module = TestTargetLoader.CompileSourceToModule("Basic/SimpleCalculator.cs");
        var weaver = new InstructionCounterWeaver();
        
        weaver.Inject(module);

        var assembly = module.ToAssembly();
        GlobalCounterContainer.ResetCounter();

        // Act
        var result = assembly.InvokeMethod<int>("TestTargets.Basic.SimpleCalculator", "Add", 2, 3);

        // Assert
        Assert.AreEqual(5, result, "Metoda Add powinna zwrócić poprawny wynik logiczny (2 + 3 = 5).");
        Assert.AreEqual(
            12L, 
            GlobalCounterContainer.GetCounter(), 
            $"Licznik instrukcji powinien wynosić 12, a wynosił: {GlobalCounterContainer.GetCounter()}");
    }
}