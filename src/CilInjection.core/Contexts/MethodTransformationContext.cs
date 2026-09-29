namespace CilInjection.Core.Contexts;

using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

internal class MethodTransformationContext
{
    private MethodDefinition Method { get; }
    private readonly Dictionary<Instruction, InstructionTransformationContext> _mutations;

    public IReadOnlyList<InstructionTransformationContext> Mutations { get; }

    public MethodTransformationContext(MethodDefinition method)
    {
        Method = method;
        _mutations = method.Body.Instructions
            .ToDictionary(inst => inst, inst => new InstructionTransformationContext());
            
        Mutations = _mutations.Values.ToList();
    }
}