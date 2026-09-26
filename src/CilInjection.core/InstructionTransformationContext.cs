namespace CilInstructionCounter.Core;

using System.Collections.Generic;
using Mono.Cecil.Cil;

public class InstructionTransformationContext
{
    // Klasa do implementacji
    
    public Instruction OriginalInstruction { get; }
    public OpCode OpCode { get; set; }
    public object? Operand { get; set; }
    public bool IsRemoved { get; private set; }
    public List<Instruction> AdditionalReplacements { get; } = new();
    
    public InstructionTransformationContext(Instruction originalInstruction)
    {
        OriginalInstruction = originalInstruction;
        OpCode = originalInstruction.OpCode;
        Operand = originalInstruction.Operand;
    }
}