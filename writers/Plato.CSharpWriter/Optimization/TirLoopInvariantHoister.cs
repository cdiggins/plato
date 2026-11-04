// Moves loop-invariant callback work out of lowered loops (--hoist-invariants).
using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Symbols;
using Ara3D.Geometry.Optimizer;

namespace Ara3D.Geometry.CSharpWriter;

/// <summary>
/// The pass after loop lowering (--hoist-invariants, TKT-1092). A lowered loop
/// (<see cref="TirLoweredLoop"/>) emits its callback's body once per element; a subexpression of
/// that body that names neither the callback's parameters nor anything bound inside the body has
/// the same value on every iteration, so it is computed once, before the loop, into a local
/// (<see cref="TirLoweredLoop.Hoisted"/>), and the body reads the local.
///
/// The rules, all of which must hold for an expression to move:
///   * the callback is a lambda literal with an expression body (a statement body is left alone);
///   * the expression is INVARIANT: no reference to a callback parameter, to a parameter of a
///     lambda nested in the body, or to a local bound in the body, and built only from calls,
///     conversions, constructions, component reads, delegate invocations of a parameter or local,
///     parameters, locals, literals, type references and loop temps;
///   * it is evaluated UNCONDITIONALLY by every iteration: the path from the body's root passes
///     only through operands that C# always evaluates (call arguments and receivers, conversion
///     operands, constructor arguments), never a conditional's arms, a short-circuit chain's later
///     terms, or a nested lambda's body. Plato's `&amp;&amp;`/`||` operators are emitted as the
///     non-short-circuit `&amp;`/`|`, so their operands are unconditional;
///   * it is worth a local: not a cheap projection (a parameter, a field read, a few whitelisted
///     scalar operators; see <see cref="TirInliner.IsCheapProjection"/>), since every hoisted
///     local also costs the JIT a local slot;
///   * the largest such expression moves, not its parts.
///
/// The hoisted locals are emitted inside `if (count &gt; 0)`, so a loop that runs no iteration
/// evaluates none of them: an expression that would fault (an out-of-range index) still faults
/// only when the first iteration would have evaluated it. In a pure language the value is the same
/// on every iteration, so the output is bit-identical. The one observable difference is which fault
/// is reported when the first iteration faults in an expression that runs before a hoisted one
/// (a fused producer stage, say): both programs fail, at the same element.
///
/// Exposure: a variant call in an unconditional position whose callee is a small forwarding
/// function (an expression body of at most 48 nodes with no conditional, short-circuit chain or lambda)
/// is expanded under the inliner's under-lambda rules when, and only when, the expansion contains
/// an expression these rules hoist. That is how `g.PartialU(s, uv)` in a loop over `uv` gives up
/// `g.SurfaceAt(s)` and `g.DomainOnce(s)`, which the call's body computes per sample.
/// </summary>
public static class TirLoopInvariantHoister
{
    // Levels of forwarding calls expanded to expose an invariant (PartialU -> EvaluatedAt -> ...).
    private const int MaxExposeDepth = 4;

    // Size of one expanded forwarding body; a larger body is the callee's real work, not a wrapper.
    private const int MaxExposedNodes = 48;

    public static TirFunction Rewrite(TirFunction tir, PassContext pass)
    {
        var ownerTypeName = pass.OwnerTypeName;
        if (!pass.On("hoist-invariants") || !pass.On("loops") || tir?.Body == null)
            return tir;
        var changed = false;
        HashSet<string> callerNames = null;
        var body = TirRewrite.Rewrite(tir.Body, n =>
        {
            if (n is not TirLoweredLoop loop)
                return n;
            var context = new Context(tir, pass.InlineHost, ownerTypeName,
                () => callerNames ??= TirInliner.BoundNamesForHoisting(tir));
            var hoisted = HoistLoop(loop, context);
            if (ReferenceEquals(hoisted, loop))
                return n;
            changed = true;
            pass.Stats.HoistedInvariants += context.Hoisted;
            pass.Stats.HoistExposingInlines += context.Inlined;
            pass.Stats.HoistedLoops++;
            return hoisted;
        });
        return changed
            ? new TirFunction(tir.Original, tir.Parameters, tir.ReturnType, body, tir.ZonkedParameterTypes, tir.ZonkedReturnType)
            : tir;
    }

    private sealed class Context
    {
        public readonly TirFunction Tir;
        public readonly ITirInlineHost Writer;
        public readonly string OwnerTypeName;
        public readonly System.Func<HashSet<string>> CallerNames;
        public readonly HashSet<object> Bound = new();
        public int Hoisted;
        public int Inlined;

        public Context(TirFunction tir, ITirInlineHost writer, string ownerTypeName, System.Func<HashSet<string>> callerNames)
            => (Tir, Writer, OwnerTypeName, CallerNames) = (tir, writer, ownerTypeName, callerNames);
    }

