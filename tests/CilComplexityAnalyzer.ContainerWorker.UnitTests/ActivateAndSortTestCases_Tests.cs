namespace CilComplexityAnalyzer.ContainerWorker.UnitTests;

[TestClass]
public class ActivateAndSortTestCases_Tests
{
    private class TC1 { public int TestNumber() => 2; }
    private class TC2 { public int TestNumber() => 0; }
    private class TC3 { public int TestNumber() => 3; }
    private class TC4 { public int TestNumber() => 1; }

    [TestMethod]
    public void HappyPath_ShouldSortAndReturnInstances()
    {
        // Arrange
        Type[] types =
        [
            typeof(TC1),
            typeof(TC2),
            typeof(TC3),
            typeof(TC4),
        ];
        
        // Act
        var result = Program.ActivateAndSortTestCases(types);
        
        // Assert
        var resultArray = result.ToArray();
        Assert.IsInstanceOfType<TC2>(resultArray[0]);
        Assert.IsInstanceOfType<TC4>(resultArray[1]);
        Assert.IsInstanceOfType<TC1>(resultArray[2]);
        Assert.IsInstanceOfType<TC3>(resultArray[3]);
    }
}