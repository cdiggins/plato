using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Parakeet;
using Parakeet.Demos.CSharp;

namespace PlatoAst
{
    public class AstFromCSharpCst
    {
        public AstNode ToIntrinsic(string name, params AstNode[] args)
        {
            if (args.Length == 0) 
                return AstInvoke.Create(AstIntrinsic.Create(name));
            return AstInvoke.Create(
                AstMemberAccess.Create(args[0], name), 
                args.Skip(1).ToArray());

        }

        public AstNode ToAst<T>(CstFilter<T> filter) where T: CstNode
            => ToAst(filter.Node);

        public AstNode ToAst(CstExpression expr)
        {
            var r = ToAst(expr.LeafExpression.Node);
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
                else if (postfix.BinaryOperation.Present)
                {
                    r = ToIntrinsic(
                        postfix.BinaryOperation.Node.BinaryOperator.Node.Text,
                        r,   
                        ToAst(postfix.BinaryOperation.Node.Expression));
                }
                else if (postfix.ConditionalMemberAccess.Present)
                {
                    throw new NotImplementedException();
                    // 
                    //r = ToIntrinsic("?.", r, postfix.MemberAccess.Node.Identifier.Node.Text.ToConstant());
                }
                else if (postfix.FunctionArgs.Present)
                {
                    r = AstInvoke.Create(r, 
                        postfix.FunctionArgs.Node.FunctionArg.Nodes.Select(n => ToAst(n.Expression)).ToArray());
                }
                else if (postfix.Indexer.Present)
                {
                    r = ToIntrinsic("[]", r, ToAst(postfix.Indexer.Node.Expression));
                }
                else if (postfix.IsOperation.Present)
                {
                    throw new NotImplementedException();
                }
                else if (postfix.MemberAccess.Present)
                {
                    r = new AstMemberAccess(r, 
                        ToAst(postfix.MemberAccess.Node.Identifier.Node));
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
                r = ToIntrinsic(prefix.Text, r);
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
            if (innerType.CompoundOrSimpleTypeExpr.Node?.CompoundTypeExpr.Present == true)
            {
                return ToAst(innerType.CompoundOrSimpleTypeExpr.Node.CompoundTypeExpr.Node);
            }

            var qi = innerType.CompoundOrSimpleTypeExpr.Node?.SimpleTypExpr.Node?.QualifiedIdentifier;

            if (qi == null)
                return AstTypeNode.Create("_UNTYPED_");

            var name = string.Join(".", qi.Node.Identifier.Nodes.Select(n => n.Text));

            if (innerType.ArrayRankSpecifiers.Node.ArrayRankSpecifier.Nodes.Count > 0)
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
            var cstIdent = forEach.Identifier.Node;
            var cstType = forEach.TypeExpr.Node;
            var astIdent = ToAst(cstIdent);
            var astType = ToAst(cstType);
            var vd = AstVarDef.Create(astIdent, astType);
            return AstBlock.Create(
                vd,
                AstLoop.Create(
                    ToIntrinsic("MoveNext", AstIdentifier.Create(astIdent.Text)),
                    ToAst(forEach.Statement)));
        }

        public AstIdentifier ToAst(CstTypeParameter typeParameter)
            => ToAst(typeParameter.Identifier.Node);

        public AstParameterDeclaration ToAst(CstFunctionParameter fp)
        {
            return new AstParameterDeclaration(
                ToAst(fp.Identifier.Node),
                ToAst(fp.TypeExpr.Node));
        }

        public IEnumerable<AstFieldDeclaration> ToAst(CstFieldDeclaration fieldDeclaration)
        {
            var varDecl = fieldDeclaration.VarDeclStatement.Node.VarDecl.Node;
            var type = ToAst(varDecl.TypeExpr.Node);
            foreach (var node in varDecl.VarWithInitialization.Nodes)
            {
                var name = ToAst(node.Identifier.Node);
                var val = ToAst(node.Initialization.Node);
                yield return new AstFieldDeclaration(name, type, val);
            }
        }

        public IEnumerable<AstMemberDeclaration> ToAst(CstMemberDeclaration memberDeclaration)
        {
            // AttributeList
            // Modifier
            // AccessSpecifier

            var attributes = memberDeclaration.DeclarationPreamble.Node?.AttributeList.Nodes;
            var acc = memberDeclaration.DeclarationPreamble.Node?.AccessSpecifier.Nodes;
            var mod = memberDeclaration.DeclarationPreamble.Node?.Modifier.Nodes;

            // TODO: validate that those things work 

            if (memberDeclaration.TypeDeclaration.Present)
            {
                throw new NotImplementedException();
            }
            else if (memberDeclaration.MethodDeclaration.Present)
            {
                var md = memberDeclaration.MethodDeclaration.Node;
                var name = md.Identifier.Node.Text;
                var ps = md.FunctionParameterList.Node.FunctionParameter.Nodes.Select(ToAst).ToList();
                Debug.WriteLine($"TODO: need to properly handle parameterized functions. I don't thing they parse.");
                var ts = Enumerable.Empty<AstIdentifier>();
                yield return new AstMethodDeclaration(
                    ToAst(md.Identifier.Node),
                    ToAst(md.TypeExpr.Node),
                    ps,
                    ts,
                    ToAst(md.FunctionBody.Node));
                    // parameters
                    // type parameters
                    // body
            }
            else if (memberDeclaration.ConstructorDeclaration.Present)
            {
                throw new NotImplementedException();
            }
            else if (memberDeclaration.ConverterDeclaration.Present)
            {
                throw new NotImplementedException();
            }
            else if (memberDeclaration.FieldDeclaration.Present)
            {
                var fields = ToAst(memberDeclaration.FieldDeclaration.Node);
                foreach (var x in fields)
                    yield return x;
            }
            else if (memberDeclaration.IndexerDeclaration.Present)
            {
                throw new NotImplementedException();
            }
            else if (memberDeclaration.OperatorDeclaration.Present)
            {
                throw new NotImplementedException();
            }
            else if (memberDeclaration.PropertyDeclaration.Present)
            {
                throw new NotImplementedException();
            }
            else
            {
                throw new Exception("Unrecognized member type");
            }
        }

        public IEnumerable<CstAttribute> GetAttributes(CstAttributeList list)
        {
            if (list == null) 
                yield break;
            foreach (var grp in list.AttributeGroup.Nodes)
            {
                foreach (var attr in grp.Attribute.Nodes)
                    yield return attr;
            }
        }

        public AstTypeDeclaration ToAst(CstTypeDeclarationWithPreamble typeDecl)
        {
            var attributes = new List<AstAttribute>();
            if (typeDecl.DeclarationPreamble.Present)
            {
                var preamble = typeDecl.DeclarationPreamble.Node;
                foreach (var attr in GetAttributes(preamble.AttributeList.Node))
                {
                    attributes.Add(new AstAttribute(attr.Text));
                }
            }
            Debug.WriteLine("TODO: store the attributes (preamble)");
            var td = typeDecl.TypeDeclaration.Node;
            return new AstTypeDeclaration(
                ToAst(td.Identifier.Node),
                td.TypeParameterList.Node?.TypeParameter.Nodes.Select(ToAst) 
                    ?? Enumerable.Empty<AstIdentifier>(),
                td.BaseClassList.Node?.TypeExpr.Nodes.Select(ToAst)
                    ?? Enumerable.Empty<AstTypeNode>(),
                attributes,
                td.MemberDeclaration.Nodes.SelectMany(ToAst).ToArray());
        }

        public AstDirective ToAst(CstUsingDirective usingDirective)
        {
            return new AstDirective("using", usingDirective.QualifiedIdentifier.Text);
        }

        public AstNamespace ToAst(CstNamespaceDeclaration cstNamespaceDeclaration)
        {
            return new AstNamespace(
                ToAst(cstNamespaceDeclaration.QualifiedIdentifier.Node),
                cstNamespaceDeclaration.UsingDirective.Nodes.Select(ToAst),
                cstNamespaceDeclaration.TypeDeclarationWithPreamble.Nodes.Select(ToAst),

                // TODO: this will have to be implemented when passed a source file with nested namespaces
                Enumerable.Empty<AstNamespace>()
            );
        }

        public AstParameterDeclaration ToAst(CstLambdaParameter lp)
        {
            return new AstParameterDeclaration(
                ToAst(lp.Identifier.Node), ToAst(lp.TypeExpr.Node));
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

            return AstInvoke.Create(
                AstIntrinsic.Create("Tuple"), expr.Expression.Nodes.Select(ToAst).ToArray());
        }

        public AstNode ToAst(CstLeafExpression expr)
        {
            if (expr.Identifier.Present)
            {
                return AstIdentifier.Create(expr.Identifier.Text);
            }
            else if (expr.Default.Present)
            {
                if (expr.Default.Node.TypeExpr.Present)
                    return ToIntrinsic("default", ToAst(expr.Default.Node.TypeExpr));
                
                // TODO: What type is this even, and how will it be figured out? 
                return ToIntrinsic("default");
            }
            else if (expr.CastExpression.Present)
            {
                return ToIntrinsic("cast", 
                    ToAst(expr.CastExpression.Node.TypeExpr),
                    ToAst(expr.CastExpression.Node.Expression));
            }
            else if (expr.LambdaExpr.Present)
            {
                var body = ToAst(expr.LambdaExpr.Node.LambdaBody);
                var parameters = expr.LambdaExpr.Node.LambdaParameters.Node.LambdaParameter.Nodes
                    .Select(ToAst).ToArray();
                return new AstLambda(body, parameters);
                throw new NotImplementedException();
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

                return ToIntrinsic("new", args.ToArray());
            }
            else if (expr.ParenthesizedExpression.Present)
            {
                return ToAst(expr.ParenthesizedExpression.Node);
            }
            else if (expr.StringInterpolation.Present)
            {
                return ToIntrinsic("interpolate",
                    expr.StringInterpolation.Node.StringInterpolationContent.Nodes.Select(ToAst).ToArray());
            }
            else if (expr.ThrowExpression.Present)
            {
                return ToIntrinsic("throw", ToAst(expr.ThrowExpression.Node.Expression));
            }
            else if (expr.TypeOf.Present)
            {
                return ToIntrinsic("typeof", ToAst(expr.TypeOf.Node.TypeExpr));
            }

            throw new Exception("Unrecognized leaf expression");
        }

        public AstNode ToAst(CstExpressionBody body)
        {
            if (body.CompoundStatement.Present)
                return ToAst(body.CompoundStatement.Node);
            if (!body.Expression.Present)
                throw new Exception("Invalid expression body");
            return AstBlock.Create(
                AstReturn.Create(ToAst(body.Expression.Node)));
        }

        public AstNode ToAst(CstNode node)
        {
            if (node == null)
                return null;

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
                    return ToAst(cstExpressionBody);

                case CstExpressionStatement cstExpressionStatement:
                    return ToAst(cstExpressionStatement.Expression);

                case CstNamespaceDeclaration cstNamespaceDeclaration:
                    return ToAst(cstNamespaceDeclaration);

                case CstFile cstFile:
                    return new AstNamespace(
                        null, 
                        cstFile.UsingDirective.Nodes.Select(ToAst),
                        Enumerable.Empty<AstTypeDeclaration>(),
                        cstFile.NamespaceDeclaration.Nodes.Select(ToAst));
                
                case CstScript cstScript:
                    return AstMulti.Create(cstScript.TypeDirectiveOrStatement.Nodes.Select(ToAst).ToArray());

                case CstFinallyClause cstFinallyClause:
                    throw new NotImplementedException();

                case CstIdentifier cstIdentifier:
                    return AstIdentifier.Create(cstIdentifier.Text);

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
                    return ToAst(cstLeafExpression);

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
                    return AstMulti.Create(
                        CreateVarDefs(cstVarDecl).ToArray());
                
                case CstVarDeclStatement cstVarDeclStatement:
                    return ToAst(cstVarDeclStatement.VarDecl);

                case CstForLoopVariant cstVariantClause:
                    return ToAst(cstVariantClause.Expression);

                case CstTypeDirectiveOrStatement cstTypeDirectiveOrStatement:
                    return ToAst(cstTypeDirectiveOrStatement.Node);

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
                        : throw new Exception();

                case CstInitialization cstInit:
                    return ToAst(cstInit);

                case CstFunctionArg functionArg:
                    return ToAst(functionArg);
            }

            return AstError.Create($"{node.GetType()} had no case statement");
        }

        public AstNode ToAst(CstFunctionArg functionArg)
        {
            if (functionArg.FunctionArgKeyword.Nodes.Count > 0)
                throw new NotImplementedException();
            return ToAst(functionArg.Expression.Node);
        }

        public AstIdentifier ToAst(CstQualifiedIdentifier cstQualifiedIdentifier)
            => new AstIdentifier(cstQualifiedIdentifier.Text);

        public AstIdentifier ToAst(CstIdentifier cstIdentifier)
            => new AstIdentifier(cstIdentifier.Text);

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
