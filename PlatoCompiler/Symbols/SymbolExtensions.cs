using System.Collections.Generic;
using System.Linq;
using Plato.Compiler.Ast;

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

        public static IEnumerable<Reference> GetReferencesTo(this DefinitionSymbol def, ExpressionSymbol within)
            => within.GetExpressionTree().OfType<Reference>().Where(rs => rs.Definition.Equals(def));

        public static bool HasImplementation(FunctionDefinition fs)
            => fs.Body != null;

        public static bool IsFullyImplementedConcept(TypeDefinitionSymbol ts)
            => ts.IsConcept() && ts.Functions.All(HasImplementation);

        public static bool IsConcept(this TypeDefinitionSymbol ts)
            => ts.Kind == TypeKind.Concept;

        public static bool IsConcreteType(this TypeDefinitionSymbol ts)
            => ts.Kind == TypeKind.ConcreteType;

        public static bool IsPrimitive(this TypeDefinitionSymbol ts)
            => ts.Kind == TypeKind.Primitive;

        public static bool IsLibrary(this TypeDefinitionSymbol ts)
            => ts.Kind == TypeKind.Library;

        public static bool IsTypeVariable(this TypeDefinitionSymbol ts)
            => ts.Kind == TypeKind.TypeVariable;
    }
}