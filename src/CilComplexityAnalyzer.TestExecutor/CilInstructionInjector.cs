using CilComplexityAnalyzer.Contract;
using Microsoft.Extensions.Logging;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

namespace CilComplexityAnalyzer.TestExecutor;

/// <summary>
/// Nazwy licznika. Licznik NIE ma nazwy zaczynającej się od '<', żeby filtry nie
/// myliły go z typami generowanymi przez kompilator (które MUSIMY liczyć: lambdy, iteratory).
/// </summary>
internal static class CounterNames
{
    public const string Namespace = "CilCounter";
    public const string Type = "GlobalCounter";
    public const string FullName = Namespace + "." + Type;
    public const string Field = "Count";
    public const string Get = "GetInstructionCount";
    public const string Reset = "ResetInstructionCount";
}

internal static class CilInstructionInjector
{
    internal const string CoreLibName = "System.Private.CoreLib";

    internal static TestSuite InjectCilToStudentSolution(this TestSuite testSuite)
    {
        testSuite.Logger()?.LogInformation($"[{testSuite.Name}] Beginning CIL instruction injection in student solution.");

        if (testSuite.StudentSolutionAssemblyBytes is null)
            throw new InvalidOperationException("StudentSolutionAssemblyBytes is null! Ensure Compilation succeeded before injecting CIL.");

        testSuite.StudentSolutionAssemblyBytes = InjectCilToAssemblyBytes(testSuite.StudentSolutionAssemblyBytes);

        testSuite.Logger()?.LogInformation($"[{testSuite.Name}] CIL instruction injection completed.");
        return testSuite;
    }

    /// <param name="assemblyBytes">Surowe bajty DLL (musi być IL-only, nie R2R).</param>
    /// <param name="counterHostAssembly">
    /// Prosta nazwa assembly, które FIZYCZNIE zawiera licznik.
    /// Domyślnie System.Private.CoreLib - wtedy każdy inny assembly tylko się do niego odwołuje,
    /// a CoreLib nigdy nie musi "wołać w górę" do assembly studenta (to właśnie powodowało cykl).
    /// Wartość "StudentWasmApp" przywraca stare zachowanie (licznik w assembly studenta) -
    /// wtedy NIE wolno instrumentować CoreLib.
    /// </param>
    /// <param name="extraRoots">Dodatkowe typy CoreLib do instrumentacji (np. "System.Array") - opcjonalne, ostrożnie.</param>
    internal static byte[] InjectCilToAssemblyBytes(
        byte[] assemblyBytes,
        string counterHostAssembly = CoreLibName,
        IEnumerable<string>? extraRoots = null,
        bool verbose = false)
    {
        using var inputStream = new MemoryStream(assemblyBytes);
        var assemblyDef = AssemblyDefinition.ReadAssembly(inputStream);
        var module = assemblyDef.MainModule;
        var assemblyName = assemblyDef.Name.Name;
        
        if (assemblyName == counterHostAssembly && module.GetType(CounterNames.FullName) != null)
            throw new InvalidOperationException(
                $"'{assemblyName}' jest już zinstrumentowany (istnieje {CounterNames.FullName}). " +
                "Instrumentuj zawsze ORYGINALNE pliki, nie nadpisane.");

        FieldReference counterField = assemblyName == counterHostAssembly
            ? DefineCounter(module)
            : ImportCounter(module, counterHostAssembly);

        var filter = InjectionFilter.Create(assemblyName, module, extraRoots);

        int typeCount = 0, methodCount = 0;

        foreach (var type in GetAllTypesRecursively(module).ToList())
        {
            if (!filter.ShouldInstrumentType(type))
                continue;

            typeCount++;
            if (verbose) Console.WriteLine($"[INSTRUMENTED TYPE] {assemblyName} -> {type.FullName}");

            foreach (var method in type.Methods.ToList())
            {
                if (!filter.ShouldInstrumentMethod(method))
                    continue;

                InjectCounter(method, counterField);
                methodCount++;
            }
        }

        Console.WriteLine($"[SUMMARY] '{assemblyName}': {typeCount} typow, {methodCount} metod zinstrumentowanych");

        using var outputStream = new MemoryStream();
        assemblyDef.Write(outputStream);
        return outputStream.ToArray();
    }

    /// <summary>Referencja do licznika zdefiniowanego w innym assembly (hoście).</summary>
    private static FieldReference ImportCounter(ModuleDefinition module, string hostAssembly)
    {
        var asmRef = module.AssemblyReferences.FirstOrDefault(a => a.Name == hostAssembly);
        if (asmRef == null)
        {
            asmRef = CreateAssemblyRef(hostAssembly);
            module.AssemblyReferences.Add(asmRef);
        }

        var containerRef = new TypeReference(CounterNames.Namespace, CounterNames.Type, module, asmRef);
        return new FieldReference(CounterNames.Field, module.TypeSystem.Int64, containerRef);
    }

    private static AssemblyNameReference CreateAssemblyRef(string host)
    {
        if (host == CoreLibName)
        {
            var n = typeof(object).Assembly.GetName();
            return new AssemblyNameReference(CoreLibName, n.Version) { PublicKeyToken = n.GetPublicKeyToken() };
        }

        return new AssemblyNameReference(host, new Version(1, 0, 0, 0));
    }

