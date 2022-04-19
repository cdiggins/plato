using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace PlatoGenerator
{
    /// <summary>
    /// A statement is either a function definition, a block,
    /// a variable declaration, a while statement, an if statement
    /// </summary>
    public class Statement
    {
        public SyntaxNode Syntax;
        public SemanticModel Model;
    }

    // Calling a known function involves replacing the parameters with arguments
    // What about captured variables? This means that the function has to be created. 
    // It is similar to calling a function that returns a function. 

    public class ExpressionStatement : Statement
    {
        public Expression Expression;
    }

    public class WhileStatement : Statement
    {
        public Statement Initialization;
        public Expression Condition;
        public Statement Body;
        public Statement Increment;
    }

    public class MultiStatement : Statement
    {
        public List<Statement> Statements = new List<Statement>();
    }

    public class BlockStatement : Statement
    {
        public List<Statement> Statements = new List<Statement>();
    }

    public class IfStatement : Statement
    {
        public Expression Condition;
        public Statement OnTrue;
        public Statement OnFalse;
    }

    public class ReturnStatement : Statement
    {
        public Expression Expression;
    }

    public class ThrowStatement : Statement
    {
        public Expression Expression;
    }

    public class EmptyStatement : Statement
    { }

    public class UnsupportedStatement : Statement
    {
    }

    public static class StatementExtensions
    {
        public static Statement CreateStatement(this StatementSyntax syntax, SemanticModel model)
        {
            Statement r = new UnsupportedStatement();

            switch (syntax)
            {
                case BlockSyntax blockSyntax:
                    r = blockSyntax.Statements.Select(x => x.CreateStatement(model)).ToBlockStatement(model);
                    break;

                case DoStatementSyntax doStatementSyntax:
                {
                    var body = CreateStatement(doStatementSyntax.Statement, model);
                    r = new WhileStatement
                    {
                        Initialization = body,
                        Body = body,
                        Condition = doStatementSyntax.Condition.CreateExpression(model),
                    };
                }
                    break;

                case ExpressionStatementSyntax expressionStatementSyntax:
                    r = CreateStatement(expressionStatementSyntax.Expression, model);
                    break;

                case ForStatementSyntax forStatementSyntax:
                    r = new WhileStatement
                    {
                        Body = CreateStatement(forStatementSyntax.Statement, model),
                        Condition = forStatementSyntax.Condition.CreateExpression(model),
                        Initialization = CreateStatement(forStatementSyntax.Initializers, model),
                        Increment = CreateStatement(forStatementSyntax.Incrementors, model),
                    };
                    break;

                case WhileStatementSyntax whileStatementSyntax:
                    r = new WhileStatement
                    {
                        Initialization = new EmptyStatement(),
                        Body = whileStatementSyntax.Statement.CreateStatement(model),
                        Condition = whileStatementSyntax.Condition.CreateExpression(model),
                        Increment = new EmptyStatement(),
                    };
                    break;


                case IfStatementSyntax ifStatementSyntax:
                    r = new IfStatement
                    {
                        Condition = ifStatementSyntax.Condition.CreateExpression(model),
                        OnTrue = CreateStatement(ifStatementSyntax.Statement, model),
                        OnFalse = ifStatementSyntax.Else == null
                            ? new EmptyStatement()
                            : CreateStatement(ifStatementSyntax.Else.Statement, model)
                    };
                    break;

                case ReturnStatementSyntax returnStatementSyntax:
                    r = returnStatementSyntax.Expression.CreateExpression(model).ToReturnStatement();
                    break;

                case LocalDeclarationStatementSyntax localDeclarationStatementSyntax:
                    r = localDeclarationStatementSyntax.CreateExpressions(model).ToMultiStatement(model);
                    break;

                case LocalFunctionStatementSyntax localFunctionStatementSyntax:
                    // TODO: decide whether this should really stay in Plato. 
                    // I do not find local functions to be very readable, but in the past they were a feature I wanted. 
                    r = FunctionDefinition.Create(localFunctionStatementSyntax, model).ToExpressionStatement();
                    break;

                case ThrowStatementSyntax throwStatementSyntax:
                    r = throwStatementSyntax.Expression.CreateExpression(model).ToThrowStatement();
                    break;

                case BreakStatementSyntax breakStatementSyntax:

                case CheckedStatementSyntax checkedStatementSyntax:

                case ForEachStatementSyntax forEachStatementSyntax:

                case ForEachVariableStatementSyntax forEachVariableStatementSyntax:

                case CommonForEachStatementSyntax commonForEachStatementSyntax:

                case ContinueStatementSyntax continueStatementSyntax:

                case FixedStatementSyntax fixedStatementSyntax:

                case GotoStatementSyntax gotoStatementSyntax:

                case LabeledStatementSyntax labeledStatementSyntax:

                case LockStatementSyntax lockStatementSyntax:

                case SwitchStatementSyntax switchStatementSyntax:

                case TryStatementSyntax tryStatementSyntax:

                case UnsafeStatementSyntax unsafeStatementSyntax:

                case UsingStatementSyntax usingStatementSyntax:

                case YieldStatementSyntax yieldStatementSyntax:
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
            r.Model = model;
            r.Syntax = syntax;
            return r;
        }

        public static Statement CreateStatement(this StatementSyntax statement, ExpressionSyntax expr,
            SemanticModel model)
        {
            if (statement != null) return statement.CreateStatement(model);
            return new ReturnStatement
            {
                Expression = expr.CreateExpression(model),
                Model = model,
                Syntax = expr,
            };
        }

        public static Statement CreateStatement(this IEnumerable<Statement> statements, SemanticModel model)
            => new BlockStatement { Statements = statements.ToList(), Model = model };

        public static Statement CreateStatement(this IEnumerable<ExpressionSyntax> syntax, SemanticModel model)
            => CreateStatement(syntax.Select(x => x.CreateStatement(model)), model);

        public static Statement CreateStatement(this ExpressionSyntax syntax, SemanticModel model)
            => syntax.CreateExpression(model).ToExpressionStatement();

        public static Statement ToExpressionStatement(this Expression expr)
            => new ExpressionStatement { Expression = expr, Syntax = expr.Syntax, Model = expr.Model };

        public static Statement ToReturnStatement(this Expression expr)
            => new ReturnStatement { Expression = expr, Syntax = expr.Syntax, Model = expr.Model };

        public static Statement ToThrowStatement(this Expression expr)
            => new ThrowStatement { Expression = expr, Syntax = expr.Syntax, Model = expr.Model };

        public static Statement ToMultiStatement(this IEnumerable<Expression> expr, SemanticModel model)
            => expr.Select(ToExpressionStatement).ToMultiStatement(model);

        public static Statement ToMultiStatement(this IEnumerable<Statement> statements, SemanticModel model)
            => new MultiStatement { Statements = statements.ToList(), Model = model};

        public static Statement ToBlockStatement(this IEnumerable<Statement> statements, SemanticModel model)
            => new BlockStatement { Statements = statements.ToList(), Model = model };
    }
}
