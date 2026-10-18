namespace CilInjection.Core;

using Extensions;
using Mono.Cecil;
using System.Runtime.Loader;

public interface IInjectionStrategy
{
    Type RuntimeMarkerType { get; }
    void Inject(ModuleDefinition module);

    void LoadRuntime(AssemblyLoadContext context);
}

public interface IInjectionStrategy<out THandle> : IInjectionStrategy
    where THandle : class
{
    THandle BuildHandle(AssemblyLoadContext context);
}

public abstract class BaseInjectionStrategy(IWeaver weaver) : IInjectionStrategy
{
    public abstract Type RuntimeMarkerType { get; }

    public void Inject(ModuleDefinition module)
    {
        ArgumentNullException.ThrowIfNull(module);
        ArgumentNullException.ThrowIfNull(weaver); 
        weaver.Inject(module);
    }

    public virtual void LoadRuntime(AssemblyLoadContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        context.LoadRuntimeFromType(RuntimeMarkerType);
    }
}

public abstract class BaseInjectionStrategy<THandle>(IWeaver weaver) : BaseInjectionStrategy(weaver), IInjectionStrategy<THandle>
    where THandle : class
{
    public abstract THandle BuildHandle(AssemblyLoadContext context);
}

