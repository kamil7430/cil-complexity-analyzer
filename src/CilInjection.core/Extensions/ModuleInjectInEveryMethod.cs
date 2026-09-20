namespace CilInjection.Core.Extensions;

using System;
using System.Collections.Generic;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CilInjection.Core.Weaver;

public static class ModuleInjectInEveryMethod
{
    /// <summary>
    /// Przechodzi po wszystkich typach i metodach w module i wstrzykuje instrukcje na poziomie CIL.
    /// </summary>
    public static void InjectInEveryMethodAtInstructionLevel(
        this ModuleDefinition module,
        Func<ILProcessor, Instruction, IEnumerable<Instruction>> instructionFactory)
    {
        ArgumentNullException.ThrowIfNull(module);

        foreach (var type in module.GetAllTypesRecursively())
        {
            if (type.IsInterface) continue;

            foreach (var method in type.Methods)
            {
                if (!method.HasBody) continue;

                method.InjectAtInstructionLevel(instructionFactory);
            }
        }
    }
}