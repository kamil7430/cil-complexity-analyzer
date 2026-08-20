using Microsoft.Extensions.Logging;

namespace CilComplexityAnalyzer.TestExecutor.Tests;

public static class Utils
{
    public static ILogger GetLogger<T>()
        => LoggerFactory.Create(b => b.AddConsole()).CreateLogger<T>();
}