using Mono.Cecil;

namespace CilInjection.Core.Tests.Fakes;

public static class CecilTestHelper
{
    /// <summary>
    /// Tworzy w pamięci fikcyjną referencję do pola, która może zostać użyta do zainicjalizowania weavera.
    /// </summary>
    public static FieldReference CreateDummyFieldReference(string typeName = "TestType", string fieldName = "TestField")
    {
        var assembly = AssemblyDefinition.CreateAssembly(
            new AssemblyNameDefinition("TestAssembly", new Version(1, 0, 0, 0)), 
            "TestModule", 
            ModuleKind.Dll);

        var module = assembly.MainModule;
        
        var typeDef = new TypeDefinition("TestNamespace", typeName, TypeAttributes.Public, module.TypeSystem.Object);
        module.Types.Add(typeDef);

        var fieldDef = new FieldDefinition(fieldName, FieldAttributes.Public | FieldAttributes.Static, module.TypeSystem.Int64);
        typeDef.Fields.Add(fieldDef);

        return fieldDef;
    }
}