using System.Reflection;
using System.Runtime.Loader;
using CilComplexityAnalyzer.LibCilInjection.Tests.Common;
using CilComplexityAnalyzer.TestExecutor;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace CilComplexityAnalyzer.LibCilInjection.Tests;

[TestClass]
public class ExceptionFilterInjectionTests
{
    private const string ExceptionFilterSource = """
        namespace DummyNamespace;

        public class FilteredCatcher
        {
            public string Handle(int code)
            {
                try
                {
                    throw new System.InvalidOperationException($"code:{code}");
                }
                catch (System.InvalidOperationException ex) when (ex.Message.Contains("42"))
                {
                    return "filtered-42";
                }
                catch (System.InvalidOperationException)
                {
                    return "generic";
                }
            }
        }
        """;

      [TestMethod]
    public void Injector_ShouldPreserveExceptionFilterSemantics_WhenFilterMatches()
    {
        byte[] assemblyBytes = DynamicSourceCompiler.CompileSource(ExceptionFilterSource);
 
        using var sandbox = InstrumentedSandbox.Create(
            assemblyBytes, nameof(Injector_ShouldPreserveExceptionFilterSemantics_WhenFilterMatches));
 
        var catcherType = sandbox.Assembly.GetType("DummyNamespace.FilteredCatcher")!;
        var instance = Activator.CreateInstance(catcherType)!;
        var handleMethod = catcherType.GetMethod("Handle")!;
 
        sandbox.Counter.ResetCounter();
 
        var result = handleMethod.Invoke(instance, new object[] { 42 });
 
        Assert.AreEqual("filtered-42", result);
 
        long executedInstructions = sandbox.Counter.GetCounter();
        Assert.IsTrue(executedInstructions > 0,
            $"Licznik CIL powinien zarejestrować wykonanie instrukcji wewnątrz filtra/handlera, a wyniósł: {executedInstructions}");
    }
 
    [TestMethod]
    public void Injector_ShouldPreserveExceptionFilterSemantics_WhenFilterFailsAndFallsBackToGenericCatch()
    {
        byte[] assemblyBytes = DynamicSourceCompiler.CompileSource(ExceptionFilterSource);
 
        using var sandbox = InstrumentedSandbox.Create(
            assemblyBytes, nameof(Injector_ShouldPreserveExceptionFilterSemantics_WhenFilterFailsAndFallsBackToGenericCatch));
 
        var catcherType = sandbox.Assembly.GetType("DummyNamespace.FilteredCatcher")!;
        var instance = Activator.CreateInstance(catcherType)!;
        var handleMethod = catcherType.GetMethod("Handle")!;
 
        sandbox.Counter.ResetCounter();
 
        var result = handleMethod.Invoke(instance, new object[] { 1 });
 
        Assert.AreEqual("generic", result);
 
        long executedInstructions = sandbox.Counter.GetCounter();
        Assert.IsTrue(executedInstructions > 0,
            $"Licznik CIL powinien zarejestrować wykonanie instrukcji, a wyniósł: {executedInstructions}");
    }



}