using System.Reflection;

namespace CilComplexityAnalyzer.ContainerWorker.Tests;

public class ContainerWorkerUnitTestsBase
{
    protected static void CaptureException(Action action)
    {
        Exception? exception = null;
        
        try
        {
            action();
        }
        catch (Exception e)
        {
            exception = e;
        }
        
        if (exception is not null)
            throw exception;
    }
}