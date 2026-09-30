namespace CilInjection.Core.Abstractions;

using Utils;
using System.Runtime.Loader;

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

    void IEngineStrategy.Transform(IMethodTransformationContext methodTransformationContext,
        IMetadataContext metadataContext)
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

    protected virtual void OnLoadRuntime(AssemblyLoadContext context) { }
}

public abstract class BaseInjectionStrategyWithRuntime : BaseInjectionStrategy
{
    private string SessionId { get; } = Guid.NewGuid().ToString("N")[..8];
    protected Func<string, string, string> Renamer { get; } = (name, sessionId) => $"{name}_{sessionId}";
    
    private readonly byte[] _dynamicDllBytes;
    
    protected BaseInjectionStrategyWithRuntime(string runtimeDllPath, IWeaver weaver) : base(weaver)
    {
        ArgumentException.ThrowIfNullOrEmpty(runtimeDllPath);

        _dynamicDllBytes = RuntimeCloner.CreateDynamicRuntimeBytes(
            baseDllPath: runtimeDllPath,
            sessionId: SessionId,
            renamer: Renamer
        );
    }
    
    protected override void OnLoadRuntime(AssemblyLoadContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        using var memoryStream = new MemoryStream(_dynamicDllBytes);
        context.LoadFromStream(memoryStream);
    }
}