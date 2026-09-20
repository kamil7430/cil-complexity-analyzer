namespace CilInjection.Core.Extensions;

using System.IO;
using System.Reflection;
using System.Runtime.Loader;

public static class AssemblyLoadContextFromType
{
    /// <summary>
    /// Ładuje bibliotekę RunTime zawierającą podany typ bezpośrednio do piaskownicy.
    /// </summary>
    public static Assembly LoadRuntimeFromType(this AssemblyLoadContext context, Type markerType)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(markerType);

        string runtimePath = markerType.Assembly.Location;

        if (string.IsNullOrEmpty(runtimePath) || !File.Exists(runtimePath))
        {
            throw new FileNotFoundException(
                $"Nie odnaleziono pliku biblioteki RunTime dla typu '{markerType.FullName}' pod adresem: {runtimePath}");
        }

        using var stream = File.OpenRead(runtimePath);
        return context.LoadFromStream(stream);
    }
}