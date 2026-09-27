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

        // Mono.Cecil wdrożył pełną obsługę FieldInfo – sama zbuduje poprawną referencję
        // do zewnętrznej biblioteki wraz z jej metadanymi (PublicKeyToken, Culture itd.).
        return targetModule.ImportReference(fieldInfo);
    }
}