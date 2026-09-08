using System.Text;
using CilComplexityAnalyzer.ContainerWorker;

namespace CilComplexityAnalyzer.Contract;

public abstract class TestCase
{
    private readonly StringBuilder _assertions = new();
    
    public abstract int TestNumber();
    
    public abstract TestCaseSettings Settings();
    
    public abstract void Arrange();
    
    public abstract void Act();
    
    public virtual void Assert()
    {
        var assertions = _assertions.ToString();
        if (!string.IsNullOrWhiteSpace(assertions))
            throw new AssertFailedException(assertions);
    }

    public void IsTrue(bool condition, string message = "")
    {
        if (condition)
            return;

        _assertions.AppendLine($"IsTrue assertion failed. {message}");
    }
}