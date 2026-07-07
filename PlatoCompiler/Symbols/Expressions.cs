using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Ara3D.Geometry.AST;
using Ara3D.Utils;

namespace Ara3D.Geometry.Compiler.Symbols
{
    public abstract class Expression : Symbol
    {
        public IReadOnlyList<Expression> Children { get; }

        protected Expression(params Expression[] children)
        {
            if (children.Contains(this))
                throw new Exception("Circular expression");
            Children = children;
        }

        public override IEnumerable<Symbol> GetChildSymbols()
            => Children;
    }

    public abstract class RefSymbol : Expression
    {
        public DefSymbol Def { get; }
        public TypeExpression Type => Def?.Type;
        public override string Name => Def.Name;

        protected RefSymbol(DefSymbol def, bool allowNulls = false)
        {
            Def = allowNulls ? def : def ?? throw new Exception("No definition provided");
        }

        public override string ToString() => Name;
    }

    public class KeywordRefSymbol : RefSymbol
    {
        public readonly string Keyword;
        public override string Name => Keyword;
        public override Symbol Rewrite(Func<Symbol, Symbol> f) => f(this);

        public KeywordRefSymbol(string keyword)
            : base(null, true)
        {
            Keyword = keyword;
        }

        public static KeywordRefSymbol Default => new KeywordRefSymbol("default");
    }

    public class TypeRefSymbol : RefSymbol
    {
        public TypeRefSymbol(TypeDef def)
            : base(def)
        {
        }

        public new TypeDef Def => base.Def as TypeDef;

        public override Symbol Rewrite(Func<Symbol, Symbol> f) => f(this);
    }

    public abstract class ParameterOrVariableRefSymbol : RefSymbol
    {
        protected ParameterOrVariableRefSymbol(DefSymbol def)
            : base(def)
        { }
    }

    public class VariableRefSymbol : ParameterOrVariableRefSymbol
    {
        public VariableRefSymbol(VariableDef def)
            : base(def)
        {
        }

        public override Symbol Rewrite(Func<Symbol, Symbol> f) => f(this);
        public new VariableDef Def => base.Def as VariableDef;
    }

    public class ParameterRefSymbol : ParameterOrVariableRefSymbol
    {
        public ParameterRefSymbol(ParameterDef def)
            : base(def)
        {
        }

        public new ParameterDef Def => base.Def as ParameterDef;
        public override Symbol Rewrite(Func<Symbol, Symbol> f) => f(this);
    }

    public class FunctionGroupRefSymbol : RefSymbol
    {
        public FunctionGroupRefSymbol(FunctionGroupDef funcs)
            : base(funcs)
        { }

        public new FunctionGroupDef Def => base.Def as FunctionGroupDef;
        public override Symbol Rewrite(Func<Symbol, Symbol> f) => f(this);
    }

    public class ConditionalExpression : Expression
    {
        public Expression Condition { get; }
        public Expression IfTrue { get; }
        public Expression IfFalse { get; }

        public ConditionalExpression(Expression condition, Expression ifTrue, Expression ifFalse)
            : base(condition, ifTrue, ifFalse)
        {
            Condition = condition;
            IfTrue = ifTrue;
            IfFalse = ifFalse;
        }

        public override string Name => "?:";

        public override string ToString()
            => $"({Condition}?{IfTrue}:{IfFalse})";

        public override Symbol Rewrite(Func<Symbol, Symbol> f) =>
            f(new ConditionalExpression(Condition?.Rewrite(f) as Expression, IfTrue?.Rewrite(f) as Expression, IfFalse?.Rewrite(f) as Expression));
    }

    public class Assignment : Expression
    {
        public Expression LValue { get; }
        public Expression RValue { get; }

        public override string Name => "=";

        public Assignment(Expression lvalue, Expression rvalue)
            : base(lvalue, rvalue)
            => (LValue, RValue) = (lvalue, rvalue);

        public override string ToString()
            => $"({LValue} = {RValue})";

        public override Symbol Rewrite(Func<Symbol, Symbol> f) 
            => f(new Assignment(
                LValue?.Rewrite(f) as Expression, 
                RValue?.Rewrite(f) as Expression));
    }

    public class Parenthesized : Expression
    {
        public Expression Expression { get; }

        public Parenthesized(Expression expression)
            : base(expression)
        {
            Expression = expression;
        }

        public override string Name => "()";

        public override string ToString()
            => $"({Expression}";

        public override Symbol Rewrite(Func<Symbol, Symbol> f) => f(
            new Parenthesized(Expression.Rewrite(f) as Expression));
    }

    public class Literal : Expression
    {
        public object Value { get; }
        public LiteralTypesEnum TypeEnum { get; }

        public Literal(LiteralTypesEnum typeEnum, object value)
            => (TypeEnum, Value) = (typeEnum, value);

        public override string Name
            => ToString();

        public override string ToString()
            => $"{Value}";

        public override Symbol Rewrite(Func<Symbol, Symbol> f) => f(this);
    }

    public class NewExpression : Expression
    {
        public TypeExpression Type { get; }
        public IReadOnlyList<Expression> Args { get; }
        public NewExpression(TypeExpression type, params Expression[] args)
            : base(args)
            => (Type, Args) = (type, args);
        public override string Name => "new";
        public override string ToString() => $"new {Type}({string.Join(", ", Args)})";
        public override Symbol Rewrite(Func<Symbol, Symbol> f) 
            => f(new NewExpression(Type.Rewrite(f) as TypeExpression, Args.Select(a => a?.Rewrite(f) as Expression).ToArray()));
    }

    public class FunctionCall : Expression
    {
        public Expression Function { get; }
        public bool HasArgList { get; }
        public IReadOnlyList<Expression> Args { get; }

        public FunctionCall(Expression function, bool hasArgList, params Expression[] args)
            : base(args.Prepend(function).ToArray())
        {
            Function = function;
            HasArgList = hasArgList;
            if (Function == null)
                Debug.WriteLine("Unexpected empty function");
            Args = args;
        }

        public override string Name
            => ".()";

        public override string ToString()
            => $"{Function}({string.Join(", ", Args)})";

        public override Symbol Rewrite(Func<Symbol, Symbol> f) => f(new FunctionCall(
            Function?.Rewrite(f) as Expression, HasArgList, 
            Args.Select(a => a?.Rewrite(f) as Expression).ToArray()));
    }

    public class Lambda : Expression
    {
        public FunctionDef Function { get; }

        public Lambda(FunctionDef function)
            => Function = function;

        public override string Name => "=>";

        public override IEnumerable<Symbol> GetChildSymbols()
            => new[] { Function };

        public override string ToString()
            //=> $"(\\({string.Join(", ", Function.Parameters)}) -> {Function.ReturnType}";
            => $"({Function.Parameters.Select(p => p.Name).JoinStringsWithComma()}) => {Function.Body}";

        public override Symbol Rewrite(Func<Symbol, Symbol> f) 
            => f(new Lambda(Function?.Rewrite(f) as FunctionDef));
    }

    public class ArrayLiteral : Expression
    {
        public override string Name => "[]";
        
        public IReadOnlyList<Expression> Expressions { get; }
        
        public ArrayLiteral(IReadOnlyList<Expression> expressions)
            => Expressions = expressions;

        public override string ToString()
            => $"[{Expressions.JoinStringsWithComma()}]";

        public override Symbol Rewrite(Func<Symbol, Symbol> f)
            => f(new ArrayLiteral(Expressions.Select(expr => expr.Rewrite(f) as Expression).ToArray()));
    }
}