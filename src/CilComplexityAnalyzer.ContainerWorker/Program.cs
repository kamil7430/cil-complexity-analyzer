using System.Text.Json;

namespace CilComplexityAnalyzer.ContainerWorker;

internal class Program
{
    internal static void Main(string[] args)
    {
        // discard any writes
        var realStdout = Console.Out;
        Console.SetOut(TextWriter.Null);

        TestResult result;
        try
        {
            var json = Console.ReadLine();
            var data = JsonSerializer.Deserialize<TestData>(json);

            // TODO: execute
            
            result = null;
        }
        catch (Exception e)
        {
            result = new TestResult(false, null, $"Internal worker exception: {e.Message}");
        }
        
        realStdout.WriteLine(JsonSerializer.Serialize(result));
    }
}