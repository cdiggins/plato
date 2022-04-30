using System.Collections.Generic;

namespace PlatoIR
{
    public class StatementIR : IR { }

    public class WhileStatementIR : StatementIR
    {
        public StatementIR Body { get; set; }
        public ExpressionIR Condition { get; set; }
    }

    public class AssignmentStatementIR : StatementIR
    {
        public AssignmentIR Assignment { get; set; }
    }

    public class ThrowStatement : StatementIR
    {
        public ThrowExpressionIR ThrowExpresion;
    }

    public class IfStatementIR : StatementIR
    {
        public ExpressionIR Condition { get; set; }
        public StatementIR OnTrue { get; set; }
        public StatementIR OnFalse { get; set; }
    }

    public class ReturnStatementIR : StatementIR
    {
        public ExpressionIR Expression { get; set; }
    }

    public class MultiStatementIR : StatementIR
    {
        public List<StatementIR> Statements { get; } = new List<StatementIR>();
    }
}