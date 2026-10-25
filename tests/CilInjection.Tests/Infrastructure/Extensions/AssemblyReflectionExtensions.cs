namespace CilInjecting.Tests.Infrastructure.Extensions;

using System;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

public static class AssemblyReflectionExtensions
{
    /// <summary>
    /// Tworzy instancję podanego typu i wywołuje na niej metodę instancyjną.
    /// </summary>
    public static TResult InvokeMethod<TResult>(
        this Assembly assembly, 
        string fullTypeName, 
        string methodName, 
        params object?[] parameters)
    {
        var type = assembly.GetType(fullTypeName);
        Assert.IsNotNull(type, $"Nie odnaleziono typu '{fullTypeName}' w załadowanym zestawieniu.");

        var instance = Activator.CreateInstance(type)
                       ?? throw new InvalidOperationException($"Nie udało się utworzyć instancji typu '{fullTypeName}'.");

        var method = type.GetMethod(methodName);
        Assert.IsNotNull(method, $"Nie odnaleziono metody '{methodName}' w typie '{fullTypeName}'.");

        return (TResult)method.Invoke(instance, parameters)!;
    }

    /// <summary>
    /// Wywołuje metodę statyczną z podanego typu.
    /// </summary>
    public static TResult InvokeStaticMethod<TResult>(
        this Assembly assembly, 
        string fullTypeName, 
        string methodName, 
        params object?[] parameters)
    {
        var type = assembly.GetType(fullTypeName);
        Assert.IsNotNull(type, $"Nie odnaleziono typu '{fullTypeName}' w załadowanym zestawieniu.");

        var method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
        Assert.IsNotNull(method, $"Nie odnaleziono metody statycznej '{methodName}' w typie '{fullTypeName}'.");

        return (TResult)method.Invoke(null, parameters)!;
    }
}