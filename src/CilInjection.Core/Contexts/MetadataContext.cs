using CilInjection.Core.Utils;

namespace CilInjection.Core.Contexts;

using System;
using Abstractions;
using Mono.Cecil;

internal class MetadataContext(ModuleDefinition module) : IMetadataContext
{
    private readonly ModuleDefinition _module = module ?? throw new ArgumentNullException(nameof(module));

    public bool IsPrivateCoreLib { get; } = CoreLibTypeCloner.IsPrivateCoreLib(module);
    
    public FieldReference ImportField(FieldReference fieldReference)
    {
        ArgumentNullException.ThrowIfNull(fieldReference);
        return _module.ImportReference(fieldReference);
    }

    public TypeReference ImportType(TypeReference typeReference)
    {
        ArgumentNullException.ThrowIfNull(typeReference);
        return _module.ImportReference(typeReference);
    }

    public MethodReference ImportMethod(MethodReference methodReference)
    {
        ArgumentNullException.ThrowIfNull(methodReference);
        return _module.ImportReference(methodReference);
    }
    
    public TypeDefinition? TryAddTypeToCoreLib(
        Type templateType,
        string? targetNamespace = null,
        string? targetTypeName = null)
    {
        ArgumentNullException.ThrowIfNull(templateType);

        if (!IsPrivateCoreLib)
        {
            return null;
        }
        
        var ns = targetNamespace ?? templateType.Namespace ?? "System.Runtime";
        var name = targetTypeName ?? templateType.Name;
        
        var existing = _module.GetType(ns, name);
        if (existing is not null)
        {
            return existing;
        }
        
        return CoreLibTypeCloner.CloneTypeIntoCoreLib(_module, templateType, ns, name);
    }
}