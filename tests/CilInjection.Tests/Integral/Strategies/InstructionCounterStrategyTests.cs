using CilInjecting.Tests.Assertions;
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
    public void Should_NotLoadDuplicateAssemblies_WhenMultipleStrategiesShareRuntime()
    {
        // Arrange
        byte[] dllBytes = _env.CreateDefaultAssemblyBytes(assemblyName: "CustomRuntime");
        var strategy1 = _env.CreateStrategy(dllBytes);
        var strategy2 = _env.CreateStrategy(dllBytes);
        
        var manager = _env.CreatePipeline(strategy1, strategy2);

        // Act
        manager.LoadRuntimesInto(_env.Alc);

        // Assert
        int loadedCount = _env.Alc.Assemblies.Count(a => a.GetName().Name == "CustomRuntime");
        Assert.AreEqual(1, loadedCount, "Biblioteka o tym samym typie markerowym nie powinna być ładowana wielokrotnie.");
    }
    
    [TestMethod]
    public void ShouldLoadTwoDifferentAssemblies_WhenDifferentStrategiesAreLoaded()
    {
        // Arrange
        const string assemblyName1 = "DifferentRuntime1";
        const string assemblyName2 = "DifferentRuntime2";
        
        byte[] dllBytes1 = _env.CreateDefaultAssemblyBytes(assemblyName: assemblyName1);
        byte[] dllBytes2 = _env.CreateDefaultAssemblyBytes(assemblyName: assemblyName2);
        var strategy1 = _env.CreateStrategy(dllBytes1);
        var strategy2 = _env.CreateStrategy(dllBytes2);
        
        var manager = _env.CreatePipeline(strategy1, strategy2);

        // Act
        manager.LoadRuntimesInto(_env.Alc);

        // Assert
        _env.Alc.ShouldHaveLoadedAssembly(assemblyName1);
        _env.Alc.ShouldHaveLoadedAssembly(assemblyName2);
    }
    
    [TestMethod]
    public void Transform_ShouldReturnOriginalBytes_WhenNoStrategiesRegistered()
    {
        // Arrange
        var manager = _env.CreatePipeline(); 
        var assemblyBytes = _env.CreateDefaultAssemblyBytes();

        // Act
        var resultBytes = manager.Transform(assemblyBytes);

        // Assert
        CollectionAssert.AreEqual(assemblyBytes, resultBytes, "Gdy brak strategii, bajty nie powinny ulec zmianie.");
    }
}