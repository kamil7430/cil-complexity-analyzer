using Microsoft.Extensions.Logging;
using TestExecutor.Contract;

namespace TestExecutor;

internal static class StaticAnalyzer
{
    internal static TestCase Analyze(this TestCase testCase)
    {
        testCase.Logger?.LogInformation($"Beginning static analysis for {testCase.NameOrHash}.");
        // TODO: un-mock
        return Random.Shared.Next(4) == 0 ? 
            throw new TestExecutionException("Forbidden instruction") : 
            testCase;
    }
}