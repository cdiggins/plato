using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace PlatoAnalyzer
{
    public record PlatoSyntaxNode;

    public record PlatoRoot;

    public record PlatoIdentifier(string Name, PlatoType Type) : PlatoSyntaxNode;

    public record PlatoMember(string Name, PlatoType Type, bool IsStatic) : PlatoIdentifier(Name, Type);

    public record PlatoField(string Name, PlatoType Type, bool IsStatic, PlatoExpression Expression) : PlatoMember(Name, Type, IsStatic);
    
    public record PlatoProperty(string Name, PlatoType Type, bool IsStatic, PlatoExpression Expression) : PlatoMember(Name, Type, IsStatic);

    public record PlatoClass(string Name, IReadOnlyList<PlatoFunction> Functions, IReadOnlyList<PlatoField> Fields,
        IReadOnlyList<PlatoProperty> Properties, PlatoTypeParameterList TypeParams) : PlatoSyntaxNode;

    public record PlatoType(string Name) : PlatoIdentifier(Name, TypeType)
    {
        public static readonly PlatoType TypeType = new ("Type");
        public static readonly PlatoType ClassType = new ("Class");
        public static readonly PlatoType ArrayType = new ("Array");
        public static readonly PlatoType TypeParamType = new ("TypeParam");
        public static readonly PlatoType StringType = new ("String");
        public static readonly PlatoType ThrowType = new ("Throw");
    }

    public record PlatoParameter(string Name, PlatoType Type) : PlatoIdentifier(Name, Type);

    public record PlatoExpression(PlatoType Type) : PlatoSyntaxNode
    {
        public virtual IEnumerable<PlatoExpression> ChildExpressions
            => Array.Empty<PlatoExpression>();

        public virtual IEnumerable<PlatoStatement> ChildStatements
            => Array.Empty<PlatoStatement>();
    }

    public class PlatoParameterList : PlatoSyntaxNode
    {
        public readonly IReadOnlyList<PlatoParameter> Parameters;

        public PlatoParameterList(IEnumerable<PlatoParameter> parameters = null)
            => Parameters = parameters.ToListOrEmpty();
    }

    public class PlatoLambda : PlatoExpression
    {
        public readonly PlatoParameterList Parameters;
        public readonly PlatoStatement Body;

        public PlatoLambda(int id, PlatoType type, PlatoParameterList parameters, PlatoStatement body)
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
        public readonly PlatoIdentifier Identifier;

        public PlatoIdentifierRef(int id, PlatoIdentifier identifier)
            : base(id, identifier.Type)
            => Identifier = identifier;
    }

    public class PlatoArray : PlatoExpression
    {
        public readonly PlatoType ElementType;
        public readonly PlatoExpression Size;
        public readonly IReadOnlyList<PlatoExpression> Elements;

        public override IEnumerable<PlatoExpression> ChildExpressions
            => Elements;

        public PlatoArray(int id, PlatoType elementType, PlatoExpression size, IEnumerable<PlatoExpression> elements = null)
            : base(id, PlatoType.ArrayType)

        {
            ElementType = elementType;
            Size = size ?? elements.Count().ToPlatoLiteral();
            Elements = elements.ToListOrEmpty();
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

        public PlatoBinary(int id, PlatoType type, string op, PlatoExpression left, PlatoExpression right)
            : base(id, type)
            => (Operator, Left, Right) = (op, left, right);

        public override IEnumerable<PlatoExpression> ChildExpressions
            => new[] { Left, Right };
    }

    public class PlatoCast : PlatoExpression
    {
        public readonly PlatoExpression Expression;

        public PlatoCast(int id, PlatoType type, PlatoExpression expr)
            : base(id, type) => Expression = expr;

        public override IEnumerable<PlatoExpression> ChildExpressions
            => new[] { Expression };
    }

    public class PlatoLiteral : PlatoExpression
    {
        public readonly object Value;

        public PlatoLiteral(int id, PlatoType type, object value)
            : base(id, type) => Value = value;
    }

    public class PlatoArg : PlatoExpression
    {
        public readonly PlatoExpression Value;
        public readonly string Name;

        public PlatoArg(int id, PlatoType type, PlatoExpression value, string name = null)
            : base(id, type) => (Value, Name) = (value, name);
    }

    public class PlatoTypeParam : PlatoIdentifier
    {
        public PlatoTypeParam(int id, string name)
            : base(id, name, PlatoType.TypeParamType)
        {
        }
    }

    public class PlatoTypeParameterList
    {
        public readonly IReadOnlyList<PlatoTypeParam> TypeParms;

        public PlatoTypeParameterList(IEnumerable<PlatoTypeParam> parameters)
            => TypeParms = parameters.ToList();
    }


    public class PlatoInterpolation : PlatoExpression
    {
        public readonly IReadOnlyList<PlatoExpression> Expressions;

        public PlatoInterpolation(int id, PlatoType type, IEnumerable<PlatoExpression> expressions)
            : base(id, type) => Expressions = expressions.ToList();

        public override IEnumerable<PlatoExpression> ChildExpressions
            => Expressions;
    }

    public class PlatoArgList : PlatoSyntaxNode
    {
        public readonly IReadOnlyList<PlatoArg> Args;

        public PlatoArgList(IEnumerable<PlatoArg> args)
            => Args = args.ToList();
    }

    public class PlatoInvoke : PlatoExpression
    {
        public readonly PlatoExpression Function;
        public readonly PlatoArgList Args;

        public PlatoInvoke(int id, PlatoType type, PlatoExpression function, PlatoArgList args)
            : base(id, type) => (Function, Args) = (function, args);

        public override IEnumerable<PlatoExpression> ChildExpressions
            => Args?.Args.Prepend(Function) ?? Enumerable.Repeat(Function, 1);
    }

    public class PlatoPostfix : PlatoExpression
    {
        public readonly string Operator;
        public readonly PlatoExpression Operand;

        public PlatoPostfix(int id, PlatoType type, string op, PlatoExpression expr)
            : base(id, type) => (Operator, Operand) = (op, expr);

        public override IEnumerable<PlatoExpression> ChildExpressions
            => new[] {Operand};
    }

    public class PlatoPrefix : PlatoExpression
    {
        public readonly string Operator;
        public readonly PlatoExpression Operand;

        public PlatoPrefix(int id, PlatoType type, string op, PlatoExpression expr)
            : base(id, type) => (Operator, Operand) = (op, expr);

        public override IEnumerable<PlatoExpression> ChildExpressions
            => new[] { Operand };
    }

    public class PlatoThis : PlatoExpression
    {
        public PlatoThis(int id, PlatoType type)
            : base(id, type) { }
    }

    public class PlatoDefault : PlatoExpression
    {
        public PlatoDefault(int id, PlatoType type)
            : base(id, type) { }
    }

    public class PlatoElementGet : PlatoExpression
    {
        public readonly PlatoExpression Receiver;
        public readonly PlatoExpression Index;

        public PlatoElementGet(int id, PlatoType type, PlatoExpression receiver, PlatoExpression index)
            : base(id, type) => (Receiver, Index) = (receiver, index);

        public override IEnumerable<PlatoExpression> ChildExpressions
            => new[] { Receiver, Index };
    }

    public class PlatoMemberGet : PlatoExpression
    {
        public readonly PlatoExpression Receiver;
        public readonly string Name;

        public PlatoMemberGet(int id, PlatoType type, PlatoExpression expr, string name)
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

        public PlatoConditional(int id, PlatoType type, PlatoExpression cond, PlatoExpression onTrue, PlatoExpression onFalse)
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

        public PlatoTuple(int id, PlatoType type, IEnumerable<PlatoExpression> expressions)
            : base(id, type) => Expressions = expressions.ToList();
    }

    public class PlatoNew : PlatoExpression
    {
        public readonly PlatoArgList Args;
        public readonly IReadOnlyList<PlatoExpression> Initializers;

        public PlatoNew(int id, PlatoType type, PlatoArgList args, IEnumerable<PlatoExpression> expressions)
            : base(id, type) => (Args, Initializers) = (args, Initializers.ToList());

        public override IEnumerable<PlatoExpression> ChildExpressions
            => Args?.Args.Concat(Initializers) ?? Initializers;
    }

    public class PlatoTypeOf : PlatoLiteral
    {
        public PlatoTypeOf(int id, PlatoType type)
            : base(id, type, type)
        { }
    }

    public class PlatoNameOf : PlatoLiteral
    {
        public PlatoNameOf(int id, string value)
            : base(id, PlatoType.StringType, value)
        { }
    }

    public class PlatoStatement : PlatoSyntaxNode
    {
        public virtual IEnumerable<PlatoExpression> ChildExpressions
            => Enumerable.Empty<PlatoExpression>();

        public virtual IEnumerable<PlatoStatement> ChildStatements
            => Enumerable.Empty<PlatoStatement>();
    }

    public class PlatoBlockStatement : PlatoStatement
    {
        public readonly IReadOnlyList<PlatoStatement> Statements;

        public override IEnumerable<PlatoStatement> ChildStatements
            => Statements;

        public PlatoBlockStatement(IEnumerable<PlatoStatement> statements)
            => Statements = statements.ToList();
    }

    public class PlatoReturnStatement : PlatoStatement
    {
        public readonly PlatoExpression Expression;

        public PlatoReturnStatement(PlatoExpression expr)
            => Expression = expr;

        public override IEnumerable<PlatoExpression> ChildExpressions
            => Enumerable.Repeat(Expression, 1);
    }

    public class PlatoIfStatement : PlatoStatement
    {
        public readonly PlatoExpression Condition;
        public readonly PlatoStatement IfStatement;
        public readonly PlatoStatement ElseStatement;

        public PlatoIfStatement(PlatoExpression cond, PlatoStatement onTrue, PlatoStatement onFalse = null)
            => (Condition, IfStatement, ElseStatement) = (cond, onTrue, onFalse ?? new PlatoEmptyStatement());

        public override IEnumerable<PlatoExpression> ChildExpressions
            => new[] { Condition};

        public override IEnumerable<PlatoStatement> ChildStatements
            => new[] {IfStatement, ElseStatement};
    }

    public class PlatoExpressionStatement : PlatoStatement
    {
        public readonly PlatoExpression Expression;

        public PlatoExpressionStatement(PlatoExpression expr)
            => Expression = expr;

        public override IEnumerable<PlatoExpression> ChildExpressions
            => new[] { Expression };
    }

    public class PlatoCompoundStatement : PlatoStatement
    {
        public readonly IReadOnlyList<PlatoStatement> Statements;

        public PlatoCompoundStatement(IEnumerable<PlatoStatement> statements)
            => Statements = statements.ToList();

        public override IEnumerable<PlatoStatement> ChildStatements
            => Statements;
    }

    public class PlatoBreakStatement : PlatoStatement
    { }

    public class PlatoEmptyStatement : PlatoStatement 
    { }

    public class PlatoContinueStatement : PlatoStatement
    { }

    public class PlatoWhileStatement : PlatoStatement
    {
        public readonly PlatoExpression Condition;
        public readonly PlatoStatement Body;

        public PlatoWhileStatement(PlatoExpression expr, PlatoStatement body)
            => (Condition, Body) = (expr, body);

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

        public PlatoForEachStatement(PlatoVarDeclStatement varDecl, PlatoExpression collection, PlatoStatement body)
            => (VarDecl, Collection, Body) = (varDecl, collection, body);

        public override IEnumerable<PlatoExpression> ChildExpressions
            => new[] { Collection };

        public override IEnumerable<PlatoStatement> ChildStatements
            => new[] { VarDecl, Body };
    }

    public class PlatoThrowExpression : PlatoExpression
    {
        public readonly PlatoExpression Expression;

        public PlatoThrowExpression(int id, PlatoExpression expr)
            : base(id, PlatoType.ThrowType)
            => Expression = expr;

        public override IEnumerable<PlatoExpression> ChildExpressions
            => new[] { Expression };
    }

    public class PlatoVarDeclStatement : PlatoStatement
    {
        public readonly PlatoType Type;
        public readonly string Name; 
        public readonly PlatoExpression Value;
        public readonly PlatoArgList Args;

        public PlatoVarDeclStatement(PlatoType type, string name, PlatoExpression value, PlatoArgList argList)
            => (Type, Name, Value, Args) = (type, name, value, argList);

        public override IEnumerable<PlatoExpression> ChildExpressions
            => Args.Args.Prepend(Value);
    }

    public class PlatoFunction : PlatoSyntaxNode
    {
        public readonly string Name;
        public readonly PlatoParameterList Parameters;
        public readonly PlatoType ReturnType;
        public readonly PlatoStatement Body;
        public readonly PlatoTypeParameterList TypeParameters;

        public PlatoFunction(string name, PlatoParameterList parameters, PlatoType returnType, PlatoStatement body,
            PlatoTypeParameterList typeParams)
        {
            Name = name;
            Parameters = parameters;
            ReturnType = returnType;
            Body = body;
            TypeParameters = typeParams;
        }
    }

    public static class Extensions
    {
        public static IReadOnlyList<T> ToListOrEmpty<T>(this IEnumerable<T> self)
            => self?.ToList() ?? new List<T>();

        public static PlatoType ToPlatoType(this Type t)
            => new PlatoType(t.FullName, -1);

        public static PlatoLiteral ToPlatoLiteral(this object value)
            => new PlatoLiteral(-1, value.GetType().ToPlatoType(), value);
    }
}
