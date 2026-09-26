namespace TestTargets.Exceptions;

using System;
using System.IO;

public class MultipleCatches
{
    public int ExecuteWithMultipleHandlers(int code)
    {
        try
        {
            return code switch
            {
                1 => throw new ArgumentNullException(nameof(code)),
                2 => throw new InvalidOperationException("Invalid operation"),
                3 => throw new IOException("IO error"),
                _ => code * 2
            };
        }
        catch (ArgumentNullException)
        {
            return 10;
        }
        catch (InvalidOperationException)
        {
            return 20;
        }
        catch (IOException)
        {
            return 30;
        }
        catch (Exception)
        {
            return 99;
        }
    }
}