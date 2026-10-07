using CilInjection.Core.Abstractions;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace CilInjection.Tests.Infrastructure.Fakes;

public class FakeMetadataContext : IMetadataContext
{
    public TypeReference ImportType(TypeReference typeReference) => typeReference;
    public FieldReference ImportField(FieldReference fieldReference) => fieldReference;
    public MethodReference ImportMethod(MethodReference methodReference) => methodReference;
}

public class FakeReadOnlyInstructionContext : IReadOnlyInstructionContext
{
    public OpCode OpCode { get; set; } = OpCodes.Nop;
    public object? Operand { get; set; }
    public IReadOnlyList<Instruction> Prefixes { get; set; } = [];
}

public class FakeInstructionInjectionContext : IInstructionInjectionContext
{
    private readonly List<Instruction> _beforeInstructions = [];
    private readonly List<Instruction> _afterInstructions = [];

    public IReadOnlyInstructionContext Target { get; set; } = new FakeReadOnlyInstructionContext();
    
    public IReadOnlyList<Instruction> BeforeInstructions => _beforeInstructions;
    public IReadOnlyList<Instruction> AfterInstructions => _afterInstructions;

    public void AddBefore(IEnumerable<Instruction> instructions)
    {
        _beforeInstructions.AddRange(instructions);
    }

    public void AddAfter(IEnumerable<Instruction> instructions)
    {
        _afterInstructions.AddRange(instructions);
    }
}

public class FakeMethodInjectionContext : IMethodInjectionContext
{
    private readonly List<FakeInstructionInjectionContext> _contexts = [];

    public IReadOnlyList<IInstructionInjectionContext> Contexts => _contexts;

    public FakeInstructionInjectionContext AddContext(OpCode opCode, object? operand = null)
    {
        var context = new FakeInstructionInjectionContext
        {
            Target = new FakeReadOnlyInstructionContext
            {
                OpCode = opCode,
                Operand = operand
            }
        };
        _contexts.Add(context);
        return context;
    }
}

public class FakeMethodTransformationContext : IMethodTransformationContext { }