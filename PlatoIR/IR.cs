using System;
using System.Collections.Generic;

namespace PlatoIR
{
    public abstract class IR
    {
        public int Id { get; set; }
        public string Source { get; set; }
        public abstract void Visit(Func<IR, bool> action);
    }

    public static class IRExtensions
    {
        public static void Visit(this IEnumerable<IR> irs, Func<IR, bool> action)
        {
            foreach (var ir in irs) ir?.Visit(action);
        }

        public static void Visit(this IR ir, Func<IR, bool> action)
        {
            if (!action(ir))
                return;
            switch (ir)
            {
                case AssignmentIR assignmentIr:
                    assignmentIr.LValue?.Visit(action);
                    assignmentIr.RValue?.Visit(action);
                    break;
                case BaseIR baseIr:
                    break;
                case BinaryOperatorIR binaryOperatorIr:
                    binaryOperatorIr.Args?.Visit(action);
                    break;
                case BlockStatementIR blockStatementIr:
                    blockStatementIr.Statements?.Visit(action);
                    break;
                case CastIR castIr:
                    castIr.CastType?.Visit(action);
                    castIr.Args?.Visit(action);
                    break;
                case ConditionalIR conditionalIr:
                    break;
                case ConstructorDeclarationIr constructorDeclarationIr:
                    break;
                case ArrayIR arrayIr:
                    break;
                case DeclarationStatementIR declarationStatementIr:
                    break;
                case DefaultIR defaultIr:
                    break;
                case DiscardIR discardIr:
                    break;
                case DoStatementIR doStatementIr:
                    break;
                case ExpressionStatementIR expressionStatementIr:
                    break;
                case FieldDeclarationIR fieldDeclarationIr:
                    break;
                case FieldReferenceIR fieldReferenceIr:
                    break;
                case IfStatementIR ifStatementIr:
                    break;
                case IndexerDeclarationIr indexerDeclarationIr:
                    break;
                case LambdaIR lambdaIr:
                    break;
                case LetIR letIr:
                    break;
                case LiteralIR literalIr:
                    break;
                case MethodReferenceIR methodReferenceIr:
                    break;
                case MultiStatementIR multiStatementIr:
                    break;
                case NamespaceReferenceIR namespaceReferenceIr:
                    break;
                case NewIR newIr:
                    break;
                case OperationDeclarationIr operationDeclarationIr:
                    break;
                case ParameterDeclarationIR parameterDeclarationIr:
                    break;
                case ParameterReferenceIR parameterReferenceIr:
                    break;
                case ParenthesizedIR parenthesizedIr:
                    break;
                case PostfixOperatorIR postfixOperatorIr:
                    break;
                case PrefixOperatorIR prefixOperatorIr:
                    break;
                case PropertyDeclarationIR propertyDeclarationIr:
                    break;
                case PropertyReferenceIR propertyReferenceIr:
                    break;
                case ReturnStatementIR returnStatementIr:
                    break;
                case SubscriptIR subscriptIr:
                    break;
                case SwitchIR switchIr:
                    break;
                case ThisIR thisIr:
                    break;
                case ThrowIR throwIr:
                    break;
                case TupleIR tupleIr:
                    break;
                case TypeDeclarationIR typeDeclarationIr:
                    break;
                case TypeOfIR typeOfIr:
                    break;
                case TypeParameterDeclarationIR typeParameterDeclarationIr:
                    break;
                case TypeReferenceIR typeReferenceIr:
                    break;
                case UnknownReferenceIR unknownReferenceIr:
                    break;
                case VariableDeclarationIR variableDeclarationIr:
                    break;
                case VariableReferenceIR variableReferenceIr:
                    break;
                case WhileStatementIR whileStatementIr:
                    break;
                case MethodDeclarationIR methodDeclarationIr:
                    break;
                case OperationIR operationIr:
                    break;
                case ReferenceIR referenceIr:
                    break;
                case StatementIR statementIr:
                    break;
                case DeclarationIR declarationIr:
                    break;
                case InvocationIR invocationIr:
                    break;
                case ExpressionIR expressionIr:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(ir));
            }

            action(ir);
        }

        public static T SetParent<T>(this T self, DeclarationIR parent) where T : DeclarationIR
        {
            self.Parent = parent;
            return self;
        }

        public static VariableReferenceIR ToReference(this VariableDeclarationIR var)
            => new VariableReferenceIR(var.Name, var) { ExpressionType = var.Type };
    }
}
