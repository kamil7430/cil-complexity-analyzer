using System.Collections.Generic;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CilInjection.Core.Abstractions;

namespace Counter.Provider.InstructionCostCounter;

public static class InstructionCostAnalyzer
{
    internal static IEnumerable<Instruction> GetCostInstructions(IReadOnlyInstructionContext context)
    {
        var opCode = context.OpCode;

        // --- 1. LINIOWY DYNAMICZNY (Rozmiar znany tylko w Runtime na szczycie stosu) ---
        // newarr   - stos: [..., liczba_elementow]
        // localloc - stos: [..., liczba_bajtow]
        // cpblk    - stos: [..., adres_docelowy, adres_zrodlowy, liczba_bajtow]
        // initblk  - stos: [..., adres_docelowy, wartosc, liczba_bajtow]
        if (opCode == OpCodes.Newarr || 
            opCode == OpCodes.Localloc ||
            opCode == OpCodes.Cpblk || 
            opCode == OpCodes.Initblk)
        {
            return GenerateLinearDynamicCost();
        }

        // --- 2. LINIOWY STATYCZNY (Rozmiar znany teraz z Operandu) ---
        // Ładowanie stringów (koszt alokacji i przekopiowania znaków)
        if (opCode == OpCodes.Ldstr && context.Operand is string str)
        {
            long stringCost = 1L + (str.Length * 2); // C# używa UTF-16 (2 bajty na znak)
            return GenerateConstantCost(stringCost);
        }

        // Operacje na strukturach (ValueTypes) - kopiowanie i alokacja w pamięci
        if (opCode == OpCodes.Ldobj || 
            opCode == OpCodes.Stobj || 
            opCode == OpCodes.Cpobj || 
            opCode == OpCodes.Initobj ||
            opCode == OpCodes.Box || 
            opCode == OpCodes.Unbox_Any)
        {
            if (context.Operand is TypeReference typeRef)
            {
                long typeSizeCost = 1L + EstimateStaticTypeSize(typeRef);
                return GenerateConstantCost(typeSizeCost);
            }
        }

        // --- 3. CZAS STAŁY (Domyślny) ---
        // Instrukcje Call są traktowane jako O(1), bo wstrzykujemy kod do wnętrza każdej wywoływanej metody.
        return GenerateConstantCost(1L);
    }

    /// <summary>
    /// Generuje instrukcje wrzucające stałą wartość (Int64) na stos CIL.
    /// </summary>
    private static IEnumerable<Instruction> GenerateConstantCost(long cost)
    {
        yield return Instruction.Create(OpCodes.Ldc_I8, cost);
    }

    /// <summary>
    /// Generuje instrukcje pobierające wartość ze szczytu stosu w locie (Runtime).
    /// </summary>
    private static IEnumerable<Instruction> GenerateLinearDynamicCost()
    {
        // Klonujemy wartość ze szczytu stosu (rozmiar)
        yield return Instruction.Create(OpCodes.Dup);
        
        // Konwersja na Int64, aby móc dodać do naszego globalnego licznika (I8)
        yield return Instruction.Create(OpCodes.Conv_I8);
        
        // Zabezpieczenie przed ujemnymi wartościami lub zerem,
        // dodajemy +1 jako bazowy koszt samej operacji, a następnie dodajemy sklonowany rozmiar.
        yield return Instruction.Create(OpCodes.Ldc_I8, 1L);
        yield return Instruction.Create(OpCodes.Add);
    }

    /// <summary>
    /// Estymuje koszt bazując na fizycznym rozmiarze typu w pamięci.
    /// Zlicza rekurencyjnie rozmiary pól dla niestandardowych struktur (Custom ValueTypes).
    /// </summary>
    private static long EstimateStaticTypeSize(TypeReference typeReference, HashSet<string>? visitedTypes = null)
    {
        // Ochrona przed wskaźnikami, referencjami i parametrami generycznymi (T)
        // W takich przypadkach przenosimy tylko adres pamięci.
        if (typeReference.IsPointer || typeReference.IsByReference || typeReference.IsPinned || typeReference.IsArray)
        {
            return 8; // Wskaźnik
        }

        if (typeReference.IsGenericParameter)
        {
            return 8; 
        }

        // Próba oszacowania rozmiaru z metadanych wbudowanych (primitives)
        if (typeReference.IsPrimitive)
        {
            return typeReference.MetadataType switch
            {
                MetadataType.Boolean or MetadataType.Byte or MetadataType.SByte => 1,
                MetadataType.Char or MetadataType.Int16 or MetadataType.UInt16 => 2,
                MetadataType.Int32 or MetadataType.UInt32 or MetadataType.Single => 4,
                MetadataType.Int64 or MetadataType.UInt64 or MetadataType.Double => 8,
                MetadataType.IntPtr or MetadataType.UIntPtr => 8,
                _ => 4
            };
        }
        
        // Dla typów referencyjnych (klasy, interfejsy, stringi) 
        // operacje takie jak Ldobj/Stobj przenoszą tylko wskaźnik.
        if (!typeReference.IsValueType)
        {
            return 8; 
        }

        // --- Złożone struktury (Custom ValueTypes) ---

        // Zbiór odwiedzonych typów
        visitedTypes ??= new HashSet<string>();
        
        if (!visitedTypes.Add(typeReference.FullName))
        {
            return 0; 
        }

        // Rozwinięcie referencji do pełnej definicji typu w celu analizy jego pól
        TypeDefinition? typeDef = typeReference.Resolve();
        if (typeDef == null)
        {
            return 16; 
        }

        // Optymalizacja: Jeśli struktura ma jawnie zdefiniowany rozmiar (np. przez [StructLayout(Size = X)])
        if (typeDef.ClassSize > 0)
        {
            return typeDef.ClassSize;
        }

        // Enumy: Z technicznego punktu widzenia to struktury. Ich rozmiar definiuje jedno specjalne pole.
        if (typeDef.IsEnum)
        {
            var underlyingField = typeDef.Fields.FirstOrDefault(f => !f.IsStatic);
            return underlyingField != null ? EstimateStaticTypeSize(underlyingField.FieldType, visitedTypes) : 4;
        }

        // Właściwa rekurencja: sumujemy rozmiary wszystkich pól INSTANCYJNYCH
        long totalSize = 0;
        foreach (var field in typeDef.Fields)
        {
            // Statyczne pola nie wliczają się do fizycznego rozmiaru instancji struktury
            if (!field.IsStatic)
            {
                totalSize += EstimateStaticTypeSize(field.FieldType, visitedTypes);
            }
        }

        // Puste struktury w C# i tak zajmują minimum 1 bajt pamięci
        return totalSize > 0 ? totalSize : 1;
    }
}