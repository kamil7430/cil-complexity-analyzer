using CilComplexityAnalyzer.Contract;
using Microsoft.VisualStudio.TestTools.UnitTesting; 
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;

namespace CilComplexityAnalyzer.TestExecutor.Tests.CilInstructionInjector.Assertions;

internal static class CilInjectorAssertions
{
    // --- STAŁE KONFIGURACYJNE ---
    public const string CounterContainerClassName = "<GlobalCounterContainer>";
    public const string CounterFieldName = "__InstructionCounter";

    // Wzorzec opkodów wstrzykiwanej sekwencji 
    public static readonly OpCode[] InjectedSequenceOpCodes = new[]
    {
        OpCodes.Ldsfld,   // Załadowanie wartości licznika 
        OpCodes.Ldc_I4_1, // Załadowanie jedynki
        OpCodes.Conv_I8,  // Konwersja int -> long
        OpCodes.Add,      // Dodanie
        OpCodes.Stsfld    // Zapis do licznika
    };

    public static int InjectedSequenceLength => InjectedSequenceOpCodes.Length;

    private static AssemblyDefinition GetStudentAssemblyDefinition(this TestSuite testSuite)
    {
        Assert.IsNotNull(testSuite.StudentSolutionAssemblyBytes, "Assembly bytes nie mogą być null.");
        
        // Strumień nie jest zamykany przez 'using' - zamknie się automatycznie po wywołaniu Dispose() na AssemblyDefinition.
        var stream = new MemoryStream(testSuite.StudentSolutionAssemblyBytes);
        
        return AssemblyDefinition.ReadAssembly(stream, new ReaderParameters
        {
            ReadingMode = ReadingMode.Immediate
        });
    }

    private static TypeDefinition GetStudentClassDefinition(this AssemblyDefinition assembly, string className)
    {
        var typeDef = assembly.MainModule.Types.FirstOrDefault(t => t.Name == className);
        Assert.IsNotNull(typeDef, $"Nie znaleziono klasy '{className}' w zmodyfikowanym assembly.");
        return typeDef;
    }

    private static MethodDefinition GetStudentMethodDefinition(this AssemblyDefinition assembly, string className, string methodName)
    {
        var typeDef = assembly.GetStudentClassDefinition(className);
        var methodDef = typeDef.Methods.FirstOrDefault(m => m.Name == methodName);
        Assert.IsNotNull(methodDef, $"Nie znaleziono metody '{methodName}' w klasie '{className}'.");
        Assert.IsTrue(methodDef.HasBody, $"Metoda '{methodName}' nie posiada ciała CIL.");
        return methodDef;
    }

    private static Collection<Instruction> GetInstructions(this AssemblyDefinition assembly, string className, string methodName)
    {
        var methodDef = assembly.GetStudentMethodDefinition(className, methodName);
        return methodDef.Body.Instructions;
    }

    // --- METODY POMOCNICZE WERYFIKACJI SEKWENCJI LICZNIKA ---

    /// <summary>
    /// Sprawdza, czy dana instrukcja jest pierwszą instrukcją sekwencji zliczającej (Ldsfld pola licznika).
    /// </summary>
    private static bool IsStartOfInjectedSequence(Instruction? instruction)
    {
        return instruction != null &&
               instruction.OpCode == InjectedSequenceOpCodes[0] &&
               instruction.Operand is MemberReference fieldRef &&
               fieldRef.Name == CounterFieldName;
    }

    /// <summary>
    /// Sprawdza, czy od danego indeksu w kolekcji rozpoczyna się pełna sekwencja zliczająca.
    /// </summary>
    private static bool IsStartOfInjectedSequence(Collection<Instruction> instructions, int index)
    {
        return index + InjectedSequenceLength <= instructions.Count &&
               IsStartOfInjectedSequence(instructions[index]);
    }

    /// <summary>
    /// Weryfikuje, czy target wskazuje na początek wstrzykniętej sekwencji licznika,
    /// sprawdza jej spójność i zwraca właściwą instrukcję znajdującą się TUŻ ZA nią (+5 kroków).
    /// </summary>
    private static Instruction ResolveTargetToOriginalInstruction(Instruction target)
    {
        // 1. Wykorzystujemy naszą metodę pomocniczą
        Assert.IsTrue(
            IsStartOfInjectedSequence(target),
            $"Cel skoku powinien wskazywać na '{InjectedSequenceOpCodes[0]}' licznika '{CounterFieldName}', a wskazuje na '{target?.OpCode}'."
        );

        // 2. Walidujemy całą 5-instrukcyjną sekwencję i przesuwamy wskaźnik .Next
        var current = target;
        for (int i = 0; i < InjectedSequenceLength; i++)
        {
            Assert.IsNotNull(current, "Sekwencja zliczająca została przedwcześnie przerwana (koniec metody).");

            Assert.AreEqual(
                InjectedSequenceOpCodes[i].Code,
                current.OpCode.Code,
                $"Niezgodność opkodu w sekwencji zliczającej na pozycji +{i}. Oczekiwano '{InjectedSequenceOpCodes[i]}', znaleziono '{current.OpCode}'."
            );

            current = current.Next;
        }

        Assert.IsNotNull(current, "Brak instrukcji docelowej po wstrzykniętej sekwencji licznika.");
        return current;
    }

    private static List<Instruction> GetNonInjectedInstructions(Collection<Instruction> instructions)
    {
        var result = new List<Instruction>();
        int i = 0;

        while (i < instructions.Count)
        {
            if (IsStartOfInjectedSequence(instructions, i))
            {
                i += InjectedSequenceLength;
            }
            else
            {
                result.Add(instructions[i]);
                i++;
            }
        }

        return result;
    }

