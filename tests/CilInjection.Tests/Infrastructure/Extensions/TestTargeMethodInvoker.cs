using System.Runtime.Loader;
using CilInjecting.Tests.Infrastructure.Extensions;
using Mono.Cecil;

public static class TestTargetExtensions
{
    public static object ExecuteMethod(this ModuleDefinition module, AssemblyLoadContext alc, string typeName, string methodName, params object[] args)
    {
        var assembly = module.ToAssemblyLoadContext(alc);
        var type = assembly.GetType(typeName) ?? throw new Exception($"Typ {typeName} nie istnieje");
        var method = type.GetMethod(methodName) ?? throw new Exception($"Metoda {methodName} nie istnieje");
        
        var instance = Activator.CreateInstance(type);
        return method.Invoke(instance, args);
    }
}