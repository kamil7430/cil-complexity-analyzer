namespace CilComplexityAnalyzer.ContainerWorkerUtils;

public record TestResult(
    bool Success,
    long? MeasuredComplexity,
    string? Message
);