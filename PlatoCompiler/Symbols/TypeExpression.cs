using System;
using System.Collections.Generic;

namespace Plato.Compiler.Symbols
{
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
    }
}