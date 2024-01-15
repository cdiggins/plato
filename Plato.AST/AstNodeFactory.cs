using System;
using System.Collections.Generic;
using System.Linq;
using Parakeet.CST;
using Plato.Parser;

namespace Plato.AST
{
    public static class AstNodeFactory
    {
        public static AstNode ToIntrinsicInvocation(this CstNode context, string name, params AstNode[] args)
            => new AstInvoke(context, new AstIdentifier(context, name), args);

        public static AstNode ToAst<T>(this CstFilter<T> filter) where T : CstNode
            => ToAst(filter.Node);

        public static AstNode ToAst(this CstExpression expr)
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

                if (postfix.BinaryOperation.Present)
                {
                    var op = postfix.BinaryOperation.Node.BinaryOperator.Node.Text;
                    r = ToIntrinsicInvocation(
                        postfix.BinaryOperation,
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
                    r = new AstInvoke(expr, r,
                        postfix.FunctionArgs.Node.FunctionArg.Nodes.Select(n => ToAst<CstExpression>(n.Expression)).ToArray());
                }
                else if (postfix.Indexer.Present)
                {
                    r = ToIntrinsicInvocation(postfix.Indexer, OperatorNameLookup.BinaryOperatorToName("[]")
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
                    r = new AstInvoke(expr, new AstIdentifier(expr, func), args.ToArray());
                }
                else if (postfix.TernaryOperation.Present)
                {
                    r = new AstConditional(
                        expr,
                        r,
                        ToAst(postfix.TernaryOperation.Node[0]),
                        ToAst(postfix.TernaryOperation.Node[1]));
                }
            }

            foreach (var prefix in expr.PrefixOperator.Nodes)
            {
                r = ToIntrinsicInvocation(prefix,
                    OperatorNameLookup.UnaryOperatorToName(prefix.Text.Trim()), r);
            }

            return r;
        }

        public static IReadOnlyList<AstParameterDeclaration> ToAstList(this CstLambdaParameters parameters)
        {
            var list = new List<AstParameterDeclaration>();
            if (parameters.LambdaParameter != null)
                list.Add(ToAst(parameters.LambdaParameter.Node, 0));

            foreach (var lp in parameters.LambdaParameter.Nodes)
                list.Add(ToAst(lp, list.Count));

            return list;
        }

        public static AstTypeNode ToAst(this CstTypeExpr type)
        {
            return type == null ? null : ToAst(type.InnerTypeExpr.Node);
        }

        public static AstTypeNode ToAst(this CstCompoundTypeExpr typeExpr)
        {
            return new AstTypeNode(typeExpr, "ValueTuple", typeExpr.TypeExpr.Nodes.Select(ToAst).ToArray());
        }

        public static AstTypeNode ToAst(this CstInnerTypeExpr innerType)
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
                return new AstTypeNode(innerType, r.Name, ps.ToArray());

            return r;
        }

        public static AstTypeNode ToAst(this CstSimpleTypeExpr type) => new AstTypeNode(type, type.Text);

        public static AstTypeNode ToAst(this CstTypeVar type) => new AstTypeNode(type, "$" + type.Text);

        public static AstTypeNode ToAst(this CstCompoundOrSimpleTypeExpr compOrSimple)
        {
            if (compOrSimple == null)
                return null;

            if (compOrSimple.CompoundTypeExpr.Present)
                return ToAst(compOrSimple.CompoundTypeExpr.Node);

            if (compOrSimple.SimpleTypeExpr.Present)
                return ToAst(compOrSimple.SimpleTypeExpr.Node);

            if (compOrSimple.TypeVar.Present)
                return ToAst(compOrSimple.TypeVar.Node);

            throw new Exception("Missing compound type, simple type, or type variable");
        }

        public static AstNode ToAst(this CstForEachStatement forEach)
        {
            var cstIdent = forEach.Identifier.Node;
            var cstType = forEach.TypeExpr.Node;
            var name = ToAst(cstIdent);
            var astType = ToAst(cstType);
            var vd = new AstVarDef(forEach.Identifier, name, null, astType);
            return new AstBlock(
                forEach.Statement,
                vd,
                new AstLoop(
                    forEach,
                    ToIntrinsicInvocation("MoveNext", null),
                    ToAst(forEach.Statement)));
        }

