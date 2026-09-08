/*using CilComplexityAnalyzer.TestExecutor.Contract;
using CilComplexityAnalyzer.TestExecutor.Contract.Results;

namespace CilComplexityAnalyzer.TestExecutor.Tests;

public static class TestHelpers
{
    private sealed class DummyTestSuite : TestSuite
    {
        private readonly string _studentSourceCode;

        public override TestSuiteSettings? Settings()
        {
            return new TestSuiteSettings()
            {
                Containerized = true
            };
        }

        public DummyTestSuite(string studentSourceCode)
        {
            _studentSourceCode = studentSourceCode;
        }

        public override string StudentSolutionSourceCode() => _studentSourceCode;
        public override string TestSuiteSourceCode() => """
                                                        using CilComplexityAnalyzer.TestExecutor.Contract;

                                                        namespace CilComplexityAnalyzer.TestExecutor.Tests;

                                                        public class MyTestSuite : TestSuite
                                                        {
                                                            public override IEnumerable<TestCase> GetTestCases()
                                                            {
                                                                yield return new MyTestCase();
                                                            }
                                                            
                                                            public class MyTestCase : TestCase
                                                            {
                                                                public override int TestNumber() => 0;
                                                                public override TestCaseSettings Settings() => new();
                                                                public override void Arrange() { }
                                                                public override void Act() { }
                                                                public override void Assert() { }
                                                            }
                                                        }
                                                        """;

        public class DummyTestCase : TestCase
        {
            public object?[]? Input { get; set; }
            public object? ExpectedOutput { get; set; }

            public override int TestNumber() => 0;
            public override TestCaseSettings Settings() => new();

            public override void Arrange()
            {
            }
            public override void Act() { }
            public override void Assert() { }
        }
    }

    public static async Task<TestResult> RunSingleTestAsync(string studentCode, object?[] input, object? expectedOutput)
    {
        var suite = new DummyTestSuite(studentCode);
        
        var executor = new global::CilComplexityAnalyzer.TestExecutor.TestExecutor(suite);
        executor.BeginExecution();
        
        return await executor.GetResult(0);
    }
}
*/