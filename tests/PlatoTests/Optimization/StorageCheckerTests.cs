// Checks the storage layer of --owned-arrays: the pre-print checker's rules on hand-built trees, and the pass wiring.
using Ara3D.Geometry.AST;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Symbols;
using Ara3D.Geometry.CSharpWriter;
using static Ara3D.Geometry.CSharpWriter.StorageChecker;

namespace PlatoTests.Optimization;

/// <summary>
/// TKT-1152 C2a. The checker is tested on small trees built by hand, one rule at a time, each with a tree that
/// breaks only that rule and a tree that keeps it. The wiring is tested on the OwnershipProbe fixture: with the
/// resolver assigning List everywhere the printed C# is byte-identical with the flag on and off, and the resolver
/// wraps the capture-rewritten body without copying it.
/// </summary>
[TestFixture]
public static class StorageCheckerTests
{
    // --- builders ------------------------------------------------------------------------------

    private static TypeExpression T(string name) => new(new TypeDef(null, TypeKind.ConcreteType, name));
    private static readonly TypeExpression Num = T("Number");
    private static readonly TypeExpression Arr = new(new TypeDef(null, TypeKind.ConcreteType, "Array"), Num);

    private static VariableDef Var(string name) => new(null, name, Arr, null);
    private static TirVariable Ref(VariableDef def) => new(def, Arr, null);
    private static TirLet Let(VariableDef def, TirNode value) => new(def, value, Arr, null);
    private static TirTempRef Temp(string name) => new(name, Arr, null);
    private static TirReturn Return(TirNode value) => new(value, null);
    private static TirBlock Block(params TirNode[] statements) => new(statements, null);
    private static TirNode Source() => new TirDefault(Arr, null);

    private static TirCall Count(TirNode array)
        => new(null, EmissionKind.Intrinsic, null, Num, new[] { array }, Num, null, "Count");

    private static TirLoweredLoop Loop(int id, string temp, TirNode fn = null)
        => new("MapRange", new[] { Source() }, fn, null, null, temp, id, Arr, null) { ElemType = Num };

    private static TirLambda Lambda(TirNode body, params ParameterDef[] parameters) => new(parameters, body, Arr, null);

    private static TirStorageScope Scope(StoragePlan plan, params TirNode[] statements)
        => new(Block(statements), plan);

    private static StackSlot Slot(string temp, int capacity = 8, int? loop = null, string element = "Number")
        => new("slot_" + temp, T(element), capacity, temp, loop);

    private sealed class Target : IStorageTarget
    {
        public bool Cycle;
        public bool Accepts(ArrayStorage value, ConsumerKind consumer) => CSharpStorageRules.Accepts(value, consumer);
        public bool MayConvert(ArrayStorage from, ArrayStorage to) => CSharpStorageRules.MayConvert(from, to);
        public bool IsObserver(TirCall call) => call.Name is "Count" or "At";
        public bool IsUnmanaged(TypeExpression element) => element.Name != "Managed";
        public int SizeOf(TypeExpression element) => element.Name == "Big" ? 32 : 8;
        public bool OnCallCycle(FunctionDef function) => Cycle;
    }

    private static List<Rule> Rules(TirStorageScope scope, bool cycle = false)
        => StorageChecker.Check(scope, new Target { Cycle = cycle }).Select(v => v.Rule).ToList();

    // --- the consumer and conversion tables ----------------------------------------------------

    [Test]
    public static void ConsumerTable()
    {
        foreach (var storage in Enum.GetValues<ArrayStorage>())
        {
            Assert.That(CSharpStorageRules.Accepts(storage, ConsumerKind.Observe), Is.True);
            Assert.That(CSharpStorageRules.Accepts(storage, ConsumerKind.LoopSource), Is.True);
            Assert.That(CSharpStorageRules.Accepts(storage, ConsumerKind.TwinBorrowedArg), Is.EqualTo(storage == ArrayStorage.Borrowed));
            Assert.That(CSharpStorageRules.Accepts(storage, ConsumerKind.Other), Is.EqualTo(storage == ArrayStorage.List));
        }
    }

    [Test]
    public static void ConversionTable()
    {
        var allowed = new[]
        {
            (ArrayStorage.Heap, ArrayStorage.List), (ArrayStorage.Heap, ArrayStorage.Borrowed), (ArrayStorage.Stack, ArrayStorage.Borrowed),
        };
        foreach (var from in Enum.GetValues<ArrayStorage>())
            foreach (var to in Enum.GetValues<ArrayStorage>())
                Assert.That(CSharpStorageRules.MayConvert(from, to), Is.EqualTo(allowed.Contains((from, to))), $"{from} -> {to}");
    }

