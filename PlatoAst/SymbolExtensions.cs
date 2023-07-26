using System;
using System.Collections.Generic;
using System.Linq;

namespace PlatoAst
{
    public static class SymbolExtensions
    {
        public static IEnumerable<FunctionSymbol> GetLambdas(this FunctionSymbol symbol)
            => symbol.Body.GetDescendantSymbols().OfType<FunctionSymbol>();

        public static IEnumerable<Symbol> GetDescendantSymbols(this Symbol symbol)
        {
            if (symbol == null)
                yield break;
            yield return symbol;
            if (symbol is Symbol cs)
                foreach (var child in cs.Children.SelectMany(GetDescendantSymbols))
                    yield return child;
        }

        // NOTE: does not include lambdas
        public static IEnumerable<FunctionSymbol> GetAllFunctions(this IEnumerable<TypeDefSymbol> typeDefs)
        {
            return typeDefs.SelectMany(t => t.Methods.Select(m => m.Function)).Where(f => f != null);
        }

        public static DefSymbol GetDef(this Symbol symbol)
        {
            return (symbol as RefSymbol)?.Def;
        }

        public static bool IsExpression(this Symbol symbol)
        {
            return symbol is FunctionSymbol || symbol is RefSymbol || 
                   symbol is FunctionCallSymbol || symbol is ConditionalExpressionSymbol;
        }

        public static bool IsPartiallyTyped(this FunctionSymbol fs)
        {
            if (fs.IsExplicitlyTyped())
                return false;

            return fs.Parameters.Any(p => p.Type != null) || fs.Type != null;
        }

        public static bool IsExplicitlyTyped(this FunctionSymbol fs)
        {
            if (fs.Parameters.All(p => p.Type != null) && fs.Type != null)
                return true;

            return false;
        }

        public static IEnumerable<RefSymbol> GetParameterReferences(this ParameterSymbol symbol,
            FunctionSymbol function)
            => symbol.GetReferencesTo(function.Body);

        public static IEnumerable<RefSymbol> GetReferencesTo(this DefSymbol def, Symbol within)
            =>  within.GetDescendantSymbols().OfType<RefSymbol>().Where(rs => rs.Def.Equals(def));
    }
}