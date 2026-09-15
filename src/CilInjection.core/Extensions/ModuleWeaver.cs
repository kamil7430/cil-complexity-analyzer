namespace CilInjection.Core.Extensions;

using Mono.Cecil;

public static class ModuleWeaver
{
    /// <summary>
    /// Zwraca wszystkie typy w module, włącznie z typami zagnieżdżonymi (lambdy, async/await state machines).
    /// </summary>
    public static IEnumerable<TypeDefinition> GetAllTypesRecursively(this ModuleDefinition module)
    {
        foreach (var type in module.Types)
        {
            yield return type;

            foreach (var nested in GetNestedTypesRecursively(type))
                yield return nested;
        }
    }

    private static IEnumerable<TypeDefinition> GetNestedTypesRecursively(TypeDefinition type)
    {
        foreach (var nested in type.NestedTypes)
        {
            yield return nested;

            foreach (var deeper in GetNestedTypesRecursively(nested))
                yield return deeper;
        }
    }
}