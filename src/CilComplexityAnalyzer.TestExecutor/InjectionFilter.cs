using Mono.Cecil;

namespace CilComplexityAnalyzer.TestExecutor;

/// <summary>
/// Decyduje, które typy/metody dostają licznik. Tworzony osobno dla każdego assembly
/// (stara wersja trzymała wyniki w statycznym AllowedTypeFullNames, który był
/// nadpisywany przy każdym assembly).
/// </summary>
public sealed class InjectionFilter
{
    private enum Mode { CoreLib, Collections, Linq, Deny, Allow }

    private static readonly string[] DefaultCoreLibRoots =
    {
        "System.Collections.Generic.List`1",
        "System.Collections.Generic.Dictionary`2",
        "System.Collections.Generic.HashSet`1",
        "System.Collections.Generic.Queue`1",
        "System.Collections.Generic.LinkedList`1",
    };

    private readonly Mode _mode;
    private readonly HashSet<string> _allowedRoots;

    private InjectionFilter(Mode mode, HashSet<string>? allowedRoots = null)
    {
        _mode = mode;
        _allowedRoots = allowedRoots ?? new HashSet<string>();
    }

    public static bool IsCollectionsNamespace(TypeDefinition topLevel) =>
        topLevel.Namespace == "System.Collections.Generic" ||
        topLevel.Namespace == "System.Collections.ObjectModel";

    public static InjectionFilter Create(
        string assemblyName,
        ModuleDefinition module,
        IEnumerable<string>? extraRoots = null,
        int maxDepth = 2,
        bool wholeAssembly = false
        )
    {
        if (wholeAssembly)
            return new InjectionFilter(Mode.Allow);
        if (assemblyName == "System.Private.CoreLib")
        {
            var roots = DefaultCoreLibRoots.Concat(extraRoots ?? Array.Empty<string>()).ToList();
            var allowed = TypeDependencyCollector.DiscoverDependentTypes(module, roots, maxDepth, IsCollectionsNamespace);
            Console.WriteLine($"[DISCOVERY] CoreLib: {allowed.Count} typow najwyzszego poziomu do instrumentacji");
            return new InjectionFilter(Mode.CoreLib, allowed);
        }

        if (assemblyName == "System.Collections") return new InjectionFilter(Mode.Collections);
        if (assemblyName == "System.Linq")        return new InjectionFilter(Mode.Linq);

        if (assemblyName.StartsWith("System.") || assemblyName.StartsWith("Microsoft.") ||
            assemblyName is "mscorlib" or "netstandard" or "WindowsBase")
            return new InjectionFilter(Mode.Deny);

        return new InjectionFilter(Mode.Allow);
    }

    public bool ShouldInstrumentType(TypeDefinition type)
    {
        // Pomijamy tylko <Module> i sam licznik.
        if (type.Name == "<Module>" || type.FullName == CounterNames.FullName || type.IsInterface)
            return false;

        var top = TypeDependencyCollector.GetTopLevelType(type);
        var ns = top.Namespace ?? "";

        return _mode switch
        {
            Mode.CoreLib     => _allowedRoots.Contains(top.FullName),
            Mode.Collections => ns.StartsWith("System.Collections"),
            Mode.Linq        => ns.StartsWith("System.Linq"),
            Mode.Allow       => true,
            _                => false,
        };
    }

    public bool ShouldInstrumentMethod(MethodDefinition method)
    {
        if (!method.HasBody) return false;
        if (method.IsConstructor && method.IsStatic) return false;   // .cctor - zostawiamy w spokoju
        return true;
    }
}