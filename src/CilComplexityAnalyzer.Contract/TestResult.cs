namespace CilComplexityAnalyzer.Contract;

public record TestResult(
    bool Success,
    long? MeasuredComplexity,
    string? Message
);