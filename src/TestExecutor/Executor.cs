using Microsoft.Extensions.Logging;
using TestExecutor.Contract;
using TestExecutor.Contract.Results;
using TestExecutor.Contract.Settings;

namespace TestExecutor;

internal static class Executor
{
    internal static TestResult Execute(this TestCase testCase)
    {
        testCase.Logger?.LogInformation($"Beginning code execution for {testCase.NameOrHash}.");
        // TODO: un-mock
        return Random.Shared.Next(4) == 0
            ? new Failure("Stack overflow")
            : new Success(
                ComplexityCalculationMethod.CilInstructionCounting,
                1_000_000
            );
    }
}