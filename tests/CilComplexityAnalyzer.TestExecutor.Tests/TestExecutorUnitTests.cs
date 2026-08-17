using CilComplexityAnalyzer.TestExecutor.Contract;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CilComplexityAnalyzer.TestExecutor.Tests;

[TestClass]
public sealed class TestExecutorUnitTests
{
    private readonly ILogger _logger = LoggerFactory.Create(b => b.AddConsole()).CreateLogger<TestExecutorUnitTests>();
    private const string AddTwoNumbersCode = """
        namespace MyApp
        {
            internal class Calculator
            {
                public int Add(int a, int b) => a + b;
            }
        }
    """;
    
    [TestMethod]
    public void Execute_AddTwoNumbersExample_HappyPath_ShouldRunAndReturnSuccess()
    {
        var testCase = new TestSuite
        {
            SourceCode = AddTwoNumbersCode,
            MethodToInvoke = "Add",
            Logger = _logger,
            TestCases =
            [
                new TestCase
                {
                    Input = [2, 3],
                    Output = 5,
                }
            ]
        };

        var result = TestExecutor.Execute(testCase)[0];
        
        Assert.IsTrue(result.IsT0);
    }
    
    [TestMethod]
    public void Execute_AddTwoNumbersExample_InvalidOutput_ShouldRunAndReturnFailure()
    {
        var testCase = new TestSuite
        {
            SourceCode = AddTwoNumbersCode,
            MethodToInvoke = "Add",
            Logger = _logger,
            TestCases =
            [
                new TestCase
                {
                    Input = [2, 2],
                    Output = 5,
                }
            ]
        };

        var result = TestExecutor.Execute(testCase)[0];
        
        Assert.IsTrue(result.IsT1);
        Assert.IsTrue(result.AsT1.Message?.Contains("Outputs don't match!"));
    }
}