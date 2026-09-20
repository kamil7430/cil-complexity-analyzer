namespace CilInjecting.Tests.Infrastructure.Compilers;

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;

/// <summary>
/// Uniwersalny generator zestawów (.dll) na potrzeby testów jednostkowych.
/// Obsługuje kompilację w pamięci (RAM -> byte[]) oraz kompilację do plików tymczasowych na dysku (-> Type, FilePath).
/// </summary>
public static class TestAssemblyGenerator
{
    private static readonly MetadataReference[] DefaultReferences = new[]
    {
        MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
        MetadataReference.CreateFromFile(typeof(Attribute).Assembly.Location)
    };
    
    private const string DefaultAssemblyName = "DefaultTestRuntime";
    private const string DefaultTypeName = "Container";

    private static (string, string, string) DefaultSourceCode(string? assemblyName, string? typeName)
    {
        assemblyName = assemblyName ?? DefaultAssemblyName;
        typeName = typeName ?? DefaultTypeName;

        return ($@"
        namespace {assemblyName};

        public static class {typeName}
        {{
            public static long Counter;
            public static long GetCounter() => Counter;
            public static void ResetCounter() => Counter = 0;
        }}", assemblyName, typeName);
    }
        

    #region Kompilacja do bajtów w pamięci RAM (byte[])

    /// <summary>
    /// Kompiluje podany kod C# wyłącznie w pamięci RAM i zwraca bajty biblioteki .dll.
    /// </summary>
    public static byte[] CompileToBytes(string sourceCode, string assemblyName = "DynamicTestAssembly")
    {
        var compilation = CreateCompilation(sourceCode, assemblyName);

        using var memoryStream = new MemoryStream();
        EmitResult emitResult = compilation.Emit(memoryStream);

        EnsureCompilationSuccess(emitResult, assemblyName);

        return memoryStream.ToArray();
    }

    /// <summary>
    /// Generuje domyślne bajty biblioteki .dll z prostą klasą kontenera.
    /// </summary>
    public static byte[] CreateDefaultBytes(string assemblyName = "DefaultTestTarget")
    {
        (string sourceCode, assemblyName, _) = DefaultSourceCode(assemblyName, null);

        return CompileToBytes(sourceCode, assemblyName);
    }

    #endregion

    #region Kompilacja do pliku tymczasowego na dysku (Type, FilePath)

    /// <summary>
    /// Kompiluje kod C# bezpośrednio do pliku .dll w folderze Temp, ładuje zestaw i zwraca typ markerowy oraz ścieżkę do pliku.
    /// </summary>
    public static (Type MarkerType, string FilePath) CompileToTempDll(
        string sourceCode,
        string assemblyName,
        string typeName)
    {
        string filePath = Path.Combine(Path.GetTempPath(), $"{assemblyName}_{Guid.NewGuid():N}.dll");
        string fullTypeName = $"{assemblyName}.{typeName}";

        var compilation = CreateCompilation(sourceCode, assemblyName);

        EmitResult emitResult = compilation.Emit(filePath);

        EnsureCompilationSuccess(emitResult, assemblyName);

        var assembly = Assembly.LoadFrom(filePath);
        var markerType = assembly.GetType(fullTypeName)
            ?? throw new InvalidOperationException($"Nie odnaleziono typu '{typeName}' w wygenerowanym zestawie '{assemblyName}'.");

        return (markerType, filePath);
    }

    /// <summary>
    /// Generuje domyślny plik tymczasowy .dll na dysku z podstawową klasą i zwraca załadowany typ oraz ścieżkę.
    /// </summary>
    public static (Type MarkerType, string FilePath) CreateDefaultTempDll(
        string? assemblyName = null, 
        string? typeName = null)
    {
        (string sourceCode, assemblyName, typeName) = DefaultSourceCode(assemblyName, typeName);
        
        return CompileToTempDll(sourceCode, assemblyName, typeName);
    }

    #endregion

    #region Helpery wewnętrzne

    private static CSharpCompilation CreateCompilation(string sourceCode, string assemblyName)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);

        return CSharpCompilation.Create(
            assemblyName,
            new[] { syntaxTree },
            DefaultReferences,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }

    private static void EnsureCompilationSuccess(EmitResult emitResult, string assemblyName)
    {
        if (!emitResult.Success)
        {
            var errors = string.Join("\n", emitResult.Diagnostics
                .Where(d => d.Severity == DiagnosticSeverity.Error)
                .Select(d => d.GetMessage()));

            throw new InvalidOperationException($"Kompilacja zestawu C# '{assemblyName}' nie powiodła się:\n{errors}");
        }
    }

    #endregion
}