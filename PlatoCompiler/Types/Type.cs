using Plato.Compiler.Symbols;
using System.Collections.Generic;
using System.Linq;
using Plato.Compiler.Utilities;

namespace Plato.Compiler.Types
{
    public abstract class Type
    {
        public string Name { get; }
        public TypeDefinitionSymbol Definition { get; }

        // ReSharper disable once UnusedParameter.Local
        protected Type(string name, TypeDefinitionSymbol definition, TypeFactory _)
        {
            Name = name;
            Definition = definition;
        }

        public override string ToString()
            => $"{Name}";

        public override bool Equals(object obj)
            => obj != null
               && GetType() == obj.GetType()
               && obj is Type other
               && Name == other.Name;

        public override int GetHashCode()
            => Hasher.Hash(Name);
    }

    public class TypeVariable : Type
    {
        public int Id { get; } = Symbol.NextId++;

        public TypeVariable(string name, TypeDefinitionSymbol definition, TypeFactory factory)
            : base(name, definition, factory)
        { }

        public override string ToString()
            => $"`{Name}_{Id}";

        public override bool Equals(object obj)
            => base.Equals(obj)
               && obj is TypeVariable other
               && Id == other.Id;

        public override int GetHashCode()
            => Hasher.Hash(base.GetHashCode(), Id);
    }

    public class AnyType : TypeVariable
    {
        public AnyType(TypeFactory factory)
            : base("Any", null, factory)
        { }
    }

    public class ConstrainedVariable : TypeVariable
    {
        public Type Constraint { get; }

        public ConstrainedVariable(Type constraint, TypeDefinitionSymbol definition, TypeFactory factory)
            : base(constraint.Name, definition, factory)
        {
            Constraint = constraint;
        }

        public override bool Equals(object obj)
            => base.Equals(obj)
               && obj is ConstrainedVariable other
               && Definition.Id == other.Definition.Id
               && Equals(Constraint, other.Constraint);

        public override int GetHashCode()
            => Hasher.Combine(base.GetHashCode(), Constraint.GetHashCode());

        public override string ToString()
            => base.ToString() + "::" + Constraint;
    }

    public class TypeReference : Type
    {
        public IReadOnlyList<Type> TypeArguments { get; }
        public int DefId => Definition.Id;

        public TypeReference(TypeDefinitionSymbol definition, IReadOnlyList<Type> args, TypeFactory factory)
            : base(definition.Name, definition, factory)
        {
            TypeArguments = args;
        }

        public override string ToString()
        {
            var tps = string.Join(", ", TypeArguments);
            return $"{Name}_{DefId}<{tps}>";
        }

        public override bool Equals(object obj)
            => base.Equals(obj)
               && obj is TypeReference other
               && DefId == other.DefId
               && TypeArguments.SequenceEqual(other.TypeArguments);

        public override int GetHashCode()
            => Hasher.Combine(base.GetHashCode(), DefId, Hasher.Hash(TypeArguments));
    }

    // A function signature generates types (and constraints)
    // It is like a function. 

    // TODO: delete
    //public class UnifiedType : Type
    //{
    //    public Type TypeA { get; }
    //    public Type TypeB { get; }
    //    public UnifiedType(Type typeA, Type typeB, TypeFactory factory)
    //        : base("_unified_", null, factory)
    //    {
    //        TypeA = typeA;
    //        TypeB = typeB;
    //    }

    //    public override string ToString()
    //        => $"Unified({TypeA}, {TypeB})";

    //    public override bool Equals(object obj)
    //        => base.Equals(obj)
    //            && obj is UnifiedType ut 
    //            && Equals(TypeA, ut.TypeA) 
    //            && Equals(TypeB, ut.TypeB);

    //    public override int GetHashCode()
    //        => Hasher.Combine(TypeA.GetHashCode(), TypeB.GetHashCode());
    //}

    //// TODO: delete
    //public class TypeUnion : Type
    //{
    //    public IReadOnlyList<Type> Options { get; }
    //    public TypeUnion(IReadOnlyList<Type> options, TypeFactory factory)
    //        : base("_union_", null, null)
    //    {
    //        if (options.Count < 2)
    //            throw new Exception("Expected at least two options");
    //        if (options.Distinct().Count() != options.Count)
    //            throw new Exception("Some of the types are the same");
    //        Options = options;
    //    }

    //    public override string ToString()
    //        => $"Union({string.Join(", ", Options)})";

    //    public override bool Equals(object obj)
    //        => base.Equals(obj)
    //            && obj is TypeUnion tu 
    //            && Options.SequenceEqual(tu.Options);

    //    public override int GetHashCode()
    //        => Hasher.Combine(Options.Select(opt => opt.GetHashCode()).ToArray());
    //}
}