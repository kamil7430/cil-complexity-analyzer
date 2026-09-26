namespace TestTargets.Basic;

public class SimpleCalculator
{
    public int Add(int a, int b)
    {
        return a + b;
    }

    public int Subtract(int a, int b)
    {
        return a - b;
    }

    public int Multiply(int a, int b)
    {
        return a * b;
    }

    public int ComputeExpression(int a, int b, int c)
    {
        return (a + b) * c;
    }
}