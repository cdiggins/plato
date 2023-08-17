using System.Collections.Generic;
using System.Linq;

namespace Plato.Compiler
{
    public static class TypeResolverExtensions
    {
        public static bool InheritsFrom(this TypeDefSymbol self, TypeDefSymbol other)
            => self.IsConcept() && other.IsConcept() && self.GetSelfAndAllInheritedTypes().Contains(other);

        public static bool Implements(this TypeDefSymbol self, TypeDefSymbol other)
            => other.IsConcept() && self.GetAllImplementedConcepts().Select(c => c.Def).Contains(other);

        public static bool IsSubType(this TypeDefSymbol self, TypeDefSymbol other)
        {
            if (self.IsLibrary() || other.IsLibrary())
                return false;
            if (self.Equals(other))
                return true;
            if (self.InheritsFrom(other))
                return true;
            if (self.Implements(other))
                return true;
            return false;
        }

        public static bool IsSubType(this TypeDefSymbol self, IEnumerable<TypeDefSymbol> others)
            => others.All(x => IsSubType(self, x));

        public static TypeDefSymbol Unify(this IEnumerable<TypeDefSymbol> conceptsA, IEnumerable<TypeDefSymbol> conceptsB)
        {
            // Which concepts in A supercede all of those in B? 
            var superTypesA = conceptsA.Where(c => IsSubType(c, conceptsB)).ToList();

            // Which concepts in B supercede all of those in B
            var superTypesB = conceptsB.Where(c => IsSubType(c, conceptsA)).ToList();

            if (superTypesA.Count > 0)
                return superTypesA[0];

            if (superTypesB.Count > 0)
                return superTypesB[0];

            return PrimitiveTypes.Error;
        }

        public static TypeDefSymbol Unify(this TypeDefSymbol a, TypeDefSymbol b)
        {
            // If one type inher

            if (a == null)
                return b;

            if (b == null)
                return a;

            if (a.Equals(b))
                return a;

            // If one type is a concept, and the other is a regular type, then choose the type. 
            if (a.IsType() && b.IsConcept())
                return a;

            if (a.IsConcept() && a.IsType())
                return a;

            if (a.IsType() && b.IsType())
            {
                var aConcepts = a.Implements.Select(i => i.Def);
                var bConcepts = b.Implements.Select(i => i.Def);
                return Unify(aConcepts, bConcepts);
            }

            if (a.IsConcept() && b.IsConcept())
            {
                var aConcepts = a.GetSelfAndAllInheritedTypes();
                var bConcepts = b.GetSelfAndAllInheritedTypes();
                return Unify(aConcepts, bConcepts);
            }

            return b;
        }

    }
}