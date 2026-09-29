using CilInjection.Core.Utils;

namespace CilInstructionCounter.Core;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Loader;
using CilInjection.Core;
using Mono.Cecil;

public class CilInjectionManager
{
    private readonly List<IInjectionStrategy> _strategies = new();
    private readonly MethodProcessor _methodProcessor;
    private readonly StrategyRuntimeLoader _runtimeLoader;

    public CilInjectionManager(params IEnumerable<IInjectionStrategy> strategies)
    {
        ArgumentNullException.ThrowIfNull(strategies);
        _strategies.AddRange(strategies);
        _methodProcessor = new MethodProcessor();
        _runtimeLoader = new StrategyRuntimeLoader();
    }

    public CilInjectionManager AddStrategy(IInjectionStrategy strategy)
    {
        ArgumentNullException.ThrowIfNull(strategy);
        _strategies.Add(strategy);
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