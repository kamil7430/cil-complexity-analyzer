namespace CilComplexityAnalyzer.Contract.SuiteAttributes;

[AttributeUsage(AttributeTargets.Class)]
public class StudentSolutionAttribute(Type studentSolutionType) : Attribute
{
    public Type StudentSolutionType { get; } = studentSolutionType;
}