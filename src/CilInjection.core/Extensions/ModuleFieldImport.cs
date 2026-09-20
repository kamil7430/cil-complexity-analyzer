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
        
        var fieldInfo = containerType.GetField(
                            fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static) 
                        ?? throw new MissingFieldException(containerType.FullName, fieldName);

        // BUG odkryty przy testowaniu bibliotek:
        // z "module: targetModule" sprawia, że Cecil traktuje do jak referencję JUŻ 
        // należącą do targetModule i pomija rejestrację assemblyRef w targetModule.AssemblyReferences. Po zapisie
        // modułu brakuje wpisu Assembly.Ref
        // dla biblioteki RunTime -> CLR przy JIT-owaniu (takie słowo xD ) szuka typu lokalnie i mamy
        // TypeLoadException.
        // TODO: ModuleFieldImportTest dla regresji
        /*
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
        */
        return targetModule.ImportReference(fieldInfo);
    }
}