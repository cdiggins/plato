using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.AST;
using Ara3D.Geometry.Compiler.Symbols;

namespace Ara3D.Geometry.Compiler.Checking
{
    /// <summary>
    /// The Typed IR (TIR): a small, fully-typed, fully-resolved node model produced by the
    /// <see cref="Elaborator"/> from a solved function. Every node carries its solved (zonked)
    /// <see cref="TypeExpression"/> — null only on the pure-statement nodes — and the originating
    /// <see cref="Symbol"/> for diagnostics.
    ///
    /// The point of the TIR is to record, once, everything the writers currently re-derive at emit
    /// time: which overload a call resolved to (<see cref="TirCall.Callee"/>), its instantiated
    /// signature, how it should be emitted (<see cref="EmissionKind"/>), and — crucially — every
    /// implicit conversion, made explicit as a <see cref="TirCoerce"/> node instead of an emit-time
    /// guess. It is shadow-mode: nothing consumes it yet.
    /// </summary>
    public abstract class TirNode
    {
        /// <summary>The solved (zonked) type of this node; null on statement nodes that have no value.</summary>
        public TypeExpression Type { get; }

        /// <summary>The symbol this node was elaborated from (for diagnostics / source location).</summary>
        public Symbol Origin { get; }

        protected TirNode(TypeExpression type, Symbol origin)
            => (Type, Origin) = (type, origin);

        /// <summary>The immediate TIR children, for tree walks. Default: none.</summary>
        public virtual IEnumerable<TirNode> Children => Enumerable.Empty<TirNode>();

        /// <summary>This node and every transitive child, pre-order.</summary>
        public IEnumerable<TirNode> Descendants()
        {
            yield return this;
            foreach (var c in Children)
                if (c != null)
                    foreach (var d in c.Descendants())
                        yield return d;
        }
    }

    /// <summary>How a resolved <see cref="TirCall"/> is meant to be emitted. Derived from the callee's
    /// <see cref="FunctionType"/> plus call shape by the elaborator; a classification for future
    /// writers, not yet consumed.</summary>
    public enum EmissionKind
    {
        InstanceMethod, // receiver.Method(args)
        Property,       // receiver.Member          (field getter / no-arg member)
        StaticMethod,   // Lib.Method(args)         (0-parameter / non-receiver)
        Operator,       // a + b, -x                (operator-named)
        Constructor,    // new T(args)
        Conversion,     // implicit/explicit cast   (Cast fn or type-named 1-arg)
        Intrinsic,      // handwritten runtime call
    }

    // --- leaves --------------------------------------------------------------

    public class TirLiteral : TirNode
    {
        public object Value { get; }
        public LiteralTypesEnum LiteralType { get; }
        public TirLiteral(object value, LiteralTypesEnum literalType, TypeExpression type, Symbol origin)
            : base(type, origin) => (Value, LiteralType) = (value, literalType);
        public override string ToString() => $"{Value}";
    }

    public class TirParameter : TirNode
    {
        public ParameterDef Def { get; }
        public TirParameter(ParameterDef def, TypeExpression type, Symbol origin)
            : base(type, origin) => Def = def;
        public override string ToString() => Def?.Name ?? "<param>";
    }

    public class TirVariable : TirNode
    {
        public VariableDef Def { get; }
        public TirVariable(VariableDef def, TypeExpression type, Symbol origin)
            : base(type, origin) => Def = def;
        public override string ToString() => Def?.Name ?? "<var>";
    }

    /// <summary>A bare type reference used as a value (e.g. a static qualifier).</summary>
    public class TirTypeRef : TirNode
    {
        public TypeDef TypeDef { get; }
        public TirTypeRef(TypeDef typeDef, TypeExpression type, Symbol origin)
            : base(type, origin) => TypeDef = typeDef;
        public override string ToString() => TypeDef?.Name ?? "<type>";
    }

    /// <summary>The <c>default</c> keyword. Its type is entirely contextual (carried in Type).</summary>
    public class TirDefault : TirNode
    {
        public TirDefault(TypeExpression type, Symbol origin) : base(type, origin) { }
        public override string ToString() => $"default({Type})";
    }

    // --- calls & coercions ---------------------------------------------------

