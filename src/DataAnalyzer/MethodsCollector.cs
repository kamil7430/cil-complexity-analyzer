using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DataAnalyzer;

public class MethodsCollector : CollectorBase
{
    public override void VisitInvocationExpression(InvocationExpressionSyntax node)
    {
        var symbol = SemanticModel!.GetSymbolInfo(node.Expression).Symbol;
        
        if (symbol is null)
            Console.WriteLine($"\tNot bound: {node.Expression.ToString()}");
        else
        {
            var name = symbol.ToDisplayString();
            Occurrences.TryAdd(name, []);
            Occurrences[name].Add(FileName!);
        }
    }
}