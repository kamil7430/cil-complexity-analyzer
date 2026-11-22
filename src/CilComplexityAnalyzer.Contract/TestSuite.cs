using Microsoft.Extensions.Logging;

namespace CilComplexityAnalyzer.Contract;

public abstract class TestSuite
{
    // Public TestSuite contract methods
    public virtual string StudentSolutionSourceCode()
        => throw new NotImplementedException();
    public virtual string TestSuiteSourceCode()
        => throw new NotImplementedException();
    public abstract TestSuiteSettings Settings();
    public virtual ILogger? Logger() 
        => null;
    public virtual CancellationToken CancellationToken() 
        => System.Threading.CancellationToken.None;
}