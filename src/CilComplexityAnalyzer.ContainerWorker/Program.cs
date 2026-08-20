using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;

[assembly: InternalsVisibleTo("CilComplexityAnalyzer.ContainerWorker.Tests")]
namespace CilComplexityAnalyzer.ContainerWorker;

internal class Program
{
    private static TextWriter? _originalStdout;
    private static int _testNo = 0;
    
    internal static void Main(string[] _)
    {
        try
        {
            // discard any writes
            _originalStdout = Console.Out;
            Console.SetOut(TextWriter.Null);

            // read all test datas from file
            var json = File.ReadAllBytes(Consts.TestDataJsonPath);
            Debug($"Received json: {Encoding.UTF8.GetString(json)}");
            
            var dataArray = JsonSerializer.Deserialize<TestData[]>(json) ??
                throw new ArgumentException("Json is 'null'.");
            Debug("Deserialized");

            // load student assembly
            var assembly = Assembly.LoadFrom(Consts.StudentSolutionDllPath);
            Debug("Loaded assembly");

            Execute(dataArray, assembly, WriteResult);
        }
        catch (Exception e)
        {
            WriteFailureAndExit($"Internal worker exception: {e.Message}");
        }
    }

    internal static void Execute(TestData[] dataArray, Assembly assembly, Action<TestResult> writeResult)
    {
        // find the method to invoke
        var types = assembly.GetTypes().Where(t => t.GetMember(dataArray[0].MethodToInvoke).Length == 1).ToArray();
        if (types.Length != 1)
        {
            WriteFailureAndExit($"There are {types.Length} types containing {dataArray[0].MethodToInvoke} " +
                $"method. Expected exactly one.");
        }

        var type = types[0];
        Debug($"Found type: {type}");
        var method = type.GetMethod(dataArray[0].MethodToInvoke)!;
        Debug($"Found method: {method}");

        // execute tests sequentially and report every output immediately
        foreach (var data in dataArray)
        {
            var obj = Activator.CreateInstance(type);
            Debug("Created instance");

            try
            {
                var input = data.Input;
                var parameters = method.GetParameters();
                for (int i = 0; i < parameters.Length; i++)
                {
                    CorrectType(ref input![i], parameters[i].ParameterType);
                }

                var returnedObj = method.Invoke(obj, input);
                Debug("Invoked");

                // TODO: assert time elapsed
                var complexity = -1L;

                var output = data.Output;
                CorrectType(ref output, method.ReturnType);
                if (returnedObj != null && !returnedObj.Equals(output))
                {
                    writeResult(new TestResult(false, complexity,
                        $"Outputs don't match!\nExpected: {output}\nActual: {returnedObj}"));
                }
                else
                {
                    writeResult(new TestResult(true, complexity, null));
                }
            }
            catch (Exception e)
            {
                writeResult(new TestResult(false, null, $"Unhandled exception: {e.Message}"));
            }
        }
    }

    internal static void CorrectType(ref object? input, Type targetType)
    {
        var inputType = input?.GetType();
        Debug($"Converting from {inputType} to {targetType}.");
        if (inputType == targetType) return;
        input = input switch
        {
            JsonElement j => JsonSerializer.Deserialize(j.GetRawText(), targetType),
            _ => throw new ArgumentException($"Invalid cast from {inputType} to {targetType}!"),
        };
    }
    
    private static void WriteResult(TestResult result)
    {
        using var file = File.Open(Consts.ResultsJsonPath(_testNo), FileMode.CreateNew);
        var json = JsonSerializer.SerializeToUtf8Bytes(result);
        file.Write(json);
        _testNo++;
        Debug($"Wrote json: {Encoding.UTF8.GetString(json)}");
    }
        
    private static void WriteFailureAndExit(string message)
    {
        WriteResult(new TestResult(false, null, message));
        Environment.Exit(1);
    }

    [Conditional("DEBUG")]
    private static void Debug(string message)
    {
        _originalStdout?.WriteLine(message);
    }
}