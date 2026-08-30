namespace CilComplexityAnalyzer.Facade.Attributes.Tests;

public class FakeExecutor
{
    public static bool Run(
        string sourceCodePath, 
        string methodToInvoke, 
        string testName, 
        object[] input,
        object?[] expectedOutput
        )
    {
        Console.WriteLine($"[FakeExecutor] test='{testName}' method='{methodToInvoke}' path='{sourceCodePath}' input=[{string.Join(",", input)}] expected={expectedOutput}");
        return File.Exists(sourceCodePath);
    }
}