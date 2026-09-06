namespace CilComplexityAnalyzer.ContainerWorker;

public static class Paths
{
    public const string TestSuiteDllPath = "/app/test-suite.dll";
    public static string ResultsJsonPath(int number) => $"/app/results/{number}.json";
}