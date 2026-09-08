namespace CilComplexityAnalyzer.Contract.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class StudentSolutionAttribute(Type studentSolutionType) : Attribute
{
    public Type StudentSolutionType { get; } = studentSolutionType
       ?? throw new ArgumentException(nameof(studentSolutionType));
}