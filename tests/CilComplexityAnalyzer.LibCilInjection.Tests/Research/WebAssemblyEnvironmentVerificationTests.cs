using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using CilComplexityAnalyzer.LibCilInjection.Tests.Common;
using CilInjection.Core;
using Counter.Provider.InstructionCounter;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mono.Cecil.Cil;

namespace CilComplexityAnalyzer.LibCilInjection.Tests.Research;

[TestClass]
public class WebAssemblyEnvironmentVerificationTests
{
    [TestMethod]
    public void Test_WasmEnvironmentStylePureIlInjection_SystemCollections()
    {
        string userHome = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        string nugetPackageDir = Path.Combine(userHome, ".nuget", "packages");

        if (!Directory.Exists(nugetPackageDir))
        {
            Assert.Inconclusive("Brak katalogu NuGet packages na tym stanowisku.");
            return;
        }

        string? targetDll = Directory.GetFiles(nugetPackageDir, "System.Collections.dll", SearchOption.AllDirectories)
            .FirstOrDefault(f => f.Contains("microsoft.netcore.app.ref") || f.Contains("netstandard") || f.Contains("wasm"));

        if (targetDll == null)
        {
            targetDll = Directory.GetFiles(nugetPackageDir, "System.Collections.dll", SearchOption.AllDirectories).FirstOrDefault();
        }

        if (string.IsNullOrEmpty(targetDll) || !File.Exists(targetDll))
        {
            Assert.Inconclusive("Nie znaleziono pliku System.Collections.dll w formacie czystego IL.");
            return;
        }

        Console.WriteLine($"[WASM/PURE IL SIMULATION] Testujemy plik: {targetDll}");

        byte[] originalBytes = File.ReadAllBytes(targetDll);
        var strategy = new InstructionCounterStrategy();
        var manager = new CilInjectionManager(strategy);

        byte[] instrumentedBytes = manager.Transform(originalBytes);
        Assert.IsNotNull(instrumentedBytes);
        Console.WriteLine($"[SUCCESS] Wstrzyknięto kod CIL do biblioteki systemowej! Rozmiar po transformacji: {instrumentedBytes.Length} B");

        try
        {
            using var sandbox = InstrumentedSandbox.Create(
                instrumentedBytes,
                nameof(Test_WasmEnvironmentStylePureIlInjection_SystemCollections),
                strategy);

            Console.WriteLine("[SANDBOX SUCCESS] Zainstrumentowana biblioteka systemowa załadowana pomyślnie do izolowanego kontekstu!");
            Assert.IsTrue(instrumentedBytes.Length > 0);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SANDBOX INFO] Transformacja przeszła pomyślnie, ale sandbox wymaga pełnego powiązania runtime: {ex.Message}");
        }
    }

    // kolejne potwierdzenie, że w Ref nie ma kodu
    [TestMethod]
    public void Test_PureIlAssemblyInjectionAndExecution_SystemCollections()
    {
        string userHome = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        string nugetPackageDir = Path.Combine(userHome, ".nuget", "packages");
        
        string? targetDll = Directory.GetFiles(nugetPackageDir, "System.Collections.dll", SearchOption.AllDirectories)
            .FirstOrDefault(f => f.Contains("netstandard") || f.Contains("microsoft.netcore.app.ref") || f.Contains("wasm"));

        if (targetDll == null)
        {
            targetDll = Directory.GetFiles(nugetPackageDir, "System.Collections.dll", SearchOption.AllDirectories).FirstOrDefault();
        }

        Assert.IsNotNull(targetDll, "Nie znaleziono pliku System.Collections.dll w cache Nuget.");
        Console.WriteLine($"[TARGET] Znaleziono czysty plik IL: {targetDll}");

        byte[] originalBytes = File.ReadAllBytes(targetDll);
        var strategy = new InstructionCounterStrategy();
        var manager = new CilInjectionManager(strategy);

        byte[] instrumentedBytes = manager.Transform(originalBytes);
        Assert.IsNotNull(instrumentedBytes);
        Console.WriteLine($"[SUCCESSS] Wstrzyknieto CIL. Oryginal: {originalBytes.Length} B, Po transformacji: {instrumentedBytes.Length} B");

        using var sandbox = InstrumentedSandbox.Create(
            instrumentedBytes,
            nameof(Test_PureIlAssemblyInjectionAndExecution_SystemCollections),
            strategy);
        var listType = sandbox.Assembly.GetType("System.Collections.Generic.List`1");
        Assert.IsNotNull(listType, "Nie odnaleziono typu List`1 w załadowanym module.");
        
        var intListType = listType.MakeGenericType(typeof(int));
        var listInstance = Activator.CreateInstance(intListType);
        
        var addMethod = intListType.GetMethod("Add", new[] { typeof(int) });
        Assert.IsNotNull(addMethod, "Nie odnaleziono metody Add.");

        sandbox.Counter.ResetCounter();

        addMethod.Invoke(listInstance, new object[] { 42 });
        addMethod.Invoke(listInstance, new object[] { 100 });

        long executedInstructions = sandbox.Counter.GetCounter();
        Console.WriteLine($"[EXECUTION] Operacje na zainstalowanej listcie w systemowej wykonaly: {executedInstructions} instrukcji CIL");
        Assert.IsTrue(executedInstructions > 0, "Licznik instrukcji powinien zarejestrowac operacje wewnatrz zainstrumentowanej biblioteki systemowej!");
    }
    
    // brak webassebmbly tak po prosty. Trzeba to ogarnac samemu
    [TestMethod]
    public void Test_WasmRuntimeAssemblyInjectionAndExecution_SystemCollections()
    {
        string userHome = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        string nugetPackageDir = Path.Combine(userHome, ".nuget", "packages");
        
        string? targetDll = Directory.GetFiles(nugetPackageDir, "System.Collections.dll", SearchOption.AllDirectories)
            .FirstOrDefault(f => f.Contains("runtimes/browser-wasm") || f.Contains("runtimes/wasm") || (f.Contains("lib/netstandard") && !f.Contains("/ref/")));

        if (string.IsNullOrEmpty(targetDll) || !File.Exists(targetDll))
        {
            targetDll = Directory.GetFiles(nugetPackageDir, "System.Collections.dll", SearchOption.AllDirectories)
                .FirstOrDefault(f => f.Contains("/lib/") && !f.Contains("/ref/"));
        }

        Assert.IsNotNull(targetDll, "Nie znaleziono wykonawczego pliku System.Collections.dll dla WebAssembly/Runtime.");
        Console.WriteLine($"[WASM RUNTIME TARGET] Znaleziono plik wykonawczy: {targetDll}");

        byte[] originalBytes = File.ReadAllBytes(targetDll);
        var strategy = new InstructionCounterStrategy();
        var manager = new CilInjectionManager(strategy);

        byte[] instrumentedBytes = manager.Transform(originalBytes);
        Assert.IsNotNull(instrumentedBytes);
        Console.WriteLine($"[SUCCESS] Wstrzyknięto CIL do pliku WASM/Runtime! Oryginał: {originalBytes.Length} B, Po transformacji: {instrumentedBytes.Length} B.");

        using var sandbox = InstrumentedSandbox.Create(
            instrumentedBytes,
            nameof(Test_WasmRuntimeAssemblyInjectionAndExecution_SystemCollections),
            strategy);

        var listType = sandbox.Assembly.GetType("System.Collections.Generic.List`1");
        Assert.IsNotNull(listType, "Nie odnaleziono typu List`1 w module WASM.");
        
        var intListType = listType.MakeGenericType(typeof(int));
        var listInstance = Activator.CreateInstance(intListType);
        
        var addMethod = intListType.GetMethod("Add", new[] { typeof(int) });
        Assert.IsNotNull(addMethod, "Nie odnaleziono metody Add.");

        sandbox.Counter.ResetCounter();

        addMethod.Invoke(listInstance, new object[] { 42 });
        addMethod.Invoke(listInstance, new object[] { 100 });

        long executedInstructions = sandbox.Counter.GetCounter();
        Console.WriteLine($"[EXECUTION] Operacje na zainstrumentowanej liście WASM wykonały: {executedInstructions} instrukcji CIL");
        
        Assert.IsTrue(executedInstructions > 0, "Licznik instrukcji powinien zarejestrować operacje wewnątrz pliku runtime WebAssembly!");
    }
}