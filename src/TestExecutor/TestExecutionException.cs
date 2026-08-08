namespace TestExecutor;

public class TestExecutionException : Exception
{
    public TestExecutionException(string? message) : base(message) { }

    public TestExecutionException(string? message, Exception? innerException) : base(message, innerException) { }
}