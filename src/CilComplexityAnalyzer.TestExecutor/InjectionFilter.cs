using Mono.Cecil;

namespace CilComplexityAnalyzer.TestExecutor;

public static class InjectionFilter
{
    public static bool ShouldInstrumentType(TypeDefinition type, string assemblyName)
    {
        if (type.Name.StartsWith("<"))
        {
            return false;
        }

        if (assemblyName.Contains("System.Collections"))
        {
            if (type.Namespace == "System.Collections.Generic")
            {
                return type.Name.StartsWith("List") ||
                       type.Name.StartsWith("Dictionary") ||
                       type.Name.StartsWith("HashSet") ||
                       type.Name.StartsWith("LinkedList");
            }
            return false;
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