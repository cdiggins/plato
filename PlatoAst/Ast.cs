using System;
using System.Collections.Generic;
using System.Linq;

namespace PlatoAst
{
    public abstract class AstNode
    {
        public virtual IEnumerable<AstNode> Children => Enumerable.Empty<AstNode>();
    }

    public class AstLeaf : AstNode
    {
        public string Text { get; }

        public AstLeaf(string text = "")
            => Text = text;
    }

    public class AstIdentifier : AstLeaf
    {
        public AstIdentifier(string text) : base(text) {} 
        public static AstIdentifier Create(string text) => new AstIdentifier(text);
        public static implicit operator AstIdentifier(string name) => Create(name);
        public static implicit operator string(AstIdentifier ident) => ident.Text;
    }

    public abstract class AstExpr : AstNode
    {
    }

    public class AstError : AstLeaf
    {
        public AstError(string message) : base(message) {}
        public static AstError Create(string message) => new AstError(message);
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
        public static AstReturn Create(AstNode value) => new AstReturn(value);
        public override IEnumerable<AstNode> Children => base.Children.Append(Value);
    }

    public class AstConstant : AstNode
    {
        public object Value { get; }

        public AstConstant(object value) => Value = value;
        public static AstConstant Create(object value) => new AstConstant(value);
        public static AstConstant<T> Create<T>(T value) => new AstConstant<T>(value);
        public static AstConstant Null => new AstConstant(null);
        public static AstConstant True => Create(true);
        public static AstConstant False => Create(false);
    }

    public class AstConstant<T> : AstConstant
    {
        public new T Value => base.Value != null ? (T)base.Value : default;
        public AstConstant(T value) : base(value) { }
    }

    public class AstLambda : AstNode
    {
        public IReadOnlyList<AstVarDef> Parameters { get; }
        public AstNode Body { get; }

        public AstLambda(AstNode body, params AstVarDef[] parameters) => (Parameters, Body) = (parameters, body);
        public static AstLambda Create(AstNode body, params AstVarDef[] parameters) => new AstLambda(body, parameters);
        public override IEnumerable<AstNode> Children => base.Children.Concat(Parameters).Append(Body);
    }

    public class AstMulti : AstNode
    {
        public IReadOnlyList<AstNode> Nodes { get; }

        public AstMulti(params AstNode[] nodes) => Nodes = nodes;
        public static AstMulti Create(params AstNode[] nodes) => new AstMulti(nodes);
        public override IEnumerable<AstNode> Children => base.Children.Concat(Nodes);
    }

    public class AstBlock : AstNode
    {
        public IReadOnlyList<AstNode> Statements { get; }

        public AstBlock(params AstNode[] statements) => Statements = statements;
        public static AstBlock Create(params AstNode[] statements) => new AstBlock(statements);
        public override IEnumerable<AstNode> Children => Statements;
    }

    public class AstIntrinsic : AstNode
    {
        public AstIdentifier Name { get; }

        public AstIntrinsic(AstIdentifier name) => Name = name;
        public static AstIntrinsic Create(AstIdentifier name) => new AstIntrinsic(name);
        public override IEnumerable<AstNode> Children => base.Children.Append(Name);
    }

    public class AstLoop : AstNode
    {
        public AstNode Condition { get; }
        public AstNode Body { get; }

        public AstLoop(AstNode condition, AstNode body) => (Condition, Body) = (condition, body);
        public static AstLoop Create(AstNode condition, AstNode body) => new AstLoop(condition, body);
        public override IEnumerable<AstNode> Children => base.Children.Append(Condition).Append(Body);

    }

    public class AstConditional : AstNode
    {
        public AstNode Condition { get; }
        public AstNode IfTrue { get; }
        public AstNode IfFalse { get; }

        public AstConditional(AstNode condition, AstNode ifTrue, AstNode ifFalse) => (Condition, IfTrue, IfFalse) = (condition, ifTrue, ifFalse);
        public static AstConditional Create(AstNode condition, AstNode ifTrue, AstNode ifFalse) => new AstConditional(condition, ifTrue, ifFalse);
        public override IEnumerable<AstNode> Children => base.Children.Append(Condition).Append(IfTrue).Append(IfFalse);
    }

    public class AstVarDef : AstNode
    {
        public AstIdentifier Name { get; }
        public AstNode Value { get; }
        public AstTypeNode Type { get; }

        public AstVarDef(AstIdentifier name, AstNode value, AstTypeNode type) => (Name, Value, Type) = (name, value, type);
        public static AstVarDef Create(AstIdentifier name, AstNode value, AstTypeNode type) =>  new AstVarDef(name, value, type);
        public static AstVarDef Create(AstIdentifier name, AstTypeNode type) => Create(name, AstNoop.Default, type);
        public override IEnumerable<AstNode> Children => base.Children.Append(Name).Append(Value).Append(Type);
    }

