using CilComplexityAnalyzer.TestExecutor.Contract;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CilComplexityAnalyzer.TestExecutor.Tests;

[TestClass]
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
    
    [TestMethod]
    public void Execute_AddTwoNumbersExample_HappyPath_ShouldRunAndReturnSuccess()
    {
        var testCase = new TestCase
        {
            SourceCode = AddTwoNumbersCode,
            MethodToInvoke = "Add",
            Input = [2, 3],
            Output = 5,
        };

        var result = TestExecutor.Execute(testCase);
        
        Assert.IsTrue(result.IsT0);
    }
    
    [TestMethod]
    public void Execute_AddTwoNumbersExample_InvalidOutput_ShouldRunAndReturnFailure()
    {
        var testCase = new TestCase
        {
            SourceCode = AddTwoNumbersCode,
            MethodToInvoke = "Add",
            Input = [2, 3],
            Output = 4,
        };

        var result = TestExecutor.Execute(testCase);
        
        Assert.IsTrue(result.IsT1);
        Assert.IsTrue(result.AsT1.Message?.Contains("Outputs don't match!"));
    }
}