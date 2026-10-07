
using Mono.Cecil;

namespace CilInjection.Core.Abstractions;

using Utils;
using System.Runtime.Loader;

/// <summary>
/// Wewnętrzny interfejs potoku (używany tylko przez silnik biblioteki)
/// </summary>
public interface IEngineStrategy
{
    internal void Transform(IMethodTransformationContext methodTransformationContext, IMetadataContext metadataContext);
    internal void Inject(IMethodInjectionContext methodInjectionContext, IMetadataContext metadataContext);
    internal void LoadRuntime(AssemblyLoadContext context);
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

public abstract class BaseInjectionStrategyWithRuntime<THandleInterface> : BaseInjectionStrategy
where THandleInterface : IHandle
{
    private static readonly string SessionId = Guid.NewGuid().ToString("N")[..8];
    private static readonly Func<string, string, string> Renamer = (name, sessionId) => $"{name}_{sessionId}";
    
    private readonly byte[] _dynamicDllBytes;
    private Dictionary<string, string> TypeMapping { get; } = new(StringComparer.Ordinal);
    
    protected BaseInjectionStrategyWithRuntime(byte[] baseDllBytes, IWeaver weaver) : base(weaver)
    {
        ArgumentNullException.ThrowIfNull(baseDllBytes);
        
        _dynamicDllBytes = RuntimeCloner.CreateDynamicRuntimeBytes(
            baseDllBytes: baseDllBytes,
            sessionId: SessionId,
            renamer: Renamer,
            typeNameDictionary: TypeMapping
        );
    }
    
    // Alternatywny konstruktor pomocniczy delegujący do głównego przez util wczytujący z dysku
    protected BaseInjectionStrategyWithRuntime(string resourceName, IWeaver weaver) 
        : this(RuntimeFileReader.ReadDllBytesFromBasePath(resourceName), weaver)
    {
    }
    
    public THandleInterface CreateHandle(AssemblyLoadContext context) 
    {
        var handleFactory = CreateHandleInstance();
            
        ((IHandle)handleFactory).BindContext(context, GetMappedFullName);

        return handleFactory;
    }
    
    protected abstract THandleInterface CreateHandleInstance();
    
    protected FieldReference ResolveRuntimeField(string originalFullTypeName, string fieldName)
    {
        string newFullTypeName = GetMappedFullName(originalFullTypeName);
        return RuntimeMetadataHelper.ResolveFieldFromBytes(_dynamicDllBytes, newFullTypeName, fieldName);
    }
    
    protected override void OnLoadRuntime(AssemblyLoadContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        using var memoryStream = new MemoryStream(_dynamicDllBytes);
        context.LoadFromStream(memoryStream);
    }
    
    private string GetMappedFullName(string originalFullTypeName)
    {
        if (TypeMapping.TryGetValue(originalFullTypeName, out var newFullName))
        {
            return newFullName;
        }
        throw new KeyNotFoundException($"Nie znalezino mapowania dla typu: {originalFullTypeName}");
    }
}