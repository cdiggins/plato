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

        public static bool IsPartiallyTyped(this FunctionDefinition fs)
            => !fs.IsExplicitlyTyped() &&
               (fs.Parameters.Any(p => p.Type != null) || fs.Type != null);

        public static bool IsExplicitlyTyped(this FunctionDefinition fs)
            => fs.Parameters.All(p => p.Type != null) && fs.Type != null;

        public static IEnumerable<Reference> GetParameterReferences(this ParameterDefinition definition,
            FunctionDefinition function)
            => definition.GetReferencesTo(function.Body);

        public static IEnumerable<Reference> GetReferencesTo(this DefinitionSymbol def, Expression within)
            => within.GetExpressionTree().OfType<Reference>().Where(rs => rs.Definition.Equals(def));

        public static bool HasImplementation(this FunctionDefinition fs)
            => fs.Body != null;

    }
}