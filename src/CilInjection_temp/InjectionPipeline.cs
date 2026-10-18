namespace CilInjection.Core;

using Mono.Cecil;
using System.Runtime.Loader;
using System.Reflection;

/// TODO zrobić metodę która zwraca interfejs do obługi wgranych bibliotek

public class InjectionPipeline
{
    private readonly List<IInjectionStrategy> _strategies = new();

    public InjectionPipeline(params IEnumerable<IInjectionStrategy> strategies)
    {
        ArgumentNullException.ThrowIfNull(strategies);
        _strategies.AddRange(strategies);
    }

    /// <summary>
    /// Rejestruje nową strategię iniekcji w potoku.
    /// </summary>
    public InjectionPipeline AddStrategy(IInjectionStrategy strategy)
    {
        ArgumentNullException.ThrowIfNull(strategy);
        _strategies.Add(strategy);
        return this;
    }

    /// <summary>
    /// Przetwarza strumień bajtów pliku .dll przez wszystkie zarejestrowane strategie.
    /// </summary>
    public byte[] Transform(byte[] assemblyBytes)
    {
        if (_strategies.Count == 0)
            return assemblyBytes; 

        using var stream = new MemoryStream(assemblyBytes);
        using var module = ModuleDefinition.ReadModule(stream);

        foreach (var strategy in _strategies)
        {
            strategy.Inject(module);
        }

        using var outputStream = new MemoryStream();
        module.Write(outputStream);
        return outputStream.ToArray();
    }

    /// <summary>
    /// Ładuje biblioteki RunTime wszystkich zarejestrowanych strategii do podanego kontekstu piaskownicy.
    /// </summary>
    public void LoadRuntimesInto(AssemblyLoadContext alc)
    {
        ArgumentNullException.ThrowIfNull(alc);

        foreach (var assembly in GetRuntimeAssembliesToLoad())
        {
            alc.LoadFromAssemblyPath(assembly.Location);
        }
    }
    
    private IEnumerable<Assembly> GetRuntimeAssembliesToLoad()
    {
        return _strategies
            .Select(s => s.RuntimeMarkerType.Assembly)
            .DistinctBy(a => a.FullName);
    }
}
    