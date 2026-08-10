using Microsoft.Extensions.Logging;
using TestExecutor.Contract;

namespace TestExecutor;

internal static class CilInstructionInjector
{
    internal static TestCase InjectCil(this TestCase testCase)
    {
        testCase.Logger?.LogInformation($"[{testCase.NameOrHash}] Beginning CIL instruction injection.");
        
        // TODO: inject CIL
        
        return testCase;
    }
}