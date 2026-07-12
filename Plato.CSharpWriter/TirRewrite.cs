using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Symbols;

namespace Ara3D.Geometry.CSharpWriter;

/// <summary>
/// Structural primitives over the Typed IR, shared by the source-level transforms
/// (<see cref="TirInliner"/> today; the loop-body inliner and delegate specializer next).
///
///   * <see cref="Rewrite"/> — the one canonical post-order tree rebuild (children first, then the
///     supplied function on the rebuilt node), covering every TIR node kind INCLUDING the optimizer
///     marker nodes emitted by <see cref="TirComponentUnroller"/>. Optional enter/exit hooks bracket
///     each subtree for context tracking (e.g. lambda depth).
///   * <see cref="Substitute"/> — capture-agnostic parameter substitution: replace every
///     <see cref="TirParameter"/> whose <see cref="ParameterDef"/> is in the map with the mapped
///     node. This is the atom of both β-reduction and function inlining.
///   * <see cref="TryBetaReduce"/> — β-reduction of an immediately-applied lambda,
///     <c>(λx….e)(a…) → e[x↦a]</c>. Sound in the pure language whenever no argument is evaluated
///     more times than the invocation would have (the <see cref="IsCheap"/> / single-use discipline,
///     shared with the inliner).
///
/// The <see cref="Substitute"/> / <see cref="TryBetaReduce"/> substitution does NOT alpha-rename:
/// it assumes the callee's bound names do not collide with names free in the substituted arguments.
/// For inlining a lambda applied to caller expressions that holds (the arguments are drawn from the
/// caller's own already-distinct scope); the inliner's lambda-parameter renaming
/// (<c>BuildLambdaParamRenames</c>) covers the one collision case the emitter cannot tolerate
/// (CS0136 shadowing).
/// </summary>
public static class TirRewrite
{
    /// <summary>Post-order structural rewrite: rebuild <paramref name="n"/> with children rewritten,
    /// then apply <paramref name="f"/> to the rebuilt node. The optional <paramref name="enter"/> /
    /// <paramref name="exit"/> hooks bracket each node's subtree.</summary>
    public static TirNode Rewrite(TirNode n, System.Func<TirNode, TirNode> f,
        System.Action<TirNode> enter = null, System.Action<TirNode> exit = null)
    {
        if (n == null)
            return null;
        enter?.Invoke(n);
        var result = RewriteCore(n, f, enter, exit);
        exit?.Invoke(n);
        return result;
    }

    private static TirNode RewriteCore(TirNode n, System.Func<TirNode, TirNode> f,
        System.Action<TirNode> enter, System.Action<TirNode> exit)
    {
        List<TirNode> Rw(IReadOnlyList<TirNode> ns)
            => ns?.Select(x => Rewrite(x, f, enter, exit)).ToList();

        switch (n)
        {
            case TirCall c:
                return f(new TirCall(c.Callee, c.EmissionKind, c.ParameterTypes, c.ReturnType,
                    Rw(c.Args), c.Type, c.Origin, c.Name));
            case TirCoerce co:
                return f(new TirCoerce(Rewrite(co.Inner, f, enter, exit), co.FromType, co.ToType, co.ConversionFn, co.Origin));
            case TirInvoke inv:
                return f(new TirInvoke(Rewrite(inv.Target, f, enter, exit), Rw(inv.Args), inv.Type, inv.Origin));
            case TirConditional cond:
                return f(new TirConditional(Rewrite(cond.Condition, f, enter, exit), Rewrite(cond.IfTrue, f, enter, exit),
                    Rewrite(cond.IfFalse, f, enter, exit), cond.Type, cond.Origin));
            case TirNew nw:
                return f(new TirNew(nw.NewType, Rw(nw.Args), nw.Type, nw.Origin));
            case TirArray arr:
                return f(new TirArray(Rw(arr.Elements), arr.Type, arr.Origin));
            case TirAssign asg:
                return f(new TirAssign(Rewrite(asg.LValue, f, enter, exit), Rewrite(asg.RValue, f, enter, exit), asg.Type, asg.Origin));
            case TirLambda lam:
                return f(new TirLambda(lam.Parameters, Rewrite(lam.Body, f, enter, exit), lam.Type, lam.Origin));
            case TirLet let:
                return f(new TirLet(let.Def, Rewrite(let.Value, f, enter, exit), let.Type, let.Origin));
            case TirBlock b:
                return f(new TirBlock(Rw(b.Statements), b.Origin));
            case TirReturn r:
                return f(new TirReturn(Rewrite(r.Value, f, enter, exit), r.Origin));
            case TirIf iff:
                return f(new TirIf(Rewrite(iff.Condition, f, enter, exit), Rewrite(iff.IfTrue, f, enter, exit), Rewrite(iff.IfFalse, f, enter, exit), iff.Origin));
            case TirLoop l:
                return f(new TirLoop(Rewrite(l.Condition, f, enter, exit), Rewrite(l.Body, f, enter, exit), l.Origin));
            case TirComponentAccess ca:
                return f(new TirComponentAccess(Rewrite(ca.Receiver, f, enter, exit), ca.FieldName, ca.CastTo, ca.ScalarComponentPrim));
            case TirConstructorCall cc:
                return f(new TirConstructorCall(cc.TypeName, Rw(cc.Args)));
            case TirBooleanChain bc:
                return f(new TirBooleanChain(bc.Op, Rw(bc.Terms)));
            default:
                return f(n); // leaves
        }
    }

