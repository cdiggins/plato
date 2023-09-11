using System;
using System.Collections.Generic;
using System.Linq;
using Plato.Compiler.Utilities;

namespace Plato.Compiler.Symbols
{
    public class TypeExpressionSymbol : Symbol
    {
        public string Name => Definition?.Name ?? throw new Exception("Unresolved");
        public TypeDefinitionSymbol Definition { get; }
        public IReadOnlyList<TypeExpressionSymbol> TypeArgs { get; }

        public TypeExpressionSymbol(TypeDefinitionSymbol def, params TypeExpressionSymbol[] args)
        {
            Definition = def;
            TypeArgs = args;
        }

        public override string ToString()
        {
            var kind = Definition?.Kind.ToString() ?? "";
            if (TypeArgs.Count > 0)
                return kind + ":" + Name + $"<{string.Join(",", TypeArgs)}>";
            return kind + ":" + Name;
        }

        public static TypeExpressionSymbol CreateFunction(params TypeExpressionSymbol[] types)
            => new TypeExpressionSymbol(PrimitiveTypeDefinitions.Function, types);

        public override IEnumerable<Symbol> GetChildSymbols()
            => TypeArgs;

        public override int GetHashCode()
            => Hasher.Combine(TypeArgs.Select(ta => ta.GetHashCode()).Append(Definition.GetHashCode()));

        public override bool Equals(object obj)
        {
            return obj is TypeExpressionSymbol tes
                   && tes.Definition.Name == Definition.Name
                   && tes.Definition.Id == Definition.Id
                   && tes.TypeArgs.SequenceEqual(TypeArgs);
        }
    }
}