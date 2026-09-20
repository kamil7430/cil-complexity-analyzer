using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CilInjection.Core;
using CilInstructionCounter;

namespace CilComplexityAnalyzer.LibCilInjection.Tests;

[TestClass]
public class InstructionCounterPipelineTests
{
    private InstructionCounterStrategy _strategy = null!;
    private InjectionPipeline _pipeline = null!;

    [TestInitialize]
    public void Setup()
    {
        _strategy = new InstructionCounterStrategy(new InstructionCounterWeaver());
        _pipeline = new InjectionPipeline(new IInjectionStrategy[] { _strategy });
    }

    [TestMethod]
    public void Pipeline_Transform_Execution_IncrementsInstructionCounter()
    {
        byte[] originalBytes = CompileSource("""
            namespace DummyNamespace;

            public class Calculator
            {
                public int Add(int a, int b) => a + b;
            }
            """);

        byte[] instrumentedBytes = _pipeline.Transform(originalBytes);

        var alc = new AssemblyLoadContext("PipelineTestContext", isCollectible: true);
        try
        {
            _pipeline.LoadRuntimesInto(alc);

            Assembly instrumentedAssembly;
            using (var ms = new MemoryStream(instrumentedBytes))
            {
                instrumentedAssembly = alc.LoadFromStream(ms);
            }

            ICounterHandle handle = _strategy.BuildHandle(alc);
            handle.ResetCounter();

            var calcType = instrumentedAssembly.GetType("DummyNamespace.Calculator")
                           ?? throw new InvalidOperationException("Nie znaleziono typu DummyNamespace.Calculator.");
            var instance = Activator.CreateInstance(calcType)!;
            var addMethod = calcType.GetMethod("Add")
                             ?? throw new InvalidOperationException("Nie znaleziono metody Add.");

            var result = (int)addMethod.Invoke(instance, new object[] { 2, 3 })!;

            Assert.AreEqual(5, result, "Wywołana metoda powinna zwrócić poprawny wynik dodawania.");

            long counterValue = handle.GetCounter();
            Assert.IsTrue(counterValue > 0, $"Licznik instrukcji powinien wynosić więcej niż 0, a wyniósł: {counterValue}");
        }
        finally
        {
            alc.Unload();
        }
    }

    [TestMethod]
    public void Pipeline_ResetCounter_ZerosOutBetweenInvocations()
    {
        byte[] originalBytes = CompileSource("""
            namespace DummyNamespace;

            public class Calculator
            {
                public int Add(int a, int b) => a + b;
            }
            """);

        byte[] instrumentedBytes = _pipeline.Transform(originalBytes);

        var alc = new AssemblyLoadContext("PipelineResetContext", isCollectible: true);
        try
        {
            _pipeline.LoadRuntimesInto(alc);

            Assembly instrumentedAssembly;
            using (var ms = new MemoryStream(instrumentedBytes))
            {
                instrumentedAssembly = alc.LoadFromStream(ms);
            }

            ICounterHandle handle = _strategy.BuildHandle(alc);

            var calcType = instrumentedAssembly.GetType("DummyNamespace.Calculator")!;
            var instance = Activator.CreateInstance(calcType)!;
            var addMethod = calcType.GetMethod("Add")!;

            handle.ResetCounter();
            addMethod.Invoke(instance, new object[] { 1, 1 });
            long firstRun = handle.GetCounter();

            handle.ResetCounter();
            Assert.AreEqual(0L, handle.GetCounter(), "Po ResetCounter() licznik powinien wynosić 0.");

            addMethod.Invoke(instance, new object[] { 1, 1 });
            long secondRun = handle.GetCounter();

            Assert.AreEqual(firstRun, secondRun, "Dwa identyczne wywołania powinny dać identyczną liczbę instrukcji.");
        }
        finally
        {
            alc.Unload();
        }
    }

    private static byte[] CompileSource(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);
        var references = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location))
            .Select(a => MetadataReference.CreateFromFile(a.Location))
            .Cast<MetadataReference>()
            .ToList();

        var compilation = CSharpCompilation.Create(
            "DynamicTestAssembly",
            new[] { syntaxTree },
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        using var ms = new MemoryStream();
        var result = compilation.Emit(ms);
        if (!result.Success)
            throw new InvalidOperationException(string.Join("\n", result.Diagnostics.Select(d => d.GetMessage())));
        return ms.ToArray();
    }
}