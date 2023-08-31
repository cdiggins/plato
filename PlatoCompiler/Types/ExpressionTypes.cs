using Plato.Compiler.Symbols;

namespace Plato.Compiler.Types
{
    public class ExpressionTypes
    {
        public ExpressionTypes Parent { get; }
        public Expression Expression { get; }
        public Type Type { get; }

        public ExpressionTypes(ExpressionTypes parent, Expression expression, Type type)
        {
            Parent = parent;
            Expression = expression;
            Type = type;
        }
    }
}