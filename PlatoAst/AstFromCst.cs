using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Parakeet;
using Parakeet.Demos.CSharp;

namespace PlatoAst
{
    public class AstFromCst
    {
        public AstNode ToIntrinsic(string name, params AstNode[] args)
        {
            throw new NotImplementedException();
        }

        public AstNode ToAst(CstExpression expr)
        {
            var leaf = ToAst(expr.LeafExpression);
            var r = leaf;
            foreach (var postfix in expr.PostfixOperator.Nodes)
            {
                if (postfix.AsOperation != null)
                {
                    if (postfix.AsOperation.Identifier.Present)
                    {
                        // Var declaration
                    }
                }
                else if (postfix.BinaryOperation != null)
                {

                }
                else if (postfix.ConditionalMemberAccess != null)
                {

                }
                else if (postfix.FunctionArgs != null)
                {

                }
                else if (postfix.Indexer != null)
                {

                }
                else if (postfix.IsOperation != null)
                {

                }
                else if (postfix.MemberAccess != null)
                {

                }
                else if (postfix.TernaryOperation != null)
                {

                }
            }

            foreach (var prefix in expr.UnaryOperator.Nodes)
            {
                switch (prefix.Text)
                {
                    case "++":
                    case "+":
                    case "-":
                    case "--":
                    case "!":
                    case "~":
                        throw new NotImplementedException();
                    default:
                        throw new NotSupportedException();
                }
            }
            return leaf;
        }

        public AstNode ToAst(CstNode node)
        {
            switch (node)
            {

                case CstBaseCall cstBaseCall:
                    break;
                case CstBaseOrThisCall cstBaseOrThisCall:
                    break;

                // Literals 
                case CstLiteral cstLiteral:
                    return ToAst(cstLiteral.Node);
                
                case CstBinaryLiteral cstBinaryLiteral:
                    break;
                case CstBooleanLiteral cstBooleanLiteral:
                    break;
                case CstCharLiteral cstCharLiteral:
                    break;
                case CstFloatLiteral cstFloatLiteral:
                    break;
                case CstNullLiteral cstNullLiteral:
                    break;
                case CstIntegerLiteral cstIntegerLiteral:
                    break;
                case CstStringLiteral cstStringLiteral:
                    break;
                case CstHexLiteral cstHexLiteral:
                    break;

                // Supported statements

                case CstBreakStatement cstBreakStatement:
                    return AstBreak.Default;
                case CstCompoundStatement cstCompoundStatement:
                    return AstBlock.Create(cstCompoundStatement.Statement.Children.Select(ToAst).ToArray());
                case CstDoWhileStatement cstDoWhileStatement:
                    break;
                case CstForEachStatement cstForEachStatement:
                    break;
                case CstForStatement cstForStatement:
                    break;
                case CstReturnStatement cstReturnStatement:
                    return AstReturn.Create(ToAst(cstReturnStatement.Expression));
                case CstWhileStatement cstWhileStatement:
                    return AstLoop.Create(ToAst(cstWhileStatement.ParenthesizedExpression), ToAst(cstWhileStatement.Statement));
                case CstContinueStatement cstContinueStatement:
                    return AstContinue.Default;

                
                case CstCastExpression cstCastExpression:
                    break;
                case CstCatchClause cstCatchClause:
                    break;
                case CstCompoundTypeExpr cstCompoundTypeExpr:
                    break;
                case CstConditionalMemberAccess cstConditionalMemberAccess:
                    break;
                case CstDefault cstDefault:
                    break;
                case CstDefaultValue cstDefaultValue:
                    break;
                
                case CstExpression cstExpression:
                    return ToAst(cstExpression);
                
                case CstExpressionBody cstExpressionBody:
                    if (cstExpressionBody.CompoundStatement != null)
                    {
                        return ToAst(cstExpressionBody.CompoundStatement);
                    }
                    else
                    {
                        return ToAst(cstExpressionBody.Expression);
                    }
                    break;

                case CstExpressionStatement cstExpressionStatement:
                    return ToAst(cstExpressionStatement.Expression);

                case CstFile cstFile:
                    break;

                case CstFinallyClause cstFinallyClause:
                    break;

                case CstIdentifier cstIdentifier:
                    return AstVarRef.Create(cstIdentifier.Text);

                case CstIfStatement cstIfStatement:
                    return AstConditional.Create(ToAst(cstIfStatement.ParenthesizedExpression),
                        ToAst(cstIfStatement.Statement),
                            cstIfStatement.ElseClause.Present 
                                ? ToAst(cstIfStatement.ElseClause.Node) 
                                : AstNoop.Default);
                    
                case CstElseClause cstElseClause:
                    return ToAst(cstElseClause.Statement);

                case CstInitializer cstInitializer:
                    break;
                case CstInitializerClause cstInitializerClause:
                    break;

                case CstInnerTypeExpr cstInnerTypeExpr:
                    break;
                case CstInvariantClause cstInvariantClause:
                    break;

                case CstLambdaBody cstLambdaBody:
                    break;
                case CstLambdaExpr cstLambdaExpr:
                    break;
                case CstLambdaParameter cstLambdaParameter:
                    break;
                case CstLambdaParameters cstLambdaParameters:
                    break;
                case CstLeafExpression cstLeafExpression:
                    return ToAst(cstLeafExpression.Node);

                case CstMemberAccess cstMemberAccess:
                    break;
                
                case CstNameOf cstNameOf:
                    return ToIntrinsic("nameof", ToAst(cstNameOf.Expression));

                case CstNewOperation cstNewOperation:
                    // TODO: we need to handle types. 
                    break;

                case CstOperatorDeclaration cstOperatorDeclaration:
                    break;
                
                case CstParenthesizedExpression cstParenthesizedExpression:
                    return ToAst(cstParenthesizedExpression.Expression);

                case CstQualifiedIdentifier cstQualifiedIdentifier:
                    break;

                case CstStatement cstStatement:
                    return ToAst(cstStatement.Node);
                    
                case CstStringInterpolation cstStringInterpolation:
                    break;
                case CstStringInterpolationContent cstStringInterpolationContent:
                    break;

                case CstThisCall cstThisCall:
                    break;
                case CstThrowExpression cstThrowExpression:
                    break;

                case CstTryStatement cstTryStatement:
                    break;
                case CstTypeArgList cstTypeArgList:
                    break;
                case CstTypeDeclaration cstTypeDeclaration:
                    break;
                case CstTypeDeclarationWithPreamble cstTypeDeclarationWithPreamble:
                    break;
                case CstTypeExpr cstTypeExpr:
                    break;
                case CstTypeKeyword cstTypeKeyword:
                    break;
                case CstTypeOf cstTypeOf:
                    break;
                case CstTypeParameter cstTypeParameter:
                    break;
                case CstTypeParameterList cstTypeParameterList:
                    break;

                case CstTypeVariance cstTypeVariance:
                    break;
                case CstVarDecl cstVarDecl:
                    break;
                case CstVarDeclStatement cstVarDeclStatement:
                    break;
                case CstVariantClause cstVariantClause:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(node));
            }

            throw new NotSupportedException($"The concrete syntax node {node.GetType().Name} is not supported");
        }
    }

    public class TypeLibrary
    {

    }
}
