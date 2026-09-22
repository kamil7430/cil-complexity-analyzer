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

            // Każda oryginalna instrukcja generuje blok 5 instrukcji (4 wstrzyknięte + 1 oryginalna)
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
                Assert.AreEqual(
                    origInstructions[i].OpCode, 
                    originalInst.OpCode, 
                    $"[{origMethod.Name}, blok #{i}] OpCode oryginalnej instrukcji uległ zmianie!");

                Assert.AreEqual(
                    origInstructions[i].Operand, 
                    originalInst.Operand, 
                    $"[{origMethod.Name}, blok #{i}] Operand oryginalnej instrukcji uległ zmianie!");
            }
        }
    }

    private static void AssertIsCounterFieldReference(object? operand, string methodName)
    {
        Assert.IsInstanceOfType(operand, typeof(FieldReference), $"[{methodName}] Operand powinien być referencją do pola (FieldReference).");
        var fieldRef = (FieldReference)operand!;
        Assert.AreEqual(nameof(GlobalCounterContainer.InstructionCounter), fieldRef.Name, $"[{methodName}] Nazwa pola powinna brzmieć InstructionCounter.");
        Assert.AreEqual(typeof(GlobalCounterContainer).FullName, fieldRef.DeclaringType.FullName, $"[{methodName}] Klasa pola powinna nazywać się '{typeof(GlobalCounterContainer).FullName}'.");
    }
}