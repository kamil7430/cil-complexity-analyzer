namespace CilInjection.Core.Abstractions;

internal interface IEngineWeaver
{
    void Transform(IMethodTransformationContext methodTransformationContext, IMetadataContext metadataContext);
    void Inject(IMethodInjectionContext methodInjectionContext, IMetadataContext metadataContext);
    void InjectType(IMetadataContext metadataContext);
}

public interface IWeaver
{
    internal IEngineWeaver Engine { get; }
}

public abstract class BaseWeaver : IEngineWeaver, IWeaver
{
    IEngineWeaver IWeaver.Engine => this;
    
    void IEngineWeaver.Transform(IMethodTransformationContext methodTransformationContext, IMetadataContext metadataContext)
    {
        ArgumentNullException.ThrowIfNull(methodTransformationContext);
        ArgumentNullException.ThrowIfNull(metadataContext);
        OnTransform(methodTransformationContext, metadataContext);
    }
    
    void IEngineWeaver.Inject(IMethodInjectionContext methodInjectionContext, IMetadataContext metadataContext)
    {
        ArgumentNullException.ThrowIfNull(methodInjectionContext);
        ArgumentNullException.ThrowIfNull(metadataContext);
        OnInject(methodInjectionContext, metadataContext);
    }
    
    void IEngineWeaver.InjectType(IMetadataContext metadataContext)
    {
        ArgumentNullException.ThrowIfNull(metadataContext);
        OnInjectType(metadataContext);
    }
    
    protected virtual void OnTransform(IMethodTransformationContext methodTransformationContext, IMetadataContext metadataContext) { }
    protected virtual void OnInject(IMethodInjectionContext methodInjectionContext, IMetadataContext metadataContext) { }
    protected virtual void OnInjectType(IMetadataContext metadataContext) { }
}