    public class AstVarRef : AstNode
    {
        public AstIdentifier Name { get; }
        public AstVarRef(AstIdentifier name) => Name = name;

        public static AstVarRef Create(AstIdentifier name) => new AstVarRef(name);
        public override IEnumerable<AstNode> Children => base.Children.Append(Name);
    }

    public class AstAssign : AstNode
    {
        public AstVarRef Var { get; }
        public AstNode Value { get; }

        public AstAssign(AstVarRef var, AstNode value) => (Var, Value) = (var, value);
        public static AstAssign Create(AstVarRef var, AstNode value) => new AstAssign(var, value);
        public override IEnumerable<AstNode> Children => new [] { Var, Value };
    }

    public class AstInvoke : AstNode
    {
        public AstNode Function { get; }
        public IReadOnlyList<AstNode> AstArguments { get; }

        public AstInvoke(AstNode function, params AstNode[] arguments) => (Function, AstArguments) = (function, arguments);
        public static AstInvoke Create(AstNode function, params AstNode[] arguments) => new AstInvoke(function, arguments);
        public override IEnumerable<AstNode> Children => new[] { Function };
    }

    public class AstTypeNode : AstNode
    {
        public AstIdentifier Name { get; }
        public IReadOnlyList<AstTypeNode> TypeArguments { get; }

        public AstTypeNode(string name, params AstTypeNode[] args) => (Name, TypeArguments) = (name, args);
        public static AstTypeNode Create(string name, params AstTypeNode[] args) => new AstTypeNode(name, args);
        public override IEnumerable<AstNode> Children => TypeArguments.Cast<AstNode>().Prepend(Name);
    }

    public abstract class AstDeclaration : AstNode
    {
        public AstIdentifier Name { get; }

        protected AstDeclaration(AstIdentifier name) => Name = name;
        public override IEnumerable<AstNode> Children => new [] { Name };
    }

    public abstract class AstMemberDeclaration : AstDeclaration
    {
        public bool IsStatic { get; }
        public AstTypeNode Type { get; }

        protected AstMemberDeclaration(AstIdentifier name, bool isStatic, AstTypeNode type) : base(name) => (IsStatic, Type) = (isStatic, type);
        public override IEnumerable<AstNode> Children => base.Children.Append(Type);
    }

    public class AstFieldDeclaration : AstMemberDeclaration
    {
        public AstNode Node { get; }

        public AstFieldDeclaration(AstIdentifier name, bool isStatic, AstTypeNode type, AstNode node) : base(name, isStatic, type) => Node = node;
        public override IEnumerable<AstNode> Children => base.Children.Append(Node);
    }

    public class AstPropertyDeclaration : AstMemberDeclaration
    {
        public AstNode Body { get; }
        public AstNode Value { get; }

        public AstPropertyDeclaration(AstIdentifier name, bool isStatic, AstTypeNode type, AstNode body, AstNode value) : base(name, isStatic, type)
        {
            Body = body;
            Value = value;
            if (body != null && value != null)
            {
                throw new Exception("Either the body is not null, or the value, not both");
            }
        }

        public override IEnumerable<AstNode> Children => base.Children.Append(Body).Append(Value);
    }

    public class AstParameterDeclaration : AstDeclaration
    {
        public AstTypeNode Type { get; }

        public AstParameterDeclaration(AstIdentifier name, AstTypeNode type) : base(name) => Type = type;
        public override IEnumerable<AstNode> Children => base.Children.Append(Type);
    }

    public class AstMethodDeclaration : AstMemberDeclaration
    {
        public AstNode Body { get; }
        public IReadOnlyList<AstIdentifier> TypeParameters { get; }
        public IReadOnlyList<AstParameterDeclaration> Parameters { get; }

        public AstMethodDeclaration(AstIdentifier name, bool isStatic, AstTypeNode type,
            IReadOnlyList<AstParameterDeclaration> parameters, IReadOnlyList<AstIdentifier> typeParameters, AstNode body) :
            base(name, isStatic, type)
        {
            Body = body;
            Parameters = parameters;
            TypeParameters = typeParameters;
        }

        public override IEnumerable<AstNode> Children =>
            base.Children.Append(Body).Concat(TypeParameters);
    }

    public class AstTypeDeclaration : AstDeclaration
    {
        public IReadOnlyList<AstIdentifier> TypeParameters { get; }
        public IReadOnlyList<AstTypeNode> BaseTypes { get; }
        public IReadOnlyList<AstMemberDeclaration> Members { get; }

        public AstTypeDeclaration(AstIdentifier name, IReadOnlyList<AstIdentifier> typeParameters,
            IReadOnlyList<AstTypeNode> baseTypes, params AstMemberDeclaration[] members) : base(name)
        {
            TypeParameters = typeParameters;
            BaseTypes = baseTypes;
            Members = members;
        }

        public override IEnumerable<AstNode> Children =>
            base.Children.Concat(TypeParameters).Concat(BaseTypes).Concat(Members);
    }
}