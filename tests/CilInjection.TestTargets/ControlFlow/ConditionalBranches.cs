namespace TestTargets.ControlFlow;

public class ConditionalBranches
{
    public int SimpleIfElse(bool condition)
    {
        if (condition)
        {
            return 100;
        }
        else
        {
            return 200;
        }
    }

    public int IfElseChain(int value)
    {
        if (value < 0)
        {
            return -1;
        }
        if (value == 0)
        {
            return 0;
        }
        else if (value < 10)
        {
            return 1;
        }
        
        return 2;
    }

    public bool ShortCircuitAndOr(int a, int b)
    {
        return (a > 0 && b > 0) || (a < -10 && b < -10);
    }
}