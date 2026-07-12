using System;
using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Symbols;

namespace Ara3D.Geometry.CSharpWriter;

// The TIR mirror of ComponentUnroller (--optimize, roadmap P3.1): rewrites recognized
// component-HOF call sites in a TIR body to direct field expressions, using the same shape
// guards and the same all-or-nothing safety rule, so the TIR emit path reproduces the legacy
// optimizer output byte-for-byte (asserted by OptimizeEmitFlagOnTests). See ComponentUnroller
// for the full design rationale; only the node model differs here.

/// <summary>Emission-only marker: direct component access "recv.Field", or
/// "((CastTo)recv.Field)". The TIR analogue of <see cref="ComponentAccessExpression"/>.</summary>
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

    public TirComponentAccess(TirNode receiver, string fieldName, string castTo, string scalarComponentPrim = null)
        : base(null, receiver?.Origin)
    {
        Receiver = receiver;
        FieldName = fieldName;
        CastTo = castTo;
        ScalarComponentPrim = scalarComponentPrim;
    }

    public override IEnumerable<TirNode> Children => new[] { Receiver };
    public override string ToString() => $"{Receiver}.{FieldName}";
}

/// <summary>Emission-only marker: "new TypeName(args...)". The TIR analogue of
/// <see cref="ConstructorCallExpression"/>.</summary>
public class TirConstructorCall : TirNode
{
    public string TypeName { get; }
    public IReadOnlyList<TirNode> Args { get; }

    public TirConstructorCall(string typeName, IReadOnlyList<TirNode> args)
        : base(null, null)
    {
        TypeName = typeName;
        Args = args ?? new List<TirNode>();
    }

    public override IEnumerable<TirNode> Children => Args;
    public override string ToString() => $"new {TypeName}({string.Join(", ", Args)})";
}

/// <summary>Emission-only marker: "t0 &amp;&amp; t1 &amp;&amp; ..." (or "||"). The TIR analogue
/// of <see cref="BooleanChainExpression"/>.</summary>
public class TirBooleanChain : TirNode
{
    public string Op { get; }
    public IReadOnlyList<TirNode> Terms { get; }

    public TirBooleanChain(string op, IReadOnlyList<TirNode> terms)
        : base(null, null)
    {
        Op = op;
        Terms = terms ?? new List<TirNode>();
    }

    public override IEnumerable<TirNode> Children => Terms;
    public override string ToString() => string.Join($" {Op} ", Terms);
}

public static class TirComponentUnroller
{
    /// <summary>Convenience wrapper: the function with its body unrolled, or the function
    /// unchanged when nothing was (safely) unrollable or the optimizer is off.</summary>
    public static TirFunction UnrollFunction(TirFunction tir, CSharpFunctionInfo fi, CSharpWriter writer)
    {
        if (!writer.Optimize || tir?.Body == null)
            return tir;
        var body = TryUnroll(tir.Body, fi, writer);
        return body == null
            ? tir
            : new TirFunction(tir.Original, tir.Parameters, tir.ReturnType, body,
                tir.ZonkedParameterTypes, tir.ZonkedReturnType);
    }

    private static readonly HashSet<string> CandidateNames = new HashSet<string>
    {
        "MapComponents", "ZipComponents",
        "AllZipComponents", "AnyZipComponents",
        "AllComponents", "AnyComponent",
        "Reduce",
    };

    /// <summary>
    /// Attempts to unroll all recognized component-HOF call sites in the TIR body. Returns the
    /// rewritten body, or null if nothing was (safely) unrollable — same contract as
    /// <see cref="ComponentUnroller.TryUnroll"/>, which supplies the decision rules.
    /// </summary>
    public static TirNode TryUnroll(TirNode body, CSharpFunctionInfo fi, CSharpWriter writer)
    {
        // Same scope rule as the legacy unroller: expression bodies only.
        if (body == null || TirRewrite.IsStatementNode(body))
            return null;

        if (!body.Descendants().OfType<TirCall>().Any(IsCandidateCall))
            return null;

        // Monomorphized C# type per parameter definition of this function instance.
        var paramTypes = new Dictionary<ParameterDef, string>();
        var impl = fi.Function.Implementation;
        for (var i = 0; i < impl.Parameters.Count && i < fi.ParameterTypes.Count; ++i)
            paramTypes[impl.Parameters[i]] = fi.ParameterTypes[i];

        var changed = false;
        var result = Rewrite(body, n =>
        {
            if (n is TirCall call)
            {
                var u = TryUnrollCall(call, paramTypes, writer);
                if (u != null)
                {
                    changed = true;
                    return u;
                }
            }
            return n;
        });

        if (!changed)
            return null;

        // All-or-nothing safety: a surviving lambda would meet marker nodes it cannot traverse
        // in the downstream capture rewrite (same rule as the legacy unroller).
        if (result.Descendants().OfType<TirLambda>().Any())
            return null;

        return result;
    }

