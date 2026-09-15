namespace CilInjection.Core;

using Mono.Cecil;

public class InjectionPipeline
{
    // Lista zarejestrowanych strategii (pole klasy)
    private readonly List<IInjectionStrategy> _strategies = new();

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
            return assemblyBytes; // Brak strategii = brak zmian

        using var stream = new MemoryStream(assemblyBytes);
        using var module = ModuleDefinition.ReadModule(stream);

        // Aplikujemy każdą strategię po kolei na TYM SAMYM module (jednokrotny odczyt)
        foreach (var strategy in _strategies)
        {
            strategy.Inject(module);
        }

        using var outputStream = new MemoryStream();
        module.Write(outputStream);
        return outputStream.ToArray();
    }

    /// <summary>
    /// Zwraca listę ścieżek do plików RunTime.dll wymaganych przez zarejestrowane strategie.
    /// </summary>
    public IEnumerable<string> GetRequiredRuntimePaths()
    {
        return _strategies.Select(s => s.GetRuntimeAssemblyPath()).Distinct();
    }

    /// <summary>
    /// Pobiera zarejestrowane strategie (np. do późniejszego wygenerowania uchwytów w ALC).
    /// </summary>
    public IReadOnlyList<IInjectionStrategy> Strategies => _strategies.AsReadOnly();
}