namespace CilInjection.Core.Weaver;

using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

public static class MethodBodyWeaverEngine
{
    /// <summary>
    /// Bezpiecznie wstrzykuje instrukcje CIL przed każdą instrukcją metody, 
    /// automatycznie dbając o prefiksy, korektę skoków (branches) i bloków try/catch.
    /// </summary>
    public static void InjectAtInstructionLevel(
        this MethodDefinition method,
        Func<ILProcessor, Instruction, IEnumerable<Instruction>> instructionFactory)
    {
        if (!method.HasBody) return;

        var body = method.Body;
        body.SimplifyMacros(); // Rozszerzamy skoki do wersji 32-bitowych przed modyfikacją
        var il = body.GetILProcessor();
        var instructions = body.Instructions.ToList();

        if (instructions.Count == 0) return;

        var entryMap = new Dictionary<Instruction, Instruction>();
        Instruction? pendingEntryPoint = null;

        foreach (var instr in instructions)
        {
            if (IsPrefixInstruction(instr))
            {
                if (pendingEntryPoint == null)
                {
                    var newInstructions = instructionFactory(il, instr);
                    pendingEntryPoint = InsertInstructionsBefore(il, instr, newInstructions);
                }
                entryMap[instr] = instr;
                continue;
            }

            if (pendingEntryPoint != null)
            {
                entryMap[instr] = pendingEntryPoint;
                pendingEntryPoint = null;
                continue;
            }

            var injectedInstructions = instructionFactory(il, instr);
            entryMap[instr] = InsertInstructionsBefore(il, instr, injectedInstructions);
        }

        RedirectAllBranches(body, entryMap);
        body.OptimizeMacros(); // Zwężamy skoki z powrotem do wersji optymalnych
    }

    private static Instruction InsertInstructionsBefore(
        ILProcessor il, 
        Instruction target, 
        IEnumerable<Instruction> instructionsToInsert)
    {
        Instruction? firstInjected = null;

        foreach (var newInstr in instructionsToInsert)
        {
            il.InsertBefore(target, newInstr);
            firstInjected ??= newInstr;
        }

        return firstInjected ?? target;
    }

    private static bool IsPrefixInstruction(Instruction instruction)
    {
        var code = instruction.OpCode.Code;
        return code == Code.Constrained
               || code == Code.Readonly
               || code == Code.Unaligned
               || code == Code.Volatile
               || code == Code.Tail;
    }

    private static void RedirectAllBranches(MethodBody body, Dictionary<Instruction, Instruction> entryMap)
    {
        foreach (var i in body.Instructions)
        {
            if (i.Operand is Instruction target && entryMap.TryGetValue(target, out var newTarget))
            {
                i.Operand = newTarget;
            }
            else if (i.Operand is Instruction[] targets)
            {
                for (int j = 0; j < targets.Length; j++)
                {
                    if (entryMap.TryGetValue(targets[j], out var newMultiTarget))
                    {
                        targets[j] = newMultiTarget;
                    }
                }
            }
        }

        if (body.HasExceptionHandlers)
        {
            foreach (var handler in body.ExceptionHandlers)
            {
                if (handler.TryStart != null && entryMap.TryGetValue(handler.TryStart, out var newTryStart))
                    handler.TryStart = newTryStart;

                if (handler.TryEnd != null && entryMap.TryGetValue(handler.TryEnd, out var newTryEnd))
                    handler.TryEnd = newTryEnd;

                if (handler.HandlerStart != null && entryMap.TryGetValue(handler.HandlerStart, out var newHandlerStart))
                    handler.HandlerStart = newHandlerStart;

                if (handler.HandlerEnd != null && entryMap.TryGetValue(handler.HandlerEnd, out var newHandlerEnd))
                    handler.HandlerEnd = newHandlerEnd;

                if (handler.FilterStart != null && entryMap.TryGetValue(handler.FilterStart, out var newFilterStart))
                    handler.FilterStart = newFilterStart;
            }
        }
    }
}