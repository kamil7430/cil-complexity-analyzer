using CilComplexityAnalyzer.Contract.Results;
using OneOf;

namespace CilComplexityAnalyzer.Contract;

[GenerateOneOf]
public partial class TestResult : OneOfBase<Success, Failure>;