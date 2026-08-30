using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace CilComplexityAnalyzer.TestExecutor.Contract;

public abstract class TestSuite
{
    // Public TestSuite contract methods
    public abstract string SourceCode();
    public abstract string MethodToInvoke();
    public virtual TestSuiteSettings? Settings() 
        => null;
    public virtual ILogger? Logger() 
        => null;
    public virtual CancellationToken CancellationToken() 
        => System.Threading.CancellationToken.None;
    
    // Internal properties needed for the testing flow
    internal string Name
        => GetType().ToString();
    internal IEnumerable<Type> TestCaseTypes()
        => GetType().GetMembers().Select(m => m.ReflectedType).Where(t => t?.IsSubclassOf(typeof(TestCase)) ?? false)!;
    internal TestCase[] TestCases()
        => TestCaseTypes().Select(t => (TestCase)Activator.CreateInstance(t)!).ToArray();
    internal SyntaxTree? SyntaxTree { get; set; }
    internal byte[]? AssemblyBytes { get; set; }
}