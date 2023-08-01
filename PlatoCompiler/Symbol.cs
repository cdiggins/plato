using System;
using System.Collections.Generic;
using System.Linq;

namespace Plato.Compiler
{
    public abstract class Symbol
    {
        public override string ToString() => $"{GetType().Name}";
        public abstract IReadOnlyList<Symbol> Children { get; }
        public int Id { get; } = NextId++;  
        public static int NextId = 0;
        public override bool Equals(object obj) => obj is Symbol s && s.Id == Id;
        public override int GetHashCode() => Id.GetHashCode();
    }

    public class FunctionGroupSymbol : DefSymbol
    {
        public IReadOnlyList<MemberDefSymbol> Functions { get;  }
        
        public FunctionGroupSymbol(IEnumerable<MemberDefSymbol> functions, string name)
            : base(null, name)
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
        protected DefSymbol(TypeRefSymbol type, string name)
        {
            Type = type;
            Name = name;
        }

        public TypeRefSymbol Type { get; }
        public string Name { get; }
        public string UniqueName => Name + "_" + Id;

        public override string ToString() => $"{GetType().Name}={Name}${Id}:{Type}";
    }

    public class RefSymbol : Symbol
    {
        public DefSymbol Def { get; }
        public TypeRefSymbol Type => Def.Type;
        public string Name => Def.Name;

        public RefSymbol(DefSymbol def)
        {
            Def = def ?? 
                  throw new Exception("No definition provided");
        }

        public override string ToString() => $"Ref=>{Def}";

        // References have no children 
        public override IReadOnlyList<Symbol> Children => Array.Empty<Symbol>();
    }

    public class NoValueSymbol : Symbol
    {
        public static NoValueSymbol Instance = new NoValueSymbol();
        public override IReadOnlyList<Symbol> Children => Array.Empty<Symbol>();
    }
    
    public class PredefinedSymbol : DefSymbol
    {
        public PredefinedSymbol(TypeRefSymbol typeRef, string name)
            : base(typeRef, name)
        { }

        public override IReadOnlyList<Symbol> Children => Array.Empty<Symbol>();
    }

    public static class PrimitiveTypes
    {
        public static TypeDefSymbol Kind = Create("Kind");

        public static TypeDefSymbol Lambda = Create("Lambda");
        public static TypeDefSymbol Function = Create("Function");
        public static TypeDefSymbol Any = Create("Any");
        public static TypeDefSymbol Self = Create("Self");

        public static TypeDefSymbol Tuple = Create("Tuple");

        public static TypeDefSymbol String = Create("String");
        public static TypeDefSymbol Bool = Create("Bool");
        public static TypeDefSymbol Int = Create("Int");
        public static TypeDefSymbol Float = Create("Float");
        public static TypeDefSymbol Type = Create("Type");

        public static TypeDefSymbol Create(string name)
            => new TypeDefSymbol(TypeKind.Primitive, name);
    }

    public class TypeDefSymbol : DefSymbol
    {
        public TypeKind Kind { get; }

        public IEnumerable<FunctionSymbol> Functions => Enumerable.Empty<FunctionSymbol>()
            .Concat(Methods.Select(m => m.Function))
            .Concat(Fields.Select(f => f.Function));

        public List<MethodDefSymbol> Methods { get; } = new List<MethodDefSymbol>();
        public List<FieldDefSymbol> Fields { get; } = new List<FieldDefSymbol>();
        public List<TypeParameterDefSymbol> TypeParameters { get; } = new List<TypeParameterDefSymbol>();
        public List<TypeRefSymbol> Inherits { get; } = new List<TypeRefSymbol>();
        public List<TypeRefSymbol> Implements { get; } = new List<TypeRefSymbol>();
        public Dictionary<string, AstNode> Lookup { get; } = new Dictionary<string, AstNode>();

        public TypeDefSymbol(TypeKind kind, string name)
            : base(null, name)
        {
            Kind = kind;
        }

        public IEnumerable<TypeDefSymbol> GetSelfAndAllInheritedTypes()
            => Inherits.SelectMany(c => c.Def.GetSelfAndAllInheritedTypes()).Append(this);

        public IEnumerable<TypeDefSymbol> GetAllImplementedConcepts()
            => Implements.SelectMany(c => c.Def.GetSelfAndAllInheritedTypes()).Distinct();

        public override IReadOnlyList<Symbol> Children 
            => Array.Empty<Symbol>().Concat(Methods).Concat(Fields).Concat(TypeParameters).ToList();

        public IEnumerable<MemberDefSymbol> Members => Enumerable.Empty<MemberDefSymbol>()
            .Concat(Methods).Concat(Fields);

        public TypeRefSymbol ToRef => new TypeRefSymbol(this);
    }

    public class ConditionalExpressionSymbol : Symbol
    {
        public Symbol Condition { get; }
        public Symbol IfTrue { get; }
        public Symbol IfFalse { get; }

