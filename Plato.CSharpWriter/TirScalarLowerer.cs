using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.AST;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Symbols;

namespace Ara3D.Geometry.CSharpWriter;

/// <summary>
/// Mission 2 of the scalar-lowering endgame (docs/plato-tir-scalar-lowering-plan): make
/// <c>--scalar</c> erasure a real TIR lowering pass instead of the emit-time
/// <see cref="ScalarEraseAnalysis"/>. This file is the SUBSTITUTION core — it rewrites the five
/// scalar wrapper types (<c>Number/Integer/Boolean/Character/String</c>) to their C# primitives
/// (<c>float/int/bool/char/string</c>) in every <see cref="TypeExpression"/> reachable from a
/// monomorphized <see cref="TirFunction"/>: each node's <see cref="TirNode.Type"/>, a
/// <see cref="TirCall"/>'s parameter/return types, a <see cref="TirCoerce"/>'s endpoints, a
/// <see cref="TirNew"/>'s constructed type, and every nested generic argument
/// (<c>IArray&lt;Number&gt;</c> → <c>IArray&lt;float&gt;</c>). After this pass a node's type IS its
/// erased emitted type, so a type-directed printer needs no float-land decisions.
///
/// It is deliberately a PURE type substitution: it never rewrites the tree shape and never inserts
/// or drops <see cref="TirCoerce"/> nodes — coercion normalization at the wrapper/primitive
/// boundary is a separate step. It does not consult the wrapper-boundary contract here either; a
/// node whose runtime value genuinely stays a wrapper (a handwritten-intrinsic result) is corrected
/// at coercion insertion, not by refusing substitution.
///
/// The primitive targets are represented by five synthetic null-scope <see cref="TypeDef"/>s (the
/// same shape <see cref="SymbolFactory"/> uses for its built-in <c>Any</c>), created once. They
/// exist only inside the writer's lowered TIR — they never re-enter the checker or monomorphizer.
/// The <paramref name="scalarMap"/> is a parameter so <c>--scalar=double</c> is a different map, not
/// a different pass.
/// </summary>
public static class TirScalarLowerer
{
    /// <summary>The default float-erasure map (<see cref="CSharpWriter.ScalarPrimitives"/>): the five
    /// wrapper type names to a synthetic primitive <see cref="TypeDef"/> each.</summary>
    public static readonly IReadOnlyDictionary<string, TypeDef> FloatMap =
        CSharpWriter.ScalarPrimitives.ToDictionary(
            kv => kv.Key,
            kv => new TypeDef(null, TypeKind.Primitive, kv.Value));

    /// <summary>Lower every type in <paramref name="tir"/> through the default float map.</summary>
    public static TirFunction Lower(TirFunction tir)
        => Lower(tir, FloatMap);

    /// <summary>The full emit-path lowering: FIRST insert the disambiguating coercions that keep
    /// overload resolution exact after erasure, THEN substitute wrapper types to primitives. The
    /// coercions are read straight off the resolved call signatures the checker recorded, so the
    /// type-directed printer needs no overload re-derivation.</summary>
    public static TirFunction LowerWithCoercions(TirFunction tir)
        => LowerWithCoercions(tir, FloatMap);

    /// <summary>See <see cref="LowerWithCoercions(TirFunction)"/>; parameterized by the scalar map
    /// (so <c>--scalar=double</c> is a different map, not a different pass).</summary>
    public static TirFunction LowerWithCoercions(TirFunction tir, IReadOnlyDictionary<string, TypeDef> scalarMap)
    {
        if (tir?.Body == null)
            return Lower(tir, scalarMap);
        var coerced = InsertScalarCoercions(tir.Body);
        var withCoercions = new TirFunction(tir.Original, tir.Parameters, tir.ReturnType, coerced,
            tir.ZonkedParameterTypes, tir.ZonkedReturnType);
        var lowered = Lower(withCoercions, scalarMap);
        // Post-substitution: restore the wrapper at scalar->non-scalar broadcast boundaries, where
        // the erased primitive argument would no longer convert (float -> Vector2 is not implicit;
        // (Number)float -> Vector2 is). Runs after lowering so the wrapper cast is not itself erased.
        return new TirFunction(lowered.Original, lowered.Parameters, lowered.ReturnType,
            RestoreWrapperAtBroadcasts(lowered.Body), lowered.ZonkedParameterTypes, lowered.ZonkedReturnType);
    }

