# The C# writer's optimizer pipeline

For component rationale, related research, source links and evaluation guidance, see the
[optimizer handbook](optimizer/README.md). This page remains the detailed pipeline contract.

This reference describes the implemented C# optimizer. Where it and a historical plan under
`docs/design/` or `docs/archive/` disagree, use this reference; implementation and tests take precedence
if either document drifts. Examples below illustrate the transformations, not byte-for-byte output
snapshots. Generated names and capture temporaries may differ. Performance measurements belong in
benchmark results, not this reference.

An agent reading only this file should be able to take a Plato expression and say which pass rewrites it
under which flags, what C# comes out, and why a pass left it alone.

## 1. What runs, in order

The order is written in one place in the code: `CSharpWriter.RunOptimizerPasses`
(`writers/Plato.CSharpWriter/CSharpWriter.cs`). It runs once per emitted function body, on the function's TIR
(typed intermediate representation: the tree the elaborator builds from Plato source after type checking
and monomorphization).

```
Plato source
  -> parse, resolve, type check, monomorphize           (src/Plato.Compiler)
  -> elaborate to TIR; Boolean && and || become
     conditionals (TirShortCircuit)                      always on, in the compiler
  -> 1. TirInliner.Inline                                --inline
  -> 1b. TirFusion.Rewrite (uses TirLiteralUnroller)      --fuse
  -> 2. TirComponentUnroller.UnrollFunction              --optimize
  -> 2b. TirMultiConsumerFusion.Rewrite                  --loops --fuse-consumers
  -> 3. TirArrayMaterializer.Rewrite                     --optimize-arrays
  -> 4. TirLoopLowerer.Rewrite                           --loops
  -> TirCSharpBodyWriter                                 always on; prints the final tree as C#
       TirExactOutput specialization                    --loops --exact-output
```

Emission preserves the callee selected by the checker. For supported concrete, non-generic
receivers, a bodied library method that collides with a record field is emitted in a separately
named static class, and its resolved calls use a fully qualified static call. This avoids both
an illegal field/method declaration and C# instance-field shadowing of extension syntax.
[ExtensionStylePlan](../writers/Plato.CSharpWriter/ExtensionStyleWriter.cs) selects the route by
the resolved FunctionDef and receiver plan, not by name alone. Field projection and unresolved
stored-function invocation keep their existing routes. A supported type-reference receiver
supplies the default receiver value. [CSharpWriter](../writers/Plato.CSharpWriter/CSharpWriter.cs)
allocates class names that avoid declared types, output names and emitted method names;
[TirCSharpBodyWriter](../writers/Plato.CSharpWriter/TirCSharpBodyWriter.cs) emits the qualified call.
This route does not establish generic-receiver collision support, change pass order or require
an optimization flag. Behavioral coverage lives in
[FunctionFieldCallTests](../tests/Plato.Interpreter.Tests/FunctionFieldCallTests.cs) and
[FunctionFieldCollisionControls](../tests/Plato.Interpreter.Tests/FunctionFieldCollisionControls.cs).

Each pass returns its input unchanged when its flag is off or nothing matched, so any subset of these
baseline flags is valid and with no flag the output is the unoptimized C#. The new
`--fuse-consumers` and `--exact-output` generation flags require C# output and `--loops`;
the CLI rejects incompatible combinations. Both are independently disabled by default.
The flags are parsed in
`src/Plato.CLI/Program.cs` and stored on `CSharpWriter` as `InlineCalls`, `FuseCombinators`, `Optimize`,
`OptimizeArrays` and `LowerLoops`. The compiled law backend in `src/Plato.Laws/CSharpProgram.cs` accepts
both `LowerLoops` and `FuseCombinators`; `plato run --backend=csharp --loops --fuse` enables them.
`--fuse` can rewrite expressions without `--loops`. Conversely, `--loops` consumes supported
`Map`/`MapRange` producer chains even without `--fuse` (section 7).

