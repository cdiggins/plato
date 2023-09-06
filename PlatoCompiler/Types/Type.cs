using System;
using Plato.Compiler.Symbols;
using System.Collections.Generic;
using System.Linq;
using Plato.Compiler.Utilities;

namespace Plato.Compiler.Types
{
    public abstract class Type
    {
        public string Name { get; }

        protected Type(string name)
            => Name = name;
    }

    public class TypeDefinition : Type
    {
        public TypeDefinitionSymbol Symbol { get; }
        
        public IReadOnlyList<TypeDefinition> TypeParameters { get; }

        public int Id => Symbol.Id;

        public TypeDefinition(TypeDefinitionSymbol symbol, IReadOnlyList<TypeDefinition> typeParameters)
            : base(symbol.Name)
        {
            Symbol = symbol;
            TypeParameters = typeParameters;
        }

        public override string ToString()
        {
            var tps = string.Join(", ", TypeParameters);
            return $"{Name}_{Id}<{tps}>";
        }

        public override bool Equals(object obj)
            => obj is TypeDefinition other
               && Name == other.Name
               && Id == other.Id
               && TypeParameters.SequenceEqual(other.TypeParameters);

        public override int GetHashCode()
            => Hasher.Combine(Name.GetHashCode(), Id, Hasher.Hash(TypeParameters));
    }

    public class TypeReference : Type
    {
        public TypeDefinition Definition { get; }
        public IReadOnlyList<Type> TypeArguments { get; }

        public TypeReference(TypeDefinition def, IReadOnlyList<Type> args)
            : base(def.Name)
        {
            Definition = def;
            TypeArguments = args;
            if (Definition.TypeParameters.Count != TypeArguments.Count)
                throw new Exception($"The reference to {Definition}" +
                                    $" does not have the expected number of parameters {Definition.TypeParameters.Count}" +
                                    $" instead it has {TypeArguments.Count}");
        }

        public override string ToString()
        {
            var tps = string.Join(", ", TypeArguments);
            return $"{Name}_{Definition}<{tps}>";
        }

        public override bool Equals(object obj)
            => obj is TypeReference other
               && Definition.Equals(other.Definition)
               && TypeArguments.SequenceEqual(other.TypeArguments);

        public override int GetHashCode()
            => Hasher.Combine(Definition.GetHashCode(), Hasher.Hash(TypeArguments));
    }

    public class TypeVar : Type
    {
        public TypeDefinition Constraint { get; }
        public int Id { get; } = NextId++;
        public static int NextId = 0;

        public TypeVar(TypeDefinition constraint)
            : base($"`")
        {
            Constraint = constraint;
        }

        public override string ToString()
            => $"`{Id}";

        public override int GetHashCode()
            => Id;

        public override bool Equals(object obj)
            => obj is TypeVar other &&
               other.Id.Equals(Id);
    }
}