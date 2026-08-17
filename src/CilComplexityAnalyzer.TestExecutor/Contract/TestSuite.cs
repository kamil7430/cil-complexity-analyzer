using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace CilComplexityAnalyzer.TestExecutor.Contract;

public class TestSuite
{
    // Public TestSuite contract properties
    public string? Name { get; set; }
    public required string SourceCode { get; set; }
    public required string MethodToInvoke { get; set; }
    public required TestCase[] TestCases { get; set; }
    public TestSettings? Settings { get; set; }
    public ILogger? Logger { get; set; }
    public CancellationToken CancellationToken { get; set; }
    
    // Internal properties needed for the testing flow
    internal string NameOrHash 
        => Name ?? SourceCode.GetHashCode().ToString("x8");
    internal SyntaxTree? SyntaxTree { get; set; }
    internal byte[]? AssemblyBytes { get; set; }
}