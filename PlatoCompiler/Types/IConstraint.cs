using Plato.Compiler.Utilities;

namespace Plato.Compiler.Types
{
    public interface IConstraint 
    { }

    public class TypeVariableConstraint : IConstraint
    {
        public TypeVariable Variable { get; }

        public TypeVariableConstraint(TypeVariable tv)
            => Variable = tv;
    }

    public class CastTypeVariableConstraint : TypeVariableConstraint
    {
        public CastTypeVariableConstraint(TypeVariable tv, IType target)
            : base(tv)
            => Target = target;

        public IType Target { get; }

        public override string ToString()
            => $"{Variable} implements {Target}";

        public override bool Equals(object obj)
            => obj is CastTypeVariableConstraint ic
               && Target.Equals(ic.Target)
               && Variable.Equals(ic.Variable);

        public override int GetHashCode()
            => Hasher.Combine(Target.GetHashCode(), Variable.GetHashCode());
    }
}