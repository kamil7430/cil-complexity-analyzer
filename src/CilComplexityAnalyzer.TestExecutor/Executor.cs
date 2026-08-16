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
        logger?.LogInformation("Initializing DockerImage...");
        _ = DockerImage;
    }
    
    internal static Contract.TestResult Execute(this TestSuite testSuite)
    {
        testSuite.Logger?.LogInformation($"[{testSuite.NameOrHash}] Beginning code execution.");

        testSuite.Logger?.LogInformation($"[{testSuite.NameOrHash}] Building test container.");
        var container = new ContainerBuilder(DockerImage)
            .WithResourceMapping(
                resourceContent: testSuite.AssemblyBytes,
                target: FilePath.Of(Consts.StudentSolutionDllPath)
            ).Build();
        
        testSuite.Logger?.LogInformation($"[{testSuite.NameOrHash}] Starting test container.");
        container.StartAsync().Wait();
        
        testSuite.Logger?.LogInformation($"[{testSuite.NameOrHash}] Sending test cases and starting test.");
        // TODO: finish
    }
}