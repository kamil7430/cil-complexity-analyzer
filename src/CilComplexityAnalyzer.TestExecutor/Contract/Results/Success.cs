using CilComplexityAnalyzer.TestExecutor.Contract.Settings;

namespace CilComplexityAnalyzer.TestExecutor.Contract.Results;

public record Success(
    ComplexityCalculationMethod ComplexityCalculationMethod,
    long MeasuredComplexity
);