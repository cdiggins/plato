using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Plato.Compiler
{
    public static class Hasher
    {
        public static int Combine(int a, int b)
        {
            unchecked
            {
                return a * 397 ^ b;
            }
        }

        public static int Combine(params int[] codes) 
            => codes.Aggregate(0, Combine);

        public static int Combine(IEnumerable<int> codes)
            => codes.Aggregate(0, Combine);

        public static int GetHashCode(object o)
            => o?.GetHashCode() ?? 0;

        public static int Hash(params object[] objects)
            => Combine(objects.Select(GetHashCode));
    }

    public abstract class Type : IEquatable<Type>
    {
        public string Name { get; }
        public Symbol Declaration { get; }

        // ReSharper disable once UnusedParameter.Local
        protected Type(string name, Symbol declaration, TypeFactory _)
        {
            Name = name;
            Declaration = declaration;
        }

        public override string ToString() 
            => $"{Name}";

        public bool Equals(Type other)
            => other != null && Name == other.Name && Equals(Declaration, other.Declaration);

        public override bool Equals(object obj)
            => obj is Type t && Equals(t);

        public override int GetHashCode()
            => Hasher.Hash(Name, Declaration);
    }

    public class TypeVariable : Type, IEquatable<TypeVariable>
    {
        public int Id { get; }

        public TypeVariable(string name, Symbol declaration, TypeFactory factory)
            : base(name,declaration, factory)
            => Id = factory.NumTypeVars;

        public override string ToString()
            => $"{Name}_{Id}";

        public bool Equals(TypeVariable other)
            => other != null && base.Equals(other) && Id == other.Id;

        public override bool Equals(object obj)
            => obj is TypeVariable tv && Equals(tv);

        public override int GetHashCode()
            => Hasher.Hash(base.GetHashCode(), Id);
    }

    public class AnyType : TypeVariable, IEquatable<AnyType>
    {
        public AnyType(TypeFactory factory)
            : base("Any", null, factory)
        { }

        public bool Equals(AnyType other)
            => base.Equals(other);
    }

    public class SelfType : TypeVariable, IEquatable<SelfType>
    {
        public SelfType(TypeFactory factory)
            : base("Self", null, factory)
        { }

        public bool Equals(SelfType other)
            => base.Equals(other);
    }

    public class TypeReference : Type, IEquatable<TypeReference>
    {
        public IReadOnlyList<Type> TypeArguments { get; }
        public int DefId => TypeDef.Id;
        public TypeDefSymbol TypeDef { get; }

        public TypeReference(TypeRefSymbol symbol, TypeFactory factory)
            : base(symbol.Name, symbol, factory)
        {
            TypeDef = symbol.Def;
            if (TypeDef == null)
                throw new Exception("Missing type definition");
            var nArgs = symbol.TypeArgs.Count;
            var nParams = symbol.Def.TypeParameters.Count;
            if (nArgs > nParams)
                throw new Exception($"Passed too many type arguments {nArgs} expected {nParams}");
            var list = new List<Type>();
            for (var i = 0; i < nParams; ++i)
            {
                var p = symbol.Def.TypeParameters[i];
                var arg = i < nArgs ? factory.CreateReference(symbol.TypeArgs[i]) : null;
                if (arg != null)
                    list.Add(arg);
                else if (p.Constraint != null)
                    list.Add(factory.CreateReference(p.Constraint));
                else
                    list.Add(factory.CreateAny());
            }

            TypeArguments = list;
        }

        public override string ToString()
        {
            var tps = string.Join(", ", TypeArguments);
            return $"{Name}_{DefId}<{tps}>";
        }

        public bool Equals(TypeReference other)
            => other != null && base.Equals(other) && DefId == other.DefId && TypeArguments.SequenceEqual(other.TypeArguments);

        public override bool Equals(object obj)
            => obj is TypeReference tr && Equals(tr);

        public override int GetHashCode()
            => Hasher.Combine(base.GetHashCode(), DefId, Hasher.Combine(TypeArguments.GetHashCode()));
    }

    // A function signature generates types (and constraints)
    // It is like a function. 
    public class FunctionSignature : IEquatable<FunctionSignature>
    {
        public IReadOnlyList<Type> Parameters { get; }
        public Type ReturnType { get; }
        public string Name { get; }

        public FunctionSignature(FunctionSymbol f, TypeFactory factory)
        {
            Name = f.Name;
            Parameters = f.Parameters.Select(factory.CreateReference).ToList();
            ReturnType = factory.CreateReference(f.Type);
        }

        public override string ToString()
            => $"{Name}({string.Join(", ", Parameters)}): {ReturnType}";

        public bool Equals(FunctionSignature other)
            => other != null
               && Name == other.Name
               && Equals(ReturnType, other.ReturnType)
               && Parameters.SequenceEqual(other.Parameters);

        public override bool Equals(object obj)
            => obj is FunctionSignature fs && Equals(fs);

        public override int GetHashCode()
            => Hasher.Hash(Parameters.Cast<object>().Append(Name).Append(ReturnType).ToArray());
    }

    public class Function
    {
        public FunctionSignature Signature { get; }
        public Function(FunctionSymbol f, TypeFactory factory)
            => Signature = new FunctionSignature(f, factory);
        public override string ToString() 
            => Signature.ToString();
    }

    public class TypeParameter : Type
    {
        public int Index { get; }

        public TypeParameter(int index, TypeParameterDefSymbol symbol, TypeFactory factory)
            : base(symbol.Name, symbol, factory) 
            => Index = index;

        public override string ToString() 
            => $"{Name}{Index}";
    }

    public class Concept
    {
        public IReadOnlyList<TypeReference> InheritedTypes { get; }
        public IReadOnlyList<Function> Functions { get; }
        public IReadOnlyList<TypeParameter> TypeParameters { get; }
        public SelfType Self { get; } 
        public TypeDefSymbol Symbol { get; }
        public string Name => Symbol.Name;

        public Concept(TypeDefSymbol symbol, TypeFactory factory)
        {
            if (!symbol.IsConcept())
                throw new Exception("Expected a concept");
            Self = factory.CreateSelf(this);
            Symbol = symbol;
            TypeParameters = symbol.TypeParameters.Select(factory.CreateTypeParameter).ToList();
            InheritedTypes = symbol.Inherits.Select(factory.CreateReference).ToList();
            Functions = symbol.Functions.Select(factory.CreateFunction).ToList();
        }
    }

    public class TypeFactory
    {
        public Dictionary<Symbol, Type> TypesFromSymbolDeclarations { get; } = new Dictionary<Symbol, Type>();
        public List<TypeVariable> TypeVariables { get; } = new List<TypeVariable>();
        public IEnumerable<TypeReference> TypeReferences => AllTypes.OfType<TypeReference>();
        public HashSet<Type> AllTypes { get; } = new HashSet<Type>();
        public IReadOnlyList<Concept> Concepts { get; }

        public int NumTypeVars => TypeVariables.Count;

        public T Register<T>(T self) where T : Type
        {
            var typeVar = self as TypeVariable;
            var typeRef = self as TypeReference;
            var sym = self.Declaration;
            if (sym != null)
            {
                TypesFromSymbolDeclarations[sym] = self;
            }

            if (typeVar != null)
            {
                Debug.Assert(!TypeVariables.Contains(typeVar));
                TypeVariables.Add(typeVar);
                Debug.Assert(!AllTypes.Contains(self));
                AllTypes.Add(self);
                return self;
            }

            if (typeRef != null)
            {
                AllTypes.Add(self);
                return self;
            }

            throw new Exception($"{self} is neither a type reference or a type variable");
        }

        public SelfType CreateSelf(Concept concept) 
            => Register(new SelfType(this));

        public TypeReference CreateReference(TypeRefSymbol symbol)
            => new TypeReference(symbol, this);

        public TypeParameter CreateTypeParameter(TypeParameterDefSymbol symbol, int index) 
            => new TypeParameter(index, symbol, this);
        
        public TypeReference CreateReference(ParameterSymbol symbol) 
            => CreateReference(symbol.Type);
        
        public AnyType CreateAny() 
            => new AnyType(this);
        
        public Function CreateFunction(FunctionSymbol symbol) 
            => new Function(symbol, this);

        public TypeFactory(IReadOnlyList<TypeDefSymbol> types)
        {
            Concepts = types.Where(t => t.IsConcept()).Select(t => new Concept(t, this)).ToList();
        }
    }
}