using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace PlatoGenerator
{
    /*
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
    }*/

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
    /// * Constant
    /// * Initalization 
    /// * Reassignment (conditional, or un-conditional, depending on context)
    /// * Known method invocation
    /// * Unknown method invovation
    /// * Built-in operator invovation
    /// * Name lookup (compile-time, or run-time)
    ///
    /// Passes:
    /// * Method inlining - is it a known function, then expand it. 
    /// * Evaluation - can the value be determined at compile-time (or at least a couple of possibilities) 
    /// * Elimination - is the value unused, and can be removed 
    /// * Coalescing - is the evaluation done multiple times?
    /// * Reordering - can reordering enable any of the other optimizations
    /// </summary>
    public class Expression
    {
        public static Dictionary<int, Expression> Lookup = new Dictionary<int, Expression>();

        public Expression()
        {
            Lookup.Add(Id, this);
        }

        public Expression(string name, ITypeSymbol type)
        {
            Name = name;
            Type = type;
        }

        public string Name = "unknown";
        public int Id = Lookup.Count;
        public SemanticModel Model;
        public SyntaxNode Syntax;
        public ISymbol Symbol;
        public ITypeSymbol Type;
        public SyntaxNode DeclarationSyntax;

        public PlatoMethodSyntax RelatedMethod => null; // Declaration as PlatoMethodSyntax;

        public object Value;
        public Expression This;
        public List<Expression> Arguments = new List<Expression>();
        public List<Expression> Initializer = new List<Expression>();

        public SyntaxKind SyntaxKind => Syntax.Kind();

        public IEnumerable<Expression> Children
        {
            get
            {
                if (This != null)
                    yield return This;
                foreach (var arg in Arguments)
                    yield return arg;
                foreach (var expr in Initializer)
                    yield return expr;
            }
        }
        public bool IsMethod => Symbol is IMethodSymbol;
    }

    public static class ExpressionExtensions
    {
        public static Expression AddArguments(this Expression self, IEnumerable<SyntaxNode> argList, SemanticModel model)
        {
            if (argList == null)
                return self;
            self.Arguments.AddRange(argList.Select(a => a.CreateExpression(model)));
            return self;
        }

        public static Expression AddInitializer(this Expression self, IEnumerable<SyntaxNode> initExprList, SemanticModel model)
        {
            if (initExprList == null)
                return self; 
            self.Initializer.AddRange(initExprList.Select(i => i.CreateExpression(model)));
            return self;
        }

        public static Expression CreateExpression(this SyntaxNode syntax, SemanticModel model)
        {
            if (syntax == null)
                return null;

            model = model.Compilation.GetSemanticModel(syntax.SyntaxTree);
            var symbol = model.GetSymbolInfo(syntax).Symbol;
            var type = model.GetTypeInfo(syntax).ConvertedType;
            var decl = symbol?.GetDeclaringSyntax();

            var r = new Expression
            {
                Name = "#unsupported",
                Syntax = syntax,
                Model = model,
                Symbol = symbol,
                Type = type,
                DeclarationSyntax = decl,
            };

            if (!(syntax is ExpressionSyntax))
            {
                if (syntax is ParameterSyntax parameterSyntax)
                {
                    if (r.Symbol != null) throw new Exception("Expected no symbol");
                    if (r.Type != null) throw new Exception("Expected no type");

                    var paramSymbol = (IParameterSymbol)model.GetDeclaredSymbol(syntax);
                    r.Symbol = paramSymbol;
                    r.Type = paramSymbol?.Type;
                    r.Name = parameterSyntax.Identifier.ToString();

                    if (r.Type == null) 
                        throw new Exception("Could not determine type");

                }
                else if (syntax is VariableDeclaratorSyntax variableSyntax)
                {
                    throw new NotImplementedException("TODO");
                }
                else if (syntax is ArgumentSyntax argumentSyntax)
                {
                    if (argumentSyntax.NameColon != null)
                        throw new NotImplementedException("Named arguments not supported yet");
                    return argumentSyntax.Expression.CreateExpression(model);
                }
                else
                {
                    throw new NotImplementedException($"Unsupported syntax {syntax}");
                }

                // TODO: handle implicit return value
                // TODO: support local variable declarations. 
                // TODO: handle this variable
            }
            else
            {
                switch (syntax)
                {
                    case IdentifierNameSyntax identifierNameSyntax:
                        r.Name = identifierNameSyntax.Identifier.ValueText;
                        break;

                    case SimpleNameSyntax simpleNameSyntax:
                        r.Name = simpleNameSyntax.Identifier.ValueText;
                        break;

                    case NameSyntax nameSyntax:
                        r.Name = nameSyntax.ToString();
                        break;

                    case ArrayCreationExpressionSyntax arrayCreation:
                        r.Name = "#array";
                        r.AddInitializer(arrayCreation.Initializer?.Expressions, model);
                        break;

                    case AssignmentExpressionSyntax assignment:
                        r.Name = "#assign" + assignment.OperatorToken;
                        r.Arguments.Add(assignment.Left.CreateExpression(model));
                        r.Arguments.Add(assignment.Right.CreateExpression(model));
                        break;

                    case ObjectCreationExpressionSyntax objectCreation:
                        r.Name = "#ctor";
                        r.AddArguments(objectCreation.ArgumentList?.Arguments, model);
                        break;

                    case BinaryExpressionSyntax binary:
                        r.Name = "#operator" + binary.OperatorToken;
                        r.Arguments.Add(CreateExpression(binary.Left, model));
                        r.Arguments.Add(CreateExpression(binary.Right, model));
                        break;

                    case CastExpressionSyntax castExpression:
                        r.Name = "#cast";
                        r.Arguments.Add(CreateExpression(castExpression.Expression, model));
                        break;

                    case CheckedExpressionSyntax checkedExpression:
                        break;

                    case ConditionalAccessExpressionSyntax conditionalAccess:
                        break;

                    // TODO: this should become a statement 
                    case ConditionalExpressionSyntax conditional:
                        r.Name = "#conditional";
                        r.Arguments.Add(CreateExpression(conditional.Condition, model));
                        r.Arguments.Add(CreateExpression(conditional.WhenTrue, model));
                        r.Arguments.Add(CreateExpression(conditional.WhenFalse, model));
                        break;

                    case DeclarationExpressionSyntax declaration:
                        break;

                    case DefaultExpressionSyntax defaultExpression:
                        r.Name = "#default";
                        break;

                    case ElementAccessExpressionSyntax elementAccess:
                        r.Name = "#at";
                        r.This = CreateExpression(elementAccess.Expression, model);
                        r.AddArguments(elementAccess.ArgumentList.Arguments, model);
                        break;

                    case ElementBindingExpressionSyntax elementBinding:
                        r.Name = "#elementbinding";
                        r.AddArguments(elementBinding.ArgumentList?.Arguments, model);
                        break;

                    case ImplicitArrayCreationExpressionSyntax implicitArrayCreation:
                        break;

                    case ImplicitElementAccessSyntax implicitElementAccess:
                        break;

                    case InitializerExpressionSyntax initializer:
                        break;

                    case InterpolatedStringExpressionSyntax interpolatedString:
                        break;

                    case InvocationExpressionSyntax invocation:
                        r.Name = "#invoke";
                        r.This = CreateExpression(invocation.Expression, model);
                        r.Arguments.AddRange(invocation.ArgumentList.Arguments.Select(x => CreateExpression(x, model)));
                        break;

                    case IsPatternExpressionSyntax isPattern:
                        break;

                    case LiteralExpressionSyntax literal:
                        r.Name = "#literal";
                        r.Value = literal.Token.Value;
                        break;

                    case MakeRefExpressionSyntax makeRef:
                        break;

                    case MemberAccessExpressionSyntax memberAccess:
                        r.Name = "#member:" + memberAccess.Name.Identifier.ToString();
                        r.This = CreateExpression(memberAccess.Expression, model);
                        break;

                    case MemberBindingExpressionSyntax memberBinding:
                        break;

                    case OmittedArraySizeExpressionSyntax omittedArraySize:
                        break;

                    case ParenthesizedLambdaExpressionSyntax parenthesizedlambda:
                        break;

                    case SimpleLambdaExpressionSyntax simpleLambda:
                        break;

                    case ParenthesizedExpressionSyntax parenthesized:
                        r.Arguments.Add(CreateExpression(parenthesized.Expression, model));
                        break;

                    case PostfixUnaryExpressionSyntax postfix:
                        r.Name = "#operatorpostfix" + postfix.OperatorToken.Text;
                        r.Arguments.Add(CreateExpression(postfix.Operand, model));
                        break;

                    case PrefixUnaryExpressionSyntax prefix:
                        r.Name = "#operatorprefix" + prefix.OperatorToken.Text;
                        r.Arguments.Add(CreateExpression(prefix.Operand, model));
                        break;

                    case RangeExpressionSyntax rangeExpression:
                        break;

                    case SwitchExpressionSyntax switchExpression:
                        break;

                    case ThisExpressionSyntax thisExpression:
                        r.Name = "#this";
                        break;

                    case ThrowExpressionSyntax throwExpression:
                        r.Name = "#throw";
                        r.Arguments.Add(CreateExpression(throwExpression.Expression, model));
                        break;

                    case TupleExpressionSyntax tuple:
                        r.Name = "#tuple";
                        r.Arguments.AddRange(tuple.Arguments.Select(a => CreateExpression(a.Expression, model)));
                        break;

                    case TypeOfExpressionSyntax typeOf:
                        r.Name = "#typeof";
                        r.Arguments.Add(CreateExpression(typeOf.Type, model));
                        break;

                    case TypeSyntax typeSyntax:
                        r.Name = "#type";
                        r.Value = typeSyntax.ToString();
                        break;

                    case WithExpressionSyntax withExpression:
                        break;

                    case AnonymousMethodExpressionSyntax anonymousMethodExpressionSyntax:
                        break;

                    case AnonymousObjectCreationExpressionSyntax anonymousObjectCreationExpressionSyntax:
                        break;

                    case AwaitExpressionSyntax awaitExpressionSyntax:
                        break;

                    case BaseExpressionSyntax baseExpressionSyntax:
                        r.Name = "#base";
                        break;

                    case ImplicitObjectCreationExpressionSyntax implicitObjectCreationExpressionSyntax:
                        r.Name = "#ctor";
                        r.AddArguments(implicitObjectCreationExpressionSyntax.ArgumentList.Arguments, model);
                        r.AddInitializer(implicitObjectCreationExpressionSyntax.Initializer?.Expressions, model);
                        break;

                    case ImplicitStackAllocArrayCreationExpressionSyntax implicitStackAllocArrayCreationExpressionSyntax:
                        break;

                    case SizeOfExpressionSyntax sizeOfExpressionSyntax:
                        break;

                    case StackAllocArrayCreationExpressionSyntax stackAllocArrayCreationExpressionSyntax:
                        break;

                    case BaseObjectCreationExpressionSyntax baseObjectCreationExpressionSyntax:
                        r.Name = "#basector";
                        r.AddArguments(baseObjectCreationExpressionSyntax.ArgumentList?.Arguments, model);
                        r.AddInitializer(baseObjectCreationExpressionSyntax.Initializer?.Expressions, model);
                        break;

                    case LambdaExpressionSyntax lambdaExpressionSyntax:
                        break;

                    case AnonymousFunctionExpressionSyntax anonymousFunctionExpressionSyntax:
                        break;

                    case InstanceExpressionSyntax instanceExpressionSyntax:
                        throw new NotImplementedException("Instance expression syntax should be derived from");

                    default:
                        throw new NotImplementedException($"Unrecognized syntax kind {syntax.Kind()}");
                }
            }

            if (r.Name == "#unsupported")
            {
                Debug.WriteLine($"Unsupported type {syntax.Kind()}");
            }

            return r;
        }
    }
}