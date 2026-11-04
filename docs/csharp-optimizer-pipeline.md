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
  -> 1. TirInliner.Inline                                --inline (or --lower-scalars: scalar callees only)
  -> 1a. TirCallbackSpecializer.Rewrite                  --specialize-callbacks (section 7e)
  -> 1a'. ConstantCounts.Rewrite                         --scalar-arrays (section 7f)
  -> 1b. TirFusion.Rewrite (uses TirLiteralUnroller)      --fuse
  -> 2. TirComponentUnroller.UnrollFunction              --optimize
  -> 2b. TirMultiConsumerFusion.Rewrite                  --loops --fuse-consumers
  -> 3. TirArrayMaterializer.Rewrite                     --optimize-arrays
  -> 4. TirLoopLowerer.Rewrite                           --loops
  -> 5. TirInPlaceUpdates.Rewrite                        --in-place
  -> 6. TirLoopInvariantHoister.Rewrite                --loops --hoist-invariants
  -> (body writer entry, after its own TirLambdaCaptureRewriter.Rewrite)
     7. TirStorageResolver.Resolve                       --owned-arrays (section 7f)
     8. StorageChecker.Check                             --owned-arrays; a violation discards 7 for that body
        (7 gives a Heap temporary a stackalloc slot under --stack-arrays, section 7h)
  -> TirCSharpBodyWriter                                 always on; prints the final tree as C#
       scalar lowering (not a TIR pass)                 --lower-scalars
       TirExactOutput specialization                    --loops --exact-output
