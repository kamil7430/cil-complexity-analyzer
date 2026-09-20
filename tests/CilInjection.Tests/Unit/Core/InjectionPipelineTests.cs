namespace CilInjecting.Tests.Unit.Core;

using System.Linq;
using System.Reflection;
using CilInjecting.Tests.Assertions;
using CilInjecting.Tests.Infrastructure.Fixtures;
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
}