using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using ISymbol = Microsoft.CodeAnalysis.ISymbol;
using SyntaxNode = Microsoft.CodeAnalysis.SyntaxNode;

namespace PlatoAnalyzer
{
    public static class PlatoSyntaxExtensions
    {
        public static PlatoExpression Rewrite(this PlatoExpression self, Func<PlatoStatement, PlatoStatement> func)
            => self.Rewrite((PlatoExpression expr) =>
                expr is PlatoLambda lambda
                    ? new PlatoLambda(lambda.Id, lambda.Type, lambda.Parameters, lambda.Body.Rewrite(func))
                    : expr
            );

        public static PlatoTypeExpr Rewrite(this PlatoTypeExpr type, Func<PlatoExpression, PlatoExpression> func)
            => ((PlatoExpression)type).Rewrite(func) as PlatoTypeExpr;

        public static PlatoArgList Rewrite(this PlatoArgList self, Func<PlatoExpression, PlatoExpression> func)
            => new PlatoArgList(self.Id, self.Args.Select(x => x.Rewrite(func) as PlatoArg));

        public static PlatoExpression Rewrite(this PlatoExpression self, Func<PlatoExpression, PlatoExpression> func)
        {
            //Debug.WriteLine(self?.ToString());
            switch (self)
            {
                case PlatoNameOf platoNameOf:
                    return func(platoNameOf);
                case PlatoTypeOf platoTypeOf:
                    return func(platoTypeOf);
                case PlatoArg platoArg:
                    return func(new PlatoArg(
                        platoArg.Id, 
                        platoArg.Type, 
                        platoArg.Value.Rewrite(func), 
                        platoArg.Name));
                case PlatoArray platoArray:
                    return func(new PlatoArray(
                        platoArray.Id, 
                        platoArray.ElementType,
                        platoArray.Size.Rewrite(func), 
                        platoArray.Elements.Select(x => x.Rewrite(func))));
                case PlatoAssignment platoAssignment:
                    return func(new PlatoAssignment(
                        platoAssignment.Id, 
                        platoAssignment.Operator,
                        platoAssignment.Left.Rewrite(func), 
                        platoAssignment.Right.Rewrite(func)));
                case PlatoBinary platoBinary:
                    return func(new PlatoBinary(
                        platoBinary.Id, 
                        platoBinary.Type, 
                        platoBinary.Operator,
                        platoBinary.Left.Rewrite(func), 
                        platoBinary.Right.Rewrite(func)));
                case PlatoCast platoCast:
                    return func(new PlatoCast(
                        platoCast.Id, 
                        platoCast.Type, 
                        platoCast.Expression.Rewrite(func)));
                case PlatoConditional platoConditional:
                    return func(new PlatoConditional(
                        platoConditional.Id, 
                        platoConditional.Type, 
                        platoConditional.Condition.Rewrite(func),
                        platoConditional.OnTrue.Rewrite(func), 
                        platoConditional.OnFalse.Rewrite(func)));
                case PlatoDefault platoDefault:
                    return func(platoDefault);
                case PlatoElementGet platoElementGet:
                    return func(new PlatoElementGet(
                        platoElementGet.Id, 
                        platoElementGet.Type,
                        platoElementGet.Receiver.Rewrite(func), 
                        platoElementGet.Index.Rewrite(func)));
                case PlatoElementSet platoElementSet:
                    return func(new PlatoElementSet(
                        platoElementSet.Id, 
                        platoElementSet.Left.Rewrite(func) as PlatoElementGet, 
                        platoElementSet.Right.Rewrite(func)));
                case PlatoIdentifierRef platoIdentifierRef:
                    return func(platoIdentifierRef);
                case PlatoInterpolation platoInterpolation:
                    return func(new PlatoInterpolation(
                        platoInterpolation.Id,
                        platoInterpolation.Expressions.Select(x => x.Rewrite(func))));
                case PlatoInvoke platoInvoke:
                    return func(new PlatoInvoke(
                        platoInvoke.Id, 
                        platoInvoke.Type, 
                        platoInvoke.Function.Rewrite(func),
                        platoInvoke.Reciever.Rewrite(func),
                        platoInvoke.Args.Rewrite(func)));
                case PlatoLambda platoLambda:
                    return func(new PlatoLambda(
                        platoLambda.Id, 
                        platoLambda.Type, 
                        platoLambda.Parameters,
                        platoLambda.Body.Rewrite(func)));
                case PlatoLiteral platoLiteral:
                    return func(platoLiteral);
                case PlatoMemberGet platoMemberGet:
                    return func(new PlatoMemberGet(
                        platoMemberGet.Id, 
                        platoMemberGet.Type,
                        platoMemberGet.Receiver.Rewrite(func), 
                        platoMemberGet.Name));
                case PlatoMemberSet platoMemberSet:
                    return func(new PlatoMemberSet(
                        platoMemberSet.Id, 
                        platoMemberSet.Left.Rewrite(func) as PlatoMemberGet,
                        platoMemberSet.Right.Rewrite(func)));
                case PlatoNew platoNew:
                    return func(new PlatoNew(
                        platoNew.Id, 
                        platoNew.Type, 
                        platoNew.Args.Rewrite(func),
                        platoNew.Initializers.Select(x => x.Rewrite(func))));
                case PlatoParenthesis platoParenthesis:
                    return func(new PlatoParenthesis(
                        platoParenthesis.Id, 
                        platoParenthesis.Expression.Rewrite(func)));
                case PlatoPostfix platoPostfix:
                    return func(new PlatoPostfix(
                        platoPostfix.Id, 
                        platoPostfix.Type, 
                        platoPostfix.Operator,
                        platoPostfix.Operand.Rewrite(func)));
                case PlatoPrefix platoPrefix:
                    return func(new PlatoPrefix(
                        platoPrefix.Id, 
                        platoPrefix.Type, 
                        platoPrefix.Operator,
                        platoPrefix.Operand.Rewrite(func)));
                case PlatoThis platoThis:
                    return func(platoThis);
                case PlatoThrowExpression platoThrowExpression:
                    return func(new PlatoThrowExpression(
                        platoThrowExpression.Id,
                        platoThrowExpression.Expression.Rewrite(func)));
                case PlatoTuple platoTuple:
                    return func(new PlatoTuple(
                        platoTuple.Id, 
                        platoTuple.Type,
                        platoTuple.Expressions.Select(x => x.Rewrite(func))));
                case PlatoTypeExpr platoType:
                    return func(platoType);
                case null:
                    return null;
                default:
                    throw new ArgumentOutOfRangeException(nameof(self));
            }
        }

