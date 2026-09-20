using System;
using System.Linq;
using CilInjecting.Tests.Infrastructure.Fixtures;
using CilInjection.Core.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mono.Cecil;

namespace CilInjecting.Tests.Unit.Core.Extensions;

[TestClass]
public class ModuleFieldImportTests
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
    public void ImportStaticField_RegistersAssemblyReference_ForExternalRuntimeType()
    {
        // Arrange
        Type runtimeMarkerType = _env.CreateDefaultDynamicRuntime("FieldImportRuntime");

        Type targetMarkerType = _env.CreateDynamicRuntime(
            "namespace DummyNamespace; public class Empty { }",
            "DummyNamespace.Empty",
            "TargetModule");

        using var targetModule = ModuleDefinition.ReadModule(targetMarkerType.Assembly.Location);

        // Act
        var fieldRef = targetModule.ImportStaticField(runtimeMarkerType, "Counter");

        // Assert
        string runtimeAssemblyName = runtimeMarkerType.Assembly.GetName().Name!;

        bool hasAssemblyRef = targetModule.AssemblyReferences.Any(ar => ar.Name == runtimeAssemblyName);
        Assert.IsTrue(hasAssemblyRef,
            $"Po imporcie pola z '{runtimeAssemblyName}' moduł powinien mieć wpis w AssemblyReferences — inaczej CLR nie znajdzie typu przy JIT-owaniu.");

        Assert.AreEqual(runtimeAssemblyName, fieldRef.DeclaringType.Scope.Name,
            "Scope zaimportowanego typu deklarującego powinien wskazywać na bibliotekę RunTime, nie na moduł docelowy.");
    }
}