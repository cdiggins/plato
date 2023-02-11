using PlatoAbstractSyntaxTree;

namespace PlatoAstWriter
{
    public class CommonCodeBuilder : CodeBuilder
    {
        public override CodeBuilder Write(AbstractNode node)
        {
            switch (node)
            {
                case null:
                    return this;

                case WhileStatement whileStatement:
                    return Keyword("while")
                        .Keyword("(")
                        .Write(whileStatement.Condition)
                        .Keyword(")")
                        .WriteLine()
                        .Write(whileStatement.Body);
               
                case CompoundStatement compoundStatement:
                    return Keyword("{")
                        .Indent()
                        .WriteLine()
                        .Write(compoundStatement.Children)
                        .Dedent()
                        .Keyword("}")
                        .WriteLine();
                
                case MultiStatement multiStatement:
                    return Write(multiStatement.Children);

                case BreakStatement breakStatement:
                    return WriteLine("break;");

                case ContinueStatement continueStatement: 
                    return WriteLine("continue;");

                case ReturnStatement returnStatement: 
                    if (returnStatement.Expression == null)
                        return WriteLine("return;");
                    return Keyword("return").Write(returnStatement.Expression).WriteLine(";");

                case TypeExpr typeExpr:
                    {
                        var r = this as CodeBuilder;
                        if (!string.IsNullOrEmpty(typeExpr.NamespaceQualifier))
                            r = r.Write(typeExpr.NamespaceQualifier).Write(".");
                        r = r.Write(typeExpr.Name);
                        if (typeExpr.TypeArgs.Count > 0)
                        {
                            r = r.Write("<")
                                .Write(typeExpr.TypeArgs, ", ")
                                .Write(">");
                        }
                        return r;
                    }
                
                case ParameterDeclaration parameterDeclaration:
                    return Write(parameterDeclaration.Type)
                        .Write(" ")
                        .Write(parameterDeclaration.Name);

                case FunctionDeclaration functionDeclaration:
                    return Keyword("public")
                        .Keyword("static")
                        .Write(functionDeclaration.ReturnType)
                        .Write("(")
                        .Write(functionDeclaration.Parameters, ", ")
                        .Write(")");

                case ConditionalExpression conditionalExpression:
                    return Write(conditionalExpression.Condition)
                        .Write(" ? ")
                        .Write(conditionalExpression.OnTrue)
                        .Write(" : ")
                        .Write(conditionalExpression.OnFalse);

                case IfStatement ifStatement:
                    return Keyword("if").Write("(")
                        .Write(ifStatement.Condition)
                        .WriteLine(")")
                        .Write(ifStatement.OnTrue)
                        .WriteIf(ifStatement.OnFalse != null,
                            cb => cb.WriteLine("else").Write(ifStatement.OnFalse!));

                case FieldDeclaration fieldDeclaration:
                    return Keyword("public")
                        .Write(fieldDeclaration.Type)
                        .Write(" ")
                        .Write(fieldDeclaration.Name)
                        .WriteLine(";");

                case ClassDeclaration classDeclaration:
                    return Keyword("public").Keyword("class")
                        .Write(classDeclaration.Name)
                        .WriteIf(classDeclaration.TypeParameters.Count > 0,
                            cb => cb
                            .Write("<")
                            .Write(classDeclaration.TypeParameters, ", ")
                            .Write(">"))
                        .WriteLine()
                        .WriteLine("{")
                        .Indent()
                        .Write(classDeclaration.Fields)
                        .Write(classDeclaration.Functions)
                        .Write(classDeclaration.ChildClasses)
                        .WriteLine("}")
                        .Dedent();
                
                case Identifier identifier:
                    return Write(identifier.Name);

                case FunctionCall functionCall:
                    return Write(functionCall.Function)
                        .Write("(")
                        .Write(functionCall.Arguments, ", ")
                        .Write(")");
                
                case VariableDeclaration variableDeclaration:
                    return Write(variableDeclaration.Type)
                        .Write(" ")
                        .Write(variableDeclaration.Name)
                        .WriteIf(variableDeclaration.InitialValue != null,
                            cb => cb.Write(" = ").Write(variableDeclaration.InitialValue!))
                        .Write(";");

                case MemberExpression memberExpression: 
                    return Write(memberExpression.Receiver)
                        .Write(".")
                        .Write(memberExpression.Name);

                case MemberAssignment memberAssignment:
                    return Write(memberAssignment.Receiver)
                        .Write(".")
                        .Write(memberAssignment.Name)
                        .Write(" = ")
                        .Write(memberAssignment.RightValue);

                case Assignment assignment:
                    return Write(assignment.LeftValue)
                        .Write(" = ")
                        .Write(assignment.RightValue)
                        .WriteLine(";");

                case ExpressionStatement expression:
                    return Write(expression.Expression).WriteLine(";");

                case Namespace nameSpace:
                    return Keyword("namespace").Write(nameSpace.Name).Write("{")
                        .Indent().WriteLine().Write(nameSpace.NestedNamespaces).WriteLine()
                        .Write(nameSpace.Classes).Dedent().WriteLine("}");

                default:
                    throw new Exception("Unhandled switch case ");
            }
        }
    }
}