        public static PlatoArgList Rewrite(this PlatoArgList self, Func<PlatoStatement, PlatoStatement> func)
            => new PlatoArgList(self.Id, self.Args.Select(x => x.Rewrite(func) as PlatoArg));

        public static PlatoStatement Rewrite(this PlatoStatement self, Func<PlatoStatement, PlatoStatement> func)
        {
            switch (self)
            {
                case PlatoCompoundStatement platoCompoundStatement:
                    return func(new PlatoCompoundStatement(
                        platoCompoundStatement.Id,
                        platoCompoundStatement.Statements.Select(x => x.Rewrite(func))));
                case PlatoBlockStatement platoBlockStatement:
                    return func(new PlatoBlockStatement(
                        platoBlockStatement.Id,
                        platoBlockStatement.Statements.Select(x => x.Rewrite(func))));
                case PlatoForEachStatement platoForEachStatement:
                    return func(new PlatoForEachStatement(
                        platoForEachStatement.Id,
                        platoForEachStatement.VarDecl.Rewrite(func) as PlatoVarDeclStatement,
                        platoForEachStatement.Collection.Rewrite(func),
                        platoForEachStatement.Body.Rewrite(func)));
                case PlatoIfStatement platoIfStatement:
                    return func(new PlatoIfStatement(
                        platoIfStatement.Id,
                        platoIfStatement.Condition.Rewrite(func),
                        platoIfStatement.IfStatement.Rewrite(func),
                        platoIfStatement.ElseStatement.Rewrite(func)));
                case PlatoWhileStatement platoWhileStatement:
                    return func(new PlatoWhileStatement(
                        platoWhileStatement.Id,
                        platoWhileStatement.Condition.Rewrite(func),
                        platoWhileStatement.Body.Rewrite(func)));
                case PlatoExpressionStatement platoExpressionStatement:
                    return func(new PlatoExpressionStatement(
                        platoExpressionStatement.Id,
                        platoExpressionStatement.Expression.Rewrite(func)));
                case PlatoReturnStatement platoReturnStatement:
                    return func(new PlatoReturnStatement(
                        platoReturnStatement.Id,
                        platoReturnStatement.Expression.Rewrite(func)
                        ));
                case PlatoVarDeclStatement platoVarDeclStatement:
                    return func(new PlatoVarDeclStatement(
                        platoVarDeclStatement.Id,
                        platoVarDeclStatement.Type,
                        platoVarDeclStatement.Name,
                        platoVarDeclStatement.Value.Rewrite(func),
                        platoVarDeclStatement.Args.Rewrite(func)));
                case PlatoBreakStatement platoBreakStatement:
                    return func(platoBreakStatement);
                case PlatoContinueStatement platoContinueStatement:
                    return func(platoContinueStatement);
                case PlatoEmptyStatement platoEmptyStatement:
                    return func(platoEmptyStatement);
                default:
                    return self;
            }
        }

