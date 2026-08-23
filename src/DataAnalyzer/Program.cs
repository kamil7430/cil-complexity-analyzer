using System.Diagnostics;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

string[] argsList = ["solutions_dir"];
string[] argsDescription = ["directory of zip-compressed solutions"];
Debug.Assert(argsList.Length == argsDescription.Length);

if (args.Length != argsList.Length) 
    Usage();

const string zipRegex = "^.+\\.zip$";
var zipsToOpen = Directory.EnumerateFiles(args[0]).Where(name => Regex.IsMatch(name, zipRegex));

Dictionary<string, int> usings = [];

// solutions analysis
foreach (var zipPath in zipsToOpen)
{
    using var zip = ZipFile.Open(zipPath, ZipArchiveMode.Read);

    const string solutionFileRegex = "^[0-9]+\\.cs$";
    var solutionFiles = zip.Entries.Where(entry => Regex.IsMatch(entry.Name, solutionFileRegex));
    
    foreach (var entry in solutionFiles)
    {
        var code = GetEntryContents(entry);
        var syntaxTree = CSharpSyntaxTree.ParseText(code);
        var root = syntaxTree.GetRoot();
        
        Traverse(root);
    }
}

foreach (var (us, no) in usings)
    Console.WriteLine($"{us}: {no}");

return;

void Traverse(SyntaxNode node)
{
    foreach (var child in node.ChildNodes())
        Traverse(child);

    if (node.IsKind(SyntaxKind.UsingDirective))
    {
        if (!usings.TryAdd(node.ToString(), 1))
            usings[node.ToString()]++;
    }
}

void Usage()
{
    Console.Error.Write($"Usage:\n\t{AppDomain.CurrentDomain.FriendlyName}");
    foreach (var arg in argsList)
        Console.Error.Write($" <{arg}>");
    Console.Error.WriteLine("\nWhere:");
    for (int i = 0; i < argsList.Length; i++)
        Console.Error.WriteLine($"\t{argsList[i]}: {argsDescription[i]}");
    Environment.Exit(1);
}

string GetEntryContents(ZipArchiveEntry entry)
{
    using var fileStream = entry.Open();
    int read = 0, offset = 0;
    var length = entry.Length;
    var buffer = new byte[length];
    
    while (read < length)
    {
        read = fileStream.Read(buffer, offset, (int)length);
        offset += read;
    }

    return Encoding.UTF8.GetString(buffer);
}