    /// <summary>
    /// Definiuje statyczną klasę CilCounter.GlobalCounter z polem Count (long) oraz
    /// metodami Get/Reset. Klasa NIE ma konstruktora statycznego -> ldsfld/stsfld nigdy
    /// nie wywołuje żadnego kodu inicjalizującego (ważne w CoreLib!).
    /// </summary>
    private static FieldDefinition DefineCounter(ModuleDefinition module)
    {
        var objectType = (TypeReference?)module.GetType("System.Object") ?? module.TypeSystem.Object;
        var int64Type  = (TypeReference?)module.GetType("System.Int64")  ?? module.TypeSystem.Int64;
        var voidType   = (TypeReference?)module.GetType("System.Void")   ?? module.TypeSystem.Void;

        var container = new TypeDefinition(
            CounterNames.Namespace,
            CounterNames.Type,
            TypeAttributes.Public | TypeAttributes.Abstract | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit,
            objectType);

        var field = new FieldDefinition(CounterNames.Field, FieldAttributes.Public | FieldAttributes.Static, int64Type);
        container.Fields.Add(field);

        var get = new MethodDefinition(CounterNames.Get,
            MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig, int64Type);
        var gIl = get.Body.GetILProcessor();
        gIl.Append(gIl.Create(OpCodes.Ldsfld, field));
        gIl.Append(gIl.Create(OpCodes.Ret));

        var reset = new MethodDefinition(CounterNames.Reset,
            MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig, voidType);
        var rIl = reset.Body.GetILProcessor();
        rIl.Append(rIl.Create(OpCodes.Ldc_I8, 0L));
        rIl.Append(rIl.Create(OpCodes.Stsfld, field));
        rIl.Append(rIl.Create(OpCodes.Ret));

        container.Methods.Add(get);
        container.Methods.Add(reset);
        module.Types.Add(container);

        return field;
    }

    private static void InjectCounter(MethodDefinition method, FieldReference counterField)
    {
        var body = method.Body;
        if (body.Instructions.Count == 0) return;

        body.SimplifyMacros();
        var il = body.GetILProcessor();
        var original = body.Instructions.ToList();

        var entryMap = new Dictionary<Instruction, Instruction>();
        Instruction? pendingPrefixBump = null;

        foreach (var instr in original)
        {
            if (IsPrefixInstruction(instr))
            {
                pendingPrefixBump ??= InsertCounterBump(il, counterField, instr);
                entryMap[instr] = pendingPrefixBump;
                continue;
            }

            if (pendingPrefixBump != null)
            {
                entryMap[instr] = pendingPrefixBump;
                pendingPrefixBump = null;
                continue;
            }

            entryMap[instr] = InsertCounterBump(il, counterField, instr);
        }

        RedirectAllBranches(body, entryMap);
        body.MaxStackSize += 2;
        body.OptimizeMacros();
    }

    private static Instruction InsertCounterBump(ILProcessor il, FieldReference counterField, Instruction before)
    {
        var loadCounter = il.Create(OpCodes.Ldsfld, counterField);
        il.InsertBefore(before, loadCounter);
        il.InsertBefore(before, il.Create(OpCodes.Ldc_I8, 1L));
        il.InsertBefore(before, il.Create(OpCodes.Add));
        il.InsertBefore(before, il.Create(OpCodes.Stsfld, counterField));
        return loadCounter;
    }

    private static void RedirectAllBranches(MethodBody body, Dictionary<Instruction, Instruction> entryMap)
    {
        foreach (var i in body.Instructions)
        {
            if (i.Operand is Instruction target && entryMap.TryGetValue(target, out var newTarget))
            {
                i.Operand = newTarget;
            }
            else if (i.Operand is Instruction[] targets)
            {
                for (int j = 0; j < targets.Length; j++)
                    if (entryMap.TryGetValue(targets[j], out var t))
                        targets[j] = t;
            }
        }

        if (!body.HasExceptionHandlers) return;

        foreach (var h in body.ExceptionHandlers)
        {
            if (h.TryStart     != null && entryMap.TryGetValue(h.TryStart,     out var a)) h.TryStart     = a;
            if (h.TryEnd       != null && entryMap.TryGetValue(h.TryEnd,       out var b)) h.TryEnd       = b;
            if (h.HandlerStart != null && entryMap.TryGetValue(h.HandlerStart, out var c)) h.HandlerStart = c;
            if (h.HandlerEnd   != null && entryMap.TryGetValue(h.HandlerEnd,   out var d)) h.HandlerEnd   = d;
            if (h.FilterStart  != null && entryMap.TryGetValue(h.FilterStart,  out var e)) h.FilterStart  = e;
        }
    }

    private static bool IsPrefixInstruction(Instruction instruction)
    {
        var code = instruction.OpCode.Code;
        return code == Code.Constrained || code == Code.Readonly || code == Code.Unaligned
            || code == Code.Volatile || code == Code.Tail;
    }

    private static IEnumerable<TypeDefinition> GetAllTypesRecursively(ModuleDefinition module)
    {
        foreach (var type in module.Types)
        {
            yield return type;
            foreach (var nested in GetNestedTypesRecursively(type))
                yield return nested;
        }
    }

    private static IEnumerable<TypeDefinition> GetNestedTypesRecursively(TypeDefinition type)
    {
        foreach (var nested in type.NestedTypes)
        {
            yield return nested;
            foreach (var deeper in GetNestedTypesRecursively(nested))
                yield return deeper;
        }
    }
}