    /// <summary>A resolved call to a named overload. Carries the winning callee, its instantiated
    /// signature, the elaborated arguments (with conversions already wrapped in
    /// <see cref="TirCoerce"/>), and how it should be emitted.</summary>
    public class TirCall : TirNode
    {
        public FunctionDef Callee { get; }
        public EmissionKind EmissionKind { get; }
        public IReadOnlyList<TypeExpression> ParameterTypes { get; }
        public TypeExpression ReturnType { get; }
        public IReadOnlyList<TirNode> Args { get; }
        public string Name => Callee?.Name;

        public TirCall(FunctionDef callee, EmissionKind emissionKind,
            IReadOnlyList<TypeExpression> parameterTypes, TypeExpression returnType,
            IReadOnlyList<TirNode> args, TypeExpression type, Symbol origin)
            : base(type, origin)
        {
            Callee = callee;
            EmissionKind = emissionKind;
            ParameterTypes = parameterTypes ?? new List<TypeExpression>();
            ReturnType = returnType;
            Args = args ?? new List<TirNode>();
        }

        public override IEnumerable<TirNode> Children => Args;
        public override string ToString() => $"{Name}[{EmissionKind}]({string.Join(", ", Args)})";
    }

    /// <summary>An explicit implicit-conversion: <see cref="Inner"/> (of <see cref="FromType"/>) is
    /// coerced to <see cref="ToType"/> via <see cref="ConversionFn"/>. THE point of this phase — the
    /// conversion class (Vector3(0.0), Number → Angle) is a deliberate IR node, not an emit-time
    /// guess.</summary>
    public class TirCoerce : TirNode
    {
        public TirNode Inner { get; }
        public TypeExpression FromType { get; }
        public TypeExpression ToType { get; }
        public IFunction ConversionFn { get; }

        public TirCoerce(TirNode inner, TypeExpression fromType, TypeExpression toType, IFunction conversionFn, Symbol origin)
            : base(toType, origin)
        {
            Inner = inner;
            FromType = fromType;
            ToType = toType;
            ConversionFn = conversionFn;
        }

        public override IEnumerable<TirNode> Children => new[] { Inner };
        public override string ToString() => $"coerce<{FromType}→{ToType}>({Inner})";
    }

    /// <summary>Applying a function *value* (a function-typed parameter/variable, a lambda, or the
    /// result of another call). Distinct from resolving a named overload — there is no callee
    /// FunctionDef and no EmissionKind; a writer emits it as an invocation.</summary>
    public class TirInvoke : TirNode
    {
        public TirNode Target { get; }
        public IReadOnlyList<TirNode> Args { get; }

        public TirInvoke(TirNode target, IReadOnlyList<TirNode> args, TypeExpression type, Symbol origin)
            : base(type, origin)
        {
            Target = target;
            Args = args ?? new List<TirNode>();
        }

        public override IEnumerable<TirNode> Children => new[] { Target }.Concat(Args);
        public override string ToString() => $"{Target}.invoke({string.Join(", ", Args)})";
    }

    // --- compound expressions ------------------------------------------------

    public class TirConditional : TirNode
    {
        public TirNode Condition { get; }
        public TirNode IfTrue { get; }
        public TirNode IfFalse { get; }

        public TirConditional(TirNode condition, TirNode ifTrue, TirNode ifFalse, TypeExpression type, Symbol origin)
            : base(type, origin) => (Condition, IfTrue, IfFalse) = (condition, ifTrue, ifFalse);

        public override IEnumerable<TirNode> Children => new[] { Condition, IfTrue, IfFalse };
        public override string ToString() => $"({Condition} ? {IfTrue} : {IfFalse})";
    }

    public class TirNew : TirNode
    {
        public TypeExpression NewType { get; }
        public IReadOnlyList<TirNode> Args { get; }

        public TirNew(TypeExpression newType, IReadOnlyList<TirNode> args, TypeExpression type, Symbol origin)
            : base(type, origin)
        {
            NewType = newType;
            Args = args ?? new List<TirNode>();
        }

        public override IEnumerable<TirNode> Children => Args;
        public override string ToString() => $"new {NewType}({string.Join(", ", Args)})";
    }

    public class TirArray : TirNode
    {
        public IReadOnlyList<TirNode> Elements { get; }

        public TirArray(IReadOnlyList<TirNode> elements, TypeExpression type, Symbol origin)
            : base(type, origin) => Elements = elements ?? new List<TirNode>();

        public override IEnumerable<TirNode> Children => Elements;
        public override string ToString() => $"[{string.Join(", ", Elements)}]";
    }

