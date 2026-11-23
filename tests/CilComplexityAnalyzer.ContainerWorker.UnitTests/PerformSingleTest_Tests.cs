using CilComplexityAnalyzer.Contract;

namespace CilComplexityAnalyzer.ContainerWorker.UnitTests;

[TestClass]
public class PerformSingleTest_Tests
{
    private class CorrectTestClass(int assertNo)
    {
        private int _no;
        public void Arrange() { _no = 15; }
        public void Act() { _no *= _no * _no; }
        public void Assert() { TestCase.IsTrue(_no == assertNo, "_no == assertNo"); }
    }

    [TestMethod]
    public void HappyPath_ShouldReturnSuccess()
    {
        var tc = new CorrectTestClass(3375);

        Utils.InvokePerformSingleTestAndCaptureException(tc, tr => { Assert.IsTrue(tr.Success); });
    }

    [TestMethod]
    public void WrongNumber_ShouldReturnFailure()
    {
        var tc = new CorrectTestClass(3370);

        Utils.InvokePerformSingleTestAndCaptureException(tc, tr =>
        {
            Assert.IsFalse(tr.Success);
            Assert.StartsWith("Assertion failed:", tr.Message);
        });
    }

    private class NoArrangeTestClass
    {
        public void Act() { }
        public void Assert() { }
    }
    
    private class NoActTestClass
    {
        public void Arrange() { }
        public void Assert() { }
    }
    
    private class NoAssertTestClass
    {
        public void Arrange() { }
        public void Act() { }
    }

    [TestMethod]
    [DataRow(typeof(NoArrangeTestClass), DisplayName = "MissingArrangeMethod_ShouldThrowAnException")]
    [DataRow(typeof(NoActTestClass), DisplayName = "MissingActMethod_ShouldThrowAnException")]
    [DataRow(typeof(NoAssertTestClass), DisplayName = "MissingAssertMethod_ShouldThrowAnException")]
    public void MissingMethod_ShouldThrowAnException(Type type)
    {
        var tc = Activator.CreateInstance(type);
        Assert.IsNotNull(tc);
        
        var action = () => Utils.InvokePerformSingleTestAndCaptureException(tc, _ => { });
        Assert.Throws<ContainerWorkerException>(action);
    }

    private class ArrangeExceptionTestClass
    {
        public void Arrange() { throw new Exception(); }
        public void Act() { }
        public void Assert() { }
    }

    [TestMethod]
    public void ThrowingExceptionInArrange_ShouldThrowException()
    {
        var tc = new ArrangeExceptionTestClass();
        
        var action = () => Utils.InvokePerformSingleTestAndCaptureException(tc, _ => { });
        Assert.Throws<ContainerWorkerException>(action);
    }

    private class ActExceptionTestClass
    {
        public void Arrange() { }
        public void Act() { throw new Exception(); }
        public void Assert() { }
    }

    [TestMethod]
    public void ThrowingExceptionInAct_ShouldReturnFailure()
    {
        var tc = new ActExceptionTestClass();
        
        Utils.InvokePerformSingleTestAndCaptureException(tc, tr =>
        {
            Assert.IsFalse(tr.Success);
            Assert.StartsWith("Unhandled exception in student code:", tr.Message);
        });
    }

    private class AssertExceptionTestClass
    {
        public void Arrange() { }
        public void Act() { }
        public void Assert() { throw new Exception(); }
    }

    [TestMethod]
    public void ThrowingExceptionInAssert_ShouldThrowException()
    {
        var tc = new AssertExceptionTestClass();
        
        var action = () => Utils.InvokePerformSingleTestAndCaptureException(tc, _ => { });
        Assert.Throws<ContainerWorkerException>(action);
    }
}