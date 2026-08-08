using Microsoft.Extensions.Logging;
using TestExecutor.Contract;
using TestExecutor.Contract.Results;

namespace TestExecutor;

public static class TestExecutor
{
    public static TestResult Execute(TestCase testCase)
    {
        testCase.Logger?.LogInformation($"Beginning test execution for {testCase.NameOrHash}.");
        TestResult result;
        try
        {
            result = testCase
                .Analyze()
                .Compile()
                .InjectCil()
                .Execute();
        }
        catch (TestExecutionException e)
        {
            testCase.Logger?.LogInformation($"Test execution for {testCase.NameOrHash} failed: {e.Message}");
            result = new Failure($"Test execution failed: {e.Message}");
        }
        testCase.Logger?.LogInformation($"Ending test execution for {testCase.NameOrHash}.");
        return result;
    }
}