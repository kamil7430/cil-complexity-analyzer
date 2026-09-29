using CilInjection.Core.Contexts;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace CilInjection.Core.Utils;

internal class ILInjector
{
    public void ApplyInjections(MethodDefinition method, MethodInjectionContext injectionContext)
    {
        var processor = method.Body.GetILProcessor();

        var injectionMapping = BuildInjectionMapping(injectionContext);

        InsertInstructions(processor, injectionContext);
        RetargetBranches(method, injectionMapping);
        RetargetExceptionHandlers(method, injectionMapping);
    }

    private static Dictionary<Instruction, Instruction> BuildInjectionMapping(
        MethodInjectionContext injectionContext)
    {
        var mapping = new Dictionary<Instruction, Instruction>();
        foreach (var instCon in injectionContext.GetContexts())
        {
            var firstInstruction = instCon.GetFirstInstruction();
            foreach (var origInst in instCon.Original.GetAllInstructions())
            {
                mapping[origInst] = firstInstruction;
            }
        }
        return mapping;
    }

    private static void InsertInstructions(
        ILProcessor processor, 
        MethodInjectionContext injectionContext)
    {
        foreach (var instCon in injectionContext.GetContexts())
        {
            var firstOriginalInstruction = instCon.Original.GetFirstInstruction();
            foreach (var beforeInst in instCon.Before)
            {
                processor.InsertBefore(firstOriginalInstruction, beforeInst);
            }
            var lastOriginalInstruction = instCon.Original.GetLastInstruction();
            foreach (var afterInst in instCon.GetAfterReversed())
            {
                processor.InsertAfter(lastOriginalInstruction, afterInst);
            }
        }
    }

    private static void RetargetBranches(MethodDefinition method, IReadOnlyDictionary<Instruction, Instruction> mapping)
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

    private static void RetargetExceptionHandlers(MethodDefinition method, IReadOnlyDictionary<Instruction, Instruction> mapping)
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
}