using CilInjection.Core.Utils;
using System.Runtime.Loader;
using CilInjection.Core.Abstractions;
using Mono.Cecil;
using CilInjection.Core.Utils;

namespace CilInjection.Core;

public class CilInjectionManager
{
    private readonly List<IEngineStrategy> _strategies = new();
    private readonly MethodProcessor _methodProcessor = new();
    private readonly StrategyRuntimeLoader _runtimeLoader = new();
    
    public CilInjectionManager(params IEnumerable<IInjectionStrategy> strategies)
    {
        AddStrategies(strategies);
    }
    
    public CilInjectionManager AddStrategy(IInjectionStrategy strategy)
    {
        ArgumentNullException.ThrowIfNull(strategy);
        var engineStrategy = strategy.Engine;
        _strategies.Add(engineStrategy);
        return this;
    }

    public CilInjectionManager AddStrategies(params IEnumerable<IInjectionStrategy> strategies)
    {
        ArgumentNullException.ThrowIfNull(strategies);
        foreach (var strategy in strategies)
        {
            AddStrategy(strategy);
        }
        return this;
    }

    public byte[] Transform(byte[] assemblyBytes)
    {
        if (_strategies.Count == 0)
            return assemblyBytes;

        using var inputStream = new MemoryStream(assemblyBytes);
        using var module = ModuleDefinition.ReadModule(inputStream);

        var methods = module.GetTypes()
            .SelectMany(t => t.Methods)
            .Where(m => m.HasBody && m.Body.Instructions.Count > 0)
            .ToList();

        foreach (var method in methods)
        {
            _methodProcessor.ProcessMethod(method, _strategies);
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
        _runtimeLoader.LoadInto(alc, _strategies);
    }
}