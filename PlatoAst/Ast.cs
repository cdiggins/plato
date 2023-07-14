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

    public class AstLeaf : AstNode
    {
        public string Text { get; }
        public AstLeaf(string text = "") => Text = text;
        public override string ToString() => Text;
    }

    public class AstIdentifier : AstLeaf
    {
        public AstIdentifier(string text) : base(text) { }
    }

    public class AstParenthesized : AstNode
    {
        public AstNode Inner;
        public AstParenthesized(AstNode inner) => Inner = inner;
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
        public string Name { get; }

        public AstIntrinsic(string name) => Name = name;
        public static AstIntrinsic Create(string name) => new AstIntrinsic(name);
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
        public string Name { get; }

        public AstMemberAccess(AstNode reciever, string name) => (Receiver, Name) = (reciever, name);
        public static AstMemberAccess Create(AstNode reciever, string name) => new AstMemberAccess(reciever, name);
        public override IEnumerable<AstNode> Children => base.Children.Append(Receiver);
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
        public string Name { get; }
        public AstNode Value { get; }
        public AstTypeNode Type { get; }

        public AstVarDef(string name, AstNode value, AstTypeNode type) => (Name, Value, Type) = (name, value, type);
        public static AstVarDef Create(string name, AstNode value, AstTypeNode type) =>  new AstVarDef(name, value, type);
        public static AstVarDef Create(string name, AstTypeNode type) => Create(name, AstNoop.Default, type);
        public override IEnumerable<AstNode> Children => base.Children.Append(Value).Append(Type);
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
        public string Name { get; }
        public IReadOnlyList<AstTypeNode> TypeArguments { get; }

        public AstTypeNode(string name, params AstTypeNode[] args) => (Name, TypeArguments) = (name, args);
        public static AstTypeNode Create(string name, params AstTypeNode[] args) => new AstTypeNode(name, args);
        public override IEnumerable<AstNode> Children => TypeArguments;
    }

    public abstract class AstDeclaration : AstNode
    {
        public string Name { get; }
        
        protected AstDeclaration(string name)
        {
            Name = name;
        }
    }

    public abstract class AstMemberDeclaration : AstDeclaration
    {
        public AstTypeNode Type { get; }
        protected AstMemberDeclaration(string name, AstTypeNode type) : base(name) 
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
        public AstNamespace(string name, IEnumerable<AstDirective> directives, IEnumerable<AstTypeDeclaration> types, IEnumerable<AstNamespace> namespaces)
        : base(name)
            => (Types, Directives, Namespaces) = (
                types.ToListOrEmpty(), directives.ToListOrEmpty(), namespaces.ToListOrEmpty());
    }

    public class AstFieldDeclaration : AstMemberDeclaration
    {
        public AstNode Node { get; }
        public AstFieldDeclaration(string name, AstTypeNode type, AstNode node) : base(name, type) => Node = node;
        public override IEnumerable<AstNode> Children => base.Children.Append(Node);
        public static AstFieldDeclaration Create(string name, AstTypeNode type, AstNode node)
            => new AstFieldDeclaration(name, type, node);
    }

    public class AstParameterDeclaration : AstDeclaration
    {
        public AstTypeNode Type { get; }

        public AstParameterDeclaration(string name, AstTypeNode type) : base(name) => Type = type;
        public override IEnumerable<AstNode> Children => base.Children.Append(Type);

        public static AstParameterDeclaration Create(string name, AstTypeNode type)
            =>  new AstParameterDeclaration(name, type);
    }

    public class AstMethodDeclaration : AstMemberDeclaration
    {
        public AstNode Body { get; }
        public IReadOnlyList<AstTypeParameter> TypeParameters { get; }
        public IReadOnlyList<AstParameterDeclaration> Parameters { get; }

        public AstMethodDeclaration(string name,
            AstTypeNode type,
            IEnumerable<AstParameterDeclaration> parameters,
            IEnumerable<AstTypeParameter> typeParameters, AstNode body) :
            base(name, type)
        {
            Body = body;
            Parameters = parameters.ToList();
            TypeParameters = typeParameters.ToList();
        }

        public static AstMethodDeclaration Create(string name,
            AstTypeNode type,
            IEnumerable<AstParameterDeclaration> parameters,
            IEnumerable<AstTypeParameter> typeParameters, AstNode body)
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

    public class AstTypeParameter : AstDeclaration
    {
        public AstTypeParameter(string name)
            : base(name) 
        { }
    }

    public class AstTypeDeclaration : AstDeclaration
    {
        public string Kind { get; }
        public IReadOnlyList<AstTypeParameter> TypeParameters { get; }
        public IReadOnlyList<AstTypeNode> BaseTypes { get; }
        public IReadOnlyList<AstMemberDeclaration> Members { get; }

        public AstTypeDeclaration(string kind, string name, IEnumerable<AstTypeParameter> typeParameters,
            IEnumerable<AstTypeNode> baseTypes, params AstMemberDeclaration[] members) 
            : base(name)
        {
            Kind = kind;
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

        public AstProject(string name, IReadOnlyList<AstFile> files) : base(name)
        {
            Files = files;
        }
    }

    public class AstFile : AstDeclaration
    {
        public AstNamespace Namespace { get; }

        public AstFile(string name, AstNamespace ns) : base(name)
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