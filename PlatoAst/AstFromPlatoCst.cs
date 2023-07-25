using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Parakeet;
using Parakeet.Demos.Plato;

namespace PlatoAst
{
    public class AstFromPlatoCst
    {
        public static AstNode Convert(CstNode node)
            => new AstFromPlatoCst().ToAst(node);

        public AstNode ToIntrinsicInvocation(string name, params AstNode[] args)
            => AstInvoke.Create(new AstIdentifier(name), args);

        public AstNode ToAst<T>(CstFilter<T> filter) where T: CstNode
            => ToAst(filter.Node);

        public AstNode ToAst(CstExpression expr)
        {
            var r = ToAst(expr.LeafExpression.Node);
            var i = 0;
            while (i < expr.PostfixOperator.Nodes.Count)
            {
                var postfix = expr.PostfixOperator.Nodes[i++];
                var nextPostfix = i < expr.PostfixOperator.Nodes.Count
                    ? expr.PostfixOperator.Nodes[i]
                    : null;
                if (postfix.AsOperation.Present)
                {
                    if (postfix.AsOperation.Node.Identifier.Present)
                    {
                        // Var declaration
                    }

                    throw new NotImplementedException();
                }
                else if (postfix.BinaryOperation.Present)
                {
                    var op = postfix.BinaryOperation.Node.BinaryOperator.Node.Text;
                    r = ToIntrinsicInvocation(
                        OperatorNameLookup.BinaryOperatorToName(op.Trim()),
                        r,   
                        ToAst(postfix.BinaryOperation.Node.Expression));
                }
                else if (postfix.ConditionalMemberAccess.Present)
                {
                    throw new NotImplementedException();
                    // 
                    //r = ToIntrinsicInvocation("?.", r, postfix.MemberAccess.Node.Identifier.Node.Text.ToConstant());
                }
                else if (postfix.FunctionArgs.Present)
                {
                    r = AstInvoke.Create(r, 
                        postfix.FunctionArgs.Node.FunctionArg.Nodes.Select(n => ToAst(n.Expression)).ToArray());
                }
                else if (postfix.Indexer.Present)
                {
                    r = ToIntrinsicInvocation(OperatorNameLookup.BinaryOperatorToName("[]")
                        , r, ToAst(postfix.Indexer.Node.Expression));
                }
                else if (postfix.IsOperation.Present)
                {
                    throw new NotImplementedException();
                }
                else if (postfix.MemberAccess.Present)
                {
                    var args = new List<AstNode>() { r };

                    // a.b => b(a)
                    // a.b(c, d) => b(a, c, d);

                    if (nextPostfix != null && nextPostfix.FunctionArgs.Present)
                    {
                        i++;
                        args.AddRange(nextPostfix.FunctionArgs.Node.FunctionArg.Nodes.Select(n => ToAst(n.Expression)));
                    }

                    var func = ToAst(postfix.MemberAccess.Node.Identifier.Node);
                    r = AstInvoke.Create(new AstIdentifier(func), args.ToArray());
                }
                else if (postfix.TernaryOperation.Present)
                {
                    r = AstConditional.Create(
                        r,
                        ToAst(postfix.TernaryOperation.Node[0]),
                        ToAst(postfix.TernaryOperation.Node[1]));
                }
            }

            foreach (var prefix in expr.PrefixOperator.Nodes)
            {
                r = ToIntrinsicInvocation(
                    OperatorNameLookup.UnaryOperatorToName(prefix.Text.Trim()), r);
            }

            return r;
        }

        public IReadOnlyList<AstParameterDeclaration> ToAstList(CstLambdaParameters parameters)
        {
            var list = new List<AstParameterDeclaration>();
            if (parameters.LambdaParameter != null)
            {
                list.Add(ToAst(parameters.LambdaParameter.Node));
            }

            foreach (var lp in parameters.LambdaParameter.Nodes)
            {
                list.Add(ToAst(lp));
            }

            return list;
        }

        public AstTypeNode ToAst(CstTypeExpr type)
        {
            if (type == null) return null;
            return ToAst(type.InnerTypeExpr.Node);
        }

        public AstTypeNode ToAst(CstCompoundTypeExpr typeExpr)
        {
            return new AstTypeNode("ValueTuple", typeExpr.TypeExpr.Nodes.Select(ToAst).ToArray());
        }