        public static PlatoStatement Rewrite(this PlatoStatement self, Func<PlatoExpression, PlatoExpression> func)
        {
            switch (self)
            {
                case PlatoForEachStatement platoForEachStatement:
                    return new PlatoForEachStatement(
                        platoForEachStatement.Id,
                        platoForEachStatement.VarDecl.Rewrite(func) as PlatoVarDeclStatement,
                        platoForEachStatement.Collection.Rewrite(func),
                        platoForEachStatement.Body.Rewrite(func));
                case PlatoIfStatement platoIfStatement:
                    return new PlatoIfStatement(
                        platoIfStatement.Id,
                        platoIfStatement.Condition.Rewrite(func),
                        platoIfStatement.IfStatement.Rewrite(func),
                        platoIfStatement.ElseStatement.Rewrite(func));
                case PlatoWhileStatement platoWhileStatement:
                    return new PlatoWhileStatement(
                        platoWhileStatement.Id,
                        platoWhileStatement.Condition.Rewrite(func),
                        platoWhileStatement.Body.Rewrite(func));
                case PlatoExpressionStatement platoExpressionStatement:
                    return new PlatoExpressionStatement(
                        platoExpressionStatement.Id,
                        platoExpressionStatement.Expression.Rewrite(func));
                case PlatoReturnStatement platoReturnStatement:
                    return new PlatoReturnStatement(
                        platoReturnStatement.Id,
                        platoReturnStatement.Expression.Rewrite(func));
                case PlatoVarDeclStatement platoVarDeclStatement:
                    return new PlatoVarDeclStatement(
                        platoVarDeclStatement.Id,
                        platoVarDeclStatement.Type,
                        platoVarDeclStatement.Name,
                        platoVarDeclStatement.Value.Rewrite(func),
                        platoVarDeclStatement.Args.Rewrite(func));
                case PlatoCompoundStatement platoCompoundStatement:
                    return new PlatoCompoundStatement(
                        platoCompoundStatement.Id,
                        platoCompoundStatement.Statements.Select(x => x.Rewrite(func)));
                case PlatoBlockStatement platoBlockStatement:
                    return new PlatoBlockStatement(
                        platoBlockStatement.Id,
                        platoBlockStatement.Statements.Select(x => x.Rewrite(func)));
                case PlatoBreakStatement platoBreakStatement:
                case PlatoContinueStatement platoContinueStatement:
                case PlatoEmptyStatement platoEmptyStatement:
                default:
                    return self;
            }
        }

        public static IEnumerable<PlatoExpression> GetAllExpressions(this PlatoExpression expr)
           => expr?.ChildExpressions.SelectMany(GetAllExpressions).Append(expr) ?? Enumerable.Empty<PlatoExpression>();

        public static IEnumerable<PlatoExpression> GetAllExpressions(this PlatoStatement st)
            => st.ChildExpressions.SelectMany(GetAllExpressions);

        public static PlatoStatement NormalizeExpressions(this PlatoStatement self)
        {
            var vars = self.ChildExpressions.Where(x => x.Id > 0).ToDictionary(x => x.Id, x => new PlatoVarDeclStatement(
                x.Id, x.Type, $"_gen_{x.Id}", x));

            var newStatement = self.Rewrite(x => vars.TryGetValue(x.Id, out var expr)
                ? new PlatoIdentifierRef(x.Id, expr)
                : x);

            return new PlatoCompoundStatement(0, vars.Values.Append(newStatement));
        }

        public static IEnumerable<PlatoLambda> GetLambdas(this PlatoSemanticMapping mapping)
            => mapping.PlatoSyntaxNodes.OfType<PlatoLambda>();

