using CilComplexityAnalyzer.LibCilInjection.Tests.Common;
using Counter.Provider.InstructionCounter;

namespace CilComplexityAnalyzer.LibCilInjection.Tests.FixedTests.ASD;

[TestClass]
public class AsdLibraryExactInstructionTests
{
    [TestMethod]
    public void Asd_CoreComponent_ShouldProduceMeasurableInstructionCount()
    {
        var sampleTypeInAsd = typeof(global::ASD.Graphs.Graph);
        byte[] asdBytes = File.ReadAllBytes(sampleTypeInAsd.Assembly.Location);

        var strategy = new InstructionCounterStrategy();

        using var sandbox = InstrumentedSandbox.Create(
            asdBytes, 
            nameof(Asd_CoreComponent_ShouldProduceMeasurableInstructionCount), 
            strategy);

        var targetType = sandbox.Assembly.GetType("ASD.Graphs.Graph")!;
        var instance = Activator.CreateInstance(targetType, new object[] { 5 })!;
        
        var method = targetType.GetMethod("Degree", new[] { typeof(int) })!;

        sandbox.Counter.ResetCounter();
        
        object? result = method.Invoke(instance, new object[] { 0 });
        
        long exactInstructions = sandbox.Counter.GetCounter();

        Console.WriteLine($"[ASD DETERMINISTIC TEST] Liczba instrukcji CIL dla Degree(): {exactInstructions}");

        Assert.IsNotNull(result);
        Assert.IsTrue(exactInstructions > 0, 
            "Licznik CIL dla komponentu ASD zwrócił 0 — brak instrumentacji.");
        
        Assert.IsTrue(exactInstructions < 100, 
            "Metoda pobierająca stopień wierzchołka powinna być szybka i lekka.");
        Assert.AreEqual(16, exactInstructions, "Liczba instrukcji CIL dla AddEdge uległa zmianie!");
    }
}