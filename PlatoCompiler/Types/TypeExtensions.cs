using Plato.Compiler.Symbols;

namespace Plato.Compiler.Types
{
    public static class TypeExtensions
    {
        public static TypeDefinition Definition(this Type type)
            => type is TypeDefinition td ? td : type is TypeReference tr ? tr.Definition : null;

        public static bool IsConcept(this Type type)
            => type.Definition()?.Symbol?.IsConcept() == true;

        public static bool IsConcreteType(this Type type)
            => type.Definition()?.Symbol?.IsConcreteType() == true;

        public static bool IsTypeVar(this Type type)
            => type is TypeVar;

        public static bool InheritsFrom(this Type from, Type to)
            => from.Definition()?.Symbol?.InheritsFrom(to.Definition()?.Symbol) == true;

        public static bool Implements(this Type self, Type other)
            => self.Definition()?.Symbol?.Implements(other.Definition()?.Symbol) == true;

        public static bool IsSubType(this Type self, Type other)
            => self.Definition()?.Symbol?.IsSubType(other.Definition()?.Symbol) == true;

        public static bool CanCastTo(this Type from, Type to, bool allowConversions = true)
            => from.IsTypeVar() || to.IsTypeVar() || from.Definition()?.Symbol.CanCastTo(to.Definition()?.Symbol, allowConversions) == true;
    }
}