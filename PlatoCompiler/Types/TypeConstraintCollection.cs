using System.Collections.Generic;

namespace Plato.Compiler.Types
{
    public class TypeConstraintCollection
    {
        public List<TypeConstraint> Constraints { get; } = new List<TypeConstraint>();
        public List<TypeConstraintCollection> Options { get; } = new List<TypeConstraintCollection>();

        public TypeConstraintCollection AddOptions()
        {
            var r = new TypeConstraintCollection();
            Options.Add(r);
            return r;
        }

        public TypeConstraintCollection Add(TypeConstraint constraint)
        {
            Constraints.Add(constraint);
            return this;
        }
    }
}