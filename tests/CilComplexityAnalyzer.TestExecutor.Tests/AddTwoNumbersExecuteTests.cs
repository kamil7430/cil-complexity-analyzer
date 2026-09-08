/*using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CilComplexityAnalyzer.TestExecutor.Tests;

[TestClass]
public sealed class AddTwoNumbersExecuteTests
{
    private const string AddTwoNumbersCode = """
                                                 namespace MyApp;

                                                 public class Calculator
                                                 {
                                                     public int Add(int a, int b) => a + b;
                                                 }
                                             """;

    [TestMethod]
    public async Task HappyPath_ShouldRunAndReturnSuccess()
    {
        var result = await TestHelpers.RunSingleTestAsync(AddTwoNumbersCode, input: [2, 3], expectedOutput: 5);

        Assert.IsTrue(result.IsT0, $"Expected Success, but got: {result.Match(_ => null, f => f.Message)}");
    }

    [TestMethod]
    public async Task InvalidOutput_ShouldRunAndReturnFailure()
    {
        var result = await TestHelpers.RunSingleTestAsync(AddTwoNumbersCode, input: [2, 2], expectedOutput: 5);

        Assert.IsTrue(result.IsT1, "Expected test to fail, but it succeeded.");
        StringAssert.Contains(result.AsT1.Message, "Outputs don't match!");
    }
}
*/