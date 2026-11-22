namespace CilComplexityAnalyzer.Contract;

public record TestCaseSettings(
    long InstructionCap,
    long TimeoutMs,
    long MemoryMb
);