    private static bool IsCandidateCall(TirCall call)
        => call.Name != null
           && CandidateNames.Contains(call.Name)
           && call.Args.Count >= 2
           && Strip(call.Args[call.Args.Count - 1]) is TirLambda;

    // The legacy unroller decides on the symbol tree, which has no solver-inserted conversion
    // wrappers; strip TirCoerce for shape tests (rendering suppresses them anyway).
    private static TirNode Strip(TirNode n)
        => n is TirCoerce c ? Strip(c.Inner) : n;

    private static TirNode TryUnrollCall(TirCall fc, Dictionary<ParameterDef, string> paramTypes, CSharpWriter writer)
    {
        if (!IsCandidateCall(fc))
            return null;

        var name = fc.Name;
        var argc = fc.Args.Count;

        int vecArgCount;
        switch (name)
        {
            case "MapComponents" when argc == 2: vecArgCount = 1; break;
            case "ZipComponents" when argc == 3 || argc == 4: vecArgCount = argc - 1; break;
            case "AllZipComponents" when argc == 3 || argc == 4: vecArgCount = argc - 1; break;
            case "AnyZipComponents" when argc == 3 || argc == 4: vecArgCount = argc - 1; break;
            case "AllComponents" when argc == 2: vecArgCount = 1; break;
            case "AnyComponent" when argc == 2: vecArgCount = 1; break;
            case "Reduce" when argc == 3: vecArgCount = 1; break;
            default: return null;
        }

        var lambda = (TirLambda)Strip(fc.Args[argc - 1]);
        var lambdaParams = lambda.Parameters;
        var expectedLambdaArity = name == "Reduce" ? 2 : vecArgCount;
        if (lambdaParams.Count != expectedLambdaArity)
            return null;

        // The lambda body must be a plain expression whose root is postfix-safe (a call or a
        // reference), with no nested lambdas.
        var lambdaBody = lambda.Body;
        if (!(Strip(lambdaBody) is TirCall || Strip(lambdaBody) is TirInvoke || Strip(lambdaBody) is TirParameter))
            return null;
        if (lambdaBody.Descendants().OfType<TirLambda>().Any())
            return null;

        // All vector arguments must be plain parameter references — or `p.Components` reads of
        // one (the components of an IArrayLike ARE its fields, so the wrapper is transparent;
        // this is what a Reduce over Components looks like after inlining exposes it) — of one
        // and the same statically-known IArrayLike type.
        var vecArgs = new TirNode[vecArgCount];
        string typeName = null;
        for (var i = 0; i < vecArgCount; ++i)
        {
            var arg = Strip(fc.Args[i]);
            if (arg is TirCall compCall && compCall.Name == "Components" && compCall.Args.Count == 1)
                arg = Strip(compCall.Args[0]);
            if (!(arg is TirParameter pr) || pr.Def == null
                || !paramTypes.TryGetValue(pr.Def, out var t))
                return null;
            if (typeName == null)
                typeName = t;
            else if (typeName != t)
                return null;
            vecArgs[i] = arg;
        }

        var fields = writer.GetComponentFields(typeName);
        if (fields == null || fields.Count == 0)
            return null;

        // The float-backed one-component primitives (Number, Angle) expose a raw
        // "float Value" field; accessing it directly needs an explicit cast back to the
        // Plato component type. Under scalar erasure the polarity flips (the legacy rule):
        // Number/Angle raw float fields are already "float-land", while the other primitives'
        // handwritten pseudo-fields (Vector3.X) return the WRAPPER Number and cast DOWN.
        var isPrimitive = CSharpWriter.PrimitiveTypes.TryGetValue(typeName, out var prim);
        var castTo = writer.ScalarErase
            ? (isPrimitive && prim != "float" ? "float" : null)
            : (isPrimitive && prim == "float" ? "Number" : null);
        string scalarComponentPrim = null;
        if (writer.ScalarErase)
        {
            var compType = writer.GetComponentPlatoType(typeName);
            if (compType != null && CSharpWriter.ScalarPrimitives.TryGetValue(compType, out var compPrim))
                scalarComponentPrim = compPrim;
        }

        switch (name)
        {
            case "MapComponents":
            case "ZipComponents":
            {
                var args = fields
                    .Select(fd => Substitute(lambdaBody, lambdaParams,
                        vecArgs.Select(v => Component(v, fd, castTo, scalarComponentPrim)).ToArray()))
                    .ToList();
                return new TirConstructorCall(typeName, args);
            }

            case "AllZipComponents":
            case "AllComponents":
            case "AnyZipComponents":
            case "AnyComponent":
            {
                var op = name.StartsWith("All") ? "&&" : "||";
                var terms = fields
                    .Select(fd => Substitute(lambdaBody, lambdaParams,
                        vecArgs.Select(v => Component(v, fd, castTo, scalarComponentPrim)).ToArray()))
                    .ToList();
                return terms.Count == 1 ? terms[0] : new TirBooleanChain(op, terms);
            }

            case "Reduce":
            {
                // Left fold; the accumulator may appear in receiver position, so it must be
                // postfix-safe too (same kinds as the legacy rule).
                var init = Strip(fc.Args[1]);
                if (!(init is TirCall || init is TirInvoke || init is TirParameter
                      || init is TirVariable || init is TirName || init is TirLiteral))
                    return null;
                if (init.Descendants().OfType<TirLambda>().Any())
                    return null;

                var acc = init;
                foreach (var fd in fields)
                    acc = Substitute(lambdaBody, lambdaParams, new[] { acc, Component(vecArgs[0], fd, castTo, scalarComponentPrim) });
                return acc;
            }
        }

        return null;
    }

