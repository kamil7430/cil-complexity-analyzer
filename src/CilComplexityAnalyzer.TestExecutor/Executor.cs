using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json;
using CilComplexityAnalyzer.ContainerWorker;
using CilComplexityAnalyzer.Contract;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Images;
using Microsoft.Extensions.Logging;

namespace CilComplexityAnalyzer.TestExecutor;

internal static class Executor
{
    // private const string DockerImageTag = "docker.io/kamil7430/cil-complexity-analyzer-container-worker:main";
    private const string DockerImageTag = "container-worker:latest";
    private static readonly Lazy<IImage> DockerImage = new(() => new DockerImage(DockerImageTag));
    
    private static readonly TimeSpan AdditionalTimeout = TimeSpan.FromSeconds(20);
    private static readonly TimeSpan JsonFetchInterval = TimeSpan.FromSeconds(1);

    internal static IEnumerable<TestResult> Execute(this InternalTestSuite internalTestSuite)
        => internalTestSuite.TestSuite.Settings().Containerized switch
        {
            false => ExecuteLocally(internalTestSuite),
            _ => ExecuteInContainer(internalTestSuite),
        };

    private static IEnumerable<TestResult> ExecuteInContainer(InternalTestSuite internalTestSuite)
    {
        internalTestSuite.TestSuite.Logger()?.LogInformation($"[{internalTestSuite.Name}] Beginning code execution.");

        internalTestSuite.TestSuite.Logger()?.LogInformation($"[{internalTestSuite.Name}] Building test container.");
        var container = new ContainerBuilder(DockerImage.Value)
            .WithCleanUp(true)
            .WithResourceMapping(
                resourceContent: internalTestSuite.TestSuiteAssemblyBytes,
                target: FilePath.Of(Paths.TestSuiteDllPath)
            ).Build();

        internalTestSuite.TestSuite.Logger()?.LogInformation($"[{internalTestSuite.Name}] Starting test container.");
        container.StartAsync(internalTestSuite.TestSuite.CancellationToken()).Wait();

        var globalTimeoutMs = internalTestSuite.TestCases.Value.Sum(t => t.Settings().TimeoutMs);
        var globalTimeout = DateTime.UtcNow + TimeSpan.FromMilliseconds(globalTimeoutMs) + AdditionalTimeout;
        internalTestSuite.TestSuite.Logger()?.LogInformation($"[{internalTestSuite.Name}] Global container timeout " +
            $"set to {globalTimeoutMs} ms ({globalTimeout.ToLongTimeString()}).");

        for (int i = 0; i < internalTestSuite.TestCases.Value.Length; i++)
        {
            var timeoutMs = internalTestSuite.TestCases.Value[i].Settings().TimeoutMs;
            internalTestSuite.TestSuite.Logger()?.LogInformation(
                $"[{internalTestSuite.Name}] Waiting for test {i + 1} to finish (timeout is {timeoutMs} ms)."
            );

            byte[]? resultBytes = null;
            while (resultBytes is null || resultBytes.Length <= 0)
            {
                if (globalTimeout <= DateTime.UtcNow)
                {
                    internalTestSuite.TestSuite.Logger()
                        ?.LogInformation($"[{internalTestSuite.Name}] Test container timed out!");
                    for (int j = i; j < internalTestSuite.TestCases.Value.Length; j++)
                    {
                        yield return new TestResult(false, null, "Test container timed out!");
                    }
                    yield break;
                }

                try
                {
                    resultBytes = container.ReadFileAsync(Paths.ResultsJsonPath(i),
                        internalTestSuite.TestSuite.CancellationToken()).Result;

                    if (resultBytes.Length <= 0)
                    {
                        Thread.Sleep(JsonFetchInterval);
                    }
                }
                catch (AggregateException e)
                {
                    if (e.InnerException is not FileNotFoundException)
                    {
                        throw e.InnerException ?? e;
                    }
                }
            }

            yield return JsonSerializer.Deserialize<TestResult>(resultBytes)!;
        }
    }

    private static IEnumerable<TestResult> ExecuteLocally(InternalTestSuite internalTestSuite)
    {
        var assembly = Assembly.Load(internalTestSuite.TestSuiteAssemblyBytes!);
        var results = new BlockingCollection<TestResult>(internalTestSuite.TestCases.Value.Length);

        Task.Run(() =>
        {
            ContainerWorker.Program.Execute(assembly, results.Add);
            results.CompleteAdding();
        });

        foreach (var result in results.GetConsumingEnumerable(internalTestSuite.TestSuite.CancellationToken()))
        {
            yield return result;
        }
    }
}