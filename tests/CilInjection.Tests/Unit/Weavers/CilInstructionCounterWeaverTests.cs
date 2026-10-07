using Microsoft.VisualStudio.TestTools.UnitTesting;
using CilInjecting.Tests.Infrastructure.Extensions;
using CilInjecting.Tests.Infrastructure.Loaders;
using CilInjection.Core.Tests.Fakes;
using CilInjection.Tests.Infrastructure.Fakes;
using CilInstructionCounter;
using CilInstructionCounter.Tests.Assertions;
using Counter.RunTime;
using Mono.Cecil.Cil;

namespace CilInjection.Tests.Unit.Weavers;

[TestClass]
public class InstructionCounterWeaverTests
{
    [TestMethod]
    public void Inject_Should_Add_Counter_Instructions_Before_Target()
    {
        // Arrange
        var fieldRef = CecilTestHelper.CreateDummyFieldReference("CounterMetadata", "_counter");
        
        ICounterWeaver weaver = new InstructionCounterWeaver();
        weaver.Initialize(fieldRef);

        var injectionContext = new FakeMethodInjectionContext();
        // Symulujemy instrukcję docelową w metodzie (np. zwykły Ret lub Nop)
        var instructionCtx = injectionContext.AddContext(OpCodes.Ret);

        var metadataContext = new FakeMetadataContext();

        // Act
        // Wywołujemy wewnętrzny interfejs IEngineWeaver ukryty za fasadą IWeaver
        weaver.Engine.Inject(injectionContext, metadataContext);

        // Assert
        var fakeInstructionContext = (FakeInstructionInjectionContext)injectionContext.Contexts[0];
        
        // Sprawdzamy czy dodano dokładnie 4 instrukcje przed celem (Ldsfld, Ldc_I8, Add, Stsfld)
        Assert.AreEqual(1, injectionContext.Contexts.Count);
        Assert.AreEqual(4, fakeInstructionContext.BeforeInstructions.Count);
        
        Assert.AreEqual(OpCodes.Ldsfld, fakeInstructionContext.BeforeInstructions[0].OpCode);
        Assert.AreEqual(OpCodes.Ldc_I8, fakeInstructionContext.BeforeInstructions[1].OpCode);
        Assert.AreEqual(1L, fakeInstructionContext.BeforeInstructions[1].Operand);
        Assert.AreEqual(OpCodes.Add, fakeInstructionContext.BeforeInstructions[2].OpCode);
        Assert.AreEqual(OpCodes.Stsfld, fakeInstructionContext.BeforeInstructions[3].OpCode);
    }
    
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