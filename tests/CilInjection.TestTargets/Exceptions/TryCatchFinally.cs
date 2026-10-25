namespace TestTargets.Exceptions;

using System;

public class TryCatchFinally
{
    public int SimpleTryCatch(int input)
    {
        try
        {
            return 100 / input;
        }
        catch (DivideByZeroException)
        {
            return -1;
        }
    }

    public int SimpleTryFinally(ref int counter)
    {
        try
        {
            counter++;
            return counter;
        }
        finally
        {
            counter += 10;
        }
    }

    public int FullTryCatchFinally(int input, ref int cleanupTracker)
    {
        int result = 0;
        try
        {
            result = 100 / input;
        }
        catch (Exception)
        {
            result = -1;
        }
        finally
        {
            cleanupTracker++;
        }
        return result;
    }
}