namespace CilInstructionCounter.Tests.Assertions;

using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mono.Cecil;
using Mono.Cecil.Cil;
using RunTime;

public static class InstructionCounterAssertions
{
    public static void ShouldHaveInjectedCounterSequenceComparedTo(
        this ModuleDefinition modifiedModule, 
        ModuleDefinition originalModule)
    {
        var originalMethods = originalModule.GetTypes()
            .SelectMany(t => t.Methods)
            .Where(m => m.HasBody && m.Body.Instructions.Count > 0)
            .ToList();

        Assert.IsTrue(originalMethods.Count > 0, "Oryginalny moduł powinien posiadać przynajmniej jedną metodę z instrukcjami.");

        foreach (var origMethod in originalMethods)
        {
            var modMethod = modifiedModule.GetTypes()
                .SelectMany(t => t.Methods)
                .FirstOrDefault(m => m.FullName == origMethod.FullName);

            Assert.IsNotNull(modMethod, $"Nie odnaleziono metody '{origMethod.FullName}' w zmodyfikowanym module.");

            var origInstructions = origMethod.Body.Instructions;
            var modInstructions = modMethod.Body.Instructions;

            int expectedCount = origInstructions.Count * 5;
            Assert.AreEqual(
                expectedCount, 
                modInstructions.Count, 
                $"Metoda '{origMethod.FullName}': oczekiwano {expectedCount} instrukcji po iniekcji.");

            for (int i = 0; i < origInstructions.Count; i++)
            {
                int baseIndex = i * 5;

                var ldsfld = modInstructions[baseIndex];
                var ldcI8 = modInstructions[baseIndex + 1];
                var add = modInstructions[baseIndex + 2];
                var stsfld = modInstructions[baseIndex + 3];
                var originalInst = modInstructions[baseIndex + 4];

                // 1. Ldsfld
                Assert.AreEqual(OpCodes.Ldsfld, ldsfld.OpCode, $"[{origMethod.Name}, blok #{i}] Oczekiwano OpCode Ldsfld.");
                AssertIsCounterFieldReference(ldsfld.Operand, origMethod.Name);

                // 2. Ldc_I8 (1L)
                Assert.AreEqual(OpCodes.Ldc_I8, ldcI8.OpCode, $"[{origMethod.Name}, blok #{i}] Oczekiwano OpCode Ldc_I8.");
                Assert.AreEqual(1L, ldcI8.Operand, $"[{origMethod.Name}, blok #{i}] Stała powinna wynosić 1L.");

                // 3. Add
                Assert.AreEqual(OpCodes.Add, add.OpCode, $"[{origMethod.Name}, blok #{i}] Oczekiwano OpCode Add.");

                // 4. Stsfld
                Assert.AreEqual(OpCodes.Stsfld, stsfld.OpCode, $"[{origMethod.Name}, blok #{i}] Oczekiwano OpCode Stsfld.");
                AssertIsCounterFieldReference(stsfld.Operand, origMethod.Name);

                // 5. Oryginalna instrukcja
                AssertOpCodesAreEqual(
                    origInstructions[i].OpCode, 
                    originalInst.OpCode, 
                    $"[{origMethod.Name}, blok #{i}] OpCode oryginalnej instrukcji uległ zmianie!");

                AssertOperandsAreEqual(
                    origInstructions[i].Operand, 
                    originalInst.Operand, 
                    $"[{origMethod.Name}, blok #{i}] Operand oryginalnej instrukcji uległ zmianie!");
            }
        }
    }

