using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PlatoIR;

namespace PlatoRoslynSyntaxAnalyzer
{
    public static class SyntaxToIR
    {
        public static SourceLocation ToLocation(this SyntaxNode node)
        {
            throw new NotImplementedException();
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

        public static ExpressionIR ToIR(this PlatoExpressionSyntax syntax, Compilation compilation, IRBuilder builder)
        {
            switch (syntax.Node)
            {
                // Supported expressions
                case AliasQualifiedNameSyntax aliasQualifiedNameSyntax:
                case AnonymousObjectCreationExpressionSyntax anonymousObjectCreationExpressionSyntax:
                case ArrayCreationExpressionSyntax arrayCreationExpressionSyntax:
                case ArrayTypeSyntax arrayTypeSyntax:
                case AssignmentExpressionSyntax assignmentExpressionSyntax:
                case BaseExpressionSyntax baseExpressionSyntax:
                case BinaryExpressionSyntax binaryExpressionSyntax:
                case CastExpressionSyntax castExpressionSyntax:
                case ConditionalAccessExpressionSyntax conditionalAccessExpressionSyntax:
                case ConditionalExpressionSyntax conditionalExpressionSyntax:
                case DeclarationExpressionSyntax declarationExpressionSyntax:
                case DefaultExpressionSyntax defaultExpressionSyntax:
                case ElementAccessExpressionSyntax elementAccessExpressionSyntax:
                case ElementBindingExpressionSyntax elementBindingExpressionSyntax:
                case GenericNameSyntax genericNameSyntax:
                case IdentifierNameSyntax identifierNameSyntax:
                case ImplicitArrayCreationExpressionSyntax implicitArrayCreationExpressionSyntax:
                case ImplicitElementAccessSyntax implicitElementAccessSyntax:
                case ImplicitObjectCreationExpressionSyntax implicitObjectCreationExpressionSyntax:
                case InitializerExpressionSyntax initializerExpressionSyntax:
                case InterpolatedStringExpressionSyntax interpolatedStringExpressionSyntax:
                case InvocationExpressionSyntax invocationExpressionSyntax:
                case IsPatternExpressionSyntax isPatternExpressionSyntax:
                case LiteralExpressionSyntax literalExpressionSyntax:
                case MakeRefExpressionSyntax makeRefExpressionSyntax:
                case MemberAccessExpressionSyntax memberAccessExpressionSyntax:
                case MemberBindingExpressionSyntax memberBindingExpressionSyntax:
                case NullableTypeSyntax nullableTypeSyntax:
                case ObjectCreationExpressionSyntax objectCreationExpressionSyntax:
                case OmittedArraySizeExpressionSyntax omittedArraySizeExpressionSyntax:
                case OmittedTypeArgumentSyntax omittedTypeArgumentSyntax:
                case ParenthesizedExpressionSyntax parenthesizedExpressionSyntax:
                case ParenthesizedLambdaExpressionSyntax parenthesizedLambdaExpressionSyntax:
                case PostfixUnaryExpressionSyntax postfixUnaryExpressionSyntax:
                case PredefinedTypeSyntax predefinedTypeSyntax:
                case PrefixUnaryExpressionSyntax prefixUnaryExpressionSyntax:
                case QualifiedNameSyntax qualifiedNameSyntax:
                case SimpleLambdaExpressionSyntax simpleLambdaExpressionSyntax:
                case SwitchExpressionSyntax switchExpressionSyntax:
                case ThisExpressionSyntax thisExpressionSyntax:
                case ThrowExpressionSyntax throwExpressionSyntax:
                case TupleExpressionSyntax tupleExpressionSyntax:
                case TupleTypeSyntax tupleTypeSyntax:
                case TypeOfExpressionSyntax typeOfExpressionSyntax:
                case BaseObjectCreationExpressionSyntax baseObjectCreationExpressionSyntax:
                case InstanceExpressionSyntax instanceExpressionSyntax:
                case LambdaExpressionSyntax lambdaExpressionSyntax:
                case SimpleNameSyntax simpleNameSyntax:
                case AnonymousFunctionExpressionSyntax anonymousFunctionExpressionSyntax:
                case NameSyntax nameSyntax:
                case TypeSyntax typeSyntax:
                    break;
            }
            
            throw new NotImplementedException();
        }
    }
}
