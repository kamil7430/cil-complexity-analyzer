namespace CilInjecting.Tests.Infrastructure.Fixtures;

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Loader;
using CilInjection.Core;
using CilInjecting.Tests.Infrastructure.Compilers;
using CilInjecting.Tests.Infrastructure.Fakes;

/// <summary>
/// Środowisko pomocnicze dla testów silnika InjectionPipeline.
/// Odpowiada za cykl życia AssemblyLoadContext, dynamiczną kompilację bibliotek C# oraz automatyczne czyszczenie plików .dll z dysku.
/// </summary>
public sealed class PipelineTestEnvironment : IDisposable
{
    private readonly List<string> _tempFilePaths = new();
    private bool _disposed;

    public AssemblyLoadContext Alc { get; }

    public PipelineTestEnvironment(string contextName = "PipelineTestALC")
    {
        Alc = new AssemblyLoadContext($"{contextName}_{Guid.NewGuid():N}", isCollectible: true);
    }

    /// <summary>
    /// Kompiluje kod C# do pliku tymczasowego na dysku i rejestruje go do automatycznego usunięcia po zakończeniu testu.
    /// </summary>
    public Type CreateDynamicRuntime(string sourceCode, string assemblyName, string typeName)
    {
        var (markerType, filePath) = TestAssemblyGenerator.CompileToTempDll(sourceCode, assemblyName, typeName);
        _tempFilePaths.Add(filePath);
        return markerType;
    }

    /// <summary>
    /// Generuje prosty, domyślny runtime C# gdy test wymaga jedynie sprawnej biblioteki .dll bez własnej logiki.
    /// </summary>
    public Type CreateDefaultDynamicRuntime(string? assemblyName = null, string? typeName = null)
    {
        var (markerType, filePath) = TestAssemblyGenerator.CreateDefaultTempDll(assemblyName, typeName);
        _tempFilePaths.Add(filePath);
        return markerType;
    }
    
    /// <summary>
    /// Kompiluje podany kod C# wyłącznie w pamięci RAM i zwraca bajty wygenerowanej biblioteki .dll.
    /// </summary>
    public byte[] CreateAssemblyBytes(string sourceCode, string assemblyName = "DynamicTestAssembly")
    {
        return TestAssemblyGenerator.CompileToBytes(sourceCode, assemblyName);
    }

    /// <summary>
    /// Generuje domyślne bajty biblioteki .dll z prostą klasą kontenera bezpośrednio w pamięci RAM.
    /// </summary>
    public byte[] CreateDefaultAssemblyBytes(string? assemblyName = null)
    {
        return TestAssemblyGenerator.CreateDefaultBytes(assemblyName);
    }

    /// <summary>
    /// Tworzy atrape strategii opartą o podany typ markerowy.
    /// </summary>
    public FakeInjectionStrategy CreateStrategy(Type markerType)
    {
        return new FakeInjectionStrategy(markerType);
    }

    /// <summary>
    /// Tworzy instancję InjectionPipeline ze wskazanymi strategiami.
    /// </summary>
    public InjectionPipeline CreatePipeline(params IInjectionStrategy[] strategies)
    {
        return new InjectionPipeline(strategies);
    }

    public void Dispose()
    {
        if (_disposed) return;

        Alc.Unload();

        foreach (var filePath in _tempFilePaths)
        {
            if (File.Exists(filePath))
            {
                try
                {
                    File.Delete(filePath);
                }
                catch
                {
                    // ewentualna blokady I/O systemu operacyjnego
                }
            }
        }

        _disposed = true;
    }
}