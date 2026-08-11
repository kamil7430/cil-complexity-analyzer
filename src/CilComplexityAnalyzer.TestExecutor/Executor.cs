using System.Reflection;
using CilComplexityAnalyzer.TestExecutor.Contract;
using CilComplexityAnalyzer.TestExecutor.Contract.Results;
using CilComplexityAnalyzer.TestExecutor.Contract.Settings;
using Microsoft.Extensions.Logging;

namespace CilComplexityAnalyzer.TestExecutor;

internal static class Executor
{
    internal static void Initialize(ILogger? logger)
    {
        // logger?.LogInformation("Initializing References...");
        // _ = References;
    }
    
    internal static TestResult Execute(this TestCase testCase)
    {
        testCase.Logger?.LogInformation($"[{testCase.NameOrHash}] Beginning code execution.");

        var assembly = testCase.Assembly ?? 
            throw new NullReferenceException("Assembly is null! Did you run compiler before executor?");

        var types = assembly.GetTypes().Where(t => t.GetMember(testCase.MethodToInvoke).Length == 1).ToArray();
        if (types.Length != 1)
            throw new TestExecutionException($"There are {types.Length} types containing {testCase.MethodToInvoke}" +
                $" method. Expected exactly one.");

        var type = types[0];
        var obj = Activator.CreateInstance(type);

        var returnedObj = type.InvokeMember(
            name: testCase.MethodToInvoke, 
            invokeAttr: BindingFlags.InvokeMethod, 
            binder: null, 
            target: obj, 
            args: testCase.Input
        );

        if (returnedObj != null && !returnedObj.Equals(testCase.Output))
            return new Failure($"Outputs don't match!\nExpected: {testCase.Output}\nActual: {returnedObj}");
        
        // TODO: assert time elapsed
        return new Success(ComplexityCalculationMethod.CilInstructionCounting, -1);
    }
}