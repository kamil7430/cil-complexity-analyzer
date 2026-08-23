using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DataAnalyzer;

public class MethodsCollector : CollectorBase
{
    public override void VisitInvocationExpression(InvocationExpressionSyntax node)
    {
        // Console.WriteLine(node);
    }
}