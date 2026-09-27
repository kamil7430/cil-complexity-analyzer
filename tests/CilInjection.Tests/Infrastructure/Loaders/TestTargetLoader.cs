namespace CilInjecting.Tests.Infrastructure.Loaders;

using System;
using System.IO;
using Mono.Cecil;
using TestTargets.Basic;
using CilInjecting.Tests.Infrastructure.Compilers;

/// <summary>
/// Pomocniczy loader odpowiadający za wczytywanie kodów źródłowych oraz
/// skompilowanych zestawów z projektu CilInjecting.TestTargets.
/// </summary>
public static class TestTargetLoader
{
    private static readonly string SourceFolder = Path.Combine(AppContext.BaseDirectory, "TestTargetsSource");

    /// <summary>
    /// Wczytuje kod źródłowy C# z pliku .cs znajdującego się w projekcie TestTargets.
    /// </summary>
    public static string LoadSource(string relativeFilePath)
    {
        var fullPath = Path.Combine(SourceFolder, relativeFilePath);
        
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"Nie odnaleziono pliku źródłowego targetu pod ścieżką: '{fullPath}'");
        }

        return File.ReadAllText(fullPath);
    }

    /// <summary>
    /// Kompiluje wskazany plik źródłowy targetu do Mono.Cecil ModuleDefinition.
    /// </summary>
    public static ModuleDefinition CompileSourceToModule(string relativeFilePath, string assemblyName = "TestAssembly")
    {
        var sourceCode = LoadSource(relativeFilePath);
        return TestAssemblyGenerator.CompileToModule(sourceCode, assemblyName);
    }

    /// <summary>
    /// Pobiera bezpośrednio skompilowane bajty zestawu CilInjecting.TestTargets.dll wygenerowane przez MSBuild.
    /// </summary>
    public static byte[] GetCompiledAssemblyBytes()
    {
        var assemblyPath = typeof(EmptyMethods).Assembly.Location;
        return File.ReadAllBytes(assemblyPath);
    }

    /// <summary>
    /// Wczytuje skompilowany zestaw CilInjecting.TestTargets.dll jako Mono.Cecil ModuleDefinition
    /// bez blokowania pliku na dysku.
    /// </summary>
    public static ModuleDefinition LoadCompiledModule()
    {
        var assemblyPath = typeof(EmptyMethods).Assembly.Location;
        var bytes = File.ReadAllBytes(assemblyPath);
    
        return ModuleDefinition.ReadModule(new MemoryStream(bytes));
    }
}