namespace PlatoAbstractSyntaxTree
{
    public class AbstractNode
    {
        public virtual IEnumerable<AbstractNode> Children => Enumerable.Empty<AbstractNode>();
    }

    public interface INamed
    {
        string Name { get; }
    }

    public interface ITyped
    {
        TypeExpr Type { get; }
    }

    public abstract class Statement : AbstractNode { }
    
    public abstract class Expression : AbstractNode { }

    public abstract class Declaration : AbstractNode, INamed
    {
        public Declaration(string name)
        {
            Name = name;
        }
        public string Name { get; }
    }

    public abstract class TypedDeclaration : Declaration, ITyped
    {
        public TypedDeclaration(string name, TypeExpr type)
            : base(name)
        {
            Type = type;
        }
        public TypeExpr Type { get; }

        public override IEnumerable<AbstractNode> Children
            => new AbstractNode[] { Type };
    }

    public class WhileStatement : Statement 
    {
        public Expression Condition { get; }
        public CompoundStatement Body { get; }
        public override IEnumerable<AbstractNode> Children => new AbstractNode[] { Condition, Body };
        public WhileStatement(Expression condition, CompoundStatement body)
        {
            (Condition, Body) = (condition, body);
        }
        public static WhileStatement Create(Expression condition, CompoundStatement body)
            => new WhileStatement(condition, body);
    }

    public class CompoundStatement : Statement 
    { 
        public IReadOnlyList<Statement> ChildStatements { get; }
        public override IEnumerable<AbstractNode> Children => ChildStatements;
        public CompoundStatement(IReadOnlyList<Statement> childStatements)
        {
            ChildStatements = childStatements;
        }

        public static CompoundStatement Create(IReadOnlyList<Statement> childStatements)
            => new CompoundStatement(childStatements);
    }

    public class MultiStatement : Statement
    {
        public IReadOnlyList<Statement> ChildStatements { get; }
        public override IEnumerable<AbstractNode> Children => ChildStatements;
        public MultiStatement(IReadOnlyList<Statement> childStatements)
        {
            ChildStatements = childStatements;
        }

        public static MultiStatement Create(IReadOnlyList<Statement> childStatements)
            => new MultiStatement(childStatements);
    }


    public class BreakStatement : Statement
    {
        public static BreakStatement Create()
            => new BreakStatement();
    }

    public class ContinueStatement : Statement
    {
        public static ContinueStatement Create()
            => new ContinueStatement();
    }

    public class ReturnStatement : Statement
    {
        public ReturnStatement(Expression? expression)
        {
            Expression = expression;
        }

        public Expression? Expression { get; }
        public override IEnumerable<AbstractNode> Children => new AbstractNode[] { Expression };

        public static ReturnStatement Create(Expression? expression = null)
            => new ReturnStatement(expression);
    }

    public class TypeExpr : AbstractNode, INamed
    {
        public string Name { get; }
        public IReadOnlyList<TypeExpr> TypeArgs { get; }
        public string NamespaceQualifier { get; }
        public override IEnumerable<AbstractNode> Children => TypeArgs;
        public TypeExpr(string name, string namespaceQualifier, params TypeExpr[] typeArgs)
        {
            (Name, NamespaceQualifier, TypeArgs) = (name, namespaceQualifier, typeArgs);
        }

        public static TypeExpr Create(string name, string namespaceQualifier, params TypeExpr[] typeArgs)
            => new TypeExpr(name, namespaceQualifier, typeArgs);

        public static TypeExpr Create(string name, params TypeExpr[] typeArgs)
            => Create(name, "", typeArgs);

        public static TypeExpr Void = Create("void");
        public static TypeExpr Var = Create("var");
    }

    public class ParameterDeclaration : TypedDeclaration 
    {
        public ParameterDeclaration(string name, TypeExpr type)
            : base(name, type)
        { }

        public static ParameterDeclaration Create(string name, TypeExpr type)
            => new ParameterDeclaration(name, type);
    }
    
    public class FunctionDeclaration : Declaration
    {
        public FunctionDeclaration(string name, IReadOnlyList<TypeParameter> typeParameters, TypeExpr returnType, CompoundStatement body, params ParameterDeclaration[] parameters)
            : base(name)
        {
            (TypeParameters, ReturnType, Body, Parameters) = (typeParameters, returnType, body, parameters);
        }

        public bool IsStatic { get; }
        public TypeExpr ReturnType { get; }
        public IReadOnlyList<ParameterDeclaration> Parameters { get; }
        public IReadOnlyList<TypeParameter> TypeParameters { get; }
        public override IEnumerable<AbstractNode> Children
            => Parameters.Cast<AbstractNode>().Concat(TypeParameters).Append(ReturnType).Append(Body);
        public CompoundStatement Body { get; }

