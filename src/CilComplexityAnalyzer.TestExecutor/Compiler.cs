using System.Collections.Immutable;
using System.Reflection;
using System.Text;
using CilComplexityAnalyzer.TestExecutor.Contract;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.Logging;

namespace CilComplexityAnalyzer.TestExecutor;

internal static class Compiler
{
    private static readonly ImmutableList<MetadataReference> References = AppDomain.CurrentDomain.GetAssemblies()
        .Where(a => !a.IsDynamic && !string.IsNullOrWhiteSpace(a.Location))
        .Select(a => MetadataReference.CreateFromFile(a.Location))
        .Cast<MetadataReference>()
        .ToImmutableList();

    private static readonly CSharpCompilationOptions CompilationOptions = new(OutputKind.DynamicallyLinkedLibrary);
    
    internal static TestCase Compile(this TestCase testCase)
    {
        testCase.Logger?.LogInformation($"[{testCase.NameOrHash}] Beginning compilation.");

        if (testCase.SyntaxTree is null)
            throw new NullReferenceException("SyntaxTree is null! Did you run analyzer before compiler?");

        using var stream = new MemoryStream();
        var compilationResult = CSharpCompilation.Create(
            testCase.NameOrHash, 
            [testCase.SyntaxTree],
            References,
            CompilationOptions
        ).Emit(stream, cancellationToken: testCase.CancellationToken);

        if (!compilationResult.Success)
        {
            var errors = new StringBuilder("Compilation failed. Errors and warnings:");
            foreach (var diagnostic in compilationResult.Diagnostics)
            {
                errors.Append($"\n{diagnostic.ToString()}");
            }
            throw new TestExecutionException(errors.ToString());
        }
            
        stream.Seek(0, SeekOrigin.Begin);
        testCase.Assembly = Assembly.Load(stream.ToArray());
        return testCase;
    }
}