namespace TestTargets.Exceptions;

using System;

public class ExceptionFilters
{
    public int ExecuteWithFilter(int errorCode)
    {
        try
        {
            if (errorCode < 0)
                throw new InvalidOperationException("Negative value error");

            return errorCode;
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Negative"))
        {
            return -100;
        }
        catch (InvalidOperationException)
        {
            return -50;
        }
    }
}