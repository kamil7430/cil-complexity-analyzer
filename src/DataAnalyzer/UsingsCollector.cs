using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DataAnalyzer;

public class UsingsCollector : CSharpSyntaxWalker
{
    private string? _fileName;
    public Dictionary<string, List<string>> Usings { get; } = [];

    public void Visit(SyntaxNode? node, string fileName)
    {
        _fileName = fileName;
        base.Visit(node);
        _fileName = null;
    }

    public override void VisitUsingDirective(UsingDirectiveSyntax node)
    {
        var name = node.NamespaceOrType.ToString();
        Usings.TryAdd(name, []);
        Usings[name].Add(_fileName!);
    }

    public void PrintUsings(bool shouldPrintFileList)
    {
        foreach (var (name, list) in Usings)
        {
            Console.WriteLine($"{name}: {list.Count}");
            if (shouldPrintFileList)
            {
                Console.Write("> ");
                foreach (var fileName in list)
                    Console.Write($"{fileName} ");
                Console.WriteLine();
            }
        }
    }
}