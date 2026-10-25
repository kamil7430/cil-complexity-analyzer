namespace CilInjecting.Tests.Infrastructure.Extensions;

using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using Mono.Cecil;

public static class ModuleTestExtensions
{
    /// <summary>
    /// Zapisuje zmodyfikowany ModuleDefinition do tablicy bajtów.
    /// </summary>
    public static byte[] ToBytes(this ModuleDefinition module)
    {
        using var stream = new MemoryStream();
        module.Write(stream);
        return stream.ToArray();
    }

    /// <summary>
    /// Ładuje zmodyfikowany ModuleDefinition bezpośrednio do domyślnego AssemblyLoadContext.
    /// </summary>
    public static Assembly ToAssembly(this ModuleDefinition module)
    {
        return Assembly.Load(module.ToBytes());
    }

    /// <summary>
    /// Ładuje zmodyfikowany ModuleDefinition do wskazanego AssemblyLoadContext.
    /// </summary>
    public static Assembly ToAssembly(this ModuleDefinition module, AssemblyLoadContext context)
    {
        using var stream = new MemoryStream(module.ToBytes());
        return context.LoadFromStream(stream);
    }
}