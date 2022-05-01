using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PlatoIR;
using PlatoRoslynSyntaxAnalyzer;

namespace PlatoRoslynSyntaxAnalyzer
{
    public class SemanticRules
    {
        public static void UnsupportedSymbol(ISymbol symbol)
            => throw new Exception($"Could not find symbol {symbol}");

        public static void CheckResolution(ISymbol symbol, IR ir)
        {
            if (ir == null)
                throw new Exception($"could not resolve IR for {symbol}");
        }

        public static void ResolutionFailed(SyntaxNode node)
            => throw new Exception($"no symbol found for {node}");

        public static void UnsupportedSyntax(SyntaxNode node)
            => throw new Exception($"unsupported syntax {node}");

        public static void NotAValidLValue(SyntaxNode syntax)
            => throw new Exception($"cannot assign to {syntax}");

        public static void OnlyOneSubscriptSupported(SyntaxNode syntax)
            => throw new Exception($"only one subscript is allowed {syntax}");

        public static void NotSupportedLiteral(LiteralExpressionSyntax literal)
            => throw new Exception($"literal is not supported {literal}");

        public static void OnlyDotNotationSupported(SyntaxNode syntax)
            => throw new Exception($"only dot notation supported {syntax}");
    }

    public static class SyntaxToIR
    {
        public static IRBuilder BuildIR(this IRBuilder builder, IEnumerable<PlatoTypeSyntax> syntaxes)
            => CreateDefinitions(CreateDeclarations(builder, syntaxes), syntaxes);

        public static IRBuilder CreateDeclarations(this IRBuilder builder, IEnumerable<PlatoTypeSyntax> syntaxes)
        {
            // 
            return builder;
        }

        public static IRBuilder CreateDefinitions(this IRBuilder builder, IEnumerable<PlatoTypeSyntax> syntaxes)
        {
            // 
            return builder;
        }

        public static IR ToIR(this PlatoSyntax syntax, Compilation compilation, IRBuilder builder)
        {
            switch (syntax)
            {
                case PlatoArgSyntax platoArgSyntax:
                    break;
                case PlatoConstructorSyntax platoConstructorSyntax:
                    break;
                case PlatoConversionSyntax platoConversionSyntax:
                    break;
                case PlatoExpressionSyntax platoExpressionSyntax:
                    break;
                case PlatoFieldSyntax platoFieldSyntax:
                    break;
                case PlatoIndexerSyntax platoIndexerSyntax:
                    break;
                case PlatoMethodSyntax platoMethodSyntax:
                    break;
                case PlatoOperatorSyntax platoOperatorSyntax:
                    break;
                case PlatoParamSyntax platoParamSyntax:
                    break;
                case PlatoPropertySyntax platoPropertySyntax:
                    break;
                case PlatoStatementSyntax platoStatementSyntax:
                    break;
                case PlatoAccessorSyntax platoAccessorSyntax:
                    break;
                case PlatoTypeRefSyntax platoTypeRefSyntax:
                    break;
                case PlatoTypeSyntax platoTypeSyntax:
                    break;
                case PlatoVariableSyntax platoVariableSyntax:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(syntax));
            }
            throw new NotImplementedException();
        }

        public static StatementIR ToIR(this PlatoStatementSyntax syntax, Compilation compilation, IRBuilder builder)
        {
            throw new NotImplementedException();
        }

        public static object ToValue(this LiteralExpressionSyntax literal)
        {
            switch (literal.Kind())
            {
                case SyntaxKind.CharacterLiteralExpression:
                    return char.Parse(literal.ToString());
                case SyntaxKind.StringLiteralExpression:
                    return literal.ToString().Substring(1, literal.ToString().Length - 2);
                case SyntaxKind.TrueLiteralExpression:
                    return true;
                case SyntaxKind.FalseLiteralExpression:
                    return false;
                case SyntaxKind.NullLiteralExpression:
                    return null;
                case SyntaxKind.NumericLiteralExpression:
                    return double.Parse(literal.ToString());
                default:
                    SemanticRules.NotSupportedLiteral(literal);
                    return null;
            }
        }