        public static IEnumerable<PlatoStatement> GetStatements(this PlatoSemanticMapping mapping)
            => mapping.PlatoSyntaxNodes.OfType<PlatoStatement>();

        public static IEnumerable<PlatoExpression> GetExpressions(this PlatoSemanticMapping mapping)
            => mapping.PlatoSyntaxNodes.OfType<PlatoExpression>();

        public static IEnumerable<PlatoFunction> GetFunctions(this PlatoSemanticMapping mapping)
            => mapping.PlatoSyntaxNodes.OfType<PlatoFunction>();

        public static IEnumerable<PlatoClass> GetClasses(this PlatoSemanticMapping mapping)
            => mapping.PlatoSyntaxNodes.OfType<PlatoClass>();

        public static string ToFormattedString(this PlatoSyntaxNode node)
            => new SyntaxWriter().Add(node).ToString();

        public static SyntaxElement GetSyntaxElement(this PlatoSemanticMapping mapping, PlatoSyntaxNode node)
        {
            var syntaxNode = mapping.GetCSharpSyntaxNode(node);
            if (syntaxNode == null)
                return null;
            if (mapping.Models.ContainsKey(syntaxNode))
            {
                var model = mapping.Models[syntaxNode];
                return new SyntaxElement(syntaxNode, model);
            }

            return null;
        }

        public static SyntaxNode GetCSharpSyntaxNode(this PlatoSemanticMapping mapping, PlatoSyntaxNode node)
        {
            if (!mapping.IdsToSyntaxNode.TryGetValue(node.Id, out var syntaxNode))
                return null;
            return syntaxNode;
        }

        public static ISymbol GetSymbol(this PlatoSemanticMapping mapping, PlatoSyntaxNode node)
            => mapping.GetSyntaxElement(node).Symbol.Symbol;

        public static PlatoFunction GetFunction(this PlatoSemanticMapping mapping, PlatoInvoke invoke)
        {
            var symbol = mapping.GetSymbol(invoke) as IMethodSymbol;
            var syntaxRef = symbol.DeclaringSyntaxReferences.FirstOrDefault();
            var syntax = syntaxRef?.GetSyntax();
            if (syntax == null)
                return null;
            if (!mapping.SyntaxNodesToIds.TryGetValue(syntax, out var id))
                return null;
            if (!mapping.Children.TryGetValue(id, out var r))
                return null;
            return r as PlatoFunction;
        }

        public static PlatoStatement InlineFunctions(this PlatoSemanticMapping mapping, PlatoInvoke invoke)
        {
            var f = mapping.GetFunction(invoke);
            if (f == null)
            {
                return new PlatoEmptyStatement(mapping.NextId);
                // throw new Exception($"Could not find function {invoke.Function}");
            }

            var newStatements = new List<PlatoStatement>();
            var args = invoke.Args.Args.Cast<PlatoExpression>().ToList();
            
            if (invoke.Reciever != null)
                args = args.Prepend(invoke.Reciever).ToList();

            if (f.Parameters.Parameters.Count != args.Count)
                    throw new Exception(
                        $"Number of arguments {invoke.Args} does not match number of parameters {f.Parameters}");

            for (var i = 0; i < f.Parameters.Parameters.Count; ++i)
            {
                var parameter = f.Parameters.Parameters[i];
                var arg = args[i];
                newStatements.Add(new PlatoVarDeclStatement(mapping.NextId, parameter.Type, parameter.Name, arg));
            }

            newStatements.Add(f.Body);

            // NOTE: the "return statement" have to replaced. And there is a problem if we are in a loop or if/statemnet, etc. 
            // then we may need a "goto", which makes the whole thing harder to rewrite.
            // Really, I want to normalize all functions into expressions. 

            // One way is to make it into a lambda, which we invoke immediately. 
            // HOWEVER, i think that just punts the idea.

            return new PlatoBlockStatement(mapping.NextId, newStatements);
        }

        public static PlatoStatement InlineFunctions(this PlatoSemanticMapping mapping, PlatoStatement st)
        {
            if (st is PlatoVarDeclStatement varDecl)
            {
                if (varDecl.Value is PlatoInvoke invoke)
                {
                    var inline = mapping.InlineFunctions(invoke);
                    return new PlatoCompoundStatement(mapping.NextId, new[] {inline, varDecl});
                }
            }

            return st;
        }
    }
}
