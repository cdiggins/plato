using Plato.Compiler.Symbols;

namespace Plato.Compiler.Types
{
    public static class TypeExtensions
    {
        public static bool IsConcept(this Type type)
            => type.Definition.IsConcept();

        public static bool IsConcreteType(this Type type)
            => type.Definition.IsConcreteType();

        public static bool IsTypeVar(this Type type)
            => type is TypeVariable;

        public static bool InheritsFrom(this Type from, Type to)
            => from.Definition.InheritsFrom(to.Definition);

        public static bool Implements(this Type self, Type other)
            => self.Definition.Implements(other.Definition);

        public static bool IsSubType(this Type self, Type other)
            => self.Definition.IsSubType(other.Definition);

        public static bool CanCastTo(this Type from, Type to, bool allowConversions = true)
            => from.IsTypeVar() || to.IsTypeVar() || from.Definition.CanCastTo(to.Definition, allowConversions);
    }
}