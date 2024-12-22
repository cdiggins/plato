using System;
using System.Collections.Generic;
using System.Linq;
using Plato.Compiler.Symbols;

namespace Plato.Compiler.Types
{
    public class TypeRelations
    {
        public Compilation Compilation { get; }

        public readonly Dictionary<TypeDef, List<TypeRelation>> RelationLookup
            = new Dictionary<TypeDef, List<TypeRelation>>();

        public readonly List<TypeRelation> Relations = new List<TypeRelation>();

        public TypeRelations(Compilation compilation)
        {
            Compilation = compilation;
            foreach (var td in compilation.TypeDefinitions)
            {
                if (td.IsLibrary())
                    continue;

                Relations.AddRange(ComputeRelationsFrom(td));
            }

            Relations.AddRange(ComputeCasts());

            foreach (var r in Relations)
            {
                if (!RelationLookup.ContainsKey(r.Source))
                {
                    RelationLookup.Add(r.Source, new List<TypeRelation>());
                }
                RelationLookup[r.Source].Add(r);
            }
        }

        public IEnumerable<TypeRelation> ComputeCasts()
        {
            foreach (var rf in Compilation.ReifiedFunctions)
            {
                if (rf.Name != rf.ReturnType.Name || rf.NumParameters != 1) 
                    continue;
                var pt = rf.ParameterTypes[0];
                if (!rf.ReturnType.Def.IsConcrete()) 
                    continue;
                if (!pt.Def.IsConcrete())
                    throw new Exception(
                        "Expected a reified function to have a first parameter of concrete type");

                yield return new TypeRelation
                {
                    Source = pt.Def,
                    Dest = rf.ReturnType.Def,
                    Expr = rf.ReturnType,
                    Depth = -1,
                    Cast = rf
                };
            }
        }

        public IEnumerable<TypeRelation> ComputeRelationsFrom(TypeDef typeDef)
        {
            foreach (var i in typeDef.Inherits)
            {
                foreach (var r in GatherRelations(typeDef, i, 1, null))
                    yield return r;
            }

            foreach (var i in typeDef.Implements)
            {
                foreach (var r in GatherRelations(typeDef, i, 1, null))
                    yield return r;
            }
        }

        public IEnumerable<TypeRelation> GatherRelations(TypeDef original, TypeExpression current, int depth, TypeParamLookup tpl)
        {
            yield return new TypeRelation
            {
                Source = original,
                Dest = current.Def,
                Expr = current,
                Depth = depth,
                Cast = null
            };

            tpl = tpl.ReplaceParameters(current);

            foreach (var i in current.Def.Inherits)
            {
                foreach (var r in GatherRelations(original, i, depth + 1, tpl))
                    yield return r;
            }
        }
    }
}