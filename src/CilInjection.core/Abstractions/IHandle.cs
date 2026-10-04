using System.Reflection;
using System.Runtime.Loader;

namespace CilInjection.Core.Abstractions;

internal interface IHandle
{
    void Initialize(AssemblyLoadContext context);
    void Initialize(AssemblyLoadContext context, Func<string,string> translateFunction);
}

public abstract class BaseRuntimeHandle : IHandle
{
    private readonly Dictionary<string, Type> _types = new();

    protected BaseRuntimeHandle(AssemblyLoadContext context, params List<string> typeNames)
    {
        LoadTypesFromContext(context, typeNames);
    }

    protected TDelegate BindMethod<TDelegate>(
        string typeName, 
        string methodName, 
        BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Static) where TDelegate : Delegate
    {
        var targetType = GetTypeOrThrow(typeName);
        var method = GetMethod(targetType, methodName, bindingFlags);
        return method.CreateDelegate<TDelegate>();
    }

    protected FieldInfo BindField(
        string typeName, 
        string fieldName, 
        BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Static)
    {
        var targetType = GetTypeOrThrow(typeName);
        return GetField(targetType, fieldName, bindingFlags);
    }

    public void Initialize(AssemblyLoadContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        
    }
    
    private Type GetTypeOrThrow(string typeName)
    {
        if (_types.TryGetValue(typeName, out var type))
        {
            return type;
        }

        throw new KeyNotFoundException($"Typ '{typeName}' nie został zarejestrowany w procesie inicjalizacji Handle.");
    }
    
    private void LoadTypesFromContext(AssemblyLoadContext context, List<string> typeNames)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(typeNames);

        var missingTypes = new HashSet<string>(typeNames);

        foreach (var assembly in context.Assemblies)
        {
            foreach (var typeName in missingTypes.ToList())
            {
                var type = assembly.GetType(typeName);
                if (type != null)
                {
                    _types[typeName] = type;
                    missingTypes.Remove(typeName);
                }
            }
            
            if (missingTypes.Count == 0)
            {
                break;
            }
        }
        
        if (missingTypes.Count > 0)
        {
            throw new InvalidOperationException(
                $"Nie odnaleziono następujących typów w podanym AssemblyLoadContext: {string.Join(", ", missingTypes)}");
        }
    }

    private static MethodInfo GetMethod(Type targetType, string methodName, BindingFlags bindingFlags)
    {
        ArgumentNullException.ThrowIfNull(targetType);
        ArgumentException.ThrowIfNullOrEmpty(methodName);

        return targetType.GetMethod(methodName, bindingFlags)
               ?? throw new InvalidOperationException($"Nie odnaleziono metody '{methodName}' w typie '{targetType.FullName}'.");
    }

    private static FieldInfo GetField(Type targetType, string fieldName, BindingFlags bindingFlags)
    {
        ArgumentNullException.ThrowIfNull(targetType);
        ArgumentException.ThrowIfNullOrEmpty(fieldName);

        return targetType.GetField(fieldName, bindingFlags)
               ?? throw new InvalidOperationException($"Nie odnaleziono pola '{fieldName}' w typie '{targetType.FullName}'.");
    }
}

