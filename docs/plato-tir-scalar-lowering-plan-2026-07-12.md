# Scalar erasure as a TIR lowering pass + type-checker completion — plan

*Written 2026-07-12 for Christopher Diggins (AI-authored, Claude). The post-C4 endgame plan: finish
the type checker so the TIR's types are trustworthy end-to-end, then turn `--scalar` erasure into a
real TIR lowering pass and reduce `TirCSharpBodyWriter` to a dumb pretty-printer. Where it
recommends, it recommends; the decisions are yours.*

---

## Status — execution log (updated 2026-07-12)

Landed increments (all gated: PlatoTests 99/99, `check-all` ALL GATES PASS — goldens byte-identical
184/184 both, conformance 204/204, GeometryTests 15/15):

- **M1.2 (F2 coercion) DONE.** `Solver.TryCoerce` now models the runtime's implicit operators in
  return position: value→single-field struct (`Vector3` → `Translation3D`) and tuple→struct with
  **per-element coercion** (`(ZAxis3D, 0.0)` → `AxisAngle` via `Number→Angle`). Checker diagnostics
  **70 → 49** (CHK101 41→20). Every case it resolved is ground, so return-coercions add no body
  `TirCoerce` (Elaborator only wraps *argument* coercions) → goldens byte-identical, proven.
- **M1.5 (TirTypeVerifier) DONE — and it reframes the problem.** New
  `PlatoCompiler/Checking/TirTypeVerifier.cs` + `PlatoTests/TirTypeVerifierTests.cs` walk every
  emitted body (ground member + generic static). **Measured: exactly 1 hard violation** — `Quad3D.Normal`
  carries an unbound type *variable* in its ground body (R6). **R1 null-type, R2 unresolved, R4
  arg/param mismatch, R5 coerce-inconsistency are all ZERO.** So the TIR the emitter consumes is
  *already* almost fully type-consistent — the "49 diagnostics" headline overstates the risk to
  Mission 2. The remaining untrust is (a) that one residual type var and (b) a **16-name
  syntactic-intrinsic census** (null-callee calls: Add/Lerp/Subtract/Average/Min/Max/Multiply/Scale/
  Normalize/Sum/Map/Clamp/Barycentric/Function/QuadraticBezier/CubicBezier) — which is the *same*
  worklist as the checker's remaining CHK201/203. Ratchet pinned at 1; burn to 0 to unblock M2.
- **M1.4 (ratchet) DONE.** `PlatoTests/CheckerCompletenessTests.cs` gates functions-with-diagnostics
  ≤ 49 (ceiling to lower).

- **M1.1 (F1) SAFE SLICE + optimizer bug fix — DONE, verifier now GREEN.** The concept-method
  `$C2 | $C3` ties (e.g. `Average → a.Lerp(b, 0.5)` on `IInterpolatable`, whose two Lerp candidates —
  the `IInterpolatable.Lerp` obligation and its `IVectorLike`-inherited surfacing — both return a
  fresh unbound Self-var) are **not a real disagreement**: each grounds to the same concrete arg type
  at monomorphization. `Solver.ResolveOverload` now commits the first winner (CHK202) when all
  winner returns are unbound type variables, instead of erroring CHK203. **Result: diagnostics
  49→39 (CHK203 25→14); verifier hard violations 1→0 (`Quad3D.Normal` cleared) — R1/R2/R4/R5/R6 all
  zero, the verifier is GREEN and Mission 2 is unblocked on the type-consistency axis.**
- **Optimizer bug found + fixed (the real gate).** Landing the tie fix first *failed to compile*:
  the now-resolved `Lerp`/`Average` become inlinable, and `Solids.NGonPoint`/`SquarePoint` emitted
  `var _var250 = f;` above `f`'s own mid-body declaration (CS0103). Root cause = a pre-existing bug in
  **`TirLambdaCaptureRewriter`**: it hoisted a lambda-captured reference by wrapping the *entire*
  body, valid only for captured PARAMETERS (in scope at the top), not captured LOCALS declared
  mid-body — and it fired even when the lambda is loop-lowered (inlined into a `for`, where the local
  is already in scope). **Fix: only hoist captured parameters; leave captured locals in place** (C#
  closures capture them correctly; the by-value snapshot is never needed in this pure
  single-assignment language). **Byte-neutral on existing goldens** (they only ever hoisted params —
  proven 184/184 both) — so it is a safe standalone cleanup that also unblocks the tie fix. With both
  landed, the 25 improved Optimized goldens (`NGonPoint` etc. now fully inlined/unrolled) **compile**,
  Unoptimized stays byte-identical, and `check-all` is ALL PASS.

  **This episode is the plan in miniature, and it relocates the risk:** a correct front-end
  improvement was unlandable purely because a back-end pass was unsafe. The real gating risk for
  Mission 2 is not the checker front-end (R1/R2/R4/R5 are already zero) but the **optimizer
  interactions** (inliner + capture-rewriter + loop-lowerer) — resolving *more* calls feeds *more*
  code through those passes and trips latent bugs. Expect S1–S2 to surface similar back-end fixes.

- **M1.1 (F1) COMPLETE — all CHK203 ambiguity errors eliminated (25 → 0).** Two further
  `Solver.ResolveOverload` refinements, both golden-neutral (184/184 both, verifier still hard=0,
  conformance 204/204):
  1. **Bounded-polymorphic ties defer to monomorphization.** A tie whose arguments still contain an
     unbound *constrained* type variable — e.g. `x.Max - x.Min` in `IBounds.Size`, receiver type
     `$TValue : IVectorLike, IDifference<$TDelta>` — is not a real ambiguity: the concrete overloads
     only "tie" by illegitimately binding the abstract variable to each concrete type, and
     monomorphization dispatches the call by the grounded argument type where it is unambiguous. Now
     emitted as an info (CHK204) and deferred (name+shape), not CHK203. Cleared 13 of the 14.
  2. **C#-style specificity tie-break.** Among equal-cost winners, the most-derived parameter type
     wins (`Map(IArray2D<T>)` over `Map(IArray<T>)` for `QuadGrid3D.PointGrid.Map(f)`). Cleared the
     last CHK203 (`Meshes.Deform`) and, as a bonus, its dependent CHK101 return-coercion (32/859
     total now; the remaining 32 are all CHK101 cannot-unify + CHK201 no-match, i.e. F2a
     tuple→generic-interface returns and library repairs — not ambiguities).
  Checker ratchet tightened to ≤ 32.

