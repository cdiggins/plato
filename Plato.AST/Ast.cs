using System;
using System.Collections.Generic;
using System.Linq;
using Parakeet;

namespace Plato.AST
{
    public abstract class AstNode
    {
        public ILocation Location { get; }
        public virtual IEnumerable<AstNode> Children => Enumerable.Empty<AstNode>();
        public override string ToString() => $"({GetType().Name})";
        protected AstNode(ILocation location) => Location = location ?? NoLocation;
        public static ILocation NoLocation = new Location();
    }

    public class AstLeaf : AstNode
    {
        public string Text { get; }
        public AstLeaf(ILocation location, string text = "") : base(location) => Text = text;
        public override string ToString() => $"({GetType().Name}={Text})";
    }

    public class AstIdentifier : AstLeaf
    {
        public AstIdentifier(ILocation location, string text) : base(location, text) { }
    }

    public class AstParenthesized : AstNode
    {
        public AstNode Inner;
        public AstParenthesized(ILocation location, AstNode inner) : base(location) => Inner = inner;
        public override string ToString() => $"({Inner})";
    }

    public class AstNoop : AstLeaf
    {
        public AstNoop() : base(NoLocation) {}
        public static AstNoop Default { get; } = new AstNoop();
    }

    public abstract class AstReducible : AstNode
    {
        protected AstReducible(ILocation location) : base(location)
        { }
    }

    public class AstBreak : AstReducible
    {
        public static AstBreak Default { get; } = new AstBreak(NoLocation);

        public AstBreak(ILocation location) : base(location)
        {
        }
    }

    public class AstContinue : AstReducible
    {
        public static AstContinue Default { get; } = new AstContinue(NoLocation);

        public AstContinue(ILocation location) : base(location)
        {
        }
    }

    public class AstReturn : AstReducible
    {
        public AstNode Value { get; }
        public AstReturn(ILocation location, AstNode value) : base(location) => Value = value;
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

        public AstConstant(ILocation location, LiteralTypesEnum typeEnum, object value) : base(location) => (TypeEnum, Value) = (typeEnum, value);

        public static AstConstant Create(object value)
        {
            if (value is string s)
                return new AstConstant(NoLocation, LiteralTypesEnum.String, s);
            if (value is bool b)
                return new AstConstant(NoLocation, LiteralTypesEnum.Boolean, b);
            if (value is double d)
                return new AstConstant(NoLocation, LiteralTypesEnum.Number, d);
            if (value is float f)
                return new AstConstant(NoLocation, LiteralTypesEnum.Number, f);
            if (value is int n)
                return new AstConstant(NoLocation, LiteralTypesEnum.Integer, n);
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
        public AstLambda(ILocation location, AstNode body, params AstParameterDeclaration[] parameters) : base(location) => (Parameters, Body) = (parameters, body);
        public override IEnumerable<AstNode> Children => base.Children.Concat(Parameters).Append(Body);
    }

    public class AstMulti : AstNode
    {
        public IReadOnlyList<AstNode> Nodes { get; }
        public AstMulti(ILocation location, params AstNode[] nodes) : base(location) => Nodes = nodes;
        public override IEnumerable<AstNode> Children => base.Children.Concat(Nodes);
    }

    public class AstBlock : AstNode
    {
        public IReadOnlyList<AstNode> Statements { get; }
        public AstBlock(ILocation location, params AstNode[] statements) : base(location) => Statements = statements;
        public override IEnumerable<AstNode> Children => Statements;
    }

    public class AstLoop : AstNode
    {
        public AstNode Condition { get; }
        public AstNode Body { get; }
        public AstLoop(ILocation location, AstNode condition, AstNode body) : base(location) => (Condition, Body) = (condition, body);
        public override IEnumerable<AstNode> Children => base.Children.Append(Condition).Append(Body);
    }

    public class AstConditional : AstNode
    {
        public AstNode Condition { get; }
        public AstNode IfTrue { get; }
        public AstNode IfFalse { get; }
        public AstConditional(ILocation location, AstNode condition, AstNode ifTrue, AstNode ifFalse) : base(location) => (Condition, IfTrue, IfFalse) = (condition, ifTrue, ifFalse);
        public override IEnumerable<AstNode> Children => base.Children.Append(Condition).Append(IfTrue).Append(IfFalse);
        public override string ToString() => $"({Condition} ? {IfTrue} : {IfFalse})";
    }

    public class AstVarDef : AstNode
    {
        public string Name { get; }
        public AstNode Value { get; }
        public AstTypeNode Type { get; }
        public AstVarDef(ILocation location, string name, AstNode value, AstTypeNode type) : base(location) => (Name, Value, Type) = (name, value, type);
        public override IEnumerable<AstNode> Children => base.Children.Append(Value).Append(Type);
    }

    public class AstAssign : AstNode
    {
        public string Var { get; }
        public AstNode Value { get; }
        public AstAssign(ILocation location, string var, AstNode value) : base(location) => (Var, Value) = (var, value);
        public override IEnumerable<AstNode> Children => new[] { Value };
    }

