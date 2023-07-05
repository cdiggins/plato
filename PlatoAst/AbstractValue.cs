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
        public TypeRef Type { get; }
        public Scope Scope { get; }
        public string Name { get; }

        protected AbstractValue(AstNode location, Scope scope, TypeRef type, string name)
            => (Location, Scope, Type, Name) = (location, scope, type, name);
    }

    public class NoValue : AbstractValue
    {
        public NoValue() 
            : base(null, null, null, "$novalue")
        { }

        public static NoValue Instance = new NoValue();
    }

    /// <summary>
    /// Represents an untyped region of code.
    /// Could be a project, file, namespace, type-declaration, statement. 
    /// </summary>
    public class Region : AbstractValue
    {
        public IReadOnlyList<AbstractValue> Children { get; }

        public Region(AstNode location, Scope scope, params AbstractValue[] children)
            : base(location, scope, TypeRef.Void, "$region")
            => Children = children;
    }

    public class Conditional : AbstractValue
    {
        public AbstractValue Condition { get; }
        public AbstractValue IfTrue { get; }
        public AbstractValue IfFalse { get; }

        public Conditional(AstNode location, Scope scope, AbstractValue condition, AbstractValue ifTrue, AbstractValue ifFalse)
            : base(location, scope, TypeRef.Unify(ifTrue.Type, ifFalse.Type), "conditional")
        {
            // TODO: unify the types of ifTrue and ifFalse
            Condition = condition;
            IfTrue = ifTrue;
            IfFalse = ifFalse;
        }
    }

    public class Function : AbstractValue
    {
        public IReadOnlyList<Parameter> Parameters { get; }
        public AbstractValue Body { get; }

        public Function(AstNode location, Scope scope, string name, TypeRef returnType, params Parameter[] parameters)
            : base(location, scope, TypeRef.CreateFunction(returnType, parameters.Select(p => p.Type).ToArray()), name)
        {
            Parameters = parameters;
        }
    }

    public class Parameter : AbstractValue
    {   
        public Parameter(AstNode location, Scope scope, string name, TypeRef type)
            : base(location, scope, type, name)
        { }
    }

    public class Variable : AbstractValue
    {
        public Variable(AstNode location, Scope scope, string name, TypeRef type)
            : base(location, scope, type, name)
        { }
    }

    public class Assignment : AbstractValue
    {
        public AbstractValue LValue { get; }
        public AbstractValue RValue { get; }

        public Assignment(AstNode location, Scope scope, AbstractValue lvalue, AbstractValue rvalue)
            : base(location, scope, lvalue.Type, "$assign")
            => (LValue, RValue) = (lvalue, rvalue);
    }

    public class Argument : AbstractValue
    {
        public AbstractValue Original { get; }
        public int Position { get; }

        public Argument(AstNode location, Scope scope, AbstractValue original, int position)
            : base(location, scope, original.Type, $"$arg{position}")
        {
            Original = original;
            Position = position;
        }
    }

    public class Literal : AbstractValue
    {
        public object Value { get; }
        public Literal(AstNode location, Scope scope, object value)
            : base(location, scope, new TypeRef(value.GetType()), "$literal")
            => Value = value;
    }

    public class MemberRef : AbstractValue
    {
        public AbstractValue Receiver { get; }
        public TypeDef TypeDef { get; }
        public Member Member { get; }

        public MemberRef(AstNode location, Scope scope, TypeDef typeDef, Member member, AbstractValue receiver)
            : base(location, scope, member.Type, "$member")
        {
            Receiver = receiver;
            TypeDef = typeDef;
            Member = member;
        }
    }

    public class FunctionResult : AbstractValue
    {
        public AbstractValue Function { get; }
        public IReadOnlyList<Argument> Args { get; }
        public FunctionResult(AstNode location, Scope scope, TypeRef resultType, AbstractValue function, params Argument[] args)
            : base(location, scope, resultType, "$result")
        {
            Function = function;
            Args = args;
        }
    }

    public class TypeRef : AbstractValue
    {
        public IReadOnlyList<TypeRef> TypeArgs { get; }

        public TypeRef(AstNode location, Scope scope, string name, params TypeRef[] args)
            : base(location, scope, MetaType, name)
        {
            TypeArgs = args;
        }

        // TODO: handle type arguments 
        public TypeRef(Type type)
            : this(null, null, type.Name)
        { }

        public static TypeRef Create(string name, params TypeRef[] args)
            => new TypeRef(null, null, name, args);

        public static TypeRef Void = Create("void");
        public static TypeRef MetaType = Create("MetaType");
        public static TypeRef NotImplemented = Create("Not implemented yet");

        public static TypeRef CreateFunction(TypeRef returnType, params TypeRef[] parameterTypes)
            => Create("Func", parameterTypes.Append(returnType).ToArray());

        public static TypeRef Unify(TypeRef a, TypeRef b)
            => throw new NotImplementedException();
    }

    public class TypeDef : AbstractValue
    {
        public IReadOnlyList<string> TypeParameters { get; }
        public IReadOnlyList<TypeDef> NestedTypes { get; }
        public IReadOnlyList<Member> Members { get; }

        public TypeDef(AstNode location, Scope scope, string name, IEnumerable<string> typeParameters,
            IEnumerable<TypeDef> nested, IEnumerable<Member> members)
            : base(location, scope, TypeRef.MetaType, name)
        {
            TypeParameters = typeParameters?.ToArray() ?? Array.Empty<string>();
            NestedTypes = nested?.ToArray() ?? Array.Empty<TypeDef>();
            Members = members?.ToArray() ?? Array.Empty<Member>();
        }
    }

    public abstract class Member : AbstractValue
    {
        protected Member(AstNode location, Scope scope, TypeRef type, string name)
            : base(location, scope, type, name)
        { }
    }

    public class Field : Member
    {
        public Variable Variable { get; }

        public Field(AstNode location, Scope scope, Variable variable)
            : base(location, scope, variable.Type, variable.Name)
            => Variable = variable;
    }

    public class Method : Member
    {
        public Function Function { get; }

        public Method(AstNode location, Scope scope, Function function)
            : base(location, scope, function.Type, function.Name)
            => Function = function;
    }

    public class Namespace : AbstractValue
    {
        public List<AbstractValue> Children { get; }

        public Namespace(AstNode location, Scope scope, string name, IEnumerable<AbstractValue> children)
            : base(location, scope, TypeRef.Void, name)
        {
            Children = children.ToList();
        }
    }

    public static class ValueExtensions
    {
    }
}