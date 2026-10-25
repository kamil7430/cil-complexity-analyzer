using System;
using System.IO;
using System.Linq;
using CilComplexityAnalyzer.TestExecutor;
using CilInjection.Core;
using CilInjection.Core.Abstractions;
using Counter.Provider.InstructionCounter;

namespace CilComplexityAnalyzer.LibCilInjection;

public class CilInjectorLibrary
{
    private readonly CilInjectionManager _manager;
    public IInjectionStrategy Strategy { get; }

    public CilInjectorLibrary(IInjectionStrategy strategy)
    {
        Strategy = strategy ?? throw new ArgumentNullException(nameof(strategy));
        _manager = new CilInjectionManager(strategy);
    }

    public byte[] AnalyzeAndInject(byte[] inputBytes)
    {
        return _manager.Transform(inputBytes);
    }
    public void AnalyzeAndInjectFromFile(string inputPath = "Graphs.dll", string outputPath = "Graphs.instrumented.dll")
    {
        if (!File.Exists(inputPath))
        {
            Console.WriteLine($"ERROR: Input file {inputPath} not found");
            return;
        }
        
        var inputBytes = File.ReadAllBytes(inputPath);
        var outputBytes = AnalyzeAndInject(inputBytes);
        File.WriteAllBytes(outputPath, outputBytes);
        Console.WriteLine($"CIL injection injection completed to {outputPath}");
    }

    public void LoadRuntimesInto(System.Runtime.Loader.AssemblyLoadContext context)
    {
        _manager.LoadRuntimesInto(context);
    }
}
