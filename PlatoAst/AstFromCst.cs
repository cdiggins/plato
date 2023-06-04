using System;
using System.Collections.Generic;
using System.Linq;
using Parakeet;
using Parakeet.Demos.CSharp;

namespace PlatoAst
{
    public static class Helper
    {
        public static AstConstant<string> ToConstant(this string s)
            => AstConstant.Create(s);
    }

    public class AstFromCst
    {
        public AstNode ToIntrinsic(string name, params AstNode[] args)
        {
            return AstInvoke.Create(AstIntrinsic.Create(name), args);
        }

        public AstNode ToAst(CstExpression expr)
        {
            var leaf = ToAst(expr.LeafExpression);
            var r = leaf;
            foreach (var postfix in expr.PostfixOperator.Nodes)
            {
                if (postfix.AsOperation.Present)
                {
                    if (postfix.AsOperation.Node.Identifier.Present)
                    {
                        // Var declaration
                    }

                    throw new NotImplementedException();
                }
                else if (postfix.BinaryOperation != null)
                {
                    return ToIntrinsic(
                        postfix.BinaryOperation.Node.BinaryOperator.Node.Text,
                        leaf,
                        ToAst(postfix.BinaryOperation.Node.Expression));
                }
                else if (postfix.ConditionalMemberAccess != null)
                {
                    return ToIntrinsic("?.", leaf, postfix.MemberAccess.Node.Identifier.Node.Text.ToConstant());
                }
                else if (postfix.FunctionArgs != null)
                {
                    return AstInvoke.Create(leaf, 
                        postfix.FunctionArgs.Node.FunctionArg.Nodes.Select(n => ToAst(n.Expression)).ToArray());
                }
                else if (postfix.Indexer != null)
                {
                    return ToIntrinsic("[]", leaf, ToAst(postfix.Indexer.Node.Expression));
                }
                else if (postfix.IsOperation != null)
                {
                    throw new NotImplementedException();
                }
                else if (postfix.MemberAccess != null)
                {
                    return ToIntrinsic(".", leaf, postfix.MemberAccess.Node.Identifier.Node.Text.ToConstant());
                }
                else if (postfix.TernaryOperation != null)
                {
                    return AstConditional.Create(
                        leaf,
                        ToAst(postfix.TernaryOperation.Node[0]),
                        ToAst(postfix.TernaryOperation.Node[1]));
                }
            }

            foreach (var prefix in expr.PrefixOperator.Nodes)
            {
                leaf = ToIntrinsic(prefix.Text, leaf);
            }

            return leaf;
        }

        public AstNode ToAst<T>(CstOptional<T> self) where T : CstNode
        {
            return self.Present ? ToAst(self.Node) : AstNoop.Default;
        }

        public IReadOnlyList<AstVarDef> ToAstList(CstLambdaParameters parameters)
        {
            var list = new List<AstVarDef>();
            if (parameters.LambdaParameter != null)
            {
                list.Add(ToAst(parameters.LambdaParameter) as AstVarDef);
            }

            foreach (var lp in parameters.LambdaParameter.Nodes)
            {
                list.Add(ToAst(lp) as AstVarDef);
            }

            return list;
        }

        public AstTypeNode ToAst(CstTypeExpr type)
        {
            return ToAst(type.InnerTypeExpr.Node);
        }

        public AstTypeNode ToAst(CstInnerTypeExpr innerType)
        {
            if (innerType.CompoundOrSimpleTypeExpr.Node?.CompoundTypeExpr != null)
                throw new NotImplementedException("Compound types not supported yet");

            var qi = innerType.CompoundOrSimpleTypeExpr.Node?.QualifiedIdentifier;

            if (qi == null)
                return AstTypeNode.Create("_UNTYPED_");

            var name = string.Join(".", qi.Node.Identifier.Nodes.Select(n => n.Text));

            if (innerType.ArrayRankSpecifiers.Children.Count > 0)
                throw new NotImplementedException("Array rank specifiers not supported yet");

            var ps = new List<AstTypeNode>();
            if (innerType.TypeArgList.Present)
            {
                foreach (var n in innerType.TypeArgList.Node.TypeExpr.Nodes)
                {
                    ps.Add(ToAst(n));
                }
            }

            return AstTypeNode.Create(name, ps.ToArray());
        }

        public AstNode ToAst(CstForEachStatement forEach)
        {
            var vd = forEach.VarDecl;
            return AstBlock.Create(
                ToAst(vd),
                AstLoop.Create(
                    ToIntrinsic("MoveNext", AstVarRef.Create(vd.Node.Identifier.Node.Text)),
                    ToAst(forEach.Statement)));
        }

        public AstNode ToAst<T>(CstFilter<T> filter) where T : CstNode
        {
            return ToAst(filter.Node);
        }

