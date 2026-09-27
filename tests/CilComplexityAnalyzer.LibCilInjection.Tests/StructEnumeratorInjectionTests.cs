using System.Reflection;
using System.Runtime.Loader;
using CilComplexityAnalyzer.TestExecutor;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace CilComplexityAnalyzer.LibCilInjection.Tests;

[TestClass]
public class StructEnumeratorInjectionTests
{
    private const string SourceCode = """
        namespace DummyNamespace;

        public class ListSummer
        {
            public int SumList(System.Collections.Generic.List<int> values)
            {
                int sum = 0;
                foreach (var v in values) // generuje constrained.callvirt na List<int>.Enumerator (struct!)
                {
                    sum += v;
                }
                return sum;
            }

            public int SumDictionaryValues(System.Collections.Generic.Dictionary<string, int> map)
            {
                int sum = 0;
                foreach (var kvp in map) // Dictionary<K,V>.Enumerator też jest structem
                {
                    sum += kvp.Value;
                }
                return sum;
            }
        }
        """;

    [TestMethod]
    public void Injector_ShouldNotThrow_ForForeachOverListStructEnumerator()
    {
        byte[] assemblyBytes = CompileSource(SourceCode);
        byte[] instrumentedBytes = CilInstructionInjector.InjectCilToAssemblyBytes(assemblyBytes);

        var alc = new AssemblyLoadContext("ListEnumeratorContext", isCollectible: true);
        try
        {
            using var ms = new MemoryStream(instrumentedBytes);
            var asm = alc.LoadFromStream(ms);

            var containerType = asm.GetType("<GlobalCounterContainer>")!;
            var resetMethod = containerType.GetMethod("ResetInstructionCount", BindingFlags.Public | BindingFlags.Static)!;
            var getMethod = containerType.GetMethod("GetInstructionCount", BindingFlags.Public | BindingFlags.Static)!;

            var summerType = asm.GetType("DummyNamespace.ListSummer")!;
            var instance = Activator.CreateInstance(summerType)!;
            var sumListMethod = summerType.GetMethod("SumList")!;

            resetMethod.Invoke(null, null);

            var listType = typeof(List<int>);
            var list = (System.Collections.IList)Activator.CreateInstance(listType)!;
            list.Add(1); list.Add(2); list.Add(3);

            var result = sumListMethod.Invoke(instance, new object[] { list });

            Assert.AreEqual(6, result);
            Assert.IsTrue((long)getMethod.Invoke(null, null)! > 0);
        }
        finally
        {
            alc.Unload();
        }
    }

    [TestMethod]
    public void Injector_ShouldNotThrow_ForForeachOverDictionaryStructEnumerator()
    {
        byte[] assemblyBytes = CompileSource(SourceCode);
        byte[] instrumentedBytes = CilInstructionInjector.InjectCilToAssemblyBytes(assemblyBytes);

        var alc = new AssemblyLoadContext("DictionaryEnumeratorContext", isCollectible: true);
        try
        {
            using var ms = new MemoryStream(instrumentedBytes);
            var asm = alc.LoadFromStream(ms);

            var containerType = asm.GetType("<GlobalCounterContainer>")!;
            var resetMethod = containerType.GetMethod("ResetInstructionCount", BindingFlags.Public | BindingFlags.Static)!;
            var getMethod = containerType.GetMethod("GetInstructionCount", BindingFlags.Public | BindingFlags.Static)!;

            var summerType = asm.GetType("DummyNamespace.ListSummer")!;
            var instance = Activator.CreateInstance(summerType)!;
            var sumDictMethod = summerType.GetMethod("SumDictionaryValues")!;

            resetMethod.Invoke(null, null);

            var dictionary = new Dictionary<string, int>
            {
                { "A", 10 },
                { "B", 20 },
                { "C", 30 }
            };

            var result = sumDictMethod.Invoke(instance, new object[] { dictionary });

            Assert.AreEqual(60, result);
            Assert.IsTrue((long)getMethod.Invoke(null, null)! > 0);
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