        public static AstTypeParameter ToAst(this CstTypeParameter typeParameter)
            => new AstTypeParameter(typeParameter, typeParameter.Identifier.Node.Text);

        public static AstTypeNode ToAst(this CstTypeAnnotation typeAnnotation)
            => ToAst(typeAnnotation?.TypeExpr?.Node?.InnerTypeExpr?.Node);

        public static AstParameterDeclaration ToAst(this CstFunctionParameter fp, int index)
            => new AstParameterDeclaration(
                fp,
                ToAst(fp.Identifier.Node),
                ToAst(fp.TypeAnnotation.Node), 
                index);

        public static AstFieldDeclaration ToAst(this CstFieldDeclaration fieldDeclaration)
        {
            var name = ToAst(fieldDeclaration.Identifier.Node);
            var type = ToAst(fieldDeclaration.TypeExpr.Node);
            return new AstFieldDeclaration(fieldDeclaration, name, type, null);
        }

        public static AstConstraint ToAst(this CstConstraint constraint)
        {
            return new AstConstraint(constraint, constraint.Identifier.Text, ToAst(constraint.TypeAnnotation.Node));
        }

        public static AstMethodDeclaration ToAst(this CstMethodDeclaration md)
        {
            return new AstMethodDeclaration(
                md,
                ToAst(md.Identifier.Node),
                ToAst(md.TypeAnnotation.Node),
                md.FunctionParameterList.Node.FunctionParameter.Nodes.Select(ToAst).ToList(),
                ToAst(md.FunctionBody.Node));
        }

        public static AstMemberDeclaration ToAst(this CstMemberDeclaration memberDeclaration)
        {
            if (memberDeclaration.MethodDeclaration.Present)
                return ToAst(memberDeclaration.MethodDeclaration.Node);
            if (memberDeclaration.FieldDeclaration.Present)
                return ToAst(memberDeclaration.FieldDeclaration.Node);
            
            throw new Exception($"Unrecognized member type {memberDeclaration.Text}");  
        }

        public static AstParameterDeclaration ToAst(this CstLambdaParameter lp, int index)
        {
            return new AstParameterDeclaration(lp, 
                ToAst(lp.Identifier.Node), null, index);
        }