    private static TirNode HoistLoop(TirLoweredLoop loop, Context context)
    {
        // Sort and SortIdx (--closure-free) write their comparator in the merge loop, outside the
        // guarded `For` that would declare the hoisted locals, and call it only from two elements on.
        if (loop.Fn == null || loop.Hoisted.Count > 0 || loop.Kind is "Sort" or "SortIdx"
            || TirRewrite.StripCoerce(loop.Fn) is not TirLambda lambda
            || lambda.Body == null || TirRewrite.IsStatementNode(lambda.Body))
            return loop;

        foreach (var p in lambda.Parameters)
            context.Bound.Add(p);
        foreach (var n in lambda.Body.Descendants())
            switch (n)
            {
                case TirLambda nested:
                    foreach (var p in nested.Parameters)
                        context.Bound.Add(p);
                    break;
                case TirLet let when let.Def != null:
                    context.Bound.Add(let.Def);
                    break;
                case TirInlineBinding binding when binding.Def != null:
                    context.Bound.Add(binding.Def);
                    break;
            }

        var exposed = Expose(lambda.Body, context, 0);
        var lets = new List<TirLet>();
        var body = Collect(exposed, context, lets, loop.Id);
        if (lets.Count == 0)
            return loop;
        context.Hoisted = lets.Count;

        var newLambda = new TirLambda(lambda.Parameters, body, lambda.Type, lambda.Origin);
        var rebuilt = new TirLoweredLoop(loop.Kind, loop.Sources, ReplaceStripped(loop.Fn, newLambda),
            loop.Seed, loop.IncludeFirst, loop.TempName, loop.Id, loop.Type, loop.Origin, loop.SourceFunction)
        {
            ElemType = loop.ElemType,
            IndexSource = loop.IndexSource,
            SourceStages = loop.SourceStages,
            Hoisted = lets,
        };
        return rebuilt;
    }

    // The callback with its lambda replaced, keeping any conversion wrapped around it.
    private static TirNode ReplaceStripped(TirNode fn, TirLambda lambda)
        => fn is TirCoerce c ? c.WithChildren(new[] { ReplaceStripped(c.Inner, lambda) }) : lambda;

    // --- the hoisting walk ---------------------------------------------------------------------

    private static TirNode Collect(TirNode n, Context context, List<TirLet> lets, int loopId)
    {
        if (n == null)
            return null;
        if (IsHoistable(n, context))
        {
            var def = new VariableDef(null, $"_inv{loopId}_{lets.Count}", n.Type, null);
            lets.Add(new TirLet(def, n, n.Type, n.Origin));
            return new TirVariable(def, n.Type, n.Origin);
        }
        return WithStrictChildren(n, c => Collect(c, context, lets, loopId));
    }

    private static int CountHoistable(TirNode n, Context context)
    {
        if (n == null)
            return 0;
        if (IsHoistable(n, context))
            return 1;
        var count = 0;
        var children = n.Children.ToArray();
        foreach (var i in StrictChildIndices(n, children.Length))
            count += CountHoistable(children[i], context);
        return count;
    }

    // Expands forwarding calls whose bodies contain a hoistable expression (see the class remarks).
    private static TirNode Expose(TirNode n, Context context, int depth)
    {
        if (n == null || IsHoistable(n, context))
            return n;
        if (depth < MaxExposeDepth && n is TirCall call && call.Args.Count > 0
            && TirInliner.TryInlineForHoisting(call, context.Tir, context.Writer, context.OwnerTypeName,
                context.CallerNames(), out var inlined)
            && IsForwardingBody(inlined))
        {
            var expanded = Expose(inlined, context, depth + 1);
            if (CountHoistable(expanded, context) > 0)
            {
                context.Inlined++;
                return expanded;
            }
        }
        return WithStrictChildren(n, c => Expose(c, context, depth));
    }

    private static bool IsForwardingBody(TirNode body)
    {
        var count = 0;
        foreach (var d in body.Descendants())
        {
            if (++count > MaxExposedNodes
                || d is TirConditional or TirBooleanChain or TirLambda or TirLet or TirAssign
                || TirRewrite.IsStatementNode(d))
                return false;
        }
        return true;
    }

    // --- the rules -----------------------------------------------------------------------------

    private static bool IsHoistable(TirNode n, Context context)
        => n.Type != null
           && n is TirCall { Name: not null } or TirInvoke or TirNew or TirConstructorCall
           && !(n is TirCall tuple && tuple.Name.StartsWith("Tuple"))
           && IsInvariant(n, context)
           && !TirInliner.IsCheapProjection(context.Writer, n);

    private static bool IsInvariant(TirNode n, Context context)
    {
        foreach (var d in n.Descendants())
        {
            switch (d)
            {
                case TirParameter p:
                    if (p.Def == null || context.Bound.Contains(p.Def)) return false;
                    break;
                case TirVariable v:
                    if (v.Def == null || context.Bound.Contains(v.Def)) return false;
                    break;
                case TirLiteral or TirTypeRef or TirTempRef or TirCoerce or TirNew or TirConstructorCall or TirComponentAccess:
                    break;
                case TirCall c:
                    if (c.Name == null || c.Name.StartsWith("Tuple")) return false;
                    break;
                case TirInvoke invoke:
                    if (TirRewrite.StripCoerce(invoke.Target) is not (TirParameter or TirVariable)) return false;
                    break;
                default:
                    return false;
            }
        }
        return true;
    }

    // The children C# evaluates every time the node is evaluated.
    private static IEnumerable<int> StrictChildIndices(TirNode n, int count)
        => n switch
        {
            TirCall or TirCoerce or TirNew or TirConstructorCall or TirComponentAccess or TirInvoke or TirArray
                => Enumerable.Range(0, count),
            TirConditional or TirBooleanChain => count > 0 ? new[] { 0 } : System.Array.Empty<int>(),
            _ => System.Array.Empty<int>(),
        };

    private static TirNode WithStrictChildren(TirNode n, System.Func<TirNode, TirNode> rewrite)
    {
        var children = n.Children.ToArray();
        TirNode[] replaced = null;
        foreach (var i in StrictChildIndices(n, children.Length))
        {
            if (children[i] == null)
                continue;
            var child = rewrite(children[i]);
            if (ReferenceEquals(child, children[i]))
                continue;
            replaced ??= (TirNode[])children.Clone();
            replaced[i] = child;
        }
        return replaced == null ? n : n.WithChildren(replaced);
    }
}
