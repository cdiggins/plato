using System;
using System.Collections.Generic;
using System.Linq;

namespace PlatoIR
{
    public static class IRRewriter
    {
        public static T[] Rewrite<T>(this IEnumerable<T> irs, Func<IR, IR> func, Dictionary<DeclarationIR, DeclarationIR> newDeclarations = null) where T : IR
            => irs.Select(ir => (T)Rewrite(ir, func, newDeclarations)).ToArray();

        public static ExpressionIR Rewrite(this ExpressionIR ir, Func<IR, IR> func, Dictionary<DeclarationIR, DeclarationIR> newDeclarations = null)
            => (ExpressionIR)Rewrite(ir as IR, func, newDeclarations);

        public static TypeReferenceIR Rewrite(this TypeReferenceIR ir, Func<IR, IR> func, Dictionary<DeclarationIR, DeclarationIR> newDeclarations = null)
            => (TypeReferenceIR)Rewrite(ir as IR, func, newDeclarations);

        public static StatementIR Rewrite(this StatementIR ir, Func<IR, IR> func, Dictionary<DeclarationIR, DeclarationIR> newDeclarations = null)
            => (StatementIR)Rewrite(ir as IR, func, newDeclarations);

        public static MethodDeclarationIR Rewrite(this MethodDeclarationIR ir, Func<IR, IR> func,
            Dictionary<DeclarationIR, DeclarationIR> newDeclarations = null)
            => (MethodDeclarationIR)Rewrite(ir as IR, func, newDeclarations);

        public static IR Rewrite(this IR ir, Func<IR, IR> func, Dictionary<DeclarationIR, DeclarationIR> newDeclarations = null)
        {
            newDeclarations = newDeclarations ?? new Dictionary<DeclarationIR, DeclarationIR>();
            if (ir == null) return null;

            // If the function rewrites the IR, then there is no need to manually recurse its children ... we assume that that the function
            // has produced the final result. 
            var tmp = func(ir);
            if (tmp != ir)
                return tmp;

            // We are going to create a copy of the IR, with its children rewritten 
            var r = CloneAndRewrite(ir, func, newDeclarations);
            r.Original = ir;

            // If we produced a declaration, we need to update the declaration lookup. This is used to keep references pointing to the correct location
            if (r is DeclarationIR oldDecl && ir is DeclarationIR newDecl)
            {
                newDeclarations.Add(oldDecl, newDecl);
            }

            // Helper for updating expression types. 
            if (r is ExpressionIR newExprIr && ir is ExpressionIR oldExprIr)
            {
                newExprIr.ExpressionType = oldExprIr.ExpressionType.Rewrite(func, newDeclarations);
            }

            return r;
        }