- **Mission 2 STARTED — S1 (TirScalarLowerer substitution core) DONE.**
  `Plato.CSharpWriter/TirScalarLowerer.cs` is the pure type-substitution half of the pass: it maps
  the five wrapper `TypeDef`s to synthetic null-scope primitive `TypeDef`s
  (`float/int/bool/char/string`, created once — the same shape `SymbolFactory` uses for its built-in
  `Any`) throughout a `TirFunction`'s **types** — every node `Type`, `TirCall` param/return types,
  `TirCoerce` endpoints, `TirNew.NewType`, generic arguments, the signature and zonked types, and the
  optimizer marker / lowered-loop types. It is parameterized by a `scalarMap`, so `--scalar=double`
  is a different map, not a different pass (S4). It never rewrites tree shape or inserts/drops
  coercions — coercion normalization is the separate second half (part of S2). Five unit tests
  (`PlatoTests/TirScalarLowererTests.cs`), including "lowering a real ground TIR leaves NO wrapper
  type in any node's type tree". **Unwired → zero output risk; goldens byte-identical by
  construction.** PlatoTests 104/104.

- **S0 (type the optimizer marker nodes) DONE.** `TirComponentAccess`/`TirConstructorCall`/
  `TirBooleanChain` now carry a semantic `TypeExpression` (component Plato type / constructed type /
  `Boolean`), set by `TirComponentUnroller` and preserved through `TirRewrite` + `TirScalarLowerer`.
  The legacy `CastTo`/`ScalarComponentPrim` strings stay authoritative (the printer still reads
  them), so it is pure plumbing — goldens byte-identical, PlatoTests 104/104.

- **S2 DIAGNOSTIC — the flip's real challenge is overload disambiguation, quantified.** A reverted
  experiment (wire `TirScalarLowerer.Lower` at the end of `RunOptimizerPasses`, type substitution
  ONLY, leaving the current printer) regenerated the full Optimized recipe and built it: **974
  compile errors, every one CS0121 (ambiguous call)** — and ZERO `new float(...)` / type-land
  breakage. This is the decisive finding for Mission 2: the `((float)…)` casts `ScalarEraseAnalysis`
  emits are **overwhelmingly for overload disambiguation** (pinning C# between a `float` overload and
  a `Number` overload), not for erased-type correctness. C#'s implicit `float↔Number` conversions
  already handle type-land; what they do NOT handle is choosing between two applicable overloads, so
  dropping a cast turns an exact bind into an ambiguous one.

  **Consequence for the design:** the lowerer's second half is not merely "insert a coerce where
  `arg.Type ≠ param.Type` at the wrapper boundary." It must insert a **disambiguating** coercion
  wherever the erased call site would otherwise be ambiguous between scalar and wrapper overloads —
  exactly the knowledge `ScalarEraseAnalysis.MatchingScalarOverloads` / `Writer.ScalarOverloads`
  encodes today. So S2 is: (1) extend `TirScalarLowerer` with an overload-aware coercion-insertion
  pass that consults the overload surface and pins each argument/receiver whose erased type would be
  ambiguous, and drops identity casts everywhere else; (2) wire it; (3) flip the printer to render
  node `Type` + explicit `TirCoerce` and delete the 16 decision sites; (4) delete `ScalarEraseAnalysis`
  (S3). The gate is compile (974 → 0 CS0121) + conformance 204/204 + cast census, reached by
  iterative regen/compile cycles. This is the large, subtle heart of the mission; the S0+S1
  foundation it needs is in place and committed.

