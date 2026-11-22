using System.Reflection;
using CilComplexityAnalyzer.Contract;
using CilComplexityAnalyzer.Contract.CaseAttributes;
using Microsoft.CodeAnalysis;

namespace CilComplexityAnalyzer.TestExecutor;

internal class InternalTestSuite(TestSuite testSuite, CancellationToken cancellationToken)
{
    internal TestSuite TestSuite { get; } = testSuite;

    internal CancellationToken CancellationToken { get; } = cancellationToken;
    
    internal string Name 
        => TestSuite.GetType().ToString();
    
    internal Lazy<TestCase[]> TestCases
        => new(() => TestSuite.GetType()
            .GetNestedTypes()
            .Where(t => t.GetCustomAttribute<TestCaseAttribute>() is not null)
            .Select(t => (TestCase)Activator.CreateInstance(t)!)
            .OrderBy(o => o.TestNumber())
            .ToArray()
        );
    
    internal SyntaxTree? StudentSolutionSyntaxTree { get; set; }
    
    internal byte[]? StudentSolutionAssemblyBytes { get; set; }
    
    internal SyntaxTree? TestSuiteSyntaxTree { get; set; }
    
    internal byte[]? TestSuiteAssemblyBytes { get; set; }
}