    // primitive name -> a wrapper TypeExpression (float -> Number), for broadcast-boundary casts.
    private static readonly IReadOnlyDictionary<string, TypeExpression> PrimToWrapperType =
        CSharpWriter.ScalarPrimitives.ToDictionary(
            kv => kv.Value,
            kv => new TypeExpression(new TypeDef(null, TypeKind.Primitive, kv.Key)));

    /// <summary>Wrap a scalar-primitive argument passed to a CONCRETE non-scalar parameter in a
    /// coercion to its wrapper (float -> Number), so the runtime's <c>Number</c>-sourced broadcast
    /// operator applies (C# will not chain <c>float -> Number -> Vector2</c> implicitly). Only fires
    /// at concrete struct parameters — generic/interface/Self parameters carry no concrete broadcast.</summary>
    private static TirNode RestoreWrapperAtBroadcasts(TirNode body)
        => TirRewrite.Rewrite(body, n =>
        {
            if (!(n is TirCall call) || call.ParameterTypes == null || call.Args.Count == 0)
                return n;
            // A call whose RECEIVER (arg 0) is itself an erased scalar is a purely scalar operation —
            // e.g. an inlined `float.Multiply(float)` (t*t from Pow3) whose call node still carries a
            // loose vector/interface parameter type. Broadcasting its arguments to (Number) would turn
            // that into an ambiguous `Number.Multiply(Vector2|Vector8)`. Strip any wrapper tag the
            // inliner left on the receiver before deciding. A genuine scalar-receiver broadcast
            // (`0.5 * vector`) has a NON-scalar argument, which is never wrapped here anyway.
            var recv0 = TirRewrite.StripCoerce(call.Args[0])?.Type;
            if (recv0?.Def != null && CSharpWriter.ScalarPrimitives.ContainsValue(recv0.Def.Name))
                return n;
            List<TirNode> newArgs = null;
            for (var i = 0; i < call.Args.Count; i++)
            {
                var pt = i < call.ParameterTypes.Count ? call.ParameterTypes[i] : null;
                if (pt?.Def == null || CSharpWriter.ScalarPrimitives.ContainsValue(pt.Def.Name))
                    continue; // scalar param: phase A handled it
                // Only a CONCRETE struct/primitive parameter carries a Number-sourced broadcast we
                // can pin. An interface parameter (INumerical) is not grounded to its concrete type
                // here, so a scalar argument at one is instead routed to the legacy path by
                // IsGroundBody (which cannot tell a vector broadcast from a scalar operation).
                if (pt.Def.Kind != TypeKind.ConcreteType && pt.Def.Kind != TypeKind.Primitive)
                    continue;
                var arg = call.Args[i];
                if (arg is TirCoerce)
                    continue;
                var it = arg.Type;
                if (it?.Def == null || !CSharpWriter.ScalarPrimitives.ContainsValue(it.Def.Name)
                    || !PrimToWrapperType.TryGetValue(it.Def.Name, out var wrapper))
                    continue; // arg is not an erased scalar primitive
                newArgs ??= call.Args.ToList();
                newArgs[i] = new TirCoerce(arg, arg.Type, wrapper, null, arg.Origin);
            }
            return newArgs == null
                ? n
                : new TirCall(call.Callee, call.EmissionKind, call.ParameterTypes, call.ReturnType,
                    newArgs, call.Type, call.Origin, call.Name);
        });

    private static bool IsScalarWrapperName(string name)
        => name != null && CSharpWriter.ScalarPrimitives.ContainsKey(name);