    // --- the rules -----------------------------------------------------------------------------

    [Test]
    public static void WellFormedTreePasses()
    {
        var a = Var("a");
        var plan = new StoragePlan();
        plan.AssignTemp("_lp1", ArrayStorage.Heap);
        plan.Assign(a, ArrayStorage.Heap);
        var scope = Scope(plan,
            Loop(1, "_lp1"),
            Let(a, Temp("_lp1")),
            Let(Var("n"), Count(Ref(a))),
            Loop(2, "_lp2", Lambda(Count(Ref(a)))),   // read in a loop callback and as a loop source below
            new TirLoweredLoop("Map", new TirNode[] { Ref(a) }, null, null, null, "_lp3", 3, Arr, null),
            Return(new TirRepresent(Ref(a), ArrayStorage.Heap, ArrayStorage.List)));
        Assert.That(Rules(scope), Is.Empty);
    }

    [Test]
    public static void S1_ConsumerReceivesAStorageItDoesNotAccept()
    {
        var a = new ParameterDef(null, "a", Arr, 0);
        var plan = new StoragePlan();
        plan.AssignParameter(a, ArrayStorage.Heap);
        Assert.That(Rules(Scope(plan, Return(new TirParameter(a, Arr, null)))), Does.Contain(Rule.S1_Mismatch), "a Heap array returned bare");

        // A binding takes its own storage exactly.
        var list = Var("list");
        Assert.That(Rules(Scope(plan, Let(list, new TirParameter(a, Arr, null)))), Is.EqualTo(new[] { Rule.S1_Mismatch }), "Heap into a List binding");

        // Arms of differing storage must each be a List.
        var arms = new TirConditional(new TirDefault(Num, null), new TirParameter(a, Arr, null), Source(), Arr, null);
        Assert.That(Rules(Scope(plan, Return(arms))), Is.EqualTo(new[] { Rule.S1_Mismatch }));
    }

    [Test]
    public static void S2_ConversionOutsideTheTable()
    {
        var heap = new ParameterDef(null, "heap", Arr, 0);
        var plan = new StoragePlan();
        plan.AssignParameter(heap, ArrayStorage.Heap);
        var parameter = new ParameterDef(null, "p", Arr, 0);
        plan.AssignParameter(parameter, ArrayStorage.Borrowed);

        var spanToList = new TirRepresent(new TirParameter(parameter, Arr, null), ArrayStorage.Borrowed, ArrayStorage.List);
        Assert.That(Rules(Scope(plan, Return(spanToList))), Is.EqualTo(new[] { Rule.S2_Conversion }), "a hidden copy");

        var heapToStack = new TirRepresent(new TirParameter(heap, Arr, null), ArrayStorage.Heap, ArrayStorage.Stack);
        Assert.That(Rules(Scope(plan, Let(Var("s"), heapToStack))).Contains(Rule.S2_Conversion), Is.True);

        // A Represent that misstates what it converts is a mismatch.
        var wrongFrom = new TirRepresent(new TirParameter(heap, Arr, null), ArrayStorage.Stack, ArrayStorage.Borrowed);
        Assert.That(Rules(Scope(plan, Return(wrongFrom))), Does.Contain(Rule.S1_Mismatch));
    }

    [Test]
    public static void S3_SpanUnderAPrintedLambda()
    {
        var parameter = new ParameterDef(null, "p", Arr, 0);
        var plan = new StoragePlan();
        plan.AssignParameter(parameter, ArrayStorage.Borrowed);
        TirNode Read() => Count(new TirParameter(parameter, Arr, null));

        Assert.That(Rules(Scope(plan, Return(Lambda(Read())))), Is.EqualTo(new[] { Rule.S3_SpanInLambda }), "a closure over a span");
        // A lambda literal given to a lowered loop is written into the loop body, not a closure.
        Assert.That(Rules(Scope(plan, Loop(1, "_lp1", Lambda(Read())))), Is.Empty);
        // A span bound inside the lambda is the lambda's own.
        var inner = Var("inner");
        var innerPlan = new StoragePlan();
        innerPlan.Assign(inner, ArrayStorage.Borrowed);
        var own = Lambda(Block(Let(inner, new TirRepresent(Ref(Var("h")), ArrayStorage.Heap, ArrayStorage.Borrowed)), Return(Count(Ref(inner)))));
        Assert.That(Rules(Scope(innerPlan, Return(own))).Contains(Rule.S3_SpanInLambda), Is.False);
    }