        private static IR CloneAndRewrite(IR ir, Func<IR, IR> func, Dictionary<DeclarationIR, DeclarationIR> newDeclarations)
        {
            switch (ir)
            {
                case null:
                    return null;

                case AssignmentIR assignmentIr:
                    return new AssignmentIR(
                        assignmentIr.LValue.Rewrite(func, newDeclarations),
                        assignmentIr.RValue.Rewrite(func, newDeclarations));

                case BaseIR _:
                    return new BaseIR();

                case BinaryOperatorIR binaryOperatorIr:
                    return new BinaryOperatorIR(binaryOperatorIr.Operator,
                        binaryOperatorIr.Function,
                        binaryOperatorIr.Operand1.Rewrite(func, newDeclarations), binaryOperatorIr.Operand2.Rewrite(func, newDeclarations));

                case BlockStatementIR blockStatementIr:
                    return new BlockStatementIR(blockStatementIr.Statements.Rewrite(func, newDeclarations));

                case CastIR castIr:
                    return new CastIR(castIr.CastType, castIr.CastExpression.Rewrite(func, newDeclarations));

                case ConditionalIR conditionalIr:
                    return new ConditionalIR(
                        conditionalIr.Condition.Rewrite(func, newDeclarations),
                        conditionalIr.OnTrue.Rewrite(func, newDeclarations),
                        conditionalIr.OnFalse.Rewrite(func, newDeclarations));

                case ConstructorDeclarationIr constructorDeclarationIr:
                    return new ConstructorDeclarationIr()
                    {
                        Body = constructorDeclarationIr.Body.Rewrite(func, newDeclarations) as BlockStatementIR,
                        IsStatic = constructorDeclarationIr.IsStatic,
                        Name = constructorDeclarationIr.Name,
                        Parameters = constructorDeclarationIr.Parameters.Rewrite(func, newDeclarations).ToList(),
                        Source = constructorDeclarationIr.Source,
                        Type = constructorDeclarationIr.Type.Rewrite(func, newDeclarations),
                        TypeParameters = constructorDeclarationIr.TypeParameters.Rewrite(func, newDeclarations).ToList()
                    };

                case ArrayIR arrayIr:
                    return new ArrayIR(arrayIr.Size, arrayIr.Args.Rewrite(func, newDeclarations))
                        { ExpressionType = arrayIr.ExpressionType };

                case DeclarationStatementIR declarationStatementIr:
                    return new DeclarationStatementIR(
                        declarationStatementIr.Declaration.Rewrite(func, newDeclarations) as DeclarationIR);

                case DefaultIR defaultIr:
                    return new DefaultIR(defaultIr.DefaultType.Rewrite(func, newDeclarations));

                case DiscardIR _:
                    return new DiscardIR();

                case DoStatementIR doStatementIr:
                    return new DoStatementIR(doStatementIr.Condition.Rewrite(func, newDeclarations),
                        doStatementIr.Body.Rewrite(func, newDeclarations));

                case ExpressionStatementIR expressionStatementIr:
                    return new ExpressionStatementIR(expressionStatementIr.Expression.Rewrite(func, newDeclarations));

                case FieldDeclarationIR fieldDeclarationIr:
                    return new FieldDeclarationIR() {
                        IsStatic = fieldDeclarationIr.IsStatic,
                        Name = fieldDeclarationIr.Name,
                        Type = fieldDeclarationIr.Type.Rewrite(func, newDeclarations),
                        InitialValue = fieldDeclarationIr.InitialValue.Rewrite(func, newDeclarations) };

                case FieldReferenceIR fieldReferenceIr:
                    return new FieldReferenceIR(
                        fieldReferenceIr.Name,
                        fieldReferenceIr.Receiver.Rewrite(func, newDeclarations),
                        newDeclarations.GetOrArgument(fieldReferenceIr.FieldDeclaration));

                case IfStatementIR ifStatementIr:
                    return new IfStatementIR(ifStatementIr.Condition.Rewrite(func, newDeclarations),
                        ifStatementIr.OnTrue.Rewrite(func, newDeclarations), ifStatementIr.OnFalse.Rewrite(func, newDeclarations));

                case IndexerDeclarationIR indexerDeclarationIr:
                    return new IndexerDeclarationIR() {
                        Body = indexerDeclarationIr.Body.Rewrite(func, newDeclarations) as BlockStatementIR,
                        IsStatic = indexerDeclarationIr.IsStatic,
                        Name = indexerDeclarationIr.Name,
                        Parameters = indexerDeclarationIr.Parameters.Rewrite(func, newDeclarations).ToList(),
                        Type = indexerDeclarationIr.Type.Rewrite(func, newDeclarations),
                        TypeParameters = indexerDeclarationIr.TypeParameters.Rewrite(func, newDeclarations).ToList(),
                        Getter = indexerDeclarationIr.Getter.Rewrite(func, newDeclarations) as MethodDeclarationIR,
                    };

                case LambdaIR lambdaIr:
                    return new LambdaIR()
                    {
                        Body = lambdaIr.Body.Rewrite(func, newDeclarations),
                        CapturedVariables = lambdaIr.CapturedVariables.Rewrite(func, newDeclarations).ToList(),
                        Parameters = lambdaIr.Parameters.Rewrite(func, newDeclarations).ToList(),
                    };

                case LetIR letIr:
                    return new LetIR(letIr.Variable.Rewrite(func, newDeclarations) as VariableDeclarationIR,
                        letIr.Expression.Rewrite(func, newDeclarations));

                case LiteralIR literalIr:
                    return new LiteralIR(literalIr.Text, literalIr.Value);

                case MethodReferenceIR methodReferenceIr:
                    return new MethodReferenceIR(
                        methodReferenceIr.Name,
                        methodReferenceIr.Receiver.Rewrite(func, newDeclarations),
                        newDeclarations.GetOrArgument(methodReferenceIr.MethodDeclaration), 
                        methodReferenceIr.TypeArguments.Rewrite(func, newDeclarations));

                case MultiStatementIR multiStatementIr:
                    return new MultiStatementIR(multiStatementIr.Statements.Rewrite(func, newDeclarations));

                case NamespaceDeclarationIR namespaceDeclarationIr:
                    throw new NotImplementedException();

                case NamespaceReferenceIR namespaceReferenceIr:
                    return new NamespaceReferenceIR(
                        namespaceReferenceIr.Name, 
                        namespaceReferenceIr.Receiver.Rewrite(func, newDeclarations), 
                        newDeclarations.GetOrArgument(namespaceReferenceIr.Namespace));

                case NewIR newIr:
                    return new NewIR(newIr.CreatedType.Rewrite(func, newDeclarations), newIr.Args.Rewrite(func, newDeclarations));

                case OperationDeclarationIR operationDeclarationIr:
                    return new OperationDeclarationIR()
                    {
                        Type = operationDeclarationIr.Type.Rewrite(func, newDeclarations),
                        Parameters = operationDeclarationIr.Parameters.Rewrite(func, newDeclarations).ToList(),
                        Body = operationDeclarationIr.Body.Rewrite(func, newDeclarations) as BlockStatementIR,
                        IsStatic = operationDeclarationIr.IsStatic,
                        Name = operationDeclarationIr.Name,
                        TypeParameters = operationDeclarationIr.TypeParameters.Rewrite(func, newDeclarations).ToList()
                    };

                case ParameterDeclarationIR parameterDeclarationIr:
                    return new ParameterDeclarationIR()
                    {
                        IsStatic = parameterDeclarationIr.IsStatic,
                        IsThisParameter = parameterDeclarationIr.IsThisParameter,
                        Name = parameterDeclarationIr.Name,
                        Type = parameterDeclarationIr.Type.Rewrite(func, newDeclarations),
                        DefaultValue = parameterDeclarationIr.DefaultValue.Rewrite(func, newDeclarations)
                    };

                case ParameterReferenceIR parameterReferenceIr:
                    return new ParameterReferenceIR(parameterReferenceIr.Name, parameterReferenceIr.ParameterDeclaration);

                case ParenthesizedIR parenthesizedIr:
                    return new ParenthesizedIR(parenthesizedIr.Expression.Rewrite(func, newDeclarations));

                case PostfixOperatorIR postfixOperatorIr:
                    return new PostfixOperatorIR(postfixOperatorIr.Operator,
                        postfixOperatorIr.Function.Rewrite(func, newDeclarations) as MethodReferenceIR, 
                        postfixOperatorIr.Operand.Rewrite(func, newDeclarations));

                case PrefixOperatorIR prefixOperatorIr:
                    return new PrefixOperatorIR(prefixOperatorIr.Operator,
                        prefixOperatorIr.Function.Rewrite(func, newDeclarations) as MethodReferenceIR,
                        prefixOperatorIr.Operand.Rewrite(func, newDeclarations));

                case PropertyDeclarationIR propertyDeclarationIr:
                    return new PropertyDeclarationIR()
                    {
                        Field = propertyDeclarationIr.Field.Rewrite(func, newDeclarations) as FieldReferenceIR,
                        Type = propertyDeclarationIr.Type.Rewrite(func, newDeclarations),
                        Getter = propertyDeclarationIr.Getter.Rewrite(func, newDeclarations) as MethodDeclarationIR,
                        IsStatic = propertyDeclarationIr.IsStatic,
                        Name = propertyDeclarationIr.Name,
                        TypeKind = propertyDeclarationIr.TypeKind,
                    };
                        
                case PropertyReferenceIR propertyReferenceIr:
                    return new PropertyReferenceIR(
                        propertyReferenceIr.Name, 
                        propertyReferenceIr.Receiver.Rewrite(func, newDeclarations),
                        newDeclarations.GetOrArgument(propertyReferenceIr.PropertyDeclaration));

                case ReturnStatementIR returnStatementIr:
                    return new ReturnStatementIR(
                        returnStatementIr.Expression.Rewrite(func, newDeclarations));

                case SubscriptIR subscriptIr:
                    return new SubscriptIR(
                        subscriptIr.Reciever.Rewrite(func, newDeclarations), 
                        subscriptIr.Subscript.Rewrite(func, newDeclarations));

                case SwitchIR switchIr:
                    return new SwitchIR(
                        switchIr.Control.Rewrite(func, newDeclarations), 
                        switchIr.Cases.Rewrite(func, newDeclarations));

                case ThisIR _:
                    return new ThisIR();

                case ThrowIR throwIr:
                    return new ThrowIR(
                        throwIr.Expression.Rewrite(func, newDeclarations));

                case TupleIR tupleIr:
                    return new TupleIR(
                        tupleIr.Args.Rewrite(func, newDeclarations));

                case TypeDeclarationIR typeDeclarationIr:
                    return new TypeDeclarationIR(typeDeclarationIr.Kind, typeDeclarationIr.Name)
                    {
                        Bases = typeDeclarationIr.Bases.Rewrite(func, newDeclarations).ToList(),
                        Fields = typeDeclarationIr.Fields.Rewrite(func, newDeclarations).ToList(),
                        Constructors = typeDeclarationIr.Constructors.Rewrite(func, newDeclarations).ToList(),
                        Methods = typeDeclarationIr.Methods.Rewrite(func, newDeclarations).ToList(),
                        Indexers = typeDeclarationIr.Indexers.Rewrite(func, newDeclarations).ToList(),
                        Properties = typeDeclarationIr.Properties.Rewrite(func, newDeclarations).ToList(),
                        Operations = typeDeclarationIr.Operations.Rewrite(func, newDeclarations).ToList(),
                        TypeParameters = typeDeclarationIr.TypeParameters.Rewrite(func, newDeclarations).ToList()
                    };

                case TypeOfIR typeOfIr:
                    return new TypeOfIR(
                        typeOfIr.TypeArgument.Rewrite(func, newDeclarations));

                case TypeParameterDeclarationIR typeParameterDeclarationIr:
                    return new TypeParameterDeclarationIR()
                    {
                        IsStatic = typeParameterDeclarationIr.IsStatic,
                        Name = typeParameterDeclarationIr.Name,
                        Type = typeParameterDeclarationIr.Type.Rewrite(func, newDeclarations)
                    };

                case TypeReferenceIR typeReferenceIr:
                    return new TypeReferenceIR(
                        typeReferenceIr.Name,
                        typeReferenceIr.Receiver.Rewrite(func, newDeclarations),
                        newDeclarations.GetOrArgument(typeReferenceIr.TypeDeclaration),
                        newDeclarations.GetOrArgument(typeReferenceIr.TypeParameterDeclaration),
                        typeReferenceIr.TypeArguments.Rewrite(func, newDeclarations));

                case UnknownReferenceIR unknownReferenceIr:
                    return new UnknownReferenceIR(
                        unknownReferenceIr.Name, 
                        unknownReferenceIr.Receiver.Rewrite(func, newDeclarations),
                        unknownReferenceIr.TypeArguments.Rewrite(func, newDeclarations));

                case VariableDeclarationIR variableDeclarationIr:
                    return new VariableDeclarationIR()
                    {
                        IsStatic = variableDeclarationIr.IsStatic,
                        Name = variableDeclarationIr.Name,
                        Type = variableDeclarationIr.Type.Rewrite(func, newDeclarations),
                        InitialValue = variableDeclarationIr.InitialValue.Rewrite(func, newDeclarations)
                    };

                case VariableReferenceIR variableReferenceIr:
                    return new VariableReferenceIR(
                        variableReferenceIr.Name, 
                        variableReferenceIr.VariableDeclaration);

                case WhileStatementIR whileStatementIr:
                    return new WhileStatementIR(
                        whileStatementIr.Condition.Rewrite(func, newDeclarations),
                        whileStatementIr.Body.Rewrite(func, newDeclarations));

                case MethodDeclarationIR methodDeclarationIr:
                    return new MethodDeclarationIR()
                    {
                        IsStatic = methodDeclarationIr.IsStatic,
                        Name = methodDeclarationIr.Name,
                        Type = methodDeclarationIr.Type.Rewrite(func, newDeclarations),
                        Parameters = methodDeclarationIr.Parameters.Rewrite(func, newDeclarations).ToList(),
                        Body = methodDeclarationIr.Body.Rewrite(func, newDeclarations) as BlockStatementIR,
                        TypeParameters = methodDeclarationIr.TypeParameters.Rewrite(func, newDeclarations).ToList(),
                    };

                case InvocationIR invocationIr:
                    return new InvocationIR(
                        invocationIr.Function.Rewrite(func, newDeclarations), 
                        invocationIr.Args.Rewrite(func, newDeclarations));

                default:
                    throw new ArgumentOutOfRangeException(nameof(ir));
            }
        }

        public static TKey GetOrArgument<T, TKey>(this Dictionary<T, T> self, TKey key)
            where TKey: T
            => key != null && self.ContainsKey(key) ? (TKey)self[key] : key;

        public static T Clone<T>(this T self) where T : IR
            => (T)self.Rewrite(x => x, new Dictionary<DeclarationIR, DeclarationIR>());

        public static IR Replace<TKey, TValue>(this IR self, Dictionary<TKey, TValue> lookup)
            where TKey : IR where TValue : IR
            => self.Rewrite(x => x is TKey key ? lookup[key] : x);
    }
}