        public AstTypeNode ToAst(CstInnerTypeExpr innerType)
        {
            if (innerType == null)
                return null;

            if (innerType.ArrayRankSpecifiers.Node.ArrayRankSpecifier.Nodes.Count > 0)
                throw new NotImplementedException("Array rank specifiers not supported yet");

            var r = ToAst(innerType.CompoundOrSimpleTypeExpr.Node);
            if (r == null)
                return null;

            var ps = new List<AstTypeNode>();
            if (innerType.TypeArgList.Present)
            {
                foreach (var n in innerType.TypeArgList.Node.TypeExpr.Nodes)
                {
                    ps.Add(ToAst(n));
                }
            }

            if (ps.Count > 0)
                return new AstTypeNode(r.Name, ps.ToArray());

            return r;
        }

        public AstTypeNode ToAst(CstCompoundOrSimpleTypeExpr compOrSimple)
        {
            if (compOrSimple == null)
                return null;

            if (compOrSimple.CompoundTypeExpr.Present)
                return ToAst(compOrSimple.CompoundTypeExpr.Node);

            var qi = compOrSimple.SimpleTypExpr.Node?.QualifiedIdentifier;

            if (qi == null)
                return null;

            var name = string.Join(".", qi.Node.Identifier.Nodes.Select(n => n.Text));
            return AstTypeNode.Create(name);
        }

        public AstNode ToAst(CstForEachStatement forEach)
        {
            var cstIdent = forEach.Identifier.Node;
            var cstType = forEach.TypeExpr.Node;
            var name = ToAst(cstIdent);
            var astType = ToAst(cstType);
            var vd = AstVarDef.Create(name, astType);
            return AstBlock.Create(
                vd,
                AstLoop.Create(
                    ToIntrinsicInvocation("MoveNext", null),
                    ToAst(forEach.Statement)));
        }

        public AstTypeParameter ToAst(CstTypeParameter typeParameter)
            => new AstTypeParameter(typeParameter.Identifier.Node.Text);

        public AstTypeNode ToAst(CstTypeAnnotation typeAnnotation)
        {
            return ToAst(typeAnnotation?.TypeExpr?.Node?.InnerTypeExpr?.Node);
        }

        public AstParameterDeclaration ToAst(CstFunctionParameter fp)
        {
            return new AstParameterDeclaration(
                ToAst(fp.Identifier.Node),
                ToAst(fp.TypeAnnotation.Node));
        }

        public AstFieldDeclaration ToAst(CstFieldDeclaration fieldDeclaration)
        {
            var name = ToAst(fieldDeclaration.Identifier.Node);
            var type = ToAst(fieldDeclaration.TypeExpr.Node);
            return new AstFieldDeclaration(name, type, null);
        }

        public AstMethodDeclaration ToAst(CstMethodDeclaration md)
        {
            var name = md.Identifier.Node.Text;
            var ps = md.FunctionParameterList.Node.FunctionParameter.Nodes.Select(ToAst).ToList();
            Debug.WriteLine($"TODO: need to properly handle parameterized functions. I don't thing they parse.");
            var ts = Enumerable.Empty<AstTypeParameter>();
            return new AstMethodDeclaration(
                ToAst(md.Identifier.Node),
                ToAst(md.TypeAnnotation.Node),
                ps,
                ts,
                ToAst(md.FunctionBody.Node));
            // parameters
            // type parameters
            // body
        }

        public AstMemberDeclaration ToAst(CstMemberDeclaration memberDeclaration)
        {
            if (memberDeclaration.MethodDeclaration.Present)
            {
                return ToAst(memberDeclaration.MethodDeclaration.Node);
            }
            else if (memberDeclaration.FieldDeclaration.Present)
            {
                return ToAst(memberDeclaration.FieldDeclaration.Node);
            }
            else
            {
                throw new Exception("Unrecognized member type");
            }
        }

        public AstParameterDeclaration ToAst(CstLambdaParameter lp)
        {
            return new AstParameterDeclaration(
                ToAst(lp.Identifier.Node), ToAst(lp.TypeAnnotation.Node));
        }

