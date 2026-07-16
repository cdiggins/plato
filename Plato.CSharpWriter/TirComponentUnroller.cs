using System;
using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Symbols;

namespace Ara3D.Geometry.CSharpWriter;

// The TIR component unroller (--optimize, roadmap P3.1): rewrites component-HOF calls in a TIR
// body to direct field expressions. Two families:
//
//   * CALL SITES of the recognized component HOFs (MapComponents, ZipComponents, ...) whose
//     receiver types are statically-known fixed-arity IArrayLike types — the original scheme
//     (see ComponentUnroller for the full design rationale);
//   * the generic ArrayLibrary BODIES themselves, monomorphized for a fixed-arity type, which
//     wrap a plain IArray combinator over Components() reads —
//         CreateFromComponents(Self, Zip(Components(a), Components(b), f))
//         All(Zip(Components(a), Components(b), f), (x) => x)
//     Their HOF argument is the body's own delegate parameter, so the call-site family never
//     fires and they previously fell through to the loop lowerer (allocating Components()
//     wrappers plus a result array per call). Fanned out per field, the constructor consumer
//     becomes `new T(f.Invoke(a.A, b.A), ...)` and the boolean consumers become && / || chains.
//
// The HOF argument may be a literal lambda (beta-reduced into the fan-out, the original rule)
// or a delegate-typed parameter/variable reference (invoked per component — the same two forms
// TirLoopLowerer.FnOk accepts).

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

    public TirComponentAccess(TirNode receiver, string fieldName, string castTo, string scalarComponentPrim = null, TypeExpression type = null)
        : base(type, receiver?.Origin)
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

    public TirConstructorCall(string typeName, IReadOnlyList<TirNode> args, TypeExpression type = null)
        : base(type, null)
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

    public TirBooleanChain(string op, IReadOnlyList<TirNode> terms, TypeExpression type = null)
        : base(type, null)
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

    // The generic-body consumer roots (see the file header): a fixed-arity fan-out of their
    // combinator argument replaces the whole consumer call.
    private static readonly HashSet<string> BodyConsumerNames = new HashSet<string>
    {
        "CreateFromComponents", "All", "Any",
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

        if (!body.Descendants().OfType<TirCall>().Any(c => IsCandidateCall(c) || IsBodyCandidateCall(c) || IsArrayCombinatorCandidate(c)))
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
                var u = TryUnrollCall(call, paramTypes, writer)
                        ?? TryUnrollComponentBody(call, paramTypes, fi, writer)
                        ?? TryUnrollArrayCombinator(call, writer);
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
           && IsHofArg(Strip(call.Args[call.Args.Count - 1]));

    private static bool IsBodyCandidateCall(TirCall call)
        => call.Name != null
           && BodyConsumerNames.Contains(call.Name)
           && call.Args.Count == 2;

    // A combinator whose list source(s) are fixed-size ARRAY LITERALS (a TirArray, e.g. the
    // 8-corner MakeArray a Corners inline exposes): unrolling it per element removes the loop the
    // loop lowerer would otherwise emit. Map/Reverse/Reduce/All/Any take one source (arg 0);
    // Zip takes 2 or 3.
    private static readonly HashSet<string> ArrayCombinatorNames = new HashSet<string>
    {
        "Map", "Reverse", "Reduce", "All", "Any", "Zip",
    };

    private static bool IsArrayCombinatorCandidate(TirCall call)
    {
        if (call.Name == null || !ArrayCombinatorNames.Contains(call.Name) || call.Args.Count == 0)
            return false;
        var srcCount = call.Name == "Zip" ? call.Args.Count - 1 : 1;
        for (var i = 0; i < srcCount && i < call.Args.Count; ++i)
            if (!(Strip(call.Args[i]) is TirArray))
                return false;
        return srcCount >= 1;
    }

    // A usable HOF argument: a literal lambda, or a delegate-typed (Function{N}) parameter or
    // variable reference — the same two forms TirLoopLowerer.FnOk accepts.
    private static bool IsHofArg(TirNode n)
        => n is TirLambda
           || ((n is TirParameter || n is TirVariable)
               && (FunctionTypeOf(n)?.Name?.StartsWith("Function") ?? false));

    /// <summary>The Function{N} type of a delegate-valued reference: the node's zonked type when
    /// it is one (monomorphized), else the declared parameter/variable type.</summary>
    private static TypeExpression FunctionTypeOf(TirNode n)
    {
        var zonked = n?.Type;
        if (zonked?.Name != null && zonked.Name.StartsWith("Function"))
            return zonked;
        var declared = (n as TirParameter)?.Def?.Type ?? (n as TirVariable)?.Def?.Type;
        return declared?.Name != null && declared.Name.StartsWith("Function") ? declared : null;
    }

    // A recognized HOF argument, applied once per component: a literal lambda beta-reduces
    // (the original rule); a delegate-typed reference is invoked (a parameter reference is pure,
    // so duplicating it per field is safe).
    private sealed class Hof
    {
        public TirLambda Lambda;
        public TirNode Delegate;

        public TirNode Apply(IReadOnlyList<TirNode> elems)
            => Lambda != null
                ? Substitute(Lambda.Body, Lambda.Parameters, elems)
                : new TirInvoke(Delegate, elems, ReturnTypeOf(Delegate), null);

        private static TypeExpression ReturnTypeOf(TirNode del)
        {
            var ft = FunctionTypeOf(del);
            return ft != null && ft.TypeArgs.Count > 0 ? ft.TypeArgs[ft.TypeArgs.Count - 1] : null;
        }
    }

    private static Hof TryGetHof(TirNode n, int arity)
    {
        n = Strip(n);
        if (n is TirLambda lam)
        {
            if (lam.Parameters.Count != arity)
                return null;
            // The lambda body must be a plain expression whose root is postfix-safe (a call or a
            // reference), with no nested lambdas.
            var lambdaBody = lam.Body;
            if (!(Strip(lambdaBody) is TirCall || Strip(lambdaBody) is TirInvoke || Strip(lambdaBody) is TirParameter))
                return null;
            if (lambdaBody.Descendants().OfType<TirLambda>().Any())
                return null;
            return new Hof { Lambda = lam };
        }
        if ((n is TirParameter || n is TirVariable) && FunctionTypeOf(n)?.Name == $"Function{arity}")
            return new Hof { Delegate = n };
        return null;
    }

    // The per-type unroll metadata: the statically-known field list plus the scalar-erasure cast
    // channels every emission marker carries.
    private sealed class ComponentInfo
    {
        public string TypeName;
        public IReadOnlyList<string> Fields;
        public string CastTo;
        public string ScalarComponentPrim;
        public TypeExpression ComponentType;
        public TypeExpression ConstructedType;
    }

    private static ComponentInfo TryGetComponentInfo(string typeName, CSharpWriter writer)
    {
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
        var compPlatoType = writer.GetComponentPlatoType(typeName);
        string scalarComponentPrim = null;
        if (writer.ScalarErase && compPlatoType != null
            && CSharpWriter.ScalarPrimitives.TryGetValue(compPlatoType, out var compPrim))
            scalarComponentPrim = compPrim;

        // The semantic (pre-erasure) types the emission markers carry, so the type-directed writer
        // and TirScalarLowerer see them like any other node. Best-effort: a null keeps the legacy
        // string channels authoritative.
        return new ComponentInfo
        {
            TypeName = typeName,
            Fields = fields,
            CastTo = castTo,
            ScalarComponentPrim = scalarComponentPrim,
            ComponentType = compPlatoType == null ? null : writer.Compilation.GetTypeDef(compPlatoType)?.ToTypeExpression(),
            ConstructedType = writer.Compilation.GetTypeDef(typeName)?.ToTypeExpression(),
        };
    }

    /// <summary>All sources resolved to the SAME statically-known concrete IArrayLike type, as
    /// either a plain parameter reference, a `p.Components` read of one (the components of an
    /// IArrayLike ARE its fields, so the wrapper is transparent), or a CHEAP PROJECTION of one
    /// (a field/property read like `x.A` — duplicating it per component only re-loads struct
    /// fields, so it is sound; this is what lets `Line3D.Eval`'s `x.A.Lerp(x.B, t)` unroll).
    /// Null otherwise.</summary>
    private static TirNode[] TryResolveVectorParams(IReadOnlyList<TirNode> args, int count,
        Dictionary<ParameterDef, string> paramTypes, CSharpWriter writer, out string typeName)
    {
        typeName = null;
        var vecArgs = new TirNode[count];
        for (var i = 0; i < count; ++i)
        {
            var arg = Strip(args[i]);
            if (arg is TirCall compCall && compCall.Name == "Components" && compCall.Args.Count == 1)
                arg = Strip(compCall.Args[0]);

            string t;
            if (arg is TirParameter pr && pr.Def != null && paramTypes.TryGetValue(pr.Def, out t))
            {
                // authoritative monomorphized parameter type
            }
            else if (TirInliner.IsCheapProjection(writer, arg) && (t = arg.Type?.Name) != null
                     && writer.GetComponentFields(t) != null)
            {
                // a cheap field/projection read of a concrete IArrayLike type
            }
            else
            {
                return null;
            }

            if (typeName == null)
                typeName = t;
            else if (typeName != t)
                return null;
            vecArgs[i] = arg;
        }
        return vecArgs;
    }

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

        var hof = TryGetHof(fc.Args[argc - 1], name == "Reduce" ? 2 : vecArgCount);
        if (hof == null)
            return null;

        var vecArgs = TryResolveVectorParams(fc.Args, vecArgCount, paramTypes, writer, out var typeName);
        if (vecArgs == null)
            return null;

        var info = TryGetComponentInfo(typeName, writer);
        if (info == null)
            return null;

        switch (name)
        {
            case "MapComponents":
            case "ZipComponents":
            {
                var args = info.Fields
                    .Select(fd => hof.Apply(vecArgs.Select(v => Component(v, fd, info)).ToList()))
                    .ToList();
                return new TirConstructorCall(typeName, args, info.ConstructedType);
            }

            case "AllZipComponents":
            case "AllComponents":
            case "AnyZipComponents":
            case "AnyComponent":
            {
                var op = name.StartsWith("All") ? "&&" : "||";
                var terms = info.Fields
                    .Select(fd => hof.Apply(vecArgs.Select(v => Component(v, fd, info)).ToList()))
                    .ToList();
                return terms.Count == 1 ? terms[0] : new TirBooleanChain(op, terms, BoolType(writer));
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
                foreach (var fd in info.Fields)
                    acc = hof.Apply(new[] { acc, Component(vecArgs[0], fd, info) });
                return acc;
            }
        }

        return null;
    }

    /// <summary>The generic-body consumer rewrite (see the file header): a
    /// CreateFromComponents/All/Any call whose argument is a recognized combinator over
    /// Components() reads of fixed-arity parameters, fanned out per field. Null when the shape
    /// does not match.</summary>
    private static TirNode TryUnrollComponentBody(TirCall fc, Dictionary<ParameterDef, string> paramTypes,
        CSharpFunctionInfo fi, CSharpWriter writer)
    {
        if (!IsBodyCandidateCall(fc))
            return null;

        switch (fc.Name)
        {
            case "CreateFromComponents":
            {
                // Args are (Self, xs). The constructed type must BE the sources' type: a
                // same-arity constructor is only guaranteed for the type the fields came from.
                var elems = TryFanOut(Strip(fc.Args[1]), paramTypes, writer, out var info);
                if (elems == null || ResolveTypeName(fc.Type, fi) != info.TypeName)
                    return null;
                return new TirConstructorCall(info.TypeName, elems, info.ConstructedType);
            }

            case "All":
            case "Any":
            {
                var pred = TryGetHof(fc.Args[1], 1);
                if (pred == null)
                    return null;
                var elems = TryFanOut(Strip(fc.Args[0]), paramTypes, writer, out _);
                if (elems == null)
                    return null;
                var op = fc.Name == "All" ? "&&" : "||";
                var terms = elems.Select(e => pred.Apply(new[] { e })).ToList();
                return terms.Count == 1 ? terms[0] : new TirBooleanChain(op, terms, BoolType(writer));
            }
        }

        return null;
    }

    /// <summary>Unroll a combinator over fixed-size array LITERALS (a TirArray, e.g. the
    /// 8-corner MakeArray a Corners inline exposes): Map/Zip/Reverse produce a new TirArray of the
    /// per-element results, All/Any a boolean chain, Reduce a left fold. This removes the loop the
    /// loop lowerer would otherwise emit for `Corners.Map(f)`. Null when the shape does not match.</summary>
    private static TirNode TryUnrollArrayCombinator(TirCall fc, CSharpWriter writer)
    {
        if (!IsArrayCombinatorCandidate(fc))
            return null;

        var name = fc.Name;
        TirArray Src(int i) => (TirArray)Strip(fc.Args[i]);

        switch (name)
        {
            case "Map":
            {
                var hof = TryGetHof(fc.Args[1], 1);
                if (hof == null)
                    return null;
                var elems = Src(0).Elements.Select(e => hof.Apply(new[] { e })).ToList();
                return new TirArray(elems, fc.Type, fc.Origin);
            }

            case "Zip":
            {
                var srcCount = fc.Args.Count - 1;
                var hof = TryGetHof(fc.Args[srcCount], srcCount);
                if (hof == null)
                    return null;
                var sources = Enumerable.Range(0, srcCount).Select(Src).ToList();
                var n = sources.Min(s => s.Elements.Count);
                var elems = Enumerable.Range(0, n)
                    .Select(i => hof.Apply(sources.Select(s => s.Elements[i]).ToList()))
                    .ToList();
                return new TirArray(elems, fc.Type, fc.Origin);
            }

            case "Reverse":
            {
                var elems = Src(0).Elements.Reverse().ToList();
                return new TirArray(elems, fc.Type, fc.Origin);
            }

            case "All":
            case "Any":
            {
                var pred = TryGetHof(fc.Args[1], 1);
                if (pred == null)
                    return null;
                var op = name == "All" ? "&&" : "||";
                var terms = Src(0).Elements.Select(e => pred.Apply(new[] { e })).ToList();
                if (terms.Count == 0)
                    return null;
                return terms.Count == 1 ? terms[0] : new TirBooleanChain(op, terms, BoolType(writer));
            }

            case "Reduce":
            {
                var hof = TryGetHof(fc.Args[2], 2);
                if (hof == null)
                    return null;
                var init = Strip(fc.Args[1]);
                if (!(init is TirCall || init is TirInvoke || init is TirParameter
                      || init is TirVariable || init is TirName || init is TirLiteral))
                    return null;
                if (init.Descendants().OfType<TirLambda>().Any())
                    return null;
                var acc = init;
                foreach (var e in Src(0).Elements)
                    acc = hof.Apply(new[] { acc, e });
                return acc;
            }
        }

        return null;
    }

    /// <summary>The per-component element expressions of a recognized combinator over
    /// Components() reads, or null: Map/Zip fan out through the HOF, Reverse reverses the field
    /// order, and a bare Components read is the fields themselves.</summary>
    private static List<TirNode> TryFanOut(TirNode n, Dictionary<ParameterDef, string> paramTypes,
        CSharpWriter writer, out ComponentInfo info)
    {
        info = null;
        if (!(n is TirCall c) || c.Name == null)
            return null;

        int srcCount;
        Hof hof = null;
        var reversed = false;
        switch (c.Name)
        {
            case "Components" when c.Args.Count == 1:
                srcCount = 1;
                break;
            case "Reverse" when c.Args.Count == 1:
                srcCount = 1;
                reversed = true;
                break;
            case "Map" when c.Args.Count == 2:
                srcCount = 1;
                if ((hof = TryGetHof(c.Args[1], 1)) == null)
                    return null;
                break;
            case "Zip" when c.Args.Count == 3 || c.Args.Count == 4:
                srcCount = c.Args.Count - 1;
                if ((hof = TryGetHof(c.Args[c.Args.Count - 1], srcCount)) == null)
                    return null;
                break;
            default:
                return null;
        }

        var vecArgs = TryResolveVectorParams(c.Args, srcCount, paramTypes, writer, out var typeName);
        if (vecArgs == null)
            return null;
        info = TryGetComponentInfo(typeName, writer);
        if (info == null)
            return null;

        var localInfo = info;
        var fields = reversed ? (IReadOnlyList<string>)info.Fields.Reverse().ToList() : info.Fields;
        return fields
            .Select(fd =>
            {
                var comps = vecArgs.Select(v => Component(v, fd, localInfo)).ToList();
                return hof == null ? comps[0] : hof.Apply(comps);
            })
            .ToList();
    }

    /// <summary>The concrete C# name a constructed-type expression resolves to
    /// (<c>Self</c> → the owner type of this function instance).</summary>
    private static string ResolveTypeName(TypeExpression t, CSharpFunctionInfo fi)
        => t?.Name == "Self" ? fi?.OwnerType?.Name : t?.Name;

    private static TypeExpression BoolType(CSharpWriter writer)
        => writer.Compilation.GetTypeDef("Boolean")?.ToTypeExpression();

    private static TirNode Component(TirNode receiver, string fieldName, ComponentInfo info)
        => new TirComponentAccess(receiver, fieldName, info.CastTo, info.ScalarComponentPrim, info.ComponentType);

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
                return f(new TirComponentAccess(Rewrite(ca.Receiver, f), ca.FieldName, ca.CastTo, ca.ScalarComponentPrim, ca.Type));
            case TirConstructorCall cc:
                return f(new TirConstructorCall(cc.TypeName, Rw(cc.Args), cc.Type));
            case TirBooleanChain bc:
                return f(new TirBooleanChain(bc.Op, Rw(bc.Terms), bc.Type));
            default:
                return f(n); // leaves
        }
    }
}
