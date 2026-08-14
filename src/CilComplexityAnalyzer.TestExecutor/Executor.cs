using CilComplexityAnalyzer.ContainerWorker;
using CilComplexityAnalyzer.TestExecutor.Contract;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Images;
using Microsoft.Extensions.Logging;

namespace CilComplexityAnalyzer.TestExecutor;

internal static class Executor
{
    private static readonly IImage DockerImage = new DockerImage("TODO");
    
    internal static void Initialize(ILogger? logger)
    {
        // logger?.LogInformation("Initializing DockerImage...");
        // _ = References;
    }
    
    internal static Contract.TestResult Execute(this TestCase testCase)
    {
        testCase.Logger?.LogInformation($"[{testCase.NameOrHash}] Beginning code execution.");

        testCase.Logger?.LogInformation($"[{testCase.NameOrHash}] Building test container.");
        var container = new ContainerBuilder(DockerImage)
            .WithResourceMapping(
                resourceContent: testCase.AssemblyBytes,
                target: FilePath.Of(Consts.StudentSolutionDllPath)
            ).Build();
        
        testCase.Logger?.LogInformation($"[{testCase.NameOrHash}] Starting test container.");
        container.StartAsync().Wait();
        
        testCase.Logger?.LogInformation($"[{testCase.NameOrHash}] Sending test cases and starting test.");
        // TODO: finish
    }
}