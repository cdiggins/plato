using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Plato.Experiments;

public static class Program
{
    private static Dictionary<string, int> Counts(SyntaxNode root)
    {
        var nodes = root.DescendantNodes().ToList();
        return new()
        {
            ["lambdas"] = nodes.Count(n => n is LambdaExpressionSyntax or AnonymousMethodExpressionSyntax),
            ["delegateInvocations"] = nodes.OfType<InvocationExpressionSyntax>().Count(n =>
                n.Expression is MemberAccessExpressionSyntax m && m.Name.Identifier.Text == "Invoke"),
            ["loops"] = nodes.Count(n => n is ForStatementSyntax or ForEachStatementSyntax or WhileStatementSyntax or DoStatementSyntax),
            ["conditionals"] = nodes.Count(n => n is ConditionalExpressionSyntax),
            ["numericLiterals"] = nodes.Count(n => n.IsKind(SyntaxKind.NumericLiteralExpression))
        };
    }

    public static void Main(string[] args)
    {
        var files = Directory.GetFiles(args[0], "*.g.cs").OrderBy(p => p).Select(path =>
        {
            var root = CSharpSyntaxTree.ParseText(File.ReadAllText(path)).GetRoot();
            return new
            {
                file = Path.GetFileName(path), counts = Counts(root),
                methods = root.DescendantNodes().OfType<MethodDeclarationSyntax>().Select(m => new
                {
                    name = m.Identifier.Text,
                    parameters = m.ParameterList.ToString(),
                    counts = Counts(m)
                }).ToArray()
            };
        }).ToArray();
        Console.WriteLine(JsonSerializer.Serialize(files));
    }
}
