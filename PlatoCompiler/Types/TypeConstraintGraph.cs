using System.Collections.Generic;
using Plato.Compiler.Symbols;

namespace Plato.Compiler.Types
{
    public class TypeConstraintCollection
    {
        public List<TypeConstraint> Constraints { get; } = new List<TypeConstraint>();
        public List<TypeConstraintOptionSet> OptionSets { get; } = new List<TypeConstraintOptionSet>();
    }

    public class TypeConstraintOption
    {
        public MemberDefinition Member { get; }
        public List<TypeConstraint> Constraints { get; } = new List<TypeConstraint>();

        public TypeConstraintOption(MemberDefinition member, List<TypeConstraint> constraints)
        {
            Member = member;
            Constraints = constraints;
        }
    }

    public class TypeConstraintOptionSet
    {
        public FunctionGroupDefinition FunctionGroupDefinition { get; }
        public List<TypeConstraintOption> Options { get; }

        public TypeConstraintOptionSet(FunctionGroupDefinition mgs, List<TypeConstraintOption> options)
        {
            FunctionGroupDefinition = mgs;
            Options = options;
        }
    }
}