        public AstConstant ToAst(CstLiteral literal)
        {
            if (literal.NullLiteral.Present)
            {
                return AstConstant.Null;
            }

            if (literal.BinaryLiteral.Present)
            {
                throw new NotImplementedException();
            }

            if (literal.BooleanLiteral.Present)
            {
                return AstConstant.Create(bool.Parse(literal.Text));
            }

            if (literal.CharLiteral.Present)
            {
                 return AstConstant.Create(char.Parse(literal.Text));
            }

            if (literal.FloatLiteral.Present)
            {
                return AstConstant.Create(double.Parse(literal.Text));
            }

            if (literal.HexLiteral.Present)
            {
                throw new NotImplementedException();
            }

            if (literal.IntegerLiteral.Present)
            {
                return AstConstant.Create(int.Parse(literal.Text));
            }

            if (literal.StringLiteral.Present)
            {
                var t = literal.Text;
                return AstConstant.Create(t.Substring(1, t.Length - 2));
            }

            throw new Exception($"Unrecognized literal: {literal.Text} of type {literal.GetType()}");
        }

        public AstNode ToAst(CstParenthesizedExpression expr)
        {
            if (expr.Expression.Nodes.Count == 0)
                throw new Exception("Expected at least one expression");

            if (expr.Expression.Nodes.Count == 1)
                return new AstParenthesized(ToAst(expr.Expression.Node));

            return ToIntrinsicInvocation("Tuple", expr.Expression.Nodes.Select(ToAst).ToArray());
        }

        public AstNode ToAst(CstLeafExpression expr)
        {
            if (expr.Identifier.Present)
            {
                return new AstIdentifier(expr.Identifier.Text);
            }
            else if (expr.Default.Present)
            {
                if (expr.Default.Node.TypeExpr.Present)
                    return ToIntrinsicInvocation("default", ToAst(expr.Default.Node.TypeExpr));
                
                // TODO: What type is this even, and how will it be figured out? 
                return ToIntrinsicInvocation("default");
            }
            else if (expr.CastExpression.Present)
            {
                return ToIntrinsicInvocation("cast", 
                    ToAst(expr.CastExpression.Node.TypeExpr),
                    ToAst(expr.CastExpression.Node.Expression));
            }
            else if (expr.LambdaExpr.Present)
            {
                var body = ToAst(expr.LambdaExpr.Node.LambdaBody);
                var parameters = expr.LambdaExpr.Node.LambdaParameters.Node.LambdaParameter.Nodes
                    .Select(ToAst).ToArray();
                return new AstLambda(body, parameters);
            }
            else if (expr.Literal.Present)
            {
                return ToAst(expr.Literal.Node);
            }
            else if (expr.NameOf.Present)
            {
                return new AstConstant(expr.NameOf.Node.Expression.Text);  
            }
            else if (expr.NewOperation.Present)
            {
                var newOp = expr.NewOperation.Node;
                if (newOp.ArraySizeSpecifier.Present)
                    throw new NotImplementedException();
                if (newOp.Initializer.Present)
                    throw new NotImplementedException();
                var args = newOp.FunctionArgs.Node.FunctionArg.Nodes
                    .Select(ToAst)
                    .Prepend(ToAst(newOp.TypeExpr));

                return ToIntrinsicInvocation("New", args.ToArray());
            }
            else if (expr.ParenthesizedExpression.Present)
            {
                return ToAst(expr.ParenthesizedExpression.Node);
            }
            else if (expr.StringInterpolation.Present)
            {
                // TODO: this actually needs to convert the interpolated parts into a literal array.
                // However, Plato doesn't support literal arrays yet. 
                return ToIntrinsicInvocation("Interpolate",
                    expr.StringInterpolation.Node.StringInterpolationContent.Nodes.Select(ToAst).ToArray());
            }
            else if (expr.ThrowExpression.Present)
            {
                return ToIntrinsicInvocation("Throw", ToAst(expr.ThrowExpression.Node.Expression));
            }
            else if (expr.TypeOf.Present)
            {
                return ToIntrinsicInvocation("TypeOf", ToAst(expr.TypeOf.Node.TypeExpr));
            }

            throw new Exception("Unrecognized leaf expression");
        }

