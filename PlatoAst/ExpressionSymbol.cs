using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace PlatoAst
{
    public abstract class Symbol
    {
        public override string ToString() => $"{GetType().Name}";
        public abstract IReadOnlyList<Symbol> Children { get; }
    }

    public abstract class ExpressionSymbol : Symbol
    {

    }

    public abstract class StatementSymbol : Symbol
    {
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

        public RefSymbol(DefSymbol def)
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
        public static NoValueSymbol Instance = new NoValueSymbol();
        public override IReadOnlyList<Symbol> Children => Array.Empty<Symbol>();
    }

    public class WhileStatement : StatementSymbol
    {
        public ExpressionSymbol Condition { get; }
        public StatementSymbol Statement { get; }

        public WhileStatement(ExpressionSymbol condition, StatementSymbol statement)
        {
            Condition = condition;
            Statement = statement;
        }

        public override IReadOnlyList<Symbol> Children => 
            new Symbol[] { Condition, Statement };
    }

    public class VarDeclStatement : StatementSymbol
    {
        public string Name { get; }
        public ExpressionSymbol Expression
        {
            get;
        }
        public VarDeclStatement(string name, ExpressionSymbol expression)
        {
            Name = name;
            Expression = expression;
        }
        public override IReadOnlyList<Symbol> Children => new[] { Expression };
    }

    public class ExpressionStatement : StatementSymbol
    {
        public ExpressionSymbol Expression { get; }
        public ExpressionStatement(ExpressionSymbol expression)
        {
            Expression = expression;
        }

        public override IReadOnlyList<Symbol> Children => new[] { Expression };
    }

    public class ReturnStatementSymbol : StatementSymbol
    {
        public ExpressionSymbol Expression { get; }
        public ReturnStatementSymbol(ExpressionSymbol expression)
        {
            Expression = expression;
        }

        public override IReadOnlyList<Symbol> Children => new[] { Expression };
    }

    public class BlockStatementSymbol : StatementSymbol
    {
        public override IReadOnlyList<Symbol> Children { get; }

        public BlockStatementSymbol(params StatementSymbol[] children)
            => Children = children;
    }

    public class PredefinedSymbol : DefSymbol
    {
        public PredefinedSymbol(TypeRefSymbol typeRef, string name)
            : base(typeRef, name)
        { }

        public override IReadOnlyList<Symbol> Children => Array.Empty<Symbol>();
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
        public static TypeDefSymbol Self = Create("Self");

        public static TypeDefSymbol Create(string name)
            => new TypeDefSymbol("primitive", name);
    }

    public class TypeDefSymbol : DefSymbol
    {
        public string Kind { get; }

        public IEnumerable<FunctionSymbol> Functions => Enumerable.Empty<FunctionSymbol>()
            .Concat(Methods.Select(m => m.Function))
            .Concat(Fields.Select(f => f.Function));

        public List<MethodDefSymbol> Methods { get; } = new List<MethodDefSymbol>();
        public List<FieldDefSymbol> Fields { get; } = new List<FieldDefSymbol>();
        public List<TypeParameterDefSymbol> TypeParameters { get; } = new List<TypeParameterDefSymbol>();
        public List<TypeRefSymbol> Inherits { get; } = new List<TypeRefSymbol>();
        public List<TypeRefSymbol> Implements { get; } = new List<TypeRefSymbol>();
        public Dictionary<string, AstNode> Lookup { get; } = new Dictionary<string, AstNode>();

        public TypeDefSymbol(string kind, string name)
            : base(null, name)
        {
            Kind = kind;
        }

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
        public Symbol ExpressionOrStatementBody { get; }

        public FunctionSymbol(string name, TypeRefSymbol returnType, Symbol expressionOrStatementBody, params ParameterSymbol[] parameters)
            : base(returnType, name)
        {
            Parameters = parameters;
            ExpressionOrStatementBody = expressionOrStatementBody;
        }

        public BlockStatementSymbol Body
        {
            get
            {
                if (ExpressionOrStatementBody == null)
                    return null;

                if (ExpressionOrStatementBody is BlockStatementSymbol bss)
                    return bss;
                if (ExpressionOrStatementBody is ExpressionSymbol es)
                    return new BlockStatementSymbol(
                        new ReturnStatementSymbol(es));
                throw new Exception("Function expressionOrStatementBody should be a block statement or an expression symbol");
            }
        }
        public override IReadOnlyList<Symbol> Children
            => Array.Empty<Symbol>().Concat(Parameters).Append(ExpressionOrStatementBody).ToList();
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
        public LiteralSymbol(object value) => Value = value;
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
        public string Name => Def?.Name ?? "unresolved";
        public TypeDefSymbol Def { get; }
        public IReadOnlyList<TypeRefSymbol> TypeArgs { get; }

        public TypeRefSymbol(TypeDefSymbol def, params TypeRefSymbol[] args)
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
            => new TypeRefSymbol(Primitives.Function, types);
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

        public string FieldVariableName()
            => $"__{Name}";

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
        public TypeParameterDefSymbol(AstNode location, Scope scope, string name)
            : base("typeparameter", name)
        { }

        public override IReadOnlyList<Symbol> Children
            => Array.Empty<Symbol>();
    }


    public static class SymbolExtensions
    {
        public static IEnumerable<FunctionSymbol> GetLambdas(this FunctionSymbol symbol)
            => symbol.ExpressionOrStatementBody.GetDescendantSymbols().OfType<FunctionSymbol>();

        public static IEnumerable<Symbol> GetDescendantSymbols(this Symbol symbol)
        {
            if (symbol == null)
                yield break;
            yield return symbol;
            if (symbol is Symbol cs)
                foreach (var child in cs.Children.SelectMany(GetDescendantSymbols))
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