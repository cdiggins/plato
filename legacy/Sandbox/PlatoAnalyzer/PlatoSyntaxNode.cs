using System.Collections.Generic;
using System.Linq;

namespace PlatoAnalyzer
{
    public interface IPlatoIdentifier
    {
        string Name { get; }
        PlatoTypeExpr Type { get; }
    }

    public class PlatoSyntaxNode
    {
        public readonly int Id;

        public PlatoSyntaxNode(int id)
            => Id = id;

        public virtual IEnumerable<PlatoExpression> ChildExpressions
            => Enumerable.Empty<PlatoExpression>();

        public virtual IEnumerable<PlatoStatement> ChildStatements
            => Enumerable.Empty<PlatoStatement>();

        public override string ToString()
            => this.ToFormattedString();
    }

    public class PlatoIdentifier : PlatoSyntaxNode, IPlatoIdentifier
    {
        public string Name { get; }
        public PlatoTypeExpr Type { get; }

        public PlatoIdentifier(int id, string name, PlatoTypeExpr type)
            : base(id) => (Name, Type) = (name, type);
    }
    
    public class PlatoMember : PlatoIdentifier
    {
        public readonly bool IsStatic;

        public PlatoMember(int id, string name, PlatoTypeExpr type, bool isStatic)
            : base(id, name, type)
        {
            IsStatic = isStatic;
        }
    }

    public class PlatoProperty : PlatoField
    {
        public readonly PlatoExpression Arrow;

        public PlatoProperty(int id, string name, PlatoTypeExpr type, bool isStatic, PlatoExpression initializer,
            PlatoExpression arrow)
            : base(id, name, type, isStatic, initializer)
        {
            Arrow = arrow;
        }
    }

    public class PlatoField : PlatoMember
    {
        public readonly PlatoExpression Initializer;

        public PlatoField(int id, PlatoVarDeclStatement statement, bool isStatic)
            : this(id, statement.Name, statement.Type, isStatic, statement.Value)
        { }

        public PlatoField(int id, string name, PlatoTypeExpr type, bool isStatic, PlatoExpression initializer)
            : base(id, name, type, isStatic)
        {
            Initializer = initializer;
        }
    }

    public class PlatoAttribute : PlatoSyntaxNode
    {
        public readonly string Name;
        public readonly PlatoArgList Args;
        public PlatoAttribute(int id, string name, PlatoArgList args)
            : base(id)
        {
            Name = name;
            Args = args;
        }
    }

    public class PlatoClass : PlatoIdentifier
    {
        public readonly IReadOnlyList<PlatoFunction> Methods;
        public readonly IReadOnlyList<PlatoField> Fields;
        public readonly IReadOnlyList<PlatoProperty> Properties;
        public readonly IReadOnlyList<PlatoTypeExpr> BaseTypes;
        public readonly IReadOnlyList<PlatoAttribute> Attributes;
        public readonly PlatoTypeParameterList TypeParameters;
        public readonly bool IsValueType;
        public readonly bool IsInterface;
        public bool IsClass => !IsValueType && !IsInterface;
        public bool IsStruct => IsValueType && !IsInterface;

        public PlatoClass(int id, string name, bool isValueType, bool isInterface,
            IEnumerable<PlatoAttribute> attrs = null,
            IEnumerable<PlatoFunction> funcs = null,
            IEnumerable<PlatoProperty> props = null,
            IEnumerable<PlatoField> fields = null,
            IEnumerable<PlatoTypeExpr> baseTypes = null,
            PlatoTypeParameterList typeParams = null)
            : base(id, name, PlatoTypeExpr.ClassType)
        {
            IsValueType = isValueType;
            IsInterface = isInterface;
            Attributes = attrs.ToListOrEmpty();
            Properties = props.ToListOrEmpty();
            Methods = funcs.ToListOrEmpty();
            Fields = fields.ToListOrEmpty();    
            BaseTypes = baseTypes.ToListOrEmpty();
            TypeParameters = typeParams ?? new PlatoTypeParameterList(0);
        }
    }

    public class PlatoTypeExpr : PlatoExpression
    {
        public string Name { get; }
        public PlatoTypeExpr Type => TypeType;
        public IReadOnlyList<PlatoTypeExpr> TypeArguments;

        public PlatoTypeExpr(int id, string name, IEnumerable<PlatoTypeExpr> typeArgs = null)
            : base(id, null) => (Name, TypeArguments) = (name, typeArgs.ToListOrEmpty());

