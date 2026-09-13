using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CilComplexityAnalyzer.Contract;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using CilComplexityAnalyzer.TestExecutor;
namespace CilComplexityAnalyzer.LibCilInjection;

public class CilInjectorLibrary
{
    public void AnalyzeAndInjectFromFile(string inputPath = "Graphs.dll", string outputPath = "Graphs.instrumented.dll")
    {
        var bytes = File.ReadAllBytes(inputPath);

        if (!File.Exists(inputPath))
        {
            Console.WriteLine($"ERROR: Input file {inputPath} not found");
            return;
        }

        var inputBytes = File.ReadAllBytes(inputPath);
        
        var outputBytes = CilInstructionInjector.InjectCilToAssemblyBytes(inputBytes);
        File.WriteAllBytes(outputPath, outputBytes as byte[]);
        Console.WriteLine($"CIL injection injection completed to {outputPath}");
    }
}
