using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Plato.Compiler.Ast;

namespace Plato.Compiler.Symbols
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

    public abstract class Reference : Expression
    {
        public DefinitionSymbol Definition { get; }
        public TypeExpression Type => Definition.Type;
        public string Name => Definition.Name;

        protected Reference(DefinitionSymbol def)
        {
            Definition = def ?? throw new Exception("No definition provided");
        }

        public override string ToString() => Name;
    }

    public class PredefinedReference : Reference
    {
        public PredefinedReference(PredefinedDefinition def)
            : base(def)
        { }

        public new PredefinedDefinition Definition => base.Definition as PredefinedDefinition;
    }

    public class ParameterReference : Reference
    {
        public ParameterReference(ParameterDefinition def)
            : base(def)
        { }

        public new ParameterDefinition Definition => base.Definition as ParameterDefinition;
    }

    public class FunctionGroupReference : Reference
    {
        public FunctionGroupReference(FunctionGroupDefinition funcs)
            : base(funcs)
        { }

        public new FunctionGroupDefinition Definition => base.Definition as FunctionGroupDefinition;
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


        public override string ToString()
            => $"({Condition}?{IfTrue}:{IfFalse})";

    }

    public class Assignment : Expression
    {
        public Expression LValue { get; }
        public Expression RValue { get; }

        public Assignment(Expression lvalue, Expression rvalue)
            : base(lvalue, rvalue)
            => (LValue, RValue) = (lvalue, rvalue);

        public override string ToString()
            => $"({LValue} = {RValue})";
    }

    public class Argument : Expression
    {
        public Expression Expression { get; }
        public int Position { get; }

        public Argument(Expression expression, int position)
            : base(expression)
        {
            Expression = expression;
            Position = position;
        }


        public override string ToString()
            => Expression.ToString();
    }

    public class Parenthesized : Expression
    {
        public Expression Expression { get; }

        public Parenthesized(Expression expression)
            : base(expression)
        {
            Expression = expression;
        }

        public override string ToString()
            => $"({Expression}";
    }

    public class Literal : Expression
    {
        public object Value { get; }
        public LiteralTypesEnum TypeEnum { get; }
        public Literal(LiteralTypesEnum typeEnum, object value)
            => (TypeEnum, Value) = (typeEnum, value);

        public override string ToString()
            => $"{Value}";
    }

    public class FunctionCall : Expression
    {
        public Expression Function { get; }
        public IReadOnlyList<Argument> Args { get; }
        public FunctionCall(Expression function, params Argument[] args)
            : base(args.Prepend(function).ToArray())
        {
            Function = function;
            if (Function == null)
                Debug.WriteLine("Unexpected empty function");
            Args = args;
        }

        public override string ToString()
            => $"{Function}({string.Join(", ", Args)})";
    }

    public class Lambda : Expression
    {
        public FunctionDefinition Function { get; }

        public Lambda(FunctionDefinition function)
        {
            Function = function;
        }

        public override IEnumerable<Symbol> GetChildSymbols()
            => new [] { Function };

        public override string ToString()
            => $"(\\({string.Join(", ", Function.Parameters)}) -> {Function.ReturnType}";
    }

    public class Tuple : Expression
    {
        public Tuple(params Expression[] children)
            : base(children)
        { }

        public override string ToString()
            => $"({string.Join(", ", Children)})";
    }

    public static class ExpressionExtensions
    {
        public static IEnumerable<Expression> GetExpressionTree(this Expression expr)
            => expr.GetSymbolTree().OfType<Expression>();
    }
}