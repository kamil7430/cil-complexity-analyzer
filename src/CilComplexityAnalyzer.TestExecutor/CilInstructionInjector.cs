using CilComplexityAnalyzer.TestExecutor.Contract;
using Microsoft.Extensions.Logging;

namespace CilComplexityAnalyzer.TestExecutor;

internal static class CilInstructionInjector
{
    internal static TestCase InjectCil(this TestCase testCase)
    {
        testCase.Logger?.LogInformation($"[{testCase.NameOrHash}] Beginning CIL instruction injection.");
        
        // TODO: inject CIL
        // TODO: inject abort mechanism
        
        return testCase;
    }
}