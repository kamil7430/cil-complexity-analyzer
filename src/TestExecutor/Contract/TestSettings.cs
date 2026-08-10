using TestExecutor.Contract.Settings;

namespace TestExecutor.Contract;

public class TestSettings
{
    public ComplexityCalculationMethod ComplexityCalculationMethod { get; set; }
    public bool ShouldCaptureProgramOutput { get; set; }
}