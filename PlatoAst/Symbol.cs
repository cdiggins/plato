using System;
using System.Collections.Generic;
using System.Linq;

namespace PlatoAst
{
    /// <summary>
    /// An abstract value has a location, scope, and a type.
    /// It is used for type-checking, and method overload resolution. 
    /// </summary>
    public abstract class Symbol
    {
        public AstNode Location { get; }
        public Scope Scope { get; }

        protected Symbol(AstNode location, Scope scope)
            => (Location, Scope) = (location, scope);

        public override string ToString() => $"{GetType().Name}";

        public abstract IReadOnlyList<Symbol> Children { get; }
    }

    public class FunctionGroupSymbol : DefSymbol
    {
        public IReadOnlyList<MemberDefSymbol> Functions { get;  }
        
        public FunctionGroupSymbol(IEnumerable<MemberDefSymbol> functions, string name)
            : base(null, null, null, name)
        {
            Functions = functions.ToList();
            foreach (var f in Functions)
                if (f.Name != name)
                    throw new Exception($"All functions in group must have same name: {name}");
        }

        public FunctionGroupSymbol Add(MemberDefSymbol method)
            => new FunctionGroupSymbol(Functions.Append(method), Name);

        public override IReadOnlyList<Symbol> Children 
            => Functions;
    }

    public abstract class DefSymbol : Symbol
    {
        protected DefSymbol(AstNode location, Scope scope, TypeRefSymbol type, string name)
            : base(location, scope)
        {
            Type = type;
            Name = name;
        }

        public TypeRefSymbol Type { get; }
        public string Name { get; }
        public int Id { get; } = NextId++;

        public static int NextId = 0;

        public override bool Equals(object obj) => obj is DefSymbol ds && ds.Id == Id;
        public override int GetHashCode() => Id.GetHashCode();

        public override string ToString() => $"{GetType().Name}={Name}${Id}:{Type}";
    }

    public class RefSymbol : Symbol
    {
        public DefSymbol Def { get; }
        public TypeRefSymbol Type => Def.Type;
        public string Name => Def.Name;
        public int Id => Def.Id;

        public RefSymbol(AstNode location, Scope scope, DefSymbol def)
            : base(location, scope)
        {
            Def = def ?? 
                  throw new Exception("No definition provided");
        }

        public override string ToString() => $"{GetType().Name}={Name}${Id}:{Type}";

        // References have no children 
        public override IReadOnlyList<Symbol> Children => Array.Empty<Symbol>();
    }

    public class NoValueSymbol : Symbol
    {
        public NoValueSymbol() 
            : base(null, null)
        { }

        public static NoValueSymbol Instance = new NoValueSymbol();

        public override IReadOnlyList<Symbol> Children => Array.Empty<Symbol>();
    }

    public class RegionSymbol : Symbol
    {
        public override IReadOnlyList<Symbol> Children { get; }

        public RegionSymbol(AstNode location, Scope scope, params Symbol[] children)
            : base(location, scope)
            => Children = children;
    }

    public class Primitive : TypeDefSymbol
    {
        public Primitive(string name)
            : base(null, null)
        { }
    }

    public static class Primitives
    {
        public static TypeDefSymbol Kind = Create("Kind");

        public static TypeDefSymbol Void = Create("void");
        public static TypeDefSymbol Intrinsic = Create("intrinsic");
        public static TypeDefSymbol NotImplemented = Create("Not implemented yet");
        public static TypeDefSymbol TypeParameter = Create("TypeParameterDefSymbol");
        public static TypeDefSymbol Member = Create("MemberDefSymbol");
        public static TypeDefSymbol Inferred = Create("Infer");
        public static TypeDefSymbol Lambda = Create("Lambda");
        public static TypeDefSymbol Function = Create("Function");
        public static TypeDefSymbol Any = Create("Any");

        public static TypeDefSymbol Create(string name)
            => new TypeDefSymbol(name);
    }

