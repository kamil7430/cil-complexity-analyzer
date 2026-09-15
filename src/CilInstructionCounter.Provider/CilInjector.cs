namespace CilInstructionCounter;

using System.Runtime.Loader;
using CilInjection.Core;
using CilInjection.Core.Extensions;
using CilInjection.Core.Weaver;
using RunTime;
using Mono.Cecil;
using Mono.Cecil.Cil;

internal static class CilInjector
{
    public static void Inject(this ModuleDefinition module)
    {
        // Rejestrujemy referencję do zewnętrznego pola w InstructionCounter.RunTime.dll
        var counterFieldRef = ImportRuntimeCounterField(module);

        // Przechodzimy po typach i metodach
        foreach (var type in module.GetAllTypesRecursively())
        {
            if (type.IsInterface) continue;

            foreach (var method in type.Methods)
            {
                if (!method.HasBody) continue;

                // Wywołujemy bezpieczny silnik CIL przekazując tylko opkody podbijające licznik
                method.InjectAtInstructionLevel((il, targetInstr) => new[]
                {
                    il.Create(OpCodes.Ldsfld, counterFieldRef),
                    il.Create(OpCodes.Ldc_I8, 1L),
                    il.Create(OpCodes.Add),
                    il.Create(OpCodes.Stsfld, counterFieldRef)
                });
            }
        }
    }
    
    private static FieldReference ImportRuntimeCounterField(ModuleDefinition targetModule)
    {
        var runtimeAssemblyName = typeof(GlobalCounterContainer).Assembly.GetName();

        var assemblyRef = new AssemblyNameReference(
            runtimeAssemblyName.Name!,
            runtimeAssemblyName.Version);

        var typeRef = new TypeReference(
            @namespace: typeof(GlobalCounterContainer).Namespace!,
            name: nameof(GlobalCounterContainer),
            module: targetModule,
            scope: assemblyRef);

        var fieldRef = new FieldReference(
            name: nameof(GlobalCounterContainer.InstructionCounter),
            fieldType: targetModule.TypeSystem.Int64,
            declaringType: typeRef);

        return targetModule.ImportReference(fieldRef);
    }
}