using Plato.Compiler.Symbols;

namespace Plato.CSharpWriter
{
    public class TypeVar
    {
        public string Name => $"T{Index}";
        public TypeParameterDef Def { get; }
        public bool HasDefinition => Def != null;
        public TypeExpression Constraint { get; }
        public int Index { get; }

        public TypeVar(int index, TypeParameterDef def, TypeExpression constraint)
        {
            Def = def;
            Index = index;
            Constraint = constraint;
        }
    }
}