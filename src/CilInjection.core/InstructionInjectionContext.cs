namespace CilInstructionCounter.Core;

using System.Collections.Generic;
using Mono.Cecil.Cil;

public class InstructionInjectionContext
{
    public Instruction TargetInstruction { get; }
    public List<Instruction> Before { get; } = new();
    public List<Instruction> After { get; } = new();

    public InstructionInjectionContext(Instruction targetInstruction)
    {
        TargetInstruction = targetInstruction;
    }
}