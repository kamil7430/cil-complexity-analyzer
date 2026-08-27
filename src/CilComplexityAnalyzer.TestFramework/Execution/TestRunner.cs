using CilComplexityAnalyzer.TestFramework.Attributes;

namespace CilComplexityAnalyzer.TestFramework.Execution;

public class TestRunner
{
    public static void RunTestCase<T>() where T : new()
    {
        var testInstance = new T();
        var type = typeof(T);

        var arrangeMethod = type.GetMethods()
            .FirstOrDefault(m => m.GetCustomAttributes(typeof(ArrangeAttribute), false).Any());
        arrangeMethod?.Invoke(testInstance, null);
        
        var actMethod = type.GetMethods()
            .FirstOrDefault(m=> m.GetCustomAttributes(typeof(ActAttribute), false).Any());
        if (actMethod != null)
        {
            var actAttribute = (ActAttribute)actMethod.GetCustomAttributes(typeof(ActAttribute), false).FirstOrDefault();
            ExecuteWithLimits(() => actMethod.Invoke(testInstance, null), actAttribute);
        }
        
        var assertMethod = type.GetMethods()
            .FirstOrDefault(m => m.GetCustomAttributes(typeof(AssertAttribute), false).Any());
        assertMethod?.Invoke(testInstance, null);
    }

    private static void ExecuteWithLimits(Action action, ActAttribute actAttribute)
    {
        action();
    }
}