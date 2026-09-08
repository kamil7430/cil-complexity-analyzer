using CilComplexityAnalyzer.Contract;
using Microsoft.Extensions.Logging;

namespace CilComplexityAnalyzer.TestExecutor;

internal static class Linker
{
    internal static TestSuite LinkTestSuite(this TestSuite testSuite)
    {
        testSuite.Logger()?.LogInformation($"[{testSuite.Name}] Beginning linking test suite.");
        
        // TODO: link modified student solution to test suite's Act methods
        
        return testSuite;
    }
}