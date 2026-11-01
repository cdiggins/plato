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
        => Resolve(body, owner, identity, null);

    /// <summary>As above; with a <paramref name="stack"/> target (--stack-arrays) an owned temporary whose count is
    /// bounded and whose every use only reads it takes a hoisted <c>stackalloc</c> slot instead (<see cref="AssignStack"/>).</summary>
    public static TirStorageScope Resolve(TirNode body, FunctionDef owner, TirArrayIdentity identity, IStorageTarget stack)
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
        // Named locals that hold a Heap array under another name: the lowerer's alias locals (TirInlineBinding) and the
        // user's own single-declaration locals (TirLet), in the order they are declared so an alias of an alias is found.
        foreach (var (def, value) in body.Descendants().Select(Declaration).Where(d => d.Def != null))
        {
            var aliased = TirRewrite.StripCoerce(value) switch
            {
                TirTempRef t => plan.OfTemp(t.Name) == ArrayStorage.Heap,
                TirVariable { Def: { } other } => plan.Of(other) == ArrayStorage.Heap,
                _ => false,
            };
            if (!aliased)
                continue;
            var refusal = Refusal(ownership.Of(def)) ?? AliasRefusal(ownership.Of(def));
            if (refusal == null)
                plan.Assign(def, ArrayStorage.Heap);
            else
                plan.Refuse(def.Name, refusal);
        }
        if (stack != null)
            AssignStack(body, owner, plan, ownership, stack);
        var resolved = new Converter(plan, identity).Walk(body, false);
        return new TirStorageScope(resolved, plan);
    }

    // --- Stack (--stack-arrays) -----------------------------------------------------------------

    /// <summary>The loop kinds that fill a fresh <c>T[n]</c> in one pass and whose count <see cref="CountBounds"/> can state.</summary>
    private static readonly HashSet<string> StackKinds = new() { "MapRange", "Map", "MapIdx", "Reverse" };

    /// <summary>Reasons a slot is refused, as they appear on <see cref="StoragePlan.Refusals"/> (after <see cref="Prefix"/>)
    /// and in the writer's counts. No number appears in one, so the counts group.</summary>
    public static class StackRefusal
    {
        public const string Prefix = "stack: ";
        public const string Kind = "loop kind not stackable";
        public const string CountUnknown = "count unknown";
        public const string CountEmpty = "constant count is zero or less";
        public const string CountTooLarge = "constant count above the slot capacity";
        public const string ElementUnknown = "element type unknown";
        public const string Managed = "element not unmanaged";
        public const string Captured = "captured by a closure";
        public const string Escapes = "used where a span cannot go";
        public const string Lifetime = "used outside the pass that allocates it";
        public const string Frames = "under a loop the plan cannot name";
        public const string Budget = "body slot budget exceeded";
        public const string Cycle = "body on a call cycle";
    }

    /// <summary>
    /// Upgrades a Heap lowered-loop temporary to <see cref="ArrayStorage.Stack"/>, with one <see cref="StackSlot"/> per
    /// allocation site, when: its loop kind fills a fresh array of a count <see cref="CountBounds"/> states (constant, or
    /// an expression the printer's run-time guard tests against the capacity: past it the array is a heap array);
    /// its element is unmanaged and the capacity <see cref="StorageChecker.SlotCapacityLimit"/> holds the constant count;
    /// every use only observes it (<c>Count</c>, <c>At</c>, a lowered loop's source) or binds it to a single-declaration
    /// local of which the same holds; no closure names it; every use sits in the loop pass that allocated it; every
    /// loop around the allocation is one the plan names (the slot is declared before the outermost); the body's slots
    /// total at most 1 KiB; and the body is not on a call cycle. A local that aliases a Stack temporary is Stack.
    /// </summary>
    private static void AssignStack(TirNode body, FunctionDef owner, StoragePlan plan, TirOwnership ownership, IStorageTarget target)
    {
        var loops = body.Descendants().OfType<TirLoweredLoop>()
            .Where(l => TirLoopLowerer.ProducesArray(l.Kind) && plan.OfTemp(l.TempName) == ArrayStorage.Heap).ToList();
        if (loops.Count == 0)
            return;
        var declarations = body.Descendants().Select(Declaration).Where(d => d.Def != null).ToList();
        var frames = StorageChecker.AllocationFrames(body, target);
        bool? cycle = null;
        var bytes = 0;
        foreach (var loop in loops)
        {
            var reason = StackRefusalOf(loop, plan, ownership, declarations, frames, target, body, owner, ref cycle, bytes,
                out var element, out var capacity, out var aliases);
            if (reason != null)
            {
                plan.Refuse(loop.TempName, StackRefusal.Prefix + reason);
                continue;
            }
            var around = frames[loop.TempName];
            plan.AssignTemp(loop.TempName, ArrayStorage.Stack);
            foreach (var alias in aliases)
                plan.Assign(alias, ArrayStorage.Stack);
            plan.AddSlot(new StackSlot($"_slot{loop.Id}", element, capacity, loop.TempName, around.Length == 0 ? null : around[0]));
            bytes += capacity * target.SizeOf(element);
        }
    }

    private static string StackRefusalOf(TirLoweredLoop loop, StoragePlan plan, TirOwnership ownership,
        List<(VariableDef Def, TirNode Value)> declarations, IReadOnlyDictionary<string, int?[]> frames, IStorageTarget target,
        TirNode body, FunctionDef owner, ref bool? cycle, int bytes, out TypeExpression element, out int capacity,
        out List<VariableDef> aliases)
    {
        (capacity, aliases) = (0, new List<VariableDef>());
        element = loop.ElemType ?? (loop.Type != null && loop.Type.TypeArgs.Count == 1 ? loop.Type.TypeArgs[0] : null);
        if (!StackKinds.Contains(loop.Kind))
            return StackRefusal.Kind;
        var bound = CountBounds.OfLoop(loop, body);
        if (bound.IsUnknown)
            return StackRefusal.CountUnknown;
        if (element == null)
            return StackRefusal.ElementUnknown;
        var size = target.SizeOf(element);
        if (!target.IsUnmanaged(element) || size <= 0)
            return StackRefusal.Managed;
        var limit = StorageChecker.SlotCapacityLimit(size);
        if (bound.IsConstant)
        {
            if (bound.Constant <= 0)
                return StackRefusal.CountEmpty;
            if (bound.Constant > limit)
                return StackRefusal.CountTooLarge;
        }
        capacity = bound.IsConstant ? bound.Constant.Value : limit;
        if (!frames.TryGetValue(loop.TempName, out var around) || around.Contains(null))
            return StackRefusal.Frames;
        var facts = ownership.OfTemp(loop.TempName);
        if (facts == null || facts.Captured)
            return StackRefusal.Captured;
        var uses = StackUses(facts, null, loop.TempName, plan, ownership, declarations, aliases);
        if (uses != null)
            return uses;
        if (bytes + capacity * size > StorageChecker.SlotBudgetBytes)
            return StackRefusal.Budget;
        cycle ??= HasSelfCall(body, owner) || target.OnCallCycle(owner, body);
        return cycle.Value ? StackRefusal.Cycle : null;
    }

    // Walks the binding and, recursively, the locals that alias it: each use must be a borrow in the pass that declared
    // the binding, and every other use must be the declaration of such a local. Returns the reason, or null.
    private static string StackUses(BindingFacts facts, VariableDef self, string tempName, StoragePlan plan, TirOwnership ownership,
        List<(VariableDef Def, TirNode Value)> declarations, List<VariableDef> aliases)
    {
        var declared = facts.DeclaredLoops;
        foreach (var use in facts.Uses)
            if (use.Loops.Length < declared.Length || !declared.SequenceEqual(use.Loops.Take(declared.Length)))
                return StackRefusal.Lifetime;
        var children = declarations.Where(d => References(d.Value, self, tempName)).Select(d => d.Def).ToList();
        if (facts.Uses.Count(u => u.Kind != TirUpdateFlow.UseKind.Borrow) != children.Count)
            return StackRefusal.Escapes;
        foreach (var child in children)
        {
            var childFacts = ownership.Of(child);
            if (plan.Of(child) != ArrayStorage.Heap || childFacts == null)
                return StackRefusal.Escapes;
            if (childFacts.Captured)
                return StackRefusal.Captured;
            if (childFacts.DeclaredLoops.Length < declared.Length || !declared.SequenceEqual(childFacts.DeclaredLoops.Take(declared.Length)))
                return StackRefusal.Lifetime;
            aliases.Add(child);
            var below = StackUses(childFacts, child, null, plan, ownership, declarations, aliases);
            if (below != null)
                return below;
        }
        return null;
    }

    private static bool References(TirNode value, VariableDef def, string tempName)
        => TirRewrite.StripCoerce(value) switch
        {
            TirTempRef t => tempName != null && t.Name == tempName,
            TirVariable { Def: { } v } => def != null && ReferenceEquals(v, def),
            _ => false,
        };

    // A direct call of the function itself is visible in its own body whatever the call graph knows. The callee is the
    // declared function and the owner may be the monomorphizer's copy of it, so the two are matched by owner type, name and arity.
    private static bool HasSelfCall(TirNode body, FunctionDef owner)
        => owner == null || body.Descendants().OfType<TirCall>().Any(c => c.Callee != null && c.Callee.Name == owner.Name
            && c.Callee.NumParameters == owner.NumParameters && c.Callee.OwnerType?.Name == owner.OwnerType?.Name);

    // The binding a node declares with the value it is initialised to, or (null, null) for any other node.
    private static (VariableDef Def, TirNode Value) Declaration(TirNode n) => n switch
    {
        TirInlineBinding b => (b.Def, b.Value),
        TirLet l => (l.Def, l.Value),
        _ => (null, null),
    };

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
                case TirInlineBinding or TirLet when _plan.Of(Declaration(n).Def) == ArrayStorage.Heap:
                    // The local holds the very array: its value is taken as it is.
                    return With(n, new[] { Walk(Declaration(n).Value, true) });
                case TirMultiReduction pair:
                    // The one source is read by the loop the printer writes. The source reference each reduction call
                    // carries is never printed; it is converted like any other and costs nothing.
                    return With(n, pair.Children.Select((child, i) => Walk(child, i == 0)).ToList());
                case TirCoerce c when TirRewrite.IsListType(c.Type) && TirRewrite.IsListType(TirRewrite.StripCoerce(c.Inner)?.Type):
                    return With(n, new[] { Walk(c.Inner, observed) });
                case TirCall call when call.Args.Count > 0 && _identity.IsObserver(call):
                    return With(n, call.Children.Select((child, i) => Walk(child, i == 0)).ToList());
                case TirLoweredLoop loop:
                {
                    // Every source is read by the loop the printer writes, its exact-output form included.
                    return With(n, loop.Children.Select((child, i) => Walk(child, i < loop.Sources.Count)).ToList());
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
