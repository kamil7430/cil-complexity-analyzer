using CilInjection.Core.Abstractions;
using CilInjection.Core.Contexts;

namespace CilInjection.Core.BaseStrategy;

internal interface IWeaver
{
    void Transform(IMethodTransformationContext methodTransformationContext);
    void Inject(IMethodInjectionContext methodInjectionContext);
}

public abstract class BaseWeaver : IWeaver
{
    void IWeaver.Transform(IMethodTransformationContext methodTransformationContext)
    {
        ArgumentNullException.ThrowIfNull(methodTransformationContext);
        OnTransform(methodTransformationContext);
    }
    
    void IWeaver.Inject(IMethodInjectionContext methodInjectionContext)
    {
        ArgumentNullException.ThrowIfNull(methodInjectionContext);
        OnInject(methodInjectionContext);
    }
    
    protected virtual void OnTransform(IMethodTransformationContext methodTransformationContext) { }
    protected virtual void OnInject(IMethodInjectionContext methodInjectionContext) { }
}