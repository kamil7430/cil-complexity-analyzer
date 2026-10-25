namespace CilInjection.Core.Utils;

using Mono.Cecil;

public static class RuntimeMetadataHelper
{
    /// <summary>
    /// Wyszukuje FieldReference w surowych bajtach zestawu na podstawie pełnej nazwy typu i nazwy pola.
    /// </summary>
    public static FieldReference ResolveFieldFromBytes(byte[] dynamicDllBytes, string fullTypeName, string fieldName)
    {
        ArgumentNullException.ThrowIfNull(dynamicDllBytes);
        ArgumentException.ThrowIfNullOrEmpty(fullTypeName);
        ArgumentException.ThrowIfNullOrEmpty(fieldName);

        using var memoryStream = new MemoryStream(dynamicDllBytes);
        using var assemblyDefinition = AssemblyDefinition.ReadAssembly(memoryStream);

        foreach (var module in assemblyDefinition.Modules)
        {
            var typeDef = module.GetTypes().FirstOrDefault(t => t.FullName == fullTypeName);
            if (typeDef != null)
            {
                var fieldDef = typeDef.Fields.FirstOrDefault(f => f.Name == fieldName);
                if (fieldDef != null)
                {
                    return module.ImportReference(fieldDef);
                }
            }
        }

        throw new MissingFieldException($"Nie znaleziono pola '{fieldName}' dla typu o nazwie po mapowaniu: '{fullTypeName}'.");
    }
}