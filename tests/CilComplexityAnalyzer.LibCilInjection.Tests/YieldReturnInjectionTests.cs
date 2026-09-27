using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using CilComplexityAnalyzer.TestExecutor;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace CilComplexityAnalyzer.LibCilInjection.Tests;

[TestClass]
public class YieldReturnInjectionTests
{
    private const string IteratorSource = """
        namespace DummyNamespace;

        public class RangeYielder
        {
            public System.Collections.Generic.IEnumerable<int> Range(int from, int to)
            {
                for (int i = from; i <= to; i++)
                    yield return i * i;
            }
        }
        """;

    [TestMethod]
    public void Injector_ShouldAccuratelyTrackInstructions_ForYieldReturnStateMachine()
    {
        byte[] assemblyBytes = CompileSource(IteratorSource);
        byte[] instrumentedBytes = CilInstructionInjector.InjectCilToAssemblyBytes(assemblyBytes);

        var alc = new AssemblyLoadContext("YieldReturnContext", isCollectible: true);
        try
        {
            using var ms = new MemoryStream(instrumentedBytes);
            var asm = alc.LoadFromStream(ms);

            var containerType = asm.GetType("<GlobalCounterContainer>")!;
            var counterField = containerType.GetField("__InstructionCounter", BindingFlags.Public | BindingFlags.Static)!;

            var yielderType = asm.GetType("DummyNamespace.RangeYielder")!;
            var instance = Activator.CreateInstance(yielderType)!;
            var rangeMethod = yielderType.GetMethod("Range")!;

            counterField.SetValue(null, 0L);

            var enumerable5 = (IEnumerable<int>)rangeMethod.Invoke(instance, new object[] { 1, 5 })!;
            var result5 = enumerable5.ToList();

            long countFor5Items = (long)counterField.GetValue(null)!;

            CollectionAssert.AreEqual(new List<int> { 1, 4, 9, 16, 25 }, result5);
            Assert.IsTrue(countFor5Items > 0, $"Licznik CIL powinien wynieść > 0, a wyniósł {countFor5Items}");

            counterField.SetValue(null, 0L);

            var enumerable2 = (IEnumerable<int>)rangeMethod.Invoke(instance, new object[] { 1, 2 })!;
            var result2 = enumerable2.ToList();

            long countFor2Items = (long)counterField.GetValue(null)!;

            CollectionAssert.AreEqual(new List<int> { 1, 4 }, result2);

            Assert.IsTrue(countFor5Items > countFor2Items, 
                $"Liczba instrukcji dla 5 elementów ({countFor5Items}) powinna być większa niż dla 2 elementów ({countFor2Items})");
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