namespace CilInjection.Core.Utils;

using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

internal static class CoreLibTypeCloner
{
    public static bool IsPrivateCoreLib(ModuleDefinition module) =>
        module.Assembly?.Name?.Name == "System.Private.CoreLib" ||
        (module.AssemblyReferences.Count == 0 && module.GetType("System", "Object") is not null);

    public static TypeDefinition CloneTypeIntoCoreLib(
        ModuleDefinition coreLibModule,
        Type templateType,
        string targetNamespace,
        string targetTypeName)
    {
        using var sourceModule = ModuleDefinition.ReadModule(templateType.Assembly.Location);
        var sourceType = sourceModule.GetType(templateType.FullName!)
            ?? throw new InvalidOperationException($"Nie znaleziono typu '{templateType.FullName}' w '{templateType.Assembly.Location}'.");

        var clonedType = new TypeDefinition(
            targetNamespace,
            targetTypeName,
            sourceType.Attributes,
            ResolveCoreLibType(coreLibModule, sourceType.BaseType));

        // Klonowanie pól
        var fieldMap = new Dictionary<FieldDefinition, FieldDefinition>();
        foreach (var sourceField in sourceType.Fields)
        {
            var clonedField = new FieldDefinition(
                sourceField.Name,
                sourceField.Attributes,
                ResolveCoreLibType(coreLibModule, sourceField.FieldType));

            CloneCustomAttributes(coreLibModule, sourceField, clonedField);
            clonedType.Fields.Add(clonedField);
            fieldMap[sourceField] = clonedField;
        }

        // Klonowanie sygnatur metod
        var methodMap = new Dictionary<MethodDefinition, MethodDefinition>();
        foreach (var sourceMethod in sourceType.Methods)
        {
            var clonedMethod = new MethodDefinition(
                sourceMethod.Name,
                sourceMethod.Attributes,
                ResolveCoreLibType(coreLibModule, sourceMethod.ReturnType));

            foreach (var param in sourceMethod.Parameters)
            {
                clonedMethod.Parameters.Add(new ParameterDefinition(
                    param.Name,
                    param.Attributes,
                    ResolveCoreLibType(coreLibModule, param.ParameterType)));
            }

            CloneCustomAttributes(coreLibModule, sourceMethod, clonedMethod);
            clonedType.Methods.Add(clonedMethod);
            methodMap[sourceMethod] = clonedMethod;
        }

        // Klonowanie ciał metod (instrukcji CIL)
        foreach (var (sourceMethod, clonedMethod) in methodMap)
        {
            if (sourceMethod.HasBody)
            {
                CloneMethodBody(coreLibModule, sourceMethod, clonedMethod, fieldMap, methodMap);
            }
        }

        coreLibModule.Types.Add(clonedType);
        return clonedType;
    }

    private static void CloneMethodBody(
        ModuleDefinition coreLibModule,
        MethodDefinition source,
        MethodDefinition target,
        Dictionary<FieldDefinition, FieldDefinition> fieldMap,
        Dictionary<MethodDefinition, MethodDefinition> methodMap)
    {
        var sourceBody = source.Body;
        var targetBody = target.Body;
        targetBody.InitLocals = sourceBody.InitLocals;

        // Mapowanie zmiennych lokalnych
        var variableMap = new Dictionary<VariableDefinition, VariableDefinition>();
        foreach (var sourceVar in sourceBody.Variables)
        {
            var clonedVar = new VariableDefinition(ResolveCoreLibType(coreLibModule, sourceVar.VariableType));
            targetBody.Variables.Add(clonedVar);
            variableMap[sourceVar] = clonedVar;
        }

        // Mapowanie parametrów metody
        var parameterMap = new Dictionary<ParameterDefinition, ParameterDefinition>();
        for (int i = 0; i < source.Parameters.Count; i++)
        {
            parameterMap[source.Parameters[i]] = target.Parameters[i];
        }

        // Klonowanie instrukcji
        var instructionMap = new Dictionary<Instruction, Instruction>();
        var il = targetBody.GetILProcessor();

        foreach (var instr in sourceBody.Instructions)
        {
            var clonedInstr = CloneInstruction(coreLibModule, instr, fieldMap, methodMap, variableMap, parameterMap);
            instructionMap[instr] = clonedInstr;
            il.Append(clonedInstr);
        }

        // Aktualizacja celów skoków 
        foreach (var instr in targetBody.Instructions)
        {
            if (instr.Operand is Instruction targetInstr && instructionMap.TryGetValue(targetInstr, out var mappedTarget))
            {
                instr.Operand = mappedTarget;
            }
            else if (instr.Operand is Instruction[] targets)
            {
                instr.Operand = targets.Select(t => instructionMap[t]).ToArray();
            }
        }
    }

