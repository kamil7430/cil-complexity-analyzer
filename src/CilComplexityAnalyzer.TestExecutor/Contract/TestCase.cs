namespace CilComplexityAnalyzer.TestExecutor.Contract;

public abstract class TestCase
{
    public string? Name { get; set; }
    public object?[]? Input { get; set; }
    public object? Output { get; set; }
    public long InstructionCap { get; set; } = 1_000_000;
    public long TimeoutMs { get; set; } = 30_000;
    public int MemoryMb { get; set; } = 512;
    public abstract void Arrange();

    public abstract void Assert();
}