- **S2 IN PROGRESS — the overload-aware coercion pass works on ground bodies; gated off behind
  `UseScalarLowering` (default false, goldens byte-identical).** Built and wired (dormant):
  - `TirScalarLowerer.InsertScalarCoercions` — wraps each scalar argument in a coercion to its
    recorded parameter type, read straight off `TirCall.ParameterTypes`. This **eliminated all 974
    CS0121 overload ambiguities** — the disambiguation the whole scheme turns on. Guards: never coerce
    a `TirTypeRef`/`TirName` (a type used as a static receiver — CS0119), and only coerce a value whose
    *stripped* type is genuinely scalar (a non-scalar arg at a loosely-typed scalar param — the
    bounded-poly `Subtract($TDelta,…)` case — must not be cast, CS0030).
  - `TirScalarLowerer.RestoreWrapperAtBroadcasts` (post-substitution) — a scalar primitive at a
    CONCRETE non-scalar parameter is re-wrapped to `Number` (`float → Number → Vector2`, which C#
    won't chain implicitly — CS1503).
  - `TirCSharpBodyWriter` type-directed rendering under `_lowered`: literals by their erased type,
    scalar `TirNew` as a cast, and **`TirCoerce` rendered as an explicit cast when its target is a
    scalar primitive/wrapper, suppressed otherwise** — the whole erased cast policy, with
    `ScalarEraseAnalysis` never built. Tested on ground member bodies (`TirScalarLowererTests`).
  - **The remaining blocker (why it's gated off): generic STATIC library bodies.** With the flip on,
    the full recipe builds down to **58 (Unopt) / 82 (Opt)** errors, ALL in unspecialized library
    bodies (`IBoundsLibrary`, `Algebra`, …) where nodes carry `null`/type-variable types (`value:$T`),
    so the type-directed insertion can't tell scalar from non-scalar and both over-casts (CS0030) and
    misses broadcasts (CS1503). Member (ground) bodies are fully typed and work. **Next step:** route
    static/generic bodies through the legacy `ScalarEraseAnalysis` path (keep it for statics only) OR
    make the elaborator type generic static bodies; then enable `UseScalarLowering`, drive Unopt+Opt
    to 0 errors, refresh goldens (cast census), and confirm conformance 204/204 before S3 deletes the
    now-unused analysis. The machinery is committed and green; flipping the flag is the remaining work.

- **S2 UPDATE — the blocker is precisely Mission-1 type fidelity, not the emit path.** Added a
  robust safety envelope so the flip only lowers bodies it can trust, all still gated off
  (byte-identical 184/184): `TirScalarLowerer.IsGroundBody` now also refuses a body with any
  UNTYPED value node or with a **scalar-parameter/concrete-non-scalar-argument mismatch** (the
  `Between(Number, …)` receiving a `Point2D` looseness that survives monomorphization in generic
  library bodies); `WasLowered` lets the printer key its type-directed mode off what the pass
  actually did; `RunOptimizerPasses(…, lowerScalars)` keeps static-emit bodies on the legacy path.
  With the flag on, these cut Optimized from 82 → **46** errors. The residue (Unopt 58 / Opt 46) is
  entirely in **generic library bodies** (`Algebra.CatmullRom`, `IBounds`/`IInterval` helpers,
  `Meshes`) whose monomorphized TIR still types a `Point2D`/`Vector2` receiver as its concept bound
  `Number` — the SAME looseness as the remaining 32 checker diagnostics. **So Mission 2's last mile
  is gated on Mission 1 finishing: until concept-method calls in library bodies carry their concrete
  types, those bodies cannot be type-directed and must stay on the legacy path.** Options to finish:
  (a) tighten the checker so monomorphized library-body calls record concrete (not concept-bound)
  argument/parameter types; or (b) permanently route the ~4 offending library classes through the
  legacy `ScalarEraseAnalysis` path and enable the flip for everything else (a smaller, shippable
  win that keeps `ScalarEraseAnalysis` alive for the residue rather than deleting it in S3). The
  machinery is complete, tested, and committed; only the enable-decision + the residue remain.

- **S2 COMPLETE — the flip is ENABLED BY DEFAULT (`UseScalarLowering = true`); both goldens compile,
  conformance 205/205, `check-all` ALL GATES PASS, PlatoTests 105/105.** The last-mile type fidelity
  was closed with a mix of the *real fix* (Mission 1 checker/monomorphizer work) and a targeted
  legacy fallback for a residue. What landed:
  1. **Checker — rigid signature variables (`Solver`/`TypeChecker`).** A function's own signature
     type variables (`$T`, `$D`) are now RIGID during overload trial/best-effort matching
     (`record == false`): they can no longer absorb a concrete parameter type, so
     `value.Between(x.Min, x.Max)` on `value: $T` binds the `IVectorLike` concept overload instead of
     poisoning `$T := Number` and casting a `Point2D` to `(float)`. Fixes the IBounds/IInterval
     family cleanly (now lowered correctly). Diagnostic count unchanged (32), all checker tests green.
  2. **Monomorphizer — concept-parameter grounding (`GroundConceptParameters` + Self-refined return
     grounding).** A concept method records every Self position as its owning interface (`Add`'s
     params as `IArithmetic`, `INumerical`); once the arguments are concrete, that interface IS the
     Self type, so each self-constrained-interface parameter/return is grounded to the concrete
     argument (receiver type for a scalar broadcast). This drove the `Algebra`/`Meshes` INumerical
     bodies from 24 (Opt) errors to 0 — the writer now sees `Vector2`/`Number`, not the loose
     interface. The broadcast stays IMPLICIT (no inserted coercion — a coercion node interacts badly
     with the component unroller); the verifier's rule 4 accepts a scalar arg at a concrete non-scalar
     param as a legitimate implicit broadcast (`TirTypeVerifier` stays 0 hard).
  3. **Writer — type-directed broadcast + precedence + the root `WasLowered` bug
     (`TirCSharpBodyWriter`/`TirScalarLowerer`).** `TirCoerce` rendering now restores the wrapper at a
     scalar→concrete broadcast (`((Number)(0f))` at `CreateWorld`'s `Vector3`), parenthesizes a
     conditional inner of a cast (`((int)(c ? a : b))`, was `((int)c ? a : b)` — a real bug), and
     `RestoreWrapperAtBroadcasts` skips a scalar-receiver call. **The decisive fix:** `WasLowered`
     keyed off wrapper-named coercions (`ContainsKey`), so the inliner's transparent
     `coerce<Number→Number>` tags made it return `true` on bodies routed to the LEGACY path — flipping
     the printer into type-directed mode on un-lowered bodies and rendering the tags as an ambiguous
     `Number.Multiply(Number)`. It now keys only off erased PRIMITIVES; this was the entire 41-error
     conformance witness failure.
  4. **Fallback residue (option b).** `IsGroundBody` routes to legacy any body whose call has a
     scalar arg at an ungrounded INTERFACE parameter, or a scalar-receiver call with a scalar arg at
     a concrete non-scalar parameter (a corrupted scalar op inherited via inlining). `ScalarEraseAnalysis`
     is therefore KEPT — **S3 is intentionally NOT done** (deleting it would break the residue path).
  Cast census (goldens, before flip-off → after flip-on): Unopt `((float)` 2439→2988, `((Number)`
  87→96; Opt `((float)` 3956→5501, `((Number)` 99→104 — the type-directed path emits an explicit
  disambiguating `TirCoerce` per scalar argument (more casts, all node-derived, no string inspection),
  the documented S2 trade-off. Emit snapshot re-baselined (parenthesization only).

- **S3 DIAGNOSTIC — the residue is far larger and more structural than "a few corrupted bodies".**
  Instrumented the flip-on Optimized recipe (stderr-logged every body the printer still builds
  `ScalarEraseAnalysis` for, and every `IsGroundBody` rejection). Findings:
  - **554 bodies still emit via the legacy `ScalarEraseAnalysis` path** (not a handful).
  - **`IsGroundBody` rejects ZERO of them** — the agent's `corruptscalarop`/`loosescalarparam`/
    `ifacebroadcast` exclusions are effectively DEAD CODE in practice. Bodies go legacy purely because
    `WasLowered(tir)` is false: either the body was never offered to lowering (`lowerScalars=false`
    for STATIC-emit bodies in `CSharpTypeWriter`), or it has no erased-primitive type after lowering.
  - Of the 554, only **~52 actually carry scalar content** in their signature; the other ~500 are
    **scalar-free** (Vector→Vector etc.) and would render *identically* type-directed (the printer's
    `_scalar` branches all no-op when there are no scalar params). The ~52 real ones are dominated by
    `MapComponents`/`ZipComponents` HOFs on scalar-component types (`Integer2/3/4`, `Time`) and static
    constants — **generic/static library bodies emitted from UNSPECIALIZED (type-variable) TIR**, which
    is exactly why they can't lower.
  So **S3 is not a cleanup — it is a structural task**: deleting `ScalarEraseAnalysis` requires the
  type-directed printer to render (a) the ~500 scalar-free legacy bodies — likely low-risk, they need
  no scalar decisions — and (b) the ~52 scalar-bearing STATIC/generic/HOF bodies, which first need the
  static-emit surface to be typed/monomorphized enough to lower (the same Mission-1 fidelity gap, one
  level deeper). Recommended next steps, in order: (1) route scalar-free bodies through the type-directed
  printer and prove goldens byte-identical (shrinks the residue ~500→0 of the harmless kind); (2) type
  or monomorphize the static/HOF surface so the ~52 lower; only THEN delete `ScalarEraseAnalysis`.
  Attempting the deletion before (1)+(2) would break the currently-green shipping flip. Left at the
  verified-green checkpoint (Plato `4b2110b`); no code changed this pass — the instrumentation was
  reverted.

- **S3 STEP 1 DONE — the `_lowered` signal is now a real per-body flag; scalar-free bodies render
  type-directed.** The printer keyed `_lowered` off `TirScalarLowerer.WasLowered(tir)` (type-sniffing
  for an erased primitive), so a SCALAR-FREE ground body — which lowers to a tree with no erased
  primitive — fell to the legacy `ScalarEraseAnalysis` path even though the pass had run on it. Fix:
  `RunOptimizerPasses` now returns (via `out bool lowered`) whether `TirScalarLowerer.LowerWithCoercions`
  actually RAN; the three body-emit sites thread that into `TirCSharpBodyWriter`'s new `lowered` ctor
  arg, and `_lowered` reads it directly. `WasLowered` is now unused (deleted in the final step). Result:
  **Optimized golden byte-identical (184/184)**; **Unoptimized: 6 files re-baselined** — a purely
  semantic-neutral cleanup where the type-directed printer drops the legacy path's unconditional
  eta-expansion of a bare function argument (`x.Components().Map((_e0) => f(_e0))` → `x.Components().Map(f)`)
  across the `MapComponents`/`ZipComponents`/`Deform` library bodies of Integer2/3/4, Time, Point2D,
  Bounds2D/3D, the mesh types, etc. This is CONSISTENT with the Optimized golden, which already emits
  the bare form (`.MapComponents(f)`, `.Map(f)`, `.Reduce(initial, f)`, `.Deform(f)`) in its lowered
  bodies — the eta-expansion was legacy noise (the plan §"Intended-output change" predicts eta only for
  a genuine function-typed `TirCoerce`, of which there are none after erasure). Gates: both Generated
  projects compile 0 errors, conformance 205/205, PlatoTests 105/105. Legacy-body residue measured
  next; `ScalarEraseAnalysis` still present (statics remain on it).

- **S3 STEP 2 DONE — the legacy residue is driven to ZERO in both recipes; every emitted body now
  lowers.** Measured after step 1: **197 legacy bodies, 100% static** (Step 1 cleared every member
  body). They stayed legacy only because `CSharpTypeWriter.WriteBody` called `RunOptimizerPasses` with
  `lowerScalars: isMember` (false for statics). Two changes drove 197 → 0:
  1. **Lower static bodies too** — `WriteBody` now passes `lowerScalars: true`; `IsGroundBody` inside
     `RunOptimizerPasses` still gates it, so a genuinely un-lowerable static stays legacy while the
     ~194 ground constants (colors, axes, `Pi`, `SqrtTwo`, …) lower. This alone cut 197 → 1 (Opt) / 3
     (Unopt); only 2 golden files changed (Constants.g.cs colors + Extensions.g.cs indexer args each
     gain the documented S2 disambiguating `((int)`/`((bool)` cast).
  2. **Relax `IsGroundBody`'s over-cautious guards** — instrumented rejection reasons showed exactly 3
     residual functions, blocked by guards that route to a legacy path S3 must delete:
     `AllQuadFaceIndices<$T>` (a signature type VARIABLE `$T` in a param type — but a type variable is
     inert to scalar lowering, never a wrapper, so it is not a blocker) and `TwoPi`/`HalfPi`
     (`Pi.Twice()`/`Pi.Half()` — a scalar `Number` at `Twice`'s `INumerical` param, a legitimate scalar
     op, not a vector broadcast). Dropped the type-variable rejections and the scalar-arg-at-interface
     guard (both dead for every MEMBER body — 0 rejections there — and only these 3 statics hit them).
     Also admit an untyped ZERO-ARG call (a bare constant reference the static elaboration leaves
     untyped) since it renders through the type-independent `Constants.X()` path. Residue → **0/0**.
     All 3 render byte-identically to their legacy form (`Constants.Pi().Twice()`, the `AllQuadFaceIndices`
     casts). Gates: both Generated projects compile 0 errors, conformance 205/205, PlatoTests 105/105,
     goldens applied (2 files/recipe). `ScalarEraseAnalysis` is now UNUSED for emission — the deletion
     (S3 final) is a pure code cleanup with byte-identical output. Cast census (step-1 golden → now):
     Unopt `((float)` 2988→3024, `((int)` 324→756, `((bool)` +159; Opt `((float)` 5501→5549,
     `((int)` 748→1180 — the +432 `((int)` is the per-component disambiguation on the ByteRGB color
     constants.

- **S3 COMPLETE — `ScalarEraseAnalysis` DELETED; `grep ScalarEraseAnalysis` over the `.cs` source is
  0.** With the residue at 0/0 the legacy emit-time analysis was unused, so it was removed outright:
  - `Plato.CSharpWriter/ScalarEraseAnalysis.cs` (the whole 307-line file) deleted.
  - `TirCSharpBodyWriter`: every `_scalar`-gated decision site removed (the `_scalar` field, the
    per-lambda `ScalarEraseAnalysis` rebuild in `TirLambda` and `WriteLoweredLoop`, `WriteCall`'s
    scalar-return wrapping, the overload-pinning + wrapper-restore arg-cast logic, `AuthoritativePrim`/
    `NodeScalarPrim`/`RestoreCastType`/`WriteScalarCastArg`/`Render`/`IsWholeScalarCast`/`WriteCallArg`
    helpers, the `_pendingLambdaParamPrim` plumbing). The printer is now purely type-directed under
    scalar erasure. A body that reaches emission un-lowered THROWS (there is no fallback), so a future
    un-lowerable stdlib addition fails loudly instead of rendering mis-typed. Stale class doc rewritten.
  - `CSharpWriter`: `ScalarOverloads`/`ScalarMemberNames`/`HandwrittenScalarExtensionMethodNames`
    fields + their construction deleted; the `UseScalarLowering` flag folded in as unconditional
    (`ShouldLower` is now `ScalarErase && lowerScalars && IsGroundBody`). `TirScalarLowerer.WasLowered`
    (the old type-sniffing signal, superseded by the threaded `lowered` flag) deleted.
  - The inliner's scalar coercion-tag block was KEPT (it now feeds `TirScalarLowerer`'s coercion
    insertion, not the deleted analysis) with its comment corrected; `WriteBareName`'s dead
    `ScalarMemberNames` clause removed (the `--no-properties` clause already covers those names).
  - **Intended-output change (NOT byte-identical): 12 (Unopt) / 28 (Opt) golden files got STRICTLY
    CLEANER casts.** Removing the per-lambda legacy analysis means lambda bodies (inside `ZipComponents`
    and loop-lowered HOFs) now render type-directed from the lowerer's coercions instead of the legacy
    path's over-casting: e.g. `((float)a1).Lerp(((float)b1), ((Number)(((float)_var14))))` →
    `((float)a1).Lerp(((float)b1), ((float)_var14))` and `RoundToZero(((Integer)(((int)digits))))` →
    `RoundToZero(digits)`. Exactly the "strictly cleaner output" §"Intended-output change" predicts.
  Gates: `grep` 0, both Generated projects compile 0 errors, conformance 205/205, PlatoTests 105/105,
  `check-all` ALL PASS. Mission 2 done. Remaining `ScalarEraseAnalysis` mentions are historical (dated
  plan docs, PROGRESS.md) and stale build artifacts, not source.

Not yet started: **M1.3** (F2d/F3 library repairs — note `Vectors.Column1-4` "fix" is a trap: making
the source explicit `m.M11` makes the *checker* type it but the emitter then adds redundant
`((float)…)` casts, an interim output regression Mission 2 is meant to remove; better fixed by
teaching the checker bare receiver-member refs, or deferred into M2). The concrete-return CHK203 ties
(`Subtract`/`Add`/`Lerp` `Vector2 | Vector3 | …` on concept-constrained receivers, ~14) are the
harder, still-open part of F1 — they emit by name+shape and do **not** corrupt emitted TIR types, so
they don't gate Mission 2. **M2/S0–S4** not started; the verifier gate it waits on is now green.

---

## Executive summary

Two missions, strictly ordered:

1. **Mission 1 — finish the type checker.** Measured today: **70 / 859** stdlib functions carry
   located error diagnostics (`CheckerDiagnosticsSummaryTests`): **CHK101 × 41, CHK203 × 25,
   CHK201 × 13**. These fall into four fix families (§M1), mostly solver work plus a handful of
   library repairs. Exit is not just "0 diagnostics": it is a new **`TirTypeVerifier`** gate (§M1.5)
   that mechanically asserts every emitted TIR body is fully typed and internally consistent —
   the precise definition of "the emitter can trust the types."

2. **Mission 2 — `TirScalarLowerer`.** A single writer-side `TirFunction → TirFunction` pass that
   substitutes the five scalar wrappers to their primitives in the TIR's *types*, normalizes/inserts
   `TirCoerce` nodes exactly at genuine conversion boundaries, and lets every downstream consumer —
   including the printer — be type-directed with **zero decisions**. Then delete
   `ScalarEraseAnalysis` (307 lines) and all ~16 scalar decision sites in `TirCSharpBodyWriter`,
   including the `WriteScalarCastArg`/`IsWholeScalarCast` string-inspection band-aid.

Why this order: the current emitter consults the **origin symbols** (the legacy expression graph)
for every scalar decision precisely because TIR node types are not yet trustworthy — the solver
unifies `Self` permissively, ties produce loose types, unresolved calls elaborate as name+shape
"syntactic" calls, and `AuthoritativePrim` explicitly documents that "zonked node types can be
looser than the emitted surface" ([TirCSharpBodyWriter.cs:553](../submodules/Plato/Plato.CSharpWriter/TirCSharpBodyWriter.cs)).
A lowering pass that compares `node.Type` against its context's expected type is only sound once
those types are right on every node of every emitted body. Close the checker first.

This is the plan C4 pointed at: *"`ScalarEraseAnalysis` (shrinks to a pure type substitution under
the uniform surface; delete what remains context-sensitive)"*
([consolidation plan](../submodules/Plato/docs/plato-consolidation-plan-2026-07-12.md), C4 original brief).

## Current state (evidence)

- **TIR is the sole C# body writer** (C4 done): `TirCSharpBodyWriter` renders every body; the
  legacy `CSharpFunctionBodyWriter` is deleted. One recipe (V2: extensions + `--scalar=float` +
  `--no-properties`, optimizers opt-in).
- **`TirCoerce` already exists** and the elaborator already wraps solver-detected implicit
  conversions ([Tir.cs:169](../submodules/Plato/PlatoCompiler/Checking/Tir.cs)). The pass
  infrastructure (`TirRewrite.Rewrite`, post-order, covers all node kinds incl. optimizer markers)
  already exists ([TirRewrite.cs:36](../submodules/Plato/Plato.CSharpWriter/TirRewrite.cs)).
- **But scalar erasure is emit-time analysis**: `ScalarEraseAnalysis` answers "which primitive is
  this expression" over origin *symbols*, and `TirCSharpBodyWriter` consults it at every scalar
  decision (inventory below). The inliner adds transparent scalar `TirCoerce` *tags*
  ([TirInliner.cs:230-243](../submodules/Plato/Plato.CSharpWriter/TirInliner.cs)) and the unroller
  carries `CastTo`/`ScalarComponentPrim` *strings* on untyped marker nodes
  ([TirComponentUnroller.cs:17-42](../submodules/Plato/Plato.CSharpWriter/TirComponentUnroller.cs))
  purely to feed that analysis — three parallel channels re-deriving what the types should say.
- **Gates available** (as of today): frozen-V1 tripwire · `regen-generated.ps1` golden diff
  (2 goldens, 184 files each, must compile) · conformance 205/205 · PlatoTests (incl. the
  emit-snapshot suite, ~30 pinned bodies, seconds-fast) · `check-all.ps1`.
- **In flight** (PROGRESS.md): C1 (one recipe / `--modern`) pending; C3 (M5 runtime port) mid-flight
  (Vector2/3/4/8, Matrix3x2/4x4, Quaternion, Plane remain); optimizer closeout pending (Plan 1 P0).

### Inventory: every scalar decision that Mission 2 deletes

In `TirCSharpBodyWriter` ([file](../submodules/Plato/Plato.CSharpWriter/TirCSharpBodyWriter.cs)):

| # | Site (≈line) | Decision it makes today | Type-directed replacement |
|---|---|---|---|
| 1 | ctor 71, field 55 | build/hold `ScalarEraseAnalysis` | — (deleted) |
| 2 | `WriteBareName` 109-111, 130-136 | `()` on scalar extension members; `((float)Number.Zero)` wrapper-static normalization | uniform name rule; `TirCoerce` node |
| 3 | `TirLiteral` 305-311 | `0.5f` vs `((Number)0.5)` | literal's erased `Type` |
| 4 | `TirParameter` 318-326 | `((float)x)` / `((float)this)` on scalar params | bare name — param signature is already the primitive; boundary coercions are explicit nodes |
| 5 | `TirCoerce` 377-389 | broadcast wrapper restoration `((Number)(…))` | coerce node's own From/To |
| 6 | `TirNew` 410-418 | `new Number(x)` → cast | erased `NewType` |
| 7 | `TirLambda` 455-466, 485-486 | per-lambda analysis swap | — (lambda param types are in the TIR) |
| 8 | `NodeScalarPrim` 545, `AuthoritativePrim` 554-589 | re-derive a node's primitive from markers/tags/origins | `node.Type` |
| 9 | `WriteCall` 597-604 | `((float)…)` around group-scalar-return calls | `TirCoerce` at the call's use site |
| 10 | no-paren clause 701-705 | `()` on no-arg member of provably-scalar receiver | receiver `Type` is a primitive + uniform name rule |
| 11 | `_pendingLambdaParamPrim` 60, 729-734, 875-877 | thread HOF element prim into lambda bodies | lambda `ParameterDef`/`TirParameter` types |
| 12 | overload pinning 739-768, 776-794 | pick all-scalar overload, cast args to declared prims | callee's erased `ParameterTypes` → `TirCoerce` per mismatched arg |
| 13 | `RestoreCastType` 1002-1032 | primitive vs wrapper restoration cast per arg | same as #12 |
| 14 | `WriteScalarCastArg`/`Render`/`IsWholeScalarCast` 1040-1081 | collapse `((float)((float)x))` by string inspection | never generated — identity coercions are dropped in the pass |
| 15 | `WriteCallArg` 1086-1102 | eta-expand function-typed args (delegate variance bridge) | function-typed `TirCoerce` renders as eta-expansion |
| 16 | `WriteLoweredLoop` 850-863 | per-lambda analysis swap inside lowered loops | — |

Plus, outside the body writer:

- `ScalarEraseAnalysis.cs` — the whole file.
- `TirInliner.cs` ≈230-243 — the scalar `TirCoerce` tag hack ("the emitter's scalar analysis cannot
  see that this position is Number — the tag carries it").
- `TirComponentUnroller.cs` 17-42, ≈224-228 — `CastTo`/`ScalarComponentPrim` strings and the
  `writer.ScalarErase` fork (markers get real `Type`s instead, §M2 step S0).
- `CSharpWriter.cs` — `ScalarMemberNames` (≈193), `ScalarOverloads` (≈201),
  `HandwrittenScalarExtensionMethodNames` (≈207) and their construction; consumed only by the
  deleted decisions.
- `CSharpFunctionInfo` / `ToFunctionInfo` `lambdaParamPrim` plumbing.
- The stale `TirCSharpBodyWriter` class doc-comment ("EXPERIMENTAL — OFF THE DEFAULT
  CODE-GENERATION PATH", "NOTHING in the production pipeline constructs this type" — false since
  C4; rewrite it regardless of the rest of this plan).

**What stays untouched:** signature/structure-level erasure — `CSharpTypeWriter.EraseScalars`
type-name rendering, `CSharpConcreteTypeWriter.WriteScalarErasedType`, `ExtensionStyleWriter`'s
erased-receiver handling. Types were always *rendered* erased; the mission replaces body-level
*decisions*, not the type-name mapping.

---

## Mission 1 — finish the type checker (70 → 0)

Measured 2026-07-12 (`CheckerDiagnosticsSummaryTests.SummarizeUnresolvedStdLibDiagnostics`):
70/859 functions, 79 error diagnostics: CHK101 (unify clash) 41 · CHK203 (ambiguous call) 25 ·
CHK201 (no match) 13. Top names: `Tuple2/3/4` 25 · `Lerp` 7 · `Average` 5 · `Subtract` 4 ·
`Scale` 4 · `Add` 3. The per-function detail clusters into four families:

### F1 — concept-dispatch ties on concept-constrained receivers (kills most CHK203, ~25)

`IBounds.Size: Ambiguous call to 'Subtract': 12 equally-specific overloads…`,
`IInterval.MoveTo: 'Add': 12 overloads…`, `Geometry.Normal: 'Normalize': 5 overloads…`, and the
`$C`-variable ties (`Core.Average: 'Lerp': $C2 | $C3`). Pattern: a call on a receiver whose type is
a concept-constrained type variable (or interface) matches *every* concrete implementor equally.
The solver should not enumerate concrete overloads into a tie here: when the receiver is
concept-constrained and the name resolves within the concept (or all candidates are instances of
one concept method), **bind the concept method and defer concrete dispatch to monomorphization** —
the same `Self`-refinement machinery that already exists for concept methods. The `$C | $C` ties
are the same root: two candidate rows differing only in unresolved constraint vars should unify,
not tie.

### F2 — value/tuple → struct coercion gaps (kills most CHK101, ~41)

Three sub-shapes, all in the solver's coercion relation (return *and* argument positions):

- **(a) tuple → struct through a generic/interface target**: `IInterval<$T>` vs `Tuple2<…>` (×9),
  `IBounds<$T,$D>` vs `Tuple2<…>` (×4), `QuadGrid3D` vs `Tuple3<…>`. The tuple→same-shape-struct
  cast exists but fails when the declared target is an interface or still contains vars; it needs
  to either defer until the target grounds or check shape against the *implementing* struct.
- **(b) value → single-field wrapper struct**: the `Transforms.*` block (×17) — `Matrix4x4` vs
  `Transform3D`, `Vector3` vs `Translation3D`/`Scaling3D`/`LookDirection3D`, `Quaternion` vs
  `Rotation3D`, `Tuple2<Vector3,Number>` vs `AxisAngle`, etc. These are 1-field (or same-shape)
  semantic wrappers; the value→struct leg of the cast relation needs to accept them (they already
  emit correctly via C# implicit conversion — the checker just can't prove it).
- **(c) component-wise widening inside a tuple coercion**: `Tuple3<Number,Number,Integer>` vs
  `Vector3`/`Point3D`, `Number` vs `NumberInterval`. The shape matches but a component needs its
  own `Integer→Number` coercion; make the tuple→struct check recurse into the coercion relation
  per component instead of requiring equality.

- **(d) genuine library bugs surfaced by the checker — fix in `stdlib-legacy`, not the solver**:
  `Vectors.Column1-4` (`Tuple4<Function1<Matrix4x4,Number>,…>` vs `Vector4` — point-free field
  references that never applied the functions), `Meshes.Lines` (`Point3D` vs `Line3D`),
  `Constants.UnitCircle`, `Geometry.To3D`'s `PolyLine3D` case. Each is a small `.plato` edit and,
  since these compile today only because emission is name+shape, each needs a conformance witness.

### F3 — no-match at argument positions + missing declarations (CHK201, ~13)

`Meshes.Scale/ScaleX/Y/Z` (`Tuple3` literal at a `Vector3` parameter — argument-position tuple
coercion, same fix as F2c), `Curves2D/3D.Eval` Bezier calls (`Vector2` receiver vs `Point2D`
params — decide: overloads in `stdlib-legacy` or a Point/Vector coercion policy),
`AngularCurves3D.Eval` (`Multiply(Point3D, Tuple3)`), `ScalarFields3D.Eval` (`Function(ScalarField3D,
Vector3)` — function-valued field invocation, Earcut-adjacent), `Vectors.Dot` (`Sum(IVector)` —
declare `Sum` for `IVector` or route through `Components`), `Geometry.Barycentric` (self-call with
mixed Point/Vector args).

### M1 increments and gates

| Inc | Content | Gate |
|---|---|---|
| M1.1 | F1 solver change (concept dispatch defers to mono) + minimal `.plato` regression tests | summary count drops ~25; conformance + goldens unchanged-or-reviewed; PlatoTests green |
| M1.2 | F2a-c coercion relation (recursive, interface-aware, both positions) | count drops ~35 incl. most `Scale`/`Tuple` CHK201s |
| M1.3 | F2d + F3 library repairs, one commit per cluster, each with a conformance witness | count → 0 |
| M1.4 | **Ratchet**: `CheckerCompletenessTests` asserts `functions-with-diagnostics == 0` (start as `<= current` and tighten per increment); promote into `check-all.ps1` | permanent |

Two cautions carried over from the [refactoring recommendations](plato-refactoring-recommendations-2026-07-12.md)
(P1 "Checker completeness"): prioritize the coercion/dispatch cases *exercised by real content*,
and tighten the deliberately-permissive holes (`Self` unification, syntactic fallbacks) only as
tests permit — permissiveness that never reaches an emitted body is not on this critical path.
Note the count drifts with library growth (68/823 on 2026-07-10 → 70/859 today): the ratchet
should track the *count*, not a fixed list.

## Mission 1.5 — `TirTypeVerifier`: define "trustworthy" mechanically

Zero diagnostics is necessary but not sufficient for Mission 2. Add a verifier pass + test
(`PlatoCompiler/Checking/TirTypeVerifier.cs` + `PlatoTests/TirTypeVerifierTests.cs`) that walks
every TIR actually used for emission (all monomorphized member bodies + all static bodies, both
recipes) and asserts, per node:

1. `Type != null` on every value node (statements exempt);
2. no `TirUnresolved` anywhere;
3. every `TirCall` has `Callee != null` **or** is a declared syntactic intrinsic (census the
   remainder — the list should be short, named, and shrinking as intrinsics get declared in
   `stdlib-legacy/intrinsics.plato`);
4. per call, `Args[i].Type` equals or coerces to the declared/instantiated `ParameterTypes[i]`
   (coercions must already be explicit `TirCoerce` nodes — the elaborator's contract);
5. `TirCoerce.FromType == Inner.Type` and `Type == ToType`;
6. member-instance bodies: fully ground (no type vars); static bodies: vars allowed only where the
   function's own signature is generic.

Report violations grouped by rule with origin locations — this is the *actual* worklist for
"the emitter can trust the TIR," and it directly measures the residue the solver's permissive
`Self` unification leaves behind. Burn it to zero (or to a pinned, named allowlist for rule 3),
then keep it green forever in `check-all`. Mission 2 does not start until this gate exists and
passes.

## Mission 2 — `TirScalarLowerer`

### Design

New pass `Plato.CSharpWriter/TirScalarLowerer.cs` (writer-side — it consults extension plans and
the intrinsic surface; TS/Rust erase natively in their own writers and are unaffected):

```csharp
public static class TirScalarLowerer
{
    // Number→float, Integer→int, Boolean→bool, Character→char, String→string.
    // The map is a parameter: --scalar=double is a different map, not a different pass.
    public static TirFunction Lower(TirFunction tir, CSharpWriter writer,
        IReadOnlyDictionary<TypeDef, TypeDef> scalarMap)
        => ...;
}
```

**Step 1 — type substitution.** Rewrite every `TypeExpression` reachable from the function —
node `Type`s, `TirCall.ParameterTypes/ReturnType`, `TirCoerce.From/To`, `TirNew.NewType`,
`TirLoweredLoop.ElemType`, signature/zonked types, generic args (`IArray<Number>` →
`IArray<float>`) — mapping the five wrapper `TypeDef`s to five **synthetic primitive `TypeDef`s**
(a writer-owned registry; `TypeExpression` requires a non-null `Def`, so primitives need stub
defs). *Exception:* positions on the **kept-wrapper boundary** keep their wrapper types — a call
whose callee is a handwritten intrinsic or struct-kept member with a wrapper signature (derived
from the extension plans + struct-surface contract, the same knowledge
`CSharpConcreteTypeWriter` already uses to emit those signatures). After this step a node's type
*is* its erased emitted type: `float` where erased, `Number` only where the runtime genuinely
traffics in the wrapper.

**Step 2 — coercion normalization/insertion.** One post-order walk comparing each child's type to
its context's expected type (call → callee's parameter types; `TirNew`/`TirConstructorCall` →
field types; `TirConditional` arms → node type; `TirReturn` → function return; HOF lambda →
function-typed parameter's instantiated arg types):

| Situation after substitution | Action |
|---|---|
| types equal | nothing |
| existing `TirCoerce` now identity (`float→float`) | **drop it** — this is what structurally deletes the `WriteScalarCastArg` band-aid |
| primitive ↔ wrapper (either direction), primitive ↔ primitive (`int→float`) | insert/retarget `TirCoerce` |
| function-typed mismatch (`Function1<Number,Number>` vs `Function1<float,float>`) | `TirCoerce` — prints as eta-expansion (delegate types are invariant; today's `WriteCallArg` rule, now a node) |
| non-scalar Plato conversions (broadcast `float→Vector3` etc.) | `TirCoerce` with the wrapper as `FromType` when the operator is wrapper-sourced (today's `TirCoerce` case #5) |
| context type unknown (generic static body position, undeclared syntactic call) | nothing — C# implicit conversion covers it, exactly today's conservative-null behavior |

**Step 3 — the printer goes dumb.** `TirCSharpBodyWriter` renders:
`TirLiteral` by its `Type` (`0.5f` / `0.5` / `"s"`); `TirParameter`/`TirVariable` as bare names;
`TirCoerce` as `(({To})…)` for scalar/struct targets, eta-expansion for function types, or
`ConversionFn` call when one is recorded; `()` on a no-arg member iff its name is not in
`StructSurfacePropertyNames` (the uniform rule — already global-by-name under `--no-properties`).
Every row of the inventory table above is deleted.

### Placement in the pipeline

Run **after** `RunOptimizerPasses` (a "layer 10.5", the last pass before printing)
([CSharpWriter.cs:236](../submodules/Plato/Plato.CSharpWriter/CSharpWriter.cs)). Rationale: the
optimizer passes continue to operate on Plato-typed TIR unchanged, and only one place must
establish the erased invariant. Prerequisite (S0): the unroller's marker nodes must carry real
`Type`s — today `TirComponentAccess`/`TirConstructorCall`/`TirBooleanChain` are constructed with
`base(null, null)` and carry `CastTo`/`ScalarComponentPrim`/`TypeName` *strings*
([TirComponentUnroller.cs:17-74](../submodules/Plato/Plato.CSharpWriter/TirComponentUnroller.cs)).
The unroller knows the component types (it reads the component table) — have it set `Type` like
every other node, and delete the string channels. The inliner's scalar-tag block becomes an
ordinary explicit coercion the pass normalizes; delete the special-case comment and the
`AuthoritativePrim` reader together.

(Optionally, a later increment can move the pass to right after monomorphization so the optimizer
passes themselves see erased types — conceptually cleaner, but it forces every pass to synthesize
erased types and buys little once the marker nodes are typed. Not on the critical path.)

### Intended-output change, not a mimic

Do **not** reproduce today's cast placement byte-for-byte — that's the C4-era mirroring discipline,
and its gates have been replaced by semantic ones. The expected diff is strictly *cleaner* output:

- `((float)x)` disappears from parameter references whose declared C# type is already `float`
  (site #4) — the largest noise class;
- `((float)…)` around calls survives only where the callee genuinely returns the wrapper
  (intrinsic boundary) — i.e., the 174 post-C4 "semantic survivors" stay, the rest go;
- `((Number)(…))` broadcast restorations become explicit `TirCoerce` renderings at exactly the
  same places (same C# meaning, now node-derived).

Verification: a **cast census** script over both goldens (count `((float)`, `((Number)`, per-file)
before/after, attached to the commit message; `regen-generated.ps1 -Apply`; both goldens
**compile**; conformance 205/205; emit-snapshot re-baselined with the diff reviewed
function-by-function (~30 bodies is a reviewable page). The Earcut P0 bugs
("scalar-erased `Integer` array leaks", mixed `Reduce` accumulator typing — see
[recommendations P0](plato-refactoring-recommendations-2026-07-12.md)) get regression tests here:
they are exactly the class of bug an emit-time-analysis miss produces and a type-directed pass
should fix structurally or at least localize to a checker gap.

### M2 increments and gates

| Inc | Content | Gate |
|---|---|---|
| S0 | Type the marker nodes; delete `CastTo`/`ScalarComponentPrim` strings; unroller/lowerer set real `Type`s | goldens byte-identical (pure plumbing); PlatoTests |
| S1 | `TirScalarLowerer` (substitution + coercion normalization) + unit tests on pinned TIRs (`TirRewriteTests` pattern); `--dump-tir` records the new phase | pass-level snapshots; no emit change yet (pass not wired) |
| S2 | Wire the pass into every body-emit site; flip the printer to type-directed rendering; delete inventory sites #2-#16 | cast census + goldens refreshed & compiling + conformance + emit-snapshot review |
| S3 | Delete `ScalarEraseAnalysis.cs`, `ScalarOverloads`/`ScalarMemberNames`/`HandwrittenScalarExtensionMethodNames` + construction, `lambdaParamPrim` plumbing, inliner tag block; fix the stale class doc | `grep ScalarEraseAnalysis` → 0 hits; full `check-all.ps1` |
| S4 | `--scalar=double`: second map + double intrinsics + cross-precision conformance (approved P1 goal — it should be nearly free now) | double golden compiles; float/double differential |

### Risks and gotchas

- **Static bodies are generic** (layer-5 TIR): type variables remain; the pass must be conservative
  there (rule: no coercion where either side is non-ground). Same envelope as today's
  conservative-null analysis, so no output regression risk — but the verifier (§M1.5 rule 6)
  keeps this honest.
- **Syntactic calls** (null `Callee`) have no declared parameter types → no coercions inserted at
  their arguments. Acceptable (C# implicit conversions, today's behavior), but the M1.5 census
  should keep shrinking this set by declaring intrinsics in `intrinsics.plato`.
- **Kept-wrapper boundary is a moving target**: the C3/M5 runtime port is actively shrinking the
  wrapper surface (Angle done; vectors/matrices/quaternion/plane pending). Sequencing M2 after C3
  finishes means fewer boundary cases and less golden churn — recommended, but not a hard
  dependency (the boundary is *derived*, not hardcoded).
- **`--scalar` without `--no-properties`**: decision sites #2/#10 currently have an
  extension-style-without-methods leg. Recommend C1 makes `ScalarErase` imply `NoProperties`
  (one recipe; the combos die), so the type-directed `()` rule is purely the uniform name rule.
  Flagging for your call — keeping the combo costs one extra receiver-type check, not an analysis.
- **Golden churn is large** (C4's cast collapse touched 58+66 files). Mitigation: S2 is one commit
  whose review artifact is the emit-snapshot diff + cast census, not the raw golden diff.

## Sequencing (both missions against in-flight work)

```
P0 closeouts (optimizer recipe/golden docs, per PROGRESS.md)   ← unchanged, first
→ M1.1 – M1.4   checker to zero + ratchet gate                 ← independent of C1/C3
→ M1.5          TirTypeVerifier green                          ← the go/no-go for M2
→ (C3 finish    M5 port of remaining types — recommended here, shrinks the boundary)
→ S0 – S3       TirScalarLowerer + printer flip + deletions
→ S4            --scalar=double rides the new pass
```

## Success criteria

1. `CheckerDiagnosticsSummaryTests`: **0** functions with error diagnostics; ratchet test in
   `check-all` keeps it there.
2. `TirTypeVerifier`: green over every emitted body under both recipes; syntactic-call allowlist
   pinned and documented.
3. `ScalarEraseAnalysis.cs` deleted; `TirCSharpBodyWriter` contains **zero** `ScalarErase`
   conditionals; `WriteScalarCastArg`/`Render`/`IsWholeScalarCast` gone; no string inspection
   anywhere in emission.
4. Every cast in the goldens is the rendering of an explicit `TirCoerce` (or a recorded
   `ConversionFn`); the cast census shows only the intended classes changed; goldens compile;
   conformance 205/205; frozen-V1 tripwire untouched.
5. The optimizer passes and the printer contain no scalar knowledge beyond the shared map; adding
   `--scalar=double` requires a map + intrinsics, no writer logic.
6. Earcut's scalar-erasure bugs have regression tests that pass under the full optimized recipe.
