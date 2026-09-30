namespace CilInjection.Core.Contexts;

using System;
using System.Reflection;
using Abstractions;
using Mono.Cecil;

internal class MetadataContext(ModuleDefinition module) : IMetadataContext
{
    private readonly ModuleDefinition _module = module ?? throw new ArgumentNullException(nameof(module));

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
}