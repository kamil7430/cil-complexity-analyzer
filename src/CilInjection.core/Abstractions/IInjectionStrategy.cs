using CilInjection.Core.Extensions;
using System.Runtime.Loader;
using CilInjection.Core.Contexts;
using CilInjection.Core.BaseStrategy;

namespace CilInjection.Core.Abstractions;

/// <summary>
/// Wewnętrzny interfejs potoku (używany tylko przez silnik biblioteki)
/// </summary>
internal interface IInjectionStrategy
{
    void Transform(IMethodTransformationContext methodTransformationContext);
    void Inject(IMethodInjectionContext methodInjectionContext);
    void LoadRuntime(AssemblyLoadContext context);
}

public abstract class BaseInjectionStrategy : IInjectionStrategy
{
    private readonly IWeaver _weaver;
    
    protected BaseInjectionStrategy(BaseWeaver weaver) 
        : this((IWeaver)weaver) { }

    internal BaseInjectionStrategy(IWeaver weaver)
    {
        _weaver = weaver ?? throw new ArgumentNullException(nameof(weaver));
    }
    
    protected abstract Type RuntimeMarkerType { get; }
    
    void IInjectionStrategy.Transform(IMethodTransformationContext methodTransformationContext)
    {
        ArgumentNullException.ThrowIfNull(methodTransformationContext);
        ArgumentNullException.ThrowIfNull(_weaver); 
        _weaver.Transform(methodTransformationContext);
    }
    
    void IInjectionStrategy.Inject(IMethodInjectionContext methodInjectionContext)
    {
        ArgumentNullException.ThrowIfNull(methodInjectionContext);
        ArgumentNullException.ThrowIfNull(_weaver); 
        _weaver.Inject(methodInjectionContext);
    }

    void IInjectionStrategy.LoadRuntime(AssemblyLoadContext context)
    {
        OnLoadRuntime(context);
    }

    protected virtual void OnLoadRuntime(AssemblyLoadContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        context.LoadRuntimeFromType(RuntimeMarkerType);
    }
}
