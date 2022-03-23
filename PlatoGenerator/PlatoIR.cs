using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace PlatoGenerator
{
    /// <summary>
    /// Every expression corresponds to the evaluation of some value
    /// and assigning it to an variable.
    ///
    /// This representation facilitates code generation, creating
    /// use-def chains, identifying unused variables, identifying
    /// coalescing opportunities, and performing partial evaluation.
    ///
    /// Perhaps surprisingly, an expression can have statements.
    /// This is because ever sub-expression is evaluated as an expression
    /// statement, and can contain an inlined method invocation.
    ///
    /// Main possibilities:
    /// 1. Constant
    /// 2. Simple reference to other expression
    /// 3. Known method invocation
    /// 4. Uknown method invovation
    /// 5. Built-in operator invovation
    ///
    /// Passes:
    /// * Method inlining - is it a known function, then expand it. 
    /// * Evaluation - can the value be determined at compile-time (or at least a couple of possibilities) 
    /// * Elimination - is the value unused, and can be removed 
    /// * Coalescing - is the evaluation done multiple times?
    /// * Reordering - can reordering enable any of the other optimizations
    /// </summary>
    public class ExpressionIR 
    {
        public static Dictionary<int, ExpressionIR> Lookup = new Dictionary<int, ExpressionIR>();

        public ExpressionIR()
            => Lookup.Add(Id = Lookup.Count, this);

        public int Id;
        public string Name;
        public List<object> ValueOptions = new List<object>();
        public bool HasValue => ValueOptions.Count == 1;
        public object Value => ValueOptions[0];
        public SyntaxNode Syntax;
        public SemanticModel Model;
        public PlatoExpression SourceExpression;
        public ITypeSymbol Type => SourceExpression?.Type;
        public SyntaxKind Kind => SourceExpression?.SyntaxKind ?? SyntaxKind.None;
        
        // A list of all references to this expression
        public List<ExpressionRefIR> Refs = new List<ExpressionRefIR>();
        
        public List<ExpressionRefIR> Args = new List<ExpressionRefIR>();
        public ExpressionRefIR This;

        public List<ExpressionIR> Children = new List<ExpressionIR>();
        public List<StatementIR> Statements = new List<StatementIR>();

        public ExpressionRefIR LastUse => Refs.Last();
        public bool IsUsed => Refs.Any();

        public bool IsParameter => Name == "#parameter";
        public bool IsThis => Name == "#this";
        public bool IsInvocation => Name == "#invocation";
        public bool IsKnownFunction => IsConstant && Value is FunctionDefinitionIR;
        public bool IsKnownFunctionInvocation => IsInvocation && This.Def.IsKnownFunction;
        public bool IsLexicalCapture => Name == "#closure";
        public bool IsConstant => Name == "#constant";

        public void CheckAssumptions()
        {
            if (IsLexicalCapture)
            {
                Assert(This.Def.IsKnownFunction);
            }

            if (IsConstant)
            {
                Assert(HasValue);
            }

            switch (Name)
            {
                case "#constant":
                case "#parameter":
                case "#invocation":
                case "#closure":
                case "#operator":
                case "":
                    break;
                default:
                    Assert(false);
                    break;
            }
        }

        public static void Assert(bool condition)
        {
            Debug.Assert(condition);
        }

        public ExpressionRefIR MakeRef()
        {
            var r = new ExpressionRefIR { Def = this };
            Refs.Add(r);
            return r;
        }

        public static ExpressionIR Create(ExpressionSyntax expr, SemanticModel model)
        {
            if (expr == null)
                return null;
            return Create(PlatoExpression.Create(expr, model));
        }

        public static ExpressionIR Create(PlatoExpression expr)
        {
            if (expr == null)
                return null;
            return new ExpressionIR()
            {
                SourceExpression = expr,
                Name = expr.Name,
                Syntax = expr.Syntax.Node,
                Model = expr.Model,
                Children = expr.Children.Select(Create).ToList(),
            };
        }

        public static ExpressionIR Create(ParameterSyntax syntax, SemanticModel model)
            => Create(PlatoParamSyntax.Create(syntax), model);

        public static ExpressionIR Create(PlatoParamSyntax expr, SemanticModel model)
        {
            if (expr == null)
                return null;
            return new ExpressionIR()
            {
                Name = expr.Name,
                Syntax = expr.Node,
                Model = model,
            };
        }

    }

    /// <summary>
    /// A reference to an expression. 
    /// </summary>
    public class ExpressionRefIR 
    {
        public ExpressionIR Def;
        public bool IsFinal => Def.Refs.Last() == this;
        public bool IsOnly => Def.Refs.Count == 1 && Def.Refs[0] == this;
    }

    /// <summary>
    /// A statement is either a function definition, a block,
    /// a variable declaration, a while statement, an if statement
    /// </summary>
    public class StatementIR
    {
        public SyntaxNode Syntax;
        public SemanticModel Model;

        public static StatementIR Create(PlatoStatementSyntax statement, PlatoExpressionSyntax expr,
            SemanticModel model)
        {
            if (statement == null && expr != null)
                return StatementIR.Create(expr.Node, model);

            if (statement != null)
                return StatementIR.Create(statement.Node, model);

            return null;
            //throw new Exception("Either Statement or expression must be null, but not both");
        }

        public static StatementIR Create(IEnumerable<StatementIR> statements, SemanticModel model)
            => new BlockStatementIR() { Statements = statements.ToList(), Model = model };

        public static StatementIR Create(IEnumerable<ExpressionSyntax> syntax, SemanticModel model)
            => Create(syntax.Select(x => StatementIR.Create(x, model)), model);

        public static StatementIR Create(ExpressionSyntax syntax, SemanticModel model)
            => new ExpressionStatementIR()
                { Expression = ExpressionIR.Create(syntax, model), Syntax = syntax, Model = model };

        public static StatementIR Create(StatementSyntax syntax, SemanticModel model)
        {
            StatementIR r = new UnsupportedStatementIR();

            switch (syntax)
            {
                case BlockSyntax blockSyntax:
                    r = new BlockStatementIR()
                    {
                        Statements = blockSyntax.Statements.Select(x => Create(x, model)).ToList(),
                    };
                    break;

                case DoStatementSyntax doStatementSyntax:
                {
                    var body = Create(doStatementSyntax.Statement, model);
                    r = new WhileStatementIR()
                    {
                        Initialization = body,
                        Body = body,
                        Condition = ExpressionIR.Create(doStatementSyntax.Condition, model),
                    };
                }
                    break;

                case ExpressionStatementSyntax expressionStatementSyntax:
                    r = Create(expressionStatementSyntax.Expression, model);
                    break;

                case ForStatementSyntax forStatementSyntax:
                    r = new WhileStatementIR()
                    {
                        Body = StatementIR.Create(forStatementSyntax.Statement, model),
                        Condition = ExpressionIR.Create(forStatementSyntax.Condition, model),
                        Initialization = StatementIR.Create(forStatementSyntax.Initializers, model),
                        Increment = StatementIR.Create(forStatementSyntax.Incrementors, model),
                    };
                    break;

                case WhileStatementSyntax whileStatementSyntax:
                    r = new WhileStatementIR()
                    {
                        Body = StatementIR.Create(whileStatementSyntax.Statement, model),
                        Condition = ExpressionIR.Create(whileStatementSyntax.Condition, model),
                    };
                    break;


                case IfStatementSyntax ifStatementSyntax:
                    r = new IfStatementIR()
                    {
                        Condition = ExpressionIR.Create(ifStatementSyntax.Condition, model),
                        OnTrue = Create(ifStatementSyntax.Statement, model),
                        OnFalse = ifStatementSyntax.Else == null
                            ? null
                            : Create(ifStatementSyntax.Else.Statement, model)
                    };
                    break;

                case ReturnStatementSyntax returnStatementSyntax:
                    r = new ReturnStatementIR()
                    {
                        Expression = ExpressionIR.Create(returnStatementSyntax.Expression, model),
                    };
                    break;

                case LocalDeclarationStatementSyntax localDeclarationStatementSyntax:
                    // TODO: 
                    break;

                case LocalFunctionStatementSyntax localFunctionStatementSyntax:
                    //TODO:
                    break;

                case ThrowStatementSyntax throwStatementSyntax:
                    r = new ThrowStatementIR() { Expression = ExpressionIR.Create(throwStatementSyntax.Expression, model) };
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
    }

    // Calling a known function involves replacing the parameters with arguments
    // What about captured variables? This means that the function has to be created. 
    // It is similar to calling a function that returns a function. 
    public class FunctionDefinitionIR : StatementIR
    {
        public string Name;
        public PlatoMethodSyntax SourceMethod;
        public ExpressionIR This;
        public bool IsStatic => This == null;
        public List<ExpressionIR> Parameters = new List<ExpressionIR>();
        public ExpressionIR Result; 
        public List<ExpressionIR> Captured = new List<ExpressionIR>();
        public bool IsClosure => Captured.Count > 0;
        public StatementIR Body;

        public static FunctionDefinitionIR Create(PlatoMethodSyntax method, SemanticModel model)
        {
            var r = new FunctionDefinitionIR()
            {
                Name = method.Name,
                Body = StatementIR.Create(method.StatementBody, method.ExpressionBody, model),
                SourceMethod = method,
                Syntax = method.Node,
                Model = model,
                Parameters = method.Parameters.Select(p => ExpressionIR.Create(p, model)).ToList()
            };

            // TODO: how do I define the expression IR for this 

            return r;
        }
    }

    /// <summary>
    /// Defines a variable
    /// </summary>
    public class ExpressionStatementIR : StatementIR
    {
        public ExpressionIR Expression;
    }

    public class WhileStatementIR : StatementIR
    {
        public StatementIR Initialization;
        public ExpressionIR Condition;
        public StatementIR Body;
        public StatementIR Increment;
    }

    public class BlockStatementIR : StatementIR
    {
        public List<StatementIR> Statements = new List<StatementIR>();
    }

    public class IfStatementIR : StatementIR
    {
        public ExpressionIR Condition;
        public StatementIR OnTrue;
        public StatementIR OnFalse;
    }

    public class ReturnStatementIR : StatementIR
    {
        public ExpressionIR Expression;
    }

    public class ThrowStatementIR : StatementIR
    {
        public ExpressionIR Expression;
    }

    public class UnsupportedStatementIR : StatementIR
    {
    }
}
