using System.Collections.Generic;

namespace Plato.Compiler.Types
{
    public class TypeConstraint
    {
    }

    public class ArgumentConstraint : TypeConstraint
    {
        public Type Type { get; }
        public Type ArgumentType { get; }
    }

    public class InvokedConstraint : TypeConstraint
    {
        public Type Type { get; }
        public IReadOnlyList<Type> Arguments { get; }
        public InvokedConstraint(Type type, IReadOnlyList<Type> arguments)
        {
            Type = type;
            Arguments = arguments;
        }
    }

    public class IsBoolConstraint : TypeConstraint
    {
        public Type Type;

        public IsBoolConstraint(Type t)
            => Type = t;
    }

    public class UnifiesConstraint : TypeConstraint
    {
        public Type Type1;
        public Type Type2;

        public UnifiesConstraint(Type type1, Type type2)
        {
            Type1 = type1;
            Type2 = type2;
        }
    }

    public class CastsToConstraint : TypeConstraint
    {
        public Type From;
        public Type To;

        public CastsToConstraint(Type from, Type to)
        {
            From = from;
            To = to;
        }
    }

    public class HasTypeArgs : TypeConstraint
    {
        public Type Type;
        public IReadOnlyList<Type> Arguments;
    }

    // Creates constraints. Easy-ish 
}