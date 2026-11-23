namespace CilComplexityAnalyzer.Contract;

public abstract class TestCase
{
    public abstract int TestNumber();

    public abstract TestCaseSettings Settings();
    
    public abstract void Arrange();
    
    public abstract void Act();

    public abstract void Assert();

    public static void IsTrue(bool condition, string message)
    {
        if (!condition)
            throw new AssertFailedException(message);
    }
}