        public AstNode ToAst(CstExpressionBody body)
        {
            return AstBlock.Create(
                AstReturn.Create(ToAst(body.Expression.Node)));
        }

        public AstTypeDeclaration ToAst(CstTopLevelDeclaration cstTopLevelDeclaration)
        {
            if (cstTopLevelDeclaration.Type.Present)
            {
                var type = cstTopLevelDeclaration.Type.Node;
                var name = ToAst(type.Identifier.Node);
                var typeParameters = type.TypeParameterList.Node?.TypeParameter.Nodes.Select(ToAst).ToArray() ?? Array.Empty<AstTypeParameter>();
                var inherits = Enumerable.Empty<AstTypeNode>();
                var implements = type.ImplementsList.Node?.TypeExpr.Nodes.Select(ToAst).ToArray();
                var members = type.FieldDeclaration.Nodes.Select(ToAst).Cast<AstMemberDeclaration>().ToArray();
                
                return new AstTypeDeclaration("type", name, typeParameters, inherits, implements, members);
            }
            else if (cstTopLevelDeclaration.Library.Present)
            {
                var module = cstTopLevelDeclaration.Library.Node;
                var name = ToAst(module.Identifier.Node);
                var typeParameters = Enumerable.Empty<AstTypeParameter>();
                var members = module.MethodDeclaration.Nodes.Select(ToAst).Cast<AstMemberDeclaration>().ToArray();

                return new AstTypeDeclaration("library", name, typeParameters, Enumerable.Empty<AstTypeNode>(), 
                    Enumerable.Empty<AstTypeNode>(), members);
            }
            else if (cstTopLevelDeclaration.Concept.Present)
            {
                var concept = cstTopLevelDeclaration.Concept.Node;

                var name = ToAst(concept.Identifier.Node);
                var typeParameters = concept.TypeParameterList.Node?.TypeParameter.Nodes.Select(ToAst).ToArray() ?? Array.Empty<AstTypeParameter>();
                var inherits = concept.InheritsList.Node.TypeExpr.Nodes.Select(ToAst).ToArray();
                var members = concept.MethodDeclaration.Nodes.Select(ToAst).ToArray();

                return new AstTypeDeclaration("concept", name, typeParameters, inherits, Enumerable.Empty<AstTypeNode>(), members);
            }

            throw new Exception("Unhandled type declaration");
        }