`LoopOptimizationOptions` carries the additional default-off options through `ToCSharp`
and the compiled-law `CSharpProgram.Build` API. These two options are generation CLI flags;
they are not currently exposed by `plato run`. `FuseConsumers` combines only contiguous
independent Number folds over a completed owned local array, before materialization changes
producer spelling. See [consumer fusion](optimizer/consumer-fusion.md) for the exact legality
boundary. `ExactOutputAllocation` specializes canonical FlatMap loop emission when a raw
literal callback proves output width, guarded by a runtime array source and capacity check.
See [exact allocation](optimizer/exact-output.md) for callback preservation and fallback.

Dumping and ordinary emission share the same scheduling sequence; diagnostics observe
each phase without running a separate copy of the pipeline.

The experimental writer property `InlineStatements` adds direct statement-body call expansion
before this pipeline. Its typed local marker, `TirInlineBinding`, preserves argument and result
conversion boundaries. Its restricted scope and verification are described in the
[inlining guide](optimizer/inlining.md). The C#-only `--inline-policy` option selects
`statements`, `lambdas`, or `iterative`; existing recipes do not enable it.
`TirInlineRegion` and `TirInlineExit` preserve branch-local prerequisites and isolate callee
returns from caller returns. Ordered loop lowering enters these regions and sequences computed
arguments using the same prerequisite representation; see the [loop guide](optimizer/loops.md).

Two diagnostics help while working on a pass. `--dump-tir=<dir>` writes one file per owner type, listing for
every body its input and the tree after each pass that changed it. `--inline-report` prints, to standard
error, each inliner refusal keyed by (caller, callee, reason), plus totals.

Passes that change the tree shape (`TirLoweredLoop`, `TirTempRef`, `TirComponentAccess`,
`TirConstructorCall`, `TirBooleanChain`) introduce marker nodes that exist only for emission. A pass that runs
earlier never sees a marker from a later pass. A pass that runs later sees the markers of earlier ones: the
loop lowerer reads `TirComponentAccess`, `TirConstructorCall` and `TirBooleanChain` and passes through them.

## 2. The runtime contract the passes assume

The array vocabulary has five host functions, declared in `stdlib/foundation/intrinsics.library.plato` and
implemented for C# in `src/Plato.Intrinsics/ArrayIntrinsics.cs`:

| Function | Role | C# runtime behaviour |
|---|---|---|
| `Count(xs)`, `At(xs, i)` | observe | `IReadOnlyList<T>.Count` and its indexer; not declared in `ArrayIntrinsics` |
| `MapRange(n, f)` | the only constructor | **eager**: allocates `T[n]`, calls `f(0..n-1)` once each, in order; `n <= 0` gives the empty array |
| `Reduce(xs, acc, f)` | the only sequential fold | a `for` loop over `xs` |
| `FlatMap(xs, f)` | the only length-varying producer | appends every element of each `f(x)` to a `List<T>` |

Everything else (`Map`, `Zip`, `MapPairs`, `Take`, `Skip`, `TakeLast`, `Slice`, `SubArray`, `Reverse`,
`EveryNth`, `Repeat`, `All`, `Any`, ...) is a Plato body in `stdlib/foundation/primitives.library.plato`,
written over those five. Derived producers implemented with `MapRange` allocate and fill a new array
unless optimized away. Consumers such as `All` and `Any` return a scalar, not an array.
The live array intrinsics do not provide lazy views.

This has been the case since TKT-350 (the `MapRange` summary in `ArrayIntrinsics.cs` gives the reason: a lazy
list re-ran `f` on every read, so the STEP mesher meshed each face once per read of its outcomes). Older text
that says `Map` returns a lazy view, or that `Take`/`Skip`/`Slice`/`Concatenate` never copy, describes the
pre-TKT-350 runtime in `src/Plato.Intrinsics/bonepile/` and is corrected or dated in the files listed in
section 9.

