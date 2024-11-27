using Plato.Compiler.Symbols;
using Plato.Compiler.Utilities;
using System.Collections.Generic;
using System.Linq;
using Ara3D.Utils;

namespace Plato.Compiler.Types
{
    public interface IType
    {
    }

    /// <summary>
    /// Right now a constraint is just a concept. 
    /// </summary>
    public class TypeConstraint
    {
        private IType Concept;

        public TypeConstraint(IType concept)
        {
            Verifier.AssertNotNull(concept, nameof(concept));
            Verifier.Assert(concept.IsConcept(), "TypeVariables can only have concepts as constraints");
            Concept = concept;
        }

        public bool IsSatisfied(IType type)
        {
            return type.Implements(Concept);
        }

        public override string ToString()
            => $"constraint={Concept}";
    }

    public class TypeVariable : IType
    {
        public int Id { get; } = NextId++;
        public static int NextId;
        public TypeConstraint[] Constraints { get; }

        public TypeVariable(params TypeConstraint[] constraints)
        {
            Constraints = constraints;
        }

        public override string ToString()
            => $"${Id}:{Constraints.JoinStringsWithComma()}";

        public override int GetHashCode()
            => Id;

        public override bool Equals(object obj)
            => obj is TypeVariable tv && Id == tv.Id;
    }

    public class TypeConstant : IType
    {
        public TypeDef TypeDef { get; }

        public TypeConstant(TypeDef tds)
        {
            Verifier.Assert(tds.IsConcept() || tds.IsConcrete());
            TypeDef = tds;
        }

        public override string ToString()
            => TypeDef.Name;

        public override int GetHashCode()
            => TypeDef.GetHashCode();

        public override bool Equals(object obj)
            => obj is TypeConstant tc && tc.TypeDef.Equals(TypeDef);
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

    public static class ITypeExtensions
    {
        public static TypeDef GetTypeDefinition(this IType type)
        {
            switch (type)
            {
                case TypeConstant tc:
                    return tc.TypeDef;
                case TypeList tl:
                    return tl.Children.First().GetTypeDefinition();
                default:
                    return null;
            }
        }

        public static bool IsConcept(this IType type)
            => type.GetTypeDefinition()?.IsConcept() ?? false;

        public static bool IsConcrete(this IType type)
            => type.GetTypeDefinition()?.IsConcrete() ?? false;

        public static bool IsTypeVariable(this IType type)
            => type is TypeVariable;

        public static int InheritsDepth(this IType type, IType other)
            => type.GetTypeDefinition()?.InheritsDepth(other.GetTypeDefinition()) ?? -1;

        public static bool Inherits(this IType type, IType other)
            => type.InheritsDepth(other) >= 0;

        public static int ImplementsDepth(this IType type, IType other)
            => type.GetTypeDefinition()?.ImplementsDepth(other.GetTypeDefinition()) ?? -1;

        public static bool Implements(this IType type, IType other)
            => type.ImplementsDepth(other) >= 0;

        public static bool IsNamedType(this IType type, string name)
            => type.GetTypeDefinition()?.Name == name;

        public static bool IsFunctionType(this IType type)
            => type.GetFunctionReturnType() != null;

        public static IType GetFunctionReturnType(this IType type)
            => type is TypeList tl && tl.Children.Count > 0 && tl.Children.First() is TypeConstant tc &&
               tc.IsNamedType("Function")
                ? tl.Children.Last()
                : null;

        public static IEnumerable<IType> GetSelfAndAllDescendants(this IType type)
        {
            yield return type;
            if (type is TypeList typeList)
            {
                foreach (var child in typeList.Children)
                {
                    foreach (var d in child.GetSelfAndAllDescendants())
                        yield return d;
                }
            }
        }

        public static IEnumerable<TypeVariable> GetAllTypeVariables(this IType type)
            => type.GetSelfAndAllDescendants().OfType<TypeVariable>();

        public static bool HasTypeVariable(this IType type)
            => type.GetAllTypeVariables().Any();
    }

    public class TypeSet : IType
    {
        public IReadOnlyList<IType> Options { get; }
        public TypeSet(params IType[] options) => Options = options;
        public override string ToString() => string.Join("|", Options);
    }
}