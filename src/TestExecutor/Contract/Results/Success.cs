using TestExecutor.Contract.Settings;

namespace TestExecutor.Contract.Results;

public record Success(
    ComplexityCalculationMethod ComplexityCalculationMethod,
    long MeasuredComplexity
);