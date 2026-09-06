using CilComplexityAnalyzer.TestExecutor.Contract;
using CilComplexityAnalyzer.TestExecutor.Contract.Results;
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
                {
                    _results[_i] = result;
                    _resultsTcs[_i].SetResult(true);
                    _i++;
                }
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

    private void FillResultsWithFailures(string message)
    {
        for (; _i < _results.Length; _i++)
        {
            _results[_i] = new Failure(message);
            _resultsTcs[_i].SetResult(true);
            _i++;
        }
    }

    public async Task<TestResult> GetResult(int i)
    {
        await _resultsTcs[i].Task;
        return _results[i]!;
    }
}