using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FlowAnalysis;
using PlatoIR;
using TypeParameterSyntax = Microsoft.CodeAnalysis.CSharp.Syntax.TypeParameterSyntax;

namespace PlatoRoslynSyntaxAnalyzer
{
    public static class SyntaxToIR
    {
        public static IRBuilder BuildIR(this IRBuilder builder, Compilation compilation, IEnumerable<PlatoTypeSyntax> syntaxes)
        {
            var r = builder.StoreDeclarations(compilation, syntaxes);
            r = builder.CreateDefinitions(compilation, syntaxes);
            r = builder.NormalizeIR(compilation);
                return r;
        }

        public static IRBuilder NormalizeIR(this IRBuilder builder, Compilation compilation)
        {
            // TODO: remove tuples from lvalues (make them all single assignments)
            // TODO: update auto property sets
            // TODO: add default constructor calls
            // TODO: fix the type-names
            // TODO: find a way to visit sub-expressions etc. 
            return builder;
        }

        public static IRBuilder StoreDeclarations(this IRBuilder builder, Compilation compilation, IEnumerable<PlatoTypeSyntax> syntaxes)
        {
            // Store all of the declarations 
            foreach (var type in syntaxes)
            {
                var typeIr = new TypeDeclarationIR(type.Kind, type.Name);
                builder.AddIR(type, typeIr);

                foreach (var ctor in type.Ctors)
                {
                    var decl = builder.AddIR(ctor, new ConstructorDeclarationIr()).SetParent(typeIr);
                    decl.Source = ctor.Node.ToString();
                    decl.IsStatic = ctor.IsStatic;
                    typeIr.Constructors.Add(decl);
                }

                foreach (var p in type.Properties)
                {
                    var decl = builder.AddIR(p, new PropertyDeclarationIR()).SetParent(typeIr);
                    decl.Source = p.Node.ToString();
                    decl.IsStatic = p.IsStatic;
                    decl.Name = p.Name;
                    typeIr.Properties.Add(decl);
                }

                foreach (var conv in type.Converters)
                {
                    var decl = builder.AddIR(conv, new OperationDeclarationIr()).SetParent(typeIr);
                    decl.Source = conv.Node.ToString();
                    decl.IsStatic = conv.IsStatic;
                    decl.Name = "operator " + conv.Node.ImplicitOrExplicitKeyword.Text;
                    typeIr.Operations.Add(decl);
                }

                foreach (var field in type.Fields)
                {
                    foreach (var fieldVar in field.Variables)
                    {
                        var decl = builder.AddIR(fieldVar, new FieldDeclarationIR()).SetParent(typeIr);
                        decl.Source = fieldVar.Node.ToString();
                        decl.IsStatic = field.IsStatic;
                        decl.Name = fieldVar.Name;
                    }
                }

                foreach (var op in type.Operators)
                {
                    var decl = builder.AddIR(op, new OperationDeclarationIr()).SetParent(typeIr);
                    decl.Source = op.Node.ToString();
                    decl.IsStatic = op.IsStatic;
                    decl.Name = "operator " + op.Name;
                    typeIr.Operations.Add(decl);
                }

                foreach (var meth in type.Methods)
                {
                    var decl = builder.AddIR(meth, new MethodDeclarationIR()).SetParent(typeIr);
                    decl.Source = meth.Node.ToString();

                    foreach (var tp in meth.Node.TypeParameterList?.Parameters ?? Enumerable.Empty<TypeParameterSyntax>())
                    {
                        var declTp = builder.AddIR(tp, new TypeParameterDeclarationIR());
                        declTp.SetParent(decl);
                        declTp.Name = tp.Identifier.ToString();
                        decl.TypeParameters.Add(declTp);
                    }

                    decl.IsStatic = meth.IsStatic;
                    decl.Name = meth.Name;
                    typeIr.Methods.Add(decl);
                }

                foreach (var idx in type.Indexers)
                {
                    var decl = builder.AddIR(idx, new IndexerDeclarationIr()).SetParent(typeIr);
                    decl.Source = idx.Node.ToString();
                    decl.IsStatic = idx.IsStatic;
                    typeIr.Indexers.Add(decl);
                }

                foreach (var tp in type.TypeParameters)
                {
                    var decl = builder.AddIR(tp, new TypeParameterDeclarationIR()).SetParent(typeIr);
                    decl.Name = tp.Node.Identifier.ToString();
                    typeIr.TypeParameters.Add(decl);
                }
            }
            return builder;
        }

