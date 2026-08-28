using System;

namespace CilComplexityAnalyzer.TestGenerator.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class CaseSettingsAttribute : Attribute
{
    public long InstructionCap { get; set; } = 1_000_000;
    public long TimeoutMs { get; set; } = 30_000;
    public int MemoryMb { get; set; } = 512;
    public CaseSettingsAttribute() { }
    public CaseSettingsAttribute(long instructionCap) => InstructionCap = instructionCap;
}