    /// <summary>Whether the body is LOWERABLE: every type is scalar-decidable. That means no unsolved
    /// type VARIABLE (<c>$T</c> — a still-inferring node the lowerer could mis-cast), but the
    /// function's own signature type PARAMETERS (<c>_T0</c> on a generic static library function) ARE
    /// allowed: they are not scalar wrappers, so the substitution and coercion insertion leave them
    /// untouched, and the scalar positions around them (the <c>bool</c>/<c>int</c> args of
    /// <c>AllQuadFaceIndices</c>) are decidable. An untyped ZERO-ARG call (a bare constant reference
    /// like <c>Pi()</c> whose node the static elaboration leaves untyped) is also allowed: it renders
    /// through the type-independent <c>Constants.X()</c> path, so its missing type is harmless. Any
    /// OTHER untyped value node still routes the body to the legacy <see cref="ScalarEraseAnalysis"/>
    /// path (the coercion insertion cannot tell a scalar from a non-scalar there).</summary>
    public static bool IsGroundBody(TirFunction tir)
    {
        if (tir?.Body == null)
            return false;
        foreach (var n in tir.AllNodes)
        {
            if (n == null) continue;
            // NOTE: a type VARIABLE ($T, the signature generic of a static library function like
            // AllQuadFaceIndices<$T>) is NOT a blocker — it is never a scalar wrapper, so the
            // substitution and coercion insertion leave it inert while the scalar positions around it
            // stay decidable. It renders as the C# generic parameter (_T0). Only genuine scalar
            // ambiguity (below) or an untyped coercible node routes a body away from lowering.
            //
            // A value node with NO type is un-lowerable: the coercion insertion cannot tell a scalar
            // from a non-scalar there (the static-emit library bodies leave receivers/args untyped),
            // so such a body stays on the legacy path rather than being mis-cast. EXCEPT a zero-arg
            // call (a bare constant reference the static elaboration left untyped), which renders
            // through the type-independent Constants.X() path.
            if (NeedsType(n) && n.Type == null && !(n is TirCall zc && zc.Args.Count == 0)) return false;
            // Untrustworthy typing: a SCALAR-wrapper parameter with a concrete NON-scalar argument
            // (Between's Number param receiving a Point2D — the generic-library-body looseness that
            // survives monomorphization). Casting there is wrong (CS0030/CS1503), so such a body is
            // not safely lowerable and stays on the legacy path.
            if (n is TirCall call && call.ParameterTypes != null)
                for (var i = 0; i < call.Args.Count && i < call.ParameterTypes.Count; i++)
                {
                    var pt = call.ParameterTypes[i];
                    if (pt?.Def == null)
                        continue;
                    if (IsScalarWrapperName(pt.Def.Name))
                    {
                        var at = TirRewrite.StripCoerce(call.Args[i])?.Type;
                        if (at?.Def != null && !IsScalarWrapperName(at.Def.Name)
                            && !CSharpWriter.ScalarPrimitives.ContainsValue(at.Def.Name)
                            && at.Def.Kind != TypeKind.TypeVariable && at.Def.Kind != TypeKind.TypeParameter)
                            return false; // scalar param, concrete non-scalar arg: loose typing
                    }
                    // (A scalar argument at an INTERFACE parameter — Pi passed to Twice(INumerical) —
                    // is a legitimate scalar operation: the scalar type implements the interface, so
                    // it renders as the scalar extension method with no broadcast. Not a blocker.)
                }
            // Corrupted scalar operation: a call whose RECEIVER is scalar yet carries a scalar
            // argument at a concrete NON-scalar parameter. This is a scalar op (`t.Multiply(t)`)
            // whose recorded signature was inherited from a vector-generic body it was inlined from
            // (Pow3 spliced into a Vector-reified Hermite). Broadcasting its scalar arguments would
            // emit an ambiguous `Number.Multiply(Vector2|Vector8)`; route the body to the legacy path.
            if (n is TirCall sc && sc.Args.Count > 1 && sc.ParameterTypes != null && IsScalarType(Recv(sc)))
                for (var i = 1; i < sc.Args.Count && i < sc.ParameterTypes.Count; i++)
                {
                    var pt = sc.ParameterTypes[i];
                    if (pt?.Def == null || IsScalarWrapperName(pt.Def.Name)
                        || CSharpWriter.ScalarPrimitives.ContainsValue(pt.Def.Name))
                        continue;
                    if (pt.Def.Kind != TypeKind.ConcreteType && pt.Def.Kind != TypeKind.Primitive)
                        continue;
                    var at = TirRewrite.StripCoerce(sc.Args[i])?.Type;
                    if (at?.Def != null && (IsScalarWrapperName(at.Def.Name)
                        || CSharpWriter.ScalarPrimitives.ContainsValue(at.Def.Name)))
                        return false; // scalar receiver + scalar arg at a vector param: corrupted
                }
        }
        return true;
    }

