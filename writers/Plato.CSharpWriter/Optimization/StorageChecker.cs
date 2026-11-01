// Verifies a storage plan against its body before anything is printed (--owned-arrays).
using System;
using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Symbols;

namespace Ara3D.Geometry.CSharpWriter;

/// <summary>What consumes an array value; the target's table says which storages each accepts.</summary>
public enum ConsumerKind
{
    /// <summary>The receiver of <c>Count</c> or <c>At</c>.</summary>
    Observe,
    /// <summary>The source a lowered loop reads.</summary>
    LoopSource,
    /// <summary>A variable declaration or assignment: the storage must equal the binding's, which the checker compares itself.</summary>
    Binding,
    /// <summary>An argument a span twin takes as a <c>ReadOnlySpan&lt;T&gt;</c>.</summary>
    TwinBorrowedArg,
    /// <summary>Everything else: returns, constructor and array-literal elements, other calls, in-place updates, lambda results.</summary>
    Other,
}

/// <summary>What one output language lets an array of each storage do. C# is <see cref="CSharpStorageTarget"/>.</summary>
public interface IStorageTarget
{
    bool Accepts(ArrayStorage value, ConsumerKind consumer);
    bool MayConvert(ArrayStorage from, ArrayStorage to);
    /// <summary>A call that reads its array receiver and keeps no reference to it (<c>Count</c>, <c>At</c>).</summary>
    bool IsObserver(TirCall call);
    /// <summary>Whether the element type contains no reference, so a slot of it may be <c>stackalloc</c>ed.</summary>
    bool IsUnmanaged(TypeExpression element);
    /// <summary>The element's size in bytes (the writer's estimate); 0 or less when unknown.</summary>
    int SizeOf(TypeExpression element);
    /// <summary>Whether the function is on a cycle of the call graph (mutual recursion included).</summary>
    bool OnCallCycle(FunctionDef function);
}

/// <summary>The C# consumer table and conversion table of the plan, free of any compilation.</summary>
public static class CSharpStorageRules
{
    /// <summary>Observe and LoopSource accept any storage; TwinBorrowedArg accepts Borrowed exactly (generic inference
    /// does not see through Span to ReadOnlySpan); every other consumer accepts List only. Binding is compared with
    /// the binding's own storage by the checker, so the table answers true.</summary>
    public static bool Accepts(ArrayStorage value, ConsumerKind consumer) => consumer switch
    {
        ConsumerKind.Observe or ConsumerKind.LoopSource or ConsumerKind.Binding => true,
        ConsumerKind.TwinBorrowedArg => value == ArrayStorage.Borrowed,
        _ => value == ArrayStorage.List,
    };

    /// <summary>Heap to List, Heap to Borrowed, Stack to Borrowed. Stack or Borrowed to List would be a hidden copy.</summary>
    public static bool MayConvert(ArrayStorage from, ArrayStorage to)
        => (from, to) is (ArrayStorage.Heap, ArrayStorage.List) or (ArrayStorage.Heap, ArrayStorage.Borrowed)
            or (ArrayStorage.Stack, ArrayStorage.Borrowed);
}

/// <summary>The C# target, answering the type questions from the writer's compilation.</summary>
public sealed class CSharpStorageTarget : IStorageTarget
{
    private readonly StructLayouts _layouts;
    private readonly TirArrayIdentity _identity;

    public CSharpStorageTarget(CSharpWriter writer)
    {
        _layouts = new StructLayouts(writer);
        _identity = new TirArrayIdentity(writer.Compilation);
    }

    public bool Accepts(ArrayStorage value, ConsumerKind consumer) => CSharpStorageRules.Accepts(value, consumer);
    public bool MayConvert(ArrayStorage from, ArrayStorage to) => CSharpStorageRules.MayConvert(from, to);
    public bool IsObserver(TirCall call) => _identity.IsObserver(call);
    public bool IsUnmanaged(TypeExpression element) => _layouts.IsUnmanaged(element);
    public int SizeOf(TypeExpression element) => _layouts.SizeOf(element);

    /// <summary>Answers yes (refuse) for every function: no call graph exists yet, and no chunk before the stack
    /// storage of C6 puts a slot in a body, so this costs nothing until then.</summary>
    public bool OnCallCycle(FunctionDef function) => true;
}

