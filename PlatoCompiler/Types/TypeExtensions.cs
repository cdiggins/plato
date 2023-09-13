using System;
using System.Collections.Generic;
using System.Linq;
using Plato.Compiler.Symbols;

namespace Plato.Compiler.Types
{
    public static class TypeDefinitionExtensions
    {
        public static bool InheritsFrom(this TypeDefinition self, TypeDefinition other)
            => self.IsConcept() && other.IsConcept() && self.GetSelfAndAllInheritedTypes().Contains(other);

        public static bool Implements(this TypeDefinition self, TypeDefinition other)
            => other.IsConcept() && self.GetAllImplementedConcepts().Select(c => c.Definition).Contains(other);

        public static bool IsSubType(this TypeDefinition self, TypeDefinition other)
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

        public static bool IsSubType(this TypeDefinition self, IEnumerable<TypeDefinition> others)
            => others.All(x => self.IsSubType(x));

        public static TypeDefinition Unify(this IEnumerable<TypeDefinition> conceptsA, IEnumerable<TypeDefinition> conceptsB)
        {
            // Which concepts in A supercede all of those in B? 
            var superTypesA = conceptsA.Where(c => c.IsSubType(conceptsB)).ToList();

            // Which concepts in B supercede all of those in B
            var superTypesB = conceptsB.Where(c => c.IsSubType(conceptsA)).ToList();

            if (superTypesA.Count > 0)
                return superTypesA[0];

            if (superTypesB.Count > 0)
                return superTypesB[0];

            return PrimitiveTypeDefinitions.Error;
        }

        public static TypeDefinition Unify(this TypeDefinition a, TypeDefinition b)
        {
            // If one type inher

            if (a == null)
                return b;

            if (b == null)
                return a;

            if (a.Equals(b))
                return a;

            // If one type is a concept, and the other is a regular type, then choose the type. 
            if (a.IsConcreteType() && b.IsConcept())
                return a;

            if (a.IsConcept() && a.IsConcreteType())
                return a;

            if (a.IsConcreteType() && b.IsConcreteType())
            {
                var aConcepts = a.Implements.Select(i => i.Definition);
                var bConcepts = b.Implements.Select(i => i.Definition);
                return aConcepts.Unify(bConcepts);
            }

            if (a.IsConcept() && b.IsConcept())
            {
                var aConcepts = a.GetSelfAndAllInheritedTypes();
                var bConcepts = b.GetSelfAndAllInheritedTypes();
                return aConcepts.Unify(bConcepts);
            }

            return b;
        }
    }
}