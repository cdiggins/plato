using System.Collections.Generic;
using System.Linq;
using Plato.Compiler.Utilities;

namespace Plato.Compiler.Types
{
    public class TypedFunctionVariation
    {
        public TypedFunction Original { get; }
        public Type NewType { get; }

        public TypedFunctionVariation(TypedFunction original, Type newType)
        {
            Original = original;
            NewType = newType;
        }

        public override bool Equals(object obj)
            => obj is TypedFunctionVariation tfv 
               && Original.Equals(tfv.Original)
               && NewType.Equals(tfv.NewType);

        public override int GetHashCode()
            => Hasher.Combine(Original.GetHashCode(), NewType.GetHashCode());

        public static IEnumerable<TypedFunctionVariation> CreateVariations(TypedFunction original, TypeFactory factory)
        {
            var typeParams = original.Parameters;
            var r = new HashSet<TypedFunctionVariation>()
            {
                new TypedFunctionVariation(original, original.FunctionType)
            };
            
            foreach (var p in typeParams)
            {
                if (p.IsConcept())
                {
                    var subs = factory.GetTypesImplementing(p.Definition()).ToList();
                    foreach (var sub in subs)
                    {
                        var newType = factory.Substitute(original.FunctionType, p, sub);
                        var variation = new TypedFunctionVariation(original, newType);
                        r.Add(variation);
                    }
                }
            }

            return r;
        }
    }
}