        public ConditionalExpressionSymbol(Symbol condition, Symbol ifTrue, Symbol ifFalse)
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

        public FunctionSymbol(string name, TypeRefSymbol returnType, Symbol body, params ParameterSymbol[] parameters)
            : base(returnType, name)
        {
            Parameters = parameters;
            Body = body;
        }
        
        public override IReadOnlyList<Symbol> Children
            => Array.Empty<Symbol>().Concat(Parameters).Append(Body).ToList();
    }

    public class ParameterSymbol : DefSymbol
    {   
        public ParameterSymbol(string name, TypeRefSymbol type)
            : base(type, name)
        { }

        public override IReadOnlyList<Symbol> Children
            => Array.Empty<Symbol>();
    }

    public class VariableSymbol : DefSymbol
    {
        public VariableSymbol(string name, TypeRefSymbol type)
            : base(type, name)
        { }

        public override IReadOnlyList<Symbol> Children
            => Array.Empty<Symbol>();
    }

    public class AssignmentSymbol : Symbol
    {
        public Symbol LValue { get; }
        public Symbol RValue { get; }

        public AssignmentSymbol(Symbol lvalue, Symbol rvalue)
            => (LValue, RValue) = (lvalue, rvalue);

        public override IReadOnlyList<Symbol> Children => new [] { LValue, RValue };
    }

    public class ArgumentSymbol : Symbol
    {
        public Symbol Original { get; }
        public int Position { get; }

        public ArgumentSymbol(Symbol original, int position)
        {
            Original = original;
            Position = position;
        }

        public override IReadOnlyList<Symbol> Children => new[] { Original };
    }

    public class LiteralSymbol : Symbol
    {
        public object Value { get; }
        public LiteralTypes Type { get; }
        public LiteralSymbol(LiteralTypes type, object value) => (Type, Value) = (type, value);
        public override IReadOnlyList<Symbol> Children => Array.Empty<Symbol>();
    }

    public class FunctionCallSymbol : Symbol
    {
        public Symbol Function { get; }
        public IReadOnlyList<ArgumentSymbol> Args { get; }
        public FunctionCallSymbol(Symbol function, params ArgumentSymbol[] args)
        {
            Function = function;
            Args = args;
        }

        public override IReadOnlyList<Symbol> Children => Args.Append(Function).ToList();
    }

    public class TypeRefSymbol : Symbol
    {
        public string Name => Def?.UniqueName ?? "unresolved";
        public TypeDefSymbol Def { get; }
        public IReadOnlyList<TypeRefSymbol> TypeArgs { get; }

        public TypeRefSymbol(TypeDefSymbol def, params TypeRefSymbol[] args)
        {
            Def = def ?? throw new Exception("Type not found");
            TypeArgs = args;
        }

        public override IReadOnlyList<Symbol> Children => TypeArgs;

        public override string ToString()
        {
            var kind = Def?.Kind.ToString() ?? "";
            if (TypeArgs.Count > 0) 
                return kind + ":" + Name + $"<{string.Join(",", TypeArgs)}>";
            return kind + ":" + Name;
        }

        public static TypeRefSymbol CreateFunction(params TypeRefSymbol[] types)
            => new TypeRefSymbol(PrimitiveTypes.Function, types);
    }

    public abstract class MemberDefSymbol : DefSymbol
    {
        protected MemberDefSymbol(TypeDefSymbol parentType, TypeRefSymbol type, string name)
            : base(type, name)
        {
            ParentType = parentType;
        }

        public TypeDefSymbol ParentType { get; }
    }

    public class FieldDefSymbol : MemberDefSymbol
    {
        public FieldDefSymbol(TypeDefSymbol parentType, TypeRefSymbol type, string name)
            : base(parentType, type, name)
        {
            Function = new FunctionSymbol(Name, Type, ToRefSymbol(),
                new ParameterSymbol("self", parentType.ToRef));
        }

        public override IReadOnlyList<Symbol> Children
            => new [] { Function };

        public RefSymbol ToRefSymbol()
            => new RefSymbol(this);

        public FunctionSymbol Function { get; }
    }

    public class MethodDefSymbol : MemberDefSymbol
    {
        // TODO: find a better way to deal with this. Don't make it mutable.
        public FunctionSymbol Function { get; set; }

        public MethodDefSymbol(TypeDefSymbol parentType, TypeRefSymbol type, string name) 
            : base(parentType, type, name)
        { }

        public override IReadOnlyList<Symbol> Children
            => new [] { Function };
    }

    public class TypeParameterDefSymbol : TypeDefSymbol
    {
        public TypeParameterDefSymbol(string name)
            : base(TypeKind.Variable, name)
        { }

        public override IReadOnlyList<Symbol> Children
            => Array.Empty<Symbol>();
    }
}