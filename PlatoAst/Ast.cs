using System.Collections.Generic;

namespace PlatoAst
{
    public class AstNode
    { }

    public class AstNoop : AstNode
    {
        private AstNoop() { }
        public static AstNoop Default { get; } = new AstNoop();
    }

    public class AstReducible : AstNode 
    { }

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

        public AstReturn(AstNode value) 
            => Value = value;

        public static AstReturn Create(AstNode value)
            => new AstReturn(value);
    }

    public class AstConstant : AstNode
    {
        public object Value { get; }

        public AstConstant(object value)
            => Value = value;

        public static AstConstant Create(object value)
            => new AstConstant(value);

        public static AstConstant<T> Create<T>(T value)
            => new AstConstant<T>(value);

        public static AstConstant Null
            => new AstConstant(null);

        public static AstConstant True 
            => Create(true);

        public static AstConstant False 
            => Create(false);
    }

    public class AstConstant<T> : AstConstant
    {
        public new T Value => 
            base.Value != null ? 
                (T)base.Value : default;

        public AstConstant(T value)
            : base(value)
        { }
    }

    public class AstLambda : AstNode
    {
        public IReadOnlyList<AstVarDef> Parameters { get; }
        public AstNode Body { get; }

        public AstLambda(AstNode body, params AstVarDef[] parameters)
            => (Parameters, Body) = (parameters, body);

        public static AstLambda Create(AstNode body, params AstVarDef[] parameters)
            => new AstLambda(body, parameters);
    }

    public class AstMulti : AstNode
    {
        public IReadOnlyList<AstNode> Nodes { get; }

        public AstMulti(params AstNode[] nodes)
            => Nodes = nodes;

        public static AstMulti Create(params AstNode[] nodes)
            => new AstMulti(nodes);
    }

    public class AstBlock : AstNode
    {
        public IReadOnlyList<AstNode> Statements { get; }

        public AstBlock(params AstNode[] statements)
            => Statements = statements;

        public static AstBlock Create(params AstNode[] statements)
            => new AstBlock(statements);
    }

    public class AstLoop : AstNode
    {
        public AstNode Condition { get; }
        public AstNode Body { get; }

        public AstLoop(AstNode condition, AstNode body)
            => (Condition, Body) = (condition, body);

        public static AstLoop Create(AstNode condition, AstNode body)
            => new AstLoop(condition, body);
    }

    public class AstConditional : AstNode
    {
        public AstNode Condition { get; }
        public AstNode IfTrue { get; }
        public AstNode IfFalse { get; }

        public AstConditional(AstNode condition, AstNode ifTrue, AstNode ifFalse)
            => (Condition, IfTrue, IfFalse) = (condition, ifTrue, ifFalse);

        public static AstConditional Create(AstNode condition, AstNode ifTrue, AstNode ifFalse)
            => new AstConditional(condition, ifTrue, ifFalse);
    }

    public class AstVarDef : AstNode
    {
        public string Name { get; }
        public AstNode Value { get; }

        public AstVarDef(string name, AstNode value)
            => (Name, Value) = (name, value);

        public static AstVarDef Create(string name, AstNode value)
            => new AstVarDef(name, value);

        public static AstVarDef Create(string name)
            => Create(name, AstNoop.Default);
    }

    public class AstVarRef : AstNode
    {
        public string Name { get; }

        public AstVarRef(string name)
            => Name = name;

        public static AstVarRef Create(string name)
            => new AstVarRef(name);

        public static implicit operator AstVarRef(string name)
            => Create(name);
    }

    public class AstAssign : AstNode
    {
        public AstVarRef Var { get; }
        public AstNode Value { get; }

        public AstAssign(AstVarRef var, AstNode value)
            => (Var, Value) = (var, value);

        public static AstAssign Create(AstVarRef var, AstNode value)
            => new AstAssign(var, value);
    }

    public class AstInvoke : AstNode
    {
        public AstNode Function { get; }
        public IReadOnlyList<AstNode> AstArguments { get; }

        public AstInvoke(AstNode function, params AstNode[] arguments)
            => (Function, AstArguments) = (function, arguments);

        public static AstInvoke Create(AstNode function, params AstNode[] arguments)
            => new AstInvoke(function, arguments);
    }
}
