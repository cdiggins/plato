using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace PlatoIR
{
    public static class IRRewriter
    {
        public static T[] Rewrite<T>(this IEnumerable<T> irs, Func<IR, IR> func) where T : IR
            => irs.Select(ir => (T)Rewrite((IR)ir, func)).ToArray();

        public static ExpressionIR Rewrite(this ExpressionIR ir, Func<IR, IR> func)
            => (ExpressionIR)Rewrite(ir as IR, func);

        public static StatementIR Rewrite(this StatementIR ir, Func<IR, IR> func)
            => (StatementIR)Rewrite(ir as IR, func);

        public static IR Rewrite(this IR ir, Func<IR, IR> func)
        {
            var tmp = func(ir);
            if (tmp != ir)
                return tmp;

            var r = CreateNewIr(ir, func);
            if (r is ExpressionIR newExprIr && ir is ExpressionIR oldExprIr)
            {
                newExprIr.ExpressionType = oldExprIr.ExpressionType.Rewrite(func) as TypeReferenceIR;
            }
            return r;
        }

        private static IR CreateNewIr(IR ir, Func<IR, IR> func)
        {
            if (ir is ReferenceIR refIr)
            {
                refIr.Receiver = refIr.Receiver.Rewrite(func);
                refIr.TypeArguments = refIr.TypeArguments.Rewrite(func).ToList();
            }

            switch (ir)
            {
                case null:
                    return null;

                case AssignmentIR assignmentIr:
                    return new AssignmentIR(
                        assignmentIr.LValue.Rewrite(func),
                        assignmentIr.RValue.Rewrite(func));

                case BaseIR baseIr:
                    return baseIr;

                case BinaryOperatorIR binaryOperatorIr:
                    return new BinaryOperatorIR(binaryOperatorIr.Operator,
                        binaryOperatorIr.Function as MethodReferenceIR,
                        binaryOperatorIr.Operand1.Rewrite(func), binaryOperatorIr.Operand2.Rewrite(func));

                case BlockStatementIR blockStatementIr:
                    return new BlockStatementIR(blockStatementIr.Statements.Rewrite(func));

                case CastIR castIr:
                    return new CastIR(castIr.CastType, castIr.CastExpression.Rewrite(func));

                case ConditionalIR conditionalIr:
                    return new ConditionalIR(
                        conditionalIr.Condition.Rewrite(func),
                        conditionalIr.OnTrue.Rewrite(func),
                        conditionalIr.OnFalse.Rewrite(func));

                case ConstructorDeclarationIr constructorDeclarationIr:
                    constructorDeclarationIr.Body.Statements =
                        constructorDeclarationIr.Body.Statements.Rewrite(func).ToList();
                    return constructorDeclarationIr;

                case ArrayIR arrayIr:
                    return new ArrayIR(arrayIr.Size, arrayIr.Args.Rewrite(func))
                        { ExpressionType = arrayIr.ExpressionType };

                case DeclarationStatementIR declarationStatementIr:
                    return new DeclarationStatementIR(
                        declarationStatementIr.Declaration.Rewrite(func) as DeclarationIR);

                case DefaultIR defaultIr:
                    return defaultIr;

                case DiscardIR discardIr:
                    return discardIr;

                case DoStatementIR doStatementIr:
                    return new DoStatementIR(doStatementIr.Condition.Rewrite(func),
                        doStatementIr.Body.Rewrite(func));

                case ExpressionStatementIR expressionStatementIr:
                    return new ExpressionStatementIR(expressionStatementIr.Expression.Rewrite(func));

                case FieldDeclarationIR fieldDeclarationIr:
                    fieldDeclarationIr.InitialValue = fieldDeclarationIr.InitialValue.Rewrite(func);
                    return fieldDeclarationIr;

                case FieldReferenceIR fieldReferenceIr:
                    return fieldReferenceIr;

                case IfStatementIR ifStatementIr:
                    return new IfStatementIR(ifStatementIr.Condition.Rewrite(func),
                        ifStatementIr.OnTrue.Rewrite(func), ifStatementIr.OnFalse.Rewrite(func));

                case IndexerDeclarationIR indexerDeclarationIr:
                    indexerDeclarationIr.Getter = indexerDeclarationIr.Getter.Rewrite(func) as MethodDeclarationIR;
                    return indexerDeclarationIr;

                case LambdaIR lambdaIr:
                    lambdaIr.Body = lambdaIr.Body.Rewrite(func);
                    return lambdaIr;

                case LetIR letIr:
                    return new LetIR(letIr.Variable.Rewrite(func) as VariableDeclarationIR,
                        letIr.Expression.Rewrite(func));

                case LiteralIR literalIr:
                    return literalIr;

                case MethodReferenceIR methodReferenceIr:
                    return methodReferenceIr;

                case MultiStatementIR multiStatementIr:
                    return new MultiStatementIR(multiStatementIr.Statements.Rewrite(func));

                case NamespaceDeclarationIR namespaceDeclarationIr:
                    return namespaceDeclarationIr;

                case NamespaceReferenceIR namespaceReferenceIr:
                    return namespaceReferenceIr;

                case NewIR newIr:
                    return new NewIR(newIr.CreatedType.Rewrite(func) as TypeReferenceIR, newIr.Args.Rewrite(func));

                case OperationDeclarationIR operationDeclarationIr:
                    operationDeclarationIr.Body = operationDeclarationIr.Body.Rewrite(func) as BlockStatementIR;
                    return operationDeclarationIr;

                case ParameterDeclarationIR parameterDeclarationIr:
                    parameterDeclarationIr.DefaultValue = parameterDeclarationIr.DefaultValue.Rewrite(func);
                    return parameterDeclarationIr;

                case ParameterReferenceIR parameterReferenceIr:
                    return parameterReferenceIr;

                case ParenthesizedIR parenthesizedIr:
                    return new ParenthesizedIR(parenthesizedIr.Expression.Rewrite(func));

                case PostfixOperatorIR postfixOperatorIr:
                    return new PostfixOperatorIR(postfixOperatorIr.Operator,
                        postfixOperatorIr.Function as MethodReferenceIR, postfixOperatorIr.Operand.Rewrite(func));

                case PrefixOperatorIR prefixOperatorIr:
                    return new PrefixOperatorIR(prefixOperatorIr.Operator,
                        prefixOperatorIr.Function as MethodReferenceIR, prefixOperatorIr.Operand.Rewrite(func));

                case PropertyDeclarationIR propertyDeclarationIr:
                    propertyDeclarationIr.Getter = propertyDeclarationIr.Getter.Rewrite(func) as MethodDeclarationIR;
                    return propertyDeclarationIr;

                case PropertyReferenceIR propertyReferenceIr:
                    return propertyReferenceIr;

                case ReturnStatementIR returnStatementIr:
                    return new ReturnStatementIR(returnStatementIr.Expression.Rewrite(func));

                case SubscriptIR subscriptIr:
                    return new SubscriptIR(subscriptIr.Reciever.Rewrite(func), subscriptIr.Subscript.Rewrite(func));

                case SwitchIR switchIr:
                    return new SwitchIR(switchIr.Control.Rewrite(func), switchIr.Cases.Rewrite(func));

                case ThisIR thisIr:
                    return thisIr;

                case ThrowIR throwIr:
                    return new ThrowIR(throwIr.Expression.Rewrite(func));

                case TupleIR tupleIr:
                    return new TupleIR(tupleIr.Args.Rewrite(func));

                case TypeDeclarationIR typeDeclarationIr:
                    typeDeclarationIr.Bases = typeDeclarationIr.Bases.Rewrite(func).ToList();
                    typeDeclarationIr.Fields = typeDeclarationIr.Fields.Rewrite(func).ToList();
                    typeDeclarationIr.Constructors = typeDeclarationIr.Constructors.Rewrite(func).ToList();
                    typeDeclarationIr.Methods = typeDeclarationIr.Methods.Rewrite(func).ToList();
                    typeDeclarationIr.Indexers = typeDeclarationIr.Indexers.Rewrite(func).ToList();
                    typeDeclarationIr.Properties = typeDeclarationIr.Properties.Rewrite(func).ToList();
                    typeDeclarationIr.Operations = typeDeclarationIr.Operations.Rewrite(func).ToList();
                    typeDeclarationIr.TypeParameters = typeDeclarationIr.TypeParameters.Rewrite(func).ToList();
                    return typeDeclarationIr;

                case TypeOfIR typeOfIr:
                    return new TypeOfIR(typeOfIr.TypeArgument.Rewrite(func) as TypeReferenceIR);

                case TypeParameterDeclarationIR typeParameterDeclarationIr:
                    return typeParameterDeclarationIr;

                case TypeReferenceIR typeReferenceIr:
                    return typeReferenceIr;

                case UnknownReferenceIR unknownReferenceIr:
                    return unknownReferenceIr;

                case VariableDeclarationIR variableDeclarationIr:
                    variableDeclarationIr.Type = variableDeclarationIr.Type.Rewrite(func) as TypeReferenceIR;
                    variableDeclarationIr.InitialValue = variableDeclarationIr.InitialValue.Rewrite(func);
                    return variableDeclarationIr;

                case VariableReferenceIR variableReferenceIr:
                    return variableReferenceIr;

                case WhileStatementIR whileStatementIr:
                    return new WhileStatementIR(whileStatementIr.Condition.Rewrite(func),
                        whileStatementIr.Body.Rewrite(func));

                case MethodDeclarationIR methodDeclarationIr:
                    methodDeclarationIr.Body = methodDeclarationIr.Body.Rewrite(func) as BlockStatementIR;
                    return methodDeclarationIr;

                case InvocationIR invocationIr:
                    return new InvocationIR(invocationIr.Function.Rewrite(func), invocationIr.Args.Rewrite(func));

                case ReferenceIR referenceIr:
                case StatementIR statementIr:
                case DeclarationIR declarationIr:
                case ExpressionIR expressionIr:
                default:
                    throw new ArgumentOutOfRangeException(nameof(ir));
            }

            throw new Exception("Internal rewriting error");
        }

        public static IR Replace<T1, T2>(this IR self, Dictionary<T1, T2> lookup) where T1: IR where T2: IR
            => self.Rewrite(ir => ir is T1 u && lookup.ContainsKey(u) ? lookup[u] : ir);
    }
}