    public class AstBinaryOp : AstNode
    {
        private AstBinaryOp(ILocation location, string op, AstNode left, AstNode right) : base(location)
            => (Op, Left, Right) = (op, left, right);
        public string Op { get; }
        public string FuncName => Operators.BinaryOperatorToName(Op);
        public AstNode Left { get; }
        public AstNode Right { get; }

        public static AstBinaryOp Create(ILocation location, string op, AstNode left, AstNode right)
        {
            if (!Operators.BinaryOperatorToNames.ContainsKey(op))
                throw new Exception($"Operation `{op}` is not a valid operator name");

            var precedence = Operators.BinaryOperatorPrecedence(op);
            if (right is AstBinaryOp binRight && binRight.Precedence < precedence)
                return Create(binRight.Location, binRight.Op, Create(location, op, left, binRight.Left), binRight.Right);
            return new AstBinaryOp(location, op, left, right);
        }

        public AstInvoke ToInvocation()
            => new AstInvoke(Location, new AstIdentifier(Location, Operators.BinaryOperatorToName(Op)), Left, Right);

        public int Precedence => Operators.BinaryOperatorPrecedence(Op);
    }

    public class AstInvoke : AstNode
    {
        public AstNode Function { get; }
        public IReadOnlyList<AstNode> Arguments { get; }
        public AstInvoke(ILocation location, AstNode function, params AstNode[] arguments) : base(location) => (Function, Arguments) = (function, arguments);
        public override IEnumerable<AstNode> Children => new[] { Function };
        public override string ToString() => $"{Function}({string.Join(",", Arguments)})";
    }

    public class AstTypeNode : AstNode
    {
        public string Name { get; }
        public IReadOnlyList<AstTypeNode> TypeArguments { get; }
        public AstTypeNode(ILocation location, string name, params AstTypeNode[] args) : base(location) => (Name, TypeArguments) = (name, args);
        public override IEnumerable<AstNode> Children => TypeArguments;
        public override string ToString() => $"({GetType().Name} {Name}<{string.Join(",", TypeArguments)}>)";
    }

    public abstract class AstDeclaration : AstNode
    {
        public string Name { get; }
        protected AstDeclaration(ILocation location, string name) : base(location) => Name = name;
        public override string ToString() => $"({GetType().Name} {Name})";
    }

    public abstract class AstMemberDeclaration : AstDeclaration
    {
        public AstTypeNode Type { get; }
        protected AstMemberDeclaration(ILocation location, string name, AstTypeNode type) : base(location, name) => Type = type;
        public override IEnumerable<AstNode> Children => base.Children.Append(Type);
    }

    public class AstNamespace : AstDeclaration
    {
        public IReadOnlyList<AstTypeDeclaration> Types { get; }
        public override IEnumerable<AstNode> Children => Types;
        public AstNamespace(ILocation location, string name, IEnumerable<AstTypeDeclaration> types) : base(location, name)
            => Types = types.ToListOrEmpty();
    }

    public class AstFieldDeclaration : AstMemberDeclaration
    {
        public AstNode Node { get; }
        public AstFieldDeclaration(ILocation location, string name, AstTypeNode type, AstNode node) : base(location, name, type) => Node = node;
        public override IEnumerable<AstNode> Children => base.Children.Append(Node);
    }

    public class AstParameterDeclaration : AstDeclaration
    {
        public int Index { get; }
        public AstTypeNode Type { get; }
        public AstParameterDeclaration(ILocation location, string name, AstTypeNode type, int index) : base(location, name) 
            => (Type, Index) = (type, index);
        public override IEnumerable<AstNode> Children => base.Children.Append(Type);
    }

    public class AstMethodDeclaration : AstMemberDeclaration
    {
        public AstNode Body { get; }
        public IReadOnlyList<AstParameterDeclaration> Parameters { get; }

        public AstMethodDeclaration(ILocation location, string name,
            AstTypeNode type,
            IEnumerable<AstParameterDeclaration> parameters,
            AstNode body) :
            base(location, name, type)
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
        public AstExpressionStatement(ILocation location, AstNode expression) : base(location) => Expression = expression;
        public override IEnumerable<AstNode> Children => base.Children.Append(Expression);
    }

    public class AstTypeParameter : AstDeclaration
    {
        public AstTypeParameter(ILocation location, string name)
            : base(location, name)
        { }
    }

    public class AstConstraint : AstDeclaration
    {
        public AstTypeNode Constraint { get; }
        public AstConstraint(ILocation location, string name, AstTypeNode type)
            : base(location, name)
            => Constraint = type;
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
        public IReadOnlyList<AstConstraint> Constraints { get; }

        public AstTypeDeclaration(
            ILocation location,
            TypeKind kind, 
            string name, 
            IEnumerable<AstTypeParameter> typeParameters,
            IEnumerable<AstTypeNode> inherits, 
            IEnumerable<AstTypeNode> implements, 
            IEnumerable<AstConstraint> constraints, 
            params AstMemberDeclaration[] members)
            : base(location, name)
        {
            Kind = kind;
            TypeParameters = typeParameters.ToList();
            Inherits = inherits.ToList();
            Implements = implements.ToList();
            Constraints = constraints.ToList();
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