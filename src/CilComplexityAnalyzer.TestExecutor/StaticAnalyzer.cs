using Microsoft.CodeAnalysis.CSharp;

namespace CilComplexityAnalyzer.TestExecutor;

internal static class StaticAnalyzer
{
    internal static InternalTestSuite AnalyzeStudentSolution(this InternalTestSuite internalTestSuite)
    {
        Console.WriteLine($"[{internalTestSuite.Name}] Beginning student solution static analysis.");

        var studentSolutionSyntaxTree = CSharpSyntaxTree.ParseText(
            text: internalTestSuite.TestSuite.StudentSolutionSourceCode(),
            cancellationToken: internalTestSuite.CancellationToken
        );
        internalTestSuite.StudentSolutionSyntaxTree = studentSolutionSyntaxTree;

        // TODO: actual analysis ;>
        
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