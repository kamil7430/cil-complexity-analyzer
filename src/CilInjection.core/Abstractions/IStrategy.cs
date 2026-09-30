using CilInjection.Core.Extensions;
using System.Runtime.Loader;

namespace CilInjection.Core.Abstractions;

/// <summary>
/// Wewnętrzny interfejs potoku (używany tylko przez silnik biblioteki)
/// </summary>
internal interface IEngineStrategy
{
    void Transform(IMethodTransformationContext methodTransformationContext, IMetadataContext metadataContext);
    void Inject(IMethodInjectionContext methodInjectionContext, IMetadataContext metadataContext);
    void LoadRuntime(AssemblyLoadContext context);
}

public interface IInjectionStrategy
{
    internal IEngineStrategy Engine { get; }
}

public abstract class BaseInjectionStrategy : IEngineStrategy, IInjectionStrategy
{
    private readonly IEngineWeaver _engineWeaver;

    protected BaseInjectionStrategy(IWeaver weaver)
    {
        ArgumentNullException.ThrowIfNull(weaver);
        _engineWeaver = weaver.Engine;
    }
    
    IEngineStrategy IInjectionStrategy.Engine => this;
    
    protected abstract Type RuntimeMarkerType { get; }
    
    void IEngineStrategy.Transform(IMethodTransformationContext methodTransformationContext, IMetadataContext metadataContext)
    {
        ArgumentNullException.ThrowIfNull(methodTransformationContext);
        ArgumentNullException.ThrowIfNull(metadataContext);
        _engineWeaver.Transform(methodTransformationContext, metadataContext);
    }
    
    void IEngineStrategy.Inject(IMethodInjectionContext methodInjectionContext, IMetadataContext metadataContext)
    {
        ArgumentNullException.ThrowIfNull(methodInjectionContext);
        ArgumentNullException.ThrowIfNull(metadataContext);
        _engineWeaver.Inject(methodInjectionContext, metadataContext);
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