        public static IRBuilder CreateDefinitions(this IRBuilder builder, Compilation compilation, IEnumerable<PlatoTypeSyntax> syntaxes)
        { 
            // Create all of the definitions. Note: there are more declarations added as we go, but those are only local.   
            foreach (var kv in builder.Declarations.ToArray())
            {
                var node = kv.Item1;
                var decl = kv.Item2;

                var model = compilation.GetSemanticModel(node.SyntaxTree);

                switch (decl)
                {
                    case ConstructorDeclarationIr constructorIr:
                    {
                        var syntax = (ConstructorDeclarationSyntax)node;
                        UpdateMethod(constructorIr, syntax, model, builder);
                        break;
                    }
                    
                    case FieldDeclarationIR fieldIr:
                    {
                        var syntax = new PlatoVariableSyntax((VariableDeclaratorSyntax)node);
                        fieldIr.Type = syntax.Type.Node.ToReference(model, builder);
                        fieldIr.InitialValue = syntax.Initializer?.Node.ToIR(model, builder);
                        break;
                    }

                    case IndexerDeclarationIr indexerIr:
                    {
                        var syntax = node as IndexerDeclarationSyntax;
                        indexerIr.Type = syntax.Type.ToReference(model, builder);
                        indexerIr.Getter = CreateGetter(syntax, model, builder);
                        break;
                    }

                    case OperationDeclarationIr operationIR:
                    {
                        if (node is OperatorDeclarationSyntax operatorSyntax)
                        {
                            UpdateMethod(operationIR, operatorSyntax, model, builder);
                        }

                        if (node is ConversionOperatorDeclarationSyntax converterSyntax)
                        {
                            UpdateMethod(operationIR, converterSyntax, model, builder);
                        }

                        break;
                    }

                    case MethodDeclarationIR methodIr:
                    {
                        var syntax = (MethodDeclarationSyntax)node;
                        UpdateMethod(methodIr, syntax, model, builder);
                        break;
                    }

                    case PropertyDeclarationIR propertyIr:
                    {
                        var syntax = (PropertyDeclarationSyntax)node;
                        propertyIr.Type = syntax.Type.ToReference(model, builder);
                        propertyIr.Getter = CreateGetter(syntax, model, builder);
                        if (propertyIr.Getter.Body == null)
                        {
                            var typeIr = propertyIr.Parent as TypeDeclarationIR;
                            if (typeIr != null && typeIr.Kind == "class")
                            {
                                var fieldName = "_" + propertyIr.Name + "_";
                                var fieldIr = new FieldDeclarationIR() { Name = fieldName };
                                builder.AddIR(null as SyntaxNode, fieldIr);
                                fieldIr.Parent = typeIr;
                                fieldIr.Type = propertyIr.Type;
                                typeIr.Fields.Add(fieldIr);
                                var fieldRef = new FieldReferenceIR(fieldName, fieldIr);
                                var retStatement = new ReturnStatementIR(fieldRef);
                                propertyIr.Getter.Body = new BlockStatementIR(retStatement);
                            }
                        }
                        break;
                    }

                    case TypeDeclarationIR typeDeclarationIr:
                        // No additional procesing required
                        break;

                    case TypeParameterDeclarationIR typeParameterDeclarationIr:
                        // No additional procesing required
                        break;

                    case VariableDeclarationIR variableDeclarationIr:
                        throw new Exception("internal error, there should be no variable declarations yet");

                    default:
                        throw new ArgumentOutOfRangeException(nameof(decl));
                }
            }

            return builder; 
        }

