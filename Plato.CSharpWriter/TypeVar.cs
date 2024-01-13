using Plato.Compiler.Symbols;

namespace Plato.CSharpWriter
{
    public class TypeVar
    {
        public string Name => $"T{Index}";
        public TypeParameterDefinition Definition { get; }
        public bool HasDefinition => Definition != null;
        public TypeExpression Constraint { get; }
        public int Index { get; }

        public TypeVar(int index, TypeParameterDefinition def, TypeExpression constraint)
        {
            Definition = def;
            Index = index;
            Constraint = constraint;
        }
    }
}