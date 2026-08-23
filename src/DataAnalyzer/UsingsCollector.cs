using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DataAnalyzer;

public class UsingsCollector : CollectorBase
{
    public override void VisitUsingDirective(UsingDirectiveSyntax node)
    {
        var name = node.NamespaceOrType.ToString();
        Occurrences.TryAdd(name, []);
        Occurrences[name].Add(FileName!);
    }
}