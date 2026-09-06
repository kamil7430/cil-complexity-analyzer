using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.Json;

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

            // load test suite assembly
            var assembly = Assembly.LoadFrom(Paths.TestSuiteDllPath);
            Debug("Loaded assembly");

            Execute(assembly, WriteResult);
        }
        catch (Exception e)
        {
            WriteFailureAndExit($"Internal worker exception: {e.Message}");
        }
    }

    internal static void Execute(Assembly assembly, Action<TestResult> writeResult)
    {
        var testCases = FindAllTestCases(assembly);
        Debug($"Found {testCases.Length} TestCase types.");

        var testCasesSorted = ActivateAndSortTestCases(testCases);
        Debug("Activated and sorted test cases.");
        
        // execute tests sequentially and report every output immediately
        foreach (var testCase in testCasesSorted)
        {
            try
            {
                PerformSingleTest(testCase, writeResult);
            }
            catch (Exception e)
            {
                writeResult(new TestResult(false, null, $"Unhandled exception: {e.Message}"));
            }
        }
    }

    private static Type[] FindAllTestCases(Assembly assembly)
    {
        const string contract = "CilComplexityAnalyzer.TestExecutor.Contract";
        var types = assembly.GetTypes();
        
        var testSuiteBase = types.FirstOrDefault(t => t.FullName == $"{contract}.TestSuite");
        if (testSuiteBase == null)
        {
            WriteFailureAndExit("TestSuite base not found.");
        }
        
        var testCaseBase = types.FirstOrDefault(t => t.Name == $"{contract}.TestCase");
        if (testCaseBase == null)
        {
            WriteFailureAndExit("TestCase base not found.");
        }
        
        var testSuite = types.FirstOrDefault(t => t.IsAssignableTo(testSuiteBase));
        if (testSuite == null)
        {
            WriteFailureAndExit("Concrete TestSuite not found.");
        }

        var testCases = testSuite!.GetMembers()
            .Select(m => m.ReflectedType)
            .Where(t => t?.IsAssignableTo(testCaseBase) ?? false)
            .ToArray();
        if (testCases.Length == 0)
        {
            WriteFailureAndExit("No TestCases found.");
        }
        
        return testCases!;
    }

    private static object[] ActivateAndSortTestCases(Type[] testCases)
    {
        var objects = testCases.Select(c => Activator.CreateInstance(c)!).ToArray();
        objects.Sort((x, y) =>
        {
            var a = (int)x.GetType().GetMethod("TestNumber")!.Invoke(x, null)!;
            var b = (int)y.GetType().GetMethod("TestNumber")!.Invoke(y, null)!;
            return a.CompareTo(b);
        });
        return objects;
    }

    private static void PerformSingleTest(object testCase, Action<TestResult> writeResult)
    {
        var arrangeMethod = testCase.GetType().GetMethod("Arrange");
        var actMethod = testCase.GetType().GetMethod("Act");
        var assertMethod = testCase.GetType().GetMethod("Assert");

        arrangeMethod!.Invoke(testCase, null);
        Debug("Invoked Arrange");
        
        actMethod!.Invoke(testCase, null);
        Debug("Invoked Act");
        
        var complexity = -1L;
        // TODO: assert time elapsed
        // if (complexity > ???)
        // {
        //     writeResult(new TestResult(false, complexity, $"Too high complexity!\nExpected: {}\nActual: {complexity}"));
        // }

        try
        {
            assertMethod!.Invoke(testCase, null);
            Debug("Invoked Assert");
        }
        catch (AssertFailedException e)
        {
            writeResult(new TestResult(false, complexity, $"Assertion failed:\n{e.Message}"));
        }
        
        writeResult(new TestResult(true, complexity, null));
    }
    
    private static void WriteResult(TestResult result)
    {
        using var file = File.Open(Paths.ResultsJsonPath(_testNo), FileMode.CreateNew);
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