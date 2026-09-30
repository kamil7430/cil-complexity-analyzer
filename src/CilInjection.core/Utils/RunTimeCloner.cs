namespace CilInjection.Core.Utils;

using System;
using System.IO;
using Mono.Cecil;

public static class RuntimeCloner
{
    /// <summary>
    /// Klonuje całą bibliotekę runtime w pamięci, unikalnie modyfikując nazwę assembly 
    /// oraz masowo aktualizując nazwy wszystkich zawartych w niej typów.
    /// </summary>
    /// <param name="baseDllPath">Ścieżka do bazowego pliku DLL.</param>
    /// <param name="sessionId">Unikalny identyfikator sesji (np. Guid).</param>
    /// <param name="typeRenamer">Opcjonalna funkcja definiująca nową nazwę dla każdego typu na podstawie jego starej nazwy.</param>
    public static byte[] CreateDynamicRuntimeBytes(
        string baseDllPath,
        string sessionId,
        Func<string, string, string> renamer)
    {
        ArgumentException.ThrowIfNullOrEmpty(baseDllPath);
        ArgumentException.ThrowIfNullOrEmpty(sessionId);
        ArgumentNullException.ThrowIfNull(renamer);

        var readerParameters = new ReaderParameters { ReadWrite = true };

        using var memoryStream = new MemoryStream(File.ReadAllBytes(baseDllPath));
        using var assemblyDefinition = AssemblyDefinition.ReadAssembly(memoryStream, readerParameters);
        
        var oldAssemblyName = assemblyDefinition.Name.Name;
        var newAssemblyName = renamer(oldAssemblyName, sessionId);
        assemblyDefinition.Name.Name = newAssemblyName;

        foreach (var module in assemblyDefinition.Modules)
        {
            // Zmieniamy nazwę samego modułu, jeśli jest powiązana z assembly
            if (module.Name == oldAssemblyName + ".dll" || module.Name == oldAssemblyName)
            {
                module.Name = newAssemblyName + ".dll";
            }

            // Mapujemy stare pełne nazwy typów na nowe, aby móc potem zaktualizować TypeReference
            var typeMapping = new Dictionary<string, (string NewNamespace, string NewName)>();

            foreach (var type in module.GetTypes())
            {
                if (type.Name == "<Module>") continue;

                var oldFullName = type.FullName;
                
                // Modyfikujemy nazwę oraz przestrzeń nazw (np. dodając sufix sesji do obu)
                var newTypeName = renamer(type.Name, sessionId);
                var newNamespace = string.IsNullOrEmpty(type.Namespace) 
                    ? sessionId 
                    : renamer(type.Namespace, sessionId); // Możesz dostosować logikę transformacji namespace

                type.Name = newTypeName;
                type.Namespace = newNamespace;

                typeMapping[oldFullName] = (newNamespace, newTypeName);
            }

            foreach (var asmRef in module.AssemblyReferences)
            {
                if (asmRef.Name == oldAssemblyName)
                {
                    asmRef.Name = newAssemblyName;
                }
            }
        }

        // Zapisujemy zmodyfikowany assembly do strumienia pamięci
        using var outputStream = new MemoryStream();
        assemblyDefinition.Write(outputStream);

        return outputStream.ToArray();
    }
}