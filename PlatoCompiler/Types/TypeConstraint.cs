using System.Collections.Generic;

namespace Plato.Compiler.Types
{
    public class TypeConstraint
    {
    }

    public class ArgumentConstraint : TypeConstraint
    {
        public Type ParameterType { get; }
        public Type ArgumentType { get; }

        public ArgumentConstraint(Type argumentType, Type parameterType)
        {
            ParameterType = parameterType;
            ArgumentType = argumentType;
        }

        public override string ToString()
            => $"ArgumentConstraint(arg={ArgumentType}, param={ParameterType})";
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

        public override string ToString()
            => $"InvokedConstraint(type={Type}, args=({string.Join(",", Arguments)}))";
    }

    public class IsBoolConstraint : TypeConstraint
    {
        public Type Type;

        public IsBoolConstraint(Type t)
            => Type = t;

        public override string ToString()
            => $"IsBoolConstraint(type={Type})";
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

        public override string ToString()
            => $"UnifiesConstraint({Type1}, {Type2})";
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

        public override string ToString()
            => $"CastsTo(from={From}, to={To})";
    }
}