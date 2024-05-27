using System;
using System.Collections.Generic;
using System.Linq;
using Plato.AST;
using Plato.Compiler.Utilities;

namespace Plato.Compiler.Symbols
{
    public class TypeExpression : Symbol
    {
        public override string Name => Definition?.Name ?? "_untyped_";
        public TypeDefinition Definition { get; }
        public IReadOnlyList<TypeExpression> TypeArgs { get; }
            
        public TypeExpression(TypeDefinition def, params TypeExpression[] args)
        {
            if (def == null)
                throw new ArgumentNullException(nameof(def));
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

        public static TypeExpression CreateTypeVar(string name)
            => name.StartsWith("$")
                ? new TypeExpression(new TypeDefinition(TypeKind.TypeVariable, name))
                : throw new Exception("Type variable names must start with $ character");

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