namespace CilInjecting.Tests.Assertions;

using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.VisualStudio.TestTools.UnitTesting;

public static class InjectionPipelineAssertions
{
    public static Assembly ShouldHaveLoadedAssembly(this AssemblyLoadContext alc, string assemblyName)
    {
        var assembly = alc.Assemblies.FirstOrDefault(a => a.GetName().Name == assemblyName);
        
        Assert.IsNotNull(
            assembly, 
            $"Biblioteka '{assemblyName}' nie została załadowana do kontekstu '{alc.Name}'.");

        var actualContext = AssemblyLoadContext.GetLoadContext(assembly);
        Assert.AreSame(
            alc, 
            actualContext, 
            $"Biblioteka '{assemblyName}' jest załadowana, ale w innym AssemblyLoadContext.");

        return assembly;
    }
}