    public class TirAssign : TirNode
    {
        public TirNode LValue { get; }
        public TirNode RValue { get; }

        public TirAssign(TirNode lvalue, TirNode rvalue, TypeExpression type, Symbol origin)
            : base(type, origin) => (LValue, RValue) = (lvalue, rvalue);

        public override IEnumerable<TirNode> Children => new[] { LValue, RValue };
        public override string ToString() => $"({LValue} = {RValue})";
    }

    public class TirLambda : TirNode
    {
        public IReadOnlyList<ParameterDef> Parameters { get; }
        public TirNode Body { get; }

        public TirLambda(IReadOnlyList<ParameterDef> parameters, TirNode body, TypeExpression type, Symbol origin)
            : base(type, origin)
        {
            Parameters = parameters ?? new List<ParameterDef>();
            Body = body;
        }

        public override IEnumerable<TirNode> Children => new[] { Body };
        public override string ToString()
            => $"({string.Join(", ", Parameters.Select(p => p.Name))}) => {Body}";
    }

    /// <summary>Total-elaboration fallback: a call (or other symbol) the solver could not resolve
    /// cleanly. Keeps the pass total and the tree walkable (any child args are still elaborated).</summary>
    public class TirUnresolved : TirNode
    {
        public Symbol Original { get; }
        public string Reason { get; }
        public IReadOnlyList<TirNode> ChildNodes { get; }

        public TirUnresolved(Symbol original, string reason, IReadOnlyList<TirNode> children = null)
            : base(null, original)
        {
            Original = original;
            Reason = reason;
            ChildNodes = children ?? new List<TirNode>();
        }

        public override IEnumerable<TirNode> Children => ChildNodes;
        public override string ToString() => $"<unresolved {Original?.Name}: {Reason}>";
    }

    // --- statements ----------------------------------------------------------

    public class TirBlock : TirNode
    {
        public IReadOnlyList<TirNode> Statements { get; }
        public TirBlock(IReadOnlyList<TirNode> statements, Symbol origin)
            : base(null, origin) => Statements = statements ?? new List<TirNode>();
        public override IEnumerable<TirNode> Children => Statements;
        public override string ToString() => $"{{ {string.Join("; ", Statements)} }}";
    }

    public class TirReturn : TirNode
    {
        public TirNode Value { get; }
        public TirReturn(TirNode value, Symbol origin) : base(value?.Type, origin) => Value = value;
        public override IEnumerable<TirNode> Children => new[] { Value };
        public override string ToString() => $"return {Value};";
    }

    public class TirIf : TirNode
    {
        public TirNode Condition { get; }
        public TirNode IfTrue { get; }
        public TirNode IfFalse { get; }
        public TirIf(TirNode condition, TirNode ifTrue, TirNode ifFalse, Symbol origin)
            : base(null, origin) => (Condition, IfTrue, IfFalse) = (condition, ifTrue, ifFalse);
        public override IEnumerable<TirNode> Children => new[] { Condition, IfTrue, IfFalse };
        public override string ToString() => $"if ({Condition}) {IfTrue} else {IfFalse}";
    }

    public class TirLoop : TirNode
    {
        public TirNode Condition { get; }
        public TirNode Body { get; }
        public TirLoop(TirNode condition, TirNode body, Symbol origin)
            : base(null, origin) => (Condition, Body) = (condition, body);
        public override IEnumerable<TirNode> Children => new[] { Condition, Body };
        public override string ToString() => $"while ({Condition}) {Body}";
    }

    // --- top level -----------------------------------------------------------

    /// <summary>A fully elaborated function: the original definition plus its TIR body.</summary>
    public class TirFunction
    {
        public FunctionDef Original { get; }
        public IReadOnlyList<ParameterDef> Parameters { get; }
        public TypeExpression ReturnType { get; }
        public TirNode Body { get; }

        public TirFunction(FunctionDef original, IReadOnlyList<ParameterDef> parameters, TypeExpression returnType, TirNode body)
        {
            Original = original;
            Parameters = parameters ?? new List<ParameterDef>();
            ReturnType = returnType;
            Body = body;
        }

        /// <summary>Every TIR node in the body, pre-order (empty when there is no body).</summary>
        public IEnumerable<TirNode> AllNodes => Body?.Descendants() ?? Enumerable.Empty<TirNode>();
    }
}
