using CilComplexityAnalyzer.TestExecutor.Contract;
using Microsoft.VisualStudio.TestTools.UnitTesting; 
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace CilComplexityAnalyzer.TestExecutor.Tests.CilInstructionInjector.Assertions;

internal static class CilInjectorAssertions
{
    public static void ShouldThrowInvalidOperationExceptionWhenInjecting(this TestSuite testSuite)
    {
        Assert.ThrowsException<InvalidOperationException>(
            () => testSuite.InjectCilToStudentSolution(),
            "Oczekiwano wyjątku InvalidOperationException z powodu braku bajtów assembly studenta."
        );
    }

    public static TypeDefinition ShouldContainGlobalCounterContainer(this TestSuite testSuite)
    {
        Assert.IsNotNull(testSuite.StudentSolutionAssemblyBytes,
            "StudentSolutionAssemblyBytes nie mogą być null po iniekcji.");

        using var stream = new MemoryStream(testSuite.StudentSolutionAssemblyBytes);
        var assemblyDef = AssemblyDefinition.ReadAssembly(stream);

        var containerType = assemblyDef.MainModule.Types
            .FirstOrDefault(t => t.Name == "<GlobalCounterContainer>");

        Assert.IsNotNull(containerType, "Klasa <GlobalCounterContainer> nie została stworzona.");
        Assert.IsTrue(containerType.IsPublic, "<GlobalCounterContainer> powinna być publiczna.");
        Assert.IsTrue(containerType.IsAbstract && containerType.IsSealed,
            "<GlobalCounterContainer> powinna być klasą statyczną.");

        return containerType;
    }

    public static void ShouldHaveStaticLongCounterField(this TypeDefinition containerType)
    {
        var counterField = containerType.Fields.FirstOrDefault(f => f.Name == "__InstructionCounter");

        Assert.IsNotNull(counterField, "Pole __InstructionCounter nie istnieje w klasie kontenera.");
        Assert.IsTrue(counterField.IsStatic, "Pole __InstructionCounter powinno być statyczne.");
        Assert.IsTrue(counterField.IsPublic, "Pole __InstructionCounter powinno być publiczne.");
        Assert.AreEqual("System.Int64", counterField.FieldType.FullName,
            "Typ pola powinien wynosić System.Int64 (long).");
    }

    public static MethodDefinition ShouldHaveInstructionSeqeunceInjected(
        this TestSuite testSuite,
        string className,
        string methodName)
    {
        Assert.IsNotNull(testSuite.StudentSolutionAssemblyBytes,
            "StudentSolutionAssemblyBytes nie mogą być null po iniekcji.");

        using var stream = new MemoryStream(testSuite.StudentSolutionAssemblyBytes);
        var assemblyDef = AssemblyDefinition.ReadAssembly(stream);

        var typeDef = assemblyDef.MainModule.Types.FirstOrDefault(t => t.Name == className);
        Assert.IsNotNull(typeDef, $"Nie znaleziono klasy '{className}' w zmodyfikowanym assembly.");

        var methodDef = typeDef.Methods.FirstOrDefault(m => m.Name == methodName);
        Assert.IsNotNull(methodDef, $"Nie znaleziono metody '{methodName}' w klasie '{className}'.");
        Assert.IsTrue(methodDef.HasBody, $"Metoda '{methodName}' nie posiada ciała CIL.");

        var instructions = methodDef.Body.Instructions;
        var hasValidSequence = false;

        for (int i = 0; i <= instructions.Count - 4; i++)
        {
            // Sprawdzenie Ldsfld __InstructionCounter
            var isLdsfld = instructions[i].OpCode == OpCodes.Ldsfld &&
                           instructions[i].Operand is MemberReference fieldLoad &&
                           fieldLoad.Name == "__InstructionCounter";

            if (!isLdsfld) continue;

            // Ładowanie stałej 1 na stos (Ldc_I4_1, Ldc_I8 z operandem 1, lub Ldc_I4 z 1)
            var op2 = instructions[i + 1];
            var isLoadOne = op2.OpCode == OpCodes.Ldc_I4_1 ||
                            (op2.OpCode == OpCodes.Ldc_I8 && Equals(op2.Operand, 1L)) ||
                            (op2.OpCode == OpCodes.Ldc_I4 && Equals(op2.Operand, 1));

            if (!isLoadOne) continue;

            // Określenie przesunięcia dla Add i Stsfld w zależności od tego, czy po Ldc pojawiła się konwersja typów (Conv_I8 / Conv_U8)
            int offset = 2;
            if (i + offset < instructions.Count &&
                (instructions[i + offset].OpCode == OpCodes.Conv_I8 ||
                 instructions[i + offset].OpCode == OpCodes.Conv_U8))
            {
                offset++; // Przesunięcie indeks o 1 dla Add i Stsfld
            }

            if (i + offset + 1 >= instructions.Count) continue;

            // Sprawdzenie Add / Add_Ovf
            var isAdd = instructions[i + offset].OpCode == OpCodes.Add ||
                        instructions[i + offset].OpCode == OpCodes.Add_Ovf;

            // Sprawdzenie Stsfld __InstructionCounter
            var isStsfld = instructions[i + offset + 1].OpCode == OpCodes.Stsfld &&
                           instructions[i + offset + 1].Operand is MemberReference fieldStore &&
                           fieldStore.Name == "__InstructionCounter";

            if (isAdd && isStsfld)
            {
                hasValidSequence = true;
                break;
            }
        }

        Assert.IsTrue(
            hasValidSequence,
            $"Metoda '{methodName}' nie zawiera poprawnej sekwencji CIL modyfikującej __InstructionCounter."
        );

        return methodDef;
    }
}
