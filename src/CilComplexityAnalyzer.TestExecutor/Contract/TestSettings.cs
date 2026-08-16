using CilComplexityAnalyzer.TestExecutor.Contract.Settings;

namespace CilComplexityAnalyzer.TestExecutor.Contract;

public class TestSettings
{
    public ComplexityCalculationMethod ComplexityCalculationMethod { get; set; } =
        ComplexityCalculationMethod.CilInstructionCounting;
}