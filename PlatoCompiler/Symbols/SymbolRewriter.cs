using System;
using System.Collections.Generic;
using System.Linq;
using Ara3D.Utils;

namespace Ara3D.Geometry.Compiler.Symbols
{
    public static class SymbolRewriter
    {
        public static int NextId = 0;

        public static Symbol RewriteLambdasCapturingVars(this Symbol body)
        {
            var capturedVars = body.GetSymbolTree()
                .OfType<Lambda>().SelectMany(lambda => lambda.Function.CapturedSymbols).ToList();

            if (capturedVars.Count <= 0)
                return body;

            body = ExpressionBodyToStatementBody(body);

            foreach (var capture in capturedVars)
            {
                if (!(capture is ParameterRefSymbol param))
                    throw new Exception("Only parameter captures supported right now");

                var newVarDef = new VariableDef(null, $"_var{NextId++}", param.Type, param);
                body = new BlockStatement(
                    newVarDef,
                    body.Replace(param, newVarDef.ToReference()));
            }

            return body;
        }

        public static Symbol ToStatement(this Symbol self)
        {
            if (self is Statement st)
                return st;
            if (self is Expression expr)
                return new ExpressionStatement(expr);
            throw new Exception("Expected expression or statement");
        }

        public static FunctionDef ExpressionBodyToStatementBody(this FunctionDef self)
            => new FunctionDef(self.Scope, self.Name, self.OwnerType, self.ReturnType, 
                ExpressionBodyToStatementBody(self.Body),
                self.Parameters.ToArray());

        public static Symbol ExpressionBodyToStatementBody(this Symbol body)
        {
            if (body is Statement)
                return body;
            if (!(body is Expression expr))
                throw new Exception("Expected expression body");
            return new ReturnStatement(expr);
        }

        public static Symbol Replace(this Symbol self, Symbol target, Symbol replace)
            => self.Rewrite(sym => ReferenceEquals(sym, target) 
                ? (replace, false) 
                : (sym, true));

        public static T[] RewriteList<T>(this IEnumerable<T> self, Func<Symbol, (Symbol, bool)> f) where T: Symbol
            => self.Select(s => s.TypedRewrite(f)).WhereNotNull().ToArray();

        public static T TypedRewrite<T>(this T self, Func<Symbol, (Symbol, bool)> f) where T : Symbol
            => self.Rewrite(f) as T;

        public static Symbol Rewrite(this Symbol self, Func<Symbol, (Symbol, bool)> f)
        {
            var r = f(self);
            self = r.Item1;
            if (!r.Item2)
                return self;

            switch (self)
            {
                case ArrayLiteral arrayLiteral:
                    return new ArrayLiteral(arrayLiteral.Expressions.RewriteList(f));

                case FunctionDef functionDef:
                    return new FunctionDef(functionDef.Scope, functionDef.Name, functionDef.OwnerType,
                        functionDef.ReturnType, functionDef.Body.TypedRewrite(f),
                        functionDef.Parameters.RewriteList(f));

                case FieldDef fieldDef:
                case FunctionGroupDef functionGroupDef:
                case MethodDef methodDef:
                case MemberDef memberDef:
                case ParameterDef parameterDef:
                case SelfType selfType:
                case TypeParameterDef typeParameterDef:
                case TypeVariable typeVariable:
                case TypeDef typeDef:
                    return self;

                case VariableDef variableDef:
                    // TEMP: I'm not sure? 
                    return self;    
                    //throw new Exception("I'm not sure how to implement this");

                case DefSymbol defSymbol:
                    throw new Exception("Should be unreachable");

                case Assignment assignment:
                    return new Assignment(assignment.LValue.TypedRewrite(f), assignment.RValue.TypedRewrite(f));

                case BlockStatement blockStatement:
                    return new BlockStatement(blockStatement.Symbols.RewriteList(f));

                case NewExpression newExpression:
                    return new NewExpression(newExpression.Type.TypedRewrite(f), newExpression.Args.RewriteList(f));

                case CommentStatement commentStatement:
                    return self;

                case ConditionalExpression conditionalExpression:
                    return new ConditionalExpression(
                        conditionalExpression.Condition.TypedRewrite(f), 
                        conditionalExpression.IfTrue.TypedRewrite(f),
                        conditionalExpression.IfFalse.TypedRewrite(f));
                
                case FunctionCall functionCall:
                    return new FunctionCall(functionCall.Function.TypedRewrite(f), functionCall.HasArgList, functionCall.Args.RewriteList(f));

                case FunctionGroupRefSymbol functionGroupRefSymbol:
                    return self;

                case Lambda lambda:
                    return new Lambda(lambda.Function.TypedRewrite(f));

                case Literal literal:
                    return self;

                case ParameterRefSymbol parameterRefSymbol:
                    return self;
                
                case Parenthesized parenthesized:
                    return new Parenthesized(parenthesized.Expression.TypedRewrite(f));

                case TypeRefSymbol typeRefSymbol:
                    return self;

                case VariableRefSymbol variableRefSymbol:
                    return self;

                case RefSymbol refSymbol:
                    return self;

                case TypeExpression typeExpression:
                    return self;

                case Expression expression:
                    throw new Exception("Unexpected case statement");

                case LoopStatement loopStatement:
                    return new LoopStatement(loopStatement.Condition.TypedRewrite(f), loopStatement.Body.TypedRewrite(f));

                case MultiStatement multiStatement:
                    return new MultiStatement(multiStatement.Symbols.RewriteList(f));

                case ReturnStatement returnStatement:
                    return new ReturnStatement(returnStatement.Expression.TypedRewrite(f));

                case Statement statement:
                    throw new Exception("Unexpected case statement");

                default:
                    throw new ArgumentOutOfRangeException(nameof(self));
            }
        }
    }
}