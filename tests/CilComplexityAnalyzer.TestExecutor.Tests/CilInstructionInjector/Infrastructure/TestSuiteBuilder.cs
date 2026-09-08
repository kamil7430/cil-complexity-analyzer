using CilComplexityAnalyzer.TestExecutor.Contract;

namespace CilComplexityAnalyzer.TestExecutor.Tests.CilInstructionInjector.Infrastructure;

internal class CilTestSuiteBuilder
{
    private string _studentCode = "public class StudentSolution { public void Execute() {} }";
    private bool _hasAssemblyBytes = true;

    public static CilTestSuiteBuilder Create() => new();

    public CilTestSuiteBuilder WithStudentCode(string sourceCode)
    {
        _studentCode = sourceCode;
        _hasAssemblyBytes = true;
        return this;
    }

    public CilTestSuiteBuilder WithoutStudentAssembly()
    {
        _hasAssemblyBytes = false;
        return this;
    }

    public TestSuite Build()
    {
        byte[]? assemblyBytes = _hasAssemblyBytes 
            ? InMemoryCompiler.Compile(_studentCode) 
            : null;

        return new CilInjectingTestSuite
        {
            StudentSolutionAssemblyBytes = assemblyBytes
        };
    }

    private class CilInjectingTestSuite : TestSuite
    {
        public override string StudentSolutionSourceCode() => "";
        public override string TestSuiteSourceCode() => "";
    }
}