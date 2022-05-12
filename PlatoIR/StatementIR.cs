using System;
using System.Collections.Generic;
using System.Linq;

namespace PlatoIR
{
    public abstract class StatementIR : IR
    {
        public virtual IEnumerable<ExpressionIR> GetExpressions() 
            => Enumerable.Empty<ExpressionIR>();

        public IEnumerable<ExpressionIR> GetAllExpressions() 
            => GetExpressions().SelectMany(x => x?.GetAllExpressions() ?? Enumerable.Empty<ExpressionIR>());

        public IEnumerable<VariableDeclarationIR> GetAllExpressionDeclarations() 
            => GetAllExpressions().SelectMany(x => x?.GetDeclarations() ?? Enumerable.Empty<VariableDeclarationIR>());

        public virtual IEnumerable<StatementIR> GetStatements()
            => Enumerable.Empty<StatementIR>();

        public IEnumerable<StatementIR> GetAllStatements() 
            => GetStatements().SelectMany(st => st.GetAllStatements()).Append(this);
    }

    public class WhileStatementIR : StatementIR
    {
        public WhileStatementIR(ExpressionIR condition, StatementIR body)
            => (Condition, Body) = (condition, body);
        
        public StatementIR Body { get; set; }
        public ExpressionIR Condition { get; set; }

        public override IEnumerable<ExpressionIR> GetExpressions() 
            => Enumerable.Repeat(Condition, 1);

        public override IEnumerable<StatementIR> GetStatements()
            => Enumerable.Repeat(Body, 1);

        public override string ToString()
            => $"while ({Condition}) {Body}";
    }

    public class DoStatementIR : StatementIR
    {
        public DoStatementIR(ExpressionIR condition, StatementIR body)
            => (Condition, Body) = (condition, body);
        
        public StatementIR Body { get; set; }
        public ExpressionIR Condition { get; set; }

        public override IEnumerable<ExpressionIR> GetExpressions() 
            => Enumerable.Repeat(Condition, 1);

        public override IEnumerable<StatementIR> GetStatements()
            => Enumerable.Repeat(Body, 1);

        public override string ToString()
            => $"do {Body} while ({Condition})";
    }

    public class ExpressionStatementIR : StatementIR
    {
        public ExpressionStatementIR(ExpressionIR expression)
            => Expression = expression;
        
        public ExpressionIR Expression { get; }

        public override IEnumerable<ExpressionIR> GetExpressions() 
            => Enumerable.Repeat(Expression, 1);

        public override string ToString()
            => $"{Expression};";
    }

    public class IfStatementIR : StatementIR
    {
        public IfStatementIR(ExpressionIR condition, StatementIR onTrue, StatementIR onFalse)
            => (Condition, OnTrue, OnFalse) = (condition, onTrue, onFalse);
        public ExpressionIR Condition { get;  }
        public StatementIR OnTrue { get; }
        public StatementIR OnFalse { get; }

        public override IEnumerable<ExpressionIR> GetExpressions() 
            => Enumerable.Repeat(Condition, 1);

        public override IEnumerable<StatementIR> GetStatements()
            => new[] { OnTrue, OnFalse };

        public override string ToString()
            => $"if ({Condition}) {OnTrue} else {OnFalse}";
    }

    public class ReturnStatementIR : StatementIR
    {
        public ReturnStatementIR(ExpressionIR expression)
            => Expression = expression;
        
        public ExpressionIR Expression { get; }

        public override IEnumerable<ExpressionIR> GetExpressions() 
            => Enumerable.Repeat(Expression, 1);

        public override string ToString()
            => $"return {Expression};";
    }

    public class MultiStatementIR : StatementIR
    {
        public MultiStatementIR(params StatementIR[] statements)
            => Statements = statements.ToList();
        
        public MultiStatementIR(IEnumerable<StatementIR> statements)
            : this(statements.ToArray()) { }
        
        public List<StatementIR> Statements { get; set; }
        
        public override IEnumerable<StatementIR> GetStatements() 
            => Statements;

        public override string ToString()
            => string.Join("\n", Statements);
    }

    public class BlockStatementIR : StatementIR
    {
        public BlockStatementIR(params StatementIR[] statements)
            => Statements = statements.ToList();
        
        public BlockStatementIR(IEnumerable<StatementIR> statements)
            : this(statements.ToArray()) {}
        
        public List<StatementIR> Statements { get; set; }
        public override IEnumerable<StatementIR> GetStatements() => Statements;

        public override string ToString()
            => "{" + string.Join("\n", Statements) + "}";
    }

    public class DeclarationStatementIR : StatementIR
    {
        public DeclarationStatementIR(DeclarationIR declaration)
            => Declaration = declaration;

        public DeclarationIR Declaration { get; }

        public override IEnumerable<ExpressionIR> GetExpressions() => Declaration.Expressions;

        public override string ToString()
            => $"{Declaration};";
    }
}