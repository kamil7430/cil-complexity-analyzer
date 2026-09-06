using System.Text.Json;
using CilComplexityAnalyzer.ContainerWorker;
using CilComplexityAnalyzer.TestExecutor.Contract;
using CilComplexityAnalyzer.TestExecutor.Contract.Results;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Images;
using Microsoft.Extensions.Logging;

namespace CilComplexityAnalyzer.TestExecutor;

internal static class Executor
{
    private const string DockerImageTag = "docker.io/kamil7430/cil-complexity-analyzer-container-worker:main";
    private static readonly Lazy<IImage> DockerImage = new(() => new DockerImage(DockerImageTag));

    internal static IEnumerable<Contract.TestResult> Execute(this TestSuite testSuite)
        => testSuite.Settings()?.Containerized switch
        {
            false => ExecuteLocally(testSuite),
            _ => ExecuteInContainer(testSuite),
        };

    private static IEnumerable<Contract.TestResult> ExecuteInContainer(TestSuite testSuite)
    {
        testSuite.Logger()?.LogInformation($"[{testSuite.Name}] Beginning code execution.");

        testSuite.Logger()?.LogInformation($"[{testSuite.Name}] Building test container.");
        var container = new ContainerBuilder(DockerImage.Value)
            .WithCleanUp(true)
            .WithResourceMapping(
                resourceContent: testSuite.TestSuiteAssemblyBytes,
                target: FilePath.Of(Paths.TestSuiteDllPath)
            ).Build();

        testSuite.Logger()?.LogInformation($"[{testSuite.Name}] Starting test container.");
        container.StartAsync(testSuite.CancellationToken()).Wait();

        var globalTimeoutMs = testSuite.TestCases.Value.Sum(t => t.Settings().TimeoutMs);
        var globalTimeout = DateTime.UtcNow + TimeSpan.FromMilliseconds(globalTimeoutMs) + TimeSpan.FromSeconds(20);
        testSuite.Logger()?.LogInformation($"[{testSuite.Name}] Global container timeout set to " +
            $"{globalTimeoutMs} ms ({globalTimeout.ToLongTimeString()}).");

        for (int i = 0; i < testSuite.TestCases.Value.Length; i++)
        {
            var timeoutMs = testSuite.TestCases.Value[i].Settings().TimeoutMs;
            testSuite.Logger()?.LogInformation($"[{testSuite.Name}] Waiting for test {i + 1} to finish " +
                $"(timeout is {timeoutMs} ms).");

            byte[]? resultBytes = null;
            while (resultBytes is null)
            {
                if (globalTimeout <= DateTime.UtcNow)
                {
                    testSuite.Logger()?.LogInformation($"[{testSuite.Name}] Test container timed out!");
                    for (int j = i; j < testSuite.TestCases.Value.Length; j++)
                        yield return new Failure("Test container timed out!");
                    yield break;
                }

                Thread.Sleep(TimeSpan.FromSeconds(1));
                try
                {
                    resultBytes = container.ReadFileAsync(Paths.ResultsJsonPath(i), testSuite.CancellationToken())
                        .Result;
                }
                catch (AggregateException e)
                {
                    if (e.InnerException is not FileNotFoundException)
                        throw e.InnerException ?? e;
                }
            }

            var result = JsonSerializer.Deserialize<ContainerWorker.TestResult>(resultBytes)!;

            yield return result.Success switch
            {
                true => new Success(result.MeasuredComplexity!.Value),
                _ => new Failure(result.Message),
            };
        }
    }

    private static IEnumerable<Contract.TestResult> ExecuteLocally(TestSuite testSuite)
    {
        
    }
}