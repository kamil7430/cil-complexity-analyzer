using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CilComplexityAnalyzer.TestExecutor;

internal static class StaticAnalyzer
{
    private static readonly IReadOnlyList<string> NamespaceWhitelist = [
        "System",
    ];
    
    internal static InternalTestSuite AnalyzeStudentSolution(this InternalTestSuite internalTestSuite)
    {
        Console.WriteLine($"[{internalTestSuite.Name}] Beginning student solution static analysis.");

        var studentSolutionSyntaxTree = CSharpSyntaxTree.ParseText(
            text: internalTestSuite.TestSuite.StudentSolutionSourceCode(),
            cancellationToken: internalTestSuite.CancellationToken
        );
        internalTestSuite.StudentSolutionSyntaxTree = studentSolutionSyntaxTree;

        Console.WriteLine($"[{internalTestSuite.Name}] Scanning used namespaces.");

        var usings = studentSolutionSyntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<UsingDirectiveSyntax>();
        
        foreach (var @using in usings)
        {
            var @namespace = @using.NamespaceOrType.ToString();
            
            if (!NamespaceWhitelist.Contains(@namespace))
            {
                throw new TestExecutionException($"{@namespace} namespace is not whitelisted!");
            }
        }
        
        return internalTestSuite;
    }
    
    internal static InternalTestSuite AnalyzeTestSuite(this InternalTestSuite internalTestSuite)
    {
        Console.WriteLine($"[{internalTestSuite.Name}] Beginning test suite static analysis.");

        var testSuiteSyntaxTree = CSharpSyntaxTree.ParseText(
            text: internalTestSuite.TestSuite.TestSuiteSourceCode(),
            cancellationToken: internalTestSuite.CancellationToken
        );
        internalTestSuite.TestSuiteSyntaxTree = testSuiteSyntaxTree;

        // TODO: actual analysis ;>
        
        return internalTestSuite;
    }
}