/*
    public static class Helpers
    {
        public static void ProcessSymbol(ISymbol symbol)
        {
            switch (symbol)
            {
                case IAliasSymbol aliasSymbol:
                    break;
                case IArrayTypeSymbol arrayTypeSymbol:
                    break;
                case ISourceAssemblySymbol sourceAssemblySymbol:
                    break;
                case IAssemblySymbol assemblySymbol:
                    break;
                case IDiscardSymbol discardSymbol:
                    break;
                case IDynamicTypeSymbol dynamicTypeSymbol:
                    break;
                case IErrorTypeSymbol errorTypeSymbol:
                    break;
                case IEventSymbol eventSymbol:
                    break;
                case IFieldSymbol fieldSymbol:
                    break;
                case IFunctionPointerTypeSymbol functionPointerTypeSymbol:
                    break;
                case ILabelSymbol labelSymbol:
                    break;
                case ILocalSymbol localSymbol:
                    break;
                case IMethodSymbol methodSymbol:
                    break;
                case IModuleSymbol moduleSymbol:
                    break;
                case INamedTypeSymbol namedTypeSymbol:
                    break;
                case INamespaceSymbol namespaceSymbol:
                    break;
                case IPointerTypeSymbol pointerTypeSymbol:
                    break;
                case ITypeParameterSymbol typeParameterSymbol:
                    break;
                case ITypeSymbol typeSymbol:
                    break;
                case INamespaceOrTypeSymbol namespaceOrTypeSymbol:
                    break;
                case IParameterSymbol parameterSymbol:
                    break;
                case IPreprocessingSymbol preprocessingSymbol:
                    break;
                case IPropertySymbol propertySymbol:
                    break;
                case IRangeVariableSymbol rangeVariableSymbol:
                    break;
            }
        }

        public static IEnumerable<ISymbol> GetSymbolsOfSyntax<T>(this SemanticModel model)
            where T : SyntaxNode
            => model.SyntaxTree.GetRoot().DescendantNodes().OfType<T>()
                .Select(node => model.GetDeclaredSymbol(node));

        public static IEnumerable<ISymbol> GetInterfaceSymbols(this SemanticModel model)
            => model.GetSymbolsOfSyntax<InterfaceDeclarationSyntax>();

        public static IEnumerable<ISymbol> GetFields(this SemanticModel model, TypeDeclarationSyntax syntax)
            => throw new NotImplementedException();

        public static IEnumerable<ISymbol> GetProperties(this SemanticModel model, TypeDeclarationSyntax syntax)
            => throw new NotImplementedException();

        public static IEnumerable<MethodDeclarationSyntax> GetMethods(this SemanticModel model, TypeDeclarationSyntax syntax)
            => throw new NotImplementedException();

        public static string ExpandMethod(MethodDeclarationSyntax method)
            => throw new NotImplementedException();

        // https://stackoverflow.com/questions/35741219/how-to-get-il-of-one-method-body-with-roslyn
        public static MethodBody Compile(CSharpCompilation initial, IMethodSymbol method)
        {
            // 1. get source
            var methodRef = method.DeclaringSyntaxReferences.Single();
            var methodSource = methodRef.SyntaxTree.GetText().GetSubText(methodRef.Span).ToString();

            // 2. compile in-memory as script
            var compilation = CSharpCompilation.CreateScriptCompilation("Temp")
                .AddReferences(initial.References)
                .AddSyntaxTrees(SyntaxFactory.ParseSyntaxTree(methodSource, CSharpParseOptions.Default.WithKind(SourceCodeKind.Script)));

            using (var dll = new MemoryStream())
            using (var pdb = new MemoryStream())
            {
                compilation.Emit(dll, pdb);

                // 3. load compiled assembly
                var assembly = Assembly.Load(dll.ToArray(), pdb.ToArray());
                var methodBase = assembly.GetType("Script").GetMethod(method.Name, new Type[0]);

                // 4. get il or even execute
                return methodBase.GetMethodBody();
            }
        }

        public static IMethodSymbol GetCalledMethod(this SemanticModel model, InvocationExpressionSyntax syntax)
        {
            return model.GetSymbolInfo(syntax).Symbol as IMethodSymbol;
        }

        public static SyntaxNode GetDeclarationSyntax(this ISymbol symbol)
        {
            var references = symbol.DeclaringSyntaxReferences;
            if (references.Count() > 1)
            {
                throw new NotImplementedException("TODO: Partial declarations are not yet handled");
            }

            return references.FirstOrDefault()?.GetSyntax();
        }
    }



public class PlatoVarDecl
{
    public string Name;
    public PlatoType DeclaredType;
    public PlatoExpression Value;
    public List<PlatoVarRef> Refs = new List<PlatoVarRef>();
    public PlatoStatement OwnerStatement;
    public bool IsMember => OwnerType != null;
    public PlatoType OwnerType;
}

public class PlatoContext
{
    public PlatoContext Owner;
    public List<PlatoContext> ChildContexts = new List<PlatoContext>();
    public List<PlatoStatement> Statements = new List<PlatoStatement>();
    public List<PlatoVarDecl> VarDecls = new List<PlatoVarDecl>();
    public List<PlatoVarRef> ReferencedVars = new List<PlatoVarRef>();
}

public enum PlatoStatementType
{
    VarDecl,
    Continue,
    Break,
    Return,
    While, 
    Block,
    For,
    ForEach,
    Do,
    If,
    Else,
    Switch,
    InitializerExpression,
}

public class PlatoStatement : PlatoContext
{
    public PlatoStatementType StatementType;
    public StatementSyntax Node;
    public string SyntaxType => Node.GetType().Name;
    public List<PlatoExpression> Expressions = new List<PlatoExpression>();
    public IEnumerable<PlatoStatement> ChildContextStatement => ChildContexts.OfType<PlatoStatement>();

    public static PlatoStatement Create(StatementSyntax syntax, SemanticModel model)
    {
        var r = new PlatoStatement();
        r.Node = syntax;
        r.ChildContexts.AddRange(syntax.ChildNodes().OfType<StatementSyntax>().Select(st => Create(st, model)));
        return r;
    }
}

public class PlatoMember
{
    public ISymbol Symbol;
    public SyntaxNode DeclarationSyntax => Symbol?.GetDeclarationSyntax();
    public string Name => Symbol?.Name ?? "";
} 

public class PlatoField : PlatoMember
{
    public IFieldSymbol FieldSymbol => Symbol as IFieldSymbol;
    public FieldDeclarationSyntax FieldSyntax => DeclarationSyntax as FieldDeclarationSyntax;

    public static PlatoField Create(IFieldSymbol symbol)
    {
        var r = new PlatoField();
        r.Symbol = symbol;
        return r;
    }
}

public class PlatoProperty : PlatoMember
{
    public IPropertySymbol PropertySymbol => Symbol as IPropertySymbol;
    public PropertyDeclarationSyntax PropertySyntax => DeclarationSyntax as PropertyDeclarationSyntax;

    public static PlatoProperty Create(IPropertySymbol symbol)
    {
        var r = new PlatoProperty();
        r.Symbol = symbol;
        return r;
    }
}

public class PlatoMethod : PlatoMember
{
    public IMethodSymbol MethodSymbol => Symbol as IMethodSymbol;
    public MethodDeclarationSyntax MethodSyntax => DeclarationSyntax as MethodDeclarationSyntax;

    public PlatoType ReturnType;
    public List<PlatoStatement> Statements = new List<PlatoStatement>();

    public static PlatoMethod Create(IMethodSymbol symbol)
    {
        var r = new PlatoMethod();
        r.Symbol = symbol;
        r.ReturnType = PlatoType.Create(symbol.ReturnType);
        return r;
    }
}

public class PlatoType : PlatoMember
{
    public ITypeSymbol TypeSymbol => Symbol as ITypeSymbol;
    public TypeKind TypeKind => TypeSymbol?.TypeKind ?? TypeKind.Unknown;
    public TypeDeclarationSyntax TypeSyntax => DeclarationSyntax as TypeDeclarationSyntax;
    public INamedTypeSymbol NamedTypeSymbol => Symbol as INamedTypeSymbol;

    public List<PlatoType> TypeParameters = new List<PlatoType>();
    public List<PlatoType> TypeArguments = new List<PlatoType>();

    public List<PlatoField> Fields = new List<PlatoField>();
    public List<PlatoProperty> Properties = new List<PlatoProperty>();
    public List<PlatoMethod> Methods = new List<PlatoMethod>();

    public static PlatoType Create(TypeDeclarationSyntax type)
    {
        IEnumerable<MemberDeclarationSyntax> members = Array.Empty<MemberDeclarationSyntax>();

        if (type is ClassDeclarationSyntax cds)
            members = cds.Members;
        if (type is StructDeclarationSyntax sds)
            members = sds.Members;
        if (type is InterfaceDeclarationSyntax ids)
            members = ids.Members;

        foreach (var m in members)
        {
            switch (m)
            {
                case FieldDeclarationSyntax fds:
                case PropertyDeclarationSyntax pds:
                case ConstructorDeclarationSyntax conds:
                case MethodDeclarationSyntax mds:
                case EventFieldDeclarationSyntax efds:
                case TypeDeclarationSyntax tds:
                default:
                    break;
            }
        }
    }

    public static PlatoType Create(ITypeSymbol symbol)
    {
        var r = new PlatoType();
        if (symbol == null)
            return r;
        r.Symbol = symbol;
        var nts = r.NamedTypeSymbol;
        if (nts != null)
        {
            r.TypeParameters = nts.TypeParameters.Select(Create).ToList();
            r.TypeArguments = nts.TypeArguments.Select(Create).ToList();
        }

        foreach (var m in symbol.GetMembers())
        {
            switch (m)
            {
                case IMethodSymbol ms:
                    r.Methods.Add(PlatoMethod.Create(ms));
                    break;
                case IPropertySymbol ps:
                    r.Properties.Add(PlatoProperty.Create(ps));
                    break;
                case IFieldSymbol fs:
                    r.Fields.Add(PlatoField.Create(fs));
                    break;
            }
        }

        // TODO: process value tuples special.
        // TODO: find the definitions. 

        return r;
    }
}

public class PlatoVarRef : PlatoExpression
{
    public string Name;
    public PlatoExpression OwnerExpression;
    public PlatoVarDecl Decl;
}

public class PlatoExpression
{
    public object Value;
    public string Name;
    public string ArgumentName;
    public string ExpressionType;
    public ISymbol Symbol;
    public ExpressionSyntax Node;
    public MethodDeclarationSyntax AssociatedMethodSyntax;
    public IMethodSymbol AssociatedMethodSymbol;
    public bool IsDiscarded;
    public PlatoExpression Parent;
    //public PlatoFunctionDefinition Function;
    public PlatoType InferredType;
    //public List<PlatoType> TypeParameters = new List<PlatoType>();
    public PlatoExpression This;
    public List<PlatoExpression> Arguments = new List<PlatoExpression>();

    public void AddArguments(BracketedArgumentListSyntax arguments, SemanticModel model)
        => Arguments.AddRange(arguments.Arguments.Select(x => Create(this, x, model)));

    public static PlatoExpression Create(PlatoExpression parent, ArgumentSyntax node, SemanticModel model)
    {
        var r = Create(parent, node.InitializerExpression, model);
        r.ArgumentName = node.NameColon?.Name.ToString();
        return r;
    }

    public static PlatoExpression Create(PlatoExpression parent, ExpressionSyntax node, SemanticModel model)
    {
        // TODO: many of these could either be built-in, or map to known functions. 
        // How do I find out if a "+" or "this[x]" is mapping to a defined function, and where it is. 
        var r = new PlatoExpression();
        r.Parent = parent;
        r.Node = node;
        r.InferredType = PlatoType.Create(model.GetTypeInfo(node).ConvertedType);
        r.Symbol = model.GetSymbolInfo(node).Symbol;

        switch (node)
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
                r.Arguments.Add(Create(r, binary.Left, model));
                r.Arguments.Add(Create(r, binary.Right, model));
                r.Name = binary.OperatorToken.ToString();
                break;

            case CastExpressionSyntax castExpression:
                r.Name = "#cast";
                r.Arguments.Add(Create(r, castExpression.InitializerExpression, model));
                break;

            case CheckedExpressionSyntax checkedExpression:
                break;

            case ConditionalAccessExpressionSyntax conditionalAccess:
                break;

            case ConditionalExpressionSyntax conditional:
                r.Name = "?";
                r.Arguments.Add(Create(r, conditional.Condition, model));
                r.Arguments.Add(Create(r, conditional.WhenTrue, model));
                r.Arguments.Add(Create(r, conditional.WhenFalse, model));
                break;

            case DeclarationExpressionSyntax declaration:
                break;

            case DefaultExpressionSyntax defaultExpression:
                r.Name = "#default";
                break;

            case ElementAccessExpressionSyntax elementAccess:
                r.Name = "#at";
                r.This = Create(r, elementAccess, model);
                r.AddArguments(elementAccess.ArgumentList, model);
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
                break;

            case IsPatternExpressionSyntax isPattern:
                break;

            case LiteralExpressionSyntax literal:
                break;

            case MakeRefExpressionSyntax makeRef:
                break;

            case MemberAccessExpressionSyntax memberAccess:
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
                break;

            case PostfixUnaryExpressionSyntax postfix:
                break;

            case PrefixUnaryExpressionSyntax prefix:
                break;

            case RangeExpressionSyntax rangeExpression:
                break;

            case SwitchExpressionSyntax switchExpression:
                break;

            case ThisExpressionSyntax thisExpression:
                break;

            case ThrowExpressionSyntax throwExpression:
                break;

            case TupleExpressionSyntax tuple:
                break;

            case TypeOfExpressionSyntax typeOf:
                break;

            case TypeSyntax typeSyntax:
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

        return r;
    }
}

public class Constructor  : PlatoExpression { }
public class Cast : PlatoExpression { }
public class Assignment : PlatoExpression { }
public class DotSelector : PlatoExpression { }
public class Invocation : PlatoExpression { }
public class TernaryUnaryOperation : PlatoExpression { }
public class PostfixUnaryOperation : PlatoExpression { }
public class PrefixUnaryOperation : PlatoExpression { }
public class BinaryOperation : PlatoExpression { }
public class Operation : PlatoExpression { public string Op => Name; }
*/


