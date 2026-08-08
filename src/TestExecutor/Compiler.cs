using Microsoft.Extensions.Logging;
using TestExecutor.Contract;

namespace TestExecutor;

internal static class Compiler
{
    internal static TestCase Compile(this TestCase testCase)
    {
        testCase.Logger?.LogInformation($"Beginning compilation for {testCase.NameOrHash}.");
        // TODO: un-mock
        testCase.ByteCode = new byte[10];
        return Random.Shared.Next(4) == 0 ? 
            throw new TestExecutionException(
                "Compiler internal error", 
                new Exception("Internal error")
            ) : 
            testCase;
    }
}