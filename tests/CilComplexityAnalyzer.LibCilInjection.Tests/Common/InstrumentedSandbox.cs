using System;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using CilInjection.Core;
using CilInstructionCounter;

namespace CilComplexityAnalyzer.LibCilInjection.Tests.Common;

/// <summary>
/// Owija cykl życia pojedynczego przypadku testowego: waży bajty przez pipeline,
/// tworzy izolowany, collectible AssemblyLoadContext, ładuje do niego runtime
/// PRZED instrumentowanym assembly (kolejność jest krytyczna — zob. ModuleFieldImportTests
/// / komentarz w ModuleFieldImport.cs), i udostępnia uchwyt licznika.
/// Dispose zwalnia AssemblyLoadContext.
///
/// ZMIANA: opcjonalne <c>instrumentedDependencies</c> — biblioteki (np. Graphs.dll), które mają
/// być zainstrumentowane tym samym pipeline'em i załadowane do TEGO SAMEGO kontekstu, dzięki czemu
/// ich instrukcje trafiają do tego samego licznika co kod główny.
/// Zależności ładowane są PRZED głównym assembly, żeby referencja z niego rozwiązała się do wersji
/// zainstrumentowanej, a nie do nieinstrumentowanej z Default ALC.
/// Dotychczasowe wywołania Create(bytes, name) działają bez zmian.
/// </summary>
internal sealed class InstrumentedSandbox : IDisposable
{
    private readonly AssemblyLoadContext _context;

    public Assembly Assembly { get; }
    public ICounterHandle Counter { get; }

    private InstrumentedSandbox(AssemblyLoadContext context, Assembly assembly, ICounterHandle counter)
    {
        _context = context;
        Assembly = assembly;
        Counter = counter;
    }

    public static InstrumentedSandbox Create(
        byte[] originalAssemblyBytes,
        string contextName,
        params byte[][] instrumentedDependencies)
    {
        var strategy = new InstructionCounterStrategy(new InstructionCounterWeaver());
        var pipeline = new InjectionPipeline(new IInjectionStrategy[] { strategy });

        // Transform wszystkiego zanim ruszymy z ładowaniem — ten sam pipeline/strategy = ten sam licznik.
        var transformedDependencies = new byte[instrumentedDependencies.Length][];
        for (int i = 0; i < instrumentedDependencies.Length; i++)
            transformedDependencies[i] = pipeline.Transform(instrumentedDependencies[i]);

        byte[] instrumentedBytes = pipeline.Transform(originalAssemblyBytes);

        var context = new AssemblyLoadContext(contextName, isCollectible: true);

        pipeline.LoadRuntimesInto(context);

        foreach (var dependencyBytes in transformedDependencies)
        {
            using var dependencyStream = new MemoryStream(dependencyBytes);
            context.LoadFromStream(dependencyStream);
        }

        Assembly assembly;
        using (var ms = new MemoryStream(instrumentedBytes))
        {
            assembly = context.LoadFromStream(ms);
        }

        var handle = strategy.BuildHandle(context);
        return new InstrumentedSandbox(context, assembly, handle);
    }

    public void Dispose() => _context.Unload();
}