    [Test]
    public static void S4_SpanEscapes()
    {
        var parameter = new ParameterDef(null, "p", Arr, 0);
        var plan = new StoragePlan();
        plan.AssignParameter(parameter, ArrayStorage.Borrowed);
        TirNode Span() => new TirParameter(parameter, Arr, null);

        Assert.That(Rules(Scope(plan, Return(Span()))), Is.EqualTo(new[] { Rule.S4_SpanEscapes }), "returned");
        Assert.That(Rules(Scope(plan, Return(new TirNew(Arr, new[] { Span() }, Arr, null)))), Is.EqualTo(new[] { Rule.S4_SpanEscapes }), "stored in a constructed value");
        Assert.That(Rules(Scope(plan, Return(new TirArray(new[] { Span() }, Arr, null)))), Is.EqualTo(new[] { Rule.S4_SpanEscapes }), "stored in an array literal");
        Assert.That(Rules(Scope(plan, Return(Count(Span())))), Is.Empty, "observed");
    }

    [Test]
    public static void S5_Slots()
    {
        // One slot, well placed.
        List<Rule> Check(Action<StoragePlan> configure, bool cycle = false, string element = "Number", int capacity = 8)
        {
            var plan = new StoragePlan();
            plan.AssignTemp("_s", ArrayStorage.Stack);
            plan.AddSlot(Slot("_s", capacity, null, element));
            configure(plan);
            return Rules(Scope(plan, Loop(1, "_s"), Return(Count(Temp("_s")))), cycle);
        }

        Assert.That(Check(_ => { }), Is.Empty);
        Assert.That(Check(_ => { }, element: "Managed"), Is.EqualTo(new[] { Rule.S5_Slot }), "a managed element");
        Assert.That(Check(_ => { }, capacity: 17), Is.EqualTo(new[] { Rule.S5_Slot }), "past 16 elements");
        Assert.That(Check(_ => { }, element: "Big", capacity: 8), Is.Empty, "8 elements of 32 bytes is the limit");
        Assert.That(Check(_ => { }, element: "Big", capacity: 9), Is.EqualTo(new[] { Rule.S5_Slot }), "past 256 bytes");
        Assert.That(Check(_ => { }, cycle: true), Is.EqualTo(new[] { Rule.S5_Slot }), "a body on a call cycle");
        Assert.That(Check(_ => { }, capacity: 0), Is.EqualTo(new[] { Rule.S5_Slot }), "no capacity");

        // A Stack temporary with no slot.
        var bare = new StoragePlan();
        bare.AssignTemp("_s", ArrayStorage.Stack);
        Assert.That(Rules(Scope(bare, Loop(1, "_s"), Return(Count(Temp("_s"))))), Is.EqualTo(new[] { Rule.S5_Slot }));

        // Together the slots may hold 1 KiB: five slots of 256 bytes do not.
        var budget = new StoragePlan();
        var statements = new List<TirNode>();
        for (var i = 0; i < 5; i++)
        {
            var temp = "_b" + i;
            budget.AssignTemp(temp, ArrayStorage.Stack);
            budget.AddSlot(Slot(temp, 8, null, "Big"));
            statements.Add(Loop(i + 1, temp));
        }
        Assert.That(Rules(Scope(budget, statements.ToArray())), Is.EqualTo(new[] { Rule.S5_Slot }), "5 x 256 B");
        statements.RemoveAt(4);
        budget = new StoragePlan();
        for (var i = 0; i < 4; i++)
        {
            budget.AssignTemp("_b" + i, ArrayStorage.Stack);
            budget.AddSlot(Slot("_b" + i, 8, null, "Big"));
        }
        Assert.That(Rules(Scope(budget, statements.ToArray())), Is.Empty, "4 x 256 B is exactly the budget");
    }

    [Test]
    public static void S5_SlotIsDeclaredAtTheOutermostLoopAroundTheAllocation()
    {
        // _s is allocated inside the callback of loop 7.
        TirStorageScope Inside(int? slotLoop)
        {
            var plan = new StoragePlan();
            plan.AssignTemp("_s", ArrayStorage.Stack);
            plan.AddSlot(Slot("_s", 8, slotLoop));
            return Scope(plan, Loop(7, "_o", Lambda(Block(Loop(8, "_s"), Return(Count(Temp("_s")))))));
        }
        Assert.That(Rules(Inside(7)), Is.Empty);
        Assert.That(Rules(Inside(null)), Is.EqualTo(new[] { Rule.S5_Slot }), "body scope: zeroed on every call");
        Assert.That(Rules(Inside(8)), Is.EqualTo(new[] { Rule.S5_Slot }), "not the outermost loop");

        // Under a loop the plan cannot name (a while loop), no slot is placeable.
        var plan = new StoragePlan();
        plan.AssignTemp("_s", ArrayStorage.Stack);
        plan.AddSlot(Slot("_s"));
        var inWhile = new TirLoop(new TirDefault(Num, null), Block(Loop(1, "_s")), null);
        Assert.That(Rules(Scope(plan, inWhile)), Is.EqualTo(new[] { Rule.S5_Slot }));
    }

