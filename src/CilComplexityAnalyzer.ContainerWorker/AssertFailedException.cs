namespace CilComplexityAnalyzer.ContainerWorker;

public class AssertFailedException : Exception
{
    internal AssertFailedException(string? message) : base(message) { }
    internal AssertFailedException(string? message, Exception? innerException) : base(message, innerException) { }
}