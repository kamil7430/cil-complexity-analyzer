using System;

namespace CilComplexityAnalyzer.TestGenerator.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class CaseSettingsAttribute(
    long instructionCap = 1_000_000,
    long timeoutMs = 30_000,
    int memoryMb = 512)
    : Attribute
{
    public long InstructionCap { get; set; } = instructionCap;
    public long TimeoutMs { get; set; } = timeoutMs;
    public int MemoryMb { get; set; } = memoryMb;
}