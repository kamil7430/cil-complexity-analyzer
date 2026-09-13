using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using CilComplexityAnalyzer.TestExecutor;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace CilComplexityAnalyzer.LibCilInjection.Tests;

[TestClass]
public class TailCallInjectionTests
{
    private const string TailCallSource = """
        namespace DummyNamespace;

        public static class TailRecursive
        {
            public static long SumTo(long n, long acc = 0)
            {
                if (n == 0) return acc;
                return SumTo(n - 1, acc + n); // kompilator MOŻE (nie musi) wyemitować tail.
            }
        }
        """;

    [TestMethod]
    public void Injector_ShouldPreserveTailCallSemanticsAndTrackInstructions()
    {
        byte[] assemblyBytes = CompileSource(TailCallSource, optimize: true);
        byte[] instrumentedBytes = CilInstructionInjector.InjectCilToAssemblyBytes(assemblyBytes);

        var alc = new AssemblyLoadContext("TailCallContext", isCollectible: true);
        try
        {
            using var ms = new MemoryStream(instrumentedBytes);
            var asm = alc.LoadFromStream(ms);

            var containerType = asm.GetType("<GlobalCounterContainer>")!;
            var fieldInfo = containerType.GetField("__InstructionCounter", BindingFlags.Public | BindingFlags.Static)!;

            var tailType = asm.GetType("DummyNamespace.TailRecursive")!;
            var sumToMethod = tailType.GetMethod("SumTo")!;

            fieldInfo.SetValue(null, 0L);

            var result = (long)sumToMethod.Invoke(null, new object[] { 100L, 0L })!;

            Assert.AreEqual(5050L, result);

            long executedInstructions = (long)fieldInfo.GetValue(null)!;
            Assert.IsTrue(executedInstructions > 0, 
                $"Licznik CIL powinien zarejestrować instrukcje w rekurencji ogonowej, a wyniósł: {executedInstructions}");

            Assert.IsTrue(executedInstructions > 500, 
                $"Liczba zarejestrowanych instrukcji ({executedInstructions}) jest za mała dla 100 wywołań rekurencyjnych.");
        }
        finally
        {
            alc.Unload();
        }
    }

    private static byte[] CompileSource(string source, bool optimize = false)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);
        var references = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location))
            .Select(a => MetadataReference.CreateFromFile(a.Location))
            .Cast<MetadataReference>()
            .ToList();

        var compilationOptions = new CSharpCompilationOptions(
            OutputKind.DynamicallyLinkedLibrary,
            optimizationLevel: optimize ? OptimizationLevel.Release : OptimizationLevel.Debug);

        var compilation = CSharpCompilation.Create(
            "DynamicTestAssembly",
            new[] { syntaxTree },
            references,
            compilationOptions);

        using var ms = new MemoryStream();
        var result = compilation.Emit(ms);
        if (!result.Success)
            throw new InvalidOperationException(string.Join("\n", result.Diagnostics.Select(d => d.GetMessage())));
        return ms.ToArray();
    }
}