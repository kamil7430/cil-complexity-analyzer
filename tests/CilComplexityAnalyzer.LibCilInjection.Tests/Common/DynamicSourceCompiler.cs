using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace CilComplexityAnalyzer.LibCilInjection.Tests.Common;

public static class DynamicSourceCompiler
{
    public static byte[] CompileSource(string source, bool optimize = false)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);

        var references = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location))
            .Select(a => MetadataReference.CreateFromFile(a.Location))
            .Cast<MetadataReference>()
            .ToList();
        
        var options = new CSharpCompilationOptions(
            OutputKind.DynamicallyLinkedLibrary,
            optimizationLevel: optimize ? OptimizationLevel.Release : OptimizationLevel.Debug);

        var compilation = CSharpCompilation.Create("DynamicTestAssembly", new[] { syntaxTree },
            references, options);

        using var ms = new MemoryStream();
        var result = compilation.Emit(ms);

        if (!result.Success)
        {
            var failures = string.Join("\n", result.Diagnostics.Select(d => d.GetMessage()));

            throw new InvalidOperationException($"Kompilacja testowego assembly nie powiodła się: \n{failures}");
        }

        return ms.ToArray();
    }
}