/// <summary>
/// Re-verifies a body that carries a <see cref="StoragePlan"/>, without trusting the resolver. Rules S1 to S4 and S6
/// repeat errors Roslyn reports anyway; S5 and S7 catch what Roslyn accepts: a slot that costs the stack, and a span
/// that holds a reused slot across loop passes.
/// </summary>
public static class StorageChecker
{
    public enum Rule
    {
        /// <summary>A consumer receives a storage it does not accept.</summary>
        S1_Mismatch,
        /// <summary>A <see cref="TirRepresent"/> outside the conversion table.</summary>
        S2_Conversion,
        /// <summary>A Stack or Borrowed binding referenced under a lambda that prints as a C# lambda.</summary>
        S3_SpanInLambda,
        /// <summary>A Stack or Borrowed value returned, stored in a constructed value or array, or given to another call.</summary>
        S4_SpanEscapes,
        /// <summary>A stack slot declared in the wrong place, of a managed or oversized element, past the body's budget, or in a body on a call cycle.</summary>
        S5_Slot,
        /// <summary>A twin call passes a borrowed argument that is not Borrowed.</summary>
        S6_TwinArg,
        /// <summary>A binding holding a Stack value outlives the loop pass that allocated it.</summary>
        S7_Lifetime,
        /// <summary>A node the C# printer has no case for (it would print as <c>/*?*/</c>).</summary>
        S8_Unprintable,
    }

    /// <summary>Capacity limit of one slot: <c>min(16, 256 / sizeof(T))</c> elements.</summary>
    public static int SlotCapacityLimit(int elementBytes)
        => elementBytes <= 0 ? 0 : Math.Min(16, 256 / elementBytes);

    /// <summary>The slots of a body together may hold at most 1 KiB.</summary>
    public const int SlotBudgetBytes = 1024;

    public static IReadOnlyList<(Rule Rule, TirNode Node)> Check(TirStorageScope scope, IStorageTarget target)
    {
        var walk = new Walk(scope, target);
        walk.Run();
        return walk.Violations;
    }

    private sealed class Allocation
    {
        public TirLoweredLoop Node;
        public int?[] Frames;   // the loops around it, outermost first; null is a loop with no id
    }

    private sealed class Walk
    {
        private readonly TirStorageScope _scope;
        private readonly StoragePlan _plan;
        private readonly IStorageTarget _target;
        private readonly List<int?> _loops = new();
        private readonly Stack<HashSet<object>> _lambdaDeclared = new();
        private readonly Dictionary<string, Allocation> _allocations = new(StringComparer.Ordinal);
        private readonly Dictionary<VariableDef, HashSet<string>> _variableAllocations = new();
        private readonly Dictionary<VariableDef, int?[]> _declaredLoops = new();

        public List<(Rule Rule, TirNode Node)> Violations { get; } = new();

        public Walk(TirStorageScope scope, IStorageTarget target)
            => (_scope, _plan, _target) = (scope, scope.Plan, target);

        private void Report(Rule rule, TirNode node) => Violations.Add((rule, node));

        public void Run()
        {
            Operand(_scope.Body, ConsumerKind.Other);
            CheckSlots();
        }

        // --- the walk ------------------------------------------------------------------------------