    [Test]
    public static void S6_TwinBorrowedArgument()
    {
        var heap = new ParameterDef(null, "heap", Arr, 0);
        var plan = new StoragePlan();
        plan.AssignParameter(heap, ArrayStorage.Heap);
        var parameter = new ParameterDef(null, "p", Arr, 0);
        plan.AssignParameter(parameter, ArrayStorage.Borrowed);

        TirTwinCall Twin(TirNode arg)
            => new(new TirCall(null, EmissionKind.StaticMethod, null, Num, new[] { arg }, Num, null, "Twin"), "Twins", "Twin", new[] { true });
        TirNode Call(TirNode arg) => Return(Twin(arg));

        Assert.That(Rules(Scope(plan, Call(new TirParameter(heap, Arr, null)))), Is.EqualTo(new[] { Rule.S6_TwinArg }), "Heap is not a span");
        Assert.That(Rules(Scope(plan, Call(new TirRepresent(new TirParameter(heap, Arr, null), ArrayStorage.Heap, ArrayStorage.Borrowed)))), Is.Empty);
        Assert.That(Rules(Scope(plan, Call(new TirParameter(parameter, Arr, null)))), Is.Empty, "a Borrowed parameter passes on");
    }

    [Test]
    public static void S7_BindingOutlivesTheLoopPassThatAllocatedIt()
    {
        // Loop 7's callback allocates _s (loop 8) and binds it. The binding must live inside loop 7.
        TirStorageScope Bound(bool declaredOutside)
        {
            var plan = new StoragePlan();
            var v = Var("v");
            plan.AssignTemp("_s", ArrayStorage.Stack);
            plan.Assign(v, ArrayStorage.Stack);
            plan.AddSlot(Slot("_s", 8, 7));
            var inner = new List<TirNode> { Loop(8, "_s") };
            if (declaredOutside)
                inner.Add(new TirAssign(Ref(v), Temp("_s"), Arr, null));
            else
                inner.Add(Let(v, Temp("_s")));
            inner.Add(Return(Count(Ref(v))));
            var loop = Loop(7, "_o", Lambda(Block(inner.ToArray())));
            // The region declares v ahead of the loop: one result for every pass.
            return declaredOutside
                ? Scope(plan, new TirInlineRegion(v, Block(loop)))
                : Scope(plan, loop);
        }
        Assert.That(Rules(Bound(declaredOutside: false)), Is.Empty);
        Assert.That(Rules(Bound(declaredOutside: true)), Is.EqualTo(new[] { Rule.S7_Lifetime }));

        // A reference after the loop that allocated it.
        var plan2 = new StoragePlan();
        plan2.AssignTemp("_s", ArrayStorage.Stack);
        plan2.AddSlot(Slot("_s", 8, 7));
        var after = Scope(plan2,
            Loop(7, "_o", Lambda(Block(Loop(8, "_s"), Return(Count(Temp("_s")))))),
            Return(Count(Temp("_s"))));
        Assert.That(Rules(after), Is.EqualTo(new[] { Rule.S7_Lifetime }));
    }

    // --- the pass ------------------------------------------------------------------------------

    private static readonly Lazy<Ara3D.Geometry.Compiler.Compilation> Compilation = new(() =>
    {
        var foundation = Path.Combine(CheckerTestSupport.FindForwardStdLib(), "foundation");
        var fixture = Path.Combine(CheckerTestSupport.FindFolder("PlatoTests"), "Optimization", "OwnershipFixtures", "ownership.plato");
        var asts = Directory.GetFiles(foundation, "*.plato").Select(CheckerTestSupport.ParseFile)
            .Append(CheckerTestSupport.ParseFile(fixture));
        return new Ara3D.Geometry.Compiler.Compilation(Ara3D.Logging.Logger.Null, asts.ToList());
    });

