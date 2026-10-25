using CilInjection.Core.Abstractions;

namespace CilInjection.Core.Contexts;
using System.Collections.Generic;
using Mono.Cecil.Cil;

internal interface IInstructionContext
{
    Instruction Instruction { get; }
    OpCode OpCode { get; set; }
    object? Operand { get; set; }
    List<Instruction> Prefixes { get; }

    List<Instruction> GetAllInstructions();
    Instruction GetFirstInstruction();
    Instruction GetLastInstruction();
}

internal class InstructionContext : IInstructionContext, IReadOnlyInstructionContext
{
    public Instruction Instruction { get; }
    public OpCode OpCode { get => Instruction.OpCode; set => Instruction.OpCode = value; }
    public object? Operand { get => Instruction.Operand; set => Instruction.Operand = value; }
    public List<Instruction> Prefixes { get; } = new();
    
    IReadOnlyList<Instruction> IReadOnlyInstructionContext.Prefixes => Prefixes;
    
    public static InstructionContext? Create(Instruction instruction)
    {
        if (IsPrefixInstruction(instruction))
            return null;
        return new InstructionContext(instruction);
    }
    
    public List<Instruction> GetAllInstructions() => [.. Prefixes, Instruction];
    
    public Instruction GetFirstInstruction() => Prefixes.Count > 0 ? Prefixes[0] : Instruction;

    public Instruction GetLastInstruction() => Instruction;
    
    private InstructionContext(Instruction instruction)
    {
        Instruction = instruction;
        ExtractPrefixes();
    }

    private void ExtractPrefixes()
    {
        var current = Instruction.Previous;
        var foundPrefixes = new List<Instruction>();

        while (current != null && IsPrefixInstruction(current))
        {
            foundPrefixes.Add(current);
            current = current.Previous;
        }

        foundPrefixes.Reverse();
        Prefixes.AddRange(foundPrefixes);
    }

    private static bool IsPrefixInstruction(Instruction instruction)
    {
        var code = instruction.OpCode.Code;
        return code is Code.Constrained
            or Code.Readonly
            or Code.Unaligned
            or Code.Volatile
            or Code.Tail;
    }
}