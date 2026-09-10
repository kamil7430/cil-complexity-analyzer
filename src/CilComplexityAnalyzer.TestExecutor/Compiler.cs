using System.Reflection;
using System.Text;
using CilComplexityAnalyzer.Contract;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic.CompilerServices;

namespace CilComplexityAnalyzer.TestExecutor;

internal static class Compiler
{
    private static readonly CSharpCompilationOptions CompilationOptions = new(
        outputKind: OutputKind.DynamicallyLinkedLibrary,
        optimizationLevel: OptimizationLevel.Debug
    );
    
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
            var errors = new StringBuilder("Failed to compile student code. Diagnostics:");
            foreach (var diagnostic in compilationResult.Diagnostics)
            {
                errors.Append($"\n{diagnostic}");
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
            // TODO: do it more sophisticated way
            references: [
                ..Basic.Reference.Assemblies.Net100.References.All,
                MetadataReference.CreateFromImage(testSuite.StudentSolutionAssemblyBytes!),
                MetadataReference.CreateFromFile(typeof(TestCase).Assembly.Location),
            ],
            options: CompilationOptions
        ).Emit(stream, cancellationToken: testSuite.CancellationToken());

        if (!compilationResult.Success)
        {
            var errors = new StringBuilder("Failed to compile test suite. Diagnostics:");
            foreach (var diagnostic in compilationResult.Diagnostics)
            {
                errors.Append($"\n{diagnostic}");
            }
            throw new TestExecutionException(errors.ToString());
        }
        
        stream.Seek(0, SeekOrigin.Begin);
        testSuite.TestSuiteAssemblyBytes = stream.ToArray();
        return testSuite;
    }
}