        public AstNode ToAst(CstNode node)
        {
            switch (node)
            {
                case CstBaseCall cstBaseCall:
                    return AstError.Create($"{node.GetType()} is not implemented");
                    
                case CstBaseOrThisCall cstBaseOrThisCall:
                    return AstError.Create($"{node.GetType()} is not implemented");

                // Literals 

                case CstLiteral cstLiteral:
                    return ToAst(cstLiteral.Node);
                
                case CstBinaryLiteral cstBinaryLiteral:
                    return AstError.Create($"{node.GetType()} is not implemented");

                case CstBooleanLiteral cstBooleanLiteral:
                    if (cstBooleanLiteral.Text == "true")
                        return AstConstant.True;
                    if (cstBooleanLiteral.Text == "false")
                        return AstConstant.False;
                    throw new Exception("Not able to compute boolean");

                case CstCharLiteral cstCharLiteral:
                    return AstConstant.Create(
                        char.Parse(cstCharLiteral.Text.Substring(1, cstCharLiteral.Text.Length - 2)));

                case CstFloatLiteral cstFloatLiteral:
                    return AstConstant.Create(double.Parse(cstFloatLiteral.Text));
                
                case CstNullLiteral cstNullLiteral:
                    return AstConstant.Null;
                
                case CstIntegerLiteral cstIntegerLiteral:
                    return AstConstant.Create(int.Parse(cstIntegerLiteral.Text));

                case CstStringLiteral cstStringLiteral:
                    return AstConstant.Create(
                        cstStringLiteral.Text.Substring(1, cstStringLiteral.Text.Length - 2));
                
                case CstHexLiteral cstHexLiteral:
                    return AstError.Create($"{node.GetType()} is not implemented");

                // Statements

                case CstBreakStatement cstBreakStatement:
                    return AstBreak.Default;

                case CstCompoundStatement cstCompoundStatement:
                    return AstBlock.Create(cstCompoundStatement.Statement.Children.Select(ToAst).ToArray());
                
                case CstDoWhileStatement cstDoWhileStatement:
                    return AstMulti.Create(
                        ToAst(cstDoWhileStatement.Statement),
                        AstLoop.Create(
                            ToAst(cstDoWhileStatement.ParenthesizedExpression),
                            ToAst(cstDoWhileStatement.Statement)));

                case CstForEachStatement cstForEachStatement:
                    return ToAst(cstForEachStatement);

                case CstForStatement cstForStatement:
                    return AstBlock.Create(
                        ToAst(cstForStatement.ForLoopInit),
                        AstLoop.Create(
                            ToAst(cstForStatement.ForLoopInvariant),
                            AstMulti.Create(
                                ToAst(cstForStatement.Statement),
                                ToAst(cstForStatement.ForLoopVariant))));

                case CstReturnStatement cstReturnStatement:
                    return AstReturn.Create(ToAst(cstReturnStatement.Expression));

                case CstWhileStatement cstWhileStatement:
                    return AstLoop.Create(ToAst(cstWhileStatement.ParenthesizedExpression), ToAst(cstWhileStatement.Statement));

                case CstContinueStatement cstContinueStatement:
                    return AstContinue.Default;

                case CstCastExpression cstCastExpression:
                    return ToIntrinsic("cast", ToAst(cstCastExpression.Expression), ToAst(cstCastExpression.TypeExpr));

                case CstCatchClause cstCatchClause:
                    return AstError.Create($"{node.GetType()} is not implemented");

                case CstDefault cstDefault:
                    return ToIntrinsic("default", ToAst(cstDefault.TypeExpr));

                case CstFunctionParameterDefaultValue cstDefaultValue:
                    return ToAst(cstDefaultValue.Expression);

                case CstExpression cstExpression:
                    return ToAst(cstExpression);
                
                case CstExpressionBody cstExpressionBody:
                    return cstExpressionBody.CompoundStatement != null ? ToAst(cstExpressionBody.CompoundStatement) : ToAst(cstExpressionBody.Expression);

                case CstExpressionStatement cstExpressionStatement:
                    return ToAst(cstExpressionStatement.Expression);

                case CstFile cstFile:
                    return AstError.Create($"{node.GetType()} is not implemented");

                case CstScript cstScript:
                    return AstMulti.Create(cstScript.TypeDirectiveOrStatement.Nodes.Select(ToAst).ToArray());

                case CstFinallyClause cstFinallyClause:
                    throw new NotImplementedException();

                case CstIdentifier cstIdentifier:
                    return AstVarRef.Create(cstIdentifier.Text);

                case CstIfStatement cstIfStatement:
                    return AstConditional.Create(ToAst(cstIfStatement.ParenthesizedExpression),
                        ToAst(cstIfStatement.Statement),
                        ToAst(cstIfStatement.ElseClause));
                    
                case CstElseClause cstElseClause:
                    return ToAst(cstElseClause.Statement);

                case CstInitializer cstInitializer:
                    throw new NotImplementedException();

                case CstInitializerClause cstInitializerClause:
                    throw new NotImplementedException();

                case CstInnerTypeExpr cstInnerTypeExpr:
                    return ToAst(cstInnerTypeExpr);

                case CstForLoopInvariant cstInvariantClause:
                    return ToAst(cstInvariantClause.Expression);

                case CstLambdaBody cstLambdaBody:
                    return cstLambdaBody.Expression != null
                        ? ToAst(cstLambdaBody.Expression)
                        : ToAst(cstLambdaBody.CompoundStatement);

                case CstLambdaExpr cstLambdaExpr:
                    return AstLambda.Create(ToAst(cstLambdaExpr.LambdaBody),
                        ToAstList(cstLambdaExpr.LambdaParameters.Node).ToArray());

                case CstLambdaParameter cstLambdaParameter:
                    return AstVarDef.Create(
                        cstLambdaParameter.Identifier.Node.Text,
                        ToAst(cstLambdaParameter.TypeExpr) as AstTypeNode);
                
                case CstLeafExpression cstLeafExpression:
                    return ToAst(cstLeafExpression.Node);

                case CstNameOf cstNameOf:
                    return ToIntrinsic("nameof", ToAst(cstNameOf.Expression));

                case CstNewOperation cstNewOperation:
                    return ToIntrinsic("new", ToAst(cstNewOperation.TypeExpr));

                case CstOperatorDeclaration cstOperatorDeclaration:
                    return AstError.Create($"{node.GetType()} is not implemented");
                
                case CstParenthesizedExpression cstParenthesizedExpression:
                    return ToAst(cstParenthesizedExpression.Expression);

                case CstQualifiedIdentifier cstQualifiedIdentifier:
                    return AstError.Create($"{node.GetType()} is not implemented");

                case CstStatement cstStatement:
                    return ToAst(cstStatement.Node);
                    
                case CstStringInterpolation cstStringInterpolation:
                    return AstError.Create($"{node.GetType()} is not implemented");

                case CstStringInterpolationContent cstStringInterpolationContent:
                    return AstError.Create($"{node.GetType()} is not implemented");

                case CstThisCall cstThisCall:
                    return ToIntrinsic("this");

                case CstThrowExpression cstThrowExpression:
                    return ToIntrinsic("throw", ToAst(cstThrowExpression.Expression));

                case CstTryStatement cstTryStatement:
                    return AstError.Create($"{node.GetType()} is not implemented");

                case CstTypeArgList cstTypeArgList:
                    return AstError.Create($"{node.GetType()} is not implemented");

                case CstTypeDeclaration cstTypeDeclaration:
                    return AstError.Create($"{node.GetType()} is not implemented");

                case CstTypeDeclarationWithPreamble cstTypeDeclarationWithPreamble:
                    return AstError.Create($"{node.GetType()} is not implemented");

                case CstTypeExpr cstTypeExpr:
                    return ToAst(cstTypeExpr.InnerTypeExpr);
                    
                case CstTypeKeyword cstTypeKeyword:
                    return AstError.Create($"{node.GetType()} is not implemented");
                    
                case CstTypeOf cstTypeOf:
                    return ToIntrinsic("typeof", ToAst(cstTypeOf.TypeExpr));

                case CstTypeVariance cstTypeVariance:
                    return AstError.Create($"{node.GetType()} is not implemented");

                case CstVarDecl cstVarDecl:
                    return AstVarDef.Create(
                        cstVarDecl.Identifier.Node.Text,
                        AstNoop.Default,
                        ToAst(cstVarDecl.TypeExpr) as AstTypeNode);
                
                case CstVarDeclStatement cstVarDeclStatement:
                    return CreateVarDef(cstVarDeclStatement.VarDecl.Node, 
                        cstVarDeclStatement.Initialization.Node);

                case CstForLoopVariant cstVariantClause:
                    return ToAst(cstVariantClause.Expression);

                case CstTypeDirectiveOrStatement cstTypeDirectiveOrStatement:
                    return ToAst(cstTypeDirectiveOrStatement.Node);

                case CstForLoopInit forLoopInit:
                    return forLoopInit.VarDecl.Present
                        ? (AstNode)CreateVarDef(forLoopInit.VarDecl.Node, forLoopInit.Initialization.Node)
                        : AstNoop.Default;
            }

            return AstError.Create($"{node.GetType()} had no case statement");
        }

        public AstVarDef CreateVarDef(CstVarDecl cstVarDecl, CstInitialization cstInit)
        {
            return AstVarDef.Create(
                cstVarDecl.Identifier.Node?.Text ?? "_",
                cstInit.InitializationValue.Present 
                    ? ToAst(cstInit.InitializationValue.Node) 
                    : AstNoop.Default,
                ToAst(cstVarDecl.TypeExpr) as AstTypeNode);
        }
    }

    public class TypeLibrary
    {

    }
}
