// Records a source type and the type that replaces it during substitution.
using Ara3D.Geometry.Compiler.Symbols;

namespace Ara3D.Geometry.Compiler.Types
{
    public class TypeReplacement
    {
        public readonly TypeDef Variable;
        public readonly TypeExpression Type;

        public TypeReplacement(TypeDef def, TypeExpression type)
            => (Variable, Type) = (def, type);
    }
}