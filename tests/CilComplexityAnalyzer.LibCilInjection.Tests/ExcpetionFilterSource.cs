using System.Reflection;
using System.Runtime.Loader;
using CilComplexityAnalyzer.TestExecutor;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace CilComplexityAnalyzer.LibCilInjection.Tests;

[TestClass]
public class ExceptionFilterInjectionTests
{
    private const string ExceptionFilterSource = """
        namespace DummyNamespace;

        public class FilteredCatcher
        {
            public string Handle(int code)
            {
                try
                {
                    throw new System.InvalidOperationException($"code:{code}");
                }
                catch (System.InvalidOperationException ex) when (ex.Message.Contains("42"))
                {
                    return "filtered-42";
                }
                catch (System.InvalidOperationException)
                {
                    return "generic";
                }
            }
        }
        """;

    [TestMethod]
    public void Injector_ShouldPreserveExceptionFilterSemantics_WhenFilterMatches()
    {
        byte[] assemblyBytes = CompileSource(ExceptionFilterSource);
        byte[] instrumentedBytes = CilInstructionInjector.InjectCilToAssemblyBytes(assemblyBytes);

        var alc = new AssemblyLoadContext("FilterMatchContext", isCollectible: true);
        try
        {
            using var ms = new MemoryStream(instrumentedBytes);
            var asm = alc.LoadFromStream(ms);

            var containerType = asm.GetType("<GlobalCounterContainer>")!;
            var fieldInfo = containerType.GetField("__InstructionCounter", BindingFlags.Public | BindingFlags.Static)!;

            var catcherType = asm.GetType("DummyNamespace.FilteredCatcher")!;
            var instance = Activator.CreateInstance(catcherType)!;
            var handleMethod = catcherType.GetMethod("Handle")!;

            fieldInfo.SetValue(null, 0L);

            var result = handleMethod.Invoke(instance, new object[] { 42 });

            Assert.AreEqual("filtered-42", result);

            long executedInstructions = (long)fieldInfo.GetValue(null)!;
            Assert.IsTrue(executedInstructions > 0, 
                $"Licznik CIL powinien zarejestrować wykonanie instrukcji wewnątrz filtra/handlera, a wyniósł: {executedInstructions}");
        }
        finally
        {
            alc.Unload();
        }
    }

    [TestMethod]
    public void Injector_ShouldPreserveExceptionFilterSemantics_WhenFilterFailsAndFallsBackToGenericCatch()
    {
        byte[] assemblyBytes = CompileSource(ExceptionFilterSource);
        byte[] instrumentedBytes = CilInstructionInjector.InjectCilToAssemblyBytes(assemblyBytes);

        var alc = new AssemblyLoadContext("FilterFallbackContext", isCollectible: true);
        try
        {
            using var ms = new MemoryStream(instrumentedBytes);
            var asm = alc.LoadFromStream(ms);

            var containerType = asm.GetType("<GlobalCounterContainer>")!;
            var fieldInfo = containerType.GetField("__InstructionCounter", BindingFlags.Public | BindingFlags.Static)!;

            var catcherType = asm.GetType("DummyNamespace.FilteredCatcher")!;
            var instance = Activator.CreateInstance(catcherType)!;
            var handleMethod = catcherType.GetMethod("Handle")!;

            fieldInfo.SetValue(null, 0L);

            var result = handleMethod.Invoke(instance, new object[] { 1 });

            Assert.AreEqual("generic", result);

            long executedInstructions = (long)fieldInfo.GetValue(null)!;
            Assert.IsTrue(executedInstructions > 0, 
                $"Licznik CIL powinien zarejestrować wykonanie instrukcji, a wyniósł: {executedInstructions}");
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