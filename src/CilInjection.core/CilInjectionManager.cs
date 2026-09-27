using System.Reflection;
using System.Runtime.Loader;
using CilInjection.Core;
using Mono.Cecil.Rocks;

namespace CilInstructionCounter.Core;

using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

public class CilInjectionManager
{
    private readonly List<IInjectionStrategy> _strategies = new();

    public CilInjectionManager(params IEnumerable<IInjectionStrategy> strategies)
    {
        ArgumentNullException.ThrowIfNull(strategies);
        _strategies.AddRange(strategies);
    }

    public CilInjectionManager AddStrategy(IInjectionStrategy strategy)
    {
        ArgumentNullException.ThrowIfNull(strategy);
        _strategies.Add(strategy);
        return this;
    }
    
    public byte[] Transform(byte[] assemblyBytes)
    {
        if (_strategies.Count == 0)
            return assemblyBytes; 

        using var stream = new MemoryStream(assemblyBytes);
        using var module = ModuleDefinition.ReadModule(stream);
        
        var methods = module.GetTypes()
            .SelectMany(t => t.Methods)
            .Where(m => m.HasBody && m.Body.Instructions.Count > 0)
            .ToList();

        foreach (var method in methods)
        {
            ProcessMethod(method);
        }
        
        using var outputStream = new MemoryStream();
        module.Write(outputStream);
        return outputStream.ToArray();
    }

    private void ProcessMethod(MethodDefinition method)
    {
        method.Body.SimplifyMacros();

        // FAZA 1: Modyfikacje / Usuwanie / Podmiany
        var transformContext = new MethodTransformationContext(method);
        foreach (var strategy in _strategies)
        {
            strategy.Transform(transformContext);
        }

        // FAZA 2: Wstrzykiwanie kodu
        var injectionContext = new MethodInjectionContext(method);
        foreach (var strategy in _strategies)
        {
            strategy.Inject(injectionContext);
        }

        // Aplikacja zmian w IL oraz wyliczenie nowej mapy celów dla skoków
        ApplyChangesAndRetarget(method, transformContext, injectionContext);

        // Optymalizacja skoków (zwijanie br do br.s tam, gdzie to możliwe)
        method.Body.OptimizeMacros();
    }

    private void ApplyChangesAndRetarget(
        MethodDefinition method, 
        MethodTransformationContext transformContext, 
        MethodInjectionContext injectionPlan)
    {
        var processor = method.Body.GetILProcessor();
        var originalInstructions = method.Body.Instructions.ToList();
        var mapping = new Dictionary<Instruction, Instruction>();

        // Budowanie mapy docelowej skoków w przebiegu wstecznym (od tyłu),
        // aby usunięte instrukcje automatycznie wskazywały na pierwszą aktywną instrukcję po nich.
        Instruction? nextActiveFirstInstruction = null;

        for (int i = originalInstructions.Count - 1; i >= 0; i--)
        {
            var origInst = originalInstructions[i];
            var mutation = transformContext.For(origInst);

            if (!mutation.IsRemoved)
            {
                var injContext = injectionPlan.For(origInst);
                var firstInstructionOfBlock = injContext.Before.FirstOrDefault() ?? origInst;

                mapping[origInst] = firstInstructionOfBlock;
                nextActiveFirstInstruction = firstInstructionOfBlock;
            }
            else
            {
                mapping[origInst] = nextActiveFirstInstruction 
                    ?? throw new InvalidOperationException($"Nie można usunąć ostatniej instrukcji ({origInst}) bez podania zastępcy.");
            }
        }

        // Aplikowanie zmian w strukturze IL
        foreach (var origInst in originalInstructions)
        {
            var mutation = transformContext.For(origInst);

            if (mutation.IsRemoved)
            {
                processor.Remove(origInst);
                continue;
            }

            // Aplikacja modyfikacji In-Place (OpCode / Operand)
            origInst.OpCode = mutation.OpCode;
            origInst.Operand = mutation.Operand;

            var injContext = injectionPlan.For(origInst);

            // Wstawianie sekwencji Before
            foreach (var beforeInst in injContext.Before)
            {
                processor.InsertBefore(origInst, beforeInst);
            }

            // Wstawianie zastępczych instrukcji (Faza 1 ReplaceWith)
            var current = origInst;
            foreach (var replInst in mutation.AdditionalReplacements)
            {
                processor.InsertAfter(current, replInst);
                current = replInst;
            }

            // Wstawianie sekwencji After
            foreach (var afterInst in injContext.After)
            {
                processor.InsertAfter(current, afterInst);
                current = afterInst;
            }
        }

        // Przepięcie skoków i bloków try-catch na nowe adresy
        RetargetBranches(method, mapping);
        RetargetExceptionHandlers(method, mapping);
    }

    private static void RetargetBranches(MethodDefinition method, Dictionary<Instruction, Instruction> mapping)
    {
        foreach (var inst in method.Body.Instructions)
        {
            if (inst.Operand is Instruction target && mapping.TryGetValue(target, out var newTarget))
            {
                inst.Operand = newTarget;
            }
            else if (inst.Operand is Instruction[] targets)
            {
                for (int i = 0; i < targets.Length; i++)
                {
                    if (mapping.TryGetValue(targets[i], out var switchTarget))
                    {
                        targets[i] = switchTarget;
                    }
                }
            }
        }
    }

    private static void RetargetExceptionHandlers(MethodDefinition method, Dictionary<Instruction, Instruction> mapping)
    {
        foreach (var handler in method.Body.ExceptionHandlers)
        {
            if (handler.TryStart != null && mapping.TryGetValue(handler.TryStart, out var ts)) handler.TryStart = ts;
            if (handler.TryEnd != null && mapping.TryGetValue(handler.TryEnd, out var te)) handler.TryEnd = te;
            if (handler.HandlerStart != null && mapping.TryGetValue(handler.HandlerStart, out var hs)) handler.HandlerStart = hs;
            if (handler.HandlerEnd != null && mapping.TryGetValue(handler.HandlerEnd, out var he)) handler.HandlerEnd = he;
            if (handler.FilterStart != null && mapping.TryGetValue(handler.FilterStart, out var fs)) handler.FilterStart = fs;
        }
    }
    
    /// <summary>
    /// Ładuje biblioteki RunTime wszystkich zarejestrowanych strategii do podanego kontekstu piaskownicy.
    /// </summary>
    public void LoadRuntimesInto(AssemblyLoadContext alc)
    {
        ArgumentNullException.ThrowIfNull(alc);

        foreach (var assembly in GetRuntimeAssembliesToLoad())
        {
            alc.LoadFromAssemblyPath(assembly.Location);
        }
    }
    
    private IEnumerable<Assembly> GetRuntimeAssembliesToLoad()
    {
        return _strategies
            .Select(s => s.RuntimeMarkerType.Assembly)
            .DistinctBy(a => a.FullName);
    }
}