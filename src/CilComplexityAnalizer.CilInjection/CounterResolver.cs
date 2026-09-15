namespace CilComplexityAnalizer.CilInjection;

using CilComplexityAnalyzer.RunTime;
using System.Reflection;
using System.Runtime.Loader;

public static class CounterResolver
{
    private static readonly string RuntimeAssemblyName = typeof(GlobalCounterContainer).Assembly.GetName().Name!;
    private static readonly string RuntimeTypeName = typeof(GlobalCounterContainer).FullName!;
    
    public static long GetCounter(AssemblyLoadContext context)
    {      
        var (getType, getMethod) = ResolveRuntimeMethod(context, nameof(GlobalCounterContainer.GetCounter));
        return (long)getMethod.Invoke(null, null)!;
    }

    public static void ResetCounter(AssemblyLoadContext context)
    {
        var (getType, resetMethod) = ResolveRuntimeMethod(context, nameof(GlobalCounterContainer.ResetCounter));
        resetMethod.Invoke(null, null);
    }

    private static (Type Type, MethodInfo Method) ResolveRuntimeMethod(AssemblyLoadContext context, string methodName)
    {
        var runtimeAssembly = context.Assemblies
                                  .FirstOrDefault(a => a.GetName().Name == RuntimeAssemblyName)
                              ?? throw new InvalidOperationException($"Biblioteka '{RuntimeAssemblyName}' nie została załadowana do kontekstu.");

        var containerType = runtimeAssembly.GetType(RuntimeTypeName)
                            ?? throw new InvalidOperationException($"Nie odnaleziono typu '{RuntimeTypeName}'.");

        var method = containerType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static)
                     ?? throw new InvalidOperationException($"Nie odnaleziono metody '{methodName}'.");

        return (containerType, method);
    }
}