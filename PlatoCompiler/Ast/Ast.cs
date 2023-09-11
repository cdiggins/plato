using System;
using System.Collections.Generic;
using System.Linq;

namespace Plato.Compiler.Ast
{
    public abstract class AstNode
    {
        public virtual IEnumerable<AstNode> Children => Enumerable.Empty<AstNode>();
        public override string ToString() => $"({GetType().Name})";
    }

    public class AstLeaf : AstNode
    {
        public string Text { get; }
        public AstLeaf(string text = "") => Text = text;
        public override string ToString() => $"({GetType().Name}={Text})";
    }

    public class AstIdentifier : AstLeaf
    {
        public AstIdentifier(string text) : base(text) { }
    }

    public class AstParenthesized : AstNode
    {
        public AstNode Inner;
        public AstParenthesized(AstNode inner) => Inner = inner;
        public override string ToString() => $"({Inner})";
    }

    public class AstNoop : AstLeaf
    {
        public static AstNoop Default { get; } = new AstNoop();
    }

    public abstract class AstReducible : AstNode
    {
    }

    public class AstBreak : AstReducible
    {
        public static AstBreak Default { get; } = new AstBreak();
    }

    public class AstContinue : AstReducible
    {
        public static AstContinue Default { get; } = new AstContinue();
    }

    public class AstReturn : AstReducible
    {
        public AstNode Value { get; }

        public AstReturn(AstNode value) => Value = value;
        public override IEnumerable<AstNode> Children => base.Children.Append(Value);
    }

    public enum LiteralTypesEnum
    {
        Integer,
        Number,
        Boolean,
        String
    }

    public class AstConstant : AstNode
    {
        public object Value { get; }
        public LiteralTypesEnum TypeEnum { get; }

        public AstConstant(LiteralTypesEnum typeEnum, object value)
            => (TypeEnum, Value) = (typeEnum, value);

        public static AstConstant Create(object value)
        {
            if (value is string s)
                return new AstConstant(LiteralTypesEnum.String, s);
            if (value is bool b)
                return new AstConstant(LiteralTypesEnum.Boolean, b);
            if (value is double d)
                return new AstConstant(LiteralTypesEnum.Number, d);
            if (value is float f)
                return new AstConstant(LiteralTypesEnum.Number, f);
            if (value is int n)
                return new AstConstant(LiteralTypesEnum.Integer, n);
            throw new Exception($"Not a recognized constant type {value}");
        }

        public static AstConstant True => Create(true);
        public static AstConstant False => Create(false);
        public override string ToString() => $"({GetType().Name}:{TypeEnum} {Value})";

    }

    public class AstLambda : AstNode
    {
        public IReadOnlyList<AstParameterDeclaration> Parameters { get; }
        public AstNode Body { get; }
        public AstLambda(AstNode body, params AstParameterDeclaration[] parameters) => (Parameters, Body) = (parameters, body);
        public override IEnumerable<AstNode> Children => base.Children.Concat(Parameters).Append(Body);
    }

    public class AstMulti : AstNode
    {
        public IReadOnlyList<AstNode> Nodes { get; }
        public AstMulti(params AstNode[] nodes) => Nodes = nodes;
        public override IEnumerable<AstNode> Children => base.Children.Concat(Nodes);
    }

    public class AstBlock : AstNode
    {
        public IReadOnlyList<AstNode> Statements { get; }
        public AstBlock(params AstNode[] statements) => Statements = statements;
        public override IEnumerable<AstNode> Children => Statements;
    }

    public class AstLoop : AstNode
    {
        public AstNode Condition { get; }
        public AstNode Body { get; }
        public AstLoop(AstNode condition, AstNode body) => (Condition, Body) = (condition, body);
        public override IEnumerable<AstNode> Children => base.Children.Append(Condition).Append(Body);
    }

    public class AstConditional : AstNode
    {
        public AstNode Condition { get; }
        public AstNode IfTrue { get; }
        public AstNode IfFalse { get; }
        public AstConditional(AstNode condition, AstNode ifTrue, AstNode ifFalse) => (Condition, IfTrue, IfFalse) = (condition, ifTrue, ifFalse);
        public override IEnumerable<AstNode> Children => base.Children.Append(Condition).Append(IfTrue).Append(IfFalse);
        public override string ToString() => $"({Condition} ? {IfTrue} : {IfFalse})";
    }

    public class AstVarDef : AstNode
    {
        public string Name { get; }
        public AstNode Value { get; }
        public AstTypeNode Type { get; }
        public AstVarDef(string name, AstNode value, AstTypeNode type) => (Name, Value, Type) = (name, value, type);
        public override IEnumerable<AstNode> Children => base.Children.Append(Value).Append(Type);
    }

    public class AstAssign : AstNode
    {
        public string Var { get; }
        public AstNode Value { get; }
        public AstAssign(string var, AstNode value) => (Var, Value) = (var, value);
        public override IEnumerable<AstNode> Children => new[] { Value };
    }