        public static ExpressionIR ToIR(this ExpressionSyntax syntax, Compilation compilation, IRBuilder builder)
            => syntax.ToIR(compilation.GetSemanticModel(syntax.SyntaxTree), builder);

        public static ExpressionIR AddArguments(this ExpressionIR ir, IEnumerable<ArgumentSyntax> arguments,
            SemanticModel model, IRBuilder builder)
        {
            if (arguments != null)
                ir.Args.AddRange(arguments.Select(x => x.ToIR(model, builder)));
            return ir;
        }

        public static ExpressionIR AddArguments(this ExpressionIR ir, IEnumerable<ExpressionSyntax> expressions,
            SemanticModel model, IRBuilder builder)
        {
            if (expressions != null)
                ir.Args.AddRange(expressions.Select(x => x.ToIR(model, builder)));
            return ir;
        }

        public static TypeReferenceIR GetTypeReference(this TypeSyntax syntax, SemanticModel model, IRBuilder builder)
            => model.GetSymbolInfo(syntax).Symbol.GetTypeReference(builder);

        public static TypeReferenceIR GetTypeReference(this ISymbol symbol, IRBuilder builder)
            => builder.GetDeclarationIR<TypeIR>(symbol)?.ToReference();

        public static FunctionReferenceIR GetFunctionReference(this ExpressionSyntax syntax, SemanticModel model, IRBuilder builder)
            => model.GetSymbolInfo(syntax).Symbol.GetFunctionReference(builder);

        public static FunctionReferenceIR GetFunctionReference(this ISymbol symbol, IRBuilder builder)
            => builder.GetDeclarationIR<FunctionIR>(symbol)?.ToReference();

        public static FunctionReferenceIR ToReference(this FunctionIR function)
            => function != null ? new FunctionReferenceIR() { Function = function } : null;

        public static TypeReferenceIR ToReference(this TypeIR type)
            => type != null ? new TypeReferenceIR() { ReferencedType = type } : null;

        public static ReferenceIR GetLValue(this ExpressionSyntax syntax, SemanticModel model, IRBuilder builder)
        {
            var r = syntax.ToIR(model, builder);
            if (!(r is VariableReferenceIR) && !(r is ParameterReferenceIR))
                SemanticRules.NotAValidLValue(syntax);
            return (ReferenceIR)r;
        }

        public static ArgumentIR ToIR(this ArgumentSyntax syntax, SemanticModel model, IRBuilder builder)
         => new ArgumentIR()
            {
                Name = syntax.NameColon?.Name.ToString(),
                Value = syntax.Expression.ToIR(model, builder)
            };

