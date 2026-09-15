namespace CilInjection.Core.Extensions;

using System;
using System.Reflection;
using Mono.Cecil;

public static class ModuleFieldImportExtensions
{
    /// <summary>
    /// Importuje referencję do statycznego pola z dowolnej klasy RunTime.
    /// Typ pola jest automatycznie rozpoznawany na podstawie refleksji.
    /// </summary>
    public static FieldReference ImportStaticField(
        this ModuleDefinition targetModule,
        Type containerType,
        string fieldName)
    {
        ArgumentNullException.ThrowIfNull(targetModule);
        ArgumentNullException.ThrowIfNull(containerType);
        
        var fieldInfo = containerType.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                        ?? throw new MissingFieldException(containerType.FullName, fieldName);

        var assemblyName = containerType.Assembly.GetName();

        var assemblyRef = new AssemblyNameReference(
            assemblyName.Name!,
            assemblyName.Version);

        var typeRef = new TypeReference(
            @namespace: containerType.Namespace!,
            name: containerType.Name,
            module: targetModule,
            scope: assemblyRef);

        var fieldTypeRef = targetModule.ImportReference(fieldInfo.FieldType);

        var fieldRef = new FieldReference(
            name: fieldName,
            fieldType: fieldTypeRef,
            declaringType: typeRef);

        return targetModule.ImportReference(fieldRef);
    }
}