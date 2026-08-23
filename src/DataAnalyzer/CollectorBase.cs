using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace DataAnalyzer;

public abstract class CollectorBase : CSharpSyntaxWalker
{
    protected string? FileName;
    protected SemanticModel? SemanticModel;
    public Dictionary<string, List<string>> Occurrences { get; } = [];
    
    public void Visit(SyntaxNode? node, string fileName, SemanticModel model)
    {
        FileName = fileName;
        SemanticModel = model;
        base.Visit(node);
        SemanticModel = null;
        FileName = null;
    }

    public void PrintOccurrences(bool shouldPrintFileList)
    {
        foreach (var (name, list) in Occurrences)
        {
            Console.WriteLine($"{name}: {list.Count}");
            if (!shouldPrintFileList) 
                continue;
            
            Console.Write("> ");
            foreach (var fileName in list)
                Console.Write($"{fileName} ");
            Console.WriteLine();
        }
    }
}