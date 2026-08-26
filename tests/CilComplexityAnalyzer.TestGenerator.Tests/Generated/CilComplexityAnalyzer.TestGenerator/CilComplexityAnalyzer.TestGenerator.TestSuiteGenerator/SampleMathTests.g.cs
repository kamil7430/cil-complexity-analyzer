using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
namespace CilComplexityAnalyzer.TestGenerator.Tests;
public partial class SampleMathTests
{
    [TestClass]
    public partial class Student1
    {
        [TestMethod]
        public void case1()
        {
            var ok = FakeExecutor.Run(@"Submissions/student1.cs", "Solve", "case1", new object[] { 2, 3 }, new object[] { 5 });
            Assert.IsTrue(ok);
        }
        [TestMethod]
        public void case2()
        {
            var ok = FakeExecutor.Run(@"Submissions/student1.cs", "Solve", "case2", new object[] { 2, 3 }, new object[] { 5 });
            Assert.IsTrue(ok);
        }
        [TestMethod]
        public void case3()
        {
            var ok = FakeExecutor.Run(@"Submissions/student1.cs", "Solve", "case3", new object[] { 2, 3 }, new object[] { 5 });
            Assert.IsTrue(ok);
        }
        [TestMethod]
        public void smiecznyCase()
        {
            var ok = FakeExecutor.Run(@"Submissions/student1.cs", "Solve", "smiecznyCase", new object[] { 2, 3 }, new object[] { 5 });
            Assert.IsTrue(ok);
        }
    }
    [TestClass]
    public partial class Student2
    {
        [TestMethod]
        public void case1()
        {
            var ok = FakeExecutor.Run(@"Submissions/student1.cs", "Solve", "case1", new object[] { 2, 3 }, new object[] { 5 });
            Assert.IsTrue(ok);
        }
        [TestMethod]
        public void case2()
        {
            var ok = FakeExecutor.Run(@"Submissions/student1.cs", "Solve", "case2", new object[] { 2, 3 }, new object[] { 5 });
            Assert.IsTrue(ok);
        }
        [TestMethod]
        public void case3()
        {
            var ok = FakeExecutor.Run(@"Submissions/student1.cs", "Solve", "case3", new object[] { 2, 3 }, new object[] { 5 });
            Assert.IsTrue(ok);
        }
        [TestMethod]
        public void smiecznyCase()
        {
            var ok = FakeExecutor.Run(@"Submissions/student1.cs", "Solve", "smiecznyCase", new object[] { 2, 3 }, new object[] { 5 });
            Assert.IsTrue(ok);
        }
    }
}
