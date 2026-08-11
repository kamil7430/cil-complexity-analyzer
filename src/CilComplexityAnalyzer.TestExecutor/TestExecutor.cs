using CilComplexityAnalyzer.TestExecutor.Contract;
using CilComplexityAnalyzer.TestExecutor.Contract.Results;
using Microsoft.Extensions.Logging;

namespace CilComplexityAnalyzer.TestExecutor;

public static class TestExecutor
{
    public static TestResult Execute(TestCase testCase)
    {
        testCase.Logger?.LogInformation($"[{testCase.NameOrHash}] Beginning test execution.");
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
            testCase.Logger?.LogInformation($"[{testCase.NameOrHash}] Test execution failed: {e.Message}");
            result = new Failure($"Test execution failed: {e.Message}");
        }
        catch (Exception e)
        {
            testCase.Logger?.LogInformation($"[{testCase.NameOrHash}] TestExecutor internal error or uncaught exception: {e.Message}");
            result = new Failure($"TestExecutor internal error or uncaught exception: {e.Message}");
        }
        testCase.Logger?.LogInformation($"[{testCase.NameOrHash}] Ending test execution.");
        return result;
    }
}