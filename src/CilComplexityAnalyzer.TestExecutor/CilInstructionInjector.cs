using CilComplexityAnalyzer.Contract;
using Microsoft.Extensions.Logging;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

namespace CilComplexityAnalyzer.TestExecutor;

// TODO: inject CIL in student code
// Done, hehe :)
        
// TODO: In ContainerWorker set __InstructionCounter to 0 at test beginning and extract it after 
// DONE, we have methods
        
// TODO: Write UnitTests
//DONE
        
// TODO: inject abort mechanism
// Not Done, not hehe :(
        
// TODO: Counting code in external libraries
// Seams to be working

internal static class CilInstructionInjector
{
    internal static TestSuite InjectCilToStudentSolution(this TestSuite testSuite)
    {
        testSuite.Logger()?.LogInformation($"[{testSuite.Name}] Beginning CIL instruction injection in student solution.");

        if (testSuite.StudentSolutionAssemblyBytes is null)
        {
            throw new InvalidOperationException("StudentSolutionAssemblyBytes is null! Ensure Compilation succeeded before injecting CIL.");
        }

        testSuite.StudentSolutionAssemblyBytes = InjectCilToAssemblyBytes(testSuite.StudentSolutionAssemblyBytes);

        testSuite.Logger()?.LogInformation($"[{testSuite.Name}] CIL instruction injection completed.");

        return testSuite;
    }


    internal static byte[] InjectCilToAssemblyBytes(byte[] assemblyBytes)
    {
        using var inputStream = new MemoryStream(assemblyBytes);
        using var outputStream = new MemoryStream();
        var assemblyDef = AssemblyDefinition.ReadAssembly(inputStream);
        var mainModule = assemblyDef.MainModule;

        var globalCounterField = CreateGlobalCounterField(mainModule);
        
        foreach (var type in GetAllTypesRecursively(mainModule))
        {
            if (type.Name == "<GlobalCounterContainer>" || type.IsInterface) 
                continue;

            foreach (var method in type.Methods)
            {
                if (!method.HasBody)
                    continue;
                
                InjectCounter(mainModule, method, globalCounterField);
            }
        }

        assemblyDef.Write(outputStream);
        return outputStream.ToArray();
    }
    
    /// <summary>
    /// Tworzy dedykowaną publiczną klasę statyczną `<GlobalCounterContainer>` zawierającą jedyne pole `__InstructionCounter`.
    /// Użycie `Public` umożliwia swobodne zerowanie i odczyt z poziomu ContainerWorker przez refleksję.
    /// </summary>
    private static FieldDefinition CreateGlobalCounterField(ModuleDefinition module)
    {
        // Stworzenie nowej klasy
        var containerType = new TypeDefinition(
            "",
            "<GlobalCounterContainer>",
            TypeAttributes.Public | TypeAttributes.Abstract | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit,
            module.TypeSystem.Object);

        // Stworzenie statycznego pola
        var counterField = new FieldDefinition(
            "__InstructionCounter",
            FieldAttributes.Public | FieldAttributes.Static,
            module.TypeSystem.Int64);

        // Rejestracja w strukturze modułu
        containerType.Fields.Add(counterField);
        module.Types.Add(containerType);
        
        CreateCounterAccessors(module, containerType, counterField);

        return counterField;
    }

