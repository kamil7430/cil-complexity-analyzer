namespace CilComplexityAnalyzer.ContainerWorker;

public static class Consts
{
    public const string StudentSolutionDllFilename = "student-solution.dll";
    public const string TestDataJsonFilename = "test-data.json";
    public static string ResultsJsonFilename(int number) => $"results/{number}.json";
}