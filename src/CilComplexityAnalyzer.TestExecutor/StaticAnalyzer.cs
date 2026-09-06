using CilComplexityAnalyzer.TestExecutor.Contract;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.Logging;

namespace CilComplexityAnalyzer.TestExecutor;

internal static class StaticAnalyzer
{
    internal static TestSuite AnalyzeStudentSolution(this TestSuite testSuite)
    {
        testSuite.Logger()?.LogInformation($"[{testSuite.Name}] Beginning student solution static analysis.");

        var studentSolutionSyntaxTree = CSharpSyntaxTree.ParseText(
            text: testSuite.StudentSolutionSourceCode(),
            cancellationToken: testSuite.CancellationToken()
        );
        testSuite.StudentSolutionSyntaxTree = studentSolutionSyntaxTree;

        // TODO: actual analysis ;>
        
        return testSuite;
    }
    
    internal static TestSuite AnalyzeTestSuite(this TestSuite testSuite)
    {
        testSuite.Logger()?.LogInformation($"[{testSuite.Name}] Beginning test suite static analysis.");

        var testSuiteSyntaxTree = CSharpSyntaxTree.ParseText(
            text: testSuite.TestSuiteSourceCode(),
            cancellationToken: testSuite.CancellationToken()
        );
        testSuite.TestSuiteSyntaxTree = testSuiteSyntaxTree;

        // TODO: actual analysis ;>
        
        return testSuite;
    }
}