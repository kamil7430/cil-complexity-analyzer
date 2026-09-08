namespace CilComplexityAnalyzer.Contract;

public abstract class TestCase
{
    public abstract int TestNumber();
    public abstract TestCaseSettings Settings();
    public abstract void Arrange();
    public abstract void Act();
    public abstract void Assert();
}