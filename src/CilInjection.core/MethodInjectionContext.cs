namespace CilInstructionCounter.Core;

using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

public class MethodInjectionContext
{
    public MethodDefinition Method { get; }
    private readonly Dictionary<Instruction, InstructionInjectionContext> _contexts;

    public IReadOnlyList<InstructionInjectionContext> Contexts { get; }

    public MethodInjectionContext(MethodDefinition method, IEnumerable<Instruction> validInstructions)
    {
        Method = method;
        _contexts = validInstructions
            .ToDictionary(inst => inst, inst => new InstructionInjectionContext(inst));

        Contexts = _contexts.Values.ToList();
    }

    public InstructionInjectionContext For(Instruction instruction) => _contexts[instruction];

    public bool HasContextFor(Instruction instruction) => _contexts.ContainsKey(instruction);
}