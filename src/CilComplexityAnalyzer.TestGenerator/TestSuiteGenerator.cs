using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace CilComplexityAnalyzer.TestGenerator.Generators;


[Generator]
public class TestSuiteGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var classes = context.SyntaxProvider.ForAttributeWithMetadataName(
            "CilComplexityAnalyzer.TestGenerator.Attributes.SolutionListAttribute",
            predicate: (node, _) => node is ClassDeclarationSyntax,
            transform: (ctx, _) => ctx)
        .Collect();
        
        context.RegisterSourceOutput(classes, Generate);
    }

    public static void Generate(SourceProductionContext spc, ImmutableArray<GeneratorAttributeSyntaxContext> items)
    {
        foreach (var item in items)
        {
            var classSymbol = (INamedTypeSymbol)item.TargetSymbol;
            var className = classSymbol.Name;
            var ns = classSymbol.ContainingNamespace.ToDisplayString();
            var attrs = classSymbol.GetAttributes();
            
            var solutionList = attrs.First(a => a.AttributeClass?.Name == "SolutionListAttribute");
            var ids = solutionList.ConstructorArguments[0].Values.Select(v => (string)v.Value!).ToArray();
            var paths = solutionList.ConstructorArguments[1].Values.Select(v => (string)v.Value!).ToArray();

            var methodToInvoke = attrs
                .FirstOrDefault(a => a.AttributeClass?.Name == "MethodToInvokeAttribute")
                ?.ConstructorArguments[0].Value as string ?? "Solve";

            var testCases = attrs.Where(a => a.AttributeClass?.Name == "TestCaseAttribute").ToArray();

            var sb = new StringBuilder();
            sb.AppendLine("using Microsoft.VisualStudio.TestTools.UnitTesting;");
            sb.AppendLine("using System;");
            sb.AppendLine($"namespace {ns};");
            sb.AppendLine($"public partial class {className}");
            sb.AppendLine("{");

            for (int s = 0; s < ids.Length; s++)
            {
                sb.AppendLine("    [TestClass]");
                sb.AppendLine($"    public partial class {Sanitize(ids[s])}");
                sb.AppendLine("    {");

                foreach (var tc in testCases)
                {
                    var name = (string)tc.ConstructorArguments[0].Value!;
                    var input = ArrayLiteral(tc.ConstructorArguments[1]);
                    var output = ArrayLiteral(tc.ConstructorArguments[2]);
                    var path = paths[s];

                    sb.AppendLine("        [TestMethod]");
                    sb.AppendLine($"        public void {Sanitize(name)}()");
                    sb.AppendLine("        {");
                    sb.AppendLine($"            var ok = FakeExecutor.Run(@\"{path}\", \"{methodToInvoke}\", \"{name}\", {input}, {output});");
                    sb.AppendLine("            Assert.IsTrue(ok);");
                    sb.AppendLine("        }");
                }

                sb.AppendLine("    }");
            }

            sb.AppendLine("}");
            spc.AddSource($"{Sanitize(className)}.g.cs", sb.ToString());
        }
    }

    public static string Sanitize(string s) => new string(s.Where(char.IsLetterOrDigit).ToArray());
    private static string Literal(object? v) => v switch
    {
        null => "null",
        string s => "\"" + s.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"",
        bool b => b ? "true" : "false",
        _ => System.Convert.ToString(v, System.Globalization.CultureInfo.InvariantCulture) ?? "null"
    };

    private static string ArrayLiteral(TypedConstant constant) =>
        $"new object[] {{ {string.Join(", ", constant.Values.Select(v => Literal(v.Value)))} }}";
}