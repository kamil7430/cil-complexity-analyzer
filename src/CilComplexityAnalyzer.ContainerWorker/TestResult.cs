namespace CilComplexityAnalyzer.ContainerWorker;

public record TestResult(
    bool Success,
    long? MeasuredComplexity,
    string? Message
);