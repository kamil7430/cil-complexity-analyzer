using CilComplexityAnalyzer.TestExecutor.Contract.Settings;

namespace CilComplexityAnalyzer.TestExecutor.Contract;

public class TestSettings
{
    public ComplexityCalculationMethod ComplexityCalculationMethod { get; set; } =
        ComplexityCalculationMethod.CilInstructionCounting;
    public long InstructionCap { get; set; } = 1_000_000;
    public long TimeoutMs { get; set; } = 30_000;
    public int MemoryMb { get; set; } = 512;
}