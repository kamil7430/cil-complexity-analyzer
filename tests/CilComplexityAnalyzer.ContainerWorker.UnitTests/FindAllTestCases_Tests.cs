namespace CilComplexityAnalyzer.ContainerWorker.UnitTests;

[TestClass]
public class FindAllTestCases_Tests
{
    public TestContext TestContext { get; set; }
    
    [TestMethod]
    public void HappyPath_ShouldReturn2TestCases()
    {
        var code = """
            using CilComplexityAnalyzer.Contract.CaseAttributes;
            using CilComplexityAnalyzer.Contract.SuiteAttributes;
            
            [TestSuite]
            public partial class Suite1
            {
                [TestCase]
                public partial class Case1 { }
              
                [TestCase]
                public partial class Case2 { }
            }
            """;
        var assembly = Utils.CompileTestSuiteMock(code, TestContext.CancellationToken);
        
        var testCases = Program.FindAllTestCases(assembly);
        
        Assert.HasCount(2, testCases);
    }
    
    [TestMethod]
    public void TwoTestSuites_ShouldThrowException()
    {
        var code = """
            using CilComplexityAnalyzer.Contract.CaseAttributes;
            using CilComplexityAnalyzer.Contract.SuiteAttributes;
            
            [TestSuite]
            public partial class Suite1 { }
            
            [TestSuite]
            public partial class Suite2 { }
            """;
        var assembly = Utils.CompileTestSuiteMock(code, TestContext.CancellationToken);
        
        var action = () => Program.FindAllTestCases(assembly);
        Assert.Throws<ContainerWorkerException>(action);
    }
    
    [TestMethod]
    public void EmptyTestSuite_ShouldThrowException()
    {
        var code = """
            using CilComplexityAnalyzer.Contract.CaseAttributes;
            using CilComplexityAnalyzer.Contract.SuiteAttributes;
            
            [TestSuite]
            public partial class Suite1 { }
            """;
        var assembly = Utils.CompileTestSuiteMock(code, TestContext.CancellationToken);
        
        var action = () => Program.FindAllTestCases(assembly);
        Assert.Throws<ContainerWorkerException>(action);
    }
    
    [TestMethod]
    public void ForgottenTestCaseAttribute_ShouldNotCountThatCase()
    {
        var code = """
            using CilComplexityAnalyzer.Contract.CaseAttributes;
            using CilComplexityAnalyzer.Contract.SuiteAttributes;
            
            [TestSuite]
            public partial class Suite1 
            {
                [TestCase]
                public partial class Case1 { }
              
                [TestCase]
                public partial class Case2 { }
            
                public partial class Case3 { }
            
                public partial class Case4 { }
            }
            """;
        var assembly = Utils.CompileTestSuiteMock(code, TestContext.CancellationToken);
        
        var testCases = Program.FindAllTestCases(assembly);
        
        Assert.HasCount(2, testCases);
    }
}