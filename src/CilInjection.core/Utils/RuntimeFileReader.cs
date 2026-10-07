namespace CilInjection.Core.Utils;

using System;
using System.IO;

internal static class RuntimeFileReader
{
    /// <summary>
    /// Odczytuje bajty pliku DLL z podanej, pełnej ścieżki.
    /// </summary>
    public static byte[] ReadDllBytesFromPath(string dllPath)
    {
        ArgumentException.ThrowIfNullOrEmpty(dllPath);
        
        if (!File.Exists(dllPath))
        {
            throw new FileNotFoundException($"Nie znaleziono pliku runtime DLL pod ścieżką: {dllPath}");
        }
        
        return File.ReadAllBytes(dllPath);
    }

    /// <summary>
    /// Odczytuje bajty pliku DLL na podstawie samej nazwy pliku, 
    /// lokalizując go automatycznie w katalogu bazowym aplikacji (AppContext.BaseDirectory).
    /// </summary>
    public static byte[] ReadDllBytesFromBasePath(string dllFileName)
    {
        ArgumentException.ThrowIfNullOrEmpty(dllFileName);

        var fullPath = Path.Combine(AppContext.BaseDirectory, dllFileName);
        
        return ReadDllBytesFromPath(fullPath);
    }
}