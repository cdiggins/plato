using System.Collections.Generic;
using System;
using System.Linq;

namespace Plato.Compiler.Symbols
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
    }

    public class MultiStatement : Statement
    {
        public IReadOnlyList<Symbol> Symbols { get; }

        public MultiStatement(params Symbol[] symbols)
            => Symbols = symbols;

        public override IEnumerable<Symbol> GetChildSymbols()
            => Symbols;

        public override string Name => ",";
    }

    public class ReturnStatement : Statement
    {
        public Symbol Expression { get; }

        public ReturnStatement(Symbol expression)
            => Expression = expression;

        public override string Name 
            => "return";

        public override IEnumerable<Symbol> GetChildSymbols()
            => Expression == null ? Array.Empty<Symbol>() : new[] { Expression };
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

        public override IEnumerable<Symbol> GetChildSymbols()
            => new[] { Body, Condition };
    }

    public class CommentStatement : Statement
    {
        public string Comment { get; }

        public CommentStatement(string comment)
        {
            Comment = comment;
        }

        public override string Name => "comment";

        public override IEnumerable<Symbol> GetChildSymbols()
            => Enumerable.Empty<Symbol>();
    }
}