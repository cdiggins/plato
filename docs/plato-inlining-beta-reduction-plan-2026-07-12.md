# Beta-reduction / delegate-inlining — emission integration plan (2026-07-12)

> Status: **increment-2 mechanism proven at the TIR level; emission integration deferred.**
> The reusable substitution/β-reduction foundation shipped in `a71b1ff`
> (`TirRewrite` + `TirRewriteTests`). The *application* of it (delegate-parameter
> substitution, β-reduction wiring, tuple tail-lift) is preserved as WIP but NOT committed —
> it breaks the scalar+methods conformance build. This doc explains why, and proposes an
> all-extension-methods runtime as the first step that de-risks the whole surface.

WIP artifacts (not on any branch): `git stash` entry on `plato-generated-projects`, and a
standalone patch at `scratchpad/increment2-wip.patch` (334 lines).

---

## 1. What the increment did, and what proved out

Goal: collapse the delegate indirection in the `Transform → Deform(lambda)` family so that, e.g.,
`bounds.Transform(t)` becomes an inline loop instead of a call to `Deform` that calls `Map` that
invokes a `Func` per element.

Three coordinated changes on top of the committed foundation:

1. **Lambda-arg gate relaxation** — a lambda argument may substitute into a delegate-typed
   parameter when every use is a *consuming* position (`TirInvoke` target, or a direct `TirCall`
   argument), not under a nested lambda, size-bounded. (`AllUsesConsuming` in `TirInliner`.)
2. **β-reduction wiring** — after substitution, a `TirInvoke` whose target became a lambda is
   reduced via `TirRewrite.TryBetaReduce` inside the same post-order rewrite.
3. **Tail-position tuple lift** — a tuple-returning body (`(f(A), f(B), f(C))`) inlines when the
   call sits in tail/return position, where the tuple→struct implicit conversion still fires.
   Driven by a top-down `InlineTails` sweep.
   Plus a supporting **owner-type receiver fallback**: `Transform(self: IDeformable3D)` emitted on
   `Bounds3D` binds `self : Bounds3D`, so the concrete callee lookup must use the *owner* type, not
   the interface the signature declares.

**The TIR mechanism is correct.** With `--dump-tir`, every intended collapse was observed:

| Type | before (input) | after (inline → loops) |
|---|---|---|
| `Bounds3D.Transform` | `Deform(self, p => …)` | `{ loop:Map -> _lp; return Bounds(_lp); }` |
| `Point3D.Transform` | `Deform(self, p => …)` | `Transform(Vector3(self), Matrix(t))` (β-reduced) |
| `Triangle3D.Transform` | `Deform(self, p => …)` | `Tuple3(Transform(Vector3(A(self)),M), …, …)` |
| `TriangleMesh3D.Transform` | `Deform(self, p => …)` | `{ loop:Map -> _lp; return Tuple2(_lp, FaceIndices(self)); }` |

Flag-off byte-identity held throughout (hard rule 2). The 12 `TirRewriteTests` stayed green.

## 2. Where it broke — the emission layer, not the inliner

The *only* recipe that runs `--inline` is the full scalar/methods pipeline
(`--csharp-style=extensions --scalar=float --optimize --optimize-arrays --inline --methods --loops`,
optionally `--no-properties`). Wiring the new inlines into that pipeline surfaced three failures,
in descending tractability:

### 2a. Tuple→struct does not convert under scalar erasure  *(gated off)*
`TranslationX ... => (((float)x), 0f, 0f)` — the struct's implicit operator is defined for the
*wrapper* tuple `(Number,Number,Number)`; erasure yields `(float,float,float)`, which does not
convert (CS0029). The original callee body compiles because *its own* return renders the wrapper
tuple; moving it to a foreign scalar call site loses that. **Currently gated off under
`ScalarErase`** — which means the entire tuple family (Triangle/Quad/Line/Ray/Mesh) does not
collapse under the only recipe that runs `--inline`. Distinct sub-problem; see §5b.

