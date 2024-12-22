using System.Linq;
using Plato.AST;
using Plato.Compiler.Symbols;

namespace Plato.Compiler.Types
{
    public static class TypeExtensions
    {
        public static bool IsFullyImplementedConcept(TypeDef ts)
            => ts.IsConcept() && ts.Functions.All(f => f.HasImplementation());

        public static bool IsConcept(this TypeDef ts)
            => ts.Kind == TypeKind.Concept;

        public static bool IsConcrete(this TypeDef ts)
            => ts.Kind == TypeKind.ConcreteType || ts.Kind == TypeKind.Primitive;

        public static bool IsLibrary(this TypeDef ts)
            => ts.Kind == TypeKind.Library;

        public static bool IsTypeVariable(this TypeDef ts)
            => ts.Kind == TypeKind.TypeVariable;

        public static bool Implements(this TypeDef a, TypeDef b)
            => a.Implements.Any(i => i.Def.Equals(b));

        public static bool Inherits(this TypeDef a, TypeDef b)
            => a.Inherits.Any(i => i.Def.Equals(b));
    }
}