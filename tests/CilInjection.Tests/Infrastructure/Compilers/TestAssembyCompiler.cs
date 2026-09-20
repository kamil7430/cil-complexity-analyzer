namespace CilInjecting.Tests.Infrastructure.Compilers;

using System;
using System.IO;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

public static class TestAssemblyCompiler
{
    public static (Type MarkerType, string FilePath) CompileToTempDll(
        string sourceCode, 
        string typeName, 
        string assemblyName = "TestRuntime")
    {
        string filePath = Path.Combine(Path.GetTempPath(), $"{assemblyName}_{Guid.NewGuid():N}.dll");

        var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
        
        var references = new[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Attribute).Assembly.Location)
        };

        var compilation = CSharpCompilation.Create(
            assemblyName,
            new[] { syntaxTree },
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var emitResult = compilation.Emit(filePath);

        if (!emitResult.Success)
        {
            var errors = string.Join("\n", emitResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
            throw new InvalidOperationException($"Kompilacja dynamicznej biblioteki nie powiodła się:\n{errors}");
        }

        // 3. Ładujemy plik .dll z dysku, aby wyciągnąć z niego obiekt Type
        var assembly = Assembly.LoadFrom(filePath);
        var markerType = assembly.GetType(typeName) 
                         ?? throw new InvalidOperationException($"Nie odnaleziono typu '{typeName}' w dynamicznej bibliotece.");

        return (markerType, filePath);
    }
}