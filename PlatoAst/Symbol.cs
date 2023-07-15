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

        public override string ToString()
        {
            return $"{GetType().Name}";
        }
    }

    public abstract class ContainerSymbol : Symbol
    {
        protected ContainerSymbol(AstNode location, Scope scope)
            : base(location, scope)
        { }

        public abstract IReadOnlyList<Symbol> Children { get; }
    }

    public class DefSymbol : Symbol
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
    }

    public class RefSymbol : Symbol
    {
        public DefSymbol Def { get; }
        public TypeRefSymbol Type => Def.Type;
        public string Name => Def.Name;

        public RefSymbol(AstNode location, Scope scope, DefSymbol def)
            : base(location, scope)
        {
            Def = def;
        }
    }

    public class NoValueSymbol : ContainerSymbol
    {
        public NoValueSymbol() 
            : base(null, null)
        { }

        public static NoValueSymbol Instance = new NoValueSymbol();

        public override IReadOnlyList<Symbol> Children => Array.Empty<Symbol>();
    }

    public class RegionSymbol : ContainerSymbol
    {
        public override IReadOnlyList<Symbol> Children { get; }

        public RegionSymbol(AstNode location, Scope scope, params Symbol[] children)
            : base(location, scope)
            => Children = children;
    }

    public class TypeDefSymbol : DefSymbol
    {
        public string Kind => AstTypeDeclaration.Kind;
        public AstTypeDeclaration AstTypeDeclaration => Location as AstTypeDeclaration;
        public List<MethodDefSymbol> Methods { get; } = new List<MethodDefSymbol>();
        public List<FieldDefSymbol> Fields { get; } = new List<FieldDefSymbol>();
        public List<TypeParameterDefSymbol> TypeParameters { get; } = new List<TypeParameterDefSymbol>();
        public Dictionary<string, AstNode> Lookup { get; } = new Dictionary<string, AstNode>();

        public TypeDefSymbol(AstTypeDeclaration location, Scope scope)
            : base(location, scope, TypeRefSymbol.Create(location.Name), location.Name)
        {
        }
    }

    public class ConditionalSymbol : ContainerSymbol
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
            : base(location, scope, TypeRefSymbol.CreateFunction(returnType, parameters.Select(p => p.Type).ToArray()), name)
        {
            Parameters = parameters;
            Body = body;
        }
    }

    public class ParameterSymbol : DefSymbol
    {   
        public ParameterSymbol(AstNode location, Scope scope, string name, TypeRefSymbol type)
            : base(location, scope, type, name)
        { }
    }

    public class VariableSymbol : DefSymbol
    {
        public VariableSymbol(AstNode location, Scope scope, string name, TypeRefSymbol type)
            : base(location, scope, type, name)
        { }
    }

    public class AssignmentSymbol : ContainerSymbol
    {
        public Symbol LValue { get; }
        public Symbol RValue { get; }

        public AssignmentSymbol(AstNode location, Scope scope, Symbol lvalue, Symbol rvalue)
            : base(location, scope)
            => (LValue, RValue) = (lvalue, rvalue);

        public override IReadOnlyList<Symbol> Children => new [] { LValue, RValue };
    }

    public class ArgumentSymbol : ContainerSymbol
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

    public class LiteralSymbol : ContainerSymbol
    {
        public object Value { get; }
        public LiteralSymbol(AstNode location, Scope scope, object value)
            : base(location, scope)
            => Value = value;

        public override IReadOnlyList<Symbol> Children => Array.Empty<Symbol>();
    }

    public class MemberRefSymbol : ContainerSymbol
    {
        public Symbol Receiver { get; }
        public string Name { get; }

        public MemberRefSymbol(AstNode location, Scope scope, string name, Symbol receiver)
            : base(location, scope)
        {
            Receiver = receiver;
            Name = name;
        }

        public override IReadOnlyList<Symbol> Children => new [] { Receiver };
    }

    public class FunctionResultSymbol : ContainerSymbol
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

    public class TypeRefSymbol : ContainerSymbol
    {
        public string Name { get; }
        public IReadOnlyList<TypeRefSymbol> TypeArgs { get; }

        public TypeRefSymbol(AstNode location, Scope scope, string name, params TypeRefSymbol[] args)
            : base(location, scope)
        {
            Name = name;
            TypeArgs = args;
        }

        // TODO: handle type arguments 
        public TypeRefSymbol(Type type)
            : this(null, null, type.Name)
        { }

        public static TypeRefSymbol Create(AstTypeNode typeNode, Scope scope)
            => new TypeRefSymbol(typeNode, scope, typeNode.Name, typeNode.TypeArguments.Select(ta => Create(ta, scope)).ToArray());

        public static TypeRefSymbol Create(string name, params TypeRefSymbol[] args)
            => new TypeRefSymbol(null, null, name, args);

        public static TypeRefSymbol Void = Create("void");
        public static TypeRefSymbol Intrinsic = Create("intrinsic");
        public static TypeRefSymbol MetaType = Create("MetaType");
        public static TypeRefSymbol NotImplemented = Create("Not implemented yet");
        public static TypeRefSymbol TypeParameter = Create("TypeParameterDefSymbol");
        public static TypeRefSymbol Member = Create("MemberDefSymbol");
        public static TypeRefSymbol Inferred = Create("Infer");

        public static TypeRefSymbol CreateFunction(TypeRefSymbol returnType, params TypeRefSymbol[] parameterTypes)
            => Create("Func", parameterTypes.Append(returnType).ToArray());

        public static TypeRefSymbol Unify(TypeRefSymbol a, TypeRefSymbol b)
            => a;

        public override IReadOnlyList<Symbol> Children => TypeArgs;

        public override string ToString()
        {
            if (TypeArgs.Count > 0) 
                return Name + $"<{string.Join(",", TypeArgs)}>";
            return Name;
        }
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
    }

    public class MethodDefSymbol : MemberDefSymbol
    {
        // TODO: find a better way to deal with this. Don't make it mutable.
        public FunctionSymbol Function { get; set; }

        public MethodDefSymbol(AstNode location, Scope scope, TypeRefSymbol type, string name)
            : base(location, scope, type, name)
        { }
    }

    public class TypeParameterDefSymbol : MemberDefSymbol
    {
        public TypeParameterDefSymbol(AstNode location, Scope scope, string name)
            : base(location, scope, TypeRefSymbol.TypeParameter, name)
        { }
    }

    public class IntrinsicSymbol : ContainerSymbol
    {
        public string Name { get; }

        public IntrinsicSymbol(AstNode location, Scope scope, string name)
            : base(location, scope)
        {
            Name = name;
        }

        public override IReadOnlyList<Symbol> Children => Array.Empty<Symbol>();
    }

    public static class SymbolExtensions
    {
        public static IEnumerable<Symbol> AllDescendantSymbols(this Symbol symbol)
        {
            if (symbol == null)
                yield break;
            yield return symbol;
            if (symbol is ContainerSymbol cs)
                foreach (var child in cs.Children.SelectMany(AllDescendantSymbols))
                    yield return child;
        }
    }
}