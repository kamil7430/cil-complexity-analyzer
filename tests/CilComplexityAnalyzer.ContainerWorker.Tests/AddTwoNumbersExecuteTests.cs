using System.Reflection;

namespace CilComplexityAnalyzer.ContainerWorker.Tests;

[TestClass]
public sealed class AddTwoNumbersExecuteTests
{
    private readonly Assembly _addTwoNumbersAssembly = Utils.LoadAssembly("AddTwoNumbers");
    
    [TestMethod]
    public void HappyPath_ShouldRunAndReturnSuccess()
    {
        TestData[] testData =
        [
            new("Add", [2, 3], 5),
            new("Add", [10, 15], 25),
        ];

        Utils.InvokeExecuteAndCaptureException(testData, _addTwoNumbersAssembly, testResult =>
        {
            Assert.IsTrue(testResult.Success);
        });
    }
    
    [TestMethod]
    public void InvalidOutput_ShouldRunAndReturnFailure()
    {
        TestData[] testData =
        [
            new("Add", [2, 2], 5),
            new("Add", [10, 10], 25),
        ];

        Utils.InvokeExecuteAndCaptureException(testData, _addTwoNumbersAssembly, testResult =>
        {
            Assert.IsFalse(testResult.Success);
            Assert.IsTrue(testResult.Message?.Contains("Outputs don't match!"));
        });
    }

    [TestMethod]
    public void InvalidCasting_ShouldRunAndReturnFailure()
    {
        TestData[] testData =
        [
            new("Add", [2.5, 2.5], 5),
            new("Add", [10, 10.5], 20.5),
            new("Add", [10.5, 10], 20.5),
            new("Add", [10.3, 10.3], 20.6),
        ];

        Utils.InvokeExecuteAndCaptureException(testData, _addTwoNumbersAssembly, testResult =>
        {
            Assert.IsFalse(testResult.Success);
            Assert.IsTrue(testResult.Message?.Contains("Invalid cast from"));
        });
    }
}