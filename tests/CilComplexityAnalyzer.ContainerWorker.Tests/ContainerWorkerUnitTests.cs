using System.Reflection;

namespace CilComplexityAnalyzer.ContainerWorker.Tests;

[TestClass]
public sealed class ContainerWorkerUnitTests : ContainerWorkerUnitTestsBase
{
    private readonly Assembly _addTwoNumbersAssembly = LoadAssembly("AddTwoNumbers");

    private static Assembly LoadAssembly(string assemblyName)
    {
        using var stream = Assembly.GetExecutingAssembly()
            .GetManifestResourceStream($"CilComplexityAnalyzer.ContainerWorker.Tests.Assemblies.{assemblyName}.dll");
        using var memoryStream = new MemoryStream();
        stream!.CopyTo(memoryStream);
        return Assembly.Load(memoryStream.ToArray());
    }
    
    [TestMethod]
    public void Execute_AddTwoNumbersExample_HappyPath_ShouldRunAndReturnSuccess()
    {
        TestData[] testData =
        [
            new("Add", [2, 3], 5),
            new("Add", [10, 15], 25),
        ];

        CaptureException(() =>
            Program.Execute(testData, _addTwoNumbersAssembly, testResult =>
            {
                Assert.IsTrue(testResult.Success);
            })
        );
    }
    
    [TestMethod]
    public void Execute_AddTwoNumbersExample_InvalidOutput_ShouldRunAndReturnFailure()
    {
        TestData[] testData =
        [
            new("Add", [2, 2], 5),
            new("Add", [10, 10], 25),
        ];

        CaptureException(() =>
            Program.Execute(testData, _addTwoNumbersAssembly, testResult =>
            {
                Assert.IsFalse(testResult.Success);
                Assert.IsTrue(testResult.Message?.Contains("Outputs don't match!"));
            })
        );
    }

    [TestMethod]
    public void Execute_AddTwoNumbersExample_InvalidCasting_ShouldRunAndReturnFailure()
    {
        TestData[] testData =
        [
            new("Add", [2.5, 2.5], 5),
            new("Add", [10, 10.5], 20.5),
            new("Add", [10.5, 10], 20.5),
            new("Add", [10.3, 10.3], 20.6),
        ];

        CaptureException(() =>
            Program.Execute(testData, _addTwoNumbersAssembly, testResult =>
            {
                Assert.IsFalse(testResult.Success);
                Assert.IsTrue(testResult.Message?.Contains("Invalid cast from"));
            })
        );
    }
}