```

Scalar lowering is part of emission, after loop lowering, like `TirExactOutput`: it rewrites no tree, so
`RunOptimizerPasses` does not list it and `--dump-tir` shows no stage for it. The only part of
`--lower-scalars` that runs inside the pass sequence is the scalar-only mode of pass 1 (section 4).

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

**Writer switches are on by default for C# (decoder wave, 2026-11-01).** The twenty-one switches of the
plato-brep regeneration (`--loops --fuse --fuse-consumers --exact-output --lean-inlining --share-calls
--lower-scalars --concrete-arrays --presize --in-place --sink-bindings --bind-scrutinee --share-calls-wide
--closure-free --in-params --switch-match --hoist-invariants --cache-constants --primitive-compare
--lower-more --specialize-callbacks`) are rows of one table, `src/Plato.CLI/WriterSwitches.cs`, and are on
by default for C# generation and for `plato run --backend=csharp|differential`. Each has a `--no-<name>`
spelling; if both spellings appear the last wins, and a `--no-` option naming no switch is an error.
`--no-writer-defaults` turns every default off, which gives the output the sentences above call "with no
flag"; an explicit `--<name>` still turns that one switch on. For other targets (TypeScript, Rust, GLSL,
C++, CUDA) and for the interpreter backend every switch starts off, so a default never raises an error;
passing a C#-only switch explicitly there is still rejected. A defaulted switch whose dependency is off is
off too, silently (`--no-loops` also drops `--fuse-consumers`, `--exact-output`, `--hoist-invariants` and
`--lower-more`); an explicit dependent switch without its dependency is rejected as before. The statements
below that call a switch "default off" describe the switch itself; the CLI now turns it on unless told not to.

Each pass returns its input unchanged when its flag is off or nothing matched, so any subset of these
baseline flags is valid and with no flag the output is the unoptimized C#. The new
`--fuse-consumers` and `--exact-output` generation flags require C# output and `--loops`;
the CLI rejects incompatible combinations. Both are independently disabled by default.
The flags are parsed in
`src/Plato.CLI/Program.cs` and stored on `CSharpWriter` as `InlineCalls`, `FuseCombinators`, `Optimize`,
`OptimizeArrays` and `LowerLoops`. The compiled law backend in `src/Plato.Laws/CSharpProgram.cs` accepts
`LowerLoops`, `FuseCombinators` and `LowerScalars`; `plato run --backend=csharp --loops --fuse` enables the
first two and `--lower-scalars` the third (section 7b).
`--fuse` can rewrite expressions without `--loops`. Conversely, `--loops` consumes supported
`Map`/`MapRange` producer chains even without `--fuse` (section 7).

`--lower-scalars` is a C#-only generation switch, off by default. The CLI rejects it with any other
target (`GenerateCommand.cs`) and does not require `--loops`: scalar lets and intrinsics lower in every
body, and the loop hooks apply when `--loops` produced loops (section 7b).

`LoopOptimizationOptions` carries the additional default-off options through `ToCSharp`
and the compiled-law `CSharpProgram.Build` API. These options are generation CLI flags;
they are not currently exposed by `plato run`. `FuseConsumers` combines only contiguous
independent Number folds over a completed owned local array, before materialization changes
producer spelling. See [consumer fusion](optimizer/consumer-fusion.md) for the exact legality
boundary. `ExactOutputAllocation` specializes canonical FlatMap loop emission when a raw
literal callback proves output width, guarded by a runtime array source and capacity check.
See [exact allocation](optimizer/exact-output.md) for callback preservation and fallback.
`HoistInvariants` (`--hoist-invariants`, TKT-1092, also default-off and requiring `--loops`)
computes, once per loop and only when the loop runs at least once, each largest subexpression of a
lowered loop's expression-bodied callback that names no callback parameter or body-bound local, that
every iteration evaluates unconditionally, and that is not a cheap projection. A small forwarding
callee is expanded in the callback only when its body holds such an expression. The `Sort` and `SortIdx`
loops of `--closure-free` are left alone (their comparator is written outside the guarded loop). The rules and their
limits are in [TirLoopInvariantHoister](../writers/Plato.CSharpWriter/Optimization/TirLoopInvariantHoister.cs).

`--lean-inlining` (`CSharpWriter.LeanInlining`, `LoopOptimizationOptions.LeanInlining`, default off, no CLI
check ties it to `--loops`) is a step at the end of `WriteAll`, not a TIR pass and not part of
`RunOptimizerPasses`. The writer marks every generated member `[MethodImpl(AggressiveInlining)]`, and
forcing all of them inline fills the JIT's per-method locals budget (the cap of section 7b), after which the
JIT refuses to inline even the small Number, Integer and Boolean wrappers. `LeanInliningPass`
(`writers/Plato.CSharpWriter/Optimization/LeanInliningPass.cs`) reads the emitted C# and removes the
attribute from every member that is not a one-line forwarding member: a member access or call chain over
plain names (`a.Name(b)`, `new T(a, b)`, `self.Field`) or one binary operator over two plain names. The
handwritten intrinsics are copied, not written, so they keep theirs. The pass only removes an attribute, so
it cannot change meaning. Pinned by `tests/PlatoTests/Optimization/LeanInliningTests.cs` (which members keep
the attribute, and that nothing else changes) and `tests/Plato.Interpreter.Tests/LeanInliningProgramTests.cs`
(a lean program compiles and runs on the C# backend). The relation to `--lower-scalars`, from the witness
bodies of plato-brep TKT-1129: the two switches act on the same budget from two sides and are independent.
With lowering alone some witness bodies kept wrapper calls, because the budget lowering freed let the JIT
inline more generated callees into them; with both switches the witnesses reach zero. The figures belong to
the plato-brep report, not to this page.

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
`TirConstructorCall`, `TirBooleanChain`, `TirSpecializedCall`) introduce marker nodes that exist only for emission. A pass that runs
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
| `NotScalarBody` | scalar-only mode: a parameter or the return type of the callee is not Number, Integer or Boolean |

The reasons trace to four rules in the file header, all of which keep substitution from changing meaning or
duplicating work: expression bodies only; a parameter used more than once accepts only a cheap argument
(parameter, variable, literal, type reference); a lambda literal is never substituted for a parameter that is
not consumed in the callee (it would lose its delegate typing); a call under a lambda inlines only
lambda-free callees with cheap arguments. A callee that calls itself is detected by remembering the
(callee, receiver type) pairs inlined in earlier passes.

**Scalar-only mode.** The pass has a second entry condition. `--lower-scalars` sets
`CSharpWriter.InlineScalarBodies` (a read-only property equal to `LowerScalars`), and the pass then runs
even when `--inline` is off, expanding only a callee whose every parameter type and return type is
`Number`, `Integer` or `Boolean`; any other callee is refused as `NotScalarBody`. A nullary constant expands
only when its value type is one of the three. Character and String are scalar wrappers but never lower
(section 7b), so a body over them is not expanded in this mode. Every other rule above applies unchanged:
expression bodies only, the cheap-argument discipline, self-containment, the body budget, the pass cap.
With `--inline` also on, the scalar gate is off and the general rules govern. The reason for the mode is
that the stdlib's derived scalar helpers (`GreaterThanOrEquals`, `LessThan`, `Abs`, `Min`, `Max`,
`Square`; `stdlib/foundation/core.library.plato`, `primitives.library.plato`) are Plato bodies over the
`LessThanOrEquals` intrinsic and the arithmetic operators, so their arithmetic lowers only when they are
expanded into the caller first. The C++ writer's host reports `InlineScalarBodies` as false.
`ITirInlineHost.InlineScalarBodies` and `HasAllScalarSignature` in `TirInliner.cs` are the definitions;
`tests/PlatoTests/Optimization/ScalarBodyInliningTests.cs` pins the mode.

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
   `Reduce` gives a chain of calls from the seed only when each substitution avoids duplicating
   computed accumulator/element expressions or placing them under a lambda. The same guard
   applies to fixed-component reductions; a refused fold stays intact for later loop lowering.
   `All` and `Any` give `&&` or `||` chains (an empty literal
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
| `Sorting.Sort`, `Sorting.SortedIndices` (`--closure-free`) | `Sort` / `SortIdx` | bottom-up merge sort with the comparator inline |
| `Primitives.MakeArray2D` (`--lower-more`) | `Make2D` | row-major nested loops into one `new T[cols * rows]`, then `new Array2D<T>(...)` |
| `Primitives.MakeArray3D` (`--lower-more`) | `Make3D` | three nested loops into one presized array, then `new Array3D<T>(...)` |
| `Primitives.CartesianProduct` (`--lower-more`) | `Cartesian` | nested loops over both lists, then `new Array2D<T>(...)` |
| `Primitives.Filter` (`--lower-more`) | `Filter` | `new T[source.Count]`, append on the predicate, `Array.Resize` once if shorter |

`--lower-more` (`CSharpWriter.LowerMore`, `LoopOptimizationOptions.LowerMore`, default off, requires
`--loops`; lambda removal L3) adds the last four kinds. Without it a `MakeArray2D` call survives as a
delegate even under `--loops`, because the stdlib body wraps `f` in a second lambda for its `MapRange`
(`MapRange(columns * rows, i => f(i % columns, i / columns))`): in the STEP mesher that was 94 call sites,
73 of them the grid builders in `Meshes.g.cs`. Lowered, the grid's element index is a running counter
(no division or remainder per cell), the callback's parameters are the loop counters, and the record is
constructed once from the filled array. A grid with a non-positive extent has no cell; the record still
carries the extents as given. The one difference from the stdlib body is the ill-formed call with every
extent negative (the body's `MapRange` count is then positive): the lowered grid is empty. `Filter` fills an
array presized to the source's count (every element may pass) and trims it with one `Array.Resize` when
fewer did, so the common case allocates once and never grows a `List`. The owner check (`Callee.OwnerType`
is `Primitives`) keeps a same-named library function out. The element type must be renderable
(`IsRenderableConcrete`) and, for the grid kinds, so must the record type; a generic site keeps its call.
`TirLoopInvariantHoister` treats the new loops like any other lambda-callback loop: its hoisted locals go
inside `if (n > 0)` around the whole loop nest, so an empty grid evaluates none of them. `--presize` is
unaffected (it sizes `List` builders in source `while` loops; these loops allocate their array once).
Pinned by `tests/PlatoTests/Optimization/LowerMoreTests.cs` (values and callback order equal on and off,
the hoist interaction, the type-variable refusal, and the switch-off text).

The result array is bound as `IReadOnlyList<T> _lp<id> = _a<id>;` because every Plato array is an
`IReadOnlyList<T>` in the emitted C# (a `T[]` temp gave compile errors in TKT-505). With `--owned-arrays`
the plan may name the temporary Heap instead; it is then declared `T[] _lp<id> = _a<id>;` (section 7f).
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

## 7a. Pass 5: `TirInPlaceUpdates` (`--in-place`, TKT-1132)

Runs after the loop lowerer. Turns `SetAt(array, index, value)` (foundation `Primitives`) into
`Intrinsics.SetAtInPlace(array, index, value)` where the old array provably has no later use in the body:
the receiver is a new array made in this body (a literal, a lowered loop's result, a `MapRange`/`Map`/
`Append`/`Prepend`/`Concatenate`/`Repeat` or `SetAt` result, or a local holding only such values), nothing
else holds it, and the update is its last use or is assigned back to the same variable. The rule, clause by
clause, and the limits (parameters, record fields and other calls' results are copied) are in
[the optimization README](../writers/Plato.CSharpWriter/Optimization/README.md#in-place-array-updates---in-place-tkt-1132).
**Runtime intrinsic:** `Intrinsics.SetAtInPlace` writes only into a `T[]` and copies any other list; the
interpreter copies. The flag needs no other flag.

## 7b. Scalar lowering at emission (`--lower-scalars`)

| | |
|---|---|
| Code | `writers/Plato.CSharpWriter/ScalarLowering.cs` (the table of scalar intrinsics and their C# spellings), `TirCSharpBodyWriter.Scalars.cs` (the renderings and the three maps of primitive-held names); hooks in `TirCSharpBodyWriter.cs`, `.Loops.cs`, `.MultiConsumer.cs`, `.ControlFlow.cs`, `.FlatMap.cs` |
| Flag | `--lower-scalars` (`CSharpWriter.LowerScalars`, default off); `plato run --backend=csharp --lower-scalars` (or `--backend=differential`) for the compiled law arm |
| Consumes / produces | the final tree after pass 4 / the same tree printed with Number, Integer and Boolean values held as C# primitives inside the body; no tree is rewritten |
| Pinned by | `tests/PlatoTests/Emission/ScalarLoweringTests.cs` (emitted shape, and both spellings of each intrinsic run over the awkward inputs), `tests/PlatoTests/Optimization/ScalarBodyInliningTests.cs` (the scalar-only inliner) |

**Why.** Plato's scalars compile to single-field readonly structs: `Number` over a machine float,
`Integer` over an int, `Boolean` over a bool. Their arithmetic costs nothing only while the JIT inlines each
one-line wrapper method. RyuJIT stops inlining into a method once the locals counted after inlining reach
about 512, and `AggressiveInlining` does not lift that cap. Loop lowering and fusion put nested producers
into one method, so a large lowered body can reach the cap, after which the remaining wrapper calls stay
real calls. Body-local lowering removes the wrapper calls from the body text instead of relying on the
JIT to remove them. The evidence is plato-brep TKT-1129 and its investigation
`docs/plans/scalar-erasure-2026-10-30.md` in the plato-brep repository. The decision record is
[body-local scalar lowering](../tracker/decisions/2026-10-30-body-local-scalar-lowering.md).

**What stays a wrapper.** Signatures, fields, interfaces, generic arguments and every call boundary keep the
wrapper structs, so unit types and interface conformance are still checked by the C# compiler. Only the
inside of a body changes. This is not the whole-program erasure that
[2026-08-01](../tracker/decisions/2026-08-01-wrapper-scalars-are-the-only-representation.md) retired.
`Character` and `String` never lower.

**Rules.** Every scalar-typed node has two renderings. `WriteNode` writes the wrapper the node's Plato
type names, as before. `WritePrimitive` writes the primitive the wrapper holds:

- A scalar intrinsic (`Add`, `Multiply`, `LessThanOrEquals`, `Equals`, `Not`, `Floor`, `Sqrt`,
  `ToInteger`, ...) is a C# operator or a `NumberMath` call over its operands' primitives. The rows are
  in `ScalarLowering.cs` and mirror `src/Plato.Intrinsics/Number.cs`, `Integer.cs` and `Boolean.cs`; a change
  to a runtime operator changes its row. `NumberMath` and `NumberValue` are the profile aliases of
  `src/Plato.Intrinsics/NumberProfile.cs` (`System.Math` and `double` under cpu64, `MathF` and `float` under
  gpu32), so the lowered body follows `--profile`.
- A literal is bare; under a 64-bit Integer profile an Integer literal carries an `L` suffix, so a local
  declared from it has the Integer's own primitive type. A scalar local, inline binding, reduce accumulator, or scalar parameter of an inlined
  loop lambda (the loop index among them) is declared as a primitive and read by name.
- A scalar-to-scalar conversion is a cast of the primitive; `Array.Count` is the list's `Count`.
- Any other scalar-valued node (a field read, a call to a generated method, a handwritten intrinsic with no
  one-expression spelling such as `Hash` or `Round`) is written as a wrapper and read through `.Value`.
- Where a wrapper is required (a call argument, a return, a field, an array element, a delegate result) the
  value is wrapped once, as `Unsafe.BitCast<NumberValue, Number>(...)` (primitive and wrapper as the case
  may be). The wrapper is a single-field sequential struct over the primitive, so the reinterpretation is the
  value the constructor would build. The constructor and the implicit operator each become a real call once
  the enclosing method is past the JIT's locals cap, while the JIT expands `Unsafe.BitCast` in its importer and
  emits no instruction for it.
- Where C# wants a condition (`if`, `while`, a conditional, a loop test, an `All` or `Any` guard) the Boolean
  is written as a primitive instead of through the wrapper's `operator true`. Where C# wants an `int` (an
  indexer, an array length) a lowered Integer is bare under a 32-bit Integer profile and is
  `checked((int)(...))` under a 64-bit one, the narrowing the wrapper's implicit operator to `int` performs.
- Two cases keep the wrapper spelling so the C# compiler does not change meaning. An Integer operation, or
  a let, over constants alone (C# would fold it at compile time and reject an overflow or a division by zero
  where the runtime wraps or throws). A scalar-valued call whose receiver is a list, an interface or a type
  variable, which may bind to the runtime or to Ara3D.Collections rather than to generated code: it is
  written as an explicit conversion to the primitive, `((NumberValue)(...))`.

The lowered spelling computes what the wrapper does. `Number` equality is `double.Equals`, so two NaNs are
equal, while `LessThanOrEquals` is the IEEE `<=`. Integer arithmetic is unchecked. `ToInteger` is the
truncating cast. `And` and `Or` evaluate both operands (`&` and `|`); Plato's `&&` and `||` are already
conditionals (section 3) and a `TirBooleanChain` keeps its own `&&` or `||`.

**Example** (illustrative, with descriptive names, for `--loops --lower-scalars` over
`SumSquares(xs: Array<Number>): Number => xs.Reduce(0.0, (acc, x) => acc + x * x)`). Without the flag:

```csharp
Number total = 0.0;
var count = xs.Count;
for (var index = 0; index < count; index++)
{
    var acc = total;
    var x = xs[index];
    total = acc.Add(x.Multiply(x));
}
return total;
```

With the flag the accumulator and the scalar parameters are primitives, the element is read once through
`.Value`, and the result is wrapped once at the return (profile cpu64):

```csharp
var total = 0.0;
var count = xs.Count;
for (var index = 0; index < count; index++)
{
    var acc = total;
    var x = xs[index].Value;
    total = (acc + (x * x));
}
return Unsafe.BitCast<NumberValue, Number>(total);
```

**Boundaries.** Lowering rewrites no tree, changes no signature and adds no pass to `RunOptimizerPasses`.
An intrinsic that has no row in `ScalarLowering.cs` stays a call on the wrapper and is read through
`.Value`; for instance `LessThanOrEquals` and `Equals` are the only comparison rows, and `Equals` has no
`Boolean` row. The interpreter arm of `plato run` ignores the flag, and the differential backend compares
the interpreter with the lowered compiled arm.

## 7c. Closure-free arrays (`--closure-free`, TKT-1149)

C#-only, off by default, a stand-alone switch (the Sort lowering needs `--loops`; the copy functions do not). Two parts, both described in `writers/Plato.CSharpWriter/Optimization/README.md`: the extensions of `Append`, `Prepend`, `SetAt`, `Concatenate`, `Take`, `Skip`, `TakeLast`, `SubArray` and `Slice` become one call to the generated `ClosureFreeArrays` (an allocation and `Array.Copy` in place of a closure and a `Func` per element), and `TirLoopLowerer` lowers a `Sorting.Sort` or `SortedIndices` call with a concrete element type to the library's merge sort with the comparator lambda inlined (kinds `Sort` and `SortIdx`). Values, order, stability and the fault of a window outside the source are the Plato bodies'; the interpreter and the C++ backend are unchanged.

## 7d. Emission shape switches (`--in-params`, `--switch-match`)

Two printing switches, both off by default and independent of every pass above. With a switch off the output
is byte-identical to the build without it (plato-brep regenerates the same 1,742 files). Both are C#-only, and
`plato run --backend=csharp` takes them for the compiled law arm.

| Switch | Code | Pinned by |
|---|---|---|
| `--in-params` (`CSharpWriter.InParameters`, TKT-1066) | `Optimization/LargeStructPassing.cs`, hooks in `TirCSharpBodyWriter.cs`, `ExtensionStyleWriter.cs`, `CSharpFunctionInfo.cs`; helper `RefAccess.g.cs` | `tests/PlatoTests/Optimization/InParamsTests.cs` |
| `--switch-match` (`CSharpWriter.SwitchMatch`, TKT-1148) | `TirCSharpBodyWriter.MatchSwitch.cs` | `tests/PlatoTests/Emission/SwitchMatchTests.cs` |

**`--in-params`.** A generated struct is large when its flattened fields (a sum's `Kind` plus every case's
fields side by side, natural alignment) take at least 256 bytes. The threshold is the first one the
2026-10-27 spike measured (plato-brep `docs/plans/surface-in-spike-2026-10-27.md`: `BrepSurface` 744 bytes
and `BrepCurve` 616 gained 1.18 to 1.25x; `in` alone was about 8% slower, so the pieces ship together). Below
it a copy is a few register moves and an `in` costs an extra indirection per field read; the spike did not
measure thresholds below 256. Under the switch: a large parameter of an extension method, its `this` receiver
included, is `in T`, unless a lambda captures it or the body assigns it; an element read `xs[i]` of a large
element type used as a receiver, an argument or a `var` initialiser is `RefAccess.RefAt(xs, i)`, a `ref readonly`
accessor over the backing `T[]` that copies for any other list; an expression-bodied extension whose body is
one such read returns `ref readonly T`; a `var` initialised by either is a `ref readonly var` unless captured or
assigned. A method group cannot convert to a delegate with an `in` parameter or a `ref` return, so an
eta-expanded reference over a large type stays a lambda. An element read through an interface-typed list
(`IReadOnlyList<T>`) keeps its copy, since C# has no ref-returning indexer on the interface.

**`--switch-match`.** The Elaborator lowers `match` to a chain of `x.IsCase() ? a : x.IsOther() ? b : ... : z`
(`ElaborateMatch`), so reaching the nth case costs n tests. With the switch on, a chain of at least three tests
on one subject (the same parameter, variable or equal pure call, compared structurally) is printed as
`(x.Kind switch { T.Kind_A => a, T.Kind_B => b, _ => z })`. C# compiles the dense constants to an IL `switch` and
the JIT makes a jump table. The subject is written once, so it is evaluated once. A generic sum prints the tag
number, since its tag constants live on the instantiated struct. Shorter chains and chains that mix other
conditions keep the ternary form (the tail after the last tag test is the default arm).

## 7e. Pass 1a: `TirCallbackSpecializer` (`--specialize-callbacks`)

A C#-only switch, off by default, with no other flag required; off, the output is byte-identical. Code:
`Optimization/TirCallbackSpecializer.cs` (the pass, the `CallbackSpecializations` registry and the
`TirSpecializedCall` marker), `CallbackSpecializationWriter.cs` (the file), one case in
`TirCSharpBodyWriter.WriteNode`. Pinned by `tests/PlatoTests/Optimization/SpecializeCallbacksTests.cs`.
`plato run --backend=csharp` takes it for the compiled law arm; the interpreter ignores it.

**What it removes.** After pass 1 the lambdas left in a body are, almost all, arguments to a Plato function
whose parameter is function-typed (`Function1<Number, Point3D>`) and whose body the inliner refused, most
often because it is a statement body. Each such call allocates a delegate (plus a closure when the lambda
captures) and the callee invokes it indirectly per sample. The pass replaces the call with a call to a CLONE
of the callee: the callback parameter is gone, every invocation of it is the lambda body β-reduced in
place, and the values the lambda captured (the caller's parameters and locals it names) arrive as extra
parameters, `_cap0`, `_cap1`, ... A clone is keyed by (resolved callee, receiver type, the shape of each
lambda with its captures abstracted by position and type), so one clone serves every site of that shape
(`SameModel`'s `Corresponds(.., (x, y) => x.Equals(y))` sites ask for one clone per element type). The callee
may be a member moved out of its struct, a member kept in it (a concept obligation such as `Deform`, rendered
in the same extension context a moved member gets), or a static library function over a runtime class with
no struct (`Array`, `Function1`: `MinimizeOverDomain`, `Corresponds`); a generic one is cloned with its body
open in the type variables under a signature spelled from the call site's types, unless a type variable is
bound to a scalar. Clones are static methods of `CallbackSpecializations.g.cs`, written after every other file, named
`{Callee}_{FNV-1a of the shape, 8 hex digits}` so that an unrelated edit renames none of them (a second
shape with the same hash takes a `_2` suffix). A clone's body goes through `RunOptimizerPasses` like any
other, so when the callee forwards its callback to another function-typed parameter the forwarded lambda is
specialized again, to `CallbackSpecializations.MaxDepth` (6): the whole inverse-search chain of plato-brep
(`MinimizeOverDomain -> CanonicalOnDomain -> MinimizeOverCuts -> SearchWindow`) becomes delegate-free, and a
callback forwarded to a combinator (`Map`, `Reduce`) reaches the loop lowerer as a literal, which inlines it.
A call site inside a lambda is specialized too; its captures may be the enclosing lambda's parameters.

**Where it refuses** (the call stays as it was; the generation log counts each reason): the caller is a
law or a witness (`Law_`, `Witness_`: run once per trial, so a clone would only grow the output); the callee
is a combinator the loop lowerer or the component unroller rewrites with its lambda in place (`Map`, `All`,
`Reduce`, `MapComponents`, ...: a loop beats a clone of the stdlib body); the callee has no ground TIR for the
receiver type, is generic over a scalar, type-level (`_` receiver), an operator or conversion, collides with
a field, or calls itself; the callback parameter is used anywhere other than as the target of an invocation
or an argument of a resolved, non-tuple, non-constructor call (let-bound, returned, stored in a record or
tuple); the lambda has a statement body, a bare name, a type variable, a nested generic-origin lambda, or a
result type that its delegate type converts (the receiver position would lose the conversion, as in the
inliner); or a β-reduction would evaluate an invocation argument more than once (the lambda uses its
parameter twice and the callee applies it to something more than a cheap projection or one element read
`xs[i]`). The clone depth limit is a refusal too. `PLATO_SPECIALIZE_TRACE=1` in the environment prints each
refusal with its site to standard error.

**Interactions.** `--in-params`: a clone's modifiers come from the same rule as a moved member's
(`LargeStructPassing.ParameterModifiers` over the clone's C# parameter types), so a large receiver or
capture is `in` unless a surviving lambda in the clone captures it. `--share-calls`: a clone call is a
syntactic call the sharing pass does not key, so two identical clone calls in one statement are not shared.
`--hoist-invariants`: the hoister sees the clone's lowered loops as any other body's; it cannot expand a
clone call (no callee body). `--cache-constants`: a clone call is never a closed constant. The inliner
runs first and keeps every β-reduction it already made; the pass takes only what it refused.

## 7f. Pass 1a': `ConstantCounts` (`--scalar-arrays`, TKT-1152 chunk C0)

A C#-only switch, off by default, independent of the other new storage switches; it unrolls through the literal rules of
section 4a, so it is meant for use with `--fuse`. Off, the output is byte-identical. Code: `Optimization/ConstantCounts.cs`
(the pass), the policy-taking overloads of `TirLiteralUnroller` (`UnrollPolicy`, `UnrollZipOverArray`), one line in
`RunOptimizerPasses` between the callback specializer and `TirFusion`. Pinned by
`tests/PlatoTests/Optimization/ScalarArraysTests.cs` (the emitted text) and
`tests/Plato.Interpreter.Tests/ScalarArraysProgramTests.cs` (values, call order, faults and the three-way law run).

**What it removes.** `ComponentArray(self: IVector)` is `self.Count.Range.Map(i => self[i])`, and the generic
vector functions (`MapComponents`, `ZipComponents`, `ReduceComponents` and so `SumSqrComponents`, `MinComponent`,
`MaxComponent`, `AllComponents`, `AnyComponent`, `Dot`, ...) consume it. Each call allocated a `Number[3]`, ran a loop
over it and, for a lambda, called a delegate per element. A fixed-arity struct knows its `Count` statically; nothing
in pass 1b used that.

**The three rewrites** (each only on a leaf receiver, so nothing that can fault is dropped or repeated):

1. `Count(v)` of a fixed-arity struct becomes the integer literal. Fixed arity here is the writer's own rule for
   the `Count` it synthesizes (`CSharpTypeWriter.GenerateFunc`): a concrete, non-generic struct of two or more fields,
   not a primitive, whose `Count` and `At` no library function implements. `VectorN`, whose `Count` is the length of its
   array, and any type with its own `Count` body, are not folded. With the count a literal, `TirFusion` unrolls the
   `MapRange` in `ComponentArray` itself into a three-element literal instead of a loop.
2. A `Map`, `Zip`, `Reduce`, `All` or `Any` whose array sources are `ComponentArray(v)` calls (stdlib identity, not
   name) or small array literals is unrolled over the array of component reads `v.X`, `v.Y`, ... The rules are the
   literal rules of section 4a, with an `UnrollPolicy`: a component read of a leaf may be repeated by a lambda body,
   a delegate-typed combining function is invoked once per element (`f(f(f(init, v.X), v.Y), v.Z)`), and a `Reduce`
   accumulator that its step reads twice (the Min and Max steps) may repeat while the unrolled expression stays under
   128 nodes, which holds for three components and refuses four or more. A refused unroll leaves the call as it was.
3. `FromComponents(v, [a, b, c])` of the matching arity is `new T(a, b, c)`, which is the library body.

Element `j` of a result is computed before element `j + 1`, so a callback is called with the same arguments in the same
order as the loop called it, and one that throws is the last called. **A repeated accumulator** (Min and Max) evaluates
the accumulator expression more than once; the values are the same pure scalar expression, so results are bit-equal, and the
budget bounds the growth.

**Interactions.** `--specialize-callbacks` runs first, so `SumSqrComponents`'s `ReduceComponents_xxxxxxxx` clone is
unrolled with its lambda. `--lower-scalars` prints the unrolled arithmetic on the primitives. A `Count` folded in an
unrelated body (`LastIndex`, `Middle`) prints as the literal `Unsafe.BitCast<int, Integer>(3)`; `--cache-constants`
numbers its holder classes by emission order, so the flag renumbers some `_ConstN` names in files it does not otherwise
change.

## 7g. Owned array storage (`--owned-arrays`, TKT-1152)

Every Plato array is an `IReadOnlyList<T>` in the emitted C#, and every read of one pays a run-time type test
(`ConcreteArrayAccess.At`) because the printer once chose the representation per print site and TKT-505 forced the
interface onto every lowered temporary. `--owned-arrays` makes the representation an explicit decision. At the body
writer's entry, after its own capture rewrite, `TirStorageResolver.Resolve` records one `ArrayStorage` per binding in
a `StoragePlan` (carried to the printer by a `TirStorageScope` root), inserts a `TirRepresent` node wherever a value's
storage changes, and `StorageChecker.Check` verifies the result before anything prints: a violation prints that body
with today's storage and counts a refusal (it throws under the tests' strict mode). The printer decides nothing: it
reads `StoragePlan.Of(node)` and prints names, declarations and `TirRepresent` mechanically.

Storage of a lowered-loop temporary in this chunk (C2b), by the rules of `TirStorageResolver`:

- **Heap** (`T[]`): the loop kind fills its own array (`Map`, `MapIdx`, `MapRange`, `Zip2`, `Zip3`, `Reverse`,
  `WithNext`, `MapPairs`, `MapTriplets`, `MapQuartets`, `Sort`, `SortIdx`, `Filter`), the name is declared once
  (`TirOwnership`), no other array is assigned to it, and no `SetAt` updates it. The declaration is
  `T[] _lp<id> = _a<id>;`.
- **List** (`IReadOnlyList<T>`, today's form): every other temporary (a `Reduce` accumulator, a `FlatMap` result,
  a grid record, an `All`/`Any` flag), and a producing temporary that fails a Heap condition. The refusal is
  recorded on the plan with its reason.

Where a Heap temporary is read, the printer writes `x[i]` and `x.Length`, with no `ConcreteArrayAccess` test: as the
receiver of `Count` or `At`, and as the source of a lowered loop. Everywhere else (a return, a `Reduce` seed, a
`FlatMap` source, a conditional or match arm, a binding, a call or constructor argument, a lambda result) the
resolver wraps the reference in `TirRepresent(Heap -> List)`, printed `((IReadOnlyList<T>)x)`, the same object, no
copy. The conversion table is `CSharpStorageRules.MayConvert`: Heap to List, Heap to Borrowed, Stack to Borrowed;
Stack or Borrowed to List would be a hidden copy and is rejected by rule S2.

Named locals (C2c). The alias rule above is the general one: a `var` (`TirLet`) or lowerer alias (`TirInlineBinding`)
whose value is a Heap temporary or a Heap local is Heap when `TirOwnership` shows one declaration, no assignment after
it, no in-place update, and no reference under a printed lambda (the capture rewrite hoists a captured local into a
List local after the resolver has run). It is declared `T[] name = ...;` (not `var`, so the element type is explicit
for the cast of a later `TirRepresent`), and its reads print as a Heap temporary's do. A local that fails a condition
stays List and the reason is on `StoragePlan.Refusals`. The sources of a `FlatMap` and of a `TirMultiReduction` are loop
sources: they take their storage from the plan, so a Heap source needs no conversion there, `ExactOutput` omits its
`source is T[]` test for a Heap source (the size test stays), and `StorageChecker` treats a multi-reduction's source as
`LoopSource`. Still List by rule, not by omission: the region result of `TirInlineRegion`, the `Reduce` accumulator, the
`FlatMap` result (a `List<T>` is built in its slow path) and every lambda result.

Under `--in-params` a read `At(xs, i)` of a large element (256 bytes or more) from a Heap array prints as `x[i]`: a `ref
readonly var w = ref x[i]` local, or the element itself as an argument or receiver, which C# passes by reference to an
`in` parameter without a copy; the `RefAccess.RefAt` call and its type test are only for a List receiver.

`TirCSharpBodyWriter.PrintsAsNode` lists the node classes the printer has a case for. Under the flag a body that holds any
other node is refused by rule S8 (`StorageChecker.Rule.S8_Unprintable`), where the printer's `/*?*/` fall-through would
leave a hole in the program; the list is extended with each new printer case (C8b added `TirTwinCall`).

An index outside a Heap array throws `IndexOutOfRangeException`; the interface form throws
`ArgumentOutOfRangeException`. The law runner and the differential run treat any two faults alike, so verdicts are
unchanged (plan open question 1, default: accepted). Tests:
`tests/PlatoTests/Emission/OwnedArrayEmissionTests.cs` compiles each TKT-505 shape of
`OwnedArrayFixtures/owned.plato` with Roslyn under the nineteen pinned flags plus `--owned-arrays`, and compares
values with the flag off; `tests/Plato.Interpreter.Tests/OwnedArrayProgramTests.cs` runs the fixture's laws in the
interpreter and compiled, flag off and on.

## 7h. Stack slots (`--stack-arrays`, TKT-1152 chunk C6)

A C#-only switch, off by default, that requires `--owned-arrays` (the writer throws without it, and the CLI table enables
it only with it). Off, the output is byte-identical. On, `TirStorageResolver` upgrades a Heap lowered-loop temporary to
`ArrayStorage.Stack` (a `System.Span<T>` over a hoisted `stackalloc` slot) when all of these hold, each with a refusal
reason on `StoragePlan.Refusals` (`TirStorageResolver.StackRefusal`) and a count on the writer (`StackSlotsPlaced`,
`StackRefusals`):

- the loop is a `MapRange`, `Map`, `MapIdx` or `Reverse` (the kinds that fill a fresh `T[n]` in one pass) and
  `CountBounds.OfLoop` states its count: a constant, or an expression over parameters and unassigned variables (unknown
  counts stay Heap);
- a guarded count may read a field of a stable base (`b.Degree + 1`, `o.Inner.Degree`, `Count(s.Values)`): a call of a field
  getter, or a component read, whose base is a parameter, a variable nothing assigns, or such a field read (values are
  immutable, so it is the same number whenever read); a call that is not a field getter stays unknown;
- the element is unmanaged (`StructLayouts.IsUnmanaged`), and a constant count is at most the slot capacity
  `min(16, 256 / sizeof(T))`; a guarded count is tested at run time against that capacity, and past it the array is a
  new heap array;
- every use of the temporary, and of each single-declaration local that aliases it, is a `Count`/`At` receiver or a lowered
  loop's source, in the loop pass that allocates it (so the span never outlives the pass that reuses the slot), and no
  printed lambda names it;
- every loop around the allocation is a lowered loop (a `while` loop, a guarded loop or a printed lambda is a loop
  the plan cannot name);
- the body's slots total at most 1 KiB, and the body is not on a call cycle.

A slot is declared as `System.Span<T> _slot<id> = stackalloc T[capacity];` before the outermost loop around the
allocation (at the allocation when no loop surrounds it), never `var` (over a `stackalloc`, `var` is a pointer in unsafe
code, and `CSharpProgram` compiles with `allowUnsafe`). The allocation prints as
`System.Span<T> _a<id> = _n<id> <= capacity ? _slot<id>.Slice(0, _n<id>) : new T[_n<id>];`. The count is the loop's
own, already clamped to zero or more, so a count of zero or less is an empty slice. Every element is written before it is
read, in index order, so a reused slot needs no clearing and a callback that faults mid-fill faults at the same element as
before. A local that aliases a Stack temporary is declared `System.Span<T>`.

**Call cycles.** `CSharpStorageTarget.OnCallCycle(function, body)` walks calls from the body about to be written (so what
the optimiser inlined is included) to the elaborated bodies of the callees, lambdas included, and reports a cycle when it
reaches a function with the same owner type, name and arity as the body's own. Functions are matched by that key and not
by reference because the monomorphizer's body names a copy of its function. A call through a bodiless concept member is not
followed, and a callee whose body cannot be elaborated is a leaf (`UnanalysedCallees`); a stack overflow cannot be caught, so
a recursive body gets no slot.

**JIT inlining.** A method with a `stackalloc` is not inlined by the JIT. The slot is declared at the allocation's outermost
loop, not at the top of the body, so a branch that does not allocate does not pay for it, but every slot-bearing method
loses inlining; C9 measures that.

`StorageChecker.AllocationFrames` gives the resolver the loops around each allocation by the same traversal that later
checks the slot (rules S5 and S7). Tests: `tests/PlatoTests/Emission/StackArrayEmissionTests.cs` (the fixture
`OwnedArrayFixtures/stack.plato` compiled with Roslyn under the nineteen pinned flags plus `--owned-arrays
--stack-arrays`, strict, and run: a constant count, counts under, at and past the capacity, zero and negative counts, a slot
reused by twenty passes, a fault mid-fill, ten windows past the budget, self- and mutual recursion, an array passed on,
returned, captured or assigned again) and `tests/Plato.Interpreter.Tests/StackArrayProgramTests.cs` (the same laws
interpreted and compiled with and without the switch, equal verdicts).

## 7i. Calls to span twins (`--span-params`, TKT-1152 chunks C8a and C8b)

`--span-params` (requires `--owned-arrays`; rejected with statement inlining) writes `BorrowedTwins.g.cs` last, a static
class with a twin of each function that only reads an array parameter (`BorrowedParameters`: every use is a `Count` or
`At` receiver or a lowered loop's source, no printed lambda names it). A twin is the original body with
`System.ReadOnlySpan<T>` in place of `IReadOnlyList<T>` for each borrowed parameter, named `Extensions_<Name>`, and it is a plain
static method, never an extension; the original stays as it was, so it is still a method group and the public surface does
not change. C8b makes the twins used.

**Routing.** `TirStorageResolver` sends a call to the twin of its callee (a `TirTwinCall`, printed
`BorrowedTwins.Extensions_Name(args)`) when `IStorageTarget.RouteOf` finds one and every argument at a borrowed position is
an array the body owns or a span the twin already holds: a Heap local or temporary is wrapped
(`new System.ReadOnlySpan<T>(x)`), a Stack slot is cast (`(System.ReadOnlySpan<T>)x`), a borrowed parameter of the twin being
written goes on as it is. One argument that is none of these (a parameter array, a call result) keeps the whole call on the
original, since the twin takes a span at every borrowed position. `RouteOf` answers for array-receiver library functions, the
ones written into `Extensions.g.cs`, and not for a member moved onto a struct; it asks `BorrowedParameters.Of(callee, null)` and
names the twin from the function alone, so no call depends on the order the originals are written in. A generic callee has no
twin and its calls stay as they were.

**Lent.** A borrowed argument of a routed call is neither a borrow nor an escape for the stack decision: the array is lent for
the duration of the call and the twin cannot keep it (a span cannot be stored in a heap object, and a borrowed parameter is
never returned). The resolver counts these arguments per binding and `--stack-arrays` takes them off the escapes it finds, so a
slot can be passed to a twin.

**Interprocedural borrowing.** A parameter handed on to a borrowed position of another twin is itself borrowed
(`BorrowedParameters.Compute` routes such calls before the checker runs): `ViaPick(xs, k) => Pick(xs, k) + SumOf(xs)` has a
twin that calls `Extensions_Pick(xs, k)` with the span as it is. The callee is asked first; a function asked about while it is
being computed is on a call cycle and borrows nothing, so a parameter passed round a cycle is not borrowed. That answer is the
least one and sound, but a function in a cycle may get a different mask depending on which member the writer meets first.

A copy of a borrowed parameter is the same span whether the capture rewrite made it (`TirLet`) or the inliner bound it to an
argument (`TirInlineBinding`, printed `var`); both are planned Borrowed, which is what lets `LocalDerivative` hand `local` on to
`DeBoorLevels`.

**Check.** Every twin a printed body names must be among the twins written: `BorrowedTwinWriter` throws at the end if one is
missing (an error of the generator, found by the tests and not by the consumer's compiler). Counts: `CSharpWriter.TwinCallsRouted`
and `Borrowed.TwinsWritten`.

Tests: `tests/PlatoTests/Emission/TwinCallEmissionTests.cs` (the fixture `OwnedArrayFixtures/twin-calls.plato` compiled and run:
a slot lent to two twins, a heap array, two owned arguments, a parameter array, a generic callee, a recursive caller, a twin that
lends its span on, a recursive callee, a read past the end inside a twin, and the original used as a method group) and
`tests/Plato.Interpreter.Tests/TwinCallProgramTests.cs` (the same laws interpreted and compiled with and without the switch).

**`--specialize-callbacks-lean`** (requires `--specialize-callbacks`; with it off the output is byte-identical
to `--specialize-callbacks` alone). Two changes, one per cost the switch showed on plato-brep.

* *Validation callers.* A site whose enclosing body is a `Check_` member is refused as `validation body`
  (the `Law_` pattern). Those bodies are validation code the mesher never runs, and the combinators they call
  (`CheckAll`, `CheckGrid`, `CheckInstances`, ...) re-wrap their callbacks, so refusing the root site also
  stops the chain of clones below it. On the shipped program (library cff5aaaed, the twenty-one flags) this
  takes 3,391 sites into 2,676 clones, 3.03 MB, to 2,167 sites into 1,475 clones, 1.64 MB; no site whose
  caller is not a `Check*` or `HoldsFor*` body is lost. The policy is the caller prefix only: a callee-family
  rule (`Check*` callees) would drop seven more sites called from `DenseTopologyChecks`,
  `RootIsLowestGrid` and the `IntegerValidation*Case` functions, validation too but not provably off the
  mesher's path from a name.
* *Parameter used twice.* When a lambda uses its parameter twice and the invocation argument is not cheap
  to repeat, the clone no longer fails (`invocation argument used more than once`). The lambda body becomes
  a helper method of the same file, `{Callee}_{hash}_L{position}`, taking the invocation's arguments then the
  captures the lambda names, and the invocation calls it. The call binds the argument once, evaluated where
  the invocation was and in the same order, so this is the let form without a statement to hoist it to (an
  invocation can sit under a conditional or the right side of `&&`, where a hoisted `let` would evaluate the
  argument eagerly). Helpers are clones in the registry (`AggressiveInlining`, the callee's context) and are
  taken back with the clone if its build then fails. The 18 chordal sites of plato-brep
  (`ElementWithinChordalShared`, `SplitWithinChordalSharedCounted` and the like) specialize.

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
- **Scalar lowering is local to a body.** A value crosses every call boundary as a wrapper, so a chain of
  small generated methods still pays for the wrappers at each boundary unless the inliner (pass 1)
  expanded it. Only the three arithmetic scalars lower, and an intrinsic without a row in
  `ScalarLowering.cs` is read through `.Value`.

## 11. Where each fact lives

| Question | File |
|---|---|
| Order of the passes | `writers/Plato.CSharpWriter/CSharpWriter.cs`, `RunOptimizerPasses` (the only place) |
| Which flag enables which pass | `src/Plato.CLI/Program.cs` and the fields on `CSharpWriter` |
| Refusal reasons for the inliner | `writers/Plato.CSharpWriter/Optimization/InlineReport.cs` |
| What a pass consumes and produces | the header of its own file; each refers here for the order |
| The array runtime | `src/Plato.Intrinsics/ArrayIntrinsics.cs`, `stdlib/foundation/intrinsics.library.plato` |
| Language semantics of `&&` and `||` | `docs/SEMANTICS.md` section 6 |
| Which scalar intrinsics lower, and to what C# | `writers/Plato.CSharpWriter/ScalarLowering.cs` (rows mirror `src/Plato.Intrinsics/Number.cs`, `Integer.cs`, `Boolean.cs`) |
| Why lowering is body-local | `tracker/decisions/2026-10-30-body-local-scalar-lowering.md` |
| Which members keep `AggressiveInlining` under `--lean-inlining` | `writers/Plato.CSharpWriter/Optimization/LeanInliningPass.cs` |
