using System.Text;
using CilComplexityAnalyzer.TestExecutor.Contract;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.Logging;
namespace CilComplexityAnalyzer.TestExecutor;

internal static class Compiler
{
    private static readonly CSharpCompilationOptions CompilationOptions = new(
        outputKind: OutputKind.DynamicallyLinkedLibrary,
        optimizationLevel: OptimizationLevel.Debug
    );

    internal static void Initialize(ILogger? logger)
    {
        logger?.LogInformation("Initializing CompilationOptions...");
        _ = CompilationOptions;
    }
    
    internal static TestSuite CompileStudentSolution(this TestSuite testSuite)
    {
        testSuite.Logger()?.LogInformation($"[{testSuite.Name}] Beginning student solution compilation.");

        if (testSuite.StudentSolutionSyntaxTree is null)
            throw new NullReferenceException("Syntax tree is null! Did you run analyzer before compiler?");

        using var stream = new MemoryStream();
        var compilationResult = CSharpCompilation.Create(
            assemblyName: "StudentSolution", 
            syntaxTrees: [testSuite.StudentSolutionSyntaxTree],
            references: Basic.Reference.Assemblies.Net100.References.All,
            options: CompilationOptions
        ).Emit(stream, cancellationToken: testSuite.CancellationToken());

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
        testSuite.StudentSolutionAssemblyBytes = stream.ToArray();
        return testSuite;
    }

    internal static TestSuite CompileTestSuite(this TestSuite testSuite)
    {
        testSuite.Logger()?.LogInformation($"[{testSuite.Name}] Beginning test suite compilation.");
        
        if (testSuite.TestSuiteSyntaxTree is null)
            throw new NullReferenceException("Syntax tree is null! Did you run analyzer before compiler?");
        
        using var stream = new MemoryStream();
        var compilationResult = CSharpCompilation.Create(
            assemblyName: testSuite.Name, 
            syntaxTrees: [testSuite.TestSuiteSyntaxTree],
            references: Basic.Reference.Assemblies.Net100.References.All,
            options: CompilationOptions
        ).Emit(stream, cancellationToken: testSuite.CancellationToken());

        if (!compilationResult.Success)
            throw new TestExecutionException("Failed to compile test suite.");
        
        stream.Seek(0, SeekOrigin.Begin);
        testSuite.TestSuiteAssemblyBytes = stream.ToArray();
        return testSuite;
    }
}