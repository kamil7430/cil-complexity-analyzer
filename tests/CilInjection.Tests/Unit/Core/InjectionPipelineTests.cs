using CilInjecting.Tests.Assertions;
using CilInjection.Tests.Infrastructure.Fixtures;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CilInjection.Tests.Unit.Core;

[TestClass]
public class InjectionManagerTests
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
        byte[] dllBytes = _env.CreateDefaultAssemblyBytes(assemblyName: "CustomRuntime");
        var strategy = _env.CreateStrategy(dllBytes);
        var pipeline = _env.CreatePipeline(strategy);

        // Act
        pipeline.LoadRuntimesInto(_env.Alc);

        // Assert
        _env.Alc.ShouldHaveLoadedAssembly("CustomRuntime");
    }

    [TestMethod]
    public void Should_NotLoadDuplicateAssemblies_WhenMultipleStrategiesShareRuntime()
    {
        // Arrange
        byte[] dllBytes = _env.CreateDefaultAssemblyBytes(assemblyName: "CustomRuntime");
        var strategy1 = _env.CreateStrategy(dllBytes);
        var strategy2 = _env.CreateStrategy(dllBytes);
        
        var pipeline = _env.CreatePipeline(strategy1, strategy2);

        // Act
        pipeline.LoadRuntimesInto(_env.Alc);

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
        
        var pipeline = _env.CreatePipeline(strategy1, strategy2);

        // Act
        pipeline.LoadRuntimesInto(_env.Alc);

        // Assert
        _env.Alc.ShouldHaveLoadedAssembly(assemblyName1);
        _env.Alc.ShouldHaveLoadedAssembly(assemblyName2);
    }
    
    [TestMethod]
    public void Transform_ShouldReturnOriginalBytes_WhenNoStrategiesRegistered()
    {
        // Arrange
        var pipeline = _env.CreatePipeline(); 
        var assemblyBytes = _env.CreateDefaultAssemblyBytes();

        // Act
        var resultBytes = pipeline.Transform(assemblyBytes);

        // Assert
        CollectionAssert.AreEqual(assemblyBytes, resultBytes, "Gdy brak strategii, bajty nie powinny ulec zmianie.");
    }
}