        public static FunctionDeclaration Create(string name, IReadOnlyList<TypeParameter> typeParameters, TypeExpr returnType, CompoundStatement body, params ParameterDeclaration[] parameters)
            => new FunctionDeclaration(name, typeParameters, returnType, body, parameters);
    
        public static FunctionDeclaration Create(string name,  TypeExpr returnType, CompoundStatement body, params ParameterDeclaration[] parameters)
            => Create(name, Array.Empty<TypeParameter>(), returnType, body, parameters);

        public static FunctionDeclaration Create(string name, CompoundStatement body, params ParameterDeclaration[] parameters)
            => Create(name, Array.Empty<TypeParameter>(), TypeExpr.Void, body, parameters);
    }

    public class ConditionalExpression : Expression 
    {
        public ConditionalExpression(Expression condition, Expression onTrue, Expression onFalse)
        {
            (Condition, OnTrue, OnFalse) = (condition, onTrue, onFalse);
        }

        public Expression Condition { get; }
        public Expression OnTrue { get; }
        public Expression OnFalse { get; }
        public override IEnumerable<AbstractNode> Children 
            => new AbstractNode[] { Condition, OnTrue, OnFalse };

        public static ConditionalExpression Create(Expression condition, Expression onTrue, Expression onFalse)
            => new ConditionalExpression(condition, onTrue, onFalse);   
    }

    public class ExpressionStatement : Statement
    {
        public ExpressionStatement(Expression expression)
        {
            Expression = expression;
        }

        public Expression Expression { get; }
        public static ExpressionStatement Create(Expression expression)
            => new ExpressionStatement(expression);
    }

    public class IfStatement : Statement 
    {
        public Expression Condition { get; }
        public Statement OnTrue { get; }
        public Statement? OnFalse { get; }

        public override IEnumerable<AbstractNode> Children
            => OnFalse == null ?
                new AbstractNode[] { Condition, OnTrue } :
                new AbstractNode[] { Condition, OnTrue, OnFalse };

        public IfStatement(Expression condition, Statement onTrue, Statement? onFalse)
        {
            (Condition, OnTrue, OnFalse) = (condition, onTrue, onFalse);
        }

        public static IfStatement Create(Expression condition, Statement onTrue, Statement? onFalse = null)
            => new IfStatement(condition, onTrue, onFalse);
    }

    public class TypeParameter : Declaration
    {
        public TypeParameter(string name)
            : base(name) { }

        public static TypeParameter Create(string name)
            => new TypeParameter(name);
    }

    public class FieldDeclaration : TypedDeclaration
    {
        public FieldDeclaration(string name, TypeExpr type)
            : base(name, type) {  }

        public static FieldDeclaration Create(string name, TypeExpr type)
            => new FieldDeclaration(name, type);
    }

    public class ClassDeclaration : Declaration 
    {
        public ClassDeclaration(string name, string classKind,
            IReadOnlyList<TypeParameter> typeParameters, 
            IReadOnlyList<FunctionDeclaration> functions,
            IReadOnlyList<FieldDeclaration> fields)
            : base(name)
        {
            ClassKind = classKind;
            TypeParameters = typeParameters;
            Functions = functions;
            Fields = fields;
        }

        public string ClassKind { get; } = "class";
        public IReadOnlyList<TypeParameter> TypeParameters { get; }
        public IReadOnlyList<FunctionDeclaration> Functions { get; }
        public IReadOnlyList<FieldDeclaration> Fields { get; }

        public override IEnumerable<AbstractNode> Children =>
            TypeParameters.Cast<AbstractNode>().Concat(Functions).Concat(Fields);
    
        public static ClassDeclaration Create(string name, string classKind, 
            IReadOnlyList<TypeParameter> typeParameters, IReadOnlyList<FunctionDeclaration> functions,
            IReadOnlyList<FieldDeclaration> fields)
            => Create(name, classKind, typeParameters, functions, fields);

        public static ClassDeclaration Create(string name, string classKind,
            IReadOnlyList<FunctionDeclaration> functions,
            IReadOnlyList<FieldDeclaration> fields)
            => Create(name, classKind, Array.Empty<TypeParameter>(), functions, fields);
    }

    public class Identifier : Expression, INamed
    {
        public string Name { get; }

        public Identifier(string name)
        {
            Name = name;
        }

        public static Identifier Create(string name) 
            => new Identifier(name);

        public static implicit operator Identifier(string name)
            => new Identifier(name);
    }

