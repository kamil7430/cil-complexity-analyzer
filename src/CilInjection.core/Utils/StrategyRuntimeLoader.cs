namespace CilInstructionCounter.Core;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using CilInjection.Core;

internal class StrategyRuntimeLoader
{
    public void LoadInto(AssemblyLoadContext alc, IEnumerable<IInjectionStrategy> strategies)
    {
        ArgumentNullException.ThrowIfNull(alc);

        foreach (var assembly in GetRuntimeAssembliesToLoad(strategies))
        {
            alc.LoadFromAssemblyPath(assembly.Location);
        }
    }

    private IEnumerable<Assembly> GetRuntimeAssembliesToLoad(IEnumerable<IInjectionStrategy> strategies)
    {
        return strategies
            .Select(s => s.RuntimeMarkerType.Assembly)
            .DistinctBy(a => a.FullName);
    }
}