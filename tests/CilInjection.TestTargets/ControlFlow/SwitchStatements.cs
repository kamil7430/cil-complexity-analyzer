namespace TestTargets.ControlFlow;

public class SwitchStatements
{
    public string DenseIntSwitch(int option)
    {
        switch (option)
        {
            case 0: return "Zero";
            case 1: return "One";
            case 2: return "Two";
            case 3: return "Three";
            default: return "Unknown";
        }
    }

    public int SwitchExpression(int code) => code switch
    {
        10 => 100,
        20 => 200,
        30 => 300,
        _ => -1
    };
}