namespace CilComplexityAnalyzer.ContainerWorker;

public static class Consts
{
    public const string StudentSolutionDllPath = "/app/student-solution.dll";
    public const string TestDataJsonPath = "/app/test-data.json";
    public static string ResultsJsonPath(int number) => $"/app/results/{number}.json";
}