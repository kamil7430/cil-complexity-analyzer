using System;
using System.Linq;
using System.Runtime.Loader;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CilInjecting.Tests.Infrastructure.Compilers;
using CilInjection.Core.Abstractions;
using CilInjection.Tests.Infrastructure.Fakes;

namespace CilInjection.Tests.Unit.Core.Base;


// 3. Klasa testowa w MSTest
[TestClass]
public class BaseStrategyTests
{
    private AssemblyLoadContext _alc = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        _alc = new AssemblyLoadContext("TestAlc_Empty", isCollectible: true);
    }

    [TestCleanup]
    public void TestCleanup()
    {
        _alc.Unload();
    }

    [TestMethod]
    public void CreateHandle_WhenDllIsNotLoaded_ShouldLoadDllIntoContext()
    {
        // Arrange
        string assemblyName = "MyTestRuntimeAssembly";
        byte[] dummyDllBytes = TestAssemblyGenerator.CreateDefaultBytes(assemblyName);
        
        var strategy = new TestableBaseStrategy(dummyDllBytes);

        // Act
        var handle = strategy.CreateHandle(_alc);

        // Assert
        Assert.IsNotNull(handle);
        
        bool isLoaded = _alc.Assemblies.Any(a => a.GetName().Name!.StartsWith(assemblyName));
        Assert.IsTrue(isLoaded, "Dynamiczna biblioteka .dll powinna zostać załadowana do ALC.");
    }

    [TestMethod]
    public void CreateHandle_WhenCalledMultipleTimes_ShouldNotThrowAndNotDuplicateLoad()
    {
        // Arrange
        string assemblyName = "MyTestRuntimeAssembly_DuplicateCheck";
        byte[] dummyDllBytes = TestAssemblyGenerator.CreateDefaultBytes(assemblyName);
        
        var strategy = new TestableBaseStrategy(dummyDllBytes);

        ((IEngineStrategy)strategy).LoadRuntime(_alc);
        
        // Act
        var handle = strategy.CreateHandle(_alc); 

        // Assert
        Assert.IsNotNull(handle);

        int loadedRuntimeCount = _alc.Assemblies.Count(a => a.GetName().Name!.StartsWith(assemblyName));
        Assert.AreEqual(1, loadedRuntimeCount, "W ALC powinna znajdować się dokładnie jedna instancja tej biblioteki.");
    }
}