// Chooses the storage of each array binding of one body (--owned-arrays).
using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Symbols;

namespace Ara3D.Geometry.CSharpWriter;

/// <summary>
/// Pass 7 of the pipeline: runs at the body writer's entry, after its own capture rewrite, and wraps the body in a
/// <see cref="TirStorageScope"/>. It gives the temporary of a lowered array loop Heap storage when the loop fills
/// its own <c>T[]</c> and the temporary is declared once, never assigned another array, and never updated in place.
/// The loop lowerer binds a temporary to a typed local (<see cref="TirInlineBinding"/>) before the call that reads
/// it; such a local is the same array under another name and takes the temporary's storage on the same conditions
/// (and when no lambda captures it). Every other binding stays List (an empty plan entry). A Heap binding is read as <c>x[i]</c> and
/// <c>x.Length</c> where it is the receiver of <c>Count</c> or <c>At</c> or the source of a lowered loop, and is
/// wrapped in a <see cref="TirRepresent"/> to List everywhere else: a return, a seed, a conditional arm, a
/// binding, a call argument, a constructor argument, a lambda result. <see cref="StorageChecker"/> verifies the
/// result before anything prints.
/// </summary>
public static class TirStorageResolver
{
    /// <summary>The body is wrapped as it is: the very same node, not a copy. Every binding is List.</summary>
    public static TirStorageScope Resolve(TirNode body, FunctionDef owner)
        => new(body, new StoragePlan(owner));

    /// <summary>Resolves <paramref name="body"/> with Heap storage for the owned lowered-loop temporaries.
    /// Returns the very same body inside the scope when no temporary qualifies.</summary>
    public static TirStorageScope Resolve(TirNode body, FunctionDef owner, TirArrayIdentity identity)
    {
        var plan = new StoragePlan(owner);
        var ownership = TirOwnership.Analyze(body, identity);
        foreach (var loop in body.Descendants().OfType<TirLoweredLoop>())
        {
            if (!TirLoopLowerer.ProducesArray(loop.Kind) || plan.OfTemp(loop.TempName) != ArrayStorage.List)
                continue;
            var refusal = Refusal(ownership.OfTemp(loop.TempName));
            if (refusal == null)
                plan.AssignTemp(loop.TempName, ArrayStorage.Heap);
            else
                plan.Refuse(loop.TempName, refusal);
        }
        // The lowerer's alias locals, in the order they are declared, so an alias of an alias is found too.
        foreach (var binding in body.Descendants().OfType<TirInlineBinding>())
        {
            var aliased = TirRewrite.StripCoerce(binding.Value) switch
            {
                TirTempRef t => plan.OfTemp(t.Name) == ArrayStorage.Heap,
                TirVariable { Def: { } other } => plan.Of(other) == ArrayStorage.Heap,
                _ => false,
            };
            if (!aliased || binding.Def == null)
                continue;
            var refusal = Refusal(ownership.Of(binding.Def)) ?? AliasRefusal(ownership.Of(binding.Def));
            if (refusal == null)
                plan.Assign(binding.Def, ArrayStorage.Heap);
            else
                plan.Refuse(binding.Def.Name, refusal);
        }
        var resolved = new Converter(plan, identity).Walk(body, false);
        return new TirStorageScope(resolved, plan);
    }

    // A lowered loop of an array kind fills its own T[] (kinds are filtered by the caller). What remains is whether
    // anything could make the temporary something else than that array: a second declaration of the name, an
    // assignment of another value, or an in-place update (kept List by the plan so SetAt stays untouched).
    private static string Refusal(BindingFacts facts)
    {
        if (facts == null) return "no ownership facts";
        if (facts.Declarations != 1) return "declared more than once";
        if (facts.Aliased) return "assigned another array";
        if (facts.Uses.Any(u => u.Kind == TirUpdateFlow.UseKind.Update)) return "updated in place";
        return null;
    }

    // A local is the temporary's alias only while nothing reassigns it and no closure names it: the per-lambda capture
    // rewrite hoists a captured local into a new List local after this pass has run.
    private static string AliasRefusal(BindingFacts facts)
    {
        if (facts.AssignedLoops.Count > 0) return "assigned after its declaration";
        if (facts.Captured) return "captured by a closure";
        return null;
    }

    /// <summary>Rewrites the body so each reference to a Heap temporary sits where its storage is accepted.</summary>
    private sealed class Converter
    {
        private readonly StoragePlan _plan;
        private readonly TirArrayIdentity _identity;

        public Converter(StoragePlan plan, TirArrayIdentity identity) => (_plan, _identity) = (plan, identity);

        /// <param name="observed">The position reads the array and keeps nothing (the receiver of Count or At,
        /// a lowered loop's source), so it accepts any storage.</param>
        public TirNode Walk(TirNode n, bool observed)
        {
            switch (n)
            {
                case null:
                    return null;
                case TirTempRef t when _plan.OfTemp(t.Name) == ArrayStorage.Heap:
                    return observed ? n : new TirRepresent(n, ArrayStorage.Heap, ArrayStorage.List);
                case TirVariable v when _plan.Of(v.Def) == ArrayStorage.Heap:
                    return observed ? n : new TirRepresent(n, ArrayStorage.Heap, ArrayStorage.List);
                case TirInlineBinding binding when _plan.Of(binding.Def) == ArrayStorage.Heap:
                    // The alias holds the very array: its value is taken as it is.
                    return With(n, new[] { Walk(binding.Value, true) });
                case TirCoerce c when TirRewrite.IsListType(c.Type) && TirRewrite.IsListType(TirRewrite.StripCoerce(c.Inner)?.Type):
                    return With(n, new[] { Walk(c.Inner, observed) });
                case TirCall call when call.Args.Count > 0 && _identity.IsObserver(call):
                    return With(n, call.Children.Select((child, i) => Walk(child, i == 0)).ToList());
                case TirLoweredLoop loop:
                {
                    // A FlatMap source reaches the exact-output and multi-consumer printers, which read a source
                    // through the List forms; the later chunk that teaches them Heap drops this exception.
                    var sources = loop.Kind != "FlatMap";
                    return With(n, loop.Children.Select((child, i) => Walk(child, sources && i < loop.Sources.Count)).ToList());
                }
                default:
                    return With(n, n.Children.Select(child => Walk(child, false)).ToList());
            }
        }

        private static TirNode With(TirNode n, IReadOnlyList<TirNode> children)
        {
            var old = n.Children.ToList();
            for (var i = 0; i < old.Count; i++)
                if (!ReferenceEquals(old[i], children[i]))
                    return n.WithChildren(children);
            return n;
        }
    }
}
