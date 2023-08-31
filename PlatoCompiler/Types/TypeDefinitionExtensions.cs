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
            if (a.IsType() && b.IsConcept())
                return a;

            if (a.IsConcept() && a.IsType())
                return a;

            if (a.IsType() && b.IsType())
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

        public static IReadOnlyList<FunctionDefinition> FindMatchingFunctions(IReadOnlyList<FunctionDefinition> funcs,
            IReadOnlyList<TypeExpression> argumentTypes)
        {
            var candidates = funcs.Where(f => f.Parameters.Count == argumentTypes.Count).ToList();

            for (var i = 0; i < argumentTypes.Count; ++i)
            {
                candidates = candidates.Where(c => MatchesScore(argumentTypes[i], c.Parameters[i].Type) > 0).ToList();
            }

            return candidates;
        }

        public static double MatchesScore(this TypeExpression argType, TypeExpression parameterType)
        {
            if (argType == null) return 1;
            if (parameterType == null) return 1;
            return CanCastTo(argType, parameterType);
        }

        public static double CanCastTo(this TypeExpression fromType, TypeExpression toType, bool allowConversions = true)
        {
            return CanCastTo(fromType?.Definition, toType?.Definition, allowConversions);
        }

        public static double CanCastTo(this TypeDefinition from, TypeDefinition to, bool allowConversions = true)
        {
            if (from == null || to == null)
                throw new Exception("Could not find type definitions");

            if (from.Equals(to))
                return 1;

            if (from.IsSubType(to))
                return 2;

            if (from.Name == "Any")
                return 3;

            if (allowConversions)
            {
                // We look for the implicit operators.
                // TODO: look for functions of the name "ToX" and "FromX" when allowing more than just the default. 

                if (to.IsType())
                {
                    // TODO: check that the type of each argument matches 
                    // TODO: add tuple support 
                    //if (fromType.Name == "Tuple") return fromType.TypeArgs.Count == toType.Def.Fields.Count ? 4 : 0;

                    // All types have a constructor that acts as an implicit cast 
                    if (to.Fields.Count == 1)
                    {
                        var fieldType = to.Fields[0].Type?.Definition;
                        return CanCastTo(from, fieldType, false) > 0 ? 4 : 0;
                    }
                }

                if (from.IsType())
                {
                    // TODO: check that the type of each argument matches 
                    // TODO: add tuple support 
                    //if (toType.Name == "Tuple") return toType.TypeArgs.Count == fromType.Def.Fields.Count ? 4 : 0;

                    // All types with one field can implicit cast to that field. 
                    if (from.Fields.Count == 1)
                    {
                        var fieldType = from.Fields[0].Type?.Definition;
                        return CanCastTo(fieldType, to, false) > 0 ? 5 : 0;
                    }
                }
            }

            return 0;
        }

    }
}