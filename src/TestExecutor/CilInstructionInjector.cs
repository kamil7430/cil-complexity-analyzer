using Microsoft.Extensions.Logging;
using TestExecutor.Contract;

namespace TestExecutor;

internal static class CilInstructionInjector
{
    internal static TestCase InjectCil(this TestCase testCase)
    {
        testCase.Logger?.LogInformation($"Beginning CIL instruction injection for {testCase.NameOrHash}.");
        // TODO: un-mock
        return Random.Shared.Next(4) == 0 ? 
            throw new TestExecutionException("Injection failure") : 
            testCase;
    }
}