using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.Compiler.Symbols;

namespace Ara3D.Geometry.Compiler.Checking;

/// <summary>
/// The TIR mirror of <see cref="SymbolRewriter.RewriteLambdasCapturingVars"/>, which the current
/// writer applies to every body before emission: each parameter/variable reference captured by a
/// lambda is hoisted into a <c>var _var{N} = x;</c> declaration wrapping the body (innermost = the
/// first capture in pre-order), and the captured reference is replaced by the new local. The C#
/// output depends on it byte-for-byte, so the TIR emit path must reproduce it exactly — including
/// drawing the <c>_var{N}</c> names from the SAME process-global counter
/// (<see cref="SymbolRewriter.NextId"/>), so a flag-on generation numbers identically to the
/// current writer's output.
///
/// Mirrors the reference precisely:
///   * capture enumeration order = lambdas in pre-order, then each lambda's captured references in
///     pre-order (<c>FunctionDef.CapturedSymbols</c>);
///   * one hoist per REFERENCE (a parameter captured twice yields two locals), each wrapping the
///     whole previous body in a new block;
///   * an expression body is first statementized (<c>return e;</c>);
///   * the same rewrite re-runs on each lambda body as it is written (the reference constructs a
///     fresh <see cref="CSharpFunctionBodyWriter"/> per lambda, whose constructor rewrites again).
/// </summary>
public static class TirLambdaCaptureRewriter
{
    /// <summary>Hoist lambda-captured references out of <paramref name="body"/>. Returns the body
    /// unchanged when no lambda captures anything.</summary>
    public static TirNode Rewrite(TirNode body)
    {
        if (body == null)
            return null;

        var captures = body.Descendants()
            .OfType<TirLambda>()
            .SelectMany(CapturedRefs)
            .ToList();

        if (captures.Count == 0)
            return body;

        if (!IsStatement(body))
            body = new TirReturn(body, body.Origin);

        // plato-308: the substitution is planned for ALL captures first and applied in ONE
        // rebuild. Replacing them one at a time rebuilt the tree after the first hoist, so every
        // later capture node lost reference identity with the tree and its `_varN` local was
        // declared but never used — leaving the original reference in the lambda. Harmless for a
        // captured parameter (still in scope inside the closure), fatal for `self` in a struct
        // method, which cannot legally appear inside a lambda at all: TriangleFace.Edges and
        // friends emitted `self.At(i)` and failed with CS0103.
        var lets = new List<TirLet>();
        var byDef = new Dictionary<object, TirNode>();

        foreach (var capture in captures)
        {
            // Only a captured PARAMETER may be hoisted: it is in scope at the body's top, so
            // wrapping the whole body in `var _varN = p; { … }` is valid. A captured LOCAL is left
            // in place — C# closures capture locals correctly, and in this pure single-assignment
            // language the by-value snapshot is never semantically needed — because hoisting it
            // above the body would forward-reference its own mid-body declaration (the
            // Solids.NGonPoint/SquarePoint CS0103 bug, surfaced once `Lerp` inlines a lambda that
            // captures the local `f`).
            if (!(capture is TirParameter))
                continue;
            var def = new VariableDef(null, $"_var{SymbolRewriter.NextId++}", capture.Type, null);
            lets.Add(new TirLet(def, capture, capture.Type, capture.Origin));
            var key = ((TirParameter)capture).Def;
            if (key != null && !byDef.ContainsKey(key))
                byDef[key] = new TirVariable(def, capture.Type, capture.Origin);
        }

        body = ReplaceNodes(body, byDef);

        // Innermost block = the first capture in pre-order, as the reference rewriter produced.
        foreach (var let in lets)
            body = new TirBlock(new List<TirNode> { let, body }, body.Origin);

        return body;
    }

