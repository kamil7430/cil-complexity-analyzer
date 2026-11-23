namespace CilInjection.Core.Contexts;

using System;
using System.Reflection;
using Abstractions;
using Mono.Cecil;

internal class MetadataContext(ModuleDefinition module) : IMetadataContext
{
    private readonly ModuleDefinition _module = module ?? throw new ArgumentNullException(nameof(module));

    private bool IsPrivateCoreLib =>
        _module.Assembly?.Name?.Name == "System.Private.CoreLib" ||
        (_module.AssemblyReferences.Count == 0 && _module.GetType("System", "Object") is not null);
    
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
        string @namespace,
        string name,
        Action<TypeDefinition, ModuleDefinition>? configureType = null)
    {
        if (!IsPrivateCoreLib)
        {
            return null;
        }

        var existingType = _module.GetType(@namespace, name);
        if (existingType is not null)
        {
            return existingType;
        }

        // Pobierz lokalną definicję System.Object (bez tworzenia zewnętrznej referencji)
        var baseObjectType = _module.GetType("System", "Object")
                             ?? throw new InvalidOperationException("System.Object definition not found inside CoreLib.");

        var newType = new TypeDefinition(
            @namespace,
            name,
            TypeAttributes.Public | TypeAttributes.Abstract | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit,
            baseObjectType);

        // 4. Pozwól strategii dodać pola/metody używając lokalnego modułu CoreLib
        configureType?.Invoke(newType, _module);

        _module.Types.Add(newType);

        return newType;
    } 
}