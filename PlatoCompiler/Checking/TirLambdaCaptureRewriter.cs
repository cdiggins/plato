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

        foreach (var capture in captures)
        {
            var def = new VariableDef(null, $"_var{SymbolRewriter.NextId++}", capture.Type, null);
            var let = new TirLet(def, capture, capture.Type, capture.Origin);
            var replaced = ReplaceNode(body, capture, new TirVariable(def, capture.Type, capture.Origin));
            body = new TirBlock(new List<TirNode> { let, replaced }, body.Origin);
        }

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

    /// <summary>Rebuild the tree with the single node identity <paramref name="target"/> replaced
    /// by <paramref name="replacement"/> (reference equality, one occurrence).</summary>
    private static TirNode ReplaceNode(TirNode n, TirNode target, TirNode replacement)
    {
        if (n == null)
            return null;
        if (ReferenceEquals(n, target))
            return replacement;

        List<TirNode> Rw(IReadOnlyList<TirNode> ns)
            => ns?.Select(x => ReplaceNode(x, target, replacement)).ToList();

        switch (n)
        {
            case TirCall c:
                return new TirCall(c.Callee, c.EmissionKind, c.ParameterTypes, c.ReturnType,
                    Rw(c.Args), c.Type, c.Origin, c.Name);
            case TirCoerce co:
                return new TirCoerce(ReplaceNode(co.Inner, target, replacement), co.FromType, co.ToType,
                    co.ConversionFn, co.Origin);
            case TirInvoke inv:
                return new TirInvoke(ReplaceNode(inv.Target, target, replacement), Rw(inv.Args), inv.Type, inv.Origin);
            case TirConditional cond:
                return new TirConditional(ReplaceNode(cond.Condition, target, replacement),
                    ReplaceNode(cond.IfTrue, target, replacement),
                    ReplaceNode(cond.IfFalse, target, replacement), cond.Type, cond.Origin);
            case TirNew nw:
                return new TirNew(nw.NewType, Rw(nw.Args), nw.Type, nw.Origin);
            case TirArray arr:
                return new TirArray(Rw(arr.Elements), arr.Type, arr.Origin);
            case TirAssign asg:
                return new TirAssign(ReplaceNode(asg.LValue, target, replacement),
                    ReplaceNode(asg.RValue, target, replacement), asg.Type, asg.Origin);
            case TirLambda lam:
                return new TirLambda(lam.Parameters, ReplaceNode(lam.Body, target, replacement), lam.Type, lam.Origin);
            case TirLet let:
                return new TirLet(let.Def, ReplaceNode(let.Value, target, replacement), let.Type, let.Origin);
            case TirBlock b:
                return new TirBlock(Rw(b.Statements), b.Origin);
            case TirReturn r:
                return new TirReturn(ReplaceNode(r.Value, target, replacement), r.Origin);
            case TirIf iff:
                return new TirIf(ReplaceNode(iff.Condition, target, replacement),
                    ReplaceNode(iff.IfTrue, target, replacement),
                    ReplaceNode(iff.IfFalse, target, replacement), iff.Origin);
            case TirLoop l:
                return new TirLoop(ReplaceNode(l.Condition, target, replacement),
                    ReplaceNode(l.Body, target, replacement), l.Origin);
            case TirUnresolved u:
                return new TirUnresolved(u.Original, u.Reason, Rw(u.ChildNodes));
            default:
                return n; // leaves
        }
    }
}