        /// <summary>The storage of the value <paramref name="n"/> produces, with each child checked against its consumer.</summary>
        private ArrayStorage Visit(TirNode n)
        {
            switch (n)
            {
                case null:
                    return ArrayStorage.List;

                case TirVariable v:
                    return Reference(n, _plan.Of(v.Def), v.Def);
                case TirParameter p:
                    return Reference(n, _plan.OfParameter(p.Def), p.Def);
                case TirTempRef t:
                    return Reference(n, _plan.OfTemp(t.Name), t.Name);

                case TirStorageScope nested:
                    return Visit(nested.Body);

                case TirRepresent r:
                {
                    var inner = Visit(r.Inner);
                    if (inner != r.From)
                        Report(Rule.S1_Mismatch, r);
                    if (!_target.MayConvert(r.From, r.To))
                        Report(Rule.S2_Conversion, r);
                    return r.To;
                }

                case TirCoerce c:
                    // A conversion between two array types changes nothing in the C# value.
                    if (TirRewrite.IsListType(c.Type) && TirRewrite.IsListType(TirRewrite.StripCoerce(c.Inner)?.Type))
                        return Visit(c.Inner);
                    Operand(c.Inner, ConsumerKind.Other);
                    return ArrayStorage.List;

                case TirLet let:
                    Bind(let.Def, let.Value, let, declares: true);
                    return ArrayStorage.List;
                case TirInlineBinding binding:
                    Bind(binding.Def, binding.Value, binding, declares: true);
                    return ArrayStorage.List;
                case TirInlineExit exit:
                    Bind(exit.Result, exit.Value, exit, declares: false);
                    return ArrayStorage.List;
                case TirAssign assign when TirRewrite.StripCoerce(assign.LValue) is TirVariable { Def: { } target }:
                    Bind(target, assign.RValue, assign, declares: false);
                    return ArrayStorage.List;

                case TirInlineRegion region:
                    if (region.Result != null)
                        _declaredLoops[region.Result] = _loops.ToArray();
                    Visit(region.Body);
                    return ArrayStorage.List;

                case TirConditional cond:
                {
                    Visit(cond.Condition);
                    var whenTrue = Visit(cond.IfTrue);
                    var whenFalse = Visit(cond.IfFalse);
                    if (whenTrue == whenFalse)
                        return whenTrue;
                    // Arms of differing storage must each be a List.
                    Consume(cond.IfTrue, whenTrue, ConsumerKind.Other);
                    Consume(cond.IfFalse, whenFalse, ConsumerKind.Other);
                    return ArrayStorage.List;
                }

                case TirCall call when call.Args.Count > 0 && _target.IsObserver(call):
                    Operand(call.Args[0], ConsumerKind.Observe);
                    foreach (var arg in call.Args.Skip(1))
                        Operand(arg, ConsumerKind.Other);
                    return ArrayStorage.List;

                case TirTwinCall twin:
                    for (var i = 0; i < twin.Call.Args.Count; i++)
                    {
                        var arg = twin.Call.Args[i];
                        if (i < twin.Borrowed.Count && twin.Borrowed[i])
                        {
                            if (!_target.Accepts(Visit(arg), ConsumerKind.TwinBorrowedArg))
                                Report(Rule.S6_TwinArg, twin);
                        }
                        else
                            Operand(arg, ConsumerKind.Other);
                    }
                    return ArrayStorage.List;

                case TirLoweredLoop loop:
                    VisitLoweredLoop(loop);
                    return ArrayStorage.List;

                case TirLambda lambda:
                    VisitPrintedLambda(lambda);
                    return ArrayStorage.List;

                case TirMultiReduction pair:
                    // The one source is read before the loop starts, by the loop the printer writes.
                    Operand(pair.Source, ConsumerKind.LoopSource);
                    _loops.Add(null);
                    foreach (var child in pair.Children.Skip(1))
                        Operand(child, ConsumerKind.Other);
                    _loops.RemoveAt(_loops.Count - 1);
                    return ArrayStorage.List;

                case TirLoop or TirGuardedLoop:
                    _loops.Add(null);
                    foreach (var child in n.Children)
                        Operand(child, ConsumerKind.Other);
                    _loops.RemoveAt(_loops.Count - 1);
                    return ArrayStorage.List;

                default:
                    if (!TirCSharpBodyWriter.PrintsAsNode(n))
                        Report(Rule.S8_Unprintable, n);
                    foreach (var child in n.Children)
                        Operand(child, ConsumerKind.Other);
                    return ArrayStorage.List;
            }
        }

        private ArrayStorage Operand(TirNode child, ConsumerKind kind)
        {
            var storage = Visit(child);
            Consume(child, storage, kind);
            return storage;
        }

        private void Consume(TirNode at, ArrayStorage storage, ConsumerKind kind)
        {
            if (_target.Accepts(storage, kind))
                return;
            var escapes = kind == ConsumerKind.Other && storage is ArrayStorage.Stack or ArrayStorage.Borrowed;
            Report(escapes ? Rule.S4_SpanEscapes : Rule.S1_Mismatch, at);
        }

        private ArrayStorage Reference(TirNode at, ArrayStorage storage, object key)
        {
            if (storage is not (ArrayStorage.Stack or ArrayStorage.Borrowed))
                return storage;
            if (_lambdaDeclared.Count > 0 && key != null && !_lambdaDeclared.Peek().Contains(key))
                Report(Rule.S3_SpanInLambda, at);
            if (!InsideAllocationLoops(AllocationsOf(key)))
                Report(Rule.S7_Lifetime, at);
            return storage;
        }

        // A declaration or assignment: the value must already have the binding's storage.
        private void Bind(VariableDef def, TirNode value, TirNode at, bool declares)
        {
            var storage = Visit(value);
            if (storage != _plan.Of(def))
                Report(Rule.S1_Mismatch, at);
            if (def == null)
                return;
            var temps = StackTemps(value).ToList();
            if (declares)
                _declaredLoops[def] = _loops.ToArray();
            if (temps.Count == 0)
                return;
            // The binding must be declared, and the value used, inside the loop pass that allocated it.
            var declared = _declaredLoops.TryGetValue(def, out var d) ? d : null;
            foreach (var temp in temps)
                if (_allocations.TryGetValue(temp, out var allocation) && InnermostLoop(allocation) is { } loop
                    && (!_loops.Contains(loop) || declared != null && !declared.Contains(loop)))
                    Report(Rule.S7_Lifetime, at);
            if (!_variableAllocations.TryGetValue(def, out var set))
                _variableAllocations[def] = set = new HashSet<string>(StringComparer.Ordinal);
            set.UnionWith(temps);
        }