        public static readonly PlatoTypeExpr TypeType = new PlatoTypeExpr(-1, "Type");
        public static readonly PlatoTypeExpr ClassType = new PlatoTypeExpr(-2, "Class");
        public static readonly PlatoTypeExpr ArrayType = new PlatoTypeExpr(-3, "Array");
        public static readonly PlatoTypeExpr TypeParamType = new PlatoTypeExpr(-4, "TypeParam");
        public static readonly PlatoTypeExpr StringType = new PlatoTypeExpr(-5, "String");
        public static readonly PlatoTypeExpr ThrowType = new PlatoTypeExpr(-6, "Throw");
        public static readonly PlatoTypeExpr VoidType = new PlatoTypeExpr(-7, "Void");

        public static PlatoTypeExpr CreateArrayType(PlatoTypeExpr elementType)
            => new PlatoTypeExpr(0, "Array", new[] {elementType});
    }

    public class PlatoParameter : PlatoIdentifier
    {
        public readonly PlatoExpression DefaultValue;

        public PlatoParameter(int id, string name, PlatoTypeExpr type, PlatoExpression defaultValue)
            : base(id, name, type)
            => DefaultValue = defaultValue;
    }

    public class PlatoExpression : PlatoSyntaxNode
    {
        public readonly PlatoTypeExpr Type;

        public PlatoExpression(int id, PlatoTypeExpr type)
            : base(id) => (Type) = (type);
    }

    public class PlatoParameterList : PlatoSyntaxNode
    {
        public readonly IReadOnlyList<PlatoParameter> Parameters;
            
        public PlatoParameterList(int id, IEnumerable<PlatoParameter> parameters = null)
            : base(id) => Parameters = parameters.ToListOrEmpty();
    }

    public class PlatoLambda : PlatoExpression
    {
        public readonly PlatoParameterList Parameters;
        public readonly PlatoStatement Body;

        public PlatoLambda(int id, PlatoTypeExpr type, PlatoParameterList parameters, PlatoStatement body)
            : base(id, type)
        {
            Parameters = parameters;
            Body = body;
        }

        public override IEnumerable<PlatoStatement> ChildStatements
            => new [] {Body};
    }

    public class PlatoIdentifierRef : PlatoExpression
    {
        public readonly IPlatoIdentifier Identifier;
        public string Name { get; }

        public PlatoIdentifierRef(int id, IPlatoIdentifier identifier)
            : base(id, identifier.Type)
            => (Identifier, Name) = (identifier, identifier.Name);

        public PlatoIdentifierRef(int id, PlatoTypeExpr type, string name)
            : base(id, type)
            => Name = name;
    }

    public class PlatoArray : PlatoExpression
    {
        public PlatoTypeExpr ElementType => Type.TypeArguments[0];
        public readonly PlatoExpression Size;
        public readonly IReadOnlyList<PlatoExpression> Elements;

        public override IEnumerable<PlatoExpression> ChildExpressions
            => Elements;

        public PlatoArray(int id, PlatoTypeExpr elementType, PlatoExpression size = null, IEnumerable<PlatoExpression> elements = null)
            : base(id, PlatoTypeExpr.CreateArrayType(elementType))
        {
            Elements = elements.ToListOrEmpty();
            Size = size ?? Elements.Count.ToPlatoLiteral();
        }
    }

    public class PlatoAssignment : PlatoExpression
    {
        public readonly string Operator;
        public readonly PlatoExpression Left;
        public readonly PlatoExpression Right;

        public PlatoAssignment(int id, string op, PlatoExpression left, PlatoExpression right)
            : base(id, left.Type)
            => (Operator, Left, Right) = (op, left, right);

        public override IEnumerable<PlatoExpression> ChildExpressions
            => new[] {Left, Right};
    }

    public class PlatoBinary : PlatoExpression
    {
        public readonly string Operator;
        public readonly PlatoExpression Left;
        public readonly PlatoExpression Right;

        public PlatoBinary(int id, PlatoTypeExpr type, string op, PlatoExpression left, PlatoExpression right)
            : base(id, type)
            => (Operator, Left, Right) = (op, left, right);

        public override IEnumerable<PlatoExpression> ChildExpressions
            => new[] { Left, Right };
    }

    public class PlatoCast : PlatoExpression
    {
        public readonly PlatoExpression Expression;

        public PlatoCast(int id, PlatoTypeExpr type, PlatoExpression expr)
            : base(id, type) => Expression = expr;

        public override IEnumerable<PlatoExpression> ChildExpressions
            => new[] { Expression };
    }

    public class PlatoLiteral : PlatoExpression
    {
        public readonly object Value;

        public PlatoLiteral(int id, PlatoTypeExpr type, object value)
            : base(id, type) => Value = value;
    }

