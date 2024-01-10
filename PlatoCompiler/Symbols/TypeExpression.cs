using System;
using System.Collections.Generic;
using System.Linq;
using Plato.Compiler.Utilities;

namespace Plato.Compiler.Symbols
{
    // TODO: this needs to be finished along with statements

    public class TypeExpression : Symbol
    {
        public string Name => Definition?.Name ?? throw new Exception("Unresolved");
        public TypeDefinition Definition { get; }
        public IReadOnlyList<TypeExpression> TypeArgs { get; }

        public TypeExpression(TypeDefinition def, params TypeExpression[] args)
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

        public static TypeExpression CreateFunction(params TypeExpression[] types)
            => new TypeExpression(PrimitiveTypeDefinitions.Function, types);

        public override IEnumerable<Symbol> GetChildSymbols()
            => TypeArgs;

        public override int GetHashCode()
            => Hasher.Combine(TypeArgs.Select(ta => ta.GetHashCode()).Append(Definition.GetHashCode()));

        public override bool Equals(object obj)
            => obj is TypeExpression tes
                   && tes.Definition.Name == Definition.Name
                   && tes.Definition.Id == Definition.Id
                   && tes.TypeArgs.SequenceEqual(TypeArgs);

        public bool IsSelfType()
            => Name == "Self";

        public bool UsesSelfType()
            => IsSelfType() || TypeArgs.Any(ta => ta.UsesSelfType());
    }
}