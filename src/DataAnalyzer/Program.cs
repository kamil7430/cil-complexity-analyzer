using System.Diagnostics;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace DataAnalyzer;

internal class Program
{
    private static readonly UsingsCollector UsingsCollector = new();
    
    private static void ForEachFile(string fileName, SyntaxNode root)
    {
        UsingsCollector.Visit(root, fileName);
    }

    private static void AfterAnalysis()
    {
        UsingsCollector.PrintUsings(false);
    }
    
    private static readonly string[] ArgsList = ["solutions_dir"];
    private static readonly string[] ArgsDescription = ["directory of zip-compressed solutions"];
    
    internal static void Main(string[] args)
    {
        Debug.Assert(ArgsList.Length == ArgsDescription.Length);
        if (args.Length != ArgsList.Length) 
            Usage();

        const string zipRegex = "^.+\\.zip$";
        var zipsToOpen = Directory.EnumerateFiles(args[0]).Where(name => Regex.IsMatch(name, zipRegex));

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
        
                ForEachFile(Path.Combine(zipPath, entry.FullName), root);
            }
        }

        AfterAnalysis();
    }
    
    private static void Usage()
    {
        Console.Error.Write($"Usage:\n\t{AppDomain.CurrentDomain.FriendlyName}");
        foreach (var arg in ArgsList)
            Console.Error.Write($" <{arg}>");
        Console.Error.WriteLine("\nWhere:");
        for (int i = 0; i < ArgsList.Length; i++)
            Console.Error.WriteLine($"\t{ArgsList[i]}: {ArgsDescription[i]}");
        Environment.Exit(1);
    }
    
    private static string GetEntryContents(ZipArchiveEntry entry)
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
}