`MapEager` and `MapRangeEager`, the names the array materializer (pass 3) writes, exist only under
`src/Plato.Intrinsics/bonepile/ArrayExtensions.cs`. `Plato.Intrinsics.projitems` does not compile that
folder. See pass 3 for what this means.

## 3. Pass 0: short-circuit `&&` and `||` (compiler, always on)

| | |
|---|---|
| Code | `src/Plato.Compiler/Checking/TirShortCircuit.cs`, called from `Elaborator.ShortCircuit` |
| Flag | none |
| Consumes / produces | TIR calls `And(a, b)` / `Or(a, b)` on `Boolean` / the conditionals `a ? b : false` and `a ? true : b` |
| Pinned by | `tests/Plato.Interpreter.Tests/ShortCircuitTests.cs` (guarded index laws; interpreter and compiled C# agree), `tests/PlatoTests/Emission/ShortCircuitEmitDifference.cs` (the Rust and TypeScript flag-on tests allow exactly this difference) |

Recognised: `And` and `Or` whose overload is the `Boolean` one. Refused: an `And` or `Or` that a library
defines for its own type (`type Flag`); it stays an ordinary call that evaluates both arguments. The C# body
writer prints the conditional as C# `a ? b : false`, which already short-circuits; the Rust and TypeScript
writers call `TirShortCircuit.TryMatch` to print `&&` and `||`. Fusion can rewrite expressions inside
the conditional without hoisting them; the loop lowerer does not descend into it (sections 4a and 7).

Constructor markers created by inlining retain the resolved result type through loop
lowering. Emission groups conditional receivers for array count/index access and groups
a conditional operand beneath a typed-index cast, preserving the TIR expression boundary.

## 4. Pass 1: `TirInliner` (`--inline`)

File `writers/Plato.CSharpWriter/Optimization/TirInliner.cs`; refusal reasons in `InlineReport.cs`; host interface
`ITirInlineHost.cs`. Pinned by `tests/PlatoTests/Optimization/InlinerTests.cs` (`PointTransformFullyBetaReduces`,
`BoundsTransformDropsDelegate`, `TriangleTransformRewritesToConstructor`,
`MeshTransformRewritesToConstructor`).

**Consumes** a TIR body. **Produces** the same body with each resolved call replaced by the callee's
monomorphized TIR body, parameters substituted by the argument nodes. It repeats until nothing changes or
`MaxPasses = 8`, because inlining exposes further calls. A callee body is at most `MaxBodyNodes = 120` nodes.
(The 3-pass, 40-node figures in `docs/design/optimizer-stage2-plan.md` are the 2026-07-10 first slice.)

**Recognised.** `Scale(xs, k)` in

```plato
Scale(xs: Array<Number>, k: Number): Array<Number> => xs.Map(x => x.Mul(k));
Total(xs: Array<Number>, zero: Number): Number => xs.Reduce(zero, (a, b) => a.Add(b));
Example(n: Integer, k: Number, zero: Number): Number => Total(Scale(MapRange(n, i => i.ToNumber), k), zero);
```

Without `--inline`, `Example` emits

```csharp
n.MapRange((i) => i.ToNumber()).Scale(k).Total(zero)
```

and with `--inline` it emits

```csharp
var _var1 = k;
return n.MapRange((i_0) => i_0.ToNumber()).Map((x) => x.Mul(_var1)).Reduce(zero, (a, b) => a.Add(b));
```

The two library calls became their bodies, so the `Map` and `Reduce` that were hidden inside `Scale` and
`Total` are now visible to fusion and the later passes. That is the purpose of the pass. Lambda parameters that collide with
names in the caller are renamed (`i` to `i_0`), using a counter (`TirInliner.NextRenameId`) reset per
generation so output is deterministic.

**Refused**, with the reason `InlineReport` records. The comments are quoted from `InlineRefusal`:

| Reason | Meaning |
|---|---|
| `NoCalleeBody` | `call.Callee?.Body == null` (unresolved / handwritten-only member) |
| `NoGroundTir` | no monomorphized TIR body for the receiver's concrete type |
| `StatementBody` | callee body is a statement tree (block/return/if/loop) |
| `ReturnTypeMismatch` | body value type != declared return type (and not a tuple lift) |
| `NotSelfContained` | bare names / eta lambdas / non-emittable member calls / nested tuples |
| `InsideLambda` | call site under a lambda; callee has a lambda or non-cheap args |
| `ArgTypeMismatch` | a known arg type != the callee's resolved parameter type |
| `LambdaArgRefused` | a lambda argument that is not all-uses-consuming / over budget / V1 recipe |
| `MultiUseCompound` | a compound arg bound to a parameter used >1 time or under a lambda |
| `ArityMismatch` | parameter count != argument count |
| `NullaryRefused` | a nullary (`Constants.X`) call that failed the static-body rules |
| `SelfRecursive` | callee resolves to the function being inlined into (would never settle) |

The reasons trace to four rules in the file header, all of which keep substitution from changing meaning or
duplicating work: expression bodies only; a parameter used more than once accepts only a cheap argument
(parameter, variable, literal, type reference); a lambda literal is never substituted for a parameter that is
not consumed in the callee (it would lose its delegate typing); a call under a lambda inlines only
lambda-free callees with cheap arguments. A callee that calls itself is detected by remembering the
(callee, receiver type) pairs inlined in earlier passes.

**Runtime intrinsic:** none. The pass only moves Plato bodies.

## 4a. Pass 1b: `TirFusion` and `TirLiteralUnroller` (`--fuse`)

Files `writers/Plato.CSharpWriter/Optimization/TirFusion.cs` and `TirLiteralUnroller.cs`.
Pinned by `tests/PlatoTests/Optimization/CombinatorFusionTests.cs` and `LiteralUnrollingTests.cs`.
The fusion pass runs immediately after inlining, before component unrolling or materialization.

**Consumes / produces:** expressions rewritten in place. It traverses lambda bodies and conditionals
without moving expressions out of their scope; this is distinct from emitting loops there.

| Recognised expression | Rewrite |
|---|---|
| `n.Range` | `n.MapRange(i => i)` |
| `xs.Take(k)` with a cheap source, or `n.MapRange(g).Take(k)` | `k.MapRange(i => xs.At(i))` or `k.MapRange(g)`, within `Take`'s valid input domain |
| `xs.Map(g).Map(f)` / `n.MapRange(g).Map(f)` | compose the functions into one producer |
| `xs.Map(g).Reduce(seed, f)`, or `All`, `Any`, `FlatMap` over `Map` | substitute the mapped value into the consumer function |
| `MapRange` with a nonnegative integer literal count no greater than `TirFusion.UnrollLimit` | an array literal of the function's values at each index |
| `Map`, `Reduce`, `All`, `Any` over a small array literal | element expressions, a nested fold, or a Boolean chain; empty `All` is true and empty `Any` is false |

Composition requires expression-bodied, non-eta lambda literals. `TirFusion.CanSubstitute` refuses
to duplicate a non-cheap expression or substitute it under a nested lambda. A refused composition can
still become one loop through the lowerer's per-stage locals (section 7). Literal expansion uses the
same substitution guard; unary `Function1` parameters and variables can also be invoked during
expansion. `Reduce` expansion requires a binary lambda. `TirFusion.UnrollLimit` is the authority for
the expansion bound; a dynamic count or a larger literal remains a producer for later lowering.

For example, `3.Range.Any(j => j.Less(k))` becomes a short-circuit disjunction of the predicate at
indices 0, 1 and 2, including when nested inside another lambda. A literal producer whose array is
still needed continues to allocate that result; unrolling alone does not remove every allocation.

**Boundary:** fusion matches expression chains, not producer definitions reached through local
variables. `Range` and `Take` normalization make additional chains visible to loop lowering;
`WithoutFuseTheRangeCallRemains` pins the distinction when `Range` is a bodiless call.

## 5. Pass 2: `TirComponentUnroller` (`--optimize`)

File `writers/Plato.CSharpWriter/Optimization/TirComponentUnroller.cs`. It uses `ComponentUnroller.cs`, the older
emission-time design, for the table of fixed-arity types and their field names
(`ComponentUnroller.BuildComponentFieldTable`) and for the decision rules; the file header of
`ComponentUnroller.cs` explains the design. Pinned by `tests/PlatoTests/Optimization/ComponentUnrollerTests.cs`
(`FixedArityBodiesUnrollToFieldExpressions`, `ThreeComponentBodiesFanOutAllFields`,
`GenuinelyDynamicArraysStillLoopLower`, `OptimizeOffLeavesBodiesToTheLoopLowerer`).

**Consumes** a TIR body. **Produces** the body with array machinery replaced by direct field expressions,
using `TirComponentAccess`, `TirConstructorCall` and `TirBooleanChain` markers. It rewrites a body only when
the result contains no lambda (all-or-nothing, because a surviving lambda would meet marker nodes the later
capture rewrite cannot traverse), and only for expression bodies.

Three families:

1. **Component higher-order calls on fixed-arity types** (`Vector3`, `Integer2`, `Color`, ...):
   `MapComponents`, `ZipComponents`, `AllZipComponents`, `AnyZipComponents`, `AllComponents`, `AnyComponent`,
   and `Reduce`, when the last argument is a lambda literal or a `Function{N}` parameter or variable.
2. **Generic library bodies monomorphized to a fixed-arity type**: `CreateFromComponents`, `All` and `Any`
   over `Components(...)`. Result, from the test cases:

   ```csharp
   Integer2 ZipComponents(this Integer2 a, Integer2 b, Func<Integer, Integer, Integer> f)
       => new Ara3D.Geometry.Integer2(f.Invoke(a.A, b.A), f.Invoke(a.B, b.B))
   Boolean AllZipComponents(...) => f.Invoke(a.A, b.A) && f.Invoke(a.B, b.B)
   ```
3. **`Map`, `Zip`, `Reverse`, `Reduce`, `All`, `Any` over an array literal** (a `TirArray`, for example the
   eight corners an inlined `Corners` exposes): unrolled per element. `Map` gives a new `TirArray`;
   `Reduce` gives a chain of calls from the seed; `All` and `Any` give `&&` or `||` chains (an empty literal
   is left alone).

**Refused:** a combinator over a dynamic array (`GenuinelyDynamicArraysStillLoopLower` checks that the pass
does not claim these), unsupported component layouts and callback shapes. Structural rewriting enters
statement blocks, inline regions and surviving lambdas; capture rewriting preserves the marker children.

**Runtime intrinsic:** none. The output is field reads and constructors.

The scratch program in section 8 has no fixed-arity type, so this pass is a no-op there.

## 6. Pass 3: `TirArrayMaterializer` (`--optimize-arrays`)

File `writers/Plato.CSharpWriter/Optimization/TirArrayMaterializer.cs`. Pinned by `tests/PlatoTests/Optimization/ArrayMaterializerTests.cs`
(`FlagOnlyRenamesMapCallSitesInMaterializationPositions`: with the flag on, the standard library's emitted
text differs from flag-off only by `Map` to `MapEager` and `MapRange` to `MapRangeEager` renames, and at
least one occurs).

**Consumes** a TIR body. **Produces** the same body with the *name* of certain two-argument `Map` and
`MapRange` calls changed to `MapEager` and `MapRangeEager`. Nothing else changes.

**Recognised.** The call must sit in a materialization position:

1. a direct argument of a constructor (`TirNew`) or of a `TupleN` call (a tuple that builds a struct), looking
   through a `TirCoerce`; or
2. the value of a `let` whose variable is referenced more than once, or referenced under a lambda or loop.

```plato
type Grid { Values: Array<Number>; }
MakeGrid(n: Integer): Grid => new Grid(MapRange(n, i => i.ToNumber));
```

emits, with `--optimize-arrays`:

```csharp
public static Grid MakeGrid(this Integer n) => new Grid(n.MapRangeEager((i) => i.ToNumber()));
```

**Refused:** a `Map` that is the argument of an ordinary call. `Scale(MapRange(n, ...), k)` inside
`new Grid(...)` stays `MapRange(...).Scale(k)`, because the position test looks at the direct argument only
(`MakeGrid2` in the scratch run).

**Runtime intrinsic:** `MapEager` and `MapRangeEager`, which exist only in `bonepile/` (section 2).
**Known defect, as of 2026-10-23:** since TKT-350 made `MapRange` eager, this pass no longer changes
behaviour, and the names it writes are not defined by the compiled runtime, so output generated with the
flag on and not later lowered by pass 4 refers to methods `Plato.Intrinsics` does not define (not built
here; read from the project file). Pass 4 accepts the
eager names at supported lowering sites. Adding `--loops` removes those calls only where its guards
allow lowering; it does not guarantee every renamed call disappears. Do not use `--optimize-arrays`
alone on a program you intend to build. The flag is kept because
`ArrayMaterializerTests` still runs it; whether to retire the pass is a decision for its own ticket.

The file header formerly said that `Map` and `MapRange` return lazy views. That is corrected in section 9.

## 7. Pass 4: `TirLoopLowerer` (`--loops`)

File `writers/Plato.CSharpWriter/Optimization/TirLoopLowerer.cs`; the loop text is printed by
`TirCSharpBodyWriter.WriteLoweredLoop` in `TirCSharpBodyWriter.Loops.cs`. Pinned by `tests/PlatoTests/Optimization/LoopLowererArrayTypeTests.cs`
(`CombinatorOverArrayLowersToForLoop`: `Map`, `MapRange`, `Reduce` and `Any` over `Array<T>`) and, for
the unroller's interplay, `ComponentUnrollerTests.GenuinelyDynamicArraysStillLoopLower`.
`CombinatorFusionTests` covers producer chains, including `RangeFlatMap`.

**Consumes** a TIR body. **Produces** ordered prerequisites and a value for each rewritten expression.
A recognized call becomes a `TirLoweredLoop` statement and a `TirTempRef` to its result local
(`_lp<id>`). Earlier operands finish before later prerequisites run. Conditional prerequisites remain
inside their selected branch; statement blocks and inline regions retain their lexical scope.
`TirGuardedLoop` keeps condition prerequisites inside every loop test. Supported callback bodies
lower in their own execution scope, with fresh identities for each use. An unchanged expression body
stays an expression. The ids come from `TirLoopLowerer.NextId`, reset per generation.

**Recognised** call names and the loop each becomes (kind names are those in the code):

| Plato call | Loop `Kind` | Shape of the C# |
|---|---|---|
| `Map` / `MapEager` with a 1- or 2-parameter function | `Map` / `MapIdx` | fill `new T[source.Count]` |
| `MapRange` / `MapRangeEager` | `MapRange` | fill `new T[Math.Max(0, (int)n)]` |
| `Zip` with 2 or 3 sources | `Zip2` / `Zip3` | fill, length of the shortest |
| `Reduce(xs, seed, f)` | `Reduce` | fold into a local |
| `All`, `Any` | `All` / `Any` | loop that stops early |
| `FlatMap` | `FlatMap` | append each returned collection to one result `List<T>` |
| `Reverse`, `WithNext`, `MapPairs`, `MapTriplets`, `MapQuartets` | same name | fill |

The result array is bound as `IReadOnlyList<T> _lp<id> = _a<id>;` because every Plato array is an
`IReadOnlyList<T>` in the emitted C# (a `T[]` temp gave compile errors in TKT-505).
`FlatMap` exposes its result list through the same interface. Normally it evaluates the callback's
collection and copies its elements. For a raw expression-bodied lambda whose body is a conditional,
the emitter can append a raw singleton-array arm directly with `List<T>.Add`, avoiding that temporary
array. Either or both arms may be singletons. The callback result, conditional, arms and lowered result
must agree exactly on a concrete one-dimensional array type, and the singleton element must have the
exact result element type. Element expressions retain the original implicit conversion context.

Wrapped callbacks, wrapped conditional results or arms, and uncertain or mismatched types retain
ordinary copying. An unmatched opposite arm still evaluates in its selected branch, binds to
`IReadOnlyList<T>` and copies its elements. In particular, `[Fault()].Take(0)` still evaluates `Fault()`
when selected. This is emission within an already lowered `FlatMap`; it does not expand the lowerer's
scope traversal or recognise empty calls. The helper is `TirCSharpBodyWriter.FlatMap.cs`;
`FlatMapFilterTests` compiles and executes the supported and refused shapes.

**Producer-chain lowering is already implemented.** For `Map` (including indexed map), `Reduce`,
`All`, `Any` and `FlatMap`, a supported receiver chain of unary `Map` calls over an array or
`MapRange` is walked inside the consumer's loop. `TirLoweredLoop.SourceStages` records the functions;
`IndexSource` selects direct index iteration for a `MapRange` base. The emitter binds each stage's
result to a local, so a consumer that uses it twice does not recompute it. This removes intermediate
producer arrays; a producing consumer still needs its result storage. It requires `--loops`, not
`--fuse`. Fusion additionally exposes `Range` and supported `Take` expressions as `MapRange`.
These rules follow the receiver expression only, not a local variable's earlier definition.

**Refused** (the call stays a normal method call):

- the call is inside a lambda that is not a recognized loop callback; arbitrary escaping lambda
  bodies remain for their normal emission path;
- the function argument is not a supported lambda of the right arity or a `Function{N}` parameter
  or variable of exactly that arity; `_eta` forwarding lambdas remain refused. Supported statement
  callbacks use a local result and exit region inside each iteration;
- the receiver or result is not one-dimensional: `TirRewrite.IsListType` accepts type names `Array` and
  `IArray` with one type argument (TKT-505 added `Array`; before, only `IArray` matched and the STEP
  mesher's 1,496 generated files lost every loop). 2D and 3D arrays are left to the runtime;
- the element type is not a concrete C# type (a solver type variable such as `$T`): `IsRenderableConcrete`.

**Runtime intrinsic:** none for the lowered combinator itself. Depending on its kind, a loop reads
`.Count` and the indexer or walks indices directly, then writes a `T[]`, accumulates a scalar, or
appends to a `List<T>`. Calls inside callback expressions can remain; refused sites use their ordinary
library or intrinsic implementation.

## 8. Worked example

Consider `Example` from section 4, with `Map` a bodiless signature in a self-contained test corpus,
alongside `Number`, `Integer`, the `Function` types, and the other operation signatures. Its expression
is `Total(Scale(MapRange(n, i => i.ToNumber), k), zero)`. The table describes the expected transformation
from the implementation, not a captured CLI transcript. `MapRange` is already explicit here;
`Range` normalization is unnecessary.

| Flags | Pass that fires | Resulting shape of `Example` |
|---|---|---|
| none | none | `n.MapRange((i) => i.ToNumber()).Scale(k).Total(zero)` |
| `--inline` | 1 | `var _var1 = k; return n.MapRange(...).Map((x) => x.Mul(_var1)).Reduce(zero, (a, b) => a.Add(b));` |
| `--inline --optimize-arrays` | 1 (3 finds no materialization position) | same as `--inline` |
| `--loops` | 4, on the one visible call | fill a `MapRange` result array, then call `Scale` and `Total`, which the lowerer does not recognise |
| `--inline --loops` | 1, then 4 | one index loop applies each producer stage into a local and folds into the accumulator; no intermediate array |
| `--inline --fuse` | 1, then 1b | compose `MapRange` and `Map`; the resulting `MapRange(...).Reduce(...)` still uses runtime calls and a producer array |
| `--inline --fuse --loops` | 1, then 1b, then 4 | one index loop with a composed producer stage feeding the fold; no intermediate array |
| `--inline --optimize --optimize-arrays --loops` | 1, then 4 (pass 2 finds no fixed-arity type, pass 3 no position) | the same loop shape as `--inline --loops` |

The `--inline --loops` row is conceptually the following C#, with descriptive local names rather
than the emitter's generated identifiers:

```csharp
Number total = zero;
var count = Math.Max(0, (int)n);
for (var index = 0; index < count; index++)
{
    var converted = ((Integer)index).ToNumber();
    var scaled = converted.Mul(k);
    total = total.Add(scaled);
}
return total;
```

`CombinatorFusionTests.MapChainIsOneLoopWithNoIntermediateArray` checks the emitted shape for direct
chains, including a mapped value used twice. `WithoutFuseTheRangeCallRemains` shows why
`n.Range.Reduce(...)` needs normalization when `Range` remains a call. The lowerer's chain recognition
is in `IsChainConsumer`, `LowerChainReceiver` and `TryLower`; none checks `FuseCombinators`.

To answer "which pass changes this expression under flags F" for another expression, run the CLI with
`--dump-tir=<dir>`. Each pass that changed the body is a heading in the dump. The input stage shows the
elaborated tree, so a call written as a library method (`Scale`) appears as itself until pass 1 replaces it.

## 9. Historical documentation

| File | Old statement | Now |
|---|---|---|
| `writers/Plato.CSharpWriter/Optimization/TirArrayMaterializer.cs` header | `Map`/`MapRange` return lazy views; the eager rename avoids re-evaluation | dated 2026-10-23: `MapRange` is eager (TKT-350); the rename is a no-op for behaviour, and the eager names live in `bonepile/` |
| `docs/design/optimizer-stage2-plan.md` | "LAZY views" in the problem statement; fusion, struct functors and consumer-side lowering as plain future work | a dated note points here; fusion and consumer-chain lowering shipped, while the plan's original problem statement remains historical |
| `stdlib/foundation/primitives.library.plato` sub-range section | "None copies" | each sub-range function is a `MapRange` and so copies its window; the note says so and is dated |

## 10. Remaining boundaries

- **Fusion across local bindings.** A producer stored in a local and consumed later is not joined by
  these expression-chain rules. Extending them requires preserving shared results and evaluation scope.
- **Broader filter-idiom lowering.** Eligible singleton arms append directly (section 7), but empty-call
  recognition, recursive conditional expansion and named Filter vocabulary remain outside that slice.
  TKT-507 in the sibling `plato-brep/tickets/TKT-507-plato-filter-idiom-lowering.md` includes vocabulary
  and consumer adoption; the compiler's singleton optimization does not complete that ticket.
- **Struct functors.** Surviving callbacks use `Func<...>` delegates. The writer has no pass replacing
  them with value-type callable representations; allocation depends on captures and C# delegate caching.
- **General escaping lambda optimization.** Loop lowering enters recognized loop callbacks and
  conditional branches, but does not descend into every surviving lambda. Unknown delegate targets
  remain calls. Statement inlining is experimental and requires an explicit policy; the existing
  expression inliner still refuses statement callees.
- **`--optimize-arrays` against the live runtime** (section 6).

## 11. Where each fact lives

| Question | File |
|---|---|
| Order of the passes | `writers/Plato.CSharpWriter/CSharpWriter.cs`, `RunOptimizerPasses` (the only place) |
| Which flag enables which pass | `src/Plato.CLI/Program.cs` and the fields on `CSharpWriter` |
| Refusal reasons for the inliner | `writers/Plato.CSharpWriter/Optimization/InlineReport.cs` |
| What a pass consumes and produces | the header of its own file; each refers here for the order |
| The array runtime | `src/Plato.Intrinsics/ArrayIntrinsics.cs`, `stdlib/foundation/intrinsics.library.plato` |
| Language semantics of `&&` and `||` | `docs/SEMANTICS.md` section 6 |
