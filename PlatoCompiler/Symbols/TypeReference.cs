using Ara3D.Geometry.AST;

namespace Ara3D.Geometry.Compiler.Symbols
{
    /// <summary>One occurrence of a type name in source, paired with what the binder resolved it
    /// to. Recorded by <see cref="SymbolFactory.ResolveType"/> so tooling can find every signature
    /// site of a type (field, parameter, return, inherits/implements, type argument) without
    /// re-implementing the binder's two-level type-parameter scoping.</summary>
    public class TypeReference
    {
        public AstTypeNode Node { get; }
        public TypeExpression Type { get; }

        public TypeReference(AstTypeNode node, TypeExpression type)
        {
            Node = node;
            Type = type;
        }

        public override string ToString() => $"{Type} @ {Node?.GetRange()}";
    }
}
