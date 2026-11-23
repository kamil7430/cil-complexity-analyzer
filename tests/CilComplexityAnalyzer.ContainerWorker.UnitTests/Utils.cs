using System.Reflection;
using CilComplexityAnalyzer.Contract;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace CilComplexityAnalyzer.ContainerWorker.UnitTests;

public static class Utils
{
    public static Assembly CompileTestSuiteMock(string testSuiteMockCode, CancellationToken cancellationToken)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(
            text: testSuiteMockCode,
            cancellationToken: cancellationToken
        );
        
        using var stream = new MemoryStream();
        CSharpCompilation.Create(
            assemblyName: "TestSuiteMock", 
            syntaxTrees: [syntaxTree],
            references: [
                ..Basic.Reference.Assemblies.Net100.References.All,
                MetadataReference.CreateFromFile(typeof(TestCase).Assembly.Location),
            ],
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        ).Emit(stream, cancellationToken: cancellationToken);
            
        stream.Seek(0, SeekOrigin.Begin);
        return Assembly.Load(stream.ToArray());
    }
    
    public static void InvokePerformSingleTestAndCaptureException(object testCase, Action<Contract.TestResult> action)
    {
        Exception? exception = null;

        Program.PerformSingleTest(testCase, testResult =>
        {
            try
            {
                action(testResult);
            }
            catch (Exception e)
            {
                // first exception should be captured
                exception ??= e;
            }
        });

        if (exception is not null)
            throw exception;
    }
}