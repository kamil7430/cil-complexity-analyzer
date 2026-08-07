namespace TestExecutor.Contract;

public class TestCase
{
    public required Stream SourceFile { get; set; }
    public object?[]? Input { get; set; }
    public object? Output { get; set; }
    public TestSettings? Settings { get; set; }
    // TODO: logger interface
}