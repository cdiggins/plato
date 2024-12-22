using Plato.Compiler.Symbols;

namespace Plato.Compiler.Types
{
    public class TypeReplacement
    {
        public readonly TypeDef Variable;
        public readonly TypeExpression Type;

        public TypeReplacement(TypeDef def, TypeExpression type)
            => (Variable, Type) = (def, type);
    }
}