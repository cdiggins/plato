using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PlatoIR;
using TypeParameterSyntax = Microsoft.CodeAnalysis.CSharp.Syntax.TypeParameterSyntax;

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

        public static void AssureBodyOrBlock(SyntaxNode syntax, bool cond)
        {
            if (!cond)
                throw new Exception($"either body or block must be present, but not both {syntax}");
        }
    }

    public static class SyntaxToIR
    {
        public static IRBuilder BuildIR(this IRBuilder builder, Compilation compilation, IEnumerable<PlatoTypeSyntax> syntaxes)
        {
            // Store all of the declarations 
            foreach (var type in syntaxes)
            {
                var ir = new TypeDeclarationIR();
                builder.AddIR(type, ir);
                foreach (var ctor in type.Ctors)
                {
                    var decl = builder.AddIR(ctor, new ConstructorIR()).SetParent(ir);
                    ir.Constructors.Add(decl);
                }

                foreach (var conv in type.Converters)
                {
                    var decl = builder.AddIR(conv, new ConverterIR()).SetParent(ir);
                    ir.Converters.Add(decl);
                }

                foreach (var field in type.Fields)
                {
                    foreach (var fieldVar in field.Variables)
                    {
                        var decl = builder.AddIR(fieldVar, new FieldIR()).SetParent(ir);
                        decl.Name = fieldVar.Name;
                    }
                }

                foreach (var op in type.Operators)
                {
                    var decl = builder.AddIR(op, new OperationIR()).SetParent(ir);
                    decl.Name = op.Name;
                    ir.Operations.Add(decl);
                }

                foreach (var meth in type.Methods)
                {
                    var decl = builder.AddIR(meth, new MethodIR()).SetParent(ir);
                    decl.Name = meth.Name;
                    ir.Methods.Add(decl);
                }

                foreach (var idx in type.Indexers)
                {
                    var decl = builder.AddIR(idx, new IndexerIR()).SetParent(ir);
                    ir.Indexers.Add(decl);
                }

                foreach (var tp in type.TypeParameters)
                {
                    var decl = builder.AddIR(tp, new TypeParameterDeclarationIR()).SetParent(ir);
                    decl.Name = tp.Node.Identifier.ToString();
                    ir.TypeParameters.Add(decl);
                }
            }

            // TODO: Create the hidden constructor

            // Create all of the definitions. Note: there are more declarations added as we go, but those are only local.   
            foreach (var kv in builder.Declarations.ToArray())
            {
                var key = kv.Key;
                var node = builder.SyntaxNodes[key];
                var decl = kv.Value;

                var model = compilation.GetSemanticModel(node.SyntaxTree);

                switch (decl)
                {
                    case ConverterIR converterIr:
                    {
                        var syntax = (ConversionOperatorDeclarationSyntax)node;
                        var returnType = syntax.Type;
                        var parameters = syntax.ParameterList.Parameters;
                        var statementBody = syntax.Body;
                        var expressionBody = syntax.ExpressionBody;
                        break;
                    }

                    case ConstructorIR constructorIr:
                    {
                        var syntax = node as PlatoConstructorSyntax;
                        var returnType = syntax.Type;
                        var parameters = syntax.ParameterList.Parameters;
                        var statementBody = syntax.Body;
                        var expressionBody = syntax.ExpressionBody;
                        break;
                    }
                    
                    case FieldIR fieldIr:
                    {
                        var syntax = new PlatoVariableSyntax((VariableDeclaratorSyntax)node);
                        fieldIr.Type = syntax.Type.Node.ToReferenceIR(model, builder);
                        fieldIr.InitialValue = syntax.Initializer.Node.ToIR(model, builder);
                        break;
                    }

                    case MethodIR methodIr:
                    {
                        var syntax = (MethodDeclarationSyntax)node;
                        methodIr.ReturnTypeDeclaration = syntax.ReturnType.ToReferenceIR(model, builder);
                        methodIr.Parameters = syntax.ParameterList.Parameters.Select(p => p.ToIR(model, builder)).ToList();
                        var statementBody = syntax.Body;
                        var expressionBody = syntax.ExpressionBody;
                        if (statementBody != null)
                        {
                            if (expressionBody != null)
                                throw new Exception("internal error: both statement and expression body can't be non-null");
                            methodIr.Body = statementBody.ToIR(model, builder);
                        }
                        else
                        {
                            if (expressionBody == null)
                                throw new Exception("internal error: both statement and expression body can't be null");
                            methodIr.Body = expressionBody.Expression.ToIR(model, builder).ToReturnStatementIR();
                        }
                        break;
                    }

                    case IndexerIR indexerIr:
                    {
                        var syntax = node as ConversionOperatorDeclarationSyntax;
                        var returnType = syntax.Type;
                        var parameters = syntax.ParameterList.Parameters;
                        var statementBody = syntax.Body;
                        var expressionBody = syntax.ExpressionBody;
                        break;
                    }

                    case PropertyIR propertyIr:
                    {
                        var syntax = (ConversionOperatorDeclarationSyntax)node;
                        var returnType = syntax.Type;
                        var parameters = syntax.ParameterList.Parameters;
                        var statementBody = syntax.Body;
                        var expressionBody = syntax.ExpressionBody;
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
            var type = syntax.Type.ToReferenceIR(model, builder);
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
                    r = new MultiStatementIR
                    (
                        forStatementSyntax.Declaration.ToStatementIR(model, builder),
                        new WhileStatementIR
                        (
                            forStatementSyntax.Condition.ToIR(model, builder),
                            new MultiStatementIR()
                        )
                    );
                    break;

                case WhileStatementSyntax whileStatementSyntax:
                    r = new WhileStatementIR(whileStatementSyntax.Condition.ToIR(model, builder),
                        whileStatementSyntax.Statement.ToIR(model, builder));
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

        public static TypeReferenceIR ToReferenceIR(this TypeSyntax syntax, SemanticModel model, IRBuilder builder)
            => model.GetSymbolInfo(syntax).Symbol.GetTypeReference(builder);

        public static TypeReferenceIR GetTypeReference(this ISymbol symbol, IRBuilder builder)
            => builder.GetDeclarationIR<TypeDeclarationIR>(symbol)?.ToReference();

        public static FunctionReferenceIR GetFunctionReference(this ExpressionSyntax syntax, SemanticModel model, IRBuilder builder)
            => model.GetSymbolInfo(syntax).Symbol.GetFunctionReference(builder);

        public static FunctionReferenceIR GetFunctionReference(this ISymbol symbol, IRBuilder builder)
            => builder.GetDeclarationIR<FunctionIR>(symbol)?.ToReference();

        public static FunctionReferenceIR ToReference(this FunctionIR function)
            => function != null ? new FunctionReferenceIR() { Function = function } : null;

        public static TypeReferenceIR ToReference(this TypeDeclarationIR typeDeclaration)
            => typeDeclaration != null ? new TypeReferenceIR() { ReferencedTypeDeclaration = typeDeclaration } : null;

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
            var originalTypeIR = builder.GetDeclarationIR<TypeDeclarationIR>(originalType);
            var finalTypeIR = builder.GetDeclarationIR<TypeDeclarationIR>(finalType);
            
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
                    r = new NewIR() { CreatedType = objectCreation.Type.ToReferenceIR(model, builder) };
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
                    r = new CastIR() { CastType = ToReferenceIR(castExpression.Type, model, builder) };
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
                    r = new DefaultIR() { DefaultType = defaultExpression.Type.ToReferenceIR(model, builder) };
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
                        r = new LiteralIR() { Value = literal.Token.Value };
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
                    r = new TypeOfIR() { TypeArgument = typeOf.Type.ToReferenceIR(model, builder) };
                    break;

                case TypeSyntax typeSyntax:
                    r = typeSyntax.ToReferenceIR(model, builder);
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
            else
            {
                if (symbol != null)
                    throw new Exception("Unexpected symbol");
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
                    break;

                case IFieldSymbol fieldSymbol:
                    return builder.GetDeclarationIR<FieldIR>(fieldSymbol);

                case ILocalSymbol localSymbol:
                    return builder.GetDeclarationIR<VariableDeclarationIR>(localSymbol);

                case IMethodSymbol methodSymbol:
                    return builder.GetDeclarationIR<MethodIR>(methodSymbol);

                case INamedTypeSymbol namedTypeSymbol:
                    return builder.GetDeclarationIR<TypeDeclarationIR>(namedTypeSymbol);

                case INamespaceSymbol namespaceSymbol:
                    break;

                case ITypeParameterSymbol typeParameterSymbol:
                    return builder.GetDeclarationIR<TypeParameterDeclarationIR>(typeParameterSymbol);

                case ITypeSymbol typeSymbol:
                    return builder.GetDeclarationIR<TypeDeclarationIR>(typeSymbol);

                case IParameterSymbol parameterSymbol:
                    return builder.GetDeclarationIR<ParameterIR>(parameterSymbol);

                case IPropertySymbol propertySymbol:
                    return builder.GetDeclarationIR<PropertyIR>(propertySymbol);

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

        public static ParameterIR ToIR(this ParameterSyntax parameter, SemanticModel model, IRBuilder builder)
        {
            var r = new ParameterIR() { Name = parameter.Identifier.ToString() };
            builder.AddIR(new PlatoParamSyntax(parameter), r);
            r.DefaultValue = parameter.Default?.Value.ToIR(model, builder);
            r.Type = parameter.Type?.ToReferenceIR(model, builder);
            return r;
        }

        public static LambdaIR CreateLambda(SyntaxNode node, SemanticModel model, IRBuilder builder,
            IEnumerable<ParameterSyntax> parameters, ExpressionSyntax body, StatementSyntax block)
            => CreateLambda(node, model, builder, parameters.Select(p => p.ToIR(model, builder)).ToList(),
                body.ToIR(model, builder), block.ToIR(model, builder));

        public static LambdaIR CreateLambda(SyntaxNode node, SemanticModel model, IRBuilder builder, IEnumerable<ParameterIR> parameters, ExpressionIR body, StatementIR block)
        {
            SemanticRules.AssureBodyOrBlock(node, body != null ^ block != null);
            if (body != null)
            {
                block = body.ToReturnStatementIR();
            }

            var dataFlow = model.AnalyzeDataFlow(node);
            var capVars = dataFlow.Captured.Select(sym => sym.ToDeclarationIR(builder)).ToList();

            return new LambdaIR()
            {
                Parameters = parameters.ToList(),
                CapturedVariables = capVars,
                Body = block,
            };
        }
    }
}
