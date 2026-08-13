namespace CilComplexityAnalyzer.ContainerWorker;

public record TestData(
    string MethodToInvoke,
    object?[]? Input,
    object? Output
);