    private static Instruction CloneInstruction(
        ModuleDefinition coreLibModule,
        Instruction instr,
        Dictionary<FieldDefinition, FieldDefinition> fieldMap,
        Dictionary<MethodDefinition, MethodDefinition> methodMap,
        Dictionary<VariableDefinition, VariableDefinition> variableMap,
        Dictionary<ParameterDefinition, ParameterDefinition> parameterMap)
    {
        return instr.Operand switch
        {
            null => Instruction.Create(instr.OpCode),
            int i => Instruction.Create(instr.OpCode, i),
            long l => Instruction.Create(instr.OpCode, l),
            float f => Instruction.Create(instr.OpCode, f),
            double d => Instruction.Create(instr.OpCode, d),
            string s => Instruction.Create(instr.OpCode, s),
            byte b => Instruction.Create(instr.OpCode, b),
            sbyte sb => Instruction.Create(instr.OpCode, sb),
            VariableDefinition v => Instruction.Create(instr.OpCode, variableMap[v]),
            ParameterDefinition p => Instruction.Create(instr.OpCode, parameterMap[p]),
            Instruction target => Instruction.Create(instr.OpCode, target),
            Instruction[] targets => Instruction.Create(instr.OpCode, targets),
            FieldReference fieldRef => Instruction.Create(instr.OpCode, ResolveCoreLibField(coreLibModule, fieldRef, fieldMap)),
            MethodReference methodRef => Instruction.Create(instr.OpCode, ResolveCoreLibMethod(coreLibModule, methodRef, methodMap)),
            TypeReference typeRef => Instruction.Create(instr.OpCode, ResolveCoreLibType(coreLibModule, typeRef)),
            _ => throw new NotSupportedException($"Nieobsługiwany operand instrukcji CIL: {instr.Operand.GetType().Name}")
        };
    }

    private static TypeReference ResolveCoreLibType(ModuleDefinition coreLibModule, TypeReference? typeRef)
    {
        if (typeRef is null)
            return null!;

        if (typeRef is ArrayType arrayType)
            return new ArrayType(ResolveCoreLibType(coreLibModule, arrayType.ElementType), arrayType.Rank);

        if (typeRef is ByReferenceType byRefType)
            return new ByReferenceType(ResolveCoreLibType(coreLibModule, byRefType.ElementType));

        var localType = coreLibModule.GetType(typeRef.Namespace, typeRef.Name);
        if (localType is not null)
            return localType;

        throw new InvalidOperationException(
            $"Typ '{typeRef.FullName}' użyty w klasie wzorcowej nie istnieje wewnątrz System.Private.CoreLib.");
    }

    private static FieldReference ResolveCoreLibField(
        ModuleDefinition coreLibModule,
        FieldReference fieldRef,
        Dictionary<FieldDefinition, FieldDefinition> fieldMap)
    {
        var resolvedDef = fieldRef.Resolve();
        if (resolvedDef is not null && fieldMap.TryGetValue(resolvedDef, out var localClonedField))
            return localClonedField;

        var declaringType = ResolveCoreLibType(coreLibModule, fieldRef.DeclaringType).Resolve();
        return declaringType.Fields.First(f => f.Name == fieldRef.Name);
    }

    private static MethodReference ResolveCoreLibMethod(
        ModuleDefinition coreLibModule,
        MethodReference methodRef,
        Dictionary<MethodDefinition, MethodDefinition> methodMap)
    {
        var resolvedDef = methodRef.Resolve();
        if (resolvedDef is not null && methodMap.TryGetValue(resolvedDef, out var localClonedMethod))
            return localClonedMethod;

        var declaringType = ResolveCoreLibType(coreLibModule, methodRef.DeclaringType).Resolve();
        return declaringType.Methods.First(m =>
            m.Name == methodRef.Name &&
            m.Parameters.Count == methodRef.Parameters.Count &&
            m.Parameters.Select(p => p.ParameterType.FullName)
                .SequenceEqual(methodRef.Parameters.Select(p => p.ParameterType.FullName)));
    }

    private static void CloneCustomAttributes(
        ModuleDefinition coreLibModule,
        ICustomAttributeProvider source,
        ICustomAttributeProvider target)
    {
        foreach (var attr in source.CustomAttributes)
        {
            var attrType = ResolveCoreLibType(coreLibModule, attr.AttributeType).Resolve();
            var ctor = attrType.Methods.First(m =>
                m.IsConstructor &&
                m.Parameters.Count == attr.Constructor.Parameters.Count);

            target.CustomAttributes.Add(new CustomAttribute(ctor));
        }
    }
}