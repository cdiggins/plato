using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

        public string Name = "#unknown";
        public int Id = Lookup.Count;
        public SemanticModel Model;
        public SyntaxNode Syntax;
        public ISymbol Symbol;
        public ITypeSymbol UnconvertedType;
        public ITypeSymbol Type;
        public SyntaxNode DeclarationSyntax;
        public IOperation Operation;
        public Conversion Conversion;

        public object Value;
        public Expression This;
        public List<Expression> Arguments = new List<Expression>();
        public List<Expression> Initializer = new List<Expression>();

        public SyntaxKind SyntaxKind => Syntax.Kind();

        public string TypeString => Type?.ToString() ?? (Symbol is IMethodSymbol ms ? "METHOD_" + ms.Name : "UNTYPED");

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

        public ITypeSymbol GetExpressionType()  => Model.GetTypeInfo(Syntax).Type;
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

        public static IEnumerable<Expression> CreateExpressions(this LocalDeclarationStatementSyntax syntax, SemanticModel model)
            => syntax.Declaration.Variables.Select(v => v.CreateExpression(model));

        public static Expression CreateExpression(this SyntaxNode syntax, SemanticModel model)
        {
            if (syntax == null)
                return null;

            model = model.Compilation.GetSemanticModel(syntax.SyntaxTree);
            var symbol = model.GetSymbolInfo(syntax).Symbol;
            var typeInfo = model.GetTypeInfo(syntax);
            var unconvertedType = typeInfo.Type;
            var convertedType = typeInfo.ConvertedType;
            var decl = symbol?.GetDeclaringSyntax();
            var op = model.GetOperation(syntax);

            var conversion = unconvertedType != null && convertedType != null 
                ? model.Compilation.ClassifyConversion(unconvertedType, convertedType)
                : new Conversion();

            var r = new Expression
            {
                Syntax = syntax,
                Model = model,
                Symbol = symbol,
                Type = convertedType,
                UnconvertedType = unconvertedType,
                DeclarationSyntax = decl,
                Operation = op,
                Conversion = conversion,
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
                    // TODO: perhaps the declared type is useful 
                    // var declaration = variableSyntax.Parent as VariableDeclarationSyntax;

                    if (r.Symbol != null) throw new Exception("Expected no symbol");
                    
                    r.Symbol = model.GetDeclaredSymbol(syntax);

                    if (r.Type == null)
                    {
                        // TODO: I suspect it might be from the initializer, if not it is the declared type. 
                        // Really it could have both. 
                        Debug.WriteLine("TODO: figure out the variable type");
                    }

                    r.Name = variableSyntax.Identifier.ToString();
                    r.AddArguments(variableSyntax.ArgumentList?.Arguments, model);

                    if (variableSyntax.Initializer != null)
                    {
                        if (variableSyntax.ArgumentList?.Arguments.Count > 0)
                        {
                            // TODO: I need to have different kinds of exceptions.
                            throw new Exception("If initializer is not null, then there are no expected arguments ");
                        }

                        // TODO: this is the first assignment, it might make more sense to move this out. 
                        // TODO: this might also be the actual type of the variable 
                        r.Arguments.Add(variableSyntax.Initializer.Value.CreateExpression(model));
                    }
                    // r.AddInitializer(variableSyntax.Initializer?.E)

                }
                else if (syntax is ArgumentSyntax argumentSyntax)
                {
                    if (argumentSyntax.NameColon != null)
                        throw new NotImplementedException("Named arguments not supported yet");
                    return argumentSyntax.Expression.CreateExpression(model);
                }
                else if (syntax is ConstantPatternSyntax constantPatternSyntax)
                {
                    return CreateExpression(constantPatternSyntax.Expression, model);
                }
                else if (syntax is DiscardPatternSyntax discardPatternSyntax)
                {
                    r.Name = "#discard";
                    return r;
                }
                else
                {
                    throw new Exception($"Unhandled non-expression syntax {syntax.Kind()}");
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
                        r.Name = "#operator?";
                        r.Arguments.Add(CreateExpression(conditional.Condition, model));
                        r.Arguments.Add(CreateExpression(conditional.WhenTrue, model));
                        r.Arguments.Add(CreateExpression(conditional.WhenFalse, model));
                        break;

                    case DeclarationExpressionSyntax declaration:
                        r.Name = "#declaration";
                        // TODO: no idea what to do.
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
                        r.Name = "#array";
                        r.AddArguments(implicitArrayCreation.Initializer.Expressions, model);
                        break;

                    case ImplicitElementAccessSyntax implicitElementAccess:
                        break;

                    case InitializerExpressionSyntax initializer:
                        break;

                    case InterpolatedStringExpressionSyntax interpolatedString:
                        r.Name = "#interpolatedstring";
                        //throw new NotImplementedException("TODO");
                        //r.Arguments.AddRange(interpolatedString.)
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
                        if (r.Type == null)
                        {
                            Debug.WriteLine("No type can be determined for the member");
                        }
                        r.Name = "#member:" + memberAccess.Name.Identifier;
                        r.This = CreateExpression(memberAccess.Expression, model);
                        break;

                    case MemberBindingExpressionSyntax memberBinding:
                        break;

                    case OmittedArraySizeExpressionSyntax omittedArraySize:
                        break;

                    case ParenthesizedLambdaExpressionSyntax parenthesizedlambda:
                        r = FunctionDefinition.Create(parenthesizedlambda, model);
                        break;

                    case SimpleLambdaExpressionSyntax simpleLambda:
                        r = FunctionDefinition.Create(simpleLambda, model);
                        break;

                    case LambdaExpressionSyntax lambdaExpressionSyntax:
                        r = FunctionDefinition.Create(lambdaExpressionSyntax, model);
                        break;

                    case AnonymousFunctionExpressionSyntax anonymousFunctionExpressionSyntax:
                        throw new NotSupportedException("Anonymous function expressions (delegates) are not supported");

                    case ParenthesizedExpressionSyntax parenthesized:
                        r.Name = "#parantheized";
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
                        r.Name = "#switch";
                        r.Arguments.Add(CreateExpression(switchExpression.GoverningExpression, model));
                        foreach (var arm in switchExpression.Arms) {
                            r.Arguments.Add(CreateExpression(arm.Expression, model));
                            r.Arguments.Add(CreateExpression(arm.Pattern, model));
                        };
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
                        r.Name = "#with";
                        // TODO: no clue what to do next.
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

                    case InstanceExpressionSyntax instanceExpressionSyntax:
                        throw new NotImplementedException("Instance expression syntax should be derived from");

                    default:
                        throw new NotImplementedException($"Unrecognized syntax kind {syntax.Kind()}");
                }
            }

            if (r.Name == "#unknown")
            {
                throw new Exception($"Unsupported expression type {syntax.Kind()}");
            }

            return r;
        }
    }
}