    public static void ShouldHaveRetargetedBranchTargetsComparedTo(
        this ModuleDefinition modifiedModule, 
        ModuleDefinition originalModule)
    {
        var originalMethods = originalModule.GetTypes()
            .SelectMany(t => t.Methods)
            .Where(m => m.HasBody && m.Body.Instructions.Count > 0)
            .ToList();

        int totalBranchesChecked = 0;

        foreach (var origMethod in originalMethods)
        {
            var modMethod = modifiedModule.GetTypes()
                .SelectMany(t => t.Methods)
                .FirstOrDefault(m => m.FullName == origMethod.FullName);

            Assert.IsNotNull(modMethod, $"Nie odnaleziono metody '{origMethod.FullName}' w zmodyfikowanym module.");

            var origInstructions = origMethod.Body.Instructions;
            var modInstructions = modMethod.Body.Instructions;

            for (int i = 0; i < origInstructions.Count; i++)
            {
                var origInst = origInstructions[i];

                if (!IsBranchInstruction(origInst))
                    continue;

                int modBranchIndex = (i * 5) + 4;
                var modBranchInst = modInstructions[modBranchIndex];

                AssertOpCodesAreEqual(
                    origInst.OpCode, 
                    modBranchInst.OpCode, 
                    $"[{origMethod.Name}, instrukcja #{i}] Instrukcja skoku uległa zmianie!");

                if (origInst.OpCode.OperandType == OperandType.InlineSwitch)
                {
                    var origTargets = (Instruction[])origInst.Operand;
                    var modTargets = (Instruction[])modBranchInst.Operand;

                    Assert.AreEqual(origTargets.Length, modTargets.Length, $"[{origMethod.Name}] Instrukcja switch ma inną liczbę gałęzi po iniekcji.");

                    for (int t = 0; t < origTargets.Length; t++)
                    {
                        int origTargetIndex = origInstructions.IndexOf(origTargets[t]);
                        var expectedModTarget = modInstructions[origTargetIndex * 5];

                        Assert.AreSame(
                            expectedModTarget, 
                            modTargets[t], 
                            $"[{origMethod.Name}, switch gałąź #{t}] Target skoku powinien wskazywać dokładnie na instrukcję Ldsfld licznika.");
                    }

                    totalBranchesChecked += origTargets.Length;
                }
                else
                {
                    var origTarget = (Instruction)origInst.Operand;
                    int origTargetIndex = origInstructions.IndexOf(origTarget);

                    var expectedModTarget = modInstructions[origTargetIndex * 5];
                    var modTarget = (Instruction)modBranchInst.Operand;

                    Assert.AreSame(
                        expectedModTarget, 
                        modTarget, 
                        $"[{origMethod.Name}, instrukcja #{i}] Cel skoku ({origInst.OpCode}) nie został prawidłowo przepięty na początek sekwencji licznika.");

                    totalBranchesChecked++;
                }
            }
        }

        Assert.IsTrue(totalBranchesChecked > 0, "Przetestowane moduły powinny zawierać przynajmniej jedną instrukcję skoku.");
    }

    private static void AssertIsCounterFieldReference(object? operand, string methodName)
    {
        Assert.IsInstanceOfType(operand, typeof(FieldReference), $"[{methodName}] Operand powinien być FieldReference.");
        var fieldRef = (FieldReference)operand!;
        Assert.AreEqual(nameof(GlobalCounterContainer.InstructionCounter), fieldRef.Name, $"[{methodName}] Zła nazwa pola.");
        Assert.AreEqual(typeof(GlobalCounterContainer).FullName, fieldRef.DeclaringType.FullName, $"[{methodName}] Zły typ pola.");
    }

    private static bool IsBranchInstruction(Instruction instruction)
    {
        var operandType = instruction.OpCode.OperandType;
        return operandType == OperandType.InlineBrTarget || 
               operandType == OperandType.ShortInlineBrTarget || 
               operandType == OperandType.InlineSwitch;
    }

    private static void AssertOpCodesAreEqual(OpCode expected, OpCode actual, string message)
    {
        var canonicalExpected = GetCanonicalOpCode(expected);
        var canonicalActual = GetCanonicalOpCode(actual);

        Assert.AreEqual(canonicalExpected, canonicalActual, message);
    }

    private static void AssertOperandsAreEqual(object? expected, object? actual, string message)
    {
        if (expected == null && actual == null) return;

        if (expected is MemberReference expMember && actual is MemberReference actMember)
        {
            Assert.AreEqual(expMember.FullName, actMember.FullName, message);
            return;
        }

        if (expected is ParameterDefinition expParam && actual is ParameterDefinition actParam)
        {
            Assert.AreEqual(expParam.Index, actParam.Index, message);
            return;
        }

        if (expected is VariableDefinition expVar && actual is VariableDefinition actVar)
        {
            Assert.AreEqual(expVar.Index, actVar.Index, message);
            return;
        }

        // Domyślne porównanie dla typów prostych (int, long, string itd.)
        Assert.AreEqual(expected, actual, message);
    }

    private static OpCode GetCanonicalOpCode(OpCode opCode)
    {
        // Mapowanie skoków krótkich (.s) na ich długie odpowiedniki
        if (opCode == OpCodes.Br_S) return OpCodes.Br;
        if (opCode == OpCodes.Brfalse_S) return OpCodes.Brfalse;
        if (opCode == OpCodes.Brtrue_S) return OpCodes.Brtrue;
        if (opCode == OpCodes.Beq_S) return OpCodes.Beq;
        if (opCode == OpCodes.Bge_S) return OpCodes.Bge;
        if (opCode == OpCodes.Bgt_S) return OpCodes.Bgt;
        if (opCode == OpCodes.Ble_S) return OpCodes.Ble;
        if (opCode == OpCodes.Blt_S) return OpCodes.Blt;
        if (opCode == OpCodes.Bne_Un_S) return OpCodes.Bne_Un;
        if (opCode == OpCodes.Bge_Un_S) return OpCodes.Bge_Un;
        if (opCode == OpCodes.Bgt_Un_S) return OpCodes.Bgt_Un;
        if (opCode == OpCodes.Ble_Un_S) return OpCodes.Ble_Un;
        if (opCode == OpCodes.Blt_Un_S) return OpCodes.Blt_Un;
        if (opCode == OpCodes.Leave_S) return OpCodes.Leave;

        return opCode;
    }
}