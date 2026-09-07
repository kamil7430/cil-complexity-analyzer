using CilComplexityAnalyzer.TestExecutor.Contract;
using CilComplexityAnalyzer.TestExecutor.Contract.Results;

namespace CilComplexityAnalyzer.TestExecutor.Tests;

public static class TestHelpers
{
    private sealed class DummyTestCase(object?[]? input, object? output) : TestCase
    {
        public override int TestNumber() => 0;
        public override TestCaseSettings Settings() => new();
        public override void Arrange() { Input = input; Output = output; }
        public override void Act() { }
        public override void Assert() { }
    }

    private sealed class DummyTestSuite(string sourceCode, TestCase[] cases) : TestSuite
    {
        public override string StudentSolutionSourceCode() => sourceCode;
        public override string TestSuiteSourceCode() => string.Empty;
    }

    public static async Task<TestResult> RunSingleTestAsync(string studentCode, object?[] input, object? expectedOutput)
    {
        var testCase = new DummyTestCase(input, expectedOutput);
        var suite = new DummyTestSuite(studentCode, [testCase]);
        
        var executor = new global::CilComplexityAnalyzer.TestExecutor.TestExecutor(suite);
        executor.BeginExecution();
        
        return await executor.GetResult(0);
    }
}