    public class PlatoArg : PlatoExpression
    {
        public readonly PlatoExpression Value;
        public readonly string Name;

        public PlatoArg(int id, PlatoTypeExpr type, PlatoExpression value, string name = null)
            : base(id, type) => (Value, Name) = (value, name);
    }

    public class PlatoTypeParam : PlatoIdentifier
    {
        public PlatoTypeParam(int id, string name)
            : base(id, name, PlatoTypeExpr.TypeParamType)
        {
        }
    }

    public class PlatoTypeParameterList : PlatoSyntaxNode
    {
        public readonly IReadOnlyList<PlatoTypeParam> TypeParameters;

        public PlatoTypeParameterList(int id, IEnumerable<PlatoTypeParam> parameters = null)
            : base(id) => TypeParameters = parameters.ToListOrEmpty();
    }

    public class PlatoPatternMatch : PlatoExpression
    {
        public readonly string Name;

        public PlatoPatternMatch(int id, string name, PlatoTypeExpr type)
            : base(id, type)
        {
            Name = name;
        }
    }

    public class PlatoPatternIs : PlatoExpression
    {
        public readonly PlatoExpression Expr;
        public readonly PlatoPatternMatch Pattern;

        public PlatoPatternIs(int id, PlatoExpression expr, PlatoPatternMatch pattern)
            : base(id, pattern.Type)
        {
            Expr = expr;
            Pattern = pattern;
        }
    }

    public class PlatoInterpolation : PlatoExpression
    {
        public readonly IReadOnlyList<PlatoExpression> Expressions;

        public PlatoInterpolation(int id, IEnumerable<PlatoExpression> expressions = null)
            : base(id, PlatoTypeExpr.StringType) => Expressions = expressions.ToListOrEmpty();

        public override IEnumerable<PlatoExpression> ChildExpressions
            => Expressions;
    }

    public class PlatoArgList : PlatoSyntaxNode
    {
        public readonly IReadOnlyList<PlatoArg> Args;

        public PlatoArgList(int id, IEnumerable<PlatoArg> args = null)
            : base(id) => Args = args.ToListOrEmpty();

        public override IEnumerable<PlatoExpression> ChildExpressions
            => Args;
    }

    public class PlatoInvoke : PlatoExpression
    {
        public readonly PlatoExpression Function;
        public readonly PlatoArgList Args;
        public readonly PlatoExpression Reciever;

        public PlatoInvoke(int id, PlatoTypeExpr type, PlatoExpression function, PlatoExpression receiver, PlatoArgList args)
            : base(id, type) => (Function, Reciever, Args) = (function, receiver, args);

        public override IEnumerable<PlatoExpression> ChildExpressions
            => Args.Args.Prepend(Function);
    }

    public class PlatoPostfix : PlatoExpression
    {
        public readonly string Operator;
        public readonly PlatoExpression Operand;

        public PlatoPostfix(int id, PlatoTypeExpr type, string op, PlatoExpression expr)
            : base(id, type) => (Operator, Operand) = (op, expr);

        public override IEnumerable<PlatoExpression> ChildExpressions
            => new[] {Operand};
    }

    public class PlatoPrefix : PlatoExpression
    {
        public readonly string Operator;
        public readonly PlatoExpression Operand;

        public PlatoPrefix(int id, PlatoTypeExpr type, string op, PlatoExpression expr)
            : base(id, type) => (Operator, Operand) = (op, expr);

        public override IEnumerable<PlatoExpression> ChildExpressions
            => new[] { Operand };
    }

    public class PlatoThis : PlatoExpression
    {
        public PlatoThis(int id, PlatoTypeExpr type)
            : base(id, type) { }
    }

    public class PlatoBase : PlatoExpression
    {
        public PlatoBase(int id, PlatoTypeExpr type)
            : base(id, type) { }
    }

    public class PlatoDefault : PlatoExpression
    {
        public PlatoDefault(int id, PlatoTypeExpr type)
            : base(id, type) { }
    }

    public class PlatoElementGet : PlatoExpression
    {
        public readonly PlatoExpression Receiver;
        public readonly PlatoExpression Index;

        public PlatoElementGet(int id, PlatoTypeExpr type, PlatoExpression receiver, PlatoExpression index)
            : base(id, type) => (Receiver, Index) = (receiver, index);

        public override IEnumerable<PlatoExpression> ChildExpressions
            => new[] { Receiver, Index };
    }

    public class PlatoConditionalAccess : PlatoExpression
    {
        public readonly PlatoExpression Expression;
        public readonly PlatoExpression WhenNotNull;

        public PlatoConditionalAccess(int id, PlatoTypeExpr type, PlatoExpression expr, PlatoExpression whenNotNull)
            : base(id, type) => (Expression, WhenNotNull) = (expr, whenNotNull);

