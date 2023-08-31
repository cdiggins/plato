namespace Plato.Compiler.Types
{
    public class Constraints
    {
        public Constraints Parent { get; }
        public TypeConstraint Constraint { get; }

        public Constraints(Constraints parent, TypeConstraint constraint)
        {
            Parent = parent;
            Constraint = constraint;
        }
    }
}