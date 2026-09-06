using System.Diagnostics.CodeAnalysis;
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

    // Konkretne klasy testowe zastępujące wcześniejsze instancjonowanie klas abstrakcyjnych
    private sealed class ConcreteTestCase : TestCase
    {
        public ConcreteTestCase(object?[]? input, object? output)
        {
            Input = input;
            Output = output;
        }

        public override void Arrange() { }
        public override void Assert() { }
    }

    private sealed class ConcreteTestSuite : TestSuite
    {
        [SetsRequiredMembers]
        public ConcreteTestSuite(string sourceCode, string methodToInvoke, ILogger logger, TestCase[] testCases)
        {
            SourceCode = sourceCode;
            MethodToInvoke = methodToInvoke;
            Logger = logger;
            TestCases = testCases;
        }
    }

    private TestSuite NewTestSuite(TestCase[] testCases)
        => new ConcreteTestSuite(AddTwoNumbersCode, "Add", _logger, testCases);

    [TestMethod]
    public void HappyPath_ShouldRunAndReturnSuccess()
    {
        var suite = NewTestSuite([
            new ConcreteTestCase(input: [2, 3], output: 5)
        ]);

        var result = TestExecutor.Execute(suite)[0];

        Assert.IsTrue(result.IsT0);
    }

    [TestMethod]
    public void InvalidOutput_ShouldRunAndReturnFailure()
    {
        var suite = NewTestSuite([
            new ConcreteTestCase(input: [2, 2], output: 5)
        ]);

        var result = TestExecutor.Execute(suite)[0];

        Assert.IsTrue(result.IsT1);
        Assert.IsTrue(result.AsT1.Message?.Contains("Outputs don't match!"));
    }
}