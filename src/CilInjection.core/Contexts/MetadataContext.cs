namespace CilInjection.Core.Contexts;

using System;
using System.Reflection;
using Abstractions;
using Mono.Cecil;

internal class MetadataContext(ModuleDefinition module) : IMetadataContext
{
    private readonly ModuleDefinition _module = module ?? throw new ArgumentNullException(nameof(module));

    private const BindingFlags MemberLookupFlags = 
        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;

    public TypeReference ImportType(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);
        return _module.ImportReference(type);
    }

    public FieldReference ImportField(FieldInfo fieldInfo)
    {
        ArgumentNullException.ThrowIfNull(fieldInfo);
        return _module.ImportReference(fieldInfo);
    }

    public MethodReference ImportMethod(MethodBase methodBase)
    {
        ArgumentNullException.ThrowIfNull(methodBase);
        return _module.ImportReference(methodBase);
    }
}