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
                return new TirComponentAccess(LowerNode(ca.Receiver, m), ca.FieldName, ca.CastTo, ca.ScalarComponentPrim);
            case TirConstructorCall cc:
                return new TirConstructorCall(cc.TypeName, LowerNodes(cc.Args, m));
            case TirBooleanChain bc:
                return new TirBooleanChain(bc.Op, LowerNodes(bc.Terms, m));
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
