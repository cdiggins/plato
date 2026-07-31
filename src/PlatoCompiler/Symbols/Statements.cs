using System.Collections.Generic;
using System;
using System.Linq;

namespace Ara3D.Geometry.Compiler.Symbols
{
    public abstract class Statement : Symbol
    { }

    public class ExpressionStatement : Statement
    {
        public Expression Expression { get; }

        public ExpressionStatement(Expression expr)
            => Expression = expr;

        public override string Name => "expression";

        public override IEnumerable<Symbol> GetChildSymbols()
            => new[] { Expression };

        public override string ToString()
            => $"{Expression};";

        public override Symbol Rewrite(Func<Symbol, Symbol> f)
            => f(new ExpressionStatement(Expression?.Rewrite(f) as Expression));
    }

    public class BlockStatement : Statement
    {
        public IReadOnlyList<Symbol> Symbols { get; }

        public override string Name
            => "{}";

        public BlockStatement(params Symbol[] symbols)
            => Symbols = symbols;

        public override IEnumerable<Symbol> GetChildSymbols()
            => Symbols;

        public override string ToString()
            => $"{{ {string.Join("\n", Symbols)} }}";

        public override Symbol Rewrite(Func<Symbol, Symbol> f)
            => f(new BlockStatement(Symbols.Select(s => s.Rewrite(f)).ToArray()));
    }

    public class MultiStatement : Statement
    {
        public IReadOnlyList<Symbol> Symbols { get; }

        public MultiStatement(params Symbol[] symbols)
            => Symbols = symbols;

        public override IEnumerable<Symbol> GetChildSymbols()
            => Symbols;

        public override string Name => ",";

        public override string ToString()
            => $"{string.Join("\n", Symbols)}";

        public override Symbol Rewrite(Func<Symbol, Symbol> f)
            => f(new MultiStatement(Symbols.Select(s => s.Rewrite(f)).ToArray()));
    }

    public class ReturnStatement : Statement
    {
        public Symbol Expression { get; }

        public ReturnStatement(Symbol expression)
            => Expression = expression;

        public override string Name 
            => "return";

        public override string ToString()
            => $"return {Expression};";

        public override IEnumerable<Symbol> GetChildSymbols()
            => Expression == null ? Array.Empty<Symbol>() : new[] { Expression };

        public override Symbol Rewrite(Func<Symbol, Symbol> f)
            => f(new ReturnStatement(Expression?.Rewrite(f)));
    }

    public class LoopStatement : Statement
    {   
        public Symbol Condition { get; }
        public Symbol Body { get; }

        public override string Name
            => "while";

        public LoopStatement(Symbol condition, Symbol body)
        {
            Condition = condition;
            Body = body;
        }

        public override string ToString()
            => $"while ({Condition}) {Body}";

        public override IEnumerable<Symbol> GetChildSymbols()
            => new[] { Body, Condition };

        public override Symbol Rewrite(Func<Symbol, Symbol> f)
            => f(new LoopStatement(Condition?.Rewrite(f), Body?.Rewrite(f)));
    }

    public class IfStatement : Statement
    {
        public Symbol Condition { get; }
        public Symbol IfTrue { get; }
        public Symbol IfFalse { get; }

        public override string Name => "if";

        public IfStatement(Symbol condition, Symbol ifTrue, Symbol ifFalse)
        {
            Condition = condition;
            IfTrue = ifTrue;
            IfFalse = ifFalse;
        }

        public override string ToString()
            => $"if ({Condition}) {IfTrue} else {IfFalse}";

        public override IEnumerable<Symbol> GetChildSymbols()
            => new[] { Condition, IfTrue, IfFalse };

        public override Symbol Rewrite(Func<Symbol, Symbol> f)
            => f(new IfStatement(Condition?.Rewrite(f), IfTrue?.Rewrite(f), IfFalse?.Rewrite(f)));

    }

    public class CommentStatement : Statement
    {
        public string Comment { get; }

        public CommentStatement(string comment)
            => Comment = comment;

        public override string Name => "comment";

        public override string ToString()
            => $"/* {Comment} */";

        public override IEnumerable<Symbol> GetChildSymbols()
            => Enumerable.Empty<Symbol>();

        public override Symbol Rewrite(Func<Symbol, Symbol> f)
            => f(this);
    }
}