    private static Instruction[] GetBranchTargets(Instruction branchInstruction)
    {
        return branchInstruction.Operand switch
        {
            Instruction singleTarget => new[] { singleTarget },
            Instruction[] switchTargets => switchTargets,
            _ => Array.Empty<Instruction>()
        };
    }

    // --- PUBLICZNE METODY ASERCJI ---

    public static void ShouldThrowInvalidOperationExceptionWhenInjecting(this TestSuite testSuite)
    {
        Assert.ThrowsException<InvalidOperationException>(
            () => testSuite.InjectCilToStudentSolution(),
            "Oczekiwano wyjątku InvalidOperationException z powodu braku bajtów assembly studenta."
        );
    }

    public static TypeDefinition ShouldContainGlobalCounterContainer(this TestSuite testSuite)
    {
        using var assemblyDef = testSuite.GetStudentAssemblyDefinition();

        var containerType = assemblyDef.MainModule.Types
            .FirstOrDefault(t => t.Name == CounterContainerClassName);

        Assert.IsNotNull(containerType, $"Klasa {CounterContainerClassName} nie została stworzona.");
        Assert.IsTrue(containerType.IsPublic, $"{CounterContainerClassName} powinna być publiczna.");
        Assert.IsTrue(containerType.IsAbstract && containerType.IsSealed,
            $"{CounterContainerClassName} powinna być klasą statyczną.");

        return containerType;
    }

    public static void ShouldHaveStaticLongCounterField(this TypeDefinition containerType)
    {
        var counterField = containerType.Fields.FirstOrDefault(f => f.Name == CounterFieldName);

        Assert.IsNotNull(counterField, $"Pole {CounterFieldName} nie istnieje w klasie kontenera.");
        Assert.IsTrue(counterField.IsStatic, $"Pole {CounterFieldName} powinno być statyczne.");
        Assert.IsTrue(counterField.IsPublic, $"Pole {CounterFieldName} powinno być publiczne.");
        Assert.AreEqual("System.Int64", counterField.FieldType.FullName,
            "Typ pola powinien wynosić System.Int64 (long).");
    }

    public static void ShouldHaveInstructionSequenceInjected(this TestSuite testSuite,
        string className,
        string methodName)
    {
        using var assembly = testSuite.GetStudentAssemblyDefinition();
        var instructions = assembly.GetInstructions(className, methodName);
        var hasValidSequence = false;

        for (int i = 0; i <= instructions.Count - InjectedSequenceLength; i++)
        {
            if (IsStartOfInjectedSequence(instructions, i))
            {
                hasValidSequence = true;
                break;
            }
        }

        Assert.IsTrue(
            hasValidSequence,
            $"Metoda '{methodName}' nie zawiera poprawnej sekwencji CIL modyfikującej pole '{CounterFieldName}'."
        );
    }

    public static void ShouldRedirectBranchTargetsToCorrectInjectedCounter(
        this TestSuite testSuite, 
        TestSuite originalTestSuite, 
        string className, 
        string methodName)
    {
        using var modifiedAssembly = testSuite.GetStudentAssemblyDefinition();
        using var originalAssembly = originalTestSuite.GetStudentAssemblyDefinition();
        
        var originalInstructions = originalAssembly.GetInstructions(className, methodName);
        var modifiedInstructions = modifiedAssembly.GetInstructions(className, methodName);

        // 1. Wyciągamy wyłącznie oryginalne instrukcje ze zmodyfikowanego kodu (omijając wstrzyknięte liczniki)
        var originalInModified = GetNonInjectedInstructions(modifiedInstructions);

        Assert.AreEqual(
            originalInstructions.Count, 
            originalInModified.Count, 
            "Liczba oryginalnych instrukcji uległa zmianie po iniekcji."
        );

        // 2. Mapujemy k-tą oryginalną instrukcję na jej odpowiednik w zmodyfikowanym CIL
        var originalToModifiedMap = new Dictionary<Instruction, Instruction>();
        for (int i = 0; i < originalInstructions.Count; i++)
        {
            originalToModifiedMap[originalInstructions[i]] = originalInModified[i];
        }

        // 3. Wyciągamy instrukcje skoku z oryginalnego kodu
        var originalBranches = originalInstructions
            .Where(i => i.OpCode.FlowControl == FlowControl.Branch || 
                        i.OpCode.FlowControl == FlowControl.Cond_Branch)
            .ToList();

        foreach (var origBranch in originalBranches)
        {
            // Odnajdujemy zmodyfikowaną instrukcję skoku bezpośrednio z mapy
            var modBranch = originalToModifiedMap[origBranch];

            var origTargets = GetBranchTargets(origBranch);
            var modTargets = GetBranchTargets(modBranch);

            Assert.AreEqual(
                origTargets.Length, 
                modTargets.Length, 
                $"Liczba celów skoku dla instrukcji '{origBranch.OpCode}' uległa zmianie."
            );

            for (int t = 0; t < origTargets.Length; t++)
            {
                var origTarget = origTargets[t];
                var actualModTarget = modTargets[t];

                var expectedModInstruction = originalToModifiedMap[origTarget];

                // Weryfikujemy sekwencję licznika i sprawdzamy, czy stoi przed właściwą instrukcją
                var instructionAfterCounter = ResolveTargetToOriginalInstruction(actualModTarget);

                Assert.AreSame(
                    expectedModInstruction,
                    instructionAfterCounter,
                    $"Skok dla '{origBranch.OpCode}' przekierował do licznika ZŁEJ instrukcji. Wstrzyknięty licznik znajduje się przed '{instructionAfterCounter.OpCode}', a powinien znajdować się przed '{expectedModInstruction.OpCode}'."
                );
            }
        }
    }
}