### 2b. Property-pin breakage under the property-ful (V1) recipe  *(root cause = duality)*
`Number.cs` / `Vector3.cs` (handwritten V1 intrinsics) reference `x.Pow2`, `x.Pow3`,
`v.AlmostZero` with **property syntax**. Those names are pinned in
`CSharpWriter.HandwrittenPropertySyntaxNames` precisely because handwritten code accesses them as
properties. My broader inlining exposed inlined bodies where the emitter's property-vs-method `()`
decision went the other way (`.Pow3()` vs `.Pow3`), and the pins no longer lined up — the
handwritten runtime stopped compiling (CS0428 / "does not contain Pow2"). **This is the property /
method duality, not the inlining.**

### 2c. The owner-fallback tension  *(needs a correct fix, not a heuristic)*
The receiver-type fallback (§1) is the crux and it pulls two ways:
- **Broad** (`!IsConcreteName(zonked)` → owner): fires for the flagship interface receivers *and*
  for legitimate type-variables, mis-selecting specializations library-wide (turned `Angle.Abs`
  into a generic `Components().Map(Abs)` fanout).
- **Narrow** (`GetTypeDef(zonked).IsInterface()` → owner): stops the corruption — but then
  **`Bounds3D.Transform` no longer collapses at all** (emitted output identical to golden). The
  narrowing that fixed 2b's cousin also killed the win.

So the narrow WIP "compiles clean under `--no-properties`" **partly because little actually
inlines** — the 12 differing files are mostly loop-counter renumbering, not real collapses.

## 3. The root cause: property / method / instance / extension duality

Every failure in §2b–§2c traces to the same thing: **the emitter must decide, per call site, how to
render a member** — property vs method (`()`), instance vs extension, and under erasure, scalar-shim
vs struct-member. That decision is made by a web of context-sensitive analysis
(`PropertySyntaxNames`, `HandwrittenPropertySyntaxNames`, `ScalarEraseAnalysis`,
`AuthoritativePrim`, per-plan `KeptNoArgPropertyNames`, receiver-type lookups). It is *tuned for
bodies emitted in their home type*. **An inlined body lands in a foreign context, and the decision
can flip.** That is the entire risk surface.

Key evidence that the duality — not inlining — is the problem:

- **V2 already uses method syntax internally** (`Plato.Intrinsics.V2/Number.cs`:
  `a.Pow3() * this + b.Pow2() * this`; `Vector3.cs`: `if (v.AlmostZero())`). No pins are needed
  for V2.
- Under **`--no-properties`** (which drops the pins — `keptNoArg = NoProperties ? empty : pins`),
  the WIP **compiles with 0 errors**. The CS0428/CS0029-in-intrinsics wave is specific to the
  property-ful V1 recipe.

## 4. Proposed first step — an all-extension-methods runtime

**Make every member function an extension method (except operator overloads), in
`Plato.Intrinsics.V2` and in the emitted surface.** Structs become *fields + operators + BCL
obligations* only; all behaviour is `static` extension methods. This is the natural terminus of the
existing `--no-properties` direction (no-arg members → methods) generalized to *all* members.

Why this de-risks the inlining integration, failure-by-failure:

- **Kills 2b outright.** No pins, because no handwritten member is accessed with property syntax.
  `HandwrittenPropertySyntaxNames` shrinks to (eventually) empty. Inlined bodies can never flip a
  pin because there are none.
- **Makes the 2c spillover harmless.** Today, broad inlining *breaks the build* only because the
  exposed members (`Reduce`, `Pow2`, …) mis-render. If every member renders as one uniform
  `Foo(x, …)` / `x.Foo(…)` extension call regardless of surrounding context, then "inlined more than
  intended" produces *more code that still compiles*, not a build break. That converts the
  owner-fallback question from a *correctness* problem into a *how-much-benefit* problem — far
  safer to iterate on.