    /// <summary>Replace every <see cref="TirParameter"/> bound to a def in <paramref name="map"/> by
    /// its mapped node. Nodes not in the map are rebuilt unchanged. No alpha-renaming (see the class
    /// remarks for the assumption this makes).</summary>
    public static TirNode Substitute(TirNode body, IReadOnlyDictionary<ParameterDef, TirNode> map)
    {
        if (body == null || map == null || map.Count == 0)
            return body;
        return Rewrite(body, n =>
            n is TirParameter p && p.Def != null && map.TryGetValue(p.Def, out var replacement)
                ? replacement
                : n);
    }

    /// <summary>β-reduce an immediately-applied lambda: <c>(λx….e)(a…) → e[x↦a]</c>. Returns false
    /// (and leaves <paramref name="reduced"/> null) when <paramref name="invoke"/>'s target is not a
    /// lambda literal, the arity disagrees, or a substitution would change how many times an argument
    /// is evaluated (a non-cheap argument bound to a parameter used more than once, or used under a
    /// nested lambda). A coercion wrapping the lambda target — the delegate target-typing this
    /// reduction eliminates — is transparent.</summary>
    public static bool TryBetaReduce(TirInvoke invoke, out TirNode reduced,
        System.Func<TirNode, bool> isCheap = null)
    {
        isCheap = isCheap ?? IsCheap;
        reduced = null;
        if (invoke?.Target == null)
            return false;
        if (!(StripCoerce(invoke.Target) is TirLambda lam))
            return false;
        var pars = lam.Parameters;
        if (pars == null || lam.Body == null || pars.Count != invoke.Args.Count)
            return false;

        Dictionary<ParameterDef, (int Count, bool UnderLambda)> uses = null;
        for (var i = 0; i < pars.Count; i++)
        {
            if (isCheap(invoke.Args[i]))
                continue;
            uses = uses ?? CountParamUses(lam.Body);
            if (uses.TryGetValue(pars[i], out var u) && (u.Count > 1 || u.UnderLambda))
                return false;
        }

        var map = new Dictionary<ParameterDef, TirNode>();
        for (var i = 0; i < pars.Count; i++)
            map[pars[i]] = invoke.Args[i];
        reduced = Substitute(lam.Body, map);
        return true;
    }

    /// <summary>Strip transparent coercions to reach the underlying value node.</summary>
    public static TirNode StripCoerce(TirNode n)
        => n is TirCoerce c ? StripCoerce(c.Inner) : n;

    /// <summary>Whether a node is a STATEMENT tree rather than an expression: a block, return, if,
    /// loop, or a lowered-loop marker. The one home for the check the inliner, unroller and body
    /// writer each used to keep a private copy of (the pre-lowering passes never see a
    /// <see cref="TirLoweredLoop"/>, so including it here is a harmless superset for them).</summary>
    public static bool IsStatementNode(TirNode n)
        => n is TirBlock || n is TirReturn || n is TirIf || n is TirLoop || n is TirLoweredLoop;

    /// <summary>A leaf whose repeated substitution costs nothing (parameter / variable / literal /
    /// type ref / default) — safe to substitute regardless of use count.</summary>
    public static bool IsCheap(TirNode n)
    {
        n = StripCoerce(n);
        return n is TirParameter || n is TirVariable || n is TirLiteral || n is TirTypeRef || n is TirDefault;
    }

    /// <summary>Per-parameter reference statistics over a body: occurrence count and whether any
    /// occurrence sits under a lambda (where a substituted compound expression would be re-evaluated
    /// per invocation and captured).</summary>
    public static Dictionary<ParameterDef, (int Count, bool UnderLambda)> CountParamUses(TirNode body)
    {
        var uses = new Dictionary<ParameterDef, (int, bool)>();
        void Walk(TirNode n, bool underLambda)
        {
            if (n == null)
                return;
            if (n is TirParameter p && p.Def != null)
            {
                uses.TryGetValue(p.Def, out var u);
                uses[p.Def] = (u.Item1 + 1, u.Item2 || underLambda);
            }
            var inner = underLambda || n is TirLambda;
            foreach (var c in n.Children)
                Walk(c, inner);
        }
        Walk(body, false);
        return uses;
    }
}
