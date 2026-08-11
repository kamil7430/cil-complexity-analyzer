using CilComplexityAnalyzer.TestExecutor.Contract.Results;
using OneOf;

namespace CilComplexityAnalyzer.TestExecutor.Contract;

[GenerateOneOf]
public partial class TestResult : OneOfBase<Success, Failure> { }