using CilComplexityAnalyzer.TestExecutor.Contract;
using CilComplexityAnalyzer.TestExecutor.Contract.Results;
using Microsoft.Extensions.Logging;

namespace CilComplexityAnalyzer.TestExecutor;

public static class TestExecutor
{
    private static bool _initialized = false;
    private static Lock _initializedBoolLock = new();
    
    public static void Initialize(ILogger? logger)
    {
        if (_initialized)
            return;
        
        lock (_initializedBoolLock)
        {
            if (_initialized)
                return;
            
            logger?.LogInformation("TestExecutor is uninitialized. Beginning initialization.");
            
            Compiler.Initialize(logger);
            Executor.Initialize(logger);
            _initialized = true;
            
            logger?.LogInformation("TestExecutor initialization finished.");
        }
    }
    
    public static TestResult Execute(TestCase testCase)
    {
        Initialize(testCase.Logger);
        
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