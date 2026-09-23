namespace CilInstructionCounter.Tests;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Assertions;
using CilInjecting.Tests.Infrastructure.Compilers;

[TestClass]
public class InstructionCounterWeaverTests
{
    [TestMethod]
    public void Inject_ShouldPrependCounterSequence_BeforeEveryInstructionInModule()
    {
        // Arrange
        using var originalModule = TestAssemblyGenerator.CreateDefaultModule();
        using var targetModule = TestAssemblyGenerator.CreateDefaultModule();
        
        var weaver = new InstructionCounterWeaver();

        // Act
        weaver.Inject(targetModule);

        // Assert
        targetModule.ShouldHaveInjectedCounterSequenceComparedTo(originalModule);
    }
    
    [TestMethod]
    public void Inject_ShouldRetargetBranchTargets_ToStartOfInjectedSequence()
    {
        const string sourceCode = $@"
        namespace TestTarget;

        public class BranchClass
        {{
            public int MethodWithIfStatement(bool condition)
            {{
                if (condition)
                {{
                    return 10;
                }}
                return 20;
            }}

            public string MethodWithSwitchStatement(int option)
            {{
                return option switch
                {{
                    1 => ""One"",
                    2 => ""Two"",
                    _ => ""Other""
                }};
            }}

            public int MethodWithForStatement(int a, int b)
            {{
                int sum = 0;
                for(int i = 0; i < a; i++)
                {{
                    sum += b;
                }}
                return sum;
            }}
        }}";
        
        using var originalModule = TestAssemblyGenerator.CompileToModule(sourceCode);
        using var targetModule = TestAssemblyGenerator.CompileToModule(sourceCode);

        var weaver = new InstructionCounterWeaver();

        // Act
        weaver.Inject(targetModule);

        // Assert
        targetModule.ShouldHaveRetargetedBranchTargetsComparedTo(originalModule);
    }
    
    [TestMethod]
    public void Inject_ShouldRetargetExceptionHandlers_ToStartOfInjectedSequence()
    {
        const string sourceCode = @"
        namespace TestTarget;

        using System;

        public class ExceptionClass
        {
            private int _state;

            public void MethodWithTryCatch()
            {
                try
                {
                    _state = 1;
                    throw new InvalidOperationException();
                }
                catch (InvalidOperationException)
                {
                    _state = 2;
                }
                catch (Exception)
                {
                    _state = 3;
                }
            }

            public void MethodWithTryFinally()
            {
                try
                {
                    _state = 10;
                }
                finally
                {
                    _state = 20;
                }
            }
        }";

        using var originalModule = TestAssemblyGenerator.CompileToModule(sourceCode);
        using var targetModule = TestAssemblyGenerator.CompileToModule(sourceCode);

        var weaver = new InstructionCounterWeaver();

        // Act
        weaver.Inject(targetModule);

        // Assert
        targetModule.ShouldHaveRetargetedExceptionHandlersComparedTo(originalModule);
    }
}