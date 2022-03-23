using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace PlatoGenerator
{
    public class PlatoExpression
    {
        public PlatoExpressionSyntax Syntax;
        public object Value; 
        public List<PlatoExpression> Arguments = new List<PlatoExpression>();
        public ISymbol Symbol;
        public SemanticModel Model;
        public string Name;
        public PlatoSyntax Declaration;
        public ITypeSymbol Type;
        public PlatoSyntax TypeDeclaration;
        public PlatoExpression This;
        public PlatoMethodSyntax RelatedMethod => Declaration as PlatoMethodSyntax;
        public string SyntaxNodeTypeName => Syntax.Node.GetType().Name;
        public string SymbolTypeName => Symbol?.GetType().Name;
        public SyntaxKind SyntaxKind => Syntax.Node.Kind();

        public List<PlatoExpression> Children = new List<PlatoExpression>();

        public bool IsMethod => Symbol is IMethodSymbol;

        public PlatoExpression Create(PlatoExpressionSyntax syntax)
            => Create(syntax, Model);

        public static PlatoExpression Create(SyntaxNode node, SemanticModel model)
            => Create(PlatoSyntax.GetSyntax(node) as PlatoExpressionSyntax, model);

        public static PlatoExpression Create(PlatoExpressionSyntax syntax, SemanticModel model)
        {
            if (syntax == null)
                return null;

            model = model.Compilation.GetSemanticModel(syntax.Node.SyntaxTree);
            var symbol = ModelExtensions.GetSymbolInfo(model, syntax.Node).Symbol;
            var decl = symbol.GetDeclaringPlatoSyntax();
            var type = model.GetTypeInfo(syntax.Node).ConvertedType;
            var typeDecl = type.GetDeclaringPlatoSyntax();

            if (decl != null)
            {
                Debug.WriteLine("Found declaration");
            }
            if (typeDecl != null)
            {
                Debug.WriteLine("Found type declaration");
            }

            var r = new PlatoExpression()
            {
                Name = "unsupported:" + syntax.Node.Kind(),
                Syntax = syntax,
                Model = model,
                Symbol = symbol,
                Type = type,
                Declaration = decl,
                TypeDeclaration = typeDecl,
            };

            switch (syntax.Node)
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
                    break;

                case AssignmentExpressionSyntax assignment:
                    break;

                case ObjectCreationExpressionSyntax objectCreation:
                    break;

                case BinaryExpressionSyntax binary:
                    r.Arguments.Add(Create(binary.Left, model));
                    r.Arguments.Add(Create(binary.Right, model));
                    r.Name = binary.OperatorToken.ToString();
                    break;

                case CastExpressionSyntax castExpression:
                    r.Name = "#cast";
                    r.Arguments.Add(Create(castExpression.Expression, model));
                    break;

                case CheckedExpressionSyntax checkedExpression:
                    break;

                case ConditionalAccessExpressionSyntax conditionalAccess:
                    break;

                case ConditionalExpressionSyntax conditional:
                    r.Name = "?";
                    r.Arguments.Add(Create(conditional.Condition, model));
                    r.Arguments.Add(Create(conditional.WhenTrue, model));
                    r.Arguments.Add(Create(conditional.WhenFalse, model));
                    break;

                case DeclarationExpressionSyntax declaration:
                    break;

                case DefaultExpressionSyntax defaultExpression:
                    r.Name = "#default";
                    break;

                case ElementAccessExpressionSyntax elementAccess:
                    r.Name = "#at";
                    r.This = Create(elementAccess.Expression, model);
                    r.Arguments.AddRange(elementAccess.ArgumentList.Arguments.Select(x => Create(x, model)));
                    break;

                case ElementBindingExpressionSyntax elementBinding:
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
                    r.This = Create(invocation.Expression, model);
                    r.Arguments.AddRange(invocation.ArgumentList.Arguments.Select(x => Create(x, model)));
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
                    r.This = Create(memberAccess.Expression, model);
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
                    r.Arguments.Add(Create(parenthesized.Expression, model));
                    break;

                case PostfixUnaryExpressionSyntax postfix:
                    r.Name = postfix.OperatorToken.Text;
                    r.Arguments.Add(Create(postfix.Operand, model));
                    break;

                case PrefixUnaryExpressionSyntax prefix:
                    r.Name = prefix.OperatorToken.Text;
                    r.Arguments.Add(Create(prefix.Operand, model));
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
                    r.Arguments.Add(Create(throwExpression.Expression, model));
                    break;

                case TupleExpressionSyntax tuple:
                    r.Name = "#tuple";
                    r.Arguments.AddRange(tuple.Arguments.Select(a => Create(a.Expression, model)));
                    break;

                case TypeOfExpressionSyntax typeOf:
                    r.Name = "#typeof";
                    r.Arguments.Add(Create(typeOf.Type, model));
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
                    break;

                case ImplicitObjectCreationExpressionSyntax implicitObjectCreationExpressionSyntax:
                    break;

                case ImplicitStackAllocArrayCreationExpressionSyntax implicitStackAllocArrayCreationExpressionSyntax:
                    break;

                case SizeOfExpressionSyntax sizeOfExpressionSyntax:
                    break;

                case StackAllocArrayCreationExpressionSyntax stackAllocArrayCreationExpressionSyntax:
                    break;

                case BaseObjectCreationExpressionSyntax baseObjectCreationExpressionSyntax:
                    break;

                case InstanceExpressionSyntax instanceExpressionSyntax:
                    break;

                case LambdaExpressionSyntax lambdaExpressionSyntax:
                    break;

                case AnonymousFunctionExpressionSyntax anonymousFunctionExpressionSyntax:
                    break;

                default:
                    break;
            }

            foreach (var x in syntax.ChildExpressions)
            {
                r.Children.Add(Create(x, model));
            }

            return r;
        }
    }
}