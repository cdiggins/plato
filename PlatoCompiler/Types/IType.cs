using Plato.Compiler.Symbols;
using Plato.Compiler.Utilities;
using Ptarmigan.Utils;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Plato.Compiler.Types
{
    public interface IType
    {
    }

    public class TypeVariable : IType
    {
        public int Id { get; } = NextId++;
        public static int NextId;
        public IReadOnlyList<IType> DeclaredConstraints { get; }

        public TypeVariable(IReadOnlyList<IType> declaredConstraints)
        {
            DeclaredConstraints = declaredConstraints;
            Verifier.AssertAll(DeclaredConstraints, i => i.IsConcept(), "TypeVariables can only have concepts as constraints");
        }

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
            Verifier.Assert(tds.IsConcept() || tds.IsPrimitive() || tds.IsConcrete());
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

    public static class ITypeExtensions
    {
        public static TypeDefinition GetTypeDefinition(this IType type)
        {
            switch (type)
            {
                case TypeConstant tc:
                    return tc.TypeDefinition;
                case TypeList tl:
                    return tl.Children.First().GetTypeDefinition();
                default:
                    return null;
            }
        }

        public static bool IsPrimitive(this IType type)
            => type.GetTypeDefinition()?.IsPrimitive() ?? false;

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
    }
}