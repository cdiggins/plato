using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PlatoAnalyzer
{
    public class PlatoDebugWriter
    {
        public StringBuilder Builder ;

        public override string ToString()
            => Builder.ToString();

        public PlatoDebugWriter(StringBuilder sb = null)
            => Builder = sb ?? new StringBuilder();

        public PlatoDebugWriter Add(IEnumerable<PlatoSyntaxNode> children)
            => children.Aggregate(this, (writer, node) => writer.Add(node));

        public PlatoDebugWriter Add(string start, string end, string delim, IEnumerable<PlatoSyntaxNode> children,
            Func<PlatoSyntaxNode, PlatoDebugWriter> func = null)
        {
            var r = this;
            func = func ?? Add;
            Builder.Append(start);
            var first = true;
            foreach (var c in children)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    Builder.Append(delim);
                }

                r = func(c);
            }
            Builder.Append(end);
            return r;
        }

        public PlatoDebugWriter Add(string text, bool addWhiteSpace = true)
        {
            if (addWhiteSpace)
                Builder.Append(" ").Append(text).Append(" ");
            else
                Builder.Append(text);

            return this;
        }

        public PlatoDebugWriter AddLine(string text)
            => Add(text).AddLine();

        public PlatoDebugWriter AddLine()
        {
            Builder.AppendLine();
            return this;
        }

        public PlatoDebugWriter AddElse(PlatoStatement elseStatement)
        {
            if (elseStatement != null)
                return Add("else").Add(elseStatement);
            return this;
        }

        public PlatoDebugWriter AddInitializer(IReadOnlyList<PlatoSyntaxNode> nodes)
        {
            if (nodes != null && nodes.Any())
                return Add("{", "}", ",", nodes);
            
            return this;
        }

        public PlatoDebugWriter Add(PlatoSyntaxNode node)
        {
            if (node is PlatoExpression expr && !(expr is PlatoTypeExpr))
            {
                // Builder.Append($"/* Id = {node.Id} NodeType = {node.GetType().Name} Type = {expr.Type} */").AppendLine();
            }

            switch (node)
            {
                case null:
                    return this;
                
                case PlatoArgList platoArgList:
                    return Add("(", ")", ",", platoArgList.Args);
                
                case PlatoArg platoArg:
                    // TODO: handle named parameters
                    return Add(platoArg.Value);

                case PlatoArray platoArray:
                {
                    var r = Add("new").Add(platoArray.ElementType).Add("[").Add(platoArray.Size).Add("]");
                    if (platoArray.Elements.Count > 0)
                        r = r.Add("{", "}", ",", platoArray.Elements);
                    return r;
                }

                case PlatoAssignment platoAssignment:
                    return Add(platoAssignment.Left).Add(platoAssignment.Operator)
                        .Add(platoAssignment.Right);
                
                case PlatoAttribute platoAttribute:
                    break;
                
                case PlatoBase platoBase:
                    return Add("base");
                
                case PlatoBinary platoBinary:
                    return Add(platoBinary.Left).Add(platoBinary.Operator).Add(platoBinary.Right);

                case PlatoBlockStatement platoBlockStatement:
                    return AddLine("{").Add(platoBlockStatement.Statements).AddLine("}");
                
                case PlatoBreakStatement platoBreakStatement:
                    return Add("break;").AddLine();
                
                case PlatoCast platoCast:
                    return Add("(").Add(platoCast.Type).Add(")").Add(platoCast.Expression);

                case PlatoClass platoClass:
                    return Add("// class ").Add(platoClass.Name).AddLine()
                        .Add(platoClass.Methods);

                case PlatoCompoundStatement platoCompoundStatement:
                    return platoCompoundStatement.Statements.Count > 1
                        ? AddLine("{").Add(platoCompoundStatement.Statements).AddLine("}")
                        : Add(platoCompoundStatement.Statements);

                case PlatoConditional platoConditional:
                    return Add(platoConditional.Condition).Add("?").Add(platoConditional.OnTrue).Add(":")
                        .Add(platoConditional.OnFalse);
                
                case PlatoConditionalAccess platoConditionalAccess:
                    return Add(platoConditionalAccess.Expression).Add("?.").Add(platoConditionalAccess.WhenNotNull);

                case PlatoContinueStatement platoContinueStatement:
                    return Add("continue");

                case PlatoDefault platoDefault:
                    return Add("default");

                case PlatoElementGet platoElementGet:
                    return Add(platoElementGet.Receiver).Add("[").Add(platoElementGet.Index).Add("]");

                case PlatoElementSet platoElementSet:
                    return Add(platoElementSet.Left).Add("=").Add(platoElementSet.Right);

                case PlatoEmptyStatement platoEmptyStatement:
                    return AddLine(";");
                
                case PlatoExpressionStatement platoExpressionStatement:
                    return Add(platoExpressionStatement.Expression).Add(";").AddLine();
                
                case PlatoForEachStatement platoForEachStatement:
                    // TODO: fix.
                    return Add("foreach");

                case PlatoFunction platoFunction:
                    return Add(platoFunction.ReturnType).Add(" ").Add(platoFunction.Name).Add(platoFunction.Parameters)
                        .AddLine("{").Add(platoFunction.Body).AddLine("}");

                case PlatoIdentifierRef platoIdentifierRef:
                    return Add(platoIdentifierRef.Name);

                case PlatoIfStatement platoIfStatement:
                    return Add("if").Add("(").Add(platoIfStatement.Condition).Add(")").AddLine()
                        .Add(platoIfStatement.IfStatement).AddElse(platoIfStatement.ElseStatement);
                
                case PlatoInterpolation platoInterpolation:
                    break;
                
                case PlatoInvoke platoInvoke:
                    // TODO: I'm confused about the reciever . 
                    //return (platoInvoke.Reciever != null ? Add(platoInvoke.Reciever).Add(".", false) : this).Add(platoInvoke.Function).Add(platoInvoke.Args);
                    return Add(platoInvoke.Function).Add(platoInvoke.Args);

                case PlatoLambda platoLambda:
                    return Add(platoLambda.Parameters).Add("=>").Add(platoLambda.Body);

                case PlatoMemberGet platoMemberGet:
                    return Add(platoMemberGet.Receiver).Add(".").Add(platoMemberGet.Name);

                case PlatoMemberSet platoMemberSet:
                    return Add(platoMemberSet.Left).Add("=").Add(platoMemberSet.Right);

                case PlatoNameOf platoNameOf:
                    return Add("nameof").Add("(").Add(platoNameOf.Value.ToString()).Add(")");
                
                case PlatoNew platoNew:
                    return Add("new").Add(platoNew.Type).Add(platoNew.Args).AddInitializer(platoNew.Initializers);

                case PlatoParameter platoParameter:
                {
                    var r = Add(platoParameter.Type).Add(" ").Add(platoParameter.Name);
                    if (platoParameter.DefaultValue != null)
                        r = r.Add("=").Add(platoParameter.DefaultValue);
                    return r;
                }

                case PlatoParameterList platoParameterList:
                    return Add("(", ")", ",", platoParameterList.Parameters);
                
                case PlatoParenthesis platoParenthesis:
                    return Add("(").Add(platoParenthesis.Expression).Add(")");
                
                case PlatoPatternIs platoPatternIs:
                    return Add(platoPatternIs.Expr).Add(" ").Add(platoPatternIs.Pattern);
                
                case PlatoPatternMatch platoPatternMatch:
                    return Add(platoPatternMatch.Name);

                case PlatoPostfix platoPostfix:
                    return Add(platoPostfix.Operand).Add(platoPostfix.Operand);

                case PlatoPrefix platoPrefix:
                    return Add(platoPrefix.Operator).Add(platoPrefix.Operand);

                case PlatoProperty platoProperty:
                    break;

                case PlatoReturnStatement platoReturnStatement:
                    return platoReturnStatement.Expression != null 
                        ? Add("return").Add(platoReturnStatement.Expression).Add(";").AddLine() 
                        : Add("return;").AddLine();

                case PlatoThis platoThis:
                    return Add("this");

                case PlatoThrowExpression platoThrowExpression:
                    return Add("throw").Add(platoThrowExpression.Expression);
                
                case PlatoTuple platoTuple:
                    return Add("(", ")", ",", platoTuple.Expressions);

                case PlatoTypeExpr platoTypeExpr:
                {
                    var r = Add(platoTypeExpr.Name);
                    if (platoTypeExpr.TypeArguments.Count > 0)
                        r = r.Add("<", ">", ",", platoTypeExpr.TypeArguments);
                    return r;
                }

                case PlatoTypeOf platoTypeOf:
                    return Add("typeof").Add("(").Add(platoTypeOf.Type).Add(")");

                case PlatoTypeParam platoTypeParam:     
                    return Add(platoTypeParam.Type);

                case PlatoTypeParameterList platoTypeParameterList:
                    return platoTypeParameterList.TypeParameters.Count > 0
                        ? Add("<", ">", ",", platoTypeParameterList.TypeParameters)
                        : this;

                case PlatoVarDeclStatement platoVarDeclStatement:
                {
                    var r = platoVarDeclStatement.Type != null 
                        ? Add(platoVarDeclStatement.Type) 
                        : Add("var").Add(" ");
                    r = r.Add(platoVarDeclStatement.Name);
                    if (platoVarDeclStatement.Value != null)
                        r = r.Add("=").Add(platoVarDeclStatement.Value);
                    r = r.Add(";").AddLine();
                    return r;
                }
                
                case PlatoWhileStatement platoWhileStatement:
                    return Add("while").Add("(").Add(platoWhileStatement.Condition).Add(")").Add(platoWhileStatement.Body);

                case PlatoField platoField:
                    break;

                case PlatoLiteral platoLiteral:
                    return Add(platoLiteral.Value.ToString());

                case PlatoStatement platoStatement:
                    break;

                case PlatoExpression platoExpression:
                    break;

                case PlatoMember platoMember:
                    break;
                
                case PlatoIdentifier platoIdentifier:
                    return Add(platoIdentifier.Name);
            }

            Builder.Append($"/* Unsupported Id = {node.Id} NodeType = {node.GetType().Name} */").AppendLine();
            return this;
        }
    }
}
