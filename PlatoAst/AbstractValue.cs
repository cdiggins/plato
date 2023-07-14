using System;
using System.Collections.Generic;
using System.Linq;

namespace PlatoAst
{
    /// <summary>
    /// An abstract value has a location, scope, and a type.
    /// It is used for type-checking, and method overload resolution. 
    /// </summary>
    public abstract class AbstractValue
    {
        public AstNode Location { get; }
        public AbstractTypeRef Type { get; }
        public Scope Scope { get; }
        public string Name { get; }

        protected AbstractValue(AstNode location, Scope scope, AbstractTypeRef type, string name)
            => (Location, Scope, Type, Name) = (location, scope, type, name);

        public abstract IReadOnlyList<AbstractValue> Children { get; }
    }

    public class AbstractNoValue : AbstractValue
    {
        public AbstractNoValue() 
            : base(null, null, null, "$novalue")
        { }

        public static AbstractNoValue Instance = new AbstractNoValue();

        public override IReadOnlyList<AbstractValue> Children => Array.Empty<AbstractValue>();
    }

    /// <summary>
    /// Represents an untyped region of code.
    /// Could be a project, file, namespace, type-declaration, statement. 
    /// </summary>
    public class AbstractRegion : AbstractValue
    {
        public override IReadOnlyList<AbstractValue> Children { get; }

        public AbstractRegion(AstNode location, Scope scope, params AbstractValue[] children)
            : base(location, scope, AbstractTypeRef.Void, "$region")
            => Children = children;
    }

    public class AbstractTypeDef : AbstractValue
    {
        public string Kind => AstTypeDeclaration.Kind;
        public AstTypeDeclaration AstTypeDeclaration => Location as AstTypeDeclaration;
        public List<AbstractMethodDef> Methods { get; } = new List<AbstractMethodDef>();
        public List<AbstractFieldDef> Fields { get; } = new List<AbstractFieldDef>();
        public List<AbstractTypeParameterDef> TypeParameters { get; } = new List<AbstractTypeParameterDef>();
        public Dictionary<string, AstNode> Lookup { get; } = new Dictionary<string, AstNode>();

        public AbstractTypeDef(AstTypeDeclaration location, Scope scope)
            : base(location, scope, AbstractTypeRef.Create(location.Name), location.Name)
        {
        }

        public override IReadOnlyList<AbstractValue> Children => Array.Empty<AbstractValue>().Concat(Methods).Concat(Fields).Concat(TypeParameters).ToList();
    }

    public class AbstractConditional : AbstractValue
    {
        public AbstractValue Condition { get; }
        public AbstractValue IfTrue { get; }
        public AbstractValue IfFalse { get; }

        public AbstractConditional(AstNode location, Scope scope, AbstractValue condition, AbstractValue ifTrue, AbstractValue ifFalse)
            : base(location, scope, AbstractTypeRef.Unify(ifTrue.Type, ifFalse.Type), "conditional")
        {
            // TODO: unify the types of ifTrue and ifFalse
            Condition = condition;
            IfTrue = ifTrue;
            IfFalse = ifFalse;
        }

        public override IReadOnlyList<AbstractValue> Children => new []{ Condition, IfTrue, IfFalse};
    }

    public class AbstractFunction : AbstractValue
    {
        public IReadOnlyList<AbstractParameter> Parameters { get; }
        public AbstractValue Body { get; }

        public AbstractFunction(AstNode location, Scope scope, string name, AbstractTypeRef returnType, AbstractValue body, params AbstractParameter[] parameters)
            : base(location, scope, AbstractTypeRef.CreateFunction(returnType, parameters.Select(p => p.Type).ToArray()), name)
        {
            Parameters = parameters;
            Body = body;
        }

        public override IReadOnlyList<AbstractValue> Children => Parameters.Append(Body).ToList();
    }

    public class AbstractParameter : AbstractValue
    {   
        public AbstractParameter(AstNode location, Scope scope, string name, AbstractTypeRef type)
            : base(location, scope, type, name)
        { }

        public override IReadOnlyList<AbstractValue> Children => Array.Empty<AbstractValue>();
    }

    public class AbstractVariable : AbstractValue
    {
        public AbstractVariable(AstNode location, Scope scope, string name, AbstractTypeRef type)
            : base(location, scope, type, name)
        { }

        public override IReadOnlyList<AbstractValue> Children => Array.Empty<AbstractValue>();
    }

    public class AbstractAssignment : AbstractValue
    {
        public AbstractValue LValue { get; }
        public AbstractValue RValue { get; }

        public AbstractAssignment(AstNode location, Scope scope, AbstractValue lvalue, AbstractValue rvalue)
            : base(location, scope, lvalue.Type, "$assign")
            => (LValue, RValue) = (lvalue, rvalue);

        public override IReadOnlyList<AbstractValue> Children => new [] { LValue, RValue };
    }

