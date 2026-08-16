using System.Reflection;
using System.Text.Json;

namespace CilComplexityAnalyzer.ContainerWorker;

internal class Program
{
    internal static void Main(string[] _)
    {
        // discard any writes (keep the real stdout for result writing)
        var realStdout = Console.Out;
        Console.SetOut(TextWriter.Null);

        try
        {
            // read all test datas from file
            var json = File.ReadAllBytes(Consts.TestDataJsonFilename);
            var dataArray = JsonSerializer.Deserialize<TestData[]>(json) ??
                throw new ArgumentException("Json is 'null'.");

            var assembly = Assembly.LoadFrom(Consts.StudentSolutionDllFilename);

            var types = assembly.GetTypes().Where(t => t.GetMember(dataArray[0].MethodToInvoke).Length == 1).ToArray();
            if (types.Length != 1)
            {
                WriteFailureAndExit($"There are {types.Length} types containing {dataArray[0].MethodToInvoke}" +
                    $" method. Expected exactly one.");
            }
            var type = types[0];
            
            // execute tests sequentially and report every output immediately
            foreach (var data in dataArray) {
                var obj = Activator.CreateInstance(type);

                try
                {
                    var returnedObj = type.InvokeMember(
                        name: data.MethodToInvoke,
                        invokeAttr: BindingFlags.InvokeMethod,
                        binder: null,
                        target: obj,
                        args: data.Input
                    );

                    // TODO: assert time elapsed
                    var complexity = -1L;

                    if (returnedObj != null && !returnedObj.Equals(data.Output))
                    {
                        WriteResult(new TestResult(false, complexity,
                            $"Outputs don't match!\nExpected: {data.Output}\nActual: {returnedObj}"));
                    }
                    else
                    {
                        WriteResult(new TestResult(true, complexity, null));
                    }
                }
                catch (Exception e)
                {
                    WriteResult(new TestResult(false, null, $"Unhandled exception in" +
                        $"student code: {e.Message}"));
                }
            }
        }
        catch (Exception e)
        {
            WriteFailureAndExit($"Internal worker exception: {e.Message}");
        }

        return;
        
        void WriteResult(TestResult result)
        {
            realStdout.WriteLine(JsonSerializer.Serialize(result));
            realStdout.Flush();
        }
        
        void WriteFailureAndExit(string message)
        {
            WriteResult(new TestResult(false, null, message));
            Environment.Exit(1);
        }
    }
}