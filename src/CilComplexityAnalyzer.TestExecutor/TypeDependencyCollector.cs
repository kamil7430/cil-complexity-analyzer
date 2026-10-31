using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace CilComplexityAnalyzer.TestExecutor;

public static class TypeDependencyCollector
{
    public static HashSet<string> DiscoverDependentTypes(ModuleDefinition module, string rootTypeName, int maxDepth = 3)
    {
        var discoveredTypes = new HashSet<string>();
        var rootType = module.Types.FirstOrDefault(t => 
            t.Name == rootTypeName || 
            t.FullName == rootTypeName ||
            t.Name.StartsWith(rootTypeName)
            );
        if (rootType == null)
        {
            return discoveredTypes;
        }

        TraverseTypeDependencies(rootType, discoveredTypes, maxDepth, 0);
        return discoveredTypes;
    }

    private static void TraverseTypeDependencies(TypeDefinition type, HashSet<string> visitedTypes, int maxDepth, int currentDepth)
    {
        if (currentDepth > maxDepth || type == null || !visitedTypes.Add(type.FullName))
            return;
        
        string effectiveNs = GetEffectiveNamespace(type);
        if (type.Module.Name.Contains("System.Private.CoreLib"))
        {
            if (effectiveNs != "System.Collections.Generic" && effectiveNs != "System.Collections.ObjectModel")
            {
                return;
            }
        }

        if (type.Module.Name.Contains("System.Private.CoreLib"))
        {
            if (type.Namespace != "System.Collections.Generic" && type.Namespace != "System.Collections.ObjectModel")
            {
                return;
            }
        }

        visitedTypes.Add(type.Name);

        foreach (var method in type.Methods)
        {
            if (!method.HasBody) continue;
            foreach (var instruction in method.Body.Instructions)
            {
                if (instruction.Operand is MethodReference methodRef)
                {
                    var declaringTypeRef = methodRef.DeclaringType;
                    if (declaringTypeRef == null) continue;

                    TypeDefinition resolvedType = declaringTypeRef.Resolve();

                    if (resolvedType != null && resolvedType.Module == type.Module)
                    {
                        TraverseTypeDependencies(resolvedType, visitedTypes, maxDepth, currentDepth + 1);
                    }
                }
            }
        }
    }
    
    public static string GetEffectiveNamespace(TypeDefinition type)
    {
        var current = type;
        while (current.IsNested && current.DeclaringType != null)
        {
            current = current.DeclaringType;
        }
        return current.Namespace ?? "";
    }
}