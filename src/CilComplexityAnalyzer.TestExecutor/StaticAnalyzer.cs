using CilComplexityAnalyzer.TestExecutor.Contract;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.Logging;

namespace CilComplexityAnalyzer.TestExecutor;

internal static class StaticAnalyzer
{
    internal static TestSuite Analyze(this TestSuite testSuite)
    {
        testSuite.Logger?.LogInformation($"[{testSuite.NameOrHash}] Beginning static analysis.");

        var syntaxTree = CSharpSyntaxTree.ParseText(
            text: testSuite.SourceCode,
            cancellationToken: testSuite.CancellationToken
        );
        testSuite.SyntaxTree = syntaxTree;

        // TODO: actual analysis ;>
        
        return testSuite;
    }
}