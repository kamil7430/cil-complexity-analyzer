namespace CilInstructionCounter.Core;

using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

public class MethodTransformationContext
{
    public MethodDefinition Method { get; }
    private readonly Dictionary<Instruction, InstructionTransformationContext> _mutations;

    public IReadOnlyList<InstructionTransformationContext> Mutations { get; }

    public MethodTransformationContext(MethodDefinition method)
    {
        Method = method;
        _mutations = method.Body.Instructions
            .ToDictionary(inst => inst, inst => new InstructionTransformationContext(inst));
            
        Mutations = _mutations.Values.ToList();
    }

    public InstructionTransformationContext For(Instruction instruction) => _mutations[instruction];
}