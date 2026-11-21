using CilComplexityAnalyzer.ContainerWorkerUtils;

namespace CilComplexityAnalyzer.Contract;

public abstract class TestCase
{
    public virtual int TestNumber()
        => throw new NotImplementedException();

    public virtual TestCaseSettings Settings()
        => throw new NotImplementedException();
    
    public abstract void Arrange();
    
    public abstract void Act();

    public abstract void Assert();

    public void IsTrue(bool condition, string message)
    {
        if (!condition)
            throw new AssertFailedException(message);
    }
}