        public override IEnumerable<PlatoExpression> ChildExpressions
            => new[] { Expression, WhenNotNull };
    }

    public class PlatoMemberGet : PlatoExpression
    {
        public readonly PlatoExpression Receiver;
        public readonly string Name;

        public PlatoMemberGet(int id, PlatoTypeExpr type, PlatoExpression expr, string name)
            : base(id, type) => (Receiver, Name) = (expr, name);

        public override IEnumerable<PlatoExpression> ChildExpressions
            => new[] {Receiver};
    }

    public class PlatoElementSet : PlatoExpression
    {
        public readonly PlatoElementGet Left;
        public readonly PlatoExpression Right;

        public PlatoElementSet(int id, PlatoElementGet left, PlatoExpression right)
            : base(id, left.Type) => (Left, Right) = (left, right);
        
        public override IEnumerable<PlatoExpression> ChildExpressions
            => new[] {Left, Right};
    }

    public class PlatoMemberSet : PlatoExpression
    {
        public readonly PlatoMemberGet Left;
        public readonly PlatoExpression Right;

        public PlatoMemberSet(int id, PlatoMemberGet left, PlatoExpression right)
            : base(id, left.Type) => (Left, Right) = (left, right);
 
        public override IEnumerable<PlatoExpression> ChildExpressions
            => new[] { Left, Right };
    }

    public class PlatoConditional : PlatoExpression
    {
        public readonly PlatoExpression Condition;
        public readonly PlatoExpression OnTrue;
        public readonly PlatoExpression OnFalse;

        public PlatoConditional(int id, PlatoTypeExpr type, PlatoExpression cond, PlatoExpression onTrue, PlatoExpression onFalse)
            : base(id, type) => (Condition, OnTrue, OnFalse) = (cond, onTrue, onFalse);

        public override IEnumerable<PlatoExpression> ChildExpressions
            => new[] { Condition, OnTrue, OnFalse };
    }

    public class PlatoParenthesis : PlatoExpression
    {
        public readonly PlatoExpression Expression;

        public PlatoParenthesis(int id, PlatoExpression expr)
            : base(id, expr.Type)
            => Expression = expr;

        public override IEnumerable<PlatoExpression> ChildExpressions
            => new[] { Expression };
    }

    public class PlatoTuple : PlatoExpression
    {
        public readonly IReadOnlyList<PlatoExpression> Expressions;

        public override IEnumerable<PlatoExpression> ChildExpressions
            => Expressions;

        public PlatoTuple(int id, PlatoTypeExpr type, IEnumerable<PlatoExpression> expressions)
            : base(id, type) => Expressions = expressions.ToListOrEmpty();
    }

    public class PlatoNew : PlatoExpression
    {
        public readonly PlatoArgList Args;
        public readonly IReadOnlyList<PlatoExpression> Initializers;

        public PlatoNew(int id, PlatoTypeExpr type, PlatoArgList args = null, IEnumerable<PlatoExpression> expressions = null)
            : base(id, type) => (Args, Initializers) = (args ?? new PlatoArgList(0), expressions.ToListOrEmpty());

        public override IEnumerable<PlatoExpression> ChildExpressions
            => Args.Args.Concat(Initializers);
    }

    public class PlatoTypeOf : PlatoLiteral
    {
        public PlatoTypeOf(int id, PlatoTypeExpr type)
            : base(id, type, type)
        { }
    }

    public class PlatoNameOf : PlatoLiteral
    {
        public PlatoNameOf(int id, string value)
            : base(id, PlatoTypeExpr.StringType, value)
        { }
    }

    public class PlatoStatement : PlatoSyntaxNode
    {
        public PlatoStatement(int id)
            : base(id)
        {
        }
    }

    public class PlatoBlockStatement : PlatoStatement
    {
        public readonly IReadOnlyList<PlatoStatement> Statements;

        public override IEnumerable<PlatoStatement> ChildStatements
            => Statements;

        public PlatoBlockStatement(int id, IEnumerable<PlatoStatement> statements = null)
            : base(id) => Statements = statements.ToListOrEmpty();
    }

    public class PlatoReturnStatement : PlatoStatement
    {
        public readonly PlatoExpression Expression;

        public PlatoReturnStatement(int id, PlatoExpression expr)
            : base(id) => Expression = expr;

        public override IEnumerable<PlatoExpression> ChildExpressions
            => new[] {Expression};
    }

    public class PlatoIfStatement : PlatoStatement
    {
        public readonly PlatoExpression Condition;
        public readonly PlatoStatement IfStatement;
        public readonly PlatoStatement ElseStatement;

