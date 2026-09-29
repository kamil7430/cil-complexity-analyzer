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

public interface IInstructionTransformationContext { }

public interface IMethodTransformationContext { }