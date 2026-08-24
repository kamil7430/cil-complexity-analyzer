using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
namespace CilComplexityAnalyzer.TestGenerator.Tests;
public partial class Lab1Evaluation
{
    [TestClass]
    public partial class Student1
    {
        [TestMethod]
        public void Case1()
        {
            var ok = FakeExecutor.Run(@"Submissions/student1.cs", "Solve", "Case1", new object[] { 1, 2 }, new object[] { 3 });
            Assert.IsTrue(ok);
        }
        [TestMethod]
        public void Case2()
        {
            var ok = FakeExecutor.Run(@"Submissions/student1.cs", "Solve", "Case2", new object[] { 2, 2 }, new object[] { 4 });
            Assert.IsTrue(ok);
        }
        [TestMethod]
        public void Case3()
        {
            var ok = FakeExecutor.Run(@"Submissions/student1.cs", "Solve", "Case3", new object[] { 3, 3 }, new object[] { 6 });
            Assert.IsTrue(ok);
        }
        [TestMethod]
        public void Case4()
        {
            var ok = FakeExecutor.Run(@"Submissions/student1.cs", "Solve", "Case4", new object[] { 3, 3 }, new object[] { 6 });
            Assert.IsTrue(ok);
        }
        [TestMethod]
        public void Case5()
        {
            var ok = FakeExecutor.Run(@"Submissions/student1.cs", "Solve", "Case5", new object[] { 3, 3 }, new object[] { 6 });
            Assert.IsTrue(ok);
        }
    }
    [TestClass]
    public partial class Student2
    {
        [TestMethod]
        public void Case1()
        {
            var ok = FakeExecutor.Run(@"Submissions/student1.cs", "Solve", "Case1", new object[] { 1, 2 }, new object[] { 3 });
            Assert.IsTrue(ok);
        }
        [TestMethod]
        public void Case2()
        {
            var ok = FakeExecutor.Run(@"Submissions/student1.cs", "Solve", "Case2", new object[] { 2, 2 }, new object[] { 4 });
            Assert.IsTrue(ok);
        }
        [TestMethod]
        public void Case3()
        {
            var ok = FakeExecutor.Run(@"Submissions/student1.cs", "Solve", "Case3", new object[] { 3, 3 }, new object[] { 6 });
            Assert.IsTrue(ok);
        }
        [TestMethod]
        public void Case4()
        {
            var ok = FakeExecutor.Run(@"Submissions/student1.cs", "Solve", "Case4", new object[] { 3, 3 }, new object[] { 6 });
            Assert.IsTrue(ok);
        }
        [TestMethod]
        public void Case5()
        {
            var ok = FakeExecutor.Run(@"Submissions/student1.cs", "Solve", "Case5", new object[] { 3, 3 }, new object[] { 6 });
            Assert.IsTrue(ok);
        }
    }
}
