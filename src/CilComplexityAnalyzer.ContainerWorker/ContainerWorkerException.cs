namespace CilComplexityAnalyzer.ContainerWorker;

public class ContainerWorkerException : Exception
{
    public ContainerWorkerException(string? message) : base(message) { }
    public ContainerWorkerException(string? message, Exception? innerException) : base(message, innerException) { }
}