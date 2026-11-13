using Mono.Cecil;

namespace CilComplexityAnalyzer.TestExecutor;

/// <summary>
/// Odkrywa, jakich jeszcze typów (w TYM SAMYM module) używają typy startowe.
/// Zwraca wyłącznie nazwy typów NAJWYŻSZEGO poziomu (typy zagnieżdżone są analizowane
/// razem ze swoim typem głównym, np. List`1/Enumerator).
/// </summary>
public static class TypeDependencyCollector
{
    public static HashSet<string> DiscoverDependentTypes(
        ModuleDefinition module,
        IEnumerable<string> rootFullNames,
        int maxDepth,
        Func<TypeDefinition, bool> isAllowed)
    {
        var result = new HashSet<string>();
        var queue = new Queue<(TypeDefinition Type, int Depth)>();

        foreach (var name in rootFullNames)
        {
            var root = module.GetType(name);
            if (root != null && result.Add(root.FullName))
                queue.Enqueue((root, 0));
        }

        while (queue.Count > 0)
        {
            var (type, depth) = queue.Dequeue();
            if (depth >= maxDepth) continue;

            foreach (var dependency in EnumerateDependencies(type))
            {
                var top = GetTopLevelType(dependency);
                if (!isAllowed(top)) continue;

                if (result.Add(top.FullName))
                    queue.Enqueue((top, depth + 1));
            }
        }

        return result;
    }

    private static IEnumerable<TypeDefinition> EnumerateDependencies(TypeDefinition topLevel)
    {
        foreach (var t in SelfAndNested(topLevel))
        {
            foreach (var method in t.Methods)
            {
                if (!method.HasBody) continue;

                foreach (var instruction in method.Body.Instructions)
                {
                    TypeReference? typeRef = instruction.Operand switch
                    {
                        MethodReference m => m.DeclaringType,
                        FieldReference f  => f.DeclaringType,
                        TypeReference tr  => tr,
                        _ => null
                    };

                    if (typeRef?.GetElementType() is TypeDefinition def)
                        yield return def;
                }
            }
        }
    }

    private static IEnumerable<TypeDefinition> SelfAndNested(TypeDefinition type)
    {
        yield return type;
        foreach (var nested in type.NestedTypes)
            foreach (var inner in SelfAndNested(nested))
                yield return inner;
    }

    public static TypeDefinition GetTopLevelType(TypeDefinition type)
    {
        var current = type;
        while (current.DeclaringType != null)
            current = current.DeclaringType;
        return current;
    }
}