        // The temporaries of lowered loops whose Stack arrays a value may be.
        private IEnumerable<string> StackTemps(TirNode value)
        {
            switch (TirRewrite.StripCoerce(value))
            {
                case TirTempRef t when _plan.OfTemp(t.Name) == ArrayStorage.Stack:
                    return new[] { t.Name };
                case TirVariable { Def: { } def } when _plan.Of(def) == ArrayStorage.Stack:
                    return _variableAllocations.TryGetValue(def, out var set) ? set : Enumerable.Empty<string>();
                case TirRepresent r:
                    return StackTemps(r.Inner);
                case TirConditional c:
                    return StackTemps(c.IfTrue).Concat(StackTemps(c.IfFalse));
                default:
                    return Enumerable.Empty<string>();
            }
        }

        private IEnumerable<string> AllocationsOf(object key) => key switch
        {
            string temp => new[] { temp },
            VariableDef def when _variableAllocations.TryGetValue(def, out var set) => set,
            _ => Enumerable.Empty<string>(),
        };

        private static int? InnermostLoop(Allocation a) => a.Frames.Length == 0 ? null : a.Frames[^1];

        private bool InsideAllocationLoops(IEnumerable<string> temps)
            => temps.All(t => !_allocations.TryGetValue(t, out var a) || InnermostLoop(a) is not { } loop || _loops.Contains(loop));

        private void VisitLoweredLoop(TirLoweredLoop loop)
        {
            foreach (var source in loop.Sources)
                Operand(source, ConsumerKind.LoopSource);
            if (loop.Seed != null)
                Operand(loop.Seed, ConsumerKind.Other);
            if (loop.IncludeFirst != null)
                Operand(loop.IncludeFirst, ConsumerKind.Other);
            foreach (var hoisted in loop.Hoisted)
                Visit(hoisted);
            if (TirLoopLowerer.ProducesArray(loop.Kind))
                _allocations[loop.TempName] = new Allocation { Node = loop, Frames = _loops.ToArray() };
            // The callbacks are written into the loop body: code in a loop, not a closure.
            _loops.Add(loop.Id);
            foreach (var callback in new[] { loop.Fn }.Concat(loop.SourceStages).Where(f => f != null))
            {
                if (TirRewrite.StripCoerce(callback) is TirLambda lambda)
                    Operand(lambda.Body, ConsumerKind.Other);
                else
                    Operand(callback, ConsumerKind.Other);
            }
            _loops.RemoveAt(_loops.Count - 1);
        }

        private void VisitPrintedLambda(TirLambda lambda)
        {
            var declared = new HashSet<object>(lambda.Parameters);
            foreach (var d in lambda.Body.Descendants())
                switch (d)
                {
                    case TirLet let: declared.Add(let.Def); break;
                    case TirInlineBinding b: declared.Add(b.Def); break;
                    case TirInlineRegion r: declared.Add(r.Result); break;
                    case TirLambda inner: declared.UnionWith(inner.Parameters); break;
                    case TirLoweredLoop l when TirLoopLowerer.ProducesArray(l.Kind): declared.Add(l.TempName); break;
                }
            _lambdaDeclared.Push(declared);
            _loops.Add(null);
            Operand(lambda.Body, ConsumerKind.Other);
            _loops.RemoveAt(_loops.Count - 1);
            _lambdaDeclared.Pop();
        }

        // --- S5 ------------------------------------------------------------------------------------

        private void CheckSlots()
        {
            var bytes = 0;
            foreach (var slot in _plan.Slots)
            {
                if (slot.Temp == null || !_allocations.TryGetValue(slot.Temp, out var allocation))
                {
                    Report(Rule.S5_Slot, _scope);
                    continue;
                }
                var misplaced = allocation.Frames.Contains(null)    // under a loop the plan cannot name
                    || slot.Loop != (allocation.Frames.Length == 0 ? null : allocation.Frames[0]);
                var size = _target.SizeOf(slot.Element);
                var bad = misplaced
                    || _plan.OfTemp(slot.Temp) != ArrayStorage.Stack
                    || !_target.IsUnmanaged(slot.Element)
                    || slot.Capacity < 1 || slot.Capacity > SlotCapacityLimit(size);
                if (bad)
                    Report(Rule.S5_Slot, allocation.Node);
                bytes += slot.Capacity * Math.Max(size, 0);
            }
            if (bytes > SlotBudgetBytes)
                Report(Rule.S5_Slot, _scope);
            if (_plan.Slots.Count > 0 && _target.OnCallCycle(_plan.Owner))
                Report(Rule.S5_Slot, _scope);
            // A Stack temporary needs its slot.
            foreach (var (name, allocation) in _allocations)
                if (_plan.OfTemp(name) == ArrayStorage.Stack && _plan.Slots.All(s => s.Temp != name))
                    Report(Rule.S5_Slot, allocation.Node);
        }
    }
}
