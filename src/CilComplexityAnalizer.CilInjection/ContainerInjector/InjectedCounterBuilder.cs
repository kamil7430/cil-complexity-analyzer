using Mono.Cecil;
using Mono.Cecil.Cil;
using CilComplexityAnalyzer.Contract;

namespace CilComplexityAnalyzer.CilInjection;

internal static class InjectedCounterBuilder
{
    /// <summary>
    /// Tworzy lub pobiera istniejącą klasę kontenera wraz z polem i metodami (Get/Reset).
    /// Jedyne źródło prawdy dla tworzenia struktury licznika w CIL.
    /// </summary>
    public static InjectedCounterDescriptor EnsureInjectedCounter(ModuleDefinition module)
    {
        var existingType = module.Types.FirstOrDefault(t => t.Name == InstrumentationContract.Symbols.ContainerTypeName);
        if (existingType != null)
        {
            return ResolveExisting(existingType);
        }

        // Tworzenie statycznej klasy kontenera
        var containerType = new TypeDefinition(
            "",
            InstrumentationContract.Symbols.ContainerTypeName,
            TypeAttributes.Public | TypeAttributes.Abstract | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit,
            module.TypeSystem.Object);

        // Tworzenie statycznego pola
        var counterField = new FieldDefinition(
            InstrumentationContract.Symbols.CounterFieldName,
            FieldAttributes.Public | FieldAttributes.Static,
            module.TypeSystem.Int64);

        containerType.Fields.Add(counterField);

        // 3. Tworzenie metody GetCounter() -> long
        var getMethod = CreateGetCounterMethod(module, counterField);
        containerType.Methods.Add(getMethod);

        // 4. Tworzenie metody ResetCounter() -> void
        var resetMethod = CreateResetCounterMethod(module, counterField);
        containerType.Methods.Add(resetMethod);

        // 5. Dodanie gotowej klasy do modułu
        module.Types.Add(containerType);

        return new InjectedCounterDescriptor(containerType, counterField, getMethod, resetMethod);
    }

    private static MethodDefinition CreateGetCounterMethod(ModuleDefinition module, FieldDefinition counterField)
    {
        var getMethod = new MethodDefinition(
            InstrumentationContract.Symbols.GetCounterMethodName,
            MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig,
            module.TypeSystem.Int64);

        var il = getMethod.Body.GetILProcessor();
        il.Emit(OpCodes.Ldsfld, counterField); 
        il.Emit(OpCodes.Ret);                 

        return getMethod;
    }

    private static MethodDefinition CreateResetCounterMethod(ModuleDefinition module, FieldDefinition counterField)
    {
        var resetMethod = new MethodDefinition(
            InstrumentationContract.Symbols.ResetCounterMethodName,
            MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig,
            module.TypeSystem.Void);

        var il = resetMethod.Body.GetILProcessor();
        il.Emit(OpCodes.Ldc_I8, 0L);           
        il.Emit(OpCodes.Stsfld, counterField); 
        il.Emit(OpCodes.Ret);                  

        return resetMethod;
    }

    private static InjectedCounterDescriptor ResolveExisting(TypeDefinition containerType)
    {
        var field = containerType.Fields.First(f => f.Name == InstrumentationContract.Symbols.CounterFieldName);
        var getMethod = containerType.Methods.First(m => m.Name == InstrumentationContract.Symbols.GetCounterMethodName);
        var resetMethod = containerType.Methods.First(m => m.Name == InstrumentationContract.Symbols.ResetCounterMethodName);

        return new InjectedCounterDescriptor(containerType, field, getMethod, resetMethod);
    }
}