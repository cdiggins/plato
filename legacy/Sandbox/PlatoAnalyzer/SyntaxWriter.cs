using System;
using System.Collections.Generic;
using System.Text;

namespace PlatoAnalyzer
{
    public class SyntaxWriter
    {
        public StringBuilder Builder = new StringBuilder();
        public int tabs;

        public override string ToString()
            => Builder.ToString().Replace("\n", "\r\n");

        public SyntaxWriter Add(string s = null)
        {
            if (s != null)
                Builder.Append(s);
            return this;
        }

        public SyntaxWriter Add(IEnumerable<PlatoSyntaxNode> syntax, string separator)
        {
            var first = true;
            var x = this;
            foreach (var syn in syntax)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    if (separator == "\n")
                        AddLine();
                    else
                        Add(separator);
                }
                x.Add(syn);
            }

            return x;
        }

        public SyntaxWriter Indent()
        {
            tabs++;
            return this;
        }

        public SyntaxWriter Outdent()
        {
            tabs--;
            return this;
        }

        public SyntaxWriter AddLine(string s = null)
            => Add(s).Add("\n").Add(new string(' ', tabs * 4));

        public SyntaxWriter AddBraced(Func<SyntaxWriter, SyntaxWriter> f)
            => f(Add("{").Indent().AddLine()).Outdent().AddLine().AddLine("}");

        public SyntaxWriter AddBracedIfNeeded(PlatoStatement statement)
            => statement is PlatoBlockStatement st 
                ? Add(st) 
                : AddBraced(x => x.Add(statement));

        public SyntaxWriter AddParenthesized(IEnumerable<PlatoSyntaxNode> nodes)
            => Add("(").Add(nodes, ", ").Add(")");

        public static string GetKind(PlatoClass cls)
            => cls.IsInterface ? "interface" : cls.IsStruct ? "struct" : "class";

        public SyntaxWriter Add(PlatoSyntaxNode syntaxNode)
        {
            switch (syntaxNode)
            {
                case PlatoNameOf platoNameOf:
                    return Add("nameof(\"").Add(platoNameOf.Value.ToString()).Add("\")");
                case PlatoTypeOf platoTypeOf:
                    return Add("typeof(").Add(platoTypeOf.Type).Add(")");
                case PlatoArg arg:
                    return Add(arg.Name != null ? arg.Name + ": " : "").Add(arg.Value);
                case PlatoArgList platoArgList:
                    return Add("(").Add(platoArgList.Args, ", ").Add(")");
                case PlatoArray platoArray:
                    return Add("new ").Add(platoArray.Type).Add("{").Add(platoArray.Elements, ", ").Add("}");
                case PlatoAssignment platoAssignment:
                    return Add(platoAssignment.Left).Add(" ").Add(platoAssignment.Operator).Add(" ").Add(platoAssignment.Right);
                case PlatoBinary platoBinary:
                    return Add(platoBinary.Left).Add(" ").Add(platoBinary.Operator).Add(" ").Add(platoBinary.Right);
                case PlatoBlockStatement platoBlockStatement:
                    return AddBraced(f => f.Add(platoBlockStatement.Statements, "\n"));
                case PlatoBreakStatement _:
                    return Add("break;");
                case PlatoCast cast:
                    return Add("(").Add(cast.Type).Add(")").Add(cast.Expression);
                case PlatoClass platoClass:
                    return Add($"public {GetKind(platoClass)} ").Add(platoClass.Name).AddLine().AddBraced(f =>
                        f.Add(platoClass.Methods, "\n").AddLine()
                        .Add(platoClass.Properties, "\n").AddLine()
                        .Add(platoClass.Fields, "\n"));
                case PlatoCompoundStatement platoCompoundStatement:
                    return Add(platoCompoundStatement.Statements, "\n");
                case PlatoConditional platoConditional:
                    return Add(platoConditional.Condition).Add(" ? ")
                        .Add(platoConditional.OnTrue).Add(" : ")
                        .Add(platoConditional.OnFalse);
                case PlatoContinueStatement _:
                    return Add("continue;");
                case PlatoDefault platoDefault:
                    return platoDefault.Type == null
                        ? Add("default")
                        : Add("default(").Add(platoDefault.Type).Add(")");
                case PlatoElementGet platoElementGet:
                    return Add(platoElementGet.Receiver).Add("[").Add(platoElementGet.Index).Add("]");
                case PlatoElementSet platoElementSet:
                    throw new NotImplementedException();
                case PlatoEmptyStatement platoEmptyStatement:
                    return Add(";");
                case PlatoExpressionStatement platoExpressionStatement:
                    return Add(platoExpressionStatement.Expression).Add(";");
                case PlatoProperty platoProperty:
                {
                    var tmp = Add("public ").Add(platoProperty.Type).Add(" ").Add(platoProperty.Name);
                    if (platoProperty.Arrow != null)
                        return tmp.Add(" => ").Add(platoProperty.Arrow).Add(";");
                    tmp = tmp.Add(" { get; }");
                    return platoProperty.Initializer == null
                        ? tmp
                        : tmp.Add(" = ").Add(platoProperty.Initializer).Add(";");
                }
                case PlatoField platoField:
                    return Add("public ").Add(platoField.Type).Add(" ").Add(platoField.Name)
                        .Add(platoField.Initializer != null ? $" = {platoField.Initializer}" : "");
                case PlatoForEachStatement forEach:
                    return Add("foreach (").Add(forEach.VarDecl.Type).Add(" ").Add(forEach.VarDecl.Name)
                        .Add(" in ").Add(forEach.VarDecl.Value).Add(")").AddLine().AddBracedIfNeeded(forEach.Body);
                case PlatoFunction platoFunction:
                    return Add("public ").Add(platoFunction.ReturnType).Add(" ").Add(platoFunction.Name)
                        .Add(platoFunction.Parameters).AddLine().Add(platoFunction.Body);
                case PlatoIdentifierRef platoIdentifierRef:
                    return Add(platoIdentifierRef.Name);
                case PlatoIfStatement platoIfStatement:
                    return Add("if (").Add(platoIfStatement.Condition).AddLine(")")
                        .AddBracedIfNeeded(platoIfStatement.IfStatement).AddLine("else")
                        .AddBracedIfNeeded(platoIfStatement.ElseStatement);
                case PlatoInterpolation platoInterpolation:
                {
                    Add("$\"");
                    foreach (var x in platoInterpolation.Expressions)
                    {
                        if (x is PlatoLiteral lit)
                            Add(lit.Value.ToString());
                        else
                            Add("{").Add(x).Add("}");
                    }
                    return Add("\"");
                }
                case PlatoInvoke platoInvoke:
                    return Add(platoInvoke.Function).Add(platoInvoke.Args);
                case PlatoLambda platoLambda:
                    return (platoLambda.Parameters == null
                        ? Add("()")
                        : Add(platoLambda.Parameters)).Add(" => ").Add(platoLambda.Body);
                case PlatoLiteral platoLiteral:
                    return Add(platoLiteral.Value?.ToString());
                case PlatoMemberGet platoMemberGet:
                    return Add(platoMemberGet.Receiver).Add(".").Add(platoMemberGet.Name);
                case PlatoMemberSet platoMemberSet:
                    throw new NotSupportedException("Member setting is not supported");
                case PlatoNew platoNew:
                    return Add("new ").Add(platoNew.Type).Add(platoNew.Args);
                case PlatoParameter platoParameter:
                    return Add(platoParameter.Type).Add(" ").Add(platoParameter.Name);
                case PlatoParameterList platoParameterList:
                    return Add("(").Add(platoParameterList.Parameters, ", ").Add(")");
                case PlatoParenthesis platoParenthesis:
                    return Add("(").Add(platoParenthesis.Expression).Add(")");
                case PlatoPostfix platoPostfix:
                    return Add(platoPostfix.Operand).Add(platoPostfix.Operator);
                case PlatoPrefix platoPrefix:
                    return Add(platoPrefix.Operator).Add(platoPrefix.Operand);
                case PlatoReturnStatement platoReturnStatement:
                    return Add("return ").Add(platoReturnStatement.Expression).Add(";");
                case PlatoThis platoThis:
                    return Add("this");
                case PlatoThrowExpression platoThrowExpression:
                    return Add("throw ").Add(platoThrowExpression.Expression);
                case PlatoTuple platoTuple:
                    return AddParenthesized(platoTuple.Expressions);
                case PlatoTypeExpr platoType:
                    return platoType.TypeArguments.Count > 0 
                        ? Add(platoType.Name).Add("<").Add(platoType.TypeArguments, ", ").Add(">") 
                        : Add(platoType.Name);
                case PlatoTypeParam platoTypeParam:
                    return Add(platoTypeParam.Name);
                case PlatoVarDeclStatement varDecl:
                    return Add(string.IsNullOrEmpty(varDecl.Type?.ToString()) ? "var" : varDecl.Type.ToString())
                        .Add(" ").Add(varDecl.Name).Add(varDecl.Value != null ? " = " : null)
                        .Add(varDecl.Value).Add(";");
                case PlatoWhileStatement platoWhileStatement:
                    return Add("while (").Add(platoWhileStatement.Condition).AddLine(")").AddBracedIfNeeded(platoWhileStatement.Body);
                case PlatoExpression platoExpression:
                    throw new Exception("Expected a specific expression type");
                case PlatoMember platoMember:
                    throw new Exception("Expected a specific member type");
                case PlatoStatement platoStatement:
                    throw new Exception("Expected a specific expression type");
                case PlatoIdentifier platoIdentifier:
                    return (platoIdentifier.Type != null
                        ? Add(platoIdentifier.Type).Add(" ")
                        : this).Add(platoIdentifier.Name);
                case null:
                    return this;
                default:
                    throw new ArgumentOutOfRangeException(nameof(syntaxNode));
            }
        }
    }
}
