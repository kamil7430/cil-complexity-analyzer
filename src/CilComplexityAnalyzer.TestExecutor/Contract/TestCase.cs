namespace CilComplexityAnalyzer.TestExecutor.Contract;

public abstract class TestCase
{
    public object?[]? Input { get; set; }
    public abstract TestCaseSettings Settings();
    public abstract void Arrange();
    public abstract void Assert();
}