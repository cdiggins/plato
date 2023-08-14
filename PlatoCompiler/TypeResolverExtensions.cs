using System;
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

        public static double CanCastTo(this TypeRefSymbol fromType, TypeRefSymbol toType, bool allowConversions = true)
        {
            return CanCastTo(fromType?.Def, toType?.Def, allowConversions);
        }

        public static double CanCastTo(this TypeDefSymbol fromDef, TypeDefSymbol toDef, bool allowConversions = true)
        {
            if (fromDef == null || toDef == null)
                throw new Exception("Could not find type definitions");

            if (fromDef.Equals(toDef))
                return 1;

            if (fromDef.IsSubType(toDef))
                return 2;

            if (fromDef.Name == "Any")
                return 3;

            if (allowConversions)
            {
                // We look for the implicit operators.
                // TODO: look for functions of the name "ToX" and "FromX" when allowing more than just the default. 

                if (toDef.IsType())
                {
                    // TODO: check that the type of each argument matches 
                    // TODO: add tuple support 
                    //if (fromType.Name == "Tuple") return fromType.TypeArgs.Count == toType.Def.Fields.Count ? 4 : 0;

                    // All types have a constructor that acts as an implicit cast 
                    if (toDef.Fields.Count == 1)
                    {
                        var fieldType = toDef.Fields[0].Type?.Def;
                        return fromDef.CanCastTo(fieldType, false) > 0 ? 4 : 0;
                    }
                }

                if (fromDef.IsType())
                {
                    // TODO: check that the type of each argument matches 
                    // TODO: add tuple support 
                    //if (toType.Name == "Tuple") return toType.TypeArgs.Count == fromType.Def.Fields.Count ? 4 : 0;

                    // All types with one field can implicit cast to that field. 
                    if (fromDef.Fields.Count == 1)
                    {
                        var fieldType = fromDef.Fields[0].Type?.Def;
                        return CanCastTo(fieldType, toDef, false) > 0 ? 5 : 0;
                    }
                }
            }

            return 0;
        }

        public static double MatchesScore(this TypeRefSymbol argType, TypeRefSymbol parameterType)
        {
            if (argType == null) return 1;
            if (parameterType == null) return 1;
            return argType.CanCastTo(parameterType);
        }

        public static IReadOnlyList<FunctionSymbol> FindMatchingFunctions(this IReadOnlyList<FunctionSymbol> funcs,
            IReadOnlyList<TypeRefSymbol> argumentTypes)
        {
            var candidates = funcs.Where(f => f.Parameters.Count == argumentTypes.Count).ToList();

            for (var i = 0; i < argumentTypes.Count; ++i)
            {
                candidates = candidates.Where(c => argumentTypes[i].MatchesScore(c.Parameters[i].Type) > 0).ToList();
            }

            return candidates;
        }

    }
}