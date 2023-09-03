using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Plato.Compiler.Ast;

namespace Plato.Compiler.Symbols
{
    public abstract class ExpressionSymbol : Symbol
    {
        public IReadOnlyList<ExpressionSymbol> Children { get; }

        protected ExpressionSymbol(params ExpressionSymbol[] children)
        {
            if (children.Contains(this))
                throw new Exception("Circular expression");
            Children = children;
        }

        public override IEnumerable<Symbol> GetChildSymbols()
            => Children;
    }

    public abstract class Reference : ExpressionSymbol
    {
        public DefinitionSymbol Definition { get; }
        public TypeExpressionSymbol Type => Definition.Type;
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

    public class ConditionalExpression : ExpressionSymbol
    {
        public ExpressionSymbol Condition { get; }
        public ExpressionSymbol IfTrue { get; }
        public ExpressionSymbol IfFalse { get; }

        public ConditionalExpression(ExpressionSymbol condition, ExpressionSymbol ifTrue, ExpressionSymbol ifFalse)
            : base(condition, ifTrue, ifFalse)
        {
            Condition = condition;
            IfTrue = ifTrue;
            IfFalse = ifFalse;
        }


        public override string ToString()
            => $"({Condition}?{IfTrue}:{IfFalse})";

    }

    public class Assignment : ExpressionSymbol
    {
        public ExpressionSymbol LValue { get; }
        public ExpressionSymbol RValue { get; }

        public Assignment(ExpressionSymbol lvalue, ExpressionSymbol rvalue)
            : base(lvalue, rvalue)
            => (LValue, RValue) = (lvalue, rvalue);

        public override string ToString()
            => $"({LValue} = {RValue})";
    }

    public class Argument : ExpressionSymbol
    {
        public ExpressionSymbol Expression { get; }
        public int Position { get; }

        public Argument(ExpressionSymbol expression, int position)
            : base(expression)
        {
            Expression = expression;
            Position = position;
        }


        public override string ToString()
            => Expression.ToString();
    }

    public class Parenthesized : ExpressionSymbol
    {
        public ExpressionSymbol Expression { get; }

        public Parenthesized(ExpressionSymbol expression)
            : base(expression)
        {
            Expression = expression;
        }

        public override string ToString()
            => $"({Expression}";
    }

    public class Literal : ExpressionSymbol
    {
        public object Value { get; }
        public LiteralTypesEnum TypeEnum { get; }
        public Literal(LiteralTypesEnum typeEnum, object value)
            => (TypeEnum, Value) = (typeEnum, value);

        public override string ToString()
            => $"{Value}";
    }

    public class FunctionCall : ExpressionSymbol
    {
        public ExpressionSymbol Function { get; }
        public IReadOnlyList<Argument> Args { get; }
        public FunctionCall(ExpressionSymbol function, params Argument[] args)
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

    public class Lambda : ExpressionSymbol
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

    public class Tuple : ExpressionSymbol
    {
        public Tuple(params ExpressionSymbol[] children)
            : base(children)
        { }

        public override string ToString()
            => $"({string.Join(", ", Children)})";
    }

    public static class ExpressionExtensions
    {
        public static IEnumerable<ExpressionSymbol> GetExpressionTree(this ExpressionSymbol expr)
        {
            if (expr == null)
                yield break;
            yield return expr;
            foreach (var c in expr.Children)
                foreach (var x in c.GetExpressionTree())
                    yield return x;
        }
    }
}