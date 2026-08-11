using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace CilComplexityAnalyzer.TestExecutor.Contract;

public class TestCase
{
    // Public TestCase contract properties
    public string? Name { get; set; }
    public required string SourceFile { get; set; }
    public required string MethodToInvoke { get; set; }
    public object?[]? Input { get; set; }
    public object? Output { get; set; }
    public TestSettings? Settings { get; set; }
    public ILogger? Logger { get; set; }
    public CancellationToken CancellationToken { get; set; }
    
    // Internal properties needed for the testing flow
    internal string NameOrHash 
        => Name ?? SourceFile.GetHashCode().ToString();
    internal SyntaxTree? SyntaxTree { get; set; }
    internal Assembly? Assembly { get; set; }
}