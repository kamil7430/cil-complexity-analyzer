namespace CilComplexityAnalyzer.CilInjection;

internal static class InstrumentationContract
{
    public static class Symbols
    {
        public const string ContainerTypeName = "<GlobalCounterContainer>";
        public const string CounterFieldName = "__InstructionCounter";
        public const string GetCounterMethodName = "GetInstructionCount";
        public const string ResetCounterMethodName = "ResetInstructionCount";
    }
}