    /// <summary>The references inside <paramref name="lam"/> (pre-order) whose definition lives
    /// outside its subtree — the TIR analogue of <c>FunctionDef.CapturedSymbols</c>.</summary>
    private static IEnumerable<TirNode> CapturedRefs(TirLambda lam)
    {
        var defs = new HashSet<object>(lam.Parameters ?? Enumerable.Empty<ParameterDef>());
        foreach (var n in lam.Body?.Descendants() ?? Enumerable.Empty<TirNode>())
        {
            switch (n)
            {
                case TirLambda nested:
                    foreach (var p in nested.Parameters ?? Enumerable.Empty<ParameterDef>())
                        defs.Add(p);
                    break;
                case TirLet let when let.Def != null:
                    defs.Add(let.Def);
                    break;
            }
        }

        foreach (var n in lam.Body?.Descendants() ?? Enumerable.Empty<TirNode>())
        {
            if (n is TirParameter p && p.Def != null && !defs.Contains(p.Def))
                yield return n;
            else if (n is TirVariable v && v.Def != null && !defs.Contains(v.Def))
                yield return n;
        }
    }

    private static bool IsStatement(TirNode n)
        => n is TirBlock || n is TirReturn || n is TirIf || n is TirLoop;

    /// <summary>Rebuild the tree, replacing every parameter reference whose DEFINITION is a key of
    /// <paramref name="subs"/> with the hoisted local. Definition-keyed rather than node-identity
    /// keyed: the capture nodes are collected before this rebuild, and matching on them alone
    /// silently dropped every substitution after the first.</summary>
    private static TirNode ReplaceNodes(TirNode n, Dictionary<object, TirNode> subs)
    {
        if (n == null)
            return null;
        if (n is TirParameter p && p.Def != null && subs.TryGetValue(p.Def, out var repl))
            return repl;

        List<TirNode> Rw(IReadOnlyList<TirNode> ns)
            => ns?.Select(x => ReplaceNodes(x, subs)).ToList();

        switch (n)
        {
            case TirCall c:
                return new TirCall(c.Callee, c.EmissionKind, c.ParameterTypes, c.ReturnType,
                    Rw(c.Args), c.Type, c.Origin, c.Name);
            case TirCoerce co:
                return new TirCoerce(ReplaceNodes(co.Inner, subs), co.FromType, co.ToType,
                    co.ConversionFn, co.Origin);
            case TirInvoke inv:
                return new TirInvoke(ReplaceNodes(inv.Target, subs), Rw(inv.Args), inv.Type, inv.Origin);
            case TirConditional cond:
                return new TirConditional(ReplaceNodes(cond.Condition, subs),
                    ReplaceNodes(cond.IfTrue, subs),
                    ReplaceNodes(cond.IfFalse, subs), cond.Type, cond.Origin);
            case TirNew nw:
                return new TirNew(nw.NewType, Rw(nw.Args), nw.Type, nw.Origin);
            case TirArray arr:
                return new TirArray(Rw(arr.Elements), arr.Type, arr.Origin);
            case TirAssign asg:
                return new TirAssign(ReplaceNodes(asg.LValue, subs),
                    ReplaceNodes(asg.RValue, subs), asg.Type, asg.Origin);
            case TirLambda lam:
                return new TirLambda(lam.Parameters, ReplaceNodes(lam.Body, subs), lam.Type, lam.Origin);
            case TirLet let:
                return new TirLet(let.Def, ReplaceNodes(let.Value, subs), let.Type, let.Origin);
            case TirBlock b:
                return new TirBlock(Rw(b.Statements), b.Origin);
            case TirReturn r:
                return new TirReturn(ReplaceNodes(r.Value, subs), r.Origin);
            case TirIf iff:
                return new TirIf(ReplaceNodes(iff.Condition, subs),
                    ReplaceNodes(iff.IfTrue, subs),
                    ReplaceNodes(iff.IfFalse, subs), iff.Origin);
            case TirLoop l:
                return new TirLoop(ReplaceNodes(l.Condition, subs),
                    ReplaceNodes(l.Body, subs), l.Origin);
            case TirUnresolved u:
                return new TirUnresolved(u.Original, u.Reason, Rw(u.ChildNodes));
            default:
                return n; // leaves
        }
    }
}