        public static AstConstant ToAst(this CstLiteral literal)
        {
            if (literal.NullLiteral.Present)
            {
                throw new NotImplementedException();
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

        public static AstNode ToAst(this CstParenthesizedExpression expr)
        {
            if (expr.Expression.Nodes.Count == 0)
                throw new Exception("Expected at least one expression");

            if (expr.Expression.Nodes.Count == 1)
                return new AstParenthesized(expr, ToAst(expr.Expression.Node));

            return ToIntrinsicInvocation(expr, "Tuple", expr.Expression.Nodes.Select(ToAst).ToArray());
        }

        public static AstNode ToAst(this CstLeafExpression expr)
        {
            if (expr.Identifier.Present)
            {
                return new AstIdentifier(expr, expr.Identifier.Text);
            }

            if (expr.Default.Present)
            {
                if (expr.Default.Node.TypeExpr.Present)
                    return ToIntrinsicInvocation(expr, "default", ToAst(expr.Default.Node.TypeExpr));

                // TODO: What type is this even, and how will it be figured out? 
                return ToIntrinsicInvocation(expr, "default");
            }
            if (expr.CastExpression.Present)
            {
                return ToIntrinsicInvocation(expr, "cast",
                    ToAst(expr.CastExpression.Node.TypeExpr),
                    ToAst(expr.CastExpression.Node.Expression));
            }
            if (expr.LambdaExpr.Present)
            {
                var body = ToAst(expr.LambdaExpr.Node.LambdaBody);
                var parameters = expr.LambdaExpr.Node.LambdaParameters.Node.LambdaParameter.Nodes
                    .Select(ToAst).ToArray();
                return new AstLambda(expr.LambdaExpr, body, parameters);
            }
            if (expr.Literal.Present)
            {
                return ToAst(expr.Literal.Node);
            }
            if (expr.NameOf.Present)
            {
                return AstConstant.Create(expr.NameOf.Node.Expression.Text);
            }
            if (expr.NewOperation.Present)
            {
                var newOp = expr.NewOperation.Node;
                if (newOp.ArraySizeSpecifier.Present)
                    throw new NotImplementedException();
                if (newOp.Initializer.Present)
                    throw new NotImplementedException();
                var args = newOp.FunctionArgs.Node.FunctionArg.Nodes
                    .Select(ToAst)
                    .Prepend(ToAst(newOp.TypeExpr));

                return ToIntrinsicInvocation(expr.NewOperation, "New", args.ToArray());
            }
            if (expr.ParenthesizedExpression.Present)
            {
                return ToAst(expr.ParenthesizedExpression.Node);
            }
            if (expr.StringInterpolation.Present)
            {
                // TODO: this actually needs to convert the interpolated parts into a literal array.
                // However, Plato doesn't support literal arrays yet. 
                return ToIntrinsicInvocation(expr.StringInterpolation, "Interpolate",
                    expr.StringInterpolation.Node.StringInterpolationContent.Nodes.Select(ToAst).ToArray());
            }
            if (expr.ThrowExpression.Present)
            {
                return ToIntrinsicInvocation(expr.ThrowExpression, "Throw", ToAst(expr.ThrowExpression.Node.Expression));
            }
            if (expr.TypeOf.Present)
            {
                return ToIntrinsicInvocation(expr.TypeOf, "TypeOf", ToAst(expr.TypeOf.Node.TypeExpr));
            }

            throw new Exception("Unrecognized leaf expression");
        }

        public static AstNode ToAst(this CstExpressionBody body)
        {
            return new AstBlock(body, 
                new AstReturn(body.Expression, ToAst(body.Expression.Node)));
        }

        public static AstTypeDeclaration ToAst(this CstTopLevelDeclaration cstTopLevelDeclaration)
        {
            if (cstTopLevelDeclaration.Type.Present)
            {
                var type = cstTopLevelDeclaration.Type.Node;
                var name = ToAst(type.Identifier.Node);
                var typeParameters = type.TypeParameterList.Node?.TypeParameter.Nodes.Select(ToAst).ToArray() ?? Array.Empty<AstTypeParameter>();
                var inherits = Enumerable.Empty<AstTypeNode>();
                var implements = type.ImplementsList.Node?.TypeExpr.Nodes.Select(ToAst).ToArray() ??
                                 Array.Empty<AstTypeNode>();
                var members = type.FieldDeclaration.Nodes.Select(ToAst).Cast<AstMemberDeclaration>().ToArray();

                return new AstTypeDeclaration(cstTopLevelDeclaration, TypeKind.ConcreteType, name, typeParameters, inherits, implements, Array.Empty<AstConstraint>(), members);
            }

            if (cstTopLevelDeclaration.Library.Present)
            {
                var module = cstTopLevelDeclaration.Library.Node;
                var name = ToAst(module.Identifier.Node);
                var typeParameters = Enumerable.Empty<AstTypeParameter>();
                var members = module.MethodDeclaration.Nodes.Select(ToAst).Cast<AstMemberDeclaration>().ToArray();

                return new AstTypeDeclaration(cstTopLevelDeclaration, TypeKind.Library, name, typeParameters, Enumerable.Empty<AstTypeNode>(),
                    Enumerable.Empty<AstTypeNode>(), Array.Empty<AstConstraint>(), members);
            }
            if (cstTopLevelDeclaration.Concept.Present)
            {
                var concept = cstTopLevelDeclaration.Concept.Node;

                var name = ToAst(concept.Identifier.Node);
                var typeParameters = concept.TypeParameterList.Node?.TypeParameter.Nodes.Select(ToAst).ToArray() ?? Array.Empty<AstTypeParameter>();
                var inherits = concept.InheritsList.Node?.TypeExpr.Nodes.Select(ToAst).ToArray() ?? Array.Empty<AstTypeNode>();
                var members = concept.MethodDeclaration.Nodes.Select(ToAst).ToArray();
                var constraints = concept.ConstraintList.Node?.Constraint.Nodes.Select(ToAst).ToArray() ?? Array.Empty<AstConstraint>();

                return new AstTypeDeclaration(cstTopLevelDeclaration, TypeKind.Concept, name, typeParameters, inherits, Enumerable.Empty<AstTypeNode>(), constraints, members);
            }

            throw new Exception("Unhandled type declaration");
        }

        public static AstNode ToAst(this CstNode node)
        {
            if (node == null)
                return null;

            switch (node)
            {
                // Literals 

                case CstLiteral cstLiteral:
                    return ToAst(cstLiteral.Node);

                case CstBinaryLiteral _:
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

                case CstNullLiteral _:
                    throw new NotImplementedException("null");

                case CstIntegerLiteral cstIntegerLiteral:
                    return AstConstant.Create(int.Parse(cstIntegerLiteral.Text));

                case CstStringLiteral cstStringLiteral:
                    return AstConstant.Create(
                        cstStringLiteral.Text.Substring(1, cstStringLiteral.Text.Length - 2));

                case CstHexLiteral _:
                    throw new NotImplementedException();

                // Statements

                case CstBreakStatement _:
                    return AstBreak.Default;

                case CstCompoundStatement cstCompoundStatement:
                    return new AstBlock(cstCompoundStatement, cstCompoundStatement.Statement.Children.Select(ToAst).ToArray());

                case CstDoWhileStatement cstDoWhileStatement:
                    return new AstMulti(cstDoWhileStatement,
                        ToAst(cstDoWhileStatement.Statement),
                        new AstLoop(
                            cstDoWhileStatement,
                            ToAst(cstDoWhileStatement.ParenthesizedExpression),
                            ToAst(cstDoWhileStatement.Statement)));

                case CstForEachStatement cstForEachStatement:
                    return ToAst(cstForEachStatement);

                case CstForStatement cstForStatement:
                    return new AstBlock(cstForStatement,
                        ToAst(cstForStatement.ForLoopInit),
                        new AstLoop(cstForStatement, ToAst(cstForStatement.ForLoopInvariant),
                            new AstMulti(cstForStatement, ToAst(cstForStatement.Statement), ToAst(cstForStatement.ForLoopVariant))));

                case CstReturnStatement cstReturnStatement:
                    return new AstReturn(cstReturnStatement, ToAst(cstReturnStatement.Expression));

                case CstWhileStatement cstWhileStatement:
                    return new AstLoop(cstWhileStatement, ToAst(cstWhileStatement.ParenthesizedExpression), ToAst(cstWhileStatement.Statement));

                case CstContinueStatement _:
                    return AstContinue.Default;

                case CstCastExpression cstCastExpression:
                    return ToIntrinsicInvocation(cstCastExpression, "Cast", ToAst(cstCastExpression.Expression), ToAst(cstCastExpression.TypeExpr));

                case CstCatchClause _:
                    throw new NotImplementedException();

                case CstDefault cstDefault:
                    return ToIntrinsicInvocation(cstDefault, "default", ToAst(cstDefault.TypeExpr));

                case CstExpression cstExpression:
                    return ToAst(cstExpression);

                case CstExpressionBody cstExpressionBody:
                    return ToAst(cstExpressionBody);

                case CstExpressionStatement cstExpressionStatement:
                    return ToAst(cstExpressionStatement.Expression);

                case CstFile cstFile:
                    return new AstNamespace(cstFile, "", cstFile.TopLevelDeclaration.Nodes.Select(ToAst));

                case CstFinallyClause _:
                    throw new NotImplementedException();

                case CstIdentifier cstIdentifier:
                    return new AstIdentifier(cstIdentifier, cstIdentifier.Text);

                case CstIfStatement cstIfStatement:
                    return new AstConditional(cstIfStatement, ToAst(cstIfStatement.ParenthesizedExpression),
                        ToAst(cstIfStatement.Statement),
                        ToAst(cstIfStatement.ElseClause));

                case CstElseClause cstElseClause:
                    return ToAst(cstElseClause.Statement);

                case CstInitializer _:
                    throw new NotImplementedException();

                case CstInitializerClause _:
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
                    return new AstLambda(cstLambdaExpr, ToAst(cstLambdaExpr.LambdaBody),
                        ToAstList(cstLambdaExpr.LambdaParameters.Node).ToArray());

                case CstLambdaParameter cstLambdaParameter:
                    return new AstVarDef(cstLambdaParameter,
                        cstLambdaParameter.Identifier.Node.Text,
                        null,
                        null);

                case CstLeafExpression cstLeafExpression:
                    return ToAst(cstLeafExpression);

                case CstNameOf cstNameOf:
                    return ToIntrinsicInvocation(cstNameOf, "NameOf", ToAst(cstNameOf.Expression));

                case CstNewOperation cstNewOperation:
                    return ToIntrinsicInvocation(cstNewOperation, "New", ToAst(cstNewOperation.TypeExpr));

                case CstParenthesizedExpression cstParenthesizedExpression:
                    return ToAst(cstParenthesizedExpression.Expression);

                case CstQualifiedIdentifier _:
                    throw new NotImplementedException();

                case CstStatement cstStatement:
                    return ToAst(cstStatement.Node);

                case CstStringInterpolation cstStringInterpolation:
                    return new AstInvoke(cstStringInterpolation, new AstIdentifier(cstStringInterpolation, "Interpolate"),
                        cstStringInterpolation.StringInterpolationContent.Nodes.Select(ToAst).ToArray());

                case CstStringInterpolationContent cstStringInterpolationContent:
                    return ToAst(cstStringInterpolationContent.Expression);

                case CstThrowExpression cstThrowExpression:
                    return ToIntrinsicInvocation(cstThrowExpression, "Throw", ToAst(cstThrowExpression.Expression));

                case CstTryStatement _:
                    throw new NotImplementedException();

                case CstTypeArgList _:
                    throw new NotImplementedException();

                case CstTypeAnnotation cstTypeDeclaration:
                    return ToAst(cstTypeDeclaration);

                case CstTypeExpr cstTypeExpr:
                    return ToAst(cstTypeExpr.InnerTypeExpr);

                case CstTypeKeyword _:
                    throw new NotImplementedException();

                case CstTypeOf cstTypeOf:
                    return ToIntrinsicInvocation(cstTypeOf, "typeof", ToAst(cstTypeOf.TypeExpr));

                case CstVarDecl cstVarDecl:
                    return new AstMulti(
                        cstVarDecl,
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

        public static AstNode ToAst(this CstFunctionArg functionArg)
        {
            if (functionArg.FunctionArgKeyword.Nodes.Count > 0)
                throw new NotImplementedException();
            return ToAst(functionArg.Expression.Node);
        }

        public static string ToAst(this CstQualifiedIdentifier cstQualifiedIdentifier)
            => cstQualifiedIdentifier.Text;

        public static string ToAst(this CstIdentifier cstIdentifier)
            => cstIdentifier.Text;

        public static IEnumerable<AstVarDef> CreateVarDefs(this CstVarDecl cstVarDecl)
        {
            foreach (var node in cstVarDecl.VarWithInitialization.Nodes)
                yield return CreateVarDef(cstVarDecl, node);
        }

        public static AstNode ToAst(this CstInitialization init)
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

        public static AstVarDef CreateVarDef(this CstVarDecl cstVarDecl, CstVarWithInitialization cstInit)
        {
            return new AstVarDef(
                cstVarDecl,
                cstVarDecl.VarWithInitialization.Node.Identifier.Node?.Text ?? "_",
                cstInit.Initialization.Present
                    ? ToAst(cstInit.Initialization.Node)
                    : AstNoop.Default,
                ToAst(cstVarDecl.TypeExpr.Node));
        }
    }
}