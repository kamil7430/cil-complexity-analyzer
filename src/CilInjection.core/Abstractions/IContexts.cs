using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace CilInjection.Core.Abstractions;

public interface IMetadataContext
{
    TypeReference ImportType(Type type);
    FieldReference ImportField(FieldInfo fieldInfo);
    FieldReference ImportField(Type declaringType, string fieldName);
    MethodReference ImportMethod(MethodBase methodBase);
}

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