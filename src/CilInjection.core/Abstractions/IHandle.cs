using System.Reflection;
using System.Runtime.Loader;

namespace CilInjection.Core.Abstractions;

public interface IHandleFactory
{
    internal void BindContext(AssemblyLoadContext context, Func<string, string> nameResolver);
}

public abstract class BaseRuntimeHandleFactory<THandle> : IHandleFactory
{
    private readonly Dictionary<string, Type> _types = new();
    private AssemblyLoadContext? _context;
    private Func<string,string> _nameResolver = (s) => s;
    
    protected bool IsBound => _context != null;

    protected abstract THandle CreateHandle();
    protected virtual void OnBound() { }

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
    
    void IHandleFactory.BindContext(AssemblyLoadContext context, Func<string, string> nameResolver)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(nameResolver);
        
        _context = context;
        _nameResolver = nameResolver;
        OnBound();
    }
    
    private Type GetTypeOrThrow(string typeName)
    {
        typeName = _nameResolver(typeName);
        if (_types.TryGetValue(typeName, out var type))
        {
            return type;
        }

        return LoadTypeFromContext(GetContextOrThrow(), typeName);
    }

    private AssemblyLoadContext GetContextOrThrow()
    {
        if (_context == null)
        {
            throw new InvalidOperationException(
                "Próbujesz powiązać metodę lub pole w momencie, gdy Handle nie został jeszcze zainicjowany. " +
                "Upewnij się, że wywołujesz BindMethod / BindField wyłącznie wewnątrz metody OnBound().");
        }
        return _context;
    }
    
    private Type LoadTypeFromContext(AssemblyLoadContext context, string typeName)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(typeName);

        foreach (var assembly in context.Assemblies)
        {
            
            var type = assembly.GetType(typeName);
            if (type != null)
            {
                _types[typeName] = type;
                return type;
            }
        }
        
        throw new InvalidOperationException(
            $"Nie odnaleziono {typeName} w podanym AssemblyLoadContext");
        
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

