using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mono.Cecil;

namespace CilComplexityAnalyzer.LibCilInjection.Tests;

[TestClass]
public class CilInjectorServiceTests
{
    private string _tempDir = null!;
    private string _inputPath = null!;
    private string _outputPath = null!;

    [TestInitialize]
    public void Setup()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDir);

        _inputPath = Path.Combine(_tempDir, "TestAssembly.dll");
        _outputPath = Path.Combine(_tempDir, "TestAssembly.instrumented.dll");

        CreateDummyAssembly(_inputPath);
    }

    [TestCleanup]
    public void Cleanup()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, recursive: true);
        }
    }

    [TestMethod]
    public void AnalyzeAndInjectFromFile_ValidAssembly_InjectsGlobalCounterContainerAndField()
    {
        // Arrange
        var service = new CilInjectorLibrary();

        // Act
        service.AnalyzeAndInjectFromFile(_inputPath, _outputPath);

        // Assert
        Assert.IsTrue(File.Exists(_outputPath), "Output file was NOT created.");

        using var assemblyDef = AssemblyDefinition.ReadAssembly(_outputPath);
        var mainModule = assemblyDef.MainModule;

        var containerType = mainModule.Types.FirstOrDefault(t => t.Name == "<GlobalCounterContainer>");
        Assert.IsNotNull(containerType, "Could not find class with <GlobalCounterContainer>.");

        var counterField = containerType.Fields.FirstOrDefault(f => f.Name == "__InstructionCounter");
        Assert.IsNotNull(counterField, "Nie znaleziono pola __InstructionCounter wewnątrz <GlobalCounterContainer>.");
        Assert.IsTrue(counterField.IsStatic, "Pole __InstructionCounter powinno być statyczne.");
        Assert.AreEqual("System.Int64", counterField.FieldType.FullName, "Typ pola __InstructionCounter powinien wynosić System.Int64.");
    }

    [TestMethod]
    public void AnalyzeAndInjectFromFile_Execution_IncrementsInstructionCounter()
    {
        var service = new CilInjectorLibrary();
        service.AnalyzeAndInjectFromFile(_inputPath, _outputPath);

        var context = new AssemblyLoadContext("TestContext", isCollectible: true);
        Assembly instrumentedAssembly;
        using (var stream = File.OpenRead(_outputPath))
        {
            instrumentedAssembly = context.LoadFromStream(stream);
        }

        try
        {
            var containerType = instrumentedAssembly.GetType("<GlobalCounterContainer>");
            Assert.IsNotNull(containerType, "Nie znaleziono typu <GlobalCounterContainer> w załadowanym assembly.");

            var counterField = containerType.GetField("__InstructionCounter", BindingFlags.Public | BindingFlags.Static);
            Assert.IsNotNull(counterField, "Nie znaleziono pola __InstructionCounter.");

            counterField.SetValue(null, 0L);

            var calcType = instrumentedAssembly.GetType("DummyNamespace.Calculator");
            Assert.IsNotNull(calcType, "Nie znaleziono typu DummyNamespace.Calculator.");

            var instance = Activator.CreateInstance(calcType);
            var addMethod = calcType.GetMethod("Add");
            Assert.IsNotNull(addMethod, "Nie znaleziono metody Add.");

            // Act
            var result = (int)addMethod.Invoke(instance, new object[] { 2, 3 })!;

            // Assert
            Assert.AreEqual(5, result, "Wywołana metoda powinna zwrócić poprawny wynik dodawania.");

            long counterValue = (long)counterField.GetValue(null)!;
            Assert.IsTrue(counterValue > 0, $"Licznik instrukcji powinien wynosić więcej niż 0, a wynosił: {counterValue}");
        }
        finally
        {
            context.Unload();
        }
    }
    private static void CreateDummyAssembly(string outputPath)
    {
        const string sourceCode = """
            namespace DummyNamespace;

            public class Calculator
            {
                public int Add(int a, int b) => a + b;
            }
            """;

        var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
        var references = new MetadataReference[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location)
        };

        var compilation = CSharpCompilation.Create(
            "TestAssembly",
            new[] { syntaxTree },
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        using var stream = File.Create(outputPath);
        var result = compilation.Emit(stream);

        if (!result.Success)
        {
            var failures = string.Join("\n", result.Diagnostics.Select(d => d.GetMessage()));
            throw new InvalidOperationException($"Kompilacja testowego assembly nie powiodła się:\n{failures}");
        }
    }
}