    public class TypeDefSymbol : DefSymbol
    {
        public string Kind => AstTypeDeclaration?.Kind ?? "unknown";
        public AstTypeDeclaration AstTypeDeclaration => Location as AstTypeDeclaration;
        public List<MethodDefSymbol> Methods { get; } = new List<MethodDefSymbol>();
        public List<FieldDefSymbol> Fields { get; } = new List<FieldDefSymbol>();
        public List<TypeParameterDefSymbol> TypeParameters { get; } = new List<TypeParameterDefSymbol>();
        public List<TypeRefSymbol> Inherits { get; } = new List<TypeRefSymbol>();
        public List<TypeRefSymbol> Implements { get; } = new List<TypeRefSymbol>();
        public Dictionary<string, AstNode> Lookup { get; } = new Dictionary<string, AstNode>();

        public TypeDefSymbol(AstTypeDeclaration location, Scope scope)
            : base(location, scope, null, location.Name)
        { }

        public TypeDefSymbol(string name)
            : base(null, null, null, name)
        { }

        public override IReadOnlyList<Symbol> Children 
            => Array.Empty<Symbol>().Concat(Methods).Concat(Fields).Concat(TypeParameters).ToList();

        public IEnumerable<MemberDefSymbol> Members => Enumerable.Empty<MemberDefSymbol>()
            .Concat(Methods).Concat(Fields);

        public TypeRefSymbol ToRef => new TypeRefSymbol(null, null, this);
    }

    public class ConditionalSymbol : Symbol
    {
        public Symbol Condition { get; }
        public Symbol IfTrue { get; }
        public Symbol IfFalse { get; }

        public ConditionalSymbol(AstNode location, Scope scope, Symbol condition, Symbol ifTrue, Symbol ifFalse)
            : base(location, scope)
        {
            // TODO: unify the types of ifTrue and ifFalse
            Condition = condition;
            IfTrue = ifTrue;
            IfFalse = ifFalse;
        }

        public override IReadOnlyList<Symbol> Children => new []{ Condition, IfTrue, IfFalse};
    }

    public class FunctionSymbol : DefSymbol
    {
        public IReadOnlyList<ParameterSymbol> Parameters { get; }
        public Symbol Body { get; }

        public FunctionSymbol(AstNode location, Scope scope, string name, TypeRefSymbol returnType, Symbol body, params ParameterSymbol[] parameters)
            : base(location, scope, returnType, name)
        {
            Parameters = parameters;
            Body = body;
        }

        public override IReadOnlyList<Symbol> Children
            => Array.Empty<Symbol>().Concat(Parameters).Append(Body).ToList();
    }

    public class ParameterSymbol : DefSymbol
    {   
        public ParameterSymbol(AstNode location, Scope scope, string name, TypeRefSymbol type)
            : base(location, scope, type, name)
        { }

        public override IReadOnlyList<Symbol> Children
            => Array.Empty<Symbol>();
    }

    public class VariableSymbol : DefSymbol
    {
        public VariableSymbol(AstNode location, Scope scope, string name, TypeRefSymbol type)
            : base(location, scope, type, name)
        { }

        public override IReadOnlyList<Symbol> Children
            => Array.Empty<Symbol>();
    }

    public class AssignmentSymbol : Symbol
    {
        public Symbol LValue { get; }
        public Symbol RValue { get; }

        public AssignmentSymbol(AstNode location, Scope scope, Symbol lvalue, Symbol rvalue)
            : base(location, scope)
            => (LValue, RValue) = (lvalue, rvalue);

        public override IReadOnlyList<Symbol> Children => new [] { LValue, RValue };
    }

    public class ArgumentSymbol : Symbol
    {
        public Symbol Original { get; }
        public int Position { get; }

        public ArgumentSymbol(AstNode location, Scope scope, Symbol original, int position)
            : base(location, scope)
        {
            Original = original;
            Position = position;
        }

        public override IReadOnlyList<Symbol> Children => new[] { Original };
    }

    public class LiteralSymbol : Symbol
    {
        public object Value { get; }
        public LiteralSymbol(AstNode location, Scope scope, object value)
            : base(location, scope)
            => Value = value;

