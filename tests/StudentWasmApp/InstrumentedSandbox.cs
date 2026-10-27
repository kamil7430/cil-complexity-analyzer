using System;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using CilInjection.Core;
using CilInjection.Core.Abstractions;
using Counter.Provider.Base;

namespace StudentWasmApp;
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
        BaseInjectionStrategyWithRuntime<ICounterHandle> strategy,  
        params byte[][] instrumentedDependencies)
    {
        var injector = new CilInjectionManager(strategy);

        var transformedDependencies = new byte[instrumentedDependencies.Length][];
        for (int i = 0; i < instrumentedDependencies.Length; i++)
        {
            transformedDependencies[i] = injector.Transform(instrumentedDependencies[i]);
        }

        byte[] instrumentedBytes = injector.Transform(originalAssemblyBytes);

        var context = new AssemblyLoadContext(contextName, isCollectible: true);

        injector.LoadRuntimesInto(context);

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

        var handle = strategy.CreateHandle(context);
        return new InstrumentedSandbox(context, assembly, handle);
    }

    public void Dispose() => _context.Unload();
}