    // The (coercion-stripped) type of a call's receiver argument.
    private static TypeExpression Recv(TirCall c)
        => c.Args.Count > 0 ? TirRewrite.StripCoerce(c.Args[0])?.Type : null;

    // A definitely-scalar type (wrapper or erased primitive) — null/unknown is NOT scalar here.
    private static bool IsScalarType(TypeExpression t)
        => t?.Def != null
           && (IsScalarWrapperName(t.Def.Name) || CSharpWriter.ScalarPrimitives.ContainsValue(t.Def.Name));

    // Nodes whose erased rendering depends on knowing their type. Statements, bare names, type
    // references, lets and the `default` keyword carry no value type and are exempt.
    private static bool NeedsType(TirNode n)
        => !(n is TirBlock || n is TirReturn || n is TirIf || n is TirLoop
             || n is TirName || n is TirTypeRef || n is TirLet || n is TirDefault
             || n is TirUnresolved || n is TirConstructorCall || n is TirBooleanChain);

    /// <summary>Whether <paramref name="tir"/> has already been scalar-lowered — any node type
    /// names an erased primitive (a non-lowered body would name the wrapper). The printer keys its
    /// type-directed mode off this, so it can never disagree with what the pass actually did.</summary>
    public static bool WasLowered(TirFunction tir)
    {
        if (tir?.Body == null)
            return false;
        // Only an erased PRIMITIVE (float/int/…) proves the pass ran. A wrapper-named coercion
        // (`coerce<Number→Number>`) is NOT a lowering artifact — the inliner emits such tags into
        // ordinary un-lowered bodies, and matching them here would flip the printer into type-directed
        // mode on a body routed to the legacy path (rendering the tag as an ambiguous `(Number)`).
        foreach (var n in tir.AllNodes)
        {
            if (n?.Type?.Def != null && CSharpWriter.ScalarPrimitives.ContainsValue(n.Type.Def.Name))
                return true;
            if (n is TirCoerce co && co.ToType?.Def != null
                && CSharpWriter.ScalarPrimitives.ContainsValue(co.ToType.Def.Name))
                return true;
        }
        return false;
    }

    /// <summary>Whether a type is a scalar (wrapper or already-erased primitive) or unknown — the
    /// only cases where casting an argument to a scalar parameter is meaningful. A known non-scalar
    /// argument at a (loosely-typed) scalar parameter — the bounded-polymorphic
    /// <c>Subtract($TDelta,…)</c> case — must NOT be cast to a primitive (CS0030).</summary>
    private static bool IsScalarish(TypeExpression t)
        => t?.Def == null
           || IsScalarWrapperName(t.Def.Name)
           || CSharpWriter.ScalarPrimitives.ContainsValue(t.Def.Name);

    /// <summary>Wrap each SCALAR-typed argument of a resolved call in an explicit coercion to its
    /// declared parameter type, so C# overload resolution stays exact once the wrappers erase to
    /// primitives (a bare <c>float</c> argument could otherwise match either the <c>float</c> or the
    /// <c>Number</c> overload — CS0121). Driven entirely by the recorded
    /// <see cref="TirCall.ParameterTypes"/>. Idempotent: an argument already coerced to that scalar
    /// target is left as-is. Scalar-return disambiguation falls out for free — a scalar-returning
    /// call used as a receiver is arg 0 of the outer call and gets wrapped there.</summary>
    private static TirNode InsertScalarCoercions(TirNode body)
        => TirRewrite.Rewrite(body, n =>
        {
            if (!(n is TirCall call) || call.ParameterTypes == null || call.Args.Count == 0)
                return n;
            List<TirNode> newArgs = null;
            for (var i = 0; i < call.Args.Count; i++)
            {
                var pt = i < call.ParameterTypes.Count ? call.ParameterTypes[i] : null;
                if (pt?.Def == null || !IsScalarWrapperName(pt.Def.Name))
                    continue;
                var arg = call.Args[i];
                var inner = TirRewrite.StripCoerce(arg);
                if (inner is TirTypeRef || inner is TirName)
                    continue; // a TYPE used as a static-call receiver is not a value to cast (CS0119)
                if (!IsScalarish(inner.Type))
                    continue; // a non-scalar value at a loosely-typed scalar param: no cast (CS0030)
                if (arg is TirCoerce cc && cc.ToType?.Def?.Name == pt.Def.Name)
                    continue; // already coerced to this scalar target
                newArgs ??= call.Args.ToList();
                newArgs[i] = new TirCoerce(arg, arg.Type, pt, null, arg.Origin);
            }
            return newArgs == null
                ? n
                : new TirCall(call.Callee, call.EmissionKind, call.ParameterTypes, call.ReturnType,
                    newArgs, call.Type, call.Origin, call.Name);
        });

