namespace CilComplexityAnalyzer.TestExecutor;

internal class TestExecutionException : Exception
{
    internal TestExecutionException(string? message) : base(message) { }
    internal TestExecutionException(string? message, Exception? innerException) : base(message, innerException) { }
}