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
    public class FunctionDefinition : Statement
    {
        public static Dictionary<int, FunctionDefinition> Lookup = new Dictionary<int, FunctionDefinition>();
        public int Id = Lookup.Count;

        public FunctionDefinition()
        {
            Lookup.Add(Id, this);
        }

        public string Name;
        public PlatoMethodSyntax SourceMethod;
        public IMethodSymbol Symbol;
        public Expression This;
        public bool IsStatic => This == null;
        public List<Expression> Parameters = new List<Expression>();
        public Expression Result; 
        //public List<Expression> Captured = new List<Expression>();
        //public bool IsClosure => Captured.Count > 0;
        public Statement Body;

        public static FunctionDefinition Create(PlatoMethodSyntax method, SemanticModel model)
        {
            var symbol = model.GetDeclaredSymbol(method.Node) as IMethodSymbol;
            var result = new Expression("#result", symbol?.ReturnType);
            var @this = symbol?.ReceiverType == null ? null : new Expression("#this", symbol?.ReceiverType);

            var r = new FunctionDefinition
            {
                Name = method.Name,
                
                Body = method.StatementBody.CreateStatement(method.ExpressionBody, model),
                SourceMethod = method,
                Syntax = method.Node,
                Symbol = symbol,
                Model = model,
                Result = result,
                This = @this,
                Parameters = method.Parameters.Select(p => p.Node.CreateExpression(model)).ToList()
            };

            return r;
        }
    }

    /// <summary>
    /// Defines a variable
    /// </summary>
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
                    r = new BlockStatement
                    {
                        Statements = blockSyntax.Statements.Select(x => x.CreateStatement(model)).ToList(),
                    };
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
                        Body = whileStatementSyntax.Statement.CreateStatement(model),
                        Condition = whileStatementSyntax.Condition.CreateExpression(model),
                    };
                    break;


                case IfStatementSyntax ifStatementSyntax:
                    r = new IfStatement
                    {
                        Condition = ifStatementSyntax.Condition.CreateExpression(model),
                        OnTrue = CreateStatement(ifStatementSyntax.Statement, model),
                        OnFalse = ifStatementSyntax.Else == null
                            ? null
                            : CreateStatement(ifStatementSyntax.Else.Statement, model)
                    };
                    break;

                case ReturnStatementSyntax returnStatementSyntax:
                    r = new ReturnStatement
                    {
                        Expression = returnStatementSyntax.Expression.CreateExpression(model),
                    };
                    break;

                case LocalDeclarationStatementSyntax localDeclarationStatementSyntax:
                    // TODO: 
                    break;

                case LocalFunctionStatementSyntax localFunctionStatementSyntax:
                    //TODO:
                    break;

                case ThrowStatementSyntax throwStatementSyntax:
                    r = new ThrowStatement { Expression = throwStatementSyntax.Expression.CreateExpression(model) };
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

        public static Statement CreateStatement(this PlatoStatementSyntax statement, PlatoExpressionSyntax expr,
            SemanticModel model) 
            => statement == null && expr != null
                ? CreateStatement(expr.Node, model)
                : statement != null
                    ? CreateStatement(statement.Node, model)
                    : null;

        public static Statement CreateStatement(this IEnumerable<Statement> statements, SemanticModel model)
            => new BlockStatement { Statements = statements.ToList(), Model = model };

        public static Statement CreateStatement(this IEnumerable<ExpressionSyntax> syntax, SemanticModel model)
            => CreateStatement(syntax.Select(x => x.CreateStatement(model)), model);

        public static Statement CreateStatement(this ExpressionSyntax syntax, SemanticModel model)
            => new ExpressionStatement { Expression = syntax.CreateExpression(model), Syntax = syntax, Model = model };
    }
}