    public class FunctionCall : Expression 
    { 
        public Expression Function { get; }
        public IReadOnlyList<Expression> Arguments { get; }
        public FunctionCall(Expression function, params Expression[] arguments)
        {
            (Function, Arguments) = (function, arguments);
        }

        public override IEnumerable<AbstractNode> Children
            => Array.Empty<AbstractNode>();

        public static FunctionCall Create(Expression function, params Expression[] arguments)
            => new FunctionCall(function, arguments);
    }

    public class VariableDeclaration : TypedDeclaration
    { 
        public VariableDeclaration(TypeExpr type, string name, Expression? initialValue)
            : base(name, type)
        {
            InitialValue = initialValue;
        }

        public Expression? InitialValue { get; }
        
        public override IEnumerable<AbstractNode> Children
            => InitialValue != null 
                ? new AbstractNode[] { InitialValue }
                : Array.Empty<AbstractNode>();

        public static VariableDeclaration Create(TypeExpr type, string name, Expression? initialValue = null)
           => new VariableDeclaration(type, name, initialValue);

        public static VariableDeclaration Create(string name, Expression? initialValue = null)
           => new VariableDeclaration(TypeExpr.Var, name, initialValue);
    }

    public class MemberExpression : Expression, INamed
    { 
        public MemberExpression(Expression receiver, string name)
        {
            Receiver = receiver;
            Name = name;
        }

        public Expression Receiver { get; }
        public string Name { get; }
        public override IEnumerable<AbstractNode> Children
            => new AbstractNode[] { Receiver };

        public static MemberExpression Create(Expression receiver, string name)
            => new MemberExpression(receiver, name);
    }

    public class MemberAssignment : Statement
    {
        public MemberAssignment(Expression receiver, string name, Expression rightValue)
        {
            Receiver = receiver;
            Name = name;
            RightValue = rightValue;
        }

        public Expression Receiver { get; }
        public string Name { get; }
        public Expression RightValue { get; }
        public override IEnumerable<AbstractNode> Children => new[] { Receiver, RightValue };

        public static MemberAssignment Create(Expression receiver, string name, Expression rightValue)
            => new MemberAssignment(receiver, name, rightValue);
    }

    public class Assignment : Statement
    { 
        public Assignment(Identifier leftValue, Expression rightValue)
        {
            LeftValue = leftValue;
            RightValue = rightValue;
        }

        public Identifier LeftValue { get; }
        public Expression RightValue { get; }
        public override IEnumerable<AbstractNode> Children => new[] { LeftValue, RightValue };
    
        public static Assignment Create(Identifier leftValue, Expression rightValue)
            => new Assignment(leftValue, rightValue);
    }

    public class Namespace : Declaration
    {
        public Namespace(string name, IReadOnlyList<Namespace> nestedNamespaces, IReadOnlyList<ClassDeclaration> classes)
            : base(name)
        {
            NestedNamespaces = nestedNamespaces ?? ;
            Classes = classes;
        }

        public IReadOnlyList<Namespace> NestedNamespaces { get; }
        public IReadOnlyList<ClassDeclaration> Classes { get; }
        public override IEnumerable<AbstractNode> Children => NestedNamespaces.Cast<AbstractNode>().Concat(Classes);
    
        public static Namespace Create(string name, IReadOnlyList<Namespace> nestedNamespace = null, IReadOnlyList<ClassDeclaration> classes = null)
            => new Namespace(name, nestedNamespace ?? Array.Empty<Namespace>(), 
                classes ?? Array.Empty<>);

        public static Namespace Create(string name, IReadOnlyList<ClassDeclaration> classes)
            => new Namespace(name, Array.Empty<Namespace>(), classes);

    }

    /*
    public class IntrinsicFunctionDeclaration : IDeclaration { }

    public interface INameLookup { }
    public interface ITypeLookup { }

    public static class CompileAst
    {
        public static IAbstractNode Reduce(this IAbstractNode node) { } 
        public static INameLookup ResolveNames(this IAbstractNode node, INameLookup lookup) { }
        public static IAbstractNode NameSimplification(this IAbstractNode node) { }
        public static ITypeLookup ResolveTypes(this IAbstractNode node, ITypeLookup lookup) { }
        public static IAbstractNode SimplifyExpressions(this IAbstractNode node) { }
        public static IAbstractNode PartialEvaluation(this IAbstractNode node) { }
        public static IAbstractNode DetectStaticFunctions(this IAbstractNode node) {  }
        public static IAbstractNode AddMissingThisParameters(this IAbstractNode node) {  }

        // For some outputs we can have more complex outputs.
        // Like switch statements. 
        public void CreateOutputForm() { } 
    }
    */
}