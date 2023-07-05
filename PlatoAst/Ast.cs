using System;
using System.Collections.Generic;
using System.Linq;

namespace PlatoAst
{
    public abstract class AstNode
    {
        public virtual IEnumerable<AstNode> Children => Enumerable.Empty<AstNode>();
        public override string ToString() => "(" + string.Join(" ", Children) + ")";
    }

    public class AstAttribute : AstNode
    {
        public string Name { get; }
        public AstAttribute(string name) => Name = name;
        public static AstAttribute Create(string name) => new AstAttribute(name);
    }

    public class AstLeaf : AstNode
    {
        public string Text { get; }
        public AstLeaf(string text = "") => Text = text;
        public override string ToString() => Text;
    }

    public class AstIdentifier : AstLeaf
    {
        public AstIdentifier(string text) : base(text) {} 
        public static AstIdentifier Create(string text) => new AstIdentifier(text);
        public static implicit operator AstIdentifier(string name) => Create(name);
        public static implicit operator string(AstIdentifier ident) => ident.Text;
    }

    public class AstParenthesized : AstNode
    {
        public AstNode Inner;
        public AstParenthesized(AstNode inner) => Inner = inner;
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
        public IReadOnlyList<AstParameterDeclaration> Parameters { get; }
        public AstNode Body { get; }

        public AstLambda(AstNode body, params AstParameterDeclaration[] parameters) => (Parameters, Body) = (parameters, body);
        public static AstLambda Create(AstNode body, params AstParameterDeclaration[] parameters) => new AstLambda(body, parameters);
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

    public class AstMemberAccess : AstNode
    {
        public AstNode Receiver { get; }
        public AstIdentifier Name { get; }

        public AstMemberAccess(AstNode reciever, AstIdentifier name) => (Receiver, Name) = (reciever, name);
        public static AstMemberAccess Create(AstNode reciever, AstIdentifier name) => new AstMemberAccess(reciever, name);
        public override IEnumerable<AstNode> Children => base.Children.Append(Receiver).Append(Name);
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

    public class AstAssign : AstNode
    {
        public string Var { get; }
        public AstNode Value { get; }

        public AstAssign(string var, AstNode value) => (Var, Value) = (var, value);
        public static AstAssign Create(string var, AstNode value) => new AstAssign(var, value);
        public override IEnumerable<AstNode> Children => new [] { Value };
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
        public IReadOnlyList<AstAttribute> Attributes { get; }
        public IReadOnlyList<AstDeclaration> Declarations { get; }

        protected AstDeclaration(AstIdentifier name, IReadOnlyList<AstAttribute> attributes)
        {
            (Name, Attributes) = (name, attributes ?? Array.Empty<AstAttribute>());
        }

        public override IEnumerable<AstNode> Children => new [] { Name };
        public bool HasAttribute(string name) => Attributes.Any(a => a.Name == name);
    }

    public abstract class AstMemberDeclaration : AstDeclaration
    {
        public AstTypeNode Type { get; }
        protected AstMemberDeclaration(AstIdentifier name, AstTypeNode type, IReadOnlyList<AstAttribute> attributes) : base(name, attributes) 
            => (Type) = (type);
        public override IEnumerable<AstNode> Children => base.Children.Append(Type);
    }

    public class AstDirective : AstNode
    {
        public string Directive;
        public string Argument;
        public static AstDirective Create(string directive, string argument)
            => new AstDirective(directive, argument);
        public AstDirective(string directive, string argument)
            => (Directive, Argument) = (directive, argument);
    }

    public class AstNamespace : AstDeclaration
    {
        public IReadOnlyList<AstTypeDeclaration> Types { get; }
        public IReadOnlyList<AstDirective> Directives { get; }
        public IReadOnlyList<AstNamespace> Namespaces { get; }
        public override IEnumerable<AstNode> Children => 
            Types.Cast<AstNode>().Concat(Namespaces).Concat(Directives);
        public AstNamespace(AstIdentifier name, IEnumerable<AstDirective> directives, IEnumerable<AstTypeDeclaration> types, IEnumerable<AstNamespace> namespaces)
        : base(name, null)
            => (Types, Directives, Namespaces) = (types.ToList(), directives.ToList(), namespaces.ToList());
    }

    public class AstFieldDeclaration : AstMemberDeclaration
    {
        public AstNode Node { get; }
        public AstFieldDeclaration(AstIdentifier name, AstTypeNode type, AstNode node) : base(name, type, null) => Node = node;
        public override IEnumerable<AstNode> Children => base.Children.Append(Node);
        public static AstFieldDeclaration Create(AstIdentifier name, AstTypeNode type, AstNode node)
            => new AstFieldDeclaration(name, type, node);
    }

