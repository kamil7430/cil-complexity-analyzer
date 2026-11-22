namespace CilComplexityAnalyzer.Contract;

public abstract class TestSuite
{
    // Public TestSuite contract methods
    public virtual string StudentSolutionSourceCode()
        => throw new NotImplementedException();
    public virtual string TestSuiteSourceCode()
        => throw new NotImplementedException();
    public abstract TestSuiteSettings Settings();
}