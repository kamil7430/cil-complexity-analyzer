using CilInjection.Core.Contexts;

namespace CilInjection.Core.Abstractions;

using System.Collections.Generic;
using Mono.Cecil;
using Mono.Cecil.Cil;

public interface IReadOnlyInstructionContext
{
    OpCode OpCode { get; }
    object? Operand { get; }
    IReadOnlyList<Instruction> Prefixes { get; }
}

public interface IInstructionInjectionContext
{
    IReadOnlyInstructionContext Target { get; }
    void AddBefore(IEnumerable<Instruction> instructions);
    void AddAfter(IEnumerable<Instruction> instructions);
}

public interface IMethodInjectionContext
{
    IReadOnlyList<IInstructionInjectionContext> Contexts { get; }
}

public interface IInstructionTransformationContext
{
    Instruction OriginalInstruction { get; }
    OpCode OpCode { get; set; }
    object? Operand { get; set; }
    bool IsRemoved { get; }
    IReadOnlyList<Instruction>? ReplacementInstructions { get; }

    void Remove();
    void ReplaceWith(IEnumerable<Instruction> instructions);
    void ReplaceWith(params Instruction[] instructions);
}

public interface IMethodTransformationContext
{
    MethodDefinition Method { get; }
    IReadOnlyList<IInstructionTransformationContext> Mutations { get; }
    
    IInstructionTransformationContext For(Instruction instruction);
}