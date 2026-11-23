using System.Reflection;

namespace CilComplexityAnalyzer.ContainerWorker.UnitTests;

public static class Utils
{
    public static Assembly LoadAssembly(string assemblyName)
    {
        using var stream = Assembly.GetExecutingAssembly()
            .GetManifestResourceStream($"CilComplexityAnalyzer.ContainerWorker.Tests.Assemblies.{assemblyName}.dll");
        using var memoryStream = new MemoryStream();
        stream!.CopyTo(memoryStream);
        return Assembly.Load(memoryStream.ToArray());
    }
    
    public static void InvokePerformSingleTestAndCaptureException(object testCase, Action<Contract.TestResult> action)
    {
        Exception? exception = null;

        Program.PerformSingleTest(testCase, testResult =>
        {
            try
            {
                action(testResult);
            }
            catch (Exception e)
            {
                // first exception should be captured
                exception ??= e;
            }
        });

        if (exception is not null)
            throw exception;
    }
}