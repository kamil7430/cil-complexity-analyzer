using CilInjection.Core.Abstractions;

namespace CilInjection.Core.Contexts;

using System.Collections.Generic;
using Mono.Cecil.Cil;

internal class InstructionInjectionContext : IInstructionInjectionContext
{
    private InstructionContext _instructionContext;
    
    public InstructionContext Original => _instructionContext;
    public List<Instruction> Before { get; } = new();
    public List<Instruction> After { get; } = new();

    public IReadOnlyInstructionContext Target => _instructionContext;
    public void AddBefore(IEnumerable<Instruction> instructions) => Before.AddRange(instructions);
    public void AddAfter(IEnumerable<Instruction> instructions) => After.AddRange(instructions);
    
    public static InstructionInjectionContext? Create(Instruction instruction)
    {
        var instructionContext = InstructionContext.Create(instruction);
        return instructionContext == null ? null : new InstructionInjectionContext(instructionContext);
    }
    
    public List<Instruction> GetAfterReversed()
    {
        var reversed = new List<Instruction>(After);
        reversed.Reverse();
        return reversed;
    }

    public Instruction GetFirstInstruction()
    {
        return Before.Count > 0 ? Before[0] : _instructionContext.GetFirstInstruction();
    }

    private InstructionInjectionContext(InstructionContext instructionContext)
    {
        _instructionContext = instructionContext;
    }
}