        public AstNode ToAst(CstNode node)
        {
            if (node == null)
                return null;

            switch (node)
            {
                // Literals 

                case CstLiteral cstLiteral:
                    return ToAst(cstLiteral.Node);
                
                case CstBinaryLiteral cstBinaryLiteral:
                    throw new NotImplementedException();

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
                    throw new NotImplementedException();

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
                    return ToIntrinsicInvocation("Cast", ToAst(cstCastExpression.Expression), ToAst(cstCastExpression.TypeExpr));

                case CstCatchClause cstCatchClause:
                    throw new NotImplementedException();

                case CstDefault cstDefault:
                    return ToIntrinsicInvocation("default", ToAst(cstDefault.TypeExpr));

                case CstExpression cstExpression:
                    return ToAst(cstExpression);
                
                case CstExpressionBody cstExpressionBody:
                    return ToAst(cstExpressionBody);

                case CstExpressionStatement cstExpressionStatement:
                    return ToAst(cstExpressionStatement.Expression);

                case CstFile cstFile:
                    return new AstNamespace(
                        null,
                        null,
                        cstFile.TopLevelDeclaration.Nodes.Select(ToAst),
                        null);

                case CstFinallyClause cstFinallyClause:
                    throw new NotImplementedException();

                case CstIdentifier cstIdentifier:
                    return new AstIdentifier(cstIdentifier.Text);

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
                        ToAst(cstLambdaParameter.TypeAnnotation) as AstTypeNode);
                
                case CstLeafExpression cstLeafExpression:
                    return ToAst(cstLeafExpression);

                case CstNameOf cstNameOf:
                    return ToIntrinsicInvocation("NameOf", ToAst(cstNameOf.Expression));

                case CstNewOperation cstNewOperation:
                    return ToIntrinsicInvocation("New", ToAst(cstNewOperation.TypeExpr));
                
                case CstParenthesizedExpression cstParenthesizedExpression:
                    return ToAst(cstParenthesizedExpression.Expression);

                case CstQualifiedIdentifier cstQualifiedIdentifier:
                    throw new NotImplementedException();

                case CstStatement cstStatement:
                    return ToAst(cstStatement.Node);
                    
                case CstStringInterpolation cstStringInterpolation:
                    return new AstInvoke(new AstIdentifier("Interpolate"),
                        cstStringInterpolation.StringInterpolationContent.Nodes.Select(ToAst).ToArray());

                case CstStringInterpolationContent cstStringInterpolationContent:
                    return ToAst(cstStringInterpolationContent.Expression);

                case CstThrowExpression cstThrowExpression:
                    return ToIntrinsicInvocation("Throw", ToAst(cstThrowExpression.Expression));

                case CstTryStatement cstTryStatement:
                    throw new NotImplementedException();

                case CstTypeArgList cstTypeArgList:
                    throw new NotImplementedException();

                case CstTypeAnnotation cstTypeDeclaration:
                    return ToAst(cstTypeDeclaration);

                case CstTypeExpr cstTypeExpr:
                    return ToAst(cstTypeExpr.InnerTypeExpr);
                    
                case CstTypeKeyword cstTypeKeyword:
                    throw new NotImplementedException();

                case CstTypeOf cstTypeOf:
                    return ToIntrinsicInvocation("typeof", ToAst(cstTypeOf.TypeExpr));

                case CstVarDecl cstVarDecl:
                    return AstMulti.Create(
                        CreateVarDefs(cstVarDecl).ToArray());
                
                case CstVarDeclStatement cstVarDeclStatement:
                    return ToAst(cstVarDeclStatement.VarDecl);

                case CstForLoopVariant cstVariantClause:
                    return ToAst(cstVariantClause.Expression);

                case CstForLoopInit forLoopInit:
                    return forLoopInit.VarDecl.Present
                        ? (AstNode)CreateVarDef(forLoopInit.VarDecl.Node, forLoopInit.VarDecl
                            .Node.VarWithInitialization.Node)
                        : AstNoop.Default;

                case CstFunctionBody fxnBody:
                    return fxnBody.CompoundStatement.Present 
                        ? ToAst(fxnBody.CompoundStatement.Node)
                        : fxnBody.ExpressionBody.Present
                        ? ToAst(fxnBody.ExpressionBody.Node)
                        : null;

                case CstInitialization cstInit:
                    return ToAst(cstInit);

                case CstFunctionArg functionArg:
                    return ToAst(functionArg);
            }

            throw new NotImplementedException();
        }

        public AstNode ToAst(CstFunctionArg functionArg)
        {
            if (functionArg.FunctionArgKeyword.Nodes.Count > 0)
                throw new NotImplementedException();
            return ToAst(functionArg.Expression.Node);
        }

        public string ToAst(CstQualifiedIdentifier cstQualifiedIdentifier)
            => cstQualifiedIdentifier.Text;

        public string ToAst(CstIdentifier cstIdentifier)
            => cstIdentifier.Text;

        public IEnumerable<AstVarDef> CreateVarDefs(CstVarDecl cstVarDecl)
        {
            foreach (var node in cstVarDecl.VarWithInitialization.Nodes)
            {
                yield return CreateVarDef(cstVarDecl, node);
            }
        }

        public AstNode ToAst(CstInitialization init)
        {
            var val = init.InitializationValue.Node;
            if (val == null)
                return null;
            if (val.Expression.Present)
            {
                if (val.ArrayInitializationValue.Present)
                    throw new Exception("Initialization expression and array initialization are both present");
                return ToAst(val.Expression);
            }
            if (val.ArrayInitializationValue.Present)
            {
                throw new NotImplementedException();
            }

            return null;
        }

        public AstVarDef CreateVarDef(CstVarDecl cstVarDecl, CstVarWithInitialization cstInit)
        {
            return AstVarDef.Create(
                cstVarDecl.VarWithInitialization.Node.Identifier.Node?.Text ?? "_",
                cstInit.Initialization.Present
                    ? ToAst(cstInit.Initialization.Node) 
                    : AstNoop.Default,
                ToAst(cstVarDecl.TypeExpr.Node));
        }
    }

}