        private static MethodDeclarationIR CreateGetter(BasePropertyDeclarationSyntax property, SemanticModel model, IRBuilder builder)
        {
            if (property == null) return null;

            var returnType = property.Type.ToReference(model, builder);
            var symbol = model.GetSymbolInfo(property).Symbol;

            var r = new MethodDeclarationIR()
            {
                Name = $"get_{symbol?.Name}",
                Type = returnType,
            };

            // Find the body of the propertyDeclaration 
            if (property is PropertyDeclarationSyntax ps && ps.ExpressionBody != null)
            {
                r.Body = new BlockStatementIR(ps.ExpressionBody?.Expression.ToIR(model, builder).ToReturnStatementIR());
            }

            if (property.AccessorList != null)
            {
                foreach (var acc in property.AccessorList?.Accessors)
                {
                    if (r.Body == null && acc.Kind() == SyntaxKind.GetAccessorDeclaration)
                    {
                        // Only if there is an actual body.
                        if (acc.ExpressionBody?.Expression != null)
                        {
                            r.Body = new BlockStatementIR(acc.ExpressionBody?.Expression.ToIR(model, builder).ToReturnStatementIR());
                        }
                        else if (acc.Body != null)
                        {
                            r.Body = (BlockStatementIR)acc.Body.ToIR(model, builder);
                        }
                    }
                }
            }

            if (property is IndexerDeclarationSyntax indexer)
            {
                r.Parameters = indexer.ParameterList.Parameters.Select(p => p.ToIR(model, builder)).ToList();
            }

            return r;
        }

        static IFieldSymbol GetAutoPropertyField(IPropertySymbol propertySymbol)
            => propertySymbol
                ?.ContainingType
                .GetMembers()
                .OfType<IFieldSymbol>()
                .FirstOrDefault(field => SymbolEqualityComparer.Default.Equals(field.AssociatedSymbol, propertySymbol));        

        public static MethodDeclarationIR UpdateMethod(MethodDeclarationIR methodDeclarationIr, BaseMethodDeclarationSyntax syntax, SemanticModel model, IRBuilder builder)
        {
            var block = syntax.Body;
            var expression = syntax.ExpressionBody?.Expression;
            methodDeclarationIr.Parameters = syntax.ParameterList.Parameters.Select(p => p.ToIR(model, builder)).ToList();
            methodDeclarationIr.Body = CreateStatementBody(block, expression, model, builder);
            
            return methodDeclarationIr;
        }
        
        public static MethodDeclarationIR UpdateMethod(OperationDeclarationIr operatorIr, ConversionOperatorDeclarationSyntax syntax, SemanticModel model, IRBuilder builder)
        {
            UpdateMethod(operatorIr, (BaseMethodDeclarationSyntax)syntax, model, builder);
            operatorIr.Type = syntax.Type.ToReference(model, builder);
            return operatorIr;
        }

        public static MethodDeclarationIR UpdateMethod(OperationDeclarationIr operatorIr, OperatorDeclarationSyntax syntax, SemanticModel model, IRBuilder builder)
        {
            UpdateMethod(operatorIr, (BaseMethodDeclarationSyntax)syntax, model, builder);
            operatorIr.Type = syntax.ReturnType.ToReference(model, builder);
            return operatorIr;
        }

        public static MethodDeclarationIR UpdateMethod(MethodDeclarationIR methodDeclarationIr, MethodDeclarationSyntax syntax, SemanticModel model, IRBuilder builder)
        {
            UpdateMethod(methodDeclarationIr, (BaseMethodDeclarationSyntax)syntax, model, builder);
            methodDeclarationIr.Type = syntax.ReturnType.ToReference(model, builder);
            return methodDeclarationIr;
        }

        public static BlockStatementIR CreateStatementBody(BlockSyntax block, ExpressionSyntax expression, SemanticModel model, IRBuilder builder)
        {
            if (block != null)
            {
                if (expression != null)
                    throw new Exception("internal error: both statement and expression body can't be non-null");
                return (BlockStatementIR)block.ToIR(model, builder);
            }

            if (expression == null)
                throw new Exception("internal error: both statement and expression body can't be null");
            return new BlockStatementIR(expression.ToIR(model, builder).ToReturnStatementIR());
        }

