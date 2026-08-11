using CilComplexityAnalyzer.TestExecutor.Contract;

namespace CilComplexityAnalyzer.TestExecutor.Tests;

public class TestExecutorTests
{
    private const string AddTwoNumbersCode = """
        namespace MyApp
        {
            internal class Calculator
            {
                public int Add(int a, int b) => a + b;
            }
        }
    """;
    
    [Fact]
    public void Execute_AddTwoNumbersExample_ShouldRunAndReturnSuccess()
    {
        var testCase = new TestCase
        {
            SourceCode = AddTwoNumbersCode,
            MethodToInvoke = "Add",
            Input = [2, 3],
            Output = 5,
        };

        var result = TestExecutor.Execute(testCase);
        
        Assert.True(result.IsT0);
    }
}