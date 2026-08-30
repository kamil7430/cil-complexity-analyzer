using CilComplexityAnalyzer.TestExecutor.Contract;
using Microsoft.Extensions.Logging;

namespace CilComplexityAnalyzer.TestExecutor;

internal static class CilInstructionInjector
{
    internal static TestSuite InjectCil(this TestSuite testSuite)
    {
        testSuite.Logger()?.LogInformation($"[{testSuite.Name}] Beginning CIL instruction injection.");
        
        // TODO: inject CIL
        // TODO: inject abort mechanism
        
        return testSuite;
    }
}