using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace PlatoAnalyzer
{
    public static class PlatoFromCSharp
    {
        public static IEnumerable<T> GetElements<T>(this SyntaxNode self) where T : SyntaxNode
            => self.DescendantNodes(x => !(x is T)).OfType<T>();

        public static PlatoExpressionStatement ToPlatoStatement(this ExpressionSyntax self, PlatoSemanticMapping mapping)
            => mapping.Add(() => new PlatoExpressionStatement(mapping.NextId, self.ToPlato(mapping)), self);

        public static PlatoReturnStatement ToPlatoReturnStatement(this ExpressionSyntax self, PlatoSemanticMapping mapping)
            => mapping.Add(() => new PlatoReturnStatement(mapping.NextId, self.ToPlato(mapping)), self);

        public static PlatoBlockStatement ToPlatoBlockStatement(this ExpressionSyntax self, PlatoSemanticMapping mapping)
            => mapping.CreateBlockStatement(() => self.ToPlatoReturnStatement(mapping));

        public static PlatoBlockStatement ToPlato(this BlockSyntax self, ExpressionSyntax expr, PlatoSemanticMapping mapping)
            => self?.ToPlato(mapping) ?? expr.ToPlatoBlockStatement(mapping);

        public static PlatoTypeExpr GetPlatoType(this ISymbol self, PlatoSemanticMapping mapping)
        {
            PlatoTypeExpr GetPlatoType_Local(ISymbol selfInner, PlatoSemanticMapping mappingInner)
            {
                switch (selfInner)
                {
                    case IAliasSymbol aliasSymbol:
                        throw selfInner.NotSupported();
                    case IArrayTypeSymbol arrayTypeSymbol:
                        return new PlatoTypeExpr(mappingInner.NextId, "Array",
                            new[] { arrayTypeSymbol.ElementType.GetPlatoType(mappingInner) });
                    case ISourceAssemblySymbol sourceAssemblySymbol:
                        throw selfInner.NotSupported();
                    case IAssemblySymbol assemblySymbol:
                        throw selfInner.NotSupported();
                    case IDiscardSymbol discardSymbol:
                        throw selfInner.NotSupported();
                    case IDynamicTypeSymbol dynamicTypeSymbol:
                        throw selfInner.NotSupported();
                    case IErrorTypeSymbol errorTypeSymbol:
                        throw selfInner.NotSupported();
                    case IEventSymbol eventSymbol:
                        throw selfInner.NotSupported();
                    case IFieldSymbol fieldSymbol:
                        return fieldSymbol.Type.GetPlatoType(mappingInner);
                    case IFunctionPointerTypeSymbol functionPointerTypeSymbol:
                        throw selfInner.NotSupported();
                    case ILabelSymbol labelSymbol:
                        throw selfInner.NotSupported();
                    case ILocalSymbol localSymbol:
                        return localSymbol.Type.GetPlatoType(mappingInner);
                    case IMethodSymbol methodSymbol:
                    {
                        var name = methodSymbol.ReturnsVoid ? "Action" : "Func";

                        var parameters = methodSymbol.Parameters.Select(p => p.GetPlatoType(mappingInner)).ToList();

                        if (methodSymbol.ReceiverType != null)
                            parameters = parameters.Prepend(methodSymbol.ReceiverType.GetPlatoType(mappingInner)).ToList();

                        if (!methodSymbol.ReturnsVoid)
                            parameters = parameters.Append(methodSymbol.ReturnType.GetPlatoType(mappingInner)).ToList();

                        return new PlatoTypeExpr(mappingInner.NextId, name, parameters);
                    }
                    case IModuleSymbol moduleSymbol:
                        throw selfInner.NotSupported();
                    case INamedTypeSymbol namedTypeSymbol:
                        return new PlatoTypeExpr(mappingInner.NextId, selfInner.Name,
                            namedTypeSymbol.TypeArguments.Select(t => GetPlatoType((ISymbol)t, mappingInner)));
                    case INamespaceSymbol namespaceSymbol:
                        throw selfInner.NotSupported();
                    case IPointerTypeSymbol pointerTypeSymbol:
                        throw selfInner.NotSupported();
                    case ITypeParameterSymbol typeParameterSymbol:
                        return new PlatoTypeExpr(mappingInner.NextId, typeParameterSymbol.Name);
                    case IParameterSymbol parameterSymbol:
                        return parameterSymbol.Type.GetPlatoType(mappingInner);
                    case IPreprocessingSymbol preprocessingSymbol:
                        throw selfInner.NotSupported();
                    case IPropertySymbol propertySymbol:
                        return propertySymbol.Type.GetPlatoType(mappingInner);
                    case IRangeVariableSymbol rangeVariableSymbol:
                        throw selfInner.NotSupported();
                    default:
                        throw new ArgumentOutOfRangeException(nameof(selfInner));
                }
            }

            var result = GetPlatoType_Local(self, mapping);
            mapping.Add(result, self);
            return result;
        }

        public static PlatoTypeExpr GetPlatoType(this SyntaxNode self, PlatoSemanticMapping mapping)
            => mapping.Model.GetTypeInfo(self).ToPlatoType(mapping) ??
               mapping.Model.GetSymbolInfo(self).Symbol?.GetPlatoType(mapping);

        public static PlatoTypeExpr ToPlatoType(this TypeInfo self, PlatoSemanticMapping mapping)
            => self.Type?.GetPlatoType(mapping);

        public static PlatoArg ToPlato(this ArgumentSyntax self, PlatoSemanticMapping mapping)
            => mapping.Add(() => new PlatoArg(mapping.NextId, self.Expression.GetPlatoType(mapping), self.Expression.ToPlato(mapping), self.NameColon?.Name.ToString()), self);

        public static PlatoArgList ToPlato(this ArgumentListSyntax self, PlatoSemanticMapping mapping)
            => mapping.Add(() =>
                new PlatoArgList(mapping.NextId, self.Arguments.Select(arg => arg.ToPlato(mapping))), self);

        public static PlatoExpression ToPlatoArraySize(this ArrayTypeSyntax self, PlatoSemanticMapping mapping)
            => self.RankSpecifiers.Count == 0
                ? null
                : self.RankSpecifiers.Count > 1
                    ? throw self.NotSupported()
                    : self.RankSpecifiers[0].Sizes.Count != 1
                        ? throw self.NotSupported()
                        : self.RankSpecifiers[0].Sizes[0].ToPlato(mapping);

        public static List<PlatoExpression> ToPlato(this InitializerExpressionSyntax self, PlatoSemanticMapping mapping)
            => self?.Expressions.ToPlato(mapping) ?? new List<PlatoExpression>();

        public static List<PlatoExpression> ToPlato(this SeparatedSyntaxList<ExpressionSyntax> self,PlatoSemanticMapping mapping)
            => self.Select(x => x.ToPlato(mapping)).ToList();

        public static PlatoExpression ToPlato(this InterpolatedStringContentSyntax self, PlatoSemanticMapping mapping)
            => self is InterpolationSyntax expr
                ? expr.Expression.ToPlato(mapping)
                : self is InterpolatedStringTextSyntax text
                    ? text.TextToken.ToPlatoLiteral(mapping)
                    : throw self.NotSupported();

        public static PlatoExpression ToPlato(this InvocationExpressionSyntax self, PlatoTypeExpr type, PlatoSemanticMapping mapping)
        {
            if (self.Expression.GetText().ToString() == "nameof")
            {
                var name = self.ArgumentList.Arguments[0].ToString();
                return new PlatoNameOf(mapping.NextId, name);;
            }

            return new PlatoInvoke(mapping.NextId, type,
                self.Expression.ToPlato(mapping),
                self.Expression is MemberAccessExpressionSyntax maes
                    ? maes.Expression.ToPlato(mapping)
                    : null,
                self.ArgumentList.ToPlato(mapping));
        }


        public static PlatoExpression ToPlato(this ExpressionSyntax self, PlatoSemanticMapping mapping)
        {
            if (self is IdentifierNameSyntax ins)
            {
                if (ins.Identifier.Text == "nameof")
                {
                    var expr = mapping.Add(() => new PlatoNameOf(mapping.NextId, "Test"));
                    return expr;
                }
            }

            var type = self.GetPlatoType(mapping);
            switch (self)
            {

                // TODO: figure this out.
                /*
                case IdentifierNameSyntax identifierNameSyntax:
                    return mapping.Add(() => new PlatoIdentifierRef(mapping.NextId, ));

                case SimpleNameSyntax simpleNameSyntax:
                    break;

                case NameSyntax nameSyntax:
                    break;
                */

                case ArrayCreationExpressionSyntax arrayCreation:
                    return mapping.Add(() => new PlatoArray(mapping.NextId, arrayCreation.Type.ElementType.ToPlato(mapping), 
                        arrayCreation.Type.ToPlatoArraySize(mapping), arrayCreation.Initializer.ToPlato(mapping)), self);

                case AssignmentExpressionSyntax assignment:
                    return mapping.Add(() => new PlatoAssignment(mapping.NextId, assignment.OperatorToken.ToString(),
                        assignment.Left.ToPlato(mapping), assignment.Right.ToPlato(mapping)), self);

                case ObjectCreationExpressionSyntax objectCreation:
                    return mapping.Add(() => new PlatoNew(mapping.NextId, type,
                        objectCreation.ArgumentList?.ToPlato(mapping), objectCreation.Initializer.ToPlato(mapping)), self);
                
                case BinaryExpressionSyntax binary:
                    return mapping.Add(() => new PlatoBinary(mapping.NextId, type,
                        binary.OperatorToken.ToString(), binary.Left.ToPlato(mapping), binary.Right.ToPlato(mapping)), self);

                case CastExpressionSyntax castExpression:
                    return mapping.Add(
                        () => new PlatoCast(mapping.NextId, type, castExpression.Expression.ToPlato(mapping)), self);
                
                case CheckedExpressionSyntax checkedExpression: 
                    throw self.NotSupported();
                
                case ConditionalAccessExpressionSyntax conditionalAccess: 
                    throw self.NotSupported();

                case ConditionalExpressionSyntax conditional:
                    return mapping.Add(() =>
                        new PlatoConditional(mapping.NextId, type, conditional.Condition.ToPlato(mapping),
                            conditional.WhenTrue.ToPlato(mapping),
                            conditional.WhenFalse.ToPlato(mapping)), self);

                case DeclarationExpressionSyntax declaration: 
                    throw self.NotSupported();

                case DefaultExpressionSyntax defaultExpression:
                    return mapping.Add(() =>
                        new PlatoDefault(mapping.NextId, type), self);

                case ElementAccessExpressionSyntax elementAccess:
                    return mapping.Add(() =>
                        new PlatoElementGet(mapping.NextId, type, elementAccess.Expression.ToPlato(mapping), 
                            elementAccess.ArgumentList.Arguments.Count != 1
                                ? throw self.NotSupported()
                                : elementAccess.ArgumentList.Arguments[0].Expression.ToPlato(mapping)), self);

                case ElementBindingExpressionSyntax elementBinding: 
                    throw self.NotSupported();

                case ImplicitArrayCreationExpressionSyntax implicitArrayCreation: 
                    throw self.NotSupported();

                case ImplicitElementAccessSyntax implicitElementAccess: 
                    throw self.NotSupported();

                case InitializerExpressionSyntax initializer: 
                    throw self.NotSupported();

                case InterpolatedStringExpressionSyntax interpolatedString:
                    return mapping.Add(() => new PlatoInterpolation(mapping.NextId, interpolatedString.Contents.Select(x => x.ToPlato(mapping))), self);

                case InvocationExpressionSyntax invocation:
                    return mapping.Add(() => invocation.ToPlato(type, mapping), self);

                case IsPatternExpressionSyntax isPattern: 
                    throw self.NotSupported();

                case LiteralExpressionSyntax literal:
                    return mapping.Add(() => new PlatoLiteral(mapping.NextId, type, literal.Token), self);
                
                case MakeRefExpressionSyntax makeRef: 
                    throw self.NotSupported();
                
                case MemberAccessExpressionSyntax memberAccess:
                    return mapping.Add(() =>
                        new PlatoMemberGet(mapping.NextId, type, memberAccess.Expression.ToPlato(mapping), memberAccess.Name.ToString()), self);
                
                case MemberBindingExpressionSyntax memberBinding: 
                    throw self.NotSupported();
                
                case OmittedArraySizeExpressionSyntax omittedArraySize: 
                    throw self.NotSupported();

                case ParenthesizedLambdaExpressionSyntax lambda:
                    return mapping.Add(() =>
                        new PlatoLambda(mapping.NextId, type, lambda.ParameterList.ToPlato(mapping.Model.GetSymbolInfo(lambda).Symbol as IMethodSymbol, mapping),
                        lambda.Block?.ToPlato(mapping) ?? lambda.ExpressionBody.ToPlatoBlockStatement(mapping)), self);

                case SimpleLambdaExpressionSyntax lambda:
                    return mapping.Add(() =>
                        new PlatoLambda(mapping.NextId, type, new PlatoParameterList(mapping.NextId),
                            lambda.Block?.ToPlato(mapping) ?? lambda.ExpressionBody.ToPlatoBlockStatement(mapping)), self);

                case ParenthesizedExpressionSyntax parenthesized:
                    return mapping.Add(
                        () => new PlatoParenthesis(mapping.NextId, parenthesized.Expression.ToPlato(mapping)), self);
                
                case PostfixUnaryExpressionSyntax postfix:
                    return mapping.Add(() => new PlatoPostfix(mapping.NextId, type, postfix.OperatorToken.Text,
                        postfix.Operand.ToPlato(mapping)), self);
                    
                case PrefixUnaryExpressionSyntax prefix:
                    return mapping.Add(() =>
                        new PlatoPostfix(mapping.NextId, type, prefix.OperatorToken.Text,
                            prefix.Operand.ToPlato(mapping)), self);

                case RangeExpressionSyntax rangeExpression: 
                    throw self.NotSupported();
                
                case SwitchExpressionSyntax switchExpression: 
                    throw self.NotSupported();

                case ThisExpressionSyntax thisExpression:
                    return mapping.Add(() => new PlatoThis(mapping.NextId, type), self);

                case ThrowExpressionSyntax throwExpression:
                    return mapping.Add(
                        () => new PlatoThrowExpression(mapping.NextId, throwExpression.Expression.ToPlato(mapping)),
                        self);

                case TupleExpressionSyntax tuple:
                    return mapping.Add(
                        () => new PlatoTuple(mapping.NextId, type,
                            tuple.Arguments.Select(arg => arg.Expression.ToPlato(mapping))), self);

                case TypeOfExpressionSyntax typeOf:
                    return mapping.Add(() => new PlatoTypeOf(mapping.NextId, type), self);

                case IdentifierNameSyntax ident:
                    return mapping.Add(() => new PlatoIdentifierRef(mapping.NextId, type, ident.Identifier.Text));

                case NameSyntax name:
                    return mapping.Add(() => new PlatoIdentifierRef(mapping.NextId, type, name.ToString()));

                // NOTE: surprisingly a NameSyntax "IS A" TypeSyntax
                case TypeSyntax typeSyntax: 
                    return mapping.Add(() => typeSyntax.ToPlato(mapping), self);
                
                case WithExpressionSyntax withExpression: 
                    throw self.NotSupported();

                case AnonymousMethodExpressionSyntax anonymousMethodExpressionSyntax:
                    throw self.NotSupported();

                case AnonymousObjectCreationExpressionSyntax anonymousObjectCreationExpressionSyntax:
                    throw self.NotSupported();

                case AwaitExpressionSyntax awaitExpressionSyntax:
                    throw self.NotSupported();

                case BaseExpressionSyntax baseExpressionSyntax:
                    return mapping.Add(() => new PlatoBase(mapping.NextId, type), self);


                case ImplicitObjectCreationExpressionSyntax implicitObjectCreationExpressionSyntax:
                case ImplicitStackAllocArrayCreationExpressionSyntax implicitStackAllocArrayCreationExpressionSyntax:
                case SizeOfExpressionSyntax sizeOfExpressionSyntax:
                case StackAllocArrayCreationExpressionSyntax stackAllocArrayCreationExpressionSyntax:
                case BaseObjectCreationExpressionSyntax baseObjectCreationExpressionSyntax:
                case InstanceExpressionSyntax instanceExpressionSyntax:
                case LambdaExpressionSyntax lambdaExpressionSyntax:
                case AnonymousFunctionExpressionSyntax anonymousFunctionExpressionSyntax:
                default: 
                    throw self.NotSupported();
            }
        }

        public static PlatoArgList ToPlato(this IEnumerable<ArgumentSyntax> self, PlatoSemanticMapping mapping)
            => mapping.Add(() => new PlatoArgList(mapping.NextId, self?.Select(x => x.ToPlato(mapping))), null);

        public static PlatoArgList ToPlato(this BracketedArgumentListSyntax self, PlatoSemanticMapping mapping)
            => self?.Arguments.ToPlato(mapping);

        public static PlatoCompoundStatement ToPlatoStatement(this IEnumerable<ExpressionSyntax> self,
            PlatoSemanticMapping mapping)
            => mapping.Add(() => new PlatoCompoundStatement(mapping.NextId, self.Select(x => x.ToPlatoStatement(mapping))), null);

        public static Exception NotSupported(this SyntaxNode self)
            => new NotSupportedException(self.GetType().Name);

        public static Exception NotSupported(this ISymbol self)
            => new NotSupportedException(self.Kind.ToString());

        public static PlatoLiteral ToPlatoLiteral(this object value, PlatoSemanticMapping mapping)
            => mapping.Add(() => new PlatoLiteral(mapping.NextId, value.GetType().ToPlatoType(), value), null);

        public static PlatoExpression ToPlatoOrTrue(this ExpressionSyntax self, PlatoSemanticMapping mapping)
            => self == null ? true.ToPlatoLiteral(mapping) : self.ToPlato(mapping);

        public static PlatoCompoundStatement CreateCompoundStatement(this PlatoSemanticMapping mapping,
            params Func<PlatoStatement>[] funcs)
            => mapping.Add(() => new PlatoCompoundStatement(mapping.NextId, funcs.Select(f => mapping.Add(f))), null);

        public static PlatoBlockStatement CreateBlockStatement(this PlatoSemanticMapping mapping,
            params Func<PlatoStatement>[] funcs)
            => mapping.Add(() => new PlatoBlockStatement(mapping.NextId, funcs.Select(f => mapping.Add(f))), null);

        public static PlatoVarDeclStatement ToPlato(this VariableDeclaratorSyntax varDecl, PlatoTypeExpr type, PlatoSemanticMapping mapping)
            => mapping.Add(() => new PlatoVarDeclStatement(mapping.NextId, type, varDecl.Identifier.ToString(), 
                varDecl.Initializer?.Value.ToPlato(mapping), varDecl.ArgumentList?.ToPlato(mapping)), varDecl);

        public static PlatoStatement ToPlato(this ForStatementSyntax self, PlatoSemanticMapping mapping)
            => mapping.CreateCompoundStatement(
                () => self.Declaration.ToPlato(mapping),
                () => mapping.Add(() => new PlatoWhileStatement(mapping.NextId, self.Condition.ToPlato(mapping),
                    mapping.CreateCompoundStatement(
                        () => self.Statement.ToPlato(mapping),
                        () => self.Incrementors.ToPlatoStatement(mapping)))));

        public static PlatoStatement ToPlato(this StatementSyntax self, PlatoSemanticMapping mapping)
        {
            switch (self)
            {
                case BlockSyntax blockStatement:
                    return blockStatement.ToPlato(mapping);

                case BreakStatementSyntax breakStatement:
                    return mapping.Add(() => new PlatoBreakStatement(mapping.NextId), breakStatement);

                case CheckedStatementSyntax checkedStatement:
                    throw self.NotSupported();

                case ForEachStatementSyntax forEachStatement:
                    throw self.NotSupported();

                case ForEachVariableStatementSyntax forEachVariableStatement:
                    throw self.NotSupported();

                case DoStatementSyntax doStatement:
                    throw self.NotSupported();

                case EmptyStatementSyntax emptyStatement:
                    return mapping.Add(() => new PlatoEmptyStatement(mapping.NextId), emptyStatement);

                case ExpressionStatementSyntax expressionStatement:
                    return mapping.Add(() => new PlatoExpressionStatement(mapping.NextId, expressionStatement.Expression.ToPlato(mapping)), expressionStatement);

                case FixedStatementSyntax fixedStatement:
                    throw self.NotSupported();

                case ForStatementSyntax forStatement:
                    return forStatement.ToPlato(mapping);

                case IfStatementSyntax ifStatement:
                    return mapping.Add(() => new PlatoIfStatement(
                        mapping.NextId,
                        ifStatement.Condition.ToPlato(mapping),
                        ifStatement.Statement.ToPlato(mapping),
                        ifStatement.Else?.Statement.ToPlato(mapping)
                    ), ifStatement);

                case GotoStatementSyntax gotoStatement:
                    throw self.NotSupported();

                case LabeledStatementSyntax labeledStatement:
                    throw self.NotSupported();

                case LocalDeclarationStatementSyntax localDeclarationStatement:
                    return localDeclarationStatement.Declaration.ToPlato(mapping);

                case LocalFunctionStatementSyntax localFunction:
                    throw self.NotSupported();

                case LockStatementSyntax lockStatement:
                    throw self.NotSupported();

                case ReturnStatementSyntax returnStatement:
                    return mapping.Add(() => new PlatoReturnStatement(mapping.NextId, 
                        returnStatement.Expression?.ToPlato(mapping)
                    ), returnStatement);

                case SwitchStatementSyntax switchStatement:
                    throw self.NotSupported();

                case ThrowStatementSyntax throwStatement:
                    return mapping.Add(() =>
                        new PlatoExpressionStatement(mapping.NextId, throwStatement.Expression?.ToPlato(mapping)
                        ), throwStatement);

                case UnsafeStatementSyntax unsafeStatement:
                    throw self.NotSupported();

                case TryStatementSyntax tryStatement:
                    throw self.NotSupported();

                case UsingStatementSyntax usingStatement:
                    throw self.NotSupported();

                case WhileStatementSyntax whileStatement:
                    throw self.NotSupported();

                case YieldStatementSyntax yieldStatement:
                    throw self.NotSupported();

                default:
                    throw self.NotSupported();
            }
        }

        public static PlatoBlockStatement ToPlato(this BlockSyntax self, PlatoSemanticMapping mapping)
            => mapping.Add(() => new PlatoBlockStatement(mapping.NextId, self.Statements.Select(st => st.ToPlato(mapping))), self);

        public static PlatoParameter ToPlato(this ParameterSyntax self, PlatoSemanticMapping mapping)
            => mapping.Add(() => new PlatoParameter(mapping.NextId, self.Identifier.ToString(), self.GetPlatoType(mapping)), self);

        public static PlatoParameter ToPlato(this IParameterSymbol self, ParameterSyntax syntax, PlatoSemanticMapping mapping)
            => mapping.Add(() => new PlatoParameter(mapping.NextId, self.Name, self.Type.GetPlatoType(mapping)), syntax);

        public static PlatoParameterList ToPlato(this ParameterListSyntax self, PlatoSemanticMapping mapping)
            => mapping.Add(() =>
                new PlatoParameterList(mapping.NextId, self.Parameters.Select(x => x.ToPlato(mapping))));

        public static PlatoParameterList ToPlato(this ParameterListSyntax self, IMethodSymbol methodSymbol,
            PlatoSemanticMapping mapping)
        {
            var parameterSyntaxNodes = self.Parameters;
            var parameterSymbols = methodSymbol.Parameters;
            var parameters = parameterSymbols.Zip(parameterSyntaxNodes, (a, b) => a.ToPlato(b, mapping));
            return mapping.Add(() => new PlatoParameterList(mapping.NextId, parameters), self);
        }

        public static PlatoReturnStatement ToPlato(this ArrowExpressionClauseSyntax self, PlatoSemanticMapping mapping)
            => mapping.Add(() => new PlatoReturnStatement(mapping.NextId, self.Expression.ToPlato(mapping)));

        public static PlatoStatement ToPlato(this BlockSyntax self, ArrowExpressionClauseSyntax arrow, PlatoSemanticMapping mapping)
            => (PlatoStatement) self?.ToPlato(mapping) ?? arrow?.ToPlato(mapping);

        public static PlatoTypeParameterList ToPlato(this TypeParameterListSyntax self, PlatoSemanticMapping mapping)
            => mapping.Add(() => new PlatoTypeParameterList(mapping.NextId, self?.Parameters.Select(t => t.ToPlato(mapping))), self);

        public static PlatoFunction ToPlato(this MethodDeclarationSyntax self, PlatoSemanticMapping mapping)
            => mapping.Add(() => new PlatoFunction(mapping.NextId,
                    self.Identifier.ToString(),
                    self.ParameterList.ToPlato(mapping),
                    null,
                    self.ReturnType.ToPlato(mapping),
                    self.Body.ToPlato(self.ExpressionBody, mapping),
                    self.TypeParameterList?.ToPlato(mapping)
                ), self);

        public static PlatoProperty ToPlato(this PropertyDeclarationSyntax self, PlatoSemanticMapping mapping)
            => mapping.Add(() => new PlatoProperty(mapping.NextId, 
                    self.Identifier.ToString(),
                    self.Type.ToPlato(mapping),
                    false,
                    self.Initializer?.Value.ToPlato(mapping),
                    self.ExpressionBody?.Expression.ToPlato(mapping)
                ), self);

        public static PlatoTypeExpr ToPlato(this TypeSyntax self, PlatoSemanticMapping mapping)
            => mapping.Model.GetSymbolInfo(self).Symbol?.GetPlatoType(mapping) 
               ?? throw new Exception("Not a recognized type");

        public static PlatoVarDeclStatement ToPlato(this VariableDeclaratorSyntax self, TypeSyntax type, PlatoSemanticMapping mapping)
            => mapping.Add(() =>
                new PlatoVarDeclStatement(mapping.NextId, type.ToPlato(mapping), self.Identifier.Text, 
                    self.Initializer?.Value.ToPlato(mapping), self.ArgumentList?.Arguments.ToPlato(mapping)), self);

        // TODO: handle static (same with properties)
        public static IEnumerable<PlatoField> ToPlato(this FieldDeclarationSyntax self, PlatoSemanticMapping mapping)
            => self.Declaration.Variables.Select(
                v => mapping.Add(() =>
                    new PlatoField(mapping.NextId, v.ToPlato(self.Declaration.Type, mapping), false), self));

        public static PlatoCompoundStatement ToPlato(this VariableDeclarationSyntax self, PlatoSemanticMapping mapping)
            => mapping.Add(() => new PlatoCompoundStatement(mapping.NextId, 
                self.Variables.Select(v => v.ToPlato(self.Type.ToPlato(mapping), mapping))), self);

        public static PlatoTypeParam ToPlato(this TypeParameterSyntax self, PlatoSemanticMapping mapping)
            => mapping.Add(() => new PlatoTypeParam(mapping.NextId, self.Identifier.ToString()), self);

        public static IEnumerable<PlatoTypeExpr> GetBaseTypes(this BaseListSyntax self,
            PlatoSemanticMapping mapping)
        {
            return self == null 
                ? Enumerable.Empty<PlatoTypeExpr>() 
                : self.Types.Select(b => b.Type.ToPlato(mapping));
        }

        public static PlatoClass ToPlato(this ClassDeclarationSyntax self, PlatoSemanticMapping mapping)
            => mapping.Add(() => new PlatoClass(mapping.NextId, 
                self.Identifier.Text,
                false,
                false,
                self.GetElements<MethodDeclarationSyntax>().Select(x => x.ToPlato(mapping)),
                self.GetElements<PropertyDeclarationSyntax>().Select(x => x.ToPlato(mapping)),
                self.GetElements<FieldDeclarationSyntax>().SelectMany(x => x.ToPlato(mapping)),
                self.BaseList.GetBaseTypes(mapping),
                self.TypeParameterList?.ToPlato(mapping) ?? new PlatoTypeParameterList(mapping.NextId)
            ), self);

        public static PlatoClass ToPlato(this StructDeclarationSyntax self, PlatoSemanticMapping mapping)
            => mapping.Add(() => new PlatoClass(mapping.NextId,
                self.Identifier.Text,
                true,
                false,
                self.GetElements<MethodDeclarationSyntax>().Select(x => x.ToPlato(mapping)),
                self.GetElements<PropertyDeclarationSyntax>().Select(x => x.ToPlato(mapping)),
                self.GetElements<FieldDeclarationSyntax>().SelectMany(x => x.ToPlato(mapping)),
                self.BaseList.GetBaseTypes(mapping),
                self.TypeParameterList?.ToPlato(mapping) ?? new PlatoTypeParameterList(mapping.NextId)
            ), self);

        public static PlatoClass ToPlato(this InterfaceDeclarationSyntax self, PlatoSemanticMapping mapping)
            => mapping.Add(() => new PlatoClass(mapping.NextId,
                self.Identifier.Text,
                false,
                true,
                self.GetElements<MethodDeclarationSyntax>().Select(x => x.ToPlato(mapping)),
                self.GetElements<PropertyDeclarationSyntax>().Select(x => x.ToPlato(mapping)),
                Enumerable.Empty<PlatoField>(),
                self.BaseList.GetBaseTypes(mapping),
                self.TypeParameterList?.ToPlato(mapping) ?? new PlatoTypeParameterList(mapping.NextId)
            ), self);
    }
}

