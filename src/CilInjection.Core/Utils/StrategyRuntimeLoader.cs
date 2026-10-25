using CilInjection.Core.Abstractions;
using System;
using System.Collections.Generic;
using System.Runtime.Loader;

namespace CilInjection.Core.Utils;

internal class StrategyRuntimeLoader
{
    public void LoadInto(AssemblyLoadContext alc, IEnumerable<IEngineStrategy> strategies)
    {
        ArgumentNullException.ThrowIfNull(alc);

        foreach (var strategy in strategies)
        {
            strategy.LoadRuntime(alc);
        }
    }
}