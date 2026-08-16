using System.Collections.Immutable;
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

    private static readonly CSharpCompilationOptions CompilationOptions = new(
        outputKind: OutputKind.DynamicallyLinkedLibrary,
        optimizationLevel: OptimizationLevel.Debug
    );

    internal static void Initialize(ILogger? logger)
    {
        logger?.LogInformation("Initializing References...");
        _ = References;
        logger?.LogInformation("Initializing CompilationOptions...");
        _ = CompilationOptions;
    }
    
    internal static TestSuite Compile(this TestSuite testSuite)
    {
        testSuite.Logger?.LogInformation($"[{testSuite.NameOrHash}] Beginning compilation.");

        if (testSuite.SyntaxTree is null)
            throw new NullReferenceException("SyntaxTree is null! Did you run analyzer before compiler?");

        using var stream = new MemoryStream();
        var compilationResult = CSharpCompilation.Create(
            assemblyName: testSuite.NameOrHash, 
            syntaxTrees: [testSuite.SyntaxTree],
            references: References,
            options: CompilationOptions
        ).Emit(stream, cancellationToken: testSuite.CancellationToken);

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
        testSuite.AssemblyBytes = stream.ToArray();
        return testSuite;
    }
}