    private static TirNode Component(TirNode receiver, string fieldName, string castTo, string scalarComponentPrim)
        => new TirComponentAccess(receiver, fieldName, castTo, scalarComponentPrim);

    /// <summary>Beta reduction: a copy of the lambda body with each reference to a lambda
    /// parameter replaced by the corresponding expression.</summary>
    private static TirNode Substitute(TirNode lambdaBody, IReadOnlyList<ParameterDef> parameters, IReadOnlyList<TirNode> replacements)
        => Rewrite(lambdaBody, n =>
        {
            if (n is TirParameter pr && pr.Def != null)
                for (var i = 0; i < parameters.Count; ++i)
                    if (ReferenceEquals(pr.Def, parameters[i]))
                        return replacements[i];
            return n;
        });

    /// <summary>Post-order structural rewrite (children first, then <paramref name="f"/> on the
    /// rebuilt node) — the TIR analogue of <c>Symbol.Rewrite</c>.</summary>
    private static TirNode Rewrite(TirNode n, Func<TirNode, TirNode> f)
    {
        if (n == null)
            return null;

        List<TirNode> Rw(IReadOnlyList<TirNode> ns)
            => ns?.Select(x => Rewrite(x, f)).ToList();

        switch (n)
        {
            case TirCall c:
                return f(new TirCall(c.Callee, c.EmissionKind, c.ParameterTypes, c.ReturnType,
                    Rw(c.Args), c.Type, c.Origin, c.Name));
            case TirCoerce co:
                return f(new TirCoerce(Rewrite(co.Inner, f), co.FromType, co.ToType, co.ConversionFn, co.Origin));
            case TirInvoke inv:
                return f(new TirInvoke(Rewrite(inv.Target, f), Rw(inv.Args), inv.Type, inv.Origin));
            case TirConditional cond:
                return f(new TirConditional(Rewrite(cond.Condition, f), Rewrite(cond.IfTrue, f),
                    Rewrite(cond.IfFalse, f), cond.Type, cond.Origin));
            case TirNew nw:
                return f(new TirNew(nw.NewType, Rw(nw.Args), nw.Type, nw.Origin));
            case TirArray arr:
                return f(new TirArray(Rw(arr.Elements), arr.Type, arr.Origin));
            case TirAssign asg:
                return f(new TirAssign(Rewrite(asg.LValue, f), Rewrite(asg.RValue, f), asg.Type, asg.Origin));
            case TirLambda lam:
                return f(new TirLambda(lam.Parameters, Rewrite(lam.Body, f), lam.Type, lam.Origin));
            case TirLet let:
                return f(new TirLet(let.Def, Rewrite(let.Value, f), let.Type, let.Origin));
            case TirBlock b:
                return f(new TirBlock(Rw(b.Statements), b.Origin));
            case TirReturn r:
                return f(new TirReturn(Rewrite(r.Value, f), r.Origin));
            case TirIf iff:
                return f(new TirIf(Rewrite(iff.Condition, f), Rewrite(iff.IfTrue, f), Rewrite(iff.IfFalse, f), iff.Origin));
            case TirLoop l:
                return f(new TirLoop(Rewrite(l.Condition, f), Rewrite(l.Body, f), l.Origin));
            case TirComponentAccess ca:
                return f(new TirComponentAccess(Rewrite(ca.Receiver, f), ca.FieldName, ca.CastTo, ca.ScalarComponentPrim));
            case TirConstructorCall cc:
                return f(new TirConstructorCall(cc.TypeName, Rw(cc.Args)));
            case TirBooleanChain bc:
                return f(new TirBooleanChain(bc.Op, Rw(bc.Terms)));
            default:
                return f(n); // leaves
        }
    }
}
