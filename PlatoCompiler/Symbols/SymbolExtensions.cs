using System.Collections.Generic;
using System.Linq;

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
            => within.GetSymbolTree().OfType<RefSymbol>().Where(rs => rs.Def.Equals(def));

        public static bool HasImplementation(this FunctionDef fs)
            => fs.Body != null;

    }
}