    /// <summary>The nineteen pinned flags of the regeneration, plus the storage layer when asked.</summary>
    private static CSharpWriter PinnedWriter(bool ownedArrays)
        => new(Compilation.Value, "unused-storage-checker")
        {
            ExtensionStyle = true, LowerLoops = true, FuseCombinators = true, FuseConsumers = true,
            ExactOutputAllocation = true, LeanInlining = true, ShareCalls = true, LowerScalars = true,
            ConcreteArrays = true, Presize = true, InPlaceUpdates = true, SinkBindings = true, BindScrutinees = true,
            ShareCallsWide = true, ClosureFree = true, InParameters = true, SwitchMatch = true, HoistInvariants = true,
            CacheConstants = true, PrimitiveCompare = true, LowerMore = true, SpecializeCallbacks = true,
            OwnedArrays = ownedArrays, OwnedArraysStrict = ownedArrays,
        };

    [Test]
    public static void FlagOnPrintsTheSameBytesAsFlagOff()
    {
        var off = PinnedWriter(false);
        off.WriteAll();
        var on = PinnedWriter(true);
        on.WriteAll();

        Assert.That(off.StorageBodiesChecked, Is.EqualTo(0), "off: the pass does not run");
        Assert.That(on.StorageBodiesChecked, Is.GreaterThan(0), "on: every body passes the checker");
        Assert.That(on.StorageRefusals, Is.EqualTo(0));
        Assert.That(on.Files.Keys, Is.EquivalentTo(off.Files.Keys));
        foreach (var (name, text) in off.Files)
            Assert.That(on.Files[name].ToString(), Is.EqualTo(text.ToString()), name);
    }

    [Test]
    public static void ResolverWrapsTheCaptureRewrittenBodyWithoutCopyingIt()
    {
        var writer = PinnedWriter(true);
        var target = new CSharpStorageTarget(writer);
        var probe = Compilation.Value.LibraryDefinitionsByName["OwnershipProbe"].Functions;
        Assert.That(probe, Is.Not.Empty);
        foreach (var function in probe)
        {
            var tir = writer.RunOptimizerPasses(writer.TryGetStaticTir(function), null);
            SymbolRewriter.NextId = 0;
            var rewritten = TirLambdaCaptureRewriter.Rewrite(tir.Body);
            var counter = SymbolRewriter.NextId;

            var resolved = writer.ResolveStorage(tir, rewritten);
            Assert.That(SymbolRewriter.NextId, Is.EqualTo(counter), $"{function.Name}: the pass takes no capture-local number");
            var scope = resolved as TirStorageScope;
            Assert.That(scope, Is.Not.Null, function.Name);
            Assert.That(ReferenceEquals(scope.Body, rewritten), Is.True, $"{function.Name}: the body is the node that was rewritten");
            SymbolRewriter.NextId = 0;
            Assert.That(scope.Body.ToString(), Is.EqualTo(TirLambdaCaptureRewriter.Rewrite(tir.Body).ToString()),
                $"{function.Name}: the unwrapped body is the tree one rewrite gives (the counter restarted)");
            Assert.That(StorageChecker.Check(scope, target), Is.Empty, function.Name);
            Assert.That(scope.Plan.Slots, Is.Empty);
            Assert.That(scope.Plan.Owner, Is.SameAs(tir.Original));
        }
    }

    [Test]
    public static void FlagOffLeavesTheBodyAlone()
    {
        var writer = PinnedWriter(false);
        var body = new TirDefault(Num, null);
        Assert.That(writer.ResolveStorage(null, body), Is.SameAs(body));
    }

    [Test]
    public static void ARefusedPlanIsDiscardedAndCounted()
    {
        var heap = Var("heap");
        TirStorageScope Wrong(TirNode body, FunctionDef owner)
        {
            var plan = new StoragePlan(owner);
            plan.Assign(heap, ArrayStorage.Heap);
            return new TirStorageScope(Block(Let(heap, Source()), Return(Ref(heap))), plan);   // a Heap array returned bare
        }
        var body = new TirDefault(Num, null);

        var writer = PinnedWriter(true);
        writer.OwnedArraysStrict = false;
        writer.StorageResolver = Wrong;
        Assert.That(writer.ResolveStorage(null, body), Is.SameAs(body), "the plan is discarded: the body prints with today's storage");
        Assert.That(writer.StorageRefusals, Is.EqualTo(1));
        Assert.That(writer.StorageRefusalLog.Single(), Does.Contain(nameof(Rule.S1_Mismatch)));

        var strict = PinnedWriter(true);
        strict.StorageResolver = Wrong;
        Assert.That(() => strict.ResolveStorage(null, body), Throws.InvalidOperationException.With.Message.Contain("S1_Mismatch"));
    }
}
