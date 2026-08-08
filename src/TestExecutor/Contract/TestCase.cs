using Microsoft.Extensions.Logging;

namespace TestExecutor.Contract;

public class TestCase
{
    // Public TestCase contract properties
    public string? Name { get; set; }
    public required string SourceFile { get; set; }
    public object?[]? Input { get; set; }
    public object? Output { get; set; }
    public TestSettings? Settings { get; set; }
    public ILogger? Logger { get; set; }
    
    // Internal properties needed for the testing flow
    internal string NameOrHash 
        => Name ?? SourceFile.GetHashCode().ToString();
    internal byte[]? ByteCode { get; set; }
}