        public override IReadOnlyList<Symbol> Children => Array.Empty<Symbol>();
    }

    public class FunctionResultSymbol : Symbol
    {
        public Symbol Function { get; }
        public IReadOnlyList<ArgumentSymbol> Args { get; }
        public FunctionResultSymbol(AstNode location, Scope scope, Symbol function, params ArgumentSymbol[] args)
            : base(location, scope)
        {
            Function = function;
            Args = args;
        }

        public override IReadOnlyList<Symbol> Children => Args.Append(Function).ToList();
    }

    public class TypeRefSymbol : Symbol
    {
        public string Name => Def?.Name ?? "unresolved";
        public TypeDefSymbol Def { get; }
        public IReadOnlyList<TypeRefSymbol> TypeArgs { get; }

        public TypeRefSymbol(AstNode location, Scope scope, TypeDefSymbol def, params TypeRefSymbol[] args)
            : base(location, scope)
        {
            // TODO resinstate once type resolution is guaranteed. 
            //if (def == null) throw new Exception("Type not found");

            Def = def;
            TypeArgs = args;
        }

        public static TypeRefSymbol Unify(TypeRefSymbol a, TypeRefSymbol b)
            => a;

        public override IReadOnlyList<Symbol> Children => TypeArgs;

        public override string ToString()
        {
            if (TypeArgs.Count > 0) 
                return Name + $"<{string.Join(",", TypeArgs)}>";
            return Name;
        }

        public static TypeRefSymbol CreateFunction(params TypeRefSymbol[] types)
            => new TypeRefSymbol(null, null, Primitives.Function, types);
    }

    public abstract class MemberDefSymbol : DefSymbol
    {
        protected MemberDefSymbol(AstNode location, Scope scope, TypeRefSymbol type, string name)
            : base(location, scope, type, name)
        { }
    }

    public class FieldDefSymbol : MemberDefSymbol
    {
        public FieldDefSymbol(AstNode location, Scope scope, TypeRefSymbol type, string name)
            : base(location, scope, type, name)
        { }

        public override IReadOnlyList<Symbol> Children
            => Array.Empty<Symbol>();
    }

    public class MethodDefSymbol : MemberDefSymbol
    {
        // TODO: find a better way to deal with this. Don't make it mutable.
        public FunctionSymbol Function { get; set; }

        public MethodDefSymbol(AstNode location, Scope scope, TypeRefSymbol type, string name)
            : base(location, scope, type, name)
        { }

        public override IReadOnlyList<Symbol> Children
            => new [] { Function };
    }

    public class TypeParameterDefSymbol : MemberDefSymbol
    {
        public TypeParameterDefSymbol(AstNode location, Scope scope, string name)
            : base(location, scope, Primitives.TypeParameter.ToRef, name)
        { }

        public override IReadOnlyList<Symbol> Children
            => Array.Empty<Symbol>();
    }

    public class IntrinsicSymbol : Symbol
    {
        public string Name { get; }

        public IntrinsicSymbol(AstNode location, Scope scope, string name)
            : base(location, scope)
        {
            Name = name;
        }

        public override IReadOnlyList<Symbol> Children 
            => Array.Empty<Symbol>();
    }

    public static class SymbolExtensions
    {
        public static IEnumerable<Symbol> AllDescendantSymbols(this Symbol symbol)
        {
            if (symbol == null)
                yield break;
            yield return symbol;
            if (symbol is Symbol cs)
                foreach (var child in cs.Children.SelectMany(AllDescendantSymbols))
                    yield return child;
        }

        // NOTE: does not include lambdas
        public static IEnumerable<FunctionSymbol> GetAllFunctions(this IEnumerable<TypeDefSymbol> typeDefs)
        {
            return typeDefs.SelectMany(t => t.Methods.Select(m => m.Function)).Where(f => f != null);
        }

        public static DefSymbol GetDef(this Symbol symbol)
        {
            return (symbol as RefSymbol)?.Def;
        }
    }
}