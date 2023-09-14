using Plato.Compiler.Symbols;
using Plato.Compiler.Utilities;
using Ptarmigan.Utils;
using System.Collections.Generic;
using System.Linq;

namespace Plato.Compiler.Types
{
    public interface IType
    {
    }

    public class TypeVariable : IType
    {
        public int Id { get; } = NextId++;
        public static int NextId;

        public override string ToString()
            => $"${Id}";

        public override int GetHashCode()
            => Id;

        public override bool Equals(object obj)
            => obj is TypeVariable tv && Id == tv.Id;
    }

    public class TypeConstant : IType
    {
        public TypeDefinition TypeDefinition { get; }

        public TypeConstant(TypeDefinition tds)
        {
            Verifier.Assert(tds.IsConcept() || tds.IsPrimitive() || tds.IsConcreteType());
            TypeDefinition = tds;
        }

        public override string ToString()
            => TypeDefinition.Name;

        public override int GetHashCode()
            => TypeDefinition.GetHashCode();

        public override bool Equals(object obj)
            => obj is TypeConstant tc && tc.TypeDefinition.Equals(TypeDefinition);
    }

    public class TypeList : IType
    {
        public TypeList(IReadOnlyList<IType> children)
            => Children = children;

        public IReadOnlyList<IType> Children { get; }

        public override string ToString()
            => $"({string.Join(", ", Children)})";

        public override int GetHashCode()
            => Hasher.Combine(Children.Select(c => c.GetHashCode()));

        public override bool Equals(object obj)
            => obj is TypeList tl && tl.Children.SequenceEqual(Children);
    }
}