    /// <summary>Lower every type in <paramref name="tir"/> through <paramref name="scalarMap"/>
    /// (wrapper type name → primitive <see cref="TypeDef"/>). Returns a new function; the original
    /// is untouched. A null or bodiless function is returned unchanged.</summary>
    public static TirFunction Lower(TirFunction tir, IReadOnlyDictionary<string, TypeDef> scalarMap)
    {
        if (tir == null || scalarMap == null)
            return tir;
        var body = LowerNode(tir.Body, scalarMap);
        return new TirFunction(
            tir.Original,
            tir.Parameters,
            LowerType(tir.ReturnType, scalarMap),
            body,
            tir.ZonkedParameterTypes?.Select(t => LowerType(t, scalarMap)).ToList(),
            LowerType(tir.ZonkedReturnType, scalarMap));
    }

    /// <summary>Substitute the scalar wrapper <see cref="TypeDef"/>s in <paramref name="t"/> (and
    /// recursively in its type arguments) with their primitive targets. Non-scalar types and their
    /// non-scalar arguments are preserved by reference where nothing changed.</summary>
    public static TypeExpression LowerType(TypeExpression t, IReadOnlyDictionary<string, TypeDef> scalarMap)
    {
        if (t?.Def == null)
            return t;
        var mappedDef = scalarMap.TryGetValue(t.Def.Name, out var prim) ? prim : t.Def;
        var args = t.TypeArgs;
        TypeExpression[] loweredArgs = null;
        for (var i = 0; i < args.Count; i++)
        {
            var lowered = LowerType(args[i], scalarMap);
            if (!ReferenceEquals(lowered, args[i]))
                loweredArgs ??= args.ToArray();
            if (loweredArgs != null)
                loweredArgs[i] = lowered;
        }
        if (ReferenceEquals(mappedDef, t.Def) && loweredArgs == null)
            return t;
        return new TypeExpression(mappedDef, loweredArgs ?? args.ToArray());
    }

    private static List<TirNode> LowerNodes(IReadOnlyList<TirNode> ns, IReadOnlyDictionary<string, TypeDef> m)
        => ns?.Select(n => LowerNode(n, m)).ToList();

    private static IReadOnlyList<TypeExpression> LowerTypes(IReadOnlyList<TypeExpression> ts, IReadOnlyDictionary<string, TypeDef> m)
        => ts?.Select(t => LowerType(t, m)).ToList();