        public PlatoIfStatement(int id, PlatoExpression cond, PlatoStatement onTrue, PlatoStatement onFalse = null)
            : base(id) => (Condition, IfStatement, ElseStatement) = (cond, onTrue, onFalse ?? new PlatoEmptyStatement(0));

        public override IEnumerable<PlatoExpression> ChildExpressions
            => new[] { Condition};

        public override IEnumerable<PlatoStatement> ChildStatements
            => new[] {IfStatement, ElseStatement};
    }

    public class PlatoExpressionStatement : PlatoStatement
    {
        public readonly PlatoExpression Expression;

        public PlatoExpressionStatement(int id, PlatoExpression expr)
            : base(id) => Expression = expr;

        public override IEnumerable<PlatoExpression> ChildExpressions
            => new[] { Expression };
    }

    public class PlatoCompoundStatement : PlatoStatement
    {
        public readonly IReadOnlyList<PlatoStatement> Statements;

        public PlatoCompoundStatement(int id, IEnumerable<PlatoStatement> statements = null)
            : base(id) => Statements = statements.ToListOrEmpty();

        public override IEnumerable<PlatoStatement> ChildStatements
            => Statements;
    }

    public class PlatoBreakStatement : PlatoStatement
    {
        public PlatoBreakStatement(int id) : base(id)
        {
        }
    }

    public class PlatoEmptyStatement : PlatoStatement
    {
        public PlatoEmptyStatement(int id) : base(id)
        {
        }
    }

    public class PlatoContinueStatement : PlatoStatement
    {
        public PlatoContinueStatement(int id) : base(id)
        {
        }
    }

    public class PlatoWhileStatement : PlatoStatement
    {
        public readonly PlatoExpression Condition;
        public readonly PlatoStatement Body;

        public PlatoWhileStatement(int id, PlatoExpression expr, PlatoStatement body)
            : base(id) => (Condition, Body) = (expr, body);

        public override IEnumerable<PlatoExpression> ChildExpressions
            => new[] { Condition };

        public override IEnumerable<PlatoStatement> ChildStatements
            => new[] {Body};
    }

    public class PlatoForEachStatement : PlatoStatement
    {
        public readonly PlatoVarDeclStatement VarDecl;
        public readonly PlatoExpression Collection;
        public readonly PlatoStatement Body;

        public PlatoForEachStatement(int id, PlatoVarDeclStatement varDecl, PlatoExpression collection, PlatoStatement body)
            : base(id) => (VarDecl, Collection, Body) = (varDecl, collection, body);

        public override IEnumerable<PlatoExpression> ChildExpressions
            => new[] { Collection };

        public override IEnumerable<PlatoStatement> ChildStatements
            => new[] { VarDecl, Body };
    }

    public class PlatoThrowExpression : PlatoExpression
    {
        public readonly PlatoExpression Expression;

        public PlatoThrowExpression(int id, PlatoExpression expr)
            : base(id, PlatoTypeExpr.ThrowType)
            => Expression = expr;

        public override IEnumerable<PlatoExpression> ChildExpressions
            => new[] { Expression };
    }

    public class PlatoVarDeclStatement : PlatoStatement, IPlatoIdentifier
    {
        public PlatoTypeExpr Type { get;  }
        public string Name { get; } 
        public readonly PlatoExpression Value;
        public readonly PlatoArgList Args;

        public PlatoVarDeclStatement(int id, PlatoTypeExpr type, string name, PlatoExpression value, PlatoArgList argList = null)
            : base(id) => (Type, Name, Value, Args) = (type, name, value, argList ?? new PlatoArgList(0));

        public override IEnumerable<PlatoExpression> ChildExpressions
            => Args.Args.Prepend(Value);
    }

    public class PlatoFunction : PlatoSyntaxNode
    {
        public readonly string Name;
        public readonly PlatoParameterList Parameters;
        public readonly PlatoTypeExpr ReturnType;
        public readonly PlatoTypeExpr ReceiverType;
        public readonly PlatoStatement Body;
        public readonly PlatoTypeParameterList TypeParameters;

        public PlatoFunction(int id, string name, PlatoParameterList parameters, PlatoTypeExpr receiverType, PlatoTypeExpr returnType, PlatoStatement body,
            PlatoTypeParameterList typeParams)
            : base(id)
        {
            Name = name;
            Parameters = parameters;
            ReturnType = returnType;
            ReceiverType = receiverType;
            Body = body;
            TypeParameters = typeParams;
        }

        public override IEnumerable<PlatoStatement> ChildStatements
            => new[] { Body};
    }
}