    public class AbstractArgument : AbstractValue
    {
        public AbstractValue Original { get; }
        public int Position { get; }

        public AbstractArgument(AstNode location, Scope scope, AbstractValue original, int position)
            : base(location, scope, original?.Type, $"$arg{position}")
        {
            Original = original;
            Position = position;
        }

        public override IReadOnlyList<AbstractValue> Children => new [] { Original };
    }

    public class Literal : AbstractValue
    {
        public object Value { get; }
        public Literal(AstNode location, Scope scope, object value)
            : base(location, scope, new AbstractTypeRef(value.GetType()), "$literal")
            => Value = value;

        public override IReadOnlyList<AbstractValue> Children => Array.Empty<AbstractValue>();
    }

    public class AbstractMemberRef : AbstractValue
    {
        public AbstractValue Receiver { get; }

        public AbstractMemberRef(AstNode location, Scope scope, string name, AbstractValue receiver)
            : base(location, scope, AbstractTypeRef.Member, name)
        {
            Receiver = receiver;
        }

        public override IReadOnlyList<AbstractValue> Children => new [] { Receiver };
    }

    public class AbstractFunctionResult : AbstractValue
    {
        public AbstractValue Function { get; }
        public IReadOnlyList<AbstractArgument> Args { get; }
        public AbstractFunctionResult(AstNode location, Scope scope, AbstractTypeRef resultType, AbstractValue function, params AbstractArgument[] args)
            : base(location, scope, resultType, "$result")
        {
            Function = function;
            Args = args;
        }

        public override IReadOnlyList<AbstractValue> Children => Args.Append(Function).ToList();
    }

    public class AbstractTypeRef : AbstractValue
    {
        public IReadOnlyList<AbstractTypeRef> TypeArgs { get; }

        public AbstractTypeRef(AstNode location, Scope scope, string name, params AbstractTypeRef[] args)
            : base(location, scope, MetaType, name)
        {
            TypeArgs = args;
        }

        // TODO: handle type arguments 
        public AbstractTypeRef(Type type)
            : this(null, null, type.Name)
        { }

        public static AbstractTypeRef Create(AstTypeNode typeNode, Scope scope)
            => new AbstractTypeRef(typeNode, scope, typeNode.Name, typeNode.TypeArguments.Select(ta => Create(ta, scope)).ToArray());

        public static AbstractTypeRef Create(string name, params AbstractTypeRef[] args)
            => new AbstractTypeRef(null, null, name, args);

        public static AbstractTypeRef Void = Create("void");
        public static AbstractTypeRef Intrinsic = Create("intrinsic");
        public static AbstractTypeRef MetaType = Create("MetaType");
        public static AbstractTypeRef NotImplemented = Create("Not implemented yet");
        public static AbstractTypeRef TypeParameter = Create("AbstractTypeParameterDef");
        public static AbstractTypeRef Member = Create("AbstractMember");
        public static AbstractTypeRef Inferred = Create("Infer");

        public static AbstractTypeRef CreateFunction(AbstractTypeRef returnType, params AbstractTypeRef[] parameterTypes)
            => Create("Func", parameterTypes.Append(returnType).ToArray());

        public static AbstractTypeRef Unify(AbstractTypeRef a, AbstractTypeRef b)
            => a;

        public override IReadOnlyList<AbstractValue> Children => TypeArgs;
    }

    public abstract class AbstractMember : AbstractValue
    {
        protected AbstractMember(AstNode location, Scope scope, AbstractTypeRef type, string name)
            : base(location, scope, type, name)
        { }
    }

    public class AbstractFieldDef : AbstractMember
    {
        public AbstractFieldDef(AstNode location, Scope scope, AbstractTypeRef type, string name)
            : base(location, scope, type, name)
        { }

        public override IReadOnlyList<AbstractValue> Children => Array.Empty<AbstractValue>();
    }

    public class AbstractMethodDef : AbstractMember
    {
        // TODO: find a better way to deal with this. Don't make it mutable.
        public AbstractFunction AbstractFunction { get; set; }

        public AbstractMethodDef(AstNode location, Scope scope, AbstractTypeRef type, string name)
            : base(location, scope, type, name)
        { }

        public override IReadOnlyList<AbstractValue> Children => new [] { AbstractFunction };
    }

    public class AbstractTypeParameterDef : AbstractMember
    {
        public AbstractTypeParameterDef(AstNode location, Scope scope, string name)
            : base(location, scope, AbstractTypeRef.TypeParameter, name)
        { }

        public override IReadOnlyList<AbstractValue> Children => Array.Empty<AbstractValue>();
    }

    public class AbstractIntrinsic : AbstractValue
    {
        public AbstractIntrinsic(AstNode location, Scope scope, string name)
            : base(location, scope, AbstractTypeRef.Intrinsic, name)
        { }
    
        public override IReadOnlyList<AbstractValue> Children => Array.Empty<AbstractValue>();
    }

}