    public class AstPropertyDeclaration : AstMemberDeclaration
    {
        public AstNode Body { get; }
        public AstNode Value { get; }

        public AstPropertyDeclaration(AstIdentifier name, AstTypeNode type, AstNode body, AstNode value) : base(name, type, null)
        {
            Body = body;
            Value = value;
            if (body != null && value != null)
            {
                throw new Exception("Either the body is not null, or the value, not both");
            }
        }

        public override IEnumerable<AstNode> Children => base.Children.Append(Body).Append(Value);

        public static AstPropertyDeclaration Create(AstIdentifier name, AstTypeNode type, AstNode body, AstNode value)
            => new AstPropertyDeclaration(name, type, body, value);
    }

    public class AstParameterDeclaration : AstDeclaration
    {
        public AstTypeNode Type { get; }

        public AstParameterDeclaration(AstIdentifier name, AstTypeNode type) : base(name, null) => Type = type;
        public override IEnumerable<AstNode> Children => base.Children.Append(Type);

        public static AstParameterDeclaration Create(AstIdentifier name, AstTypeNode type)
            =>  new AstParameterDeclaration(name, type);
    }

    public class AstMethodDeclaration : AstMemberDeclaration
    {
        public AstNode Body { get; }
        public IReadOnlyList<AstIdentifier> TypeParameters { get; }
        public IReadOnlyList<AstParameterDeclaration> Parameters { get; }

        public AstMethodDeclaration(AstIdentifier name,
            AstTypeNode type,
            IEnumerable<AstParameterDeclaration> parameters,
            IEnumerable<AstIdentifier> typeParameters, AstNode body) :
            base(name, type, null)
        {
            Body = body;
            Parameters = parameters.ToList();
            TypeParameters = typeParameters.ToList();
        }

        public static AstMethodDeclaration Create(AstIdentifier name,
            AstTypeNode type,
            IEnumerable<AstParameterDeclaration> parameters,
            IEnumerable<AstIdentifier> typeParameters, AstNode body)
            => new AstMethodDeclaration(name, type, parameters, typeParameters, body);

        public override IEnumerable<AstNode> Children =>
            base.Children.Append(Body).Concat(TypeParameters);
    }

    public class AstExpressionStatement : AstNode
    {
        public AstNode Expression { get; }
        public AstExpressionStatement(AstNode expression) => Expression = expression;
        public static AstExpressionStatement Create(AstNode expression)
            => new AstExpressionStatement(expression);
        public override IEnumerable<AstNode> Children => base.Children.Append(Expression);
    }

    public class AstTypeDeclaration : AstDeclaration
    {
        public IReadOnlyList<AstIdentifier> TypeParameters { get; }
        public IReadOnlyList<AstTypeNode> BaseTypes { get; }
        public IReadOnlyList<AstMemberDeclaration> Members { get; }

        public AstTypeDeclaration(AstIdentifier name, IEnumerable<AstIdentifier> typeParameters,
            IEnumerable<AstTypeNode> baseTypes, IEnumerable<AstAttribute> attributes, params AstMemberDeclaration[] members) : base(name, attributes.ToList())
        {
            TypeParameters = typeParameters.ToList();
            BaseTypes = baseTypes.ToList();
            Members = members;
        }

        public override IEnumerable<AstNode> Children =>
            base.Children.Concat(TypeParameters).Concat(BaseTypes).Concat(Members);
    }

    public class AstProject : AstDeclaration
    {
        public IReadOnlyList<AstFile> Files { get; }

        public AstProject(AstIdentifier name, IReadOnlyList<AstFile> files) : base(name, null)
        {
            Files = files;
        }
    }

    public class AstFile : AstDeclaration
    {
        public AstNamespace Namespace { get; }

        public AstFile(AstIdentifier name, AstNamespace ns) : base(name, null)
        {
            Namespace = ns;
        }
    }

    public static class AstNodeExtensions
    {
        public static IEnumerable<AstNode> GetAllDescendants(this AstNode node)
        {
            if (node == null)
                yield break;
            ;
            yield return node;
            foreach (var child in node.Children.SelectMany(GetAllDescendants))
                yield return child;
        }

        public static IEnumerable<AstTypeDeclaration> GetAllTypes(this AstNode node)
            => node.GetAllDescendants().OfType<AstTypeDeclaration>();
    }
}