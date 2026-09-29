using CilInjection.Core.Extensions;
using System.Runtime.Loader;
using CilInjection.Core.Contexts;
using CilInjection.Core.BaseStrategy;

namespace CilInjection.Core.Abstractions;

/// <summary>
/// Wewnętrzny interfejs potoku (używany tylko przez silnik biblioteki)
/// </summary>
internal interface IEngineStrategy
{
    void Transform(IMethodTransformationContext methodTransformationContext);
    void Inject(IMethodInjectionContext methodInjectionContext);
    void LoadRuntime(AssemblyLoadContext context);
}

public interface IInjectionStrategy
{
    internal IEngineStrategy Engine { get; }
}

public abstract class BaseEngineStrategy : IEngineStrategy, IInjectionStrategy
{
    private readonly IWeaver _weaver;
    
    protected BaseEngineStrategy(BaseWeaver weaver) 
        : this((IWeaver)weaver) { }

    internal BaseEngineStrategy(IWeaver weaver)
    {
        _weaver = weaver ?? throw new ArgumentNullException(nameof(weaver));
    }
    
    IEngineStrategy IInjectionStrategy.Engine => this;
    
    protected abstract Type RuntimeMarkerType { get; }
    
    void IEngineStrategy.Transform(IMethodTransformationContext methodTransformationContext)
    {
        ArgumentNullException.ThrowIfNull(methodTransformationContext);
        ArgumentNullException.ThrowIfNull(_weaver); 
        _weaver.Transform(methodTransformationContext);
    }
    
    void IEngineStrategy.Inject(IMethodInjectionContext methodInjectionContext)
    {
        ArgumentNullException.ThrowIfNull(methodInjectionContext);
        ArgumentNullException.ThrowIfNull(_weaver); 
        _weaver.Inject(methodInjectionContext);
    }

    void IEngineStrategy.LoadRuntime(AssemblyLoadContext context)
    {
        OnLoadRuntime(context);
    }

    protected virtual void OnLoadRuntime(AssemblyLoadContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        context.LoadRuntimeFromType(RuntimeMarkerType);
    }
}
