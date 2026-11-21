using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace CilComplexityAnalyzer.Contract;

public abstract class TestSuite
{
    // Public TestSuite contract methods
    public virtual string StudentSolutionSourceCode()
        => throw new NotImplementedException();
    public virtual string TestSuiteSourceCode()
        => throw new NotImplementedException();
    public abstract TestSuiteSettings Settings();
    public virtual ILogger? Logger() 
        => null;
    public virtual CancellationToken CancellationToken() 
        => System.Threading.CancellationToken.None;
    
    // Internal properties needed for the testing flow
    internal string Name
        => GetType().ToString();
    internal Lazy<Type[]> TestCaseTypes
        => new(() => GetType()
            .GetNestedTypes(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic)
            .Where(t => t.IsSubclassOf(typeof(TestCase)) && !t.IsAbstract)
            .ToArray()
        );
    internal Lazy<TestCase[]> TestCases
        => new(() => TestCaseTypes.Value.Select(t => (TestCase)Activator.CreateInstance(t)!).ToArray());
    internal SyntaxTree? StudentSolutionSyntaxTree { get; set; }
    internal byte[]? StudentSolutionAssemblyBytes { get; set; }
    internal SyntaxTree? TestSuiteSyntaxTree { get; set; }
    internal byte[]? TestSuiteAssemblyBytes { get; set; }
}