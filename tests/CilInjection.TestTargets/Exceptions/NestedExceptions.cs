namespace TestTargets.Exceptions;

using System;

public class NestedExceptions
{
    public int ProcessNested(int a, int b)
    {
        int result = 0;
        try
        {
            try
            {
                result = a / b;
            }
            catch (DivideByZeroException)
            {
                result = -1;
                throw;
            }
        }
        catch (Exception)
        {
            result -= 10;
        }

        return result;
    }
}