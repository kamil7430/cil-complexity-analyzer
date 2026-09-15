using CilComplexityAnalyzer.Contract;
using Microsoft.Extensions.Logging;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

namespace CilComplexityAnalyzer.CilInjection;

public static class InstrumentationRegistry
{
    // =========================================================================
    // SYMBOLE I NAZWY 
    // =========================================================================
    public static class Symbols
    {
        public const string ContainerTypeName = "<GlobalCounterContainer>";
        public const string CounterFieldName = "__InstructionCounter";
        public const string CustomCheckerAssemblyName = "CilComplexityAnalyzer.Runtime";
    }

    // =========================================================================
    // DEFINICJA TYPU I POLA (Jednolite źródło struktury)
    // =========================================================================
    public static class Container
    {
        /// <summary>
        /// Tworzy nową statyczną klasę kontenera oraz pole zliczające w podanym module (lub pobiera istniejące).
        /// </summary>
        public static FieldDefinition EnsureGlobalCounterField(ModuleDefinition module)
        {
            // 1. Sprawdź, czy kontener już został wstrzyknięty
            var existingContainer = module.Types.FirstOrDefault(t => t.Name == Symbols.ContainerTypeName);
            if (existingContainer != null)
            {
                var existingField = existingContainer.Fields.FirstOrDefault(f => f.Name == Symbols.CounterFieldName);
                if (existingField != null)
                {
                    return existingField;
                }
            }

            // 2. Tworzenie klasy statycznej (Public + Abstract + Sealed = static w C#)
            var containerType = new TypeDefinition(
                "",
                Symbols.ContainerTypeName,
                TypeAttributes.Public | TypeAttributes.Abstract | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit,
                module.TypeSystem.Object);

            // 3. Tworzenie statycznego pola (public static long __InstructionCounter)
            var counterField = new FieldDefinition(
                Symbols.CounterFieldName,
                FieldAttributes.Public | FieldAttributes.Static,
                module.TypeSystem.Int64);

            // 4. Rejestracja w module CIL
            containerType.Fields.Add(counterField);
            module.Types.Add(containerType);

            return counterField;
        }
    }
    
    // =========================================================================
    // FABRYKA SEKWENCJI CIL 
    // =========================================================================
    public static class Sequences
    {
        private static readonly FieldReference DummyField = new("__dummy", null);

        /// <summary>
        /// Tworzy sekwencję CIL zwiększającą globalny licznik instrukcji o podaną wartość.
        /// Kod CIL odpowiadający: GlobalCounterContainer.__InstructionCounter += amount;
        /// </summary>
        public static Instruction[] CreateIncrementCounter(FieldReference counterField, long amount = 1)
        {
            return new[]
            {
                Instruction.Create(OpCodes.Ldsfld, counterField), // [counter]
                Instruction.Create(OpCodes.Ldc_I8, amount), // [counter, amount]
                Instruction.Create(OpCodes.Add), // [counter + amount]
                Instruction.Create(OpCodes.Stsfld, counterField) // []
            };
        }

        /// <summary>
        /// Długość wstrzykiwanej sekwencji wyliczana dynamicznie z faktycznej liczby zwracanych instrukcji.
        /// </summary>
        public static int InjectedSequenceLength { get; } = CreateIncrementCounter(DummyField).Length;
    }
}