using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json;
using CilComplexityAnalyzer.ContainerWorker;
using CilComplexityAnalyzer.Contract;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Images;

namespace CilComplexityAnalyzer.TestExecutor;

internal static class Executor
{
    // private const string DockerImageTag = "docker.io/kamil7430/cil-complexity-analyzer-container-worker:main";
    private const string DockerImageTag = "container-worker:latest";
    private static readonly Lazy<IImage> DockerImage = new(() => new DockerImage(DockerImageTag));
    
    private const long AdditionalTimeoutMs = 20_000;
    private static readonly TimeSpan JsonFetchInterval = TimeSpan.FromSeconds(1);

    internal static IEnumerable<TestResult> Execute(this InternalTestSuite internalTestSuite)
        => internalTestSuite.TestSuite.Settings().Containerized switch
        {
            false => ExecuteLocally(internalTestSuite),
            _ => ExecuteInContainer(internalTestSuite),
        };

    private static IEnumerable<TestResult> ExecuteInContainer(InternalTestSuite internalTestSuite)
    {
        Console.WriteLine($"[{internalTestSuite.Name}] Beginning code execution.");

        Console.WriteLine($"[{internalTestSuite.Name}] Building test container.");
        var container = new ContainerBuilder(DockerImage.Value)
            .WithCleanUp(true)
            .WithResourceMapping(
                resourceContent: internalTestSuite.TestSuiteAssemblyBytes,
                target: FilePath.Of(Paths.TestSuiteDllPath)
            ).Build();

        Console.WriteLine($"[{internalTestSuite.Name}] Starting test container.");
        container.StartAsync(internalTestSuite.TestSuite.CancellationToken()).Wait();

        var globalTimeoutMs = internalTestSuite.TestCases.Value.Sum(t => t.Settings().TimeoutMs) + AdditionalTimeoutMs;
        var globalTimeout = DateTime.UtcNow + TimeSpan.FromMilliseconds(globalTimeoutMs);
        Console.WriteLine($"[{internalTestSuite.Name}] Global container timeout " +
            $"set to {globalTimeoutMs} ms ({globalTimeout.ToLongTimeString()}).");

        for (int i = 0; i < internalTestSuite.TestCases.Value.Length; i++)
        {
            var timeoutMs = internalTestSuite.TestCases.Value[i].Settings().TimeoutMs;
            Console.WriteLine(
                $"[{internalTestSuite.Name}] Waiting for test {i} to finish (timeout is {timeoutMs} ms)."
            );

            byte[]? resultBytes = null;
            while (resultBytes is null || resultBytes.Length <= 0)
            {
                if (globalTimeout <= DateTime.UtcNow)
                {
                    Console.WriteLine($"[{internalTestSuite.Name}] Test container timed out!");
                    for (int j = i; j < internalTestSuite.TestCases.Value.Length; j++)
                    {
                        yield return new TestResult(false, null, "Test container timed out!");
                    }
                    yield break;
                }

                try
                {
                    Console.WriteLine($"[{internalTestSuite.Name}] Trying to fetch test {i} results.");
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
                        
                    Thread.Sleep(JsonFetchInterval);
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