    /// <summary>Rebuild <paramref name="n"/> with every embedded <see cref="TypeExpression"/>
    /// lowered and every child lowered. Structure is preserved exactly.</summary>
    private static TirNode LowerNode(TirNode n, IReadOnlyDictionary<string, TypeDef> m)
    {
        switch (n)
        {
            case null:
                return null;

            // --- leaves that carry a type ---------------------------------------
            case TirLiteral lit:
                return new TirLiteral(lit.Value, lit.LiteralType, LowerType(lit.Type, m), lit.Origin);
            case TirParameter p:
                return new TirParameter(p.Def, LowerType(p.Type, m), p.Origin);
            case TirVariable v:
                return new TirVariable(v.Def, LowerType(v.Type, m), v.Origin);
            case TirTypeRef tr:
                return new TirTypeRef(tr.TypeDef, LowerType(tr.Type, m), tr.Origin, tr.NamespaceQualified);
            case TirName nm:
                return new TirName(nm.Name, LowerType(nm.Type, m), nm.Origin);
            case TirDefault d:
                return new TirDefault(LowerType(d.Type, m), d.Origin);
            case TirTempRef tmp:
                return new TirTempRef(tmp.Name, LowerType(tmp.Type, m), tmp.Origin);

            // --- calls & coercions ----------------------------------------------
            case TirCall c:
                return new TirCall(c.Callee, c.EmissionKind, LowerTypes(c.ParameterTypes, m),
                    LowerType(c.ReturnType, m), LowerNodes(c.Args, m), LowerType(c.Type, m), c.Origin, c.Name);
            case TirCoerce co:
                return new TirCoerce(LowerNode(co.Inner, m), LowerType(co.FromType, m),
                    LowerType(co.ToType, m), co.ConversionFn, co.Origin);
            case TirInvoke inv:
                return new TirInvoke(LowerNode(inv.Target, m), LowerNodes(inv.Args, m), LowerType(inv.Type, m), inv.Origin);

            // --- compound expressions -------------------------------------------
            case TirConditional cond:
                return new TirConditional(LowerNode(cond.Condition, m), LowerNode(cond.IfTrue, m),
                    LowerNode(cond.IfFalse, m), LowerType(cond.Type, m), cond.Origin);
            case TirNew nw:
                return new TirNew(LowerType(nw.NewType, m), LowerNodes(nw.Args, m), LowerType(nw.Type, m), nw.Origin);
            case TirArray arr:
                return new TirArray(LowerNodes(arr.Elements, m), LowerType(arr.Type, m), arr.Origin);
            case TirAssign asg:
                return new TirAssign(LowerNode(asg.LValue, m), LowerNode(asg.RValue, m), LowerType(asg.Type, m), asg.Origin);
            case TirLambda lam:
                return new TirLambda(lam.Parameters, LowerNode(lam.Body, m), LowerType(lam.Type, m), lam.Origin);
            case TirLet let:
                return new TirLet(let.Def, LowerNode(let.Value, m), LowerType(let.Type, m), let.Origin);

            // --- statements -----------------------------------------------------
            case TirBlock b:
                return new TirBlock(LowerNodes(b.Statements, m), b.Origin);
            case TirReturn r:
                return new TirReturn(LowerNode(r.Value, m), r.Origin);
            case TirIf iff:
                return new TirIf(LowerNode(iff.Condition, m), LowerNode(iff.IfTrue, m), LowerNode(iff.IfFalse, m), iff.Origin);
            case TirLoop l:
                return new TirLoop(LowerNode(l.Condition, m), LowerNode(l.Body, m), l.Origin);

            // --- optimizer marker nodes (Plato.CSharpWriter) --------------------
            case TirComponentAccess ca:
                return new TirComponentAccess(LowerNode(ca.Receiver, m), ca.FieldName, ca.CastTo, ca.ScalarComponentPrim, LowerType(ca.Type, m));
            case TirConstructorCall cc:
                return new TirConstructorCall(cc.TypeName, LowerNodes(cc.Args, m), LowerType(cc.Type, m));
            case TirBooleanChain bc:
                return new TirBooleanChain(bc.Op, LowerNodes(bc.Terms, m), LowerType(bc.Type, m));
            case TirLoweredLoop ll:
                return LowerLoweredLoop(ll, m);

            // --- unresolved (should not reach a lowered body, but stay total) ---
            case TirUnresolved u:
                return new TirUnresolved(u.Original, u.Reason, LowerNodes(u.ChildNodes, m));

            default:
                return n;
        }
    }

    private static TirNode LowerLoweredLoop(TirLoweredLoop ll, IReadOnlyDictionary<string, TypeDef> m)
    {
        var lowered = new TirLoweredLoop(ll.Kind, LowerNodes(ll.Sources, m), LowerNode(ll.Fn, m),
            LowerNode(ll.Seed, m), LowerNode(ll.IncludeFirst, m), ll.TempName, ll.Id,
            LowerType(ll.Type, m), ll.Origin);
        lowered.ElemType = LowerType(ll.ElemType, m);
        return lowered;
    }
}
