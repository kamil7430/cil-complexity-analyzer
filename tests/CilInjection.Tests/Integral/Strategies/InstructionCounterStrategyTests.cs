using System.Reflection;
using CilInjecting.Tests.Assertions;
using CilInjecting.Tests.Infrastructure.Loaders;
using CilInjection.Core;
using CilInjection.Tests.Infrastructure.Fixtures;
using Counter.Provider.Base;
using Counter.Provider.InstructionCounter;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CilInjection.Tests.Integral.Strategies;

[TestClass]
public class InstructionCounterStrategyTests
{
    private ManagerTestEnvironment _env = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        _env = new ManagerTestEnvironment();
    }

    [TestCleanup]
    public void TestCleanup()
    {
        _env.Dispose();
    }

    [TestMethod]
    public void Should_LoadDynamicRuntime_IntoAssemblyLoadContext()
    {
        // Arrange
        var instructionCounterStrategy = new InstructionCounterStrategy();
        var manager = new CilInjectionManager(instructionCounterStrategy);
        var assemblyName = instructionCounterStrategy.GetMappedFullName(CounterMetadata.ResourceName);

        // Act
        manager.LoadRuntimesInto(_env.Alc);

        // Assert
        _env.Alc.ShouldHaveLoadedAssembly(assemblyName);
    } 
    
    [TestMethod]
    public void ShouldLoadTwoDifferentAssemblies_WhenStrategyIsLoadedTwice()
    {
        // Arrange
        var instructionCounterStrategy1 = new InstructionCounterStrategy();
        var instructionCounterStrategy2 = new InstructionCounterStrategy();
        var manager = new CilInjectionManager(instructionCounterStrategy1, instructionCounterStrategy2);
        var assemblyName1 = instructionCounterStrategy1.GetMappedFullName(CounterMetadata.ResourceName);
        var assemblyName2 = instructionCounterStrategy2.GetMappedFullName(CounterMetadata.ResourceName);

        // Act
        manager.LoadRuntimesInto(_env.Alc);

        // Assert
        _env.Alc.ShouldHaveLoadedAssembly(assemblyName1);
        _env.Alc.ShouldHaveLoadedAssembly(assemblyName2);
    }
    
    [DataTestMethod]
    // Format: [DataRow(Ścieżka, PełnaNazwaKlasy, NazwaMetody, WArgumenty_Do_Metody)]
    [DataRow("Basic/SimpleCalculator.cs", "TestTargets.Basic.SimpleCalculator", "Add", 2, 3)]
    [DataRow("Basic/SimpleCalculator.cs", "TestTargets.Basic.SimpleCalculator", "Subtract", 10, 5)]
    [DataRow("Basic/SimpleCalculator.cs", "TestTargets.Basic.SimpleCalculator", "Multiply", 2, 3)]
    [DataRow("Basic/SimpleCalculator.cs", "TestTargets.Basic.SimpleCalculator", "ComputeExpression", 2, 3, 4)]
    public void InstructionCounter_ShouldCountInstructions_ForVariousMethods(
        string relativeFilePath, 
        string fullTypeName, 
        string methodName, 
        params object[] methodArguments) 
    {
        // Arrange
        using var module = TestTargetLoader.CompileSourceToModule(relativeFilePath);

        var instructionCounterStrategy = new InstructionCounterStrategy();
        var manager = new CilInjectionManager(instructionCounterStrategy);

        // Act - Transformacja i wczytanie
        manager.Transform(module);
        manager.LoadRuntimesInto(_env.Alc);
        
        var handle = instructionCounterStrategy.CreateHandle(_env.Alc);
        handle.ResetCounter();

        // Act - Wywołanie metody docelowej
        module.ExecuteMethod(_env.Alc, fullTypeName, methodName, methodArguments);

        // Assert
        var instructionCount = handle.GetCounter();
        Assert.IsTrue(instructionCount > 0, 
            $"Licznik instrukcji dla {fullTypeName}.{methodName} powinien być > 0, a wynosił: {instructionCount}");
    }
}