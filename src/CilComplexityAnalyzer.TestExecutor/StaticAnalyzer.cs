using CilComplexityAnalyzer.TestExecutor.Contract;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.Logging;

namespace CilComplexityAnalyzer.TestExecutor;

internal static class StaticAnalyzer
{
    internal static TestCase Analyze(this TestCase testCase)
    {
        testCase.Logger?.LogInformation($"[{testCase.NameOrHash}] Beginning static analysis.");

        var syntaxTree = CSharpSyntaxTree.ParseText(
            text: testCase.SourceFile,
            cancellationToken: testCase.CancellationToken
        );
        testCase.SyntaxTree = syntaxTree;

        // TODO: actual analysis ;>
        
        return testCase;
    }
}