namespace CilInjection.Core;

using Extensions;
using Mono.Cecil;
using System.Runtime.Loader;

public interface IInjectionStrategy
{
    // CIL Weaving: modyfikuje bajty Cecil
    void Inject(ModuleDefinition module);

    // Ładowanie wymaganej biblioteki RunTime do piaskownicy
    void LoadRuntime(AssemblyLoadContext context);
}

public interface IInjectionStrategy<out THandle> : IInjectionStrategy
    where THandle : class
{
    THandle BuildHandle(AssemblyLoadContext context);
}

public abstract class BaseInjectionStrategy<THandle> : IInjectionStrategy<THandle>
    where THandle : class
{
    /// <summary>
    /// Typ znajdujący się w bibliotece RunTime.dll danej strategii.
    /// Służy do automatycznego zlokalizowania i załadowania pliku .dll do ALC.
    /// </summary>
    protected abstract Type RuntimeMarkerType { get; }
    
    public abstract void Inject(ModuleDefinition module);

    public virtual void LoadRuntime(AssemblyLoadContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        context.LoadRuntimeFromType(RuntimeMarkerType);
    }

    public abstract THandle BuildHandle(AssemblyLoadContext context);
}

