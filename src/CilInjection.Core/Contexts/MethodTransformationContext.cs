using CilInjection.Core.Abstractions;

namespace CilInjection.Core.Contexts;

using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

internal class MethodTransformationContext : IMethodTransformationContext
{
    private readonly Dictionary<Instruction, InstructionTransformationContext> _mutations;

    public IReadOnlyList<InstructionTransformationContext> Mutations { get; }

    public MethodTransformationContext(MethodDefinition method)
    {
        _mutations = method.Body.Instructions
            .ToDictionary(inst => inst, inst => new InstructionTransformationContext());
            
        Mutations = _mutations.Values.ToList();
    }
}