- **Simplifies scalar erasure structurally.** `Number → float` is the reason instance methods must
  become extension shims under erasure; `ScalarEraseAnalysis`/`AuthoritativePrim` exist largely to
  navigate that transform and the property/method decision inside it. If the runtime is *already*
  all-extension-methods, erasure is close to a pure type substitution — much less analysis for an
  inlined body to fall foul of.

What it does **not** fix on its own:

- **2a (tuple under erasure)** is orthogonal — it is about struct *construction*, not member
  dispatch. See §5b.
- **2c's benefit half** — even with a uniform runtime, the inliner still needs to *correctly*
  resolve an interface receiver to the owner concrete type to make the flagship cases fire (§5c).
  The uniform runtime removes the *downside* of getting it slightly wrong, not the *need* to get it
  right for the win.

Cost / scope of the first step:
- Port `Plato.Intrinsics.V2/*.cs` member methods → extension methods (keep operators as struct
  members; keep BCL/`IReadOnlyList` obligations as required). Mechanical but wide.
- Emitter: teach the extension-style writer (and the scalar path) to emit *all* members as
  extensions under a new flag (or fold into `--no-properties` as `--extensions-only`), keeping
  off-flag output byte-identical (hard rule 2).
- Gate: a V2 conformance recipe that builds + runs against the all-extension runtime, green before
  any inlining is layered back on.

This is worth doing on its own merits (it is the endgame of the V2 property-free runtime); the
inlining increment is one beneficiary, not the justification.

### §4 execution log (2026-07-12)

Runtime port started; a **scalar-erased vs non-erased** boundary emerged that splits the step:

- **Scalar-erased types — DONE** (`Number`, `Integer`, `Boolean`; commits a4c5b0f, 4f8d15a).
  Handwritten intrinsics moved to `<Type>Intrinsics` extension classes. These port cleanly with
  no emitter change: under `--scalar=float` the *generated* extensions land on `float`/`int`/`bool`
  while the handwritten ones stay on the wrapper (`Number`/`Integer`/`Boolean`), so the receiver
  types differ and there is no collision. `Character`/`String` have nothing to move (only
  conversions + IArrayLike obligations).
- **Non-erased wrapper types — BLOCKED on the emitter** (`Angle`, `Vector2/3/4/8`,
  `Matrix3x2/4x4`, `Quaternion`, `Plane`). The emitter already generates a *forwarder* extension
  for each library function on the same wrapper type (`AngleExtensions.Cos(this Angle)` forwarding
  to the intrinsic). While the intrinsic is an INSTANCE method, instance-vs-extension resolution
  picks the instance and there is no conflict; once the intrinsic becomes an extension, the two
  `Cos(this Angle)` extensions are **CS0121-ambiguous** (and the forwarder would self-recurse). So
  these types need the emitter to stop emitting the duplicate forwarder (emit the library function
  as a direct call to the intrinsic, or not at all when the intrinsic already covers it) BEFORE
  their intrinsics can move. That is the emitter half of §4.

- **Verification: DONE** — a new `Ara3D.SDK.ConformanceTests.V2Runtime` suite (commit 3007529;
  regen `tools/regen-conformance-v2runtime.ps1`, commit 5264b35 in studio) imports
  `Plato.Intrinsics.V2` and runs the full `--no-properties` recipe. **204/204**, matching Scalar.
  Every runtime-port step is now gated by it.

Revised near-term order: (1) emitter change to drop/redirect the wrapper-type forwarders so
non-erased intrinsics can move (unblocks `Vector3.AlmostZero` etc. — the actual inlining pins);
(2) finish the non-erased runtime port behind that; (3) then §5c owner-resolution + re-apply the
inlining WIP against the now-uniform surface.

## 5. Brainstorm — other ways to address the risky refactor

Complementary or alternative, roughly independent:

**5a. Develop `--inline` *only* against the `--no-properties`/V2 recipe.**
`--inline` already lives only in the V2 (`--no-properties`) Generated.Optimized recipe. Drop the
expectation that it works under property-ful V1 at all. Cheapest possible framing: the V1 scalar
conformance simply never runs `--inline`. Pairs naturally with §4 (V2 is the future runtime).

**5b. Wrapper-aware tuple emission under erasure** (fixes 2a, unlocks the tuple/mesh family).
Two options: (i) when tail-lifting a tuple body under `ScalarErase`, emit `new Struct(elem…)`
instead of a bare tuple — needs the struct's constructor shape, which the plan already knows; or
(ii) wrap each erased element back to its wrapper type at the tuple site. (i) is cleaner and
position-independent. Without this, the tuple family stays deferred even after §4.

**5c. A reliable interface-receiver → owner resolution** (unlocks 2c's benefit).
The narrow `IsInterface()` check does not fire for `Bounds3D.Transform`; diagnose why (is `zonked`
literally the interface name, or a `Self`-constrained type variable?), and resolve the concrete
receiver from *the assignment* `owner ⊑ zonked` rather than a name test. Thread the owner type as a
first-class input (the WIP already plumbs `ownerTypeName` through `Inline`/`TryInlineCall`). With
§4 in place, over-firing is no longer catastrophic, so this can be tuned empirically.

**5d. Post-inline emission normalization.**
Instead of making the runtime uniform, make the *inliner* re-annotate inlined nodes so the emitter's
existing decisions come out right in the new context (recompute scalar tags, syntax hints,
`AuthoritativePrim` inputs). This is what the committed foundation's `TirCoerce` scalar-tagging
already does for scalar *parameter* types; it would need extending to the property/method decision.
Higher-effort and fights the duality rather than removing it — a fallback if §4 is judged too broad.

**5e. Narrow the feature to the exact delegate-forwarding pattern.**
Recognize only `wrapper(f) => …container.Map(f)…` / `f(x)` shapes and special-case them, rather than
general lambda-substitution + tail-lift. Smaller blast radius, but a point solution that will not
generalize to the next HOF-inlining opportunity.

## 6. Recommended sequencing

1. **§4 all-extension-methods runtime** (V2 + emitter flag + green V2 conformance). Independently
   valuable; removes the entire property/method risk class.
2. **§5c owner resolution** — make the flagship interface-receiver collapses actually fire, now that
   over-firing is safe.
3. Re-apply the WIP (`scratchpad/increment2-wip.patch`) on top; expect the non-tuple family
   (Bounds/Points/Point) to collapse and compile under the V2 recipe.
4. **§5b wrapper-aware tuple emission** — lift the tuple/mesh family into the scalar recipe.
5. Add a focused emit test asserting the collapse (no delegate `Invoke`, no residual `.Map(` for the
   direct cases) and re-run the full gate battery.

## 7. Reproduction / where things are

- Foundation (committed, green): `Plato.CSharpWriter/TirRewrite.cs`, `PlatoTests/TirRewriteTests.cs`.
- WIP (not committed): `git stash show -p` on `plato-generated-projects`, or
  `scratchpad/increment2-wip.patch`. Touches `TirInliner.cs` (gate relaxation, `InlineTails`,
  `TryBetaReduce` wiring, tuple tail-lift, `ownerTypeName` plumbing) and `CSharpWriter.cs`
  (`fi.OwnerType.Name` passthrough).
- To see the collapses: apply the WIP, `dotnet run --project Plato.CLI -c Release -- plato-src <out>
  <full recipe> --dump-tir=<dir>`, read `<dir>/Bounds3D.tir.txt` etc.
- To reproduce the V1 break: `tools/regen-conformance-scalar.ps1 -Test` (property-ful, pins).
- To reproduce the V2 clean compile: `tools/regen-generated.ps1 -Apply` then build
  `Generated/Plato.Generated.Optimized` (`--no-properties`); restore golden with
  `git checkout -- Generated/`.
