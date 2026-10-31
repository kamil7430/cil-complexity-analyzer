using Mono.Cecil;

namespace CilComplexityAnalyzer.TestExecutor;

public static class InjectionFilter
{
    public static HashSet<string> AllowedTypeFullNames { get; set; } = new();
    public static bool ShouldInstrumentTypeByHand(TypeDefinition type, string assemblyName)
    {
        if (type.Name.StartsWith("<"))
        {
            return false;
        }
        

        if (assemblyName.Contains("System.Private.CoreLib") || assemblyName.Contains("System.Collections"))
        {
            if (type.Namespace == "System.Collections.Generic")
            {
                return type.Name.StartsWith("List") ||
                       type.Name.StartsWith("Dictionary") ||
                       type.Name.StartsWith("HashSet") ||
                       type.Name.StartsWith("LinkedList") ||
                       type.Name.StartsWith("EqualityComparer");
            }
            return false;
        }

        if (assemblyName.Contains("System.Linq"))
        {
            return type.Name == "Enumerable" || type.Name.Contains("Enumerable");
        }

        return true;
    }

    public static bool ShouldInstrumentType(TypeDefinition type, string assemblyName)
    {
        if (type.Name.StartsWith("<") || type.IsInterface)
            return false;

        string effectiveNs = TypeDependencyCollector.GetEffectiveNamespace(type);

        if (assemblyName.Contains("System.Private.CoreLib"))
        {
            if (effectiveNs != "System.Collections.Generic" && effectiveNs != "System.Collections.ObjectModel")
            {
                return false;
            }

            if (AllowedTypeFullNames.Count > 0)
            {
                return AllowedTypeFullNames.Contains(type.FullName);
            }

            return true;
        }

        if (assemblyName.Contains("System.Collections"))
        {
            if (AllowedTypeFullNames.Count > 0)
            {
                return AllowedTypeFullNames.Contains(type.FullName) || effectiveNs == "System.Collections.Generic";
            }
            return effectiveNs == "System.Collections.Generic";
        }

        if (assemblyName.Contains("System.Linq"))
        {
            return type.Name == "Enumerable" || type.Name.Contains("Enumerable");
        }

        return true;
    }
    
    public static bool ShouldInstrumentMethod(MethodDefinition method)
    {
        if (!method.HasBody) return false;
        if (method.IsConstructor && method.IsStatic) return false;
        return true;
    }
}