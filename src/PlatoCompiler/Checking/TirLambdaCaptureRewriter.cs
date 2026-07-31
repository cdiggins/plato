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
        // rebuild. It stays keyed on NODE IDENTITY (the captured reference itself), which is what
        // confines it to the occurrences inside the lambda: keying it on the parameter DEFINITION
        // also rewrote the receiver OUTSIDE the lambda, where `this` is the only legal spelling.
        var lets = new List<TirLet>();
        var byNode = new Dictionary<TirNode, TirNode>(ReferenceEquality.Instance);
        var lambdaBound = new HashSet<object>(body.Descendants()
            .OfType<TirLambda>()
            .SelectMany(l => l.Parameters ?? Enumerable.Empty<ParameterDef>()));

        foreach (var capture in captures)
        {
            // Only a captured PARAMETER may be hoisted: it is in scope at the body's top, so
            // wrapping the whole body in `var _varN = p; { … }` is valid. A captured LOCAL is left
            // in place — C# closures capture locals correctly, and in this pure single-assignment
            // language the by-value snapshot is never semantically needed — because hoisting it
            // above the body would forward-reference its own mid-body declaration (the
            // Solids.NGonPoint/SquarePoint CS0103 bug, surfaced once `Lerp` inlines a lambda that
            // captures the local `f`).
            if (!(capture is TirParameter p))
                continue;
            // ... and only one bound by the ENCLOSING FUNCTION. A parameter of an outer LAMBDA is
            // a capture of the inner lambda too, but it does not exist at the body's top: hoisting
            // it emitted `var _varN = l;` above the lambda that binds `l` (Brep3D.Validate, whose
            // inlined predicates nest two lambdas deep). Left in place, like a captured local.
            if (p.Def != null && lambdaBound.Contains(p.Def))
                continue;
            var def = new VariableDef(null, $"_var{SymbolRewriter.NextId++}", capture.Type, null);
            lets.Add(new TirLet(def, capture, capture.Type, capture.Origin));
            // A reference node is shared when the inliner substituted one argument into several
            // callee positions, so the same capture can be listed twice: the FIRST local wins for
            // every occurrence and the later ones stay declared-but-unused, exactly as the
            // one-capture-at-a-time rewrite this replaced behaved.
            if (!byNode.ContainsKey(capture))
                byNode[capture] = new TirVariable(def, capture.Type, capture.Origin);
        }

        body = ReplaceNodes(body, byNode);

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

    /// <summary>Rebuild the tree with every node identity in <paramref name="subs"/> replaced by
    /// the hoisted local. Structural via <see cref="TirNode.WithChildren"/> rather than a switch
    /// over the node kinds: a switch could only name the kinds declared in THIS assembly, so every
    /// emission-only marker a writer adds (a constructor call, a component access, a lowered loop)
    /// was an opaque leaf whose subtree kept the captured reference — the `self.At(i)` CS0103
    /// leak, which showed up only once the inliner started folding bodies into those markers.</summary>
    private static TirNode ReplaceNodes(TirNode n, Dictionary<TirNode, TirNode> subs)
    {
        if (n == null)
            return null;
        if (subs.TryGetValue(n, out var repl))
            return repl;
        var children = n.Children.Select(c => ReplaceNodes(c, subs)).ToList();
        return children.Count == 0 ? n : n.WithChildren(children);
    }

    private class ReferenceEquality : IEqualityComparer<TirNode>
    {
        public static readonly ReferenceEquality Instance = new ReferenceEquality();
        public bool Equals(TirNode a, TirNode b) => ReferenceEquals(a, b);
        public int GetHashCode(TirNode n) => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(n);
    }
}
