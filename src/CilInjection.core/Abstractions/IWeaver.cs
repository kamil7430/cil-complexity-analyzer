using CilInjection.Core.Contexts;

namespace CilInjection.Core.BaseStrategy;

internal interface IWeaver
{
    void Transform(MethodTransformationContext methodTransformationContext);
    void Inject(MethodInjectionContext methodInjectionContext);
}

public abstract class BaseWeaver : IWeaver
{
    void IWeaver.Transform(MethodTransformationContext methodTransformationContext)
    {
        OnTransform(methodTransformationContext);
    }

    void IWeaver.Inject(MethodInjectionContext methodInjectionContext)
    {
        OnInject(methodInjectionContext);
    }
    
    protected abstract void OnTransform(MethodTransformationContext methodTransformationContext);
    protected abstract void OnInject(MethodInjectionContext methodInjectionContext);
}