    public class AstInvoke : AstNode
    {
        public AstNode Function { get; }
        public IReadOnlyList<AstNode> Arguments { get; }
        public AstInvoke(AstNode function, params AstNode[] arguments) => (Function, Arguments) = (function, arguments);
        public override IEnumerable<AstNode> Children => new[] { Function };
        public override string ToString() => $"{Function}({string.Join(",", Arguments)})";
    }

    public class AstTypeNode : AstNode
    {
        public string Name { get; }
        public IReadOnlyList<AstTypeNode> TypeArguments { get; }
        public AstTypeNode(string name, params AstTypeNode[] args) => (Name, TypeArguments) = (name, args);
        public override IEnumerable<AstNode> Children => TypeArguments;
        public override string ToString() => $"({GetType().Name} {Name}<{string.Join(",", TypeArguments)}>)";
    }

    public abstract class AstDeclaration : AstNode
    {
        public string Name { get; }
        protected AstDeclaration(string name) => Name = name;
        public override string ToString() => $"({GetType().Name} {Name})";
    }

    public abstract class AstMemberDeclaration : AstDeclaration
    {
        public AstTypeNode Type { get; }
        protected AstMemberDeclaration(string name, AstTypeNode type) : base(name) => Type = type;
        public override IEnumerable<AstNode> Children => base.Children.Append(Type);
    }

    public class AstNamespace : AstDeclaration
    {
        public IReadOnlyList<AstTypeDeclaration> Types { get; }
        public override IEnumerable<AstNode> Children => Types;
        public AstNamespace(string name, IEnumerable<AstTypeDeclaration> types) : base(name)
            => Types = types.ToListOrEmpty();
    }

    public class AstFieldDeclaration : AstMemberDeclaration
    {
        public AstNode Node { get; }
        public AstFieldDeclaration(string name, AstTypeNode type, AstNode node) : base(name, type) => Node = node;
        public override IEnumerable<AstNode> Children => base.Children.Append(Node);
    }

    public class AstParameterDeclaration : AstDeclaration
    {
        public AstTypeNode Type { get; }
        public AstParameterDeclaration(string name, AstTypeNode type) : base(name) => Type = type;
        public override IEnumerable<AstNode> Children => base.Children.Append(Type);
    }

    public class AstMethodDeclaration : AstMemberDeclaration
    {
        public AstNode Body { get; }
        public IReadOnlyList<AstParameterDeclaration> Parameters { get; }

        public AstMethodDeclaration(string name,
            AstTypeNode type,
            IEnumerable<AstParameterDeclaration> parameters,
            AstNode body) :
            base(name, type)
        {
            Body = body;
            Parameters = parameters.ToList();
        }

        public override IEnumerable<AstNode> Children =>
            base.Children.Append(Body);
    }

    public class AstExpressionStatement : AstNode
    {
        public AstNode Expression { get; }
        public AstExpressionStatement(AstNode expression) => Expression = expression;
        public override IEnumerable<AstNode> Children => base.Children.Append(Expression);
    }

    public class AstTypeParameter : AstDeclaration
    {
        public AstTypeNode Constraint { get; }
        public AstTypeParameter(string name, AstTypeNode constraint)
            : base(name)
            => Constraint = constraint;
    }

    public enum TypeKind
    {
        ConcreteType,
        Library,
        Concept,
        Primitive,
        TypeVariable,
    }

    public class AstTypeDeclaration : AstDeclaration
    {
        public TypeKind Kind { get; }
        public IReadOnlyList<AstTypeParameter> TypeParameters { get; }
        public IReadOnlyList<AstTypeNode> Inherits { get; }
        public IReadOnlyList<AstTypeNode> Implements { get; }
        public IReadOnlyList<AstMemberDeclaration> Members { get; }

        public AstTypeDeclaration(TypeKind kind, string name, IEnumerable<AstTypeParameter> typeParameters,
            IEnumerable<AstTypeNode> inherits, IEnumerable<AstTypeNode> implements, params AstMemberDeclaration[] members)
            : base(name)
        {
            Kind = kind;
            TypeParameters = typeParameters.ToList();
            Inherits = inherits.ToList();
            Implements = implements.ToList();
            Members = members;
        }

        public override IEnumerable<AstNode> Children =>
            base.Children.Concat(TypeParameters).Concat(Inherits).Concat(Implements).Concat(Members);

        public override string ToString()
        {
            return $"({Kind} {Name}<{string.Join(",", TypeParameters)}> ({string.Join(";", Members)}))";
        }
    }

    public static class AstNodeExtensions
    {
        public static IEnumerable<AstNode> GetAllDescendants(this AstNode node)
        {
            if (node == null)
                yield break;
            yield return node;
            foreach (var child in node.Children.SelectMany(GetAllDescendants))
                yield return child;
        }

        public static IEnumerable<AstTypeDeclaration> GetAllTypes(this AstNode node)
            => node.GetAllDescendants().OfType<AstTypeDeclaration>();

        public static IReadOnlyList<T> ToListOrEmpty<T>(this IEnumerable<T> items)
            => items?.ToList() ?? new List<T>();
    }
}