using System.Text.Json;
using CilComplexityAnalyzer.ContainerWorker;
using CilComplexityAnalyzer.TestExecutor.Contract;
using CilComplexityAnalyzer.TestExecutor.Contract.Results;
using CilComplexityAnalyzer.TestExecutor.Contract.Settings;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Images;
using Microsoft.Extensions.Logging;

namespace CilComplexityAnalyzer.TestExecutor;

internal static class Executor
{
    private const string DockerImageTag = "docker.io/kamil7430/cil-complexity-analyzer-container-worker:latest";
    private static readonly IImage DockerImage = new DockerImage(DockerImageTag);
    
    internal static void Initialize(ILogger? logger)
    {
        logger?.LogInformation("Initializing DockerImage...");
        _ = DockerImage;
    }
    
    internal static Contract.TestResult[] Execute(this TestSuite testSuite)
    {
        testSuite.Logger?.LogInformation($"[{testSuite.NameOrHash}] Beginning code execution.");
        
        testSuite.Logger?.LogInformation($"[{testSuite.NameOrHash}] Serializing test suite.");
        var testDatas = testSuite.TestCases
            .Select(t => new TestData(testSuite.MethodToInvoke, t.Input, t.Output))
            .ToArray();
        var testDatasBytes = JsonSerializer.SerializeToUtf8Bytes(testDatas);

        testSuite.Logger?.LogInformation($"[{testSuite.NameOrHash}] Building test container.");
        var container = new ContainerBuilder(DockerImage)
            .WithCleanUp(true)
            .WithResourceMapping(
                resourceContent: testSuite.AssemblyBytes,
                target: FilePath.Of(Consts.StudentSolutionDllPath)
            ).WithResourceMapping(
                resourceContent: testDatasBytes,
                target: FilePath.Of(Consts.TestDataJsonPath)
            ).Build();
        
        testSuite.Logger?.LogInformation($"[{testSuite.NameOrHash}] Starting test container.");
        container.StartAsync(testSuite.CancellationToken).Wait();

        var globalTimeoutMs = testSuite.TestCases.Sum(t => t.TimeoutMs);
        var globalTimeout = DateTime.UtcNow + TimeSpan.FromMilliseconds(globalTimeoutMs) + TimeSpan.FromSeconds(20);
        testSuite.Logger?.LogInformation($"[{testSuite.NameOrHash}] Global container timeout set to " +
            $"{globalTimeoutMs} ms ({globalTimeout.ToLongTimeString()}).");
        
        List<Contract.TestResult> results = [];
        for (int i = 0; i < testDatas.Length; i++)
        {
            var timeoutMs = testSuite.TestCases[i].TimeoutMs;
            testSuite.Logger?.LogInformation($"[{testSuite.NameOrHash}] Waiting for test {i + 1} to finish " +
                $"(timeout is {timeoutMs} ms).");

            byte[]? resultBytes = null;
            while (resultBytes is null)
            {
                if (globalTimeout <= DateTime.UtcNow)
                {
                    testSuite.Logger?.LogInformation($"[{testSuite.NameOrHash}] Test container timed out!");
                    results.Add(new Failure("Test container timed out!"));
                    return results.ToArray();
                }
                
                Thread.Sleep(TimeSpan.FromSeconds(1));
                resultBytes = container.ReadFileAsync(Consts.ResultsJsonPath(i), testSuite.CancellationToken).Result;
            }
            var result = JsonSerializer.Deserialize<ContainerWorker.TestResult>(resultBytes)!;

            if (result.Success)
            {
                results.Add(new Success(
                    ComplexityCalculationMethod.CilInstructionCounting, 
                    result.MeasuredComplexity!.Value
                ));
            }
            else
            {
                results.Add(new Failure(result.Message));
            }
        }

        return results.ToArray();
    }
}