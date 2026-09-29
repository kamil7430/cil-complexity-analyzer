using CilInjection.Core.Extensions;
using System.Runtime.Loader;
using CilInjection.Core.Contexts;
using CilInjection.Core.BaseStrategy;

/// <summary>
/// Wewnętrzny interfejs potoku (używany tylko przez silnik biblioteki)
/// </summary>
internal interface IInjectionStrategy
{
    Type RuntimeMarkerType { get; }
    void Transform(MethodTransformationContext methodTransformationContext);
    void Inject(MethodInjectionContext methodInjectionContext);
    void LoadRuntime(AssemblyLoadContext context);
}

public abstract class BaseInjectionStrategy : IInjectionStrategy
{
    private readonly IWeaver _weaver;
    
    internal BaseInjectionStrategy(IWeaver weaver)
    {
        _weaver = weaver ?? throw new ArgumentNullException(nameof(weaver));
    }
    
    public abstract Type RuntimeMarkerType { get; }
    
    void IInjectionStrategy.Transform(MethodTransformationContext methodTransformationContext)
    {
        ArgumentNullException.ThrowIfNull(methodTransformationContext);
        ArgumentNullException.ThrowIfNull(_weaver); 
        _weaver.Transform(methodTransformationContext);
    }
    
    void IInjectionStrategy.Inject(MethodInjectionContext methodInjectionContext)
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
