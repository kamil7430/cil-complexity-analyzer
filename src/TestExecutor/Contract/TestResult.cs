using OneOf;
using TestExecutor.Contract.Results;

namespace TestExecutor.Contract;

[GenerateOneOf]
public partial class TestResult : OneOfBase<Success, Failure> { }