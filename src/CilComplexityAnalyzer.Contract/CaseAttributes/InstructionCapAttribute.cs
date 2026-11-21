namespace CilComplexityAnalyzer.Contract.CaseAttributes;

[AttributeUsage(AttributeTargets.Class)]
public class InstructionCapAttribute(long instructionCap) : Attribute
{
    public long InstructionCap { get; } = instructionCap;
}