        public static ExpressionIR ToIR(this ExpressionSyntax syntax, SemanticModel model, IRBuilder builder)
        {
            var type = ModelExtensions.GetTypeInfo(model, syntax);
            var originalType = type.Type;
            var finalType = type.ConvertedType;
            var originalTypeIR = builder.GetDeclarationIR<TypeIR>(originalType);
            var finalTypeIR = builder.GetDeclarationIR<TypeIR>(finalType);
            
            var conversion = originalType != null && finalType != null
                ? model.Compilation.ClassifyConversion(originalType, finalType)
                : new Conversion();

            var conversionFunction = conversion.MethodSymbol.GetFunctionReference(builder);

            // TODO: make conversion calls.
            // TODO: indexers 
            // TODO: store local declarations 

            ExpressionIR r = null;

            switch (syntax)
            {
                case IdentifierNameSyntax identifierNameSyntax:
                    r = new NameIR() { Name = identifierNameSyntax.Identifier.ToString() };
                    break;

                case SimpleNameSyntax simpleNameSyntax:
                    r = new NameIR() { Name = simpleNameSyntax.Identifier.ToString() };
                    break;

                case NameSyntax nameSyntax:
                    SemanticRules.UnsupportedSyntax(nameSyntax);
                    break;

                case ArrayCreationExpressionSyntax arrayCreation:
                    r = new ArrayIR();
                    r.AddArguments(arrayCreation.Initializer?.Expressions, model, builder);
                    break;

                case AssignmentExpressionSyntax assignment:
                    r = new AssignmentIR() { LValue = GetLValue(assignment.Left, model, builder), };
                    r.Args.Add(assignment.Right.ToIR(model, builder));
                    break;

                case ObjectCreationExpressionSyntax objectCreation:
                    r = new NewIR() { CreatedType = objectCreation.Type.GetTypeReference(model, builder) };
                    if (objectCreation.Initializer != null)
                        SemanticRules.UnsupportedSyntax(objectCreation.Initializer);
                    r.AddArguments(objectCreation.ArgumentList?.Arguments, model, builder);
                    break;

                case BinaryExpressionSyntax binary:
                    r = new BinaryOperationIR() { Operator = binary.OperatorToken.ToString() };
                    r.Args.Add(binary.Left.ToIR(model, builder));
                    r.Args.Add(binary.Right.ToIR(model, builder));
                    break;

                case CastExpressionSyntax castExpression:
                    r = new CastIR() { CastType = GetTypeReference(castExpression.Type, model, builder) };
                    r.Args.Add(castExpression.Expression.ToIR(model, builder));
                    break;

                case CheckedExpressionSyntax checkedExpression:
                    SemanticRules.UnsupportedSyntax(checkedExpression);
                    break;

                case ConditionalAccessExpressionSyntax conditionalAccess:
                    SemanticRules.UnsupportedSyntax(conditionalAccess);
                    break;

                case ConditionalExpressionSyntax conditional:
                    r = new ConditionalIR(); 
                    r.Args.Add(conditional.Condition.ToIR(model, builder));
                    r.Args.Add(conditional.WhenTrue.ToIR(model, builder));
                    r.Args.Add(conditional.WhenFalse.ToIR(model, builder));
                    break;

                case DeclarationExpressionSyntax declaration:
                    SemanticRules.UnsupportedSyntax(declaration);
                    break;

                case DefaultExpressionSyntax defaultExpression:
                    r = new DefaultIR() { DefaultType = defaultExpression.Type.GetTypeReference(model, builder) };
                    break;

                case ElementAccessExpressionSyntax elementAccess:
                    r = new SubscriptIR();
                    r.Args.Add(elementAccess.Expression.ToIR(model, builder));
                    if (elementAccess.ArgumentList.Arguments.Count != 1)
                        SemanticRules.OnlyOneSubscriptSupported(elementAccess);
                    r.AddArguments(elementAccess.ArgumentList.Arguments, model, builder);
                    break;

                case ElementBindingExpressionSyntax elementBinding:
                    SemanticRules.UnsupportedSyntax(elementBinding);
                    break;

                case ImplicitArrayCreationExpressionSyntax implicitArrayCreation:
                    r = new ArrayIR();
                    r.AddArguments(implicitArrayCreation.Initializer.Expressions, model, builder);
                    break;

                case ImplicitElementAccessSyntax implicitElementAccess:
                    SemanticRules.UnsupportedSyntax(implicitElementAccess);
                    break;

                case InitializerExpressionSyntax initializer:
                    SemanticRules.UnsupportedSyntax(initializer);
                    break;

                case InterpolatedStringExpressionSyntax interpolatedString:
                    SemanticRules.UnsupportedSyntax(interpolatedString);
                    break;

                case InvocationExpressionSyntax invocation:
                    r = new InvocationIR() { Function = invocation.Expression.GetFunctionReference(model, builder), };
                    r.AddArguments(invocation.ArgumentList.Arguments, model, builder);
                    break;

                case IsPatternExpressionSyntax isPattern:
                    SemanticRules.UnsupportedSyntax(isPattern);
                    break;

                case LiteralExpressionSyntax literal:
                    if (literal.Kind() == SyntaxKind.DefaultLiteralExpression)
                    {
                        r = new DefaultIR() { DefaultType = finalTypeIR.ToReference() };
                    }
                    else
                    {
                        r = new LiteralIR() { Value = literal.ToValue() };
                    }
                    break;

                case MakeRefExpressionSyntax makeRef:
                    SemanticRules.UnsupportedSyntax(makeRef);
                    break;

                case MemberAccessExpressionSyntax memberAccess:
                    if (memberAccess.OperatorToken.ToString() != ".")
                        SemanticRules.OnlyDotNotationSupported(memberAccess);

                    r = new NameIR()
                    {
                        Object = memberAccess.Expression.ToIR(model, builder),
                        Name = memberAccess.Name.ToString(),
                    };
                    break;

                case MemberBindingExpressionSyntax memberBinding:
                    SemanticRules.UnsupportedSyntax(memberBinding);
                    break;

                case OmittedArraySizeExpressionSyntax omittedArraySize:
                    SemanticRules.UnsupportedSyntax(omittedArraySize);
                    break;

                case ParenthesizedLambdaExpressionSyntax parenthesizedlambda:
                    r = FunctionDefinition.Create(parenthesizedlambda, model);
                    break;

                case SimpleLambdaExpressionSyntax simpleLambda:
                    r = FunctionDefinition.Create(simpleLambda, model);
                    break;

                case LambdaExpressionSyntax lambdaExpressionSyntax:
                    r = lambdaExpressionSyntax.
                    FunctionDefinition.Create(lambdaExpressionSyntax, model);
                    break;

                case AnonymousFunctionExpressionSyntax anonymousFunctionExpressionSyntax:
                    SemanticRules.UnsupportedSyntax(anonymousFunctionExpressionSyntax);
                    break;

                case ParenthesizedExpressionSyntax parenthesized:
                    r = new ParenthesizedIR();
                    r.Args.Add(parenthesized.Expression.ToIR(model, builder));
                    break;

                case PostfixUnaryExpressionSyntax postfix:
                    r = new PostfixOperationIR() { Operator = postfix.OperatorToken.ToString() };
                    r.Args.Add(postfix.Operand.ToIR(model, builder));
                    break;

                case PrefixUnaryExpressionSyntax prefix:
                    r = new PrefixOperationIR() { Operator = prefix.OperatorToken.ToString() };
                    r.Args.Add(prefix.Operand.ToIR(model, builder));
                    break;

                case RangeExpressionSyntax rangeExpression:
                    SemanticRules.UnsupportedSyntax(rangeExpression);
                    break;

                case SwitchExpressionSyntax switchExpression:
                    r = new SwitchIR();
                    r.Args.Add(switchExpression.GoverningExpression.ToIR(model, builder));
                    foreach (var arm in switchExpression.Arms)
                    {
                        r.Args.Add(arm.Expression.ToIR(model, builder));
                        r.Args.Add(arm.Pattern.ToIR(model, builder));
                    }
                    break;

                case ThisExpressionSyntax thisExpression:
                    r = new ThisIR();
                    break;

                case ThrowExpressionSyntax throwExpression:
                    r = new ThrowIR();
                    r.Args.Add(throwExpression.Expression.ToIR(model, builder));
                    break;

                case TupleExpressionSyntax tuple:
                    r = new TupleIR();
                    r.AddArguments(tuple.Arguments, model, builder);
                    break;

                case TypeOfExpressionSyntax typeOf:
                    r = new TypeOfIR() { TypeArgument = typeOf.Type.GetTypeReference(model, builder) };
                    break;

                case TypeSyntax typeSyntax:
                    r = typeSyntax.GetTypeReference(model, builder);
                    break;

                case WithExpressionSyntax withExpression:
                    SemanticRules.UnsupportedSyntax(withExpression);
                    break;

                case AnonymousObjectCreationExpressionSyntax anonymousObjectCreationExpressionSyntax:
                    SemanticRules.UnsupportedSyntax(anonymousObjectCreationExpressionSyntax);
                    break;

                case AwaitExpressionSyntax awaitExpressionSyntax:
                    SemanticRules.UnsupportedSyntax(awaitExpressionSyntax);
                    break;

                case BaseExpressionSyntax baseExpressionSyntax:
                    r = new BaseIR();
                    break;

                case ImplicitObjectCreationExpressionSyntax implicitObjectCreationExpressionSyntax:
                    r = new NewIR() { CreatedType = finalTypeIR.ToReference() };
                    if (implicitObjectCreationExpressionSyntax.Initializer != null)
                        SemanticRules.UnsupportedSyntax(implicitObjectCreationExpressionSyntax.Initializer);
                    break;

                case ImplicitStackAllocArrayCreationExpressionSyntax implicitStackAllocArrayCreationExpressionSyntax:
                    SemanticRules.UnsupportedSyntax(implicitStackAllocArrayCreationExpressionSyntax);
                    break;

                case SizeOfExpressionSyntax sizeOfExpressionSyntax:
                    SemanticRules.UnsupportedSyntax(sizeOfExpressionSyntax);
                    break;

                case StackAllocArrayCreationExpressionSyntax stackAllocArrayCreationExpressionSyntax:
                    SemanticRules.UnsupportedSyntax(stackAllocArrayCreationExpressionSyntax);
                    break;

                case BaseObjectCreationExpressionSyntax baseObjectCreationExpressionSyntax:
                    SemanticRules.UnsupportedSyntax(baseObjectCreationExpressionSyntax);
                    break;

                case InstanceExpressionSyntax instanceExpressionSyntax:
                    SemanticRules.UnsupportedSyntax(instanceExpressionSyntax);
                    break;

                default:
                    SemanticRules.UnsupportedSyntax(syntax);
                    break;
            }

            var symbol = ModelExtensions.GetSymbolInfo(model, syntax).Symbol;

            if (r is NameIR nameIR)
            {
                switch (symbol)
                {
                    case null:
                        break;

                    case IArrayTypeSymbol arrayTypeSymbol:
                        break;

                    case IDiscardSymbol discardSymbol:
                        r = new DiscardIR();
                        break;

                    case IFieldSymbol fieldSymbol:
                        nameIR.ReferencedIR = builder.GetDeclarationIR<FieldIR>(fieldSymbol);
                        break;

                    case ILocalSymbol localSymbol:
                        nameIR.ReferencedIR = builder.GetDeclarationIR<VariableDeclarationIR>(localSymbol);
                        break;

                    case IMethodSymbol methodSymbol:
                        nameIR.ReferencedIR = builder.GetDeclarationIR<MethodIR>(methodSymbol);
                        break;

                    case INamedTypeSymbol namedTypeSymbol:
                        nameIR.ReferencedIR = builder.GetDeclarationIR<TypeIR>(namedTypeSymbol);
                        break;

                    case INamespaceSymbol namespaceSymbol:
                        break;

                    case ITypeParameterSymbol typeParameterSymbol:
                        nameIR.ReferencedIR = builder.GetDeclarationIR<TypeParameterIR>(typeParameterSymbol);
                        break;

                    case ITypeSymbol typeSymbol:
                        nameIR.ReferencedIR = builder.GetDeclarationIR<TypeIR>(typeSymbol);
                        break;

                    case IParameterSymbol parameterSymbol:
                        nameIR.ReferencedIR = builder.GetDeclarationIR<ParameterIR>(parameterSymbol);
                        break;

                    case IPropertySymbol propertySymbol:
                        nameIR.ReferencedIR = builder.GetDeclarationIR<PropertyIR>(propertySymbol);
                        break;

                    //case INamespaceOrTypeSymbol namespaceOrTypeSymbol:
                    case IModuleSymbol moduleSymbol:
                    //case IPointerTypeSymbol pointerTypeSymbol:
                    case IPreprocessingSymbol preprocessingSymbol:
                    //case IFunctionPointerTypeSymbol functionPointerTypeSymbol:
                    case ILabelSymbol labelSymbol:
                    case IRangeVariableSymbol rangeVariableSymbol:
                    //case IDynamicTypeSymbol dynamicTypeSymbol:
                    //case IErrorTypeSymbol errorTypeSymbol:
                    case IEventSymbol eventSymbol:
                    case IAliasSymbol aliasSymbol:
                    default:
                        SemanticRules.UnsupportedSymbol(symbol);
                        break;
                }
            }
            else
            {
                if (symbol != null)
                    throw new Exception("Unexpected symbol");
            }

            return r;
        }
    }
}
