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
    }
}