        public static VariableDeclarationIR ToDeclarationIR(this VariableDeclaratorSyntax syntax, TypeReferenceIR type, SemanticModel model,
            IRBuilder builder)
        {
            return builder.AddIR(syntax, new VariableDeclarationIR()
            {
                Name = syntax.Identifier.ToString(),
                InitialValue = syntax.Initializer?.Value.ToIR(model, builder),
                Type = type,
            });
        }

        public static MultiStatementIR ToStatementIR(this VariableDeclarationSyntax syntax, SemanticModel model,
            IRBuilder builder)
            => syntax.ToStatementsIR(model, builder).ToMultiStatementIR();

        public static IEnumerable<StatementIR> ToStatementsIR(this VariableDeclarationSyntax syntax,
            SemanticModel model, IRBuilder builder)
            => syntax.ToDeclarationsIR(model, builder).Select(decl => decl.ToStatementIR());

        public static IEnumerable<VariableDeclarationIR> ToDeclarationsIR(this VariableDeclarationSyntax syntax,
            SemanticModel model, IRBuilder builder)
        {
            var type = syntax.Type.ToReference(model, builder);
            return syntax.Variables.Select(v => v.ToDeclarationIR(type, model, builder));
        }

        public static MultiStatementIR ToMultiStatementIR(this IEnumerable<StatementIR> children)
            => new MultiStatementIR(children);

        public static BlockStatementIR ToBlockStatementIR(this IEnumerable<StatementIR> children)
            => new BlockStatementIR(children);

        public static DeclarationStatementIR ToStatementIR(this DeclarationIR declaration)
            => new DeclarationStatementIR(declaration);

        public static ReturnStatementIR ToReturnStatementIR(this ExpressionIR expression)
            => new ReturnStatementIR(expression);

        public static ExpressionStatementIR ToExpressionStatementIR(this ExpressionIR expression)
            => new ExpressionStatementIR(expression);

        public static ThrowIR ToThrowIR(this ExpressionIR expression)
            => new ThrowIR() { Args = new List<ExpressionIR>() { expression } };

