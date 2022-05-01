using System.Collections.Generic;
using System.Linq;

namespace PlatoIR
{
    public class StatementIR : IR { }

    public class WhileStatementIR : StatementIR
    {
        public WhileStatementIR(ExpressionIR condition, StatementIR body)
            => (Condition, Body) = (condition, body);
        public StatementIR Body { get; set; }
        public ExpressionIR Condition { get; set; }
    }

    public class DoStatementIR : StatementIR
    {
        public DoStatementIR(ExpressionIR condition, StatementIR body)
            => (Condition, Body) = (condition, body);
        public StatementIR Body { get; set; }
        public ExpressionIR Condition { get; set; }
    }

    public class ExpressionStatementIR : StatementIR
    {
        public ExpressionStatementIR(ExpressionIR expression)
            => Expression = expression;
        public ExpressionIR Expression { get; }
    }

    public class IfStatementIR : StatementIR
    {
        public IfStatementIR(ExpressionIR condition, StatementIR onTrue, StatementIR onFalse)
            => (Condition, OnTrue, OnFalse) = (condition, onTrue, onFalse);
        public ExpressionIR Condition { get;  }
        public StatementIR OnTrue { get; }
        public StatementIR OnFalse { get; }
    }

    public class ReturnStatementIR : StatementIR
    {
        public ReturnStatementIR(ExpressionIR expression)
            => Expression = expression;
        public ExpressionIR Expression { get; }
    }

    public class MultiStatementIR : StatementIR
    {
        public MultiStatementIR(params StatementIR[] statements)
            => Statements = statements.ToList();
        public MultiStatementIR(IEnumerable<StatementIR> statements)
            : this(statements.ToArray()) { }
        public List<StatementIR> Statements { get; }
    }

    public class BlockStatementIR : StatementIR
    {
        public BlockStatementIR(params StatementIR[] statements)
            => Statements = statements.ToList();
        public BlockStatementIR(IEnumerable<StatementIR> statements)
            : this(statements.ToArray()) {}
        public List<StatementIR> Statements { get; }
    }

    public class DeclarationStatementIR : StatementIR
    {
        public DeclarationStatementIR(DeclarationIR declaration)
            => Declaration = declaration;
        public DeclarationIR Declaration { get; }
    }
}