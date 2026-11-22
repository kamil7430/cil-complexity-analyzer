using System.Text;
using CilComplexityAnalyzer.Contract;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace CilComplexityAnalyzer.TestExecutor;

internal static class Compiler
{
    private static readonly CSharpCompilationOptions CompilationOptions = new(
        outputKind: OutputKind.DynamicallyLinkedLibrary,
        optimizationLevel: OptimizationLevel.Debug
    );
    
    internal static InternalTestSuite CompileStudentSolution(this InternalTestSuite internalTestSuite)
    {
        Console.WriteLine($"[{internalTestSuite.Name}] Beginning student solution compilation.");

        if (internalTestSuite.StudentSolutionSyntaxTree is null)
            throw new NullReferenceException("Syntax tree is null! Did you run analyzer before compiler?");

        using var stream = new MemoryStream();
        var compilationResult = CSharpCompilation.Create(
            assemblyName: "StudentSolution", 
            syntaxTrees: [internalTestSuite.StudentSolutionSyntaxTree],
            references: Basic.Reference.Assemblies.Net100.References.All,
            options: CompilationOptions
        ).Emit(stream, cancellationToken: internalTestSuite.CancellationToken);

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
        internalTestSuite.StudentSolutionAssemblyBytes = stream.ToArray();
        return internalTestSuite;
    }

    internal static InternalTestSuite CompileTestSuite(this InternalTestSuite internalTestSuite)
    {
        Console.WriteLine($"[{internalTestSuite.Name}] Beginning test suite compilation.");
        
        if (internalTestSuite.TestSuiteSyntaxTree is null)
            throw new NullReferenceException("Syntax tree is null! Did you run analyzer before compiler?");
        
        using var stream = new MemoryStream();
        var compilationResult = CSharpCompilation.Create(
            assemblyName: internalTestSuite.Name, 
            syntaxTrees: [internalTestSuite.TestSuiteSyntaxTree],
            // TODO: do it more sophisticated way
            references: [
                ..Basic.Reference.Assemblies.Net100.References.All,
                MetadataReference.CreateFromImage(internalTestSuite.StudentSolutionAssemblyBytes!),
                MetadataReference.CreateFromFile(typeof(TestCase).Assembly.Location),
            ],
            options: CompilationOptions
        ).Emit(
            stream, 
            // TODO: ok? Looks ok
            manifestResources: [
                new ResourceDescription(
                    "StudentSolution", 
                    () => new MemoryStream(internalTestSuite.StudentSolutionAssemblyBytes!), 
                    true
                ),
            ],
            cancellationToken: internalTestSuite.CancellationToken
        );

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
        internalTestSuite.TestSuiteAssemblyBytes = stream.ToArray();
        return internalTestSuite;
    }
}