        public static StatementIR ToIR(this StatementSyntax syntax, SemanticModel model, IRBuilder builder)
        {
            StatementIR r = null;

            switch (syntax)
            {
                // Supported statements
                case BlockSyntax blockSyntax:
                    r = blockSyntax.Statements.Select(st => st.ToIR(model, builder)).ToBlockStatementIR();
                    break;

                case DoStatementSyntax doStatementSyntax:
                    r = new DoStatementIR
                    (
                        doStatementSyntax.Condition.ToIR(model, builder),
                        doStatementSyntax.Statement.ToIR(model, builder)
                    );
                    break;

                case ExpressionStatementSyntax expressionStatementSyntax:
                    r = expressionStatementSyntax.Expression.ToIR(model, builder).ToExpressionStatementIR();
                    break;

                case ForStatementSyntax forStatementSyntax:
                    r = new BlockStatementIR
                    (
                        forStatementSyntax.Declaration.ToStatementIR(model, builder),
                        forStatementSyntax.Initializers
                            .Select(x => x.ToIR(model, builder).ToExpressionStatementIR()).ToMultiStatementIR(),
                        new WhileStatementIR
                        (
                            forStatementSyntax.Condition.ToIR(model, builder),
                            new BlockStatementIR
                            (
                                forStatementSyntax.Statement.ToIR(model, builder),
                                forStatementSyntax.Incrementors
                                    .Select(x => x.ToIR(model, builder).ToExpressionStatementIR()).ToMultiStatementIR()
                            )
                        )
                    );
                    break;

                case WhileStatementSyntax whileStatementSyntax:
                    r = new WhileStatementIR
                    (
                        whileStatementSyntax.Condition.ToIR(model, builder),
                        whileStatementSyntax.Statement.ToIR(model, builder)
                    );
                    break;
                
                case IfStatementSyntax ifStatementSyntax:
                    r = new IfStatementIR
                    (
                        ifStatementSyntax.Condition.ToIR(model, builder),
                        ifStatementSyntax.Statement.ToIR(model, builder),
                        ifStatementSyntax.Else?.Statement.ToIR(model, builder)
                    );
                    break;

                case ReturnStatementSyntax returnStatementSyntax:
                    r = returnStatementSyntax.Expression.ToIR(model, builder).ToReturnStatementIR();
                    break;
                
                case LocalDeclarationStatementSyntax localDeclarationStatementSyntax:
                    r = localDeclarationStatementSyntax.Declaration.ToStatementIR(model, builder);
                    break;

                case ThrowStatementSyntax throwStatementSyntax:
                    r = throwStatementSyntax.Expression.ToIR(model, builder).ToThrowIR().ToExpressionStatementIR();
                    break;

                // Unsupported statements
                case LocalFunctionStatementSyntax localFunctionStatementSyntax:
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
                default:
                    SemanticRules.UnsupportedSyntax(syntax);
                    break;
            }

            if (r == null)
                throw new Exception($"internal error");
            return r;
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

        public static TypeReferenceIR ToReference(this TypeSyntax syntax, SemanticModel model, IRBuilder builder)
            => model.GetSymbolInfo(syntax).Symbol.GetTypeReference(builder);

        public static IEnumerable<TypeReferenceIR> GetTypeArguments(this ISymbol symbol, IRBuilder builder)
            => symbol is INamedTypeSymbol nts ? nts.TypeArguments.Select(arg => arg.GetTypeReference(builder))
                : symbol is IMethodSymbol ms ? ms.TypeArguments.Select(arg => arg.GetTypeReference(builder))
            : Enumerable.Empty<TypeReferenceIR>();

        public static TypeReferenceIR GetTypeReference(this ISymbol symbol, IRBuilder builder)
            => symbol is ITypeParameterSymbol tps 
                ? new TypeReferenceIR(tps.Name, builder.GetDeclarationIR<TypeParameterDeclarationIR>(tps))
                : new TypeReferenceIR(symbol?.Name, builder.GetDeclarationIR<TypeDeclarationIR>(symbol), symbol.GetTypeArguments(builder));

        public static MethodReferenceIR GetFunctionReference(this ExpressionSyntax syntax, SemanticModel model, IRBuilder builder)
            => model.GetSymbolInfo(syntax).Symbol.GetFunctionReference(builder);

        public static MethodReferenceIR GetFunctionReference(this ISymbol symbol, IRBuilder builder)
            => new MethodReferenceIR(symbol?.Name, builder.GetDeclarationIR<MethodDeclarationIR>(symbol), symbol.GetTypeArguments(builder));

        // TODO: maybe add some checks about what can be assigned to.
        public static ExpressionIR GetLValue(this ExpressionSyntax syntax, SemanticModel model, IRBuilder builder)
            => syntax.ToIR(model, builder);

        public static ArgumentIR ToIR(this ArgumentSyntax syntax, SemanticModel model, IRBuilder builder)
            => new ArgumentIR
            (
                syntax.NameColon?.Name.ToString(),
                syntax.Expression.ToIR(model, builder)
            );

        public static ExpressionIR ToIR(this PatternSyntax syntax, SemanticModel model, IRBuilder builder)
        {
            switch (syntax)
            {
                case DiscardPatternSyntax discardPatternSyntax:
                    return new DiscardIR();
                case ConstantPatternSyntax constantPatternSyntax:
                    return constantPatternSyntax.Expression.ToIR(model, builder);
                case ParenthesizedPatternSyntax parenthesizedPatternSyntax:
                    return parenthesizedPatternSyntax.Pattern.ToIR(model, builder);

                case BinaryPatternSyntax binaryPatternSyntax:
                case DeclarationPatternSyntax declarationPatternSyntax:
                case ListPatternSyntax listPatternSyntax:
                case RecursivePatternSyntax recursivePatternSyntax:
                case RelationalPatternSyntax relationalPatternSyntax:
                case SlicePatternSyntax slicePatternSyntax:
                case TypePatternSyntax typePatternSyntax:
                case UnaryPatternSyntax unaryPatternSyntax:
                case VarPatternSyntax varPatternSyntax:
                default:
                    SemanticRules.UnsupportedSyntax(syntax);
                    break;
            }

            return null;
        }

        public static ExpressionIR ToIR(this ExpressionSyntax syntax, SemanticModel model, IRBuilder builder)
        {
            var type = ModelExtensions.GetTypeInfo(model, syntax);
            var originalType = type.Type;
            var finalType = type.ConvertedType;
            var originalTypeIR = originalType?.GetTypeReference(builder);
            var finalTypeIR = finalType?.GetTypeReference(builder);
            
            var conversion = originalType != null && finalType != null
                ? model.Compilation.ClassifyConversion(originalType, finalType)
                : new Conversion();

            //var conversionFunction = conversion.MethodSymbol.GetFunctionReference(builder);

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
                    // TODO: this could be tuples, and should be broken up into multiple assignments.
                    r = new AssignmentIR() { LValue = GetLValue(assignment.Left, model, builder), };
                    r.Args.Add(assignment.Right.ToIR(model, builder));
                    break;

                case ObjectCreationExpressionSyntax objectCreation:
                    r = new NewIR() { CreatedType = objectCreation.Type.ToReference(model, builder) };
                    if (objectCreation.Initializer != null)
                        SemanticRules.UnsupportedSyntax(objectCreation.Initializer);
                    r.AddArguments(objectCreation.ArgumentList?.Arguments, model, builder);
                    break;

                case BinaryExpressionSyntax binary:
                    r = new BinaryOperatorIr() { Operator = binary.OperatorToken.ToString() };
                    r.Args.Add(binary.Left.ToIR(model, builder));
                    r.Args.Add(binary.Right.ToIR(model, builder));
                    break;

                case CastExpressionSyntax castExpression:
                    r = new CastIR() { CastType = ToReference(castExpression.Type, model, builder) };
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
                    r = new DefaultIR() { DefaultType = defaultExpression.Type.ToReference(model, builder) };
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
                        r = new DefaultIR() { DefaultType = finalTypeIR };
                    }
                    else
                    {
                        r = new LiteralIR() { Text = literal.Token.ToString(), Value = literal.Token.Value };
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
                    r = CreateLambda(syntax, model, builder, parenthesizedlambda.ParameterList.Parameters, parenthesizedlambda.ExpressionBody, parenthesizedlambda.Block);
                    break;

                case SimpleLambdaExpressionSyntax simpleLambda:
                    r = CreateLambda(syntax, model, builder, new[] { simpleLambda.Parameter }, simpleLambda.ExpressionBody, simpleLambda.Block);
                    break;

                case LambdaExpressionSyntax lambdaExpressionSyntax:
                    // Covered by ParenthesizedLambdaExpressionSyntax and SimpleLambdaExpressionSyntax
                    SemanticRules.UnsupportedSyntax(lambdaExpressionSyntax);
                    break;

                case AnonymousFunctionExpressionSyntax anonymousFunctionExpressionSyntax:
                    SemanticRules.UnsupportedSyntax(anonymousFunctionExpressionSyntax);
                    break;

                case ParenthesizedExpressionSyntax parenthesized:
                    r = new ParenthesizedIR();
                    r.Args.Add(parenthesized.Expression.ToIR(model, builder));
                    break;

                case PostfixUnaryExpressionSyntax postfix:
                    r = new PostfixOperatorIr() { Operator = postfix.OperatorToken.ToString() };
                    r.Args.Add(postfix.Operand.ToIR(model, builder));
                    break;

                case PrefixUnaryExpressionSyntax prefix:
                    r = new PrefixOperatorIr() { Operator = prefix.OperatorToken.ToString() };
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
                        r.Args.Add(arm.Pattern.ToIR(model, builder));
                        r.Args.Add(arm.Expression.ToIR(model, builder));
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
                    r = new TypeOfIR() { TypeArgument = typeOf.Type.ToReference(model, builder) };
                    break;

                case TypeSyntax typeSyntax:
                    r = typeSyntax.ToReference(model, builder);
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
                    r = new NewIR() { CreatedType = finalTypeIR };
                    if (implicitObjectCreationExpressionSyntax.Initializer != null)
                        SemanticRules.UnsupportedSyntax(implicitObjectCreationExpressionSyntax.Initializer);
                    r.AddArguments(implicitObjectCreationExpressionSyntax.ArgumentList?.Arguments, model, builder);
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
                if (symbol != null)
                {
                    if (symbol is IDiscardSymbol)
                    {
                        r = new DiscardIR();
                    }
                    else
                    {
                        if (symbol.Name != nameIR.Name)
                            throw new Exception($"symbol name {symbol.Name} does not match syntax name {nameIR.Name}");
                        nameIR.ReferencedIR = symbol.ToDeclarationIR(builder);
                    }
                }
            }

            return r;
        }

