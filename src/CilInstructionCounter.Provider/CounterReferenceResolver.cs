namespace CilComplexityAnalyzer.CilInjection;

using Mono.Cecil;
using CilComplexityAnalyzer.RunTime;

public static class CounterReferenceResolver
{
    /// <summary>
    /// Tworzy i importuje do docelowego modułu referencję do statycznego pola 
    /// InstructionCounter z zewnętrznej biblioteki CilInstructionCounter.RunTime.
    /// </summary>
    /// <param name="targetModule">Moduł CIL studenta lub biblioteki pomocniczej, który będzie instrumentowany.</param>
    /// <returns>Zaznaczony i zaimportowany FieldReference gotowy do użycia w instrukcjach ldsfld/stsfld.</returns>
    public static FieldReference ResolveAndImportCounterField(ModuleDefinition targetModule)
    {
        ArgumentNullException.ThrowIfNull(targetModule);

        // 1. Odczytujemy metadane zestawu RunTime bezpośrednio z typu C#
        var runtimeAssemblyName = typeof(GlobalCounterContainer).Assembly.GetName();

        var assemblyRef = new AssemblyNameReference(
            runtimeAssemblyName.Name!,
            runtimeAssemblyName.Version
        );

        // 2. Budujemy referencję do typu GlobalCounterContainer w zasięgu zewnętrznego Assembly
        var typeRef = new TypeReference(
            @namespace: typeof(GlobalCounterContainer).Namespace!,
            name: nameof(GlobalCounterContainer),
            module: targetModule,
            scope: assemblyRef
        );

        // 3. Budujemy referencję do pola InstructionCounter (jako Int64 / long)
        var fieldRef = new FieldReference(
            name: nameof(GlobalCounterContainer.InstructionCounter),
            fieldType: targetModule.TypeSystem.Int64,
            declaringType: typeRef
        );

        // 4. Importujemy referencję do nagłówka targetModule.
        // Mono.Cecil automatycznie doda 'AssemblyReference' do manifestu .dll, jeśli jeszcze go tam nie ma.
        return targetModule.ImportReference(fieldRef);
    }
}