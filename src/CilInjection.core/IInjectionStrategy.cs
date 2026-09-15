namespace CilInjection.Core;

using Mono.Cecil;
using System.Runtime.Loader;

public interface IInjectionStrategy
{
    // CIL Weaving: modyfikuje bajty Cecil
    void Inject(ModuleDefinition module);

    // Metoda dostarczająca ścieżkę do właściwego RunTime.dll
    string GetRuntimeAssemblyPath();

    // Fabryka uchwytu dla użytkownika po załadowaniu ALC
    object CreateHandle(AssemblyLoadContext context);
}