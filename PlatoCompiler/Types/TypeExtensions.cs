using System.Collections.Generic;
using System.Linq;
using Ara3D.Utils;
using Plato.Compiler.Ast;
using Plato.Compiler.Symbols;

namespace Plato.Compiler.Types
{
    public static class TypeDefinitionExtensions
    {
        public static bool IsFullyImplementedConcept(TypeDefinition ts)
            => ts.IsConcept() && ts.Functions.All(f => f.HasImplementation());

        public static bool IsConcept(this TypeDefinition ts)
            => ts.Kind == TypeKind.Concept;

        public static bool IsConcrete(this TypeDefinition ts)
            => ts.Kind == TypeKind.ConcreteType;

        public static bool IsPrimitive(this TypeDefinition ts)
            => ts.Kind == TypeKind.Primitive;

        public static bool IsLibrary(this TypeDefinition ts)
            => ts.Kind == TypeKind.Library;

        public static bool IsTypeVariable(this TypeDefinition ts)
            => ts.Kind == TypeKind.TypeVariable;

        public static int InheritsDepth(this TypeDefinition self, TypeDefinition other)
        {
            if (self.Equals(other))
                return 0;
            var r = self.Inherits
                .MinWhere(x => x.Definition?.InheritsDepth(other) ?? -1,
                x => x >= 0, -1);
            if (r >= 0)
                return r + 1;
            return -1;
        }

        public static int ImplementsDepth(this TypeDefinition self, TypeDefinition other)
        {
            return self.Implements
                .MinWhere(x => x.Definition?.InheritsDepth(other) ?? -1,
                    x => x >= 0, -1);
        }

        public static bool InheritsFrom(this TypeDefinition self, TypeDefinition other)
            => self.InheritsDepth(other) >= 0;

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
            if (a.IsConcrete() && b.IsConcept())
                return a;

            if (a.IsConcept() && a.IsConcrete())
                return a;

            if (a.IsConcrete() && b.IsConcrete())
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