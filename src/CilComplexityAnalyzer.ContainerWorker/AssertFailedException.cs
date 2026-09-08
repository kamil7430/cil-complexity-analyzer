namespace CilComplexityAnalyzer.ContainerWorker;

public class AssertFailedException : Exception
{
    public AssertFailedException(string? message) : base(message) { }
    public AssertFailedException(string? message, Exception? innerException) : base(message, innerException) { }
}