    private static void InjectCounter(
        ModuleDefinition module, 
        MethodDefinition method, 
        FieldDefinition counterField)
    {
        // Pobranie obiektu, który udostępnia metody do wstawiania, usuwania i podmieniania instrukcji CIL w ciele danej metody 
        var body = method.Body;
        body.SimplifyMacros(); // zmiana krótkich skosów na pełne 32-bitowe skoki czyli br.s, brtrue.s leave.s będą miały odpowiednie skoki 
        var il = body.GetILProcessor();
        var instructions = method.Body.Instructions.ToList();

        if (instructions.Count == 0) return;

        var entryMap = new Dictionary<Instruction, Instruction>();
        Instruction? pendingEntryPoint = null;

        foreach (var instr in instructions)
        {
            if (IsPrefixInstruction(instr))
            {
                if (pendingEntryPoint == null)
                {
                    pendingEntryPoint = InsertCounterBump(il, counterField, instr);
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

            entryMap[instr] = InsertCounterBump(il, counterField, instr);
        }

        RedirectAllBranches(body, entryMap);
        body.OptimizeMacros();
    }

    private static Instruction InsertCounterBump(
        ILProcessor il, FieldDefinition counterField, Instruction before)
    {
        var loadCounter = il.Create(OpCodes.Ldsfld, counterField);
        var loadOne = il.Create(OpCodes.Ldc_I8, 1L);
        var add = il.Create(OpCodes.Add);
        var storeCounter = il.Create(OpCodes.Stsfld, counterField);

        il.InsertBefore(before, loadCounter);
        il.InsertBefore(before, loadOne);
        il.InsertBefore(before, add);
        il.InsertBefore(before, storeCounter);

        return loadCounter;
    }

    private static void RedirectAllBranches(MethodBody body, Dictionary<Instruction, Instruction> entryMap)
    {
        foreach (var i in body.Instructions)
        {
            // Pojedyncze skoki
            if (i.Operand is Instruction target && entryMap.TryGetValue(target, out var newTarget))
            {
                i.Operand = newTarget;
            }
            // Skoki wielodrożne (switch)
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

        // Aktualizacja bloków try/catch/finally
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

    /// <summary>
    /// Przekierowuje wszystkie instrukcje skoków oraz bloki obsługi błędów ze starej instrukcji na nową.
    /// </summary>
    private static void RedirectBranches(MethodDefinition method, Instruction oldTarget, Instruction newTarget)
    {
        foreach (var i in method.Body.Instructions)
        {
            // Sprawdzenie czy instrukcja jest pojedynczym skokiem warunkowym lub bezwarunkowym do oldTargeta
            if (i.Operand is Instruction target && target == oldTarget)
            {
                // Podmiana destynacji skoku
                i.Operand = newTarget;
            }
            // Sprawdzenie czy intrukcja jest Skokiem wielodrożnym
            else if (i.Operand is Instruction[] targets)
            {
                // Iteracja po wszystkich instrukcjach docelowych
                for (int j = 0; j < targets.Length; j++)
                {
                    // Sprawdzenie czy któryś z targetów nie jest naszym szukanym i ewentualna jego zmiana
                    if (targets[j] == oldTarget)
                        targets[j] = newTarget;
                }
            }
        }

        // Aktualizacja granic bloków obsługi wyjątków (Exception Handlers)
        if (method.Body.HasExceptionHandlers)
        {
            foreach (var handler in method.Body.ExceptionHandlers)
            {
                if (handler.TryStart == oldTarget) handler.TryStart = newTarget;
                if (handler.TryEnd == oldTarget) handler.TryEnd = newTarget;
                if (handler.HandlerStart == oldTarget) handler.HandlerStart = newTarget;
                if (handler.HandlerEnd == oldTarget) handler.HandlerEnd = newTarget;
                if (handler.FilterStart == oldTarget) handler.FilterStart = newTarget;
            }
        }
    }
    
    /// <summary>
    /// Sprawdza, czy instrukcja jest prefiksem CIL modyfikującym zachowanie następnej instrukcji.
    /// </summary>
    private static bool IsPrefixInstruction(Instruction instruction)
    {
        var code = instruction.OpCode.Code;
        return code == Code.Constrained
               || code == Code.Readonly
               || code == Code.Unaligned
               || code == Code.Volatile
               || code == Code.Tail;
    }

    /// <summary>
    ///  Dodaje do kontenera metody statyczne umożliwiające odczyt i zerowanie licznika
    /// </summary>
    private static void CreateCounterAccessors(
        ModuleDefinition module,
        TypeDefinition containerType,
        FieldDefinition counterField)
    {
        var getMethod = new MethodDefinition(
            "GetInstructionCount",
            MethodAttributes.Public | MethodAttributes.Static,
            module.TypeSystem.Int64);

        var getIl = getMethod.Body.GetILProcessor();
        getIl.Append(getIl.Create(OpCodes.Ldsfld, counterField));
        getIl.Append(getIl.Create(OpCodes.Ret));
        
    var resetMethod = new MethodDefinition(
        "ResetInstructionCount",
        MethodAttributes.Public | MethodAttributes.Static,
        module.TypeSystem.Void);

        var resetIl = resetMethod.Body.GetILProcessor();
        resetIl.Append(resetIl.Create(OpCodes.Ldc_I8, 0L));
        resetIl.Append(resetIl.Create(OpCodes.Stsfld, counterField));
        resetIl.Append(resetIl.Create(OpCodes.Ret));

        containerType.Methods.Add(getMethod);
        containerType.Methods.Add(resetMethod);

    }
    
    /// <summary>
    /// Zwraca wszystkie typy w module, włącznie z dowolnie zagnieżdżonymi typami
    /// (maszyny stanów dla yield/async, domknięcia lambd, klasy anonimowe LINQ),
    /// bo ModuleDefinition.Types zwraca tylko typy najwyższego poziomu.
    /// </summary>
    private static IEnumerable<TypeDefinition> GetAllTypesRecursively(ModuleDefinition module)
    {
        foreach (var type in module.Types)
        {
            yield return type;

            foreach (var nested in GetNestedTypesRecursively(type))
                yield return nested;
        }
    }

    private static IEnumerable<TypeDefinition> GetNestedTypesRecursively(TypeDefinition type)
    {
        foreach (var nested in type.NestedTypes)
        {
            yield return nested;

            foreach (var deeperNested in GetNestedTypesRecursively(nested))
                yield return deeperNested;
        }
    }
}