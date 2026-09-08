using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace CilComplexityAnalyzer.TestExecutor.Tests.CilInstructionInjector.Infrastructure;

internal static class InMemoryCompiler
{
    public static byte[] Compile(string sourceCode)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);

        var references = new MetadataReference[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Console).Assembly.Location)
        };

        var compilation = CSharpCompilation.Create(
            assemblyName: $"TestStudentAssembly_{Guid.NewGuid():N}",
            syntaxTrees: new[] { syntaxTree },
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        using var ms = new MemoryStream();
        var emitResult = compilation.Emit(ms);

        if (!emitResult.Success)
        {
            var errors = string.Join("\n", emitResult.Diagnostics.Select(d => d.GetMessage()));
            throw new InvalidOperationException($"InMemoryCompiler emit failed:\n{errors}");
        }

        return ms.ToArray();
    }
}