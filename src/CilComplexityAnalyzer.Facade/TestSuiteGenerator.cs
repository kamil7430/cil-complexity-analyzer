using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CilComplexityAnalyzer.TestGenerator;

[Generator]
public class TestSuiteGenerator : IIncrementalGenerator
{
    private const string TestSuiteBaseFullName = "CilComplexityAnalyzer.TestExecutor.Contract.TestSuite";
    private const string TestCaseBaseFullName = "CilComplexityAnalyzer.TestExecutor.Contract.TestCase";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var suites = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => node is ClassDeclarationSyntax, // { BaseList: not null },
                transform: static (ctx, _) => GetSuiteOrNull(ctx))
            .Where(static suite => suite is not null)
            .Select(static (suite, _) => suite!)
            .Collect();
        context.RegisterPostInitializationOutput(ctx => 
        {
            ctx.AddSource("GeneratorDebug.g.cs", "// Generator dziala!");
        });

        context.RegisterSourceOutput(suites, Generate);
    }

    private static SuiteInfo? GetSuiteOrNull(GeneratorSyntaxContext ctx)
    {
        var classDecl = (ClassDeclarationSyntax)ctx.Node;

        if (ctx.SemanticModel.GetDeclaredSymbol(classDecl) is not { } suiteSymbol)
            return null;

        if (!InheritsFrom(suiteSymbol, TestSuiteBaseFullName))
            return null;

        var suiteAttr = suiteSymbol.GetAttributes()
            .FirstOrDefault(a => a.AttributeClass?.Name is "MethodToInvokeAttribute" or "MethodToInvoke");

        string? defaultSuiteMethod = suiteAttr?.ConstructorArguments.FirstOrDefault().Value as string;

        var casesBuilder = ImmutableArray.CreateBuilder<CaseInfo>() ?? throw new ArgumentNullException("ImmutableArray.CreateBuilder<CaseInfo>()");

        foreach (var member in suiteSymbol.GetTypeMembers())
        {
            if (InheritsFrom(member, TestCaseBaseFullName))
            {
                var caseInfo = GetCaseInfo(member, defaultSuiteMethod);
                casesBuilder.Add(caseInfo);
            }
        }

        var isContainerized = suiteSymbol.GetAttributes()
            .Any(a => a.AttributeClass?.Name is "IsContainerizedAttribute" or "IsContainerized");

        var nameAttr = suiteSymbol.GetAttributes()
            .FirstOrDefault(a => a.AttributeClass?.Name is "NameAttribute" or "Name");
        
        var name = nameAttr?.ConstructorArguments.FirstOrDefault().Value as string;

        return new SuiteInfo(
            Namespace: suiteSymbol.ContainingNamespace.ToDisplayString(),
            ClassName: suiteSymbol.Name,
            Name: string.IsNullOrWhiteSpace(name) ? suiteSymbol.Name : name!,
            IsContainerized: isContainerized,
            Cases: casesBuilder.ToImmutable());
    }

    private static CaseInfo GetCaseInfo(INamedTypeSymbol caseSymbol, string? defaultSuiteMethod)
    {
        var settings = caseSymbol.GetAttributes()
            .FirstOrDefault(a => a.AttributeClass?.Name is "CaseSettingsAttribute" or "CaseSettings" or "TestSettingsAttribute" or "TestSettings" or "SecretTestName");

        long instructionCap = GetInstructionCap(settings, 1_000_000L);
        var methodToInvokeAttr = caseSymbol.GetAttributes()
            .FirstOrDefault(a => a.AttributeClass?.Name is "MethodToInvokeAttribute" or "MethodToInvoke");

        string? methodName = null;
        if (methodToInvokeAttr is not null && methodToInvokeAttr.ConstructorArguments.Length > 0)
        {
            methodName = methodToInvokeAttr.ConstructorArguments[0].Value as string;
        }

        methodName ??= defaultSuiteMethod;
        var location = caseSymbol.Locations.FirstOrDefault();
        
        return new CaseInfo(caseSymbol.Name, instructionCap, methodName, location);
    }

    private static long GetInstructionCap(AttributeData? attr, long fallback)
    {
        if (attr is null) return fallback;

        var namedArg = attr.NamedArguments.FirstOrDefault(a => a.Key == "InstructionCap");
        if (namedArg.Value.Value is not null)
            return Convert.ToInt64(namedArg.Value.Value);

        if (attr.ConstructorArguments.Length > 0 && attr.ConstructorArguments[0].Value is not null)
            return Convert.ToInt64(attr.ConstructorArguments[0].Value);

        return fallback;
    }

    private static bool InheritsFrom(INamedTypeSymbol? symbol, string fullyQualifiedBaseName)
    {
        for (var baseType = symbol?.BaseType; baseType is not null; baseType = baseType.BaseType)
        {
            if (baseType.ToDisplayString() == fullyQualifiedBaseName)
                return true;
        }
        return false;
    }
    
    private static void Generate(SourceProductionContext spc, ImmutableArray<SuiteInfo> suites)
    {
        foreach (var suite in suites)
        {
            var sb = new StringBuilder();
            sb.AppendLine("// <auto-generated/>");
            sb.AppendLine("#nullable enable");
            sb.AppendLine("using Microsoft.VisualStudio.TestTools.UnitTesting;");
            sb.AppendLine("using System.Threading.Tasks;");
            sb.AppendLine("using System.Linq;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using ExecEngine = global::CilComplexityAnalyzer.TestExecutor.TestExecutor;");
            sb.AppendLine();

            if (!string.IsNullOrEmpty(suite.Namespace) && suite.Namespace != "<global namespace>")
            {
                sb.AppendLine($"namespace {suite.Namespace};");
                sb.AppendLine();
            }

            sb.AppendLine("[TestClass]");
            sb.AppendLine($"public partial class {suite.ClassName}_Tests");
            sb.AppendLine("{");
            sb.AppendLine("    private static List<string> _results = new List<string>();");
            //sb.AppendLine("    private TestExecutor? _executor;");
            sb.AppendLine();
            sb.AppendLine("    [ClassInitialize]");
            sb.AppendLine("    public static void Initialize(TestContext context)");
            sb.AppendLine("    {");
            sb.AppendLine($"        var suiteInstance = new {suite.ClassName}();");
            //sb.AppendLine($"        var testResults = ExecEngine.Execute(suiteInstance);");
            sb.AppendLine("    }");
            sb.AppendLine();

            var numberOfTests = 0;
            foreach (var testCase in suite.Cases)
            {
                sb.AppendLine("    [TestMethod]");
                sb.AppendLine($"    public async Task {testCase.Name}()");
                sb.AppendLine("    {");
                sb.AppendLine();
                sb.AppendLine($"       // var testResult = await TestExecutor.GetResult(\"{testCase.Name}\");");
                sb.AppendLine($"        var testResult =\" cos\";");
                sb.AppendLine();
                sb.AppendLine($"        _results[{numberOfTests}] = \"Success\";");
                sb.AppendLine("        var typeName = testResult.GetType().Name;");
                sb.AppendLine("        if (typeName == \"Failure\")");
                sb.AppendLine("        {");
                sb.AppendLine("            var message = testResult.GetType().GetProperty(\"Message\")?.GetValue(testResult)?.ToString() ?? \"Unknown failure\";");
                sb.AppendLine($"            _results[{numberOfTests}] = message;");
                sb.AppendLine($"            Assert.Fail($\"Test '{testCase.Name}' zakończył się niepowodzeniem: {{message}}\");");
                sb.AppendLine("        }");
                sb.AppendLine("    }");
                sb.AppendLine();
                numberOfTests++;
            }

            sb.AppendLine("}");

            spc.AddSource($"{suite.ClassName}Tests.g.cs", sb.ToString());
        }
    }

    private sealed record SuiteInfo(
        string Namespace, 
        string ClassName, 
        string Name, 
        bool IsContainerized,
        ImmutableArray<CaseInfo> Cases);

    private sealed record CaseInfo(
        string Name, 
        long InstructionCap, 
        string? MethodName, 
        Location? Location);
}