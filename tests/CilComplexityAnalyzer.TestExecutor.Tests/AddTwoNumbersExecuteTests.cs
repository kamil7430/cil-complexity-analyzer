using CilComplexityAnalyzer.TestExecutor.Contract;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CilComplexityAnalyzer.TestExecutor.Tests;

[TestClass]
public sealed class AddTwoNumbersExecuteTests
{
    private readonly ILogger _logger = Utils.GetLogger<AddTwoNumbersExecuteTests>();
    private const string AddTwoNumbersCode = """
        namespace MyApp
        {
            internal class Calculator
            {
                public int Add(int a, int b) => a + b;
            }
        }
    """;
    
    private TestSuite NewTestSuite(TestCase[] testCases)
        => new TestSuite
        {
            SourceCode = AddTwoNumbersCode,
            MethodToInvoke = "Add",
            Logger = _logger,
            TestCases = testCases,
        };
    
    [TestMethod]
    public void HappyPath_ShouldRunAndReturnSuccess()
    {
        var testCase = NewTestSuite(
        [
            new TestCase
            {
                Input = [2, 3],
                Output = 5,
            }
        ]);

        var result = TestExecutor.Execute(testCase)[0];
        
        Assert.IsTrue(result.IsT0);
    }
    
    [TestMethod]
    public void InvalidOutput_ShouldRunAndReturnFailure()
    {
        var testCase = NewTestSuite(
            [
                new TestCase
                {
                    Input = [2, 2],
                    Output = 5,
                }
            ]
        );

        var result = TestExecutor.Execute(testCase)[0];
        
        Assert.IsTrue(result.IsT1);
        Assert.IsTrue(result.AsT1.Message?.Contains("Outputs don't match!"));
    }
}