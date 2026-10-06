using System.Diagnostics;
using CilComplexityAnalyzer.Contract;
using CilComplexityAnalyzer.Contract.Results;
using Microsoft.Extensions.Logging;

namespace CilComplexityAnalyzer.TestExecutor;

public class TestExecutor
{
    private TestSuite _testSuite;
    private TestResult?[] _results;
    private TaskCompletionSource<bool>[] _resultsTcs;
    private int _i = 0;
    private bool _started = false;
    private Lock _startedLock = new();

    public TestExecutor(TestSuite testSuite)
    {
        _testSuite = testSuite;
        var length = testSuite.TestCases.Value.Length;
        _results = new TestResult[length];
        _resultsTcs = new TaskCompletionSource<bool>[length];
        for (int i = 0; i < length; i++)
        {
            _resultsTcs[i] = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        }
    }
    
    public void BeginExecution()
    {
        if (_started)
            return;

        lock (_startedLock)
        {
            if (_started)
                return;
            _started = true;
        }

        // Skip our whole CIL-injecting-magic when the debugger is attached
        if (Debugger.IsAttached)
        {
            _testSuite.Logger()?.LogInformation($"[{_testSuite.Name}] Debugger is attached. The tests will fail.");
            FillResultsWithFailures("Debugger is attached.");

            foreach (var testCase in _testSuite.TestCases.Value)
            {
                testCase.Arrange();
                testCase.Act();
                testCase.Assert();
            }
        }
        
        Task.Run(() =>
        {
            _testSuite.Logger()?.LogInformation($"[{_testSuite.Name}] Beginning test execution.");
            try
            {
                _testSuite.AnalyzeStudentSolution()
                    .CompileStudentSolution()
                    .InjectCilToStudentSolution();

                _testSuite.AnalyzeTestSuite()
                    .CompileTestSuite()
                    .LinkTestSuite();
                    
                foreach (var result in _testSuite.Execute())
                    SetResult(_i++, result);
            }
            catch (TestExecutionException e)
            {
                _testSuite.Logger()?.LogInformation($"[{_testSuite.Name}] Test execution failed: {e.Message}");
                FillResultsWithFailures($"Test execution failed: {e.Message}");
            }
            catch (Exception e)
            {
                _testSuite.Logger()
                    ?.LogInformation(
                        $"[{_testSuite.Name}] TestExecutor internal error or uncaught exception: {e.Message}");
                FillResultsWithFailures($"TestExecutor internal error or uncaught exception: {e.Message}");
            }

            _testSuite.Logger()?.LogInformation($"[{_testSuite.Name}] Ending test execution.");
        });
    }

    private void SetResult(int i, TestResult result)
    {
        _results[i] = result;
        _resultsTcs[i].SetResult(true);
    }

    private void FillResultsWithFailures(string message)
    {
        for (; _i < _results.Length; _i++)
            SetResult(_i, new Failure(message));
    }

    public async Task<TestResult> GetResult(int i)
    {
        if (_testSuite.TestCases is null || _testSuite.TestCases.Value.Length == 0)
        {
            throw new InvalidOperationException(
                $"TestSuite '{_testSuite.GetType().Name}' does not contain any TestCases. Ensure TestCases array is initialized.");
        }

        if (i < 0 || i >= _testSuite.TestCases.Value.Length)
        {
            throw new ArgumentOutOfRangeException(
                nameof(i), 
                $"Requested test index {i} is out of bounds. Suite only contains {_testSuite.TestCases.Value.Length} test cases.");
        }
        await _resultsTcs[i].Task;
        return _results[i]!;
    }
}