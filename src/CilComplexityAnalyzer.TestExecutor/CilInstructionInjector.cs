using System.IO;
using System.Linq;
using CilComplexityAnalyzer.TestExecutor.Contract;
using Microsoft.Extensions.Logging;
using Mono.Cecil;
using Mono.Cecil.Cil;

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
    internal static TestSuite InjectCil(this TestSuite testSuite)
    {
        testSuite.Logger?.LogInformation($"[{testSuite.NameOrHash}] Beginning CIL instruction injection.");

        if (testSuite.AssemblyBytes is null)
        {
            throw new InvalidOperationException("AssemblyBytes is null! Ensure Compilation succeeded before injecting CIL.");
        }

        using var inputStream = new MemoryStream(testSuite.AssemblyBytes);
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
            // Pominięcie wygenerowanej klasy kontenera, interfejsów i typów generowanych automatycznie przez kompilator
            // (np. typy anonimowe, closure dla lambd/LINQ, generatory async/yield)
            if (type.Name == "<GlobalCounterContainer>" || type.IsInterface || type.Name.StartsWith("<")) 
                continue;

            // Przejście po metodach (klas, struktur, rekordów, delegat, typów generycznych)
            // type.Methods pominie enumy
            foreach (var method in type.Methods)
            {
                // Pominięcie metod bez bajtkodu CIL oraz konstruktory
                // odrzuca delegaty - HasBody == false
                if (!method.HasBody || method.IsConstructor)
                    continue;
                
                InjectCounter(mainModule, method, globalCounterField, isEntryPoint);
            }
        }

        // Zapisanie zmodyfikowanego assembly z powrotem do pamięci
        assemblyDef.Write(outputStream);
        testSuite.AssemblyBytes = outputStream.ToArray();

        testSuite.Logger?.LogInformation($"[{testSuite.NameOrHash}] CIL instruction injection completed.");

        return testSuite;
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
        var il = method.Body.GetILProcessor();
        var instructions = method.Body.Instructions.ToList();

        if (instructions.Count == 0) return;

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

            // Wstawienie nowej sekwencji przed analizowaną instrukcję
            il.InsertBefore(instr, loadCounter);
            il.InsertBefore(instr, loadOne);
            il.InsertBefore(instr, add);
            il.InsertBefore(instr, storeCounter);

            // Naprawa etykiet skoków (Branch Fixup) 
            // Jeśli jakakolwiek inna instrukcja w metodzie skakała do 'instr',
            // to po wstawieniu inkrementacji musi teraz skakać do 'loadCounter'.
            RedirectBranches(method, instr, loadCounter);
        }

        // Optymalizacja rozmiarów skoków i przesunięć
        method.Body.Optimize();
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