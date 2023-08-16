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

        public static int Hash<T>(IEnumerable<T> objects)
            => Combine(objects.Select(x => GetHashCode(x)));
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
            => other != null && Name == other.Name;

        public override bool Equals(object obj)
            => obj is Type t && Equals(t);

        public override int GetHashCode()
            => Hasher.Hash(Name);
    }

    public class TypeVariable : Type, IEquatable<TypeVariable>
    {
        public int Id { get; } = Symbol.NextId++;

        public TypeVariable(string name, Symbol declaration, TypeFactory factory)
            : base(name,declaration, factory)
        { }

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
            => Hasher.Combine(base.GetHashCode(), DefId, Hasher.Hash(TypeArguments));
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

    public class Concept
    {
        public IReadOnlyList<TypeReference> InheritedTypes { get; }
        public IReadOnlyList<Function> Functions { get; }
        public SelfType Self { get; } 
        public TypeDefSymbol Symbol { get; }
        public string Name => Symbol.Name;

        public Concept(TypeDefSymbol symbol, TypeFactory factory)
        {
            if (!symbol.IsConcept())
                throw new Exception("Expected a concept");
            Self = factory.CreateSelf(this);
            Symbol = symbol;
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
        public List<Function> Functions { get; } = new List<Function>();

        public Function Register(Function f)
        {
            Functions.Add(f);
            return f;
        }

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
            => Register(new TypeReference(symbol, this));
        
        public TypeReference CreateReference(ParameterSymbol symbol) 
            => CreateReference(symbol.Type);
        
        public AnyType CreateAny() 
            => Register(new AnyType(this));
        
        public Function CreateFunction(FunctionSymbol symbol) 
            => Register(new Function(symbol, this));

        public TypeFactory(IReadOnlyList<TypeDefSymbol> types)
        {
            Concepts = types.Where(t => t.IsConcept()).Select(t => new Concept(t, this)).ToList();

            // Note: interesting exercise in assuring that your hash-code/equals are implemented correctly. 
            foreach (var tr1 in TypeReferences)
            {
                foreach (var tr2 in TypeReferences)
                {
                    var refEquals = ReferenceEquals(tr1, tr2);
                    Debug.Assert(refEquals == ReferenceEquals(tr2, tr1), "Reference equals should be equivalent regardless of order");

                    var equals = tr1.Equals(tr2);
                    Debug.Assert(equals == tr2.Equals(tr1), "Equals should be equivalent regardless of order");
                    
                    var s1 = tr1.ToString();
                    var s2 = tr2.ToString();
                    var strEquals = s1.Equals(s2);
                    Debug.Assert(equals == strEquals, "Value and string equivalency should be the same");
                    
                    var h1 = s1.GetHashCode();
                    var h2 = s2.GetHashCode();
                    Debug.Assert(!equals || h1 == h2, "When two values are equal, they should have the same hash code");
                    
                }
            }
        }
    }
}