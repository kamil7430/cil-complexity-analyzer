namespace CilInjection.Core.Utils;

using System;
using System.IO;
using Mono.Cecil;

internal static class RuntimeCloner
{
    /// <summary>
    /// Klonuje całą bibliotekę runtime w pamięci, unikalnie modyfikując nazwę assembly 
    /// oraz masowo aktualizując nazwy wszystkich zawartych w niej typów.
    /// </summary>
    /// <param name="baseDllBytes">Tablica bajtów bazowego pliku .dll</param>
    /// <param name="sessionId">Unikalny identyfikator sesji (np. Guid).</param>
    /// <param name="renamer">Funkcja, która przyjmuje starą nazwę i identyfikator sesji, a zwraca nową nazwę.</param>
    /// <param name="typeNameDictionary">Słownik wyjściowy przechowujący mapowanie starych nazw typów na nowe.</param>
    public static byte[] CreateDynamicRuntimeBytes(
        byte[] baseDllBytes,
        string sessionId,
        Func<string, string, string> renamer,
        Dictionary<string, string> typeNameDictionary)
    {
        ArgumentNullException.ThrowIfNull(baseDllBytes);
        ArgumentException.ThrowIfNullOrEmpty(sessionId);
        ArgumentNullException.ThrowIfNull(renamer);
        ArgumentNullException.ThrowIfNull(typeNameDictionary);

        var readerParameters = new ReaderParameters { ReadWrite = true };

        using var memoryStream = new MemoryStream(baseDllBytes);
        using var assemblyDefinition = AssemblyDefinition.ReadAssembly(memoryStream, readerParameters);
        
        var oldAssemblyName = assemblyDefinition.Name.Name;
        var newAssemblyName = renamer(oldAssemblyName, sessionId);
        assemblyDefinition.Name.Name = newAssemblyName;

        foreach (var module in assemblyDefinition.Modules)
        {
            if (module.Name == oldAssemblyName + ".dll" || module.Name == oldAssemblyName)
            {
                module.Name = newAssemblyName + ".dll";
            }
            foreach (var type in module.GetTypes())
            {
                if (type.Name == "<Module>") continue;
                
                string oldFullName = type.FullName;
                
                var newNamespace = string.IsNullOrEmpty(type.Namespace) 
                    ? sessionId 
                    : renamer(type.Namespace, sessionId); 

                type.Namespace = newNamespace;
                
                string newFullName = type.FullName; 
                typeNameDictionary[oldFullName] = newFullName;
            }

            foreach (var asmRef in module.AssemblyReferences)
            {
                if (asmRef.Name == oldAssemblyName)
                {
                    asmRef.Name = newAssemblyName;
                }
            }
        }

        using var outputStream = new MemoryStream();
        assemblyDefinition.Write(outputStream);

        return outputStream.ToArray();
    }
}