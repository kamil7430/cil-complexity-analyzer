using System.Reflection;
using System.Text;
using System.Text.Json;

namespace CilComplexityAnalyzer.ContainerWorker;

internal class Program
{
    private const bool Debug = false;
    
    internal static void Main(string[] _)
    {
        var testNo = 0;
        
        try
        {
            // discard any writes
            if (!Debug) Console.SetOut(TextWriter.Null);

            // read all test datas from file
            var json = File.ReadAllBytes(Consts.TestDataJsonPath);
            if (Debug) Console.WriteLine(Encoding.UTF8.GetString(json));
            var dataArray = JsonSerializer.Deserialize<TestData[]>(json) ??
                throw new ArgumentException("Json is 'null'.");
            if (Debug) Console.WriteLine("Deserialized");

            // load student assembly
            var assembly = Assembly.LoadFrom(Consts.StudentSolutionDllPath);
            if (Debug) Console.WriteLine("Loaded assembly");

            // find the method to invoke
            var types = assembly.GetTypes().Where(t => t.GetMember(dataArray[0].MethodToInvoke).Length == 1).ToArray();
            if (types.Length != 1)
            {
                WriteFailureAndExit($"There are {types.Length} types containing {dataArray[0].MethodToInvoke} " +
                    $"method. Expected exactly one.");
            }
            var type = types[0];
            if (Debug) Console.WriteLine($"Found type: {type}");
            var method = type.GetMethod(dataArray[0].MethodToInvoke)!;
            if (Debug) Console.WriteLine($"Found method: {method}");
            
            // execute tests sequentially and report every output immediately
            foreach (var data in dataArray) {
                var obj = Activator.CreateInstance(type);
                if (Debug) Console.WriteLine("Created instance");

                try
                {
                    var input = data.Input;
                    var parameters = method.GetParameters();
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        CorrectType(ref input![i], parameters[i].ParameterType);
                    }

                    var returnedObj = method.Invoke(obj, input);
                    if (Debug) Console.WriteLine("Invoked");

                    // TODO: assert time elapsed
                    var complexity = -1L;

                    var output = data.Output;
                    CorrectType(ref output, method.ReturnType);
                    if (returnedObj != null && !returnedObj.Equals(output))
                    {
                        WriteResult(new TestResult(false, complexity,
                            $"Outputs don't match!\nExpected: {output}\nActual: {returnedObj}"));
                    }
                    else
                    {
                        WriteResult(new TestResult(true, complexity, null));
                    }
                }
                catch (Exception e)
                {
                    WriteResult(new TestResult(false, null, $"Unhandled exception in " +
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
            using var file = File.Open(Consts.ResultsJsonPath(testNo), FileMode.CreateNew);
            var json = JsonSerializer.SerializeToUtf8Bytes(result);
            file.Write(json);
            testNo++;
            if (Debug) Console.WriteLine(Encoding.UTF8.GetString(json));
        }
        
        void WriteFailureAndExit(string message)
        {
            WriteResult(new TestResult(false, null, message));
            Environment.Exit(1);
        }
    }

    private static void CorrectType(ref object? input, Type targetType)
    {
        if (Debug) Console.WriteLine($"Converting from {input?.GetType()} to {targetType}.");
        input = input switch
        {
            JsonElement j => JsonSerializer.Deserialize(j.GetRawText(), targetType),
            _ => Convert.ChangeType(input, targetType),
        };
    }
}