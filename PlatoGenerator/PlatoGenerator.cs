using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace PlatoGenerator
{
    [Generator]
    public class PlatoGenerator : ISourceGenerator
    {
        public static string LogFileName = Path.Combine(Path.GetTempPath(), "plato.txt");

        public static void OutputExpression(StreamWriter sw, PlatoExpressionSyntax expr, SemanticModel model,
            string indent = "")
        {
            // TODO: Figure out how this happens.
            if (expr == null)
                return;

            OutputExpression(sw, PlatoExpression.Create(expr, model), indent);
        }

        public static void OutputExpression(StreamWriter sw, PlatoExpression expr, string indent = "")
        {
            if (expr == null)
                return;

            sw.WriteLine($"{indent}Def:{expr.Name} Value:{expr.Value} IsMethod:{expr.IsMethod} Symbol:{expr.Symbol} Type:{expr.Type} Kind:{expr.SyntaxKind} Decl:{expr.Declaration?.Id}");
            if (expr.Arguments.Count > 0)
            {
                sw.WriteLine(indent + "  Arguments:");
                foreach (var a in expr.Arguments)
                {
                    OutputExpression(sw, a, indent + "    ");
                }
            }
            if (expr.Children.Count > 0)
            {
                sw.WriteLine(indent + "  Children:");
                foreach (var c in expr.Children)
                {
                    OutputExpression(sw, c, indent + "   ");
                }
            }
        }

        public static void OutputStatement(StreamWriter sw, PlatoStatementSyntax st, SemanticModel model, string indent = "")
        {
            if (st == null) 
                return;

            sw.WriteLine($"{indent}{st.Node.GetType().Name}@{st.Id}");

            //var statementSymbol = model.GetSymbolInfo(st.Node).Symbol;
            //sw.WriteLine($"Has statement symbol {statementSymbol}");

            if (st.ChildExpressions.Count > 0)
            {
                sw.WriteLine(indent + "  Expressions:");
                foreach (var c in st.ChildExpressions)
                {
                    OutputExpression(sw, c, model, indent + "     ");
                }
            }

            if (st.ChildStatements.Count > 0)
            {
                sw.WriteLine(indent + "  Statements:");
                foreach (var c in st.ChildStatements)
                {
                    OutputStatement(sw, c, model, indent + "     ");
                }
            }
        }

        public static void OutputVariable(StreamWriter sw, PlatoVariableSyntax v)
        {
            sw.Write($"{v.Type?.Text} {v.Name}@{v.Id}");
        }

        public static void OutputStatementOrExpression(StreamWriter sw, PlatoStatementSyntax st, PlatoExpressionSyntax expr, SemanticModel model, string indent = "")
        {
            if (st != null)
            {
                OutputStatement(sw, st, model, indent);
            }

            if (expr != null)
            {
                OutputExpression(sw, expr, model, indent);
            }
        }

        public static void OutputExpression(StreamWriter sw, ExpressionIR expression, string indent = "")
        {
            if (expression == null)
                return;
            sw.WriteLine($"{indent}{expression.Name}@{expression.Id}:{expression.Type} {expression.Kind}");
            foreach (var x in expression.Children)
                OutputExpression(sw, x, indent + "--");

            // TODO: look for 
            //if (expression.SourceExpression)
        }

        public static void OutputStatement(StreamWriter sw, StatementIR statement, string indent = "")
        {
            if (statement == null)
                return;
            switch (statement)
            {
                case BlockStatementIR blockStatementIr:
                    sw.WriteLine($"{indent}{{");
                    foreach (var s in blockStatementIr.Statements)
                        OutputStatement(sw, s, indent + "  ");
                    sw.WriteLine($"{indent}}}");
                    break;
                case ExpressionStatementIR expressionStatementIr:
                    OutputExpression(sw, expressionStatementIr.Expression, indent);
                    sw.WriteLine($"{indent};");
                    break;
                case FunctionDefinitionIR functionDefinitionIr:
                    sw.WriteLine("UNSUPPORTED FUNCTION DEFINITION");
                    break;
                case IfStatementIR ifStatementIr:
                    sw.WriteLine($"{indent}if(");
                    OutputExpression(sw, ifStatementIr.Condition, indent + "  ");
                    sw.WriteLine($"{indent})");
                    OutputStatement(sw, ifStatementIr.OnTrue, indent + "  ");
                    if (ifStatementIr.OnFalse != null)
                    {
                        sw.WriteLine($"{indent}else");
                        OutputStatement(sw, ifStatementIr.OnFalse);
                    }
                    break;
                case ReturnStatementIR returnStatementIr:
                    sw.WriteLine($"{indent}return ");
                    OutputExpression(sw, returnStatementIr.Expression, indent + "  ");
                    sw.WriteLine($"{indent};");
                    break;
                case ThrowStatementIR throwStatementIr:
                    sw.WriteLine($"{indent}throw ");
                    if (throwStatementIr.Expression != null)
                        OutputExpression(sw, throwStatementIr.Expression, indent + "  ");
                    sw.WriteLine($"{indent};");
                    break;
                case UnsupportedStatementIR unsupportedStatementIr:
                    sw.WriteLine($"UNSUPPORTED {unsupportedStatementIr.Syntax.Kind()}");
                    break;
                case WhileStatementIR whileStatementIr:
                    OutputStatement(sw, whileStatementIr.Initialization, indent);
                    sw.WriteLine($"{indent}while(");
                    OutputExpression(sw, whileStatementIr.Condition, indent + "  ");
                    sw.WriteLine($"{indent})");
                    sw.WriteLine($"{indent}{{");
                    OutputStatement(sw, whileStatementIr.Body, indent + "  ");
                    OutputStatement(sw, whileStatementIr.Increment, indent + "  ");
                    sw.WriteLine($"{indent}}}");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(statement));
            }
        }

        public static void OutputFunction(StreamWriter sw, FunctionDefinitionIR func, string indent = "")
        {
            var paramList = string.Join(", ", func.Parameters.Select(p => $"{p.Name}@{p.Id}:{p.Type}"));
            sw.WriteLine($"{indent}Function {func.Name}({paramList})");
            sw.WriteLine($"{indent}{{");
            OutputStatement(sw, func.Body, indent + "  ");
            sw.WriteLine($"{indent}}}");
        }

        public static void OutputTypes2(GeneratorExecutionContext context)
        {
            var types = context.Compilation.SyntaxTrees.GetPlatoTypes();

            using (var sw = new StreamWriter(File.OpenWrite(LogFileName)))
            {
                foreach (var t in types)
                {
                    var model = context.Compilation.GetSemanticModel(t.Node.SyntaxTree);

                    sw.WriteLine($"{t.Kind} {t.Name}");
                    foreach (var m in t.Methods)
                    {
                        var func = FunctionDefinitionIR.Create(m, model);
                        OutputFunction(sw, func, "  ");
                    }
                }
            }
        }

        public static void OutputTypes(GeneratorExecutionContext context)
        {
            var types = context.Compilation.SyntaxTrees.GetPlatoTypes();

            using (var sw = new StreamWriter(File.OpenWrite(LogFileName)))
            {
                foreach (var t in types)
                {
                    var model = context.Compilation.GetSemanticModel(t.Node.SyntaxTree);

                    sw.WriteLine($"{t.Kind} {t.Name}@{t.Id}");
                    sw.WriteLine("{");

                    var typeSymbol = model.GetDeclaredSymbol(t.Node) as ITypeSymbol;
                    sw.WriteLine($"Has type symbol {typeSymbol}");

                    sw.WriteLine("// Methods");
                    foreach (var m in t.Methods)
                    {
                        var parameters = string.Join(", ", m.Parameters.Select(p => $"{p.Type.Text} {p.Name}"));
                        sw.WriteLine($"{m.ReturnType.Text} {m.Name}@{m.Id}({parameters})");

                        if (m.ExpressionBody != null)
                        {
                            sw.WriteLine("=> " + m.ExpressionBody.Text);
                        }

                        OutputStatement(sw, m.StatementBody, model, "  ");

                        //var methodSymbol = model.GetDeclaredSymbol(m.Node) as IMethodSymbol;
                        //sw.WriteLine($"Has method symbol {methodSymbol}");
                    }

                    sw.WriteLine("// Properties");
                    foreach (var p in t.Properties)
                    {
                        sw.Write($"{p.Type.Text} {p.Name}@{p.Id}");
                        if (p.ArrowExpression != null)
                        {
                            sw.Write(" => " + p.ArrowExpression.Text);
                        }

                        if (p.InitializerExpression != null)
                        {
                            sw.Write(" = " + p.InitializerExpression.Text);
                        }

                        sw.WriteLine();

                        if (p.Getter != null)
                        {
                            sw.WriteLine("get ");
                            OutputStatementOrExpression(sw, p.Getter.BlockStatement, p.Getter.ArrowExpression, model);
                        }


                        //var propSymbol = model.GetDeclaredSymbol(p.Node) as IPropertySymbol;
                        //sw.WriteLine($"Has property symbol {propSymbol}");
                    }

                    sw.WriteLine("// Fields");
                    foreach (var f in t.Fields)
                    {
                        OutputVariable(sw, f.Variable);
                        sw.WriteLine();

                        //var fieldSymbol = model.GetDeclaredSymbol(f.Node) as IFieldSymbol;
                        //sw.WriteLine($"Has field symbol {fieldSymbol}");
                    }

                    sw.WriteLine("}");
                }
                sw.Close();
            }
        }

        public void Execute(GeneratorExecutionContext context)
        {
            Console.WriteLine("I am executing!");

            OutputTypes2(context);

            // begin creating the source we'll inject into the users compilation
            var sourceBuilder = new StringBuilder(@"
using System;
namespace HelloWorldGenerated
{
    public static class HelloWorld
    {
        public static void SayHello() 
        {
            Console.WriteLine(""Hello from generated code5!"");
        }
    }
}");

            // inject the created source into the users compilation
            context.AddSource("helloWorldGenerator", SourceText.From(sourceBuilder.ToString(), Encoding.UTF8));
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            Debug.WriteLine("Initialization");
        }
    }
}
