using System.Collections.Generic;
using System.Linq;

namespace Plato.Compiler
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
            => typeDefs.SelectMany(t => t.Methods.Select(m => m.Function)).Where(f => f != null);

        public static DefSymbol GetDef(this Symbol symbol)
            => symbol as DefSymbol ?? (symbol as RefSymbol)?.Def;

        public static bool IsExpression(this Symbol symbol)
            => symbol is FunctionSymbol || symbol is RefSymbol || 
                   symbol is FunctionCallSymbol || symbol is ConditionalExpressionSymbol;

        public static bool IsPartiallyTyped(this FunctionSymbol fs)
            => !fs.IsExplicitlyTyped() && 
               (fs.Parameters.Any(p => p.Type != null) || fs.Type != null);

        public static bool IsExplicitlyTyped(this FunctionSymbol fs) 
            => fs.Parameters.All(p => p.Type != null) && fs.Type != null;

        public static IEnumerable<RefSymbol> GetParameterReferences(this ParameterSymbol symbol,
            FunctionSymbol function)
            => symbol.GetReferencesTo(function.Body);

        public static IEnumerable<RefSymbol> GetReferencesTo(this DefSymbol def, Symbol within)
            =>  within.GetDescendantSymbols().OfType<RefSymbol>().Where(rs => rs.Def.Equals(def));

        public static bool HasImplementation(FunctionSymbol fs)
            => fs.Body != null;

        public static bool IsFullyImplementedConcept(TypeDefSymbol ts)
            => ts.IsConcept() && ts.Functions.All(HasImplementation);

        public static bool IsConcept(this TypeDefSymbol ts)
            => ts.Kind == TypeKind.Concept;

        public static bool IsType(this TypeDefSymbol ts)
            => ts.Kind == TypeKind.Type;

        public static bool IsPrimitive(this TypeDefSymbol ts)
            => ts.Kind == TypeKind.Primitive;

        public static bool IsLibrary(this TypeDefSymbol ts)
            => ts.Kind == TypeKind.Library;
    }
}