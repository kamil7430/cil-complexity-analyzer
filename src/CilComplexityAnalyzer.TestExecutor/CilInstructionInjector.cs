using CilComplexityAnalyzer.Contract;
using Microsoft.Extensions.Logging;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

namespace CilComplexityAnalyzer.TestExecutor;

// TODO: inject CIL in student code
// Done, hehe :)
        
// TODO: In ContainerWorker set __InstructionCounter to 0 at test beginning and extract it after 
// Not Done, not hehe :(
        
// TODO: Write UnitTests
// Not Done, not hehe :(
        
// TODO: inject abort mechanism
// Not Done, not hehe :(
        
// TODO: Counting code in external libraries
// Not Done, not hehe :(

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
        // Wczytanie skompilowanego assembly z pamięci
        var assemblyDef = AssemblyDefinition.ReadAssembly(inputStream);
        var mainModule = assemblyDef.MainModule;

        // Publiczna klasa statyczna z publicznym polem, dostępna do zerowania z zewnątrz przez refleksję
        var globalCounterField = CreateGlobalCounterField(mainModule);
        
        // Przejście po definicjach typów w module
        // (klasy, interfejsy, struktury, enumy, delegaty, rekordy, typy anonimowe, typy generyczne)
        foreach (var type in mainModule.Types)
        {
            // Pominięcie wygenerowanej klasy kontenera, interfejsów
            if (type.Name == "<GlobalCounterContainer>" || type.IsInterface) 
                continue;

            // Przejście po metodach (klas, struktur, rekordów, delegat, typów generycznych)
            // type.Methods pominie enumy
            foreach (var method in type.Methods)
            {
                // Pominięcie metod bez bajtkodu CIL
                // odrzuca delegaty - HasBody == false
                if (!method.HasBody)
                    continue;
                
                InjectCounter(mainModule, method, globalCounterField);
            }
        }

        // Zapisanie zmodyfikowanego assembly z powrotem do pamięci
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

        return counterField;
    }

    private static void InjectCounter(
        ModuleDefinition module, 
        MethodDefinition method, 
        FieldDefinition counterField)
    {
        // Pobranie obiektu, który udostępnia metody do wstawiania, usuwania i podmieniania instrukcji CIL w ciele danej metody 
        var body = method.Body;
        body.SimplifyMacros(); // zmiana krótkich skosów na pełne 32-bitowe skoki
        var il = body.GetILProcessor();
        var instructions = method.Body.Instructions.ToList();

        if (instructions.Count == 0) return;

        var entryMap = new Dictionary<Instruction, Instruction>();

        // Inkrementacja licznika przed każdą instrukcją
        foreach (var instr in instructions)
        {
            // Wczytanie aktualnej wartości pola __InstructionCounter (typu long) na stos obliczeniowy
            var loadCounter = il.Create(OpCodes.Ldsfld, counterField);
            // Wrzucenie na stos stałą wartość liczbową 1 typu 64-bitowego
            var loadOne = il.Create(OpCodes.Ldc_I8, 1L);
            // Zdjęcie dwóch górnych wartości ze stosu, dodanie ich do siebie i wrzucenie wyniku (__InstructionCounter + 1) z powrotem na stos
            var add = il.Create(OpCodes.Add);
            // Zdjęcie wyniku ze stosu i zapisanie go z powrotem do pola __InstructionCounter
            var storeCounter = il.Create(OpCodes.Stsfld, counterField);
            
            entryMap[instr] = loadCounter;// Zapisujemy, że początkiem dawnej instrukcji 'instr' jest teraz 'loadCounter'

            // Wstawienie nowej sekwencji przed analizowaną instrukcję
            il.InsertBefore(instr, loadCounter);
            il.InsertBefore(instr, loadOne);
            il.InsertBefore(instr, add);
            il.InsertBefore(instr, storeCounter);

            // Naprawa etykiet skoków (Branch Fixup) 
            // Jeśli jakakolwiek inna instrukcja w metodzie skakała do 'instr',
            // to po wstawieniu inkrementacji musi teraz skakać do 'loadCounter'.
            //RedirectBranches(method, instr, loadCounter);
        }

        RedirectAllBranches(body, entryMap);
        // Optymalizacja rozmiarów skoków i przesunięć
        body.Optimize();
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
    
    
}