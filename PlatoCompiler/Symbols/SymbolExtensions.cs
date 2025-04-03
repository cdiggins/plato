using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Plato.Compiler.Symbols
{
    public static class SymbolExtensions
    {
        public static IEnumerable<Symbol> GetSymbolTree(this Symbol symbol)
        {
            if (symbol == null)
                yield break;

            yield return symbol;
            foreach (var child in symbol.GetChildSymbols())
            foreach (var x in child.GetSymbolTree())
                yield return x;
        }

        public static bool IsPartiallyTyped(this FunctionDef fs)
            => !fs.IsExplicitlyTyped() &&
               (fs.Parameters.Any(p => p.Type != null) || fs.Type != null);

        public static bool IsExplicitlyTyped(this FunctionDef fs)
            => fs.Parameters.All(p => p.Type != null) && fs.Type != null;

        public static IEnumerable<RefSymbol> GetParameterReferences(this ParameterDef def,
            FunctionDef function)
            => def.GetReferencesTo(function.Body);

        public static IEnumerable<RefSymbol> GetReferencesTo(this DefSymbol def, Symbol within)
            => within.GetSymbolTree().OfType<RefSymbol>().Where(rs => rs.Def?.Equals(def) == true);

        public static bool HasImplementation(this FunctionDef fs)
            => fs.Body != null;

        public static IEnumerable<Expression> GetExpressionTree(this Symbol symbol)
        {
            if (symbol is Expression expr)
                return expr.GetExpressionTree();
            if (symbol is Statement st)
                return st.GetExpressionTree();
            return Enumerable.Empty<Expression>();
        }

        public static IEnumerable<Expression> GetExpressionTree(this Statement st)
        {
            switch (st)
            {
                case BlockStatement blockStatement:
                    break;
                case CommentStatement commentStatement:
                    break;
                case ExpressionStatement expressionStatement:
                    return expressionStatement.Expression.GetExpressionTree();
                case LoopStatement loopStatement:
                    return loopStatement.Condition.GetExpressionTree();
                case MultiStatement multiStatement:
                    break;
                case ReturnStatement returnStatement:
                    return returnStatement.Expression.GetExpressionTree();
                default:
                    throw new ArgumentOutOfRangeException(nameof(st));
            }

            return Enumerable.Empty<Expression>();
        }

        public static IEnumerable<Expression> GetExpressionTree(this Expression expr)
        {
            foreach (var child in expr.Children)
                yield return child;
            yield return expr;
        }
    }
}