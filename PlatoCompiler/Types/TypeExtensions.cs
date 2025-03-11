using System.Linq;
using Plato.AST;
using Plato.Compiler.Symbols;

namespace Plato.Compiler.Types
{
    public static class TypeExtensions
    {
        public static bool IsFullyImplementedConcept(TypeDef ts)
            => ts.IsInterface() && ts.Functions.All(f => f.HasImplementation());

        public static bool IsInterface(this TypeDef ts)
            => ts.Kind == TypeKind.Concept;

        public static bool IsConcrete(this TypeDef ts)
            => ts.Kind == TypeKind.ConcreteType || ts.Kind == TypeKind.Primitive;

        public static bool IsLibrary(this TypeDef ts)
            => ts.Kind == TypeKind.Library;

        public static bool IsTypeVariable(this TypeDef ts)
            => ts.Kind == TypeKind.TypeVariable;

        public static bool IsImplementing(this TypeExpression a, TypeExpression b)
        {
            if (a.Name != b.Name)
                return false;
            if (a.TypeArgs.Count != b.TypeArgs.Count)
                return false;
            for (var i = 0; i < a.TypeArgs.Count; i++)
                if (!a.TypeArgs[i].IsImplementing(b.TypeArgs[i]))
                    return false;
            if (a.Name == b.Name)
                return true;
            foreach (var tmp in a.Def.GetAllImplementedConcepts())
                if (tmp.IsImplementing(b))
                    return true;
            return true;
        }

        public static bool Implements(this TypeDef a, TypeExpression b)
            => a.GetAllImplementedConcepts().Any(i => i.IsImplementing(b));

        public static bool Inherits(this TypeDef a, TypeDef b)
            => a.Inherits.Any(i => i.Def.Equals(b));
    }
}