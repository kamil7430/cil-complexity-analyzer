using CilInjecting.Tests.Infrastructure.Fakes;

namespace CilInjecting.Tests.Unit.Core;

using System.Linq;
using CilInjection.Core;
using Assertions;
using Infrastructure.Fixtures;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class InjectionPipelineTests
{
    private PipelineTestEnvironment _env = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        _env = new PipelineTestEnvironment();
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
        Type markerType = _env.CreateDefaultDynamicRuntime("CustomRuntime");
        var strategy = _env.CreateStrategy(markerType);
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
        Type sharedMarkerType = _env.CreateDefaultDynamicRuntime("SharedRuntime");
        var strategy1 = _env.CreateStrategy(sharedMarkerType);
        var strategy2 = _env.CreateStrategy(sharedMarkerType);
        
        var pipeline = _env.CreatePipeline(strategy1, strategy2);

        // Act
        pipeline.LoadRuntimesInto(_env.Alc);

        // Assert
        int loadedCount = _env.Alc.Assemblies.Count(a => a.GetName().Name == "SharedRuntime");
        Assert.AreEqual(1, loadedCount, "Biblioteka o tym samym typie markerowym nie powinna być ładowana wielokrotnie.");
    }
    
    [TestMethod]
    public void ShouldLoadTwoDifferentAssemblies_WhenDifferentStrategiesAreLoaded()
    {
        // Arrange
        const string assemblyName1 = "DifferentRuntime1";
        const string assemblyName2 = "DifferentRuntime2";
        
        Type markerType1 = _env.CreateDefaultDynamicRuntime(assemblyName: assemblyName1, typeName: "MarkerType1");
        Type markerType2 = _env.CreateDefaultDynamicRuntime(assemblyName: assemblyName2, typeName: "MarkerType2");
        var strategy1 = _env.CreateStrategy(markerType1);
        var strategy2 = _env.CreateStrategy(markerType2);
        
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