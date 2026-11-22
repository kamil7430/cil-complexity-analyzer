using System.Diagnostics;
using CilComplexityAnalyzer.Contract;

namespace CilComplexityAnalyzer.TestExecutor;

public class TestExecutor
{
    private InternalTestSuite _internalTestSuite;
    private TestResult?[] _results;
    private TaskCompletionSource<bool>[] _resultsTcs;
    private int _i = 0;
    private bool _started = false;
    private Lock _startedLock = new();

    public TestExecutor(TestSuite testSuite, CancellationToken cancellationToken)
    {
        _internalTestSuite = new InternalTestSuite(testSuite, cancellationToken);
        var length = _internalTestSuite.TestCases.Value.Length;
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
            Console.WriteLine($"[{_internalTestSuite.Name}] Debugger is attached. The tests will fail.");
            FillResultsWithFailures("Debugger is attached.");

            foreach (var testCase in _internalTestSuite.TestCases.Value)
            {
                testCase.Arrange();
                testCase.Act();
                testCase.Assert();
            }
        }
        
        Task.Run(() =>
        {
            Console.WriteLine($"[{_internalTestSuite.Name}] Beginning test execution.");
            try
            {
                _internalTestSuite.AnalyzeStudentSolution()
                    .CompileStudentSolution()
                    .InjectCilToStudentSolution();

                _internalTestSuite.AnalyzeTestSuite()
                    .CompileTestSuite();
                    
                foreach (var result in _internalTestSuite.Execute())
                    SetResult(_i++, result);
            }
            catch (TestExecutionException e)
            {
                Console.WriteLine($"[{_internalTestSuite.Name}] Test execution failed: {e.Message}");
                FillResultsWithFailures($"Test execution failed: {e.Message}");
            }
            catch (Exception e)
            {
                Console.WriteLine(
                    $"[{_internalTestSuite.Name}] TestExecutor internal error or uncaught exception: {e.Message}"
                );
                FillResultsWithFailures($"TestExecutor internal error or uncaught exception: {e.Message}");
            }

            Console.WriteLine($"[{_internalTestSuite.Name}] Ending test execution.");
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
        {
            SetResult(_i, new TestResult(false, null, message));
        }
    }

    public async Task<TestResult> GetResult(int i)
    {
        await _resultsTcs[i].Task;
        return _results[i]!;
    }
}