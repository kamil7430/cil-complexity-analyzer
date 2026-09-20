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

    public static InstrumentedSandbox Create(byte[] originalAssemblyBytes, string contextName)
    {
        var strategy = new InstructionCounterStrategy(new InstructionCounterWeaver());
        var pipeline = new InjectionPipeline(new IInjectionStrategy[] { strategy });

        byte[] instrumentedBytes = pipeline.Transform(originalAssemblyBytes);

        var context = new AssemblyLoadContext(contextName, isCollectible: true);

        pipeline.LoadRuntimesInto(context);

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