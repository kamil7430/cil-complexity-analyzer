using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.Logging;

namespace CilComplexityAnalyzer.TestExecutor;

internal static class StaticAnalyzer
{
    internal static InternalTestSuite AnalyzeStudentSolution(this InternalTestSuite internalTestSuite)
    {
        internalTestSuite.TestSuite.Logger()
            ?.LogInformation($"[{internalTestSuite.Name}] Beginning student solution static analysis.");

        var studentSolutionSyntaxTree = CSharpSyntaxTree.ParseText(
            text: internalTestSuite.TestSuite.StudentSolutionSourceCode(),
            cancellationToken: internalTestSuite.TestSuite.CancellationToken()
        );
        internalTestSuite.StudentSolutionSyntaxTree = studentSolutionSyntaxTree;

        // TODO: actual analysis ;>
        
        return internalTestSuite;
    }
    
    internal static InternalTestSuite AnalyzeTestSuite(this InternalTestSuite internalTestSuite)
    {
        internalTestSuite.TestSuite.Logger()
            ?.LogInformation($"[{internalTestSuite.Name}] Beginning test suite static analysis.");

        var testSuiteSyntaxTree = CSharpSyntaxTree.ParseText(
            text: internalTestSuite.TestSuite.TestSuiteSourceCode(),
            cancellationToken: internalTestSuite.TestSuite.CancellationToken()
        );
        internalTestSuite.TestSuiteSyntaxTree = testSuiteSyntaxTree;

        // TODO: actual analysis ;>
        
        return internalTestSuite;
    }
}