        public static DeclarationIR ToDeclarationIR(this ISymbol symbol, IRBuilder builder)
        {
            switch (symbol)
            {
                case null:
                    break;

                case IArrayTypeSymbol arrayTypeSymbol:
                    throw new Exception(@"Not supported : {nameof(arrayTypeSymbol)}");

                case IFieldSymbol fieldSymbol:
                    return builder.GetDeclarationIR<FieldDeclarationIR>(fieldSymbol);

                case ILocalSymbol localSymbol:
                    return builder.GetDeclarationIR<VariableDeclarationIR>(localSymbol);

                case IMethodSymbol methodSymbol:
                    return builder.GetDeclarationIR<MethodDeclarationIR>(methodSymbol);

                case INamedTypeSymbol namedTypeSymbol:
                    return builder.GetDeclarationIR<TypeDeclarationIR>(namedTypeSymbol);

                case INamespaceSymbol namespaceSymbol:
                    break;

                case ITypeParameterSymbol typeParameterSymbol:
                    return builder.GetDeclarationIR<TypeParameterDeclarationIR>(typeParameterSymbol);

                case ITypeSymbol typeSymbol:
                    return builder.GetDeclarationIR<TypeDeclarationIR>(typeSymbol);

                case IParameterSymbol parameterSymbol:
                    return builder.GetDeclarationIR<ParameterDeclarationIR>(parameterSymbol);

                case IPropertySymbol propertySymbol:
                    return builder.GetDeclarationIR<PropertyDeclarationIR>(propertySymbol);

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

            return null;
        }

        public static ParameterDeclarationIR ToIR(this ParameterSyntax parameter, SemanticModel model, IRBuilder builder)
        {
            var r = new ParameterDeclarationIR() { Name = parameter.Identifier.ToString() };
            builder.AddIR(new PlatoParamSyntax(parameter), r);
            r.DefaultValue = parameter.Default?.Value.ToIR(model, builder);
            r.Type = parameter.Type?.ToReference(model, builder);
            return r;
        }

        public static LambdaIR CreateLambda(SyntaxNode node, SemanticModel model, IRBuilder builder,
            IEnumerable<ParameterSyntax> parameters, ExpressionSyntax body, BlockSyntax block)
            => CreateLambda(node, model, builder, parameters.Select(p => p.ToIR(model, builder)).ToList(),
                CreateStatementBody(block, body, model, builder));

        public static LambdaIR CreateLambda(SyntaxNode node, SemanticModel model, IRBuilder builder, IEnumerable<ParameterDeclarationIR> parameters, StatementIR body)
        {
            var dataFlow = model.AnalyzeDataFlow(node);
            var capVars = dataFlow.Captured.Select(sym => sym.ToDeclarationIR(builder)).ToList();

            return new LambdaIR()
            {
                Parameters = parameters.ToList(),
                CapturedVariables = capVars,
                Body = body,
            };
        }

        public static string TypeToKeyword(string s)
        {
            switch (s)
            {
                case "Boolean": return "bool";
                case "Byte": return "byte";
                case "SByte": return "sbyte";
                case "Char": return "char";
                case "Decimal": return "decimal";
                case "Double": return "double";
                case "Single": return "float";
                case "Int32": return "int";
                case "UInt32": return "uint";
                case "IntPtr": return "nint";
                case "UIntPtr": return "nuint";
                case "Int64": return "long";
                case "UInt64": return "ulong";
                case "Int16": return "short";
                case "UInt16": return "ushort";
                case "Void": return "void";
                case "String": return "string";
                default: return s;
            }
        }
    }
}
