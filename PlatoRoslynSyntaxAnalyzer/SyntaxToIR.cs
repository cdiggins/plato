using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            => builder.StoreDeclarations(compilation, syntaxes)
                .CreateDefinitions(compilation, syntaxes)
                .NormalizeIR(compilation);

        public static IRBuilder NormalizeIR(this IRBuilder builder, Compilation compilation)
        {
            // Add generated fields 
            foreach (var t in builder.GetTypes())
            {
                foreach (var p in t.Properties)
                {
                    var fieldDecl = p.Field?.FieldDeclaration;
                    if (fieldDecl != null)
                        t.Fields.Add(fieldDecl);
                }
            }

            // TODO: remove tuples from lvalues (make them all single assignments)
            // TODO: add default constructor calls
            // TODO: fix the type-names
            return builder;
        }

        public static IRBuilder StoreDeclarations(this IRBuilder builder, Compilation compilation, IEnumerable<PlatoTypeSyntax> syntaxes)
        {
            // Store all of the declarations 
            foreach (var type in syntaxes)
            {
                var typeIr = new TypeDeclarationIR(type.Kind, type.Name);
                typeIr.IsStatic = type.Node.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword));
                builder.AddIR(type, typeIr);

                foreach (var ctor in type.Ctors)
                {
                    var decl = builder.AddIR(ctor, new ConstructorDeclarationIr());
                    decl.Source = ctor.Node.ToString();
                    decl.Name = type.Name;
                    decl.IsStatic = ctor.IsStatic;
                    typeIr.Constructors.Add(decl);
                }

                foreach (var p in type.Properties)
                {
                    var decl = builder.AddIR(p, new PropertyDeclarationIR() { TypeKind = type.Kind });
                    decl.Source = p.Node.ToString();
                    decl.IsStatic = p.IsStatic;
                    decl.Name = p.Name;
                    typeIr.Properties.Add(decl);
                }

                foreach (var conv in type.Converters)
                {
                    var decl = builder.AddIR(conv, new OperationDeclarationIR());
                    decl.Source = conv.Node.ToString();
                    decl.IsStatic = conv.IsStatic;
                    decl.Name = conv.Node.ImplicitOrExplicitKeyword.Text + " operator";
                    typeIr.Operations.Add(decl);
                }

                foreach (var field in type.Fields)
                {
                    foreach (var fieldVar in field.Variables)
                    {
                        var decl = builder.AddIR(fieldVar, new FieldDeclarationIR());
                        decl.Source = fieldVar.Node.ToString();
                        decl.IsStatic = field.IsStatic;
                        decl.Name = fieldVar.Name;
                        typeIr.Fields.Add(decl);
                    }
                }

                foreach (var op in type.Operators)
                {
                    var decl = builder.AddIR(op, new OperationDeclarationIR());
                    decl.Source = op.Node.ToString();
                    decl.IsStatic = op.IsStatic;
                    decl.Name = "operator " + op.Name;
                    typeIr.Operations.Add(decl);
                }

                foreach (var meth in type.Methods)
                {
                    var decl = builder.AddIR(meth, new MethodDeclarationIR());
                    decl.Source = meth.Node.ToString();

                    foreach (var tp in meth.Node.TypeParameterList?.Parameters ?? Enumerable.Empty<TypeParameterSyntax>())
                    {
                        var declTp = builder.AddIR(tp, new TypeParameterDeclarationIR());
                        declTp.Name = tp.Identifier.ToString();
                        decl.TypeParameters.Add(declTp);
                    }

                    decl.IsStatic = meth.IsStatic;
                    decl.Name = meth.Name;
                    typeIr.Methods.Add(decl);
                }

                foreach (var idx in type.Indexers)
                {
                    var decl = builder.AddIR(idx, new IndexerDeclarationIR());
                    decl.Source = idx.Node.ToString();
                    decl.IsStatic = idx.IsStatic;
                    typeIr.Indexers.Add(decl);
                }

                foreach (var tp in type.TypeParameters)
                {
                    var decl = builder.AddIR(tp, new TypeParameterDeclarationIR());
                    decl.Name = tp.Node.Identifier.ToString();
                    typeIr.TypeParameters.Add(decl);
                }
            }
            return builder;
        }

        public static IRBuilder CreateDefinitions(this IRBuilder builder, Compilation compilation, IEnumerable<PlatoTypeSyntax> syntaxes)
        { 
            // Create all of the definitions. Note: there are more declarations added as we go, but those are only local.   
            foreach (var kv in builder.Declarations.Values.ToArray())
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

                    case IndexerDeclarationIR indexerIr:
                    {
                        var syntax = node as IndexerDeclarationSyntax;
                        indexerIr.Type = syntax.Type.ToReference(model, builder);
                        indexerIr.Getter = CreateGetter(syntax, model, builder);
                        break;
                    }

                    case OperationDeclarationIR operationIR:
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
                            // TODO: figure out a better way to communicate this 
                            if (propertyIr.TypeKind == "class")
                            {
                                var fieldName = "_" + propertyIr.Name + "_";
                                // The field is added during the "Normalization" phase 
                                var fieldIr = new FieldDeclarationIR { Name = fieldName };
                                builder.AddIR(null as SyntaxNode, fieldIr);
                                fieldIr.Type = propertyIr.Type.Clone();
                                var fieldRef = fieldIr.ToReference();
                                var retStatement = new ReturnStatementIR(fieldRef);
                                propertyIr.Getter.Body = new BlockStatementIR(retStatement);
                                propertyIr.Field = fieldIr.ToReference(null);
                            }
                        }
                        break;
                    }

                    case TypeParameterDeclarationIR typeParameterDeclarationIr:
                        // No additional procesing required
                        break;

                    case TypeDeclarationIR typeDeclarationIr:
                        typeDeclarationIr.Bases = 
                            ((TypeDeclarationSyntax)node).BaseList?.Types.Select(t => t.Type.ToReference(model, builder))
                            .ToList() ?? new List<TypeReferenceIR>();
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
                r.Body = CreateStatementBody(null, ps.ExpressionBody?.Expression, model, builder, false);
            }

            // Find the body amd parameters of the indexerDeclaration 
            if (property is IndexerDeclarationSyntax indexer)
            {
                if (indexer.ExpressionBody != null)
                {
                    r.Body = CreateStatementBody(null, indexer.ExpressionBody?.Expression, model, builder, false);
                }

                r.Parameters = indexer.ParameterList.Parameters.Select(p => p.ToIR(model, builder)).ToList();
            }

            if (property.AccessorList != null)
            {
                foreach (var acc in property.AccessorList?.Accessors)
                {
                    if (r.Body == null && acc.ExpressionBody?.Expression != null && acc.Kind() == SyntaxKind.GetAccessorDeclaration)
                    {
                        r.Body = CreateStatementBody(acc.Body, acc.ExpressionBody?.Expression, model, builder, false);
                    }
                }
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
            methodDeclarationIr.Body = CreateStatementBody(block, expression, model, builder, syntax.IsVoid());
            
            return methodDeclarationIr;
        }

        public static bool IsVoid(this BaseMethodDeclarationSyntax syntax)
        {
            if (syntax is ConstructorDeclarationSyntax)
                return true;
            if (syntax is MethodDeclarationSyntax ms)
                return ms.ReturnType is PredefinedTypeSyntax predefined && predefined.Keyword.IsKind(SyntaxKind.VoidKeyword);
            return false;
        }
        
        public static MethodDeclarationIR UpdateMethod(OperationDeclarationIR operatorIr, ConversionOperatorDeclarationSyntax syntax, SemanticModel model, IRBuilder builder)
        {
            UpdateMethod(operatorIr, (BaseMethodDeclarationSyntax)syntax, model, builder);
            operatorIr.Type = syntax.Type.ToReference(model, builder);
            return operatorIr;
        }

        public static MethodDeclarationIR UpdateMethod(OperationDeclarationIR operatorIr, OperatorDeclarationSyntax syntax, SemanticModel model, IRBuilder builder)
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

        public static BlockStatementIR CreateStatementBody(BlockSyntax block, ExpressionSyntax expression, SemanticModel model, IRBuilder builder, bool isVoid)
        {
            if (block != null)
            {
                if (expression != null)
                    throw new Exception("internal error: both statement and expression body can't be non-null");
                return (BlockStatementIR)block.ToIR(model, builder);
            }

            if (expression == null)
                throw new Exception("internal error: both statement and expression body can't be null");

            var expr = expression.ToIR(model, builder);
            
            return new BlockStatementIR(isVoid
                ? expr.ToExpressionStatementIR()
                : expr.ToReturnStatementIR());
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

        public static StatementIR ToExpressionStatementIR(this ExpressionIR expression)
            => new ExpressionStatementIR(expression);

        public static ThrowIR ToThrowIR(this ExpressionIR expression)
            => new ThrowIR(expression);

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

        public static ExpressionIR[] ToIR(this IEnumerable<ArgumentSyntax> arguments,
            SemanticModel model, IRBuilder builder)
            => arguments?.Select(x => x.ToIR(model, builder) as ExpressionIR).ToArray() ?? Array.Empty<ExpressionIR>();

        public static ExpressionIR[] ToIR(this IEnumerable<ExpressionSyntax> expressions,
            SemanticModel model, IRBuilder builder)
            => expressions.Select(x => x.ToIR(model, builder)).ToArray() ?? Array.Empty<ExpressionIR>();

        public static TypeReferenceIR ToReference(this TypeSyntax syntax, SemanticModel model, IRBuilder builder)
            => model.GetSymbolInfo(syntax).Symbol.ToTypeReference(builder);

        public static IEnumerable<TypeReferenceIR> GetTypeArguments(this ISymbol symbol, IRBuilder builder)
            => symbol is INamedTypeSymbol nts 
                ? nts.TypeArguments.Select(arg => arg.ToTypeReference(builder)) : symbol is IMethodSymbol ms 
                ? ms.TypeArguments.Select(arg => arg.ToTypeReference(builder)) : symbol is IArrayTypeSymbol ats
                ? Enumerable.Repeat(ats.ElementType.ToTypeReference(builder), 1) : Enumerable.Empty<TypeReferenceIR>();

        public static ExpressionIR GetReceiver(this ISymbol symbol, IRBuilder builder)
        {
            if (symbol.ContainingType != null)
                return symbol.ContainingType.ToTypeReference(builder);

            if (symbol.ContainingNamespace != null && !symbol.ContainingNamespace.IsGlobalNamespace)
                return symbol.ContainingNamespace.ToReference(builder);

            return null;
        }

        public static NamespaceReferenceIR ToReference(this INamespaceSymbol nameSpace, IRBuilder builder)
            => new NamespaceReferenceIR(nameSpace.Name, GetReceiver(nameSpace, builder), builder.GetDeclarationIR<NamespaceDeclarationIR>(nameSpace));

        // TODO: validate
        public static TypeReferenceIR ToTypeReference(this ISymbol symbol, IRBuilder builder)
            => symbol is ITypeParameterSymbol tps 
                ? new TypeReferenceIR(tps.Name, null, builder.GetDeclarationIR<TypeParameterDeclarationIR>(tps))
                : symbol is IArrayTypeSymbol ats 
                ? new TypeReferenceIR("[]", symbol.GetReceiver(builder), builder.GetDeclarationIR<TypeDeclarationIR>(symbol), symbol.GetTypeArguments(builder))
                : new TypeReferenceIR(symbol?.Name, symbol.GetReceiver(builder), builder.GetDeclarationIR<TypeDeclarationIR>(symbol), symbol.GetTypeArguments(builder));

        // TODO: validate
        public static MethodReferenceIR GetFunctionReference(this ExpressionSyntax syntax, SemanticModel model, IRBuilder builder)
            => model.GetSymbolInfo(syntax).Symbol.GetFunctionReference(builder);

        public static MethodReferenceIR GetFunctionReference(this ISymbol symbol, IRBuilder builder)
            => new MethodReferenceIR(symbol?.Name, null, builder.GetDeclarationIR<MethodDeclarationIR>(symbol), symbol.GetTypeArguments(builder));

        // TODO: maybe add some checks about what can be assigned to.
        public static ExpressionIR GetLValue(this ExpressionSyntax syntax, SemanticModel model, IRBuilder builder)
            => syntax.ToIR(model, builder);

        public static ExpressionIR ToIR(this ArgumentSyntax syntax, SemanticModel model, IRBuilder builder)
            => string.IsNullOrEmpty(syntax.NameColon?.Name.ToString())
                ? syntax.Expression.ToIR(model, builder)
                : throw SemanticRules.LabeledArgsNotSupportedException(syntax);

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
            var originalTypeIR = originalType?.ToTypeReference(builder);
            var finalTypeIR = finalType?.ToTypeReference(builder);
            
            var conversion = originalType != null && finalType != null
                ? model.Compilation.ClassifyConversion(originalType, finalType)
                : new Conversion();

            //var conversionFunction = conversion.MethodSymbol.GetFunctionReference(builder);

            // TODO: make conversion calls.
            // TODO: indexers 
            // TODO: store local declarations 

            var symbol = model.GetSymbolInfo(syntax).Symbol;

            ExpressionIR r = null;

            switch (syntax)
            {
                case IdentifierNameSyntax identifierNameSyntax:
                    r = symbol.ToReference(identifierNameSyntax.Identifier.ToString(), null, builder);
                    break;

                case SimpleNameSyntax simpleNameSyntax:
                    r = symbol.ToReference(simpleNameSyntax.Identifier.ToString(), null, builder);
                    break;

                case NameSyntax nameSyntax:
                    SemanticRules.UnsupportedSyntax(nameSyntax);
                    break;

                case ArrayCreationExpressionSyntax arrayCreation:
                    if (arrayCreation.Type.RankSpecifiers.Count != 1
                        || arrayCreation.Type.RankSpecifiers[0].Sizes.Count() != 1)
                        SemanticRules.InvalidArray(arrayCreation);
                    r = new ArrayIR(
                        arrayCreation.Type.RankSpecifiers[0].Sizes.First().ToIR(model, builder),
                        arrayCreation.Initializer?.Expressions.ToIR(model, builder));
                    break;

                case AssignmentExpressionSyntax assignment:
                {
                    var lvalue = GetLValue(assignment.Left, model, builder);
                    var rvalue = assignment.Right.ToIR(model, builder);
                    if (lvalue is TupleIR tuple)
                    {
                        // Deconstruct "(x, y, z) = tuple" 
                        // into "let tmp = tuple in (x = tmp.Item1, y = tmp.Item2, z = tmp.Item3)"
                        var newVarDecl = builder.CreateNewVar(rvalue);
                        var newRValue = builder.CreateTuple(
                            tuple.Args.Select((arg, i)
                                => builder.CreateAssignment(arg, 
                                    new FieldReferenceIR(
                                        $"Item{i+1}",
                                        builder.CreateReference(newVarDecl),
                                        null)))
                                .Cast<ExpressionIR>().
                                ToArray());
                        r = builder.CreateLet(newVarDecl, newRValue);
                    }
                    else
                    {
                        r = builder.CreateAssignment(lvalue, rvalue);
                    }
                    break;
                }

                case ObjectCreationExpressionSyntax objectCreation:
                    if (objectCreation.Initializer != null)
                        SemanticRules.UnsupportedSyntax(objectCreation.Initializer);
                    r = new NewIR(objectCreation.Type.ToReference(model, builder),
                        objectCreation.ArgumentList?.Arguments.ToIR(model, builder));
                    break;

                case BinaryExpressionSyntax binary:
                    r = new BinaryOperatorIR(binary.OperatorToken.ToString(),
                        symbol.GetFunctionReference(builder),
                        binary.Left.ToIR(model, builder),
                        binary.Right.ToIR(model, builder));
                    break;

                case CastExpressionSyntax castExpression:
                    r = new CastIR(castExpression.Type.ToReference(model, builder), 
                        castExpression.Expression.ToIR(model, builder));
                    break;

                case CheckedExpressionSyntax checkedExpression:
                    SemanticRules.UnsupportedSyntax(checkedExpression);
                    break;

                case ConditionalAccessExpressionSyntax conditionalAccess:
                    SemanticRules.UnsupportedSyntax(conditionalAccess);
                    break;

                case ConditionalExpressionSyntax conditional:
                    r = new ConditionalIR(
                        conditional.Condition.ToIR(model, builder),
                        conditional.WhenTrue.ToIR(model, builder),
                        conditional.WhenFalse.ToIR(model, builder));
                    break;

                case DeclarationExpressionSyntax declaration:
                    SemanticRules.UnsupportedSyntax(declaration);
                    break;

                case DefaultExpressionSyntax defaultExpression:
                    r = new DefaultIR(defaultExpression.Type.ToReference(model, builder));
                    break;

                case ElementAccessExpressionSyntax elementAccess:
                    if (elementAccess.ArgumentList.Arguments.Count != 1)
                        SemanticRules.OnlyOneSubscriptSupported(elementAccess);
                    r = new SubscriptIR(
                        elementAccess.Expression.ToIR(model, builder),
                        elementAccess.ArgumentList.Arguments[0].ToIR(model, builder));
                    break;

                case ElementBindingExpressionSyntax elementBinding:
                    SemanticRules.UnsupportedSyntax(elementBinding);
                    break;

                case ImplicitArrayCreationExpressionSyntax implicitArrayCreation:
                    r = new ArrayIR(
                        implicitArrayCreation.Initializer.Expressions.Count.ToLiteralIR(),
                        implicitArrayCreation.Initializer.Expressions.ToIR(model, builder));
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
                    r = new InvocationIR(invocation.Expression.ToIR(model, builder),
                        invocation.ArgumentList.Arguments.ToIR(model, builder));
                    break;

                case IsPatternExpressionSyntax isPattern:
                    SemanticRules.UnsupportedSyntax(isPattern);
                    break;

                case LiteralExpressionSyntax literal:
                    if (literal.Kind() == SyntaxKind.DefaultLiteralExpression)
                    {
                        r = new DefaultIR(finalTypeIR);
                    }
                    else
                    {
                        r = new LiteralIR(literal.Token.ToString(), literal.Token.Value);
                    }
                    break;

                case MakeRefExpressionSyntax makeRef:
                    SemanticRules.UnsupportedSyntax(makeRef);
                    break;

                case MemberAccessExpressionSyntax memberAccess:
                    if (memberAccess.OperatorToken.ToString() != ".")
                        SemanticRules.OnlyDotNotationSupported(memberAccess);
                    {
                        var name = memberAccess.Name.ToString();
                        var typeArgs = new List<TypeReferenceIR>();
                        if (memberAccess.Name is GenericNameSyntax gns)
                        {
                            name = gns.Identifier.Text;
                            typeArgs = gns.TypeArgumentList.Arguments.Select(t => t.ToReference(model, builder)).ToList();
                        }
                        var refIr = symbol.ToReference(name, memberAccess.Expression.ToIR(model, builder), builder);
                        refIr.TypeArguments.AddRange(typeArgs);
                        r = refIr;
                    }
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
                    r = new ParenthesizedIR(parenthesized.Expression.ToIR(model, builder));
                    break;

                case PostfixUnaryExpressionSyntax postfix:
                    r = new PostfixOperatorIR(
                        postfix.OperatorToken.ToString(),
                        symbol.GetFunctionReference(builder),
                        postfix.Operand.ToIR(model, builder));
                    break;

                case PrefixUnaryExpressionSyntax prefix:
                    r = new PrefixOperatorIR(
                        prefix.OperatorToken.ToString(),
                        symbol.GetFunctionReference(builder),
                        prefix.Operand.ToIR(model, builder));
                    break;

                case RangeExpressionSyntax rangeExpression:
                    SemanticRules.UnsupportedSyntax(rangeExpression);
                    break;

                case SwitchExpressionSyntax switchExpression:
                    r = new SwitchIR(switchExpression.GoverningExpression.ToIR(model, builder));
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
                    r = new ThrowIR(throwExpression.Expression.ToIR(model, builder));
                    break;

                case TupleExpressionSyntax tuple:
                    r = new TupleIR(tuple.Arguments.ToIR(model, builder));
                    break;

                case TypeOfExpressionSyntax typeOf:
                    r = new TypeOfIR(typeOf.Type.ToReference(model, builder));
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
                    if (implicitObjectCreationExpressionSyntax.Initializer != null)
                        SemanticRules.UnsupportedSyntax(implicitObjectCreationExpressionSyntax.Initializer);
                    r = new NewIR(finalTypeIR,
                        implicitObjectCreationExpressionSyntax.ArgumentList?.Arguments.ToIR(model, builder));
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

            r.ExpressionType = finalTypeIR;
            return builder.AddIR(r);
        }

        public static ReferenceIR ToReference(this ISymbol symbol, string name, ExpressionIR reciever, IRBuilder builder)
        {
            if (string.IsNullOrWhiteSpace(name))
                name = symbol.Name;
            if (name != symbol.Name)
                throw new Exception($"Given name {name} does not match symbol name {symbol.Name}");

            switch (symbol)
            {
                case null:
                    break;

                case IArrayTypeSymbol arrayTypeSymbol:
                    throw new Exception(@"Not supported : {nameof(arrayTypeSymbol)}");

                case IFieldSymbol fieldSymbol:
                    {
                        var decl = builder.GetDeclarationIR<FieldDeclarationIR>(fieldSymbol);
                        if (reciever == null && !decl.IsStatic) reciever = new ThisIR();
                        return new FieldReferenceIR(name, reciever, decl);
                    }
                case ILocalSymbol localSymbol:
                    if (reciever != null) throw new Exception("Expected no reciever when accessing local variable");
                    return new VariableReferenceIR(name, builder.GetDeclarationIR<VariableDeclarationIR>(localSymbol));

                case IMethodSymbol methodSymbol:
                    {
                        var decl = builder.GetDeclarationIR<MethodDeclarationIR>(methodSymbol);
                        if (reciever == null && !decl.IsStatic) reciever = new ThisIR();
                        return new MethodReferenceIR(name, reciever, decl, methodSymbol.GetTypeArguments(builder));
                    }
                case INamedTypeSymbol namedTypeSymbol:
                    return namedTypeSymbol.ToTypeReference(builder);

                case INamespaceSymbol namespaceSymbol:
                    return namespaceSymbol.ToReference(builder);

                case ITypeParameterSymbol typeParameterSymbol:
                    return typeParameterSymbol.ToTypeReference(builder);

                case ITypeSymbol typeSymbol:
                    return typeSymbol.ToTypeReference(builder);

                case IParameterSymbol parameterSymbol:
                    if (reciever != null) throw new Exception("Expected no parameter symbol");
                    return new ParameterReferenceIR(name, builder.GetDeclarationIR<ParameterDeclarationIR>(parameterSymbol));

                case IPropertySymbol propertySymbol:
                    {
                        var decl = builder.GetDeclarationIR<PropertyDeclarationIR>(propertySymbol);
                        if (reciever == null && !decl.IsStatic) reciever = new ThisIR();
                        return new PropertyReferenceIR(name, reciever, decl);
                    }
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

            throw new Exception($"Cannot resolve symbol {symbol}");
        }

        public static ParameterDeclarationIR ToIR(this ParameterSyntax parameter, SemanticModel model, IRBuilder builder)
        {
            var r = new ParameterDeclarationIR()
            {
                Name = parameter.Identifier.ToString(),
                IsThisParameter = parameter.Modifiers.Any(m => m.IsKind(SyntaxKind.ThisKeyword))
            };
            builder.AddIR(new PlatoParamSyntax(parameter), r);
            r.DefaultValue = parameter.Default?.Value.ToIR(model, builder);
            r.Type = parameter.Type?.ToReference(model, builder);
            return r;
        }

        public static LambdaIR CreateLambda(SyntaxNode node, SemanticModel model, IRBuilder builder,
            IEnumerable<ParameterSyntax> parameters, ExpressionSyntax body, BlockSyntax block)
            => CreateLambda(node, model, builder, parameters.Select(p => p.ToIR(model, builder)).ToList(),
                CreateStatementBody(block, body, model, builder, false));

        public static LambdaIR CreateLambda(SyntaxNode node, SemanticModel model, IRBuilder builder, IEnumerable<ParameterDeclarationIR> parameters, StatementIR body)
        {
            var dataFlow = model.AnalyzeDataFlow(node);
            var capVars = dataFlow.Captured.Select(sym => sym.ToReference(null, null, builder)).ToList();

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
