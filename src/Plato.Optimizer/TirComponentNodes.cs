// The marker nodes the component unroller and the shared passes build: a field read, a constructor call and a
// boolean chain. Backend-neutral TIR; each writer prints them.
using System.Collections.Generic;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Symbols;

namespace Ara3D.Geometry.Optimizer;

/// <summary>Emission-only marker: direct component access "recv.Field", or
/// "((CastTo)recv.Field)". The TIR analogue of C# <c>ComponentAccessExpression</c>.</summary>
public class TirComponentAccess : TirNode
{
    public TirNode Receiver { get; }
    public string FieldName { get; }
    public string CastTo { get; }

    // Scalar erasure only (null otherwise): the primitive this component erases to. Mirrors
    // ComponentAccessExpression.ScalarComponentPrim — under erasure, scalar-receiver method-call
    // syntax decisions can't come from the ORIGIN symbols (they still show the pre-unroll lambda
    // parameters), so the marker itself carries the knowledge.
    public string ScalarComponentPrim { get; }

    public TirComponentAccess(TirNode receiver, string fieldName, string castTo, string scalarComponentPrim = null, TypeExpression type = null)
        : base(type, receiver?.Origin)
    {
        Receiver = receiver;
        FieldName = fieldName;
        CastTo = castTo;
        ScalarComponentPrim = scalarComponentPrim;
    }

    public override IEnumerable<TirNode> Children => new[] { Receiver };
    public override TirNode WithChildren(IReadOnlyList<TirNode> children)
        => new TirComponentAccess(children[0], FieldName, CastTo, ScalarComponentPrim, Type);
    public override string ToString() => $"{Receiver}.{FieldName}";
}

/// <summary>Emission-only marker: "new TypeName(args...)". The TIR analogue of
/// C# <c>ConstructorCallExpression</c>.</summary>
public class TirConstructorCall : TirNode
{
    public string TypeName { get; }
    public IReadOnlyList<TirNode> Args { get; }

    public TirConstructorCall(string typeName, IReadOnlyList<TirNode> args, TypeExpression type = null)
        : base(type, null)
    {
        TypeName = typeName;
        Args = args ?? new List<TirNode>();
    }

    public override IEnumerable<TirNode> Children => Args;
    public override TirNode WithChildren(IReadOnlyList<TirNode> children)
        => new TirConstructorCall(TypeName, children, Type);
    public override string ToString() => $"new {TypeName}({string.Join(", ", Args)})";
}

/// <summary>Emission-only marker: "t0 &amp;&amp; t1 &amp;&amp; ..." (or "||"). The TIR analogue
/// of C# <c>BooleanChainExpression</c>.</summary>
public class TirBooleanChain : TirNode
{
    public string Op { get; }
    public IReadOnlyList<TirNode> Terms { get; }

    public TirBooleanChain(string op, IReadOnlyList<TirNode> terms, TypeExpression type = null)
        : base(type, null)
    {
        Op = op;
        Terms = terms ?? new List<TirNode>();
    }

    public override IEnumerable<TirNode> Children => Terms;
    public override TirNode WithChildren(IReadOnlyList<TirNode> children)
        => new TirBooleanChain(Op, children, Type);
    public override string ToString() => string.Join($" {Op} ", Terms);
}
