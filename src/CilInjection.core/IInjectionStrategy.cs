namespace CilInjection.Core;

using Mono.Cecil;
using System.Runtime.Loader;

public interface IInjectionStrategy
{
    // CIL Weaving: modyfikuje bajty Cecil
    void Inject(ModuleDefinition module);

    // Ładowanie wymaganej biblioteki RunTime do piaskownicy
    void LoadRuntime(System.Runtime.Loader.AssemblyLoadContext context);

    // Fabryka uchwytu dla użytkownika po załadowaniu ALC
    object CreateHandle(System.Runtime.Loader.AssemblyLoadContext context);
}