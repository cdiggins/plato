---
id: plato-308
title: Generated forward-stdlib C# does not compile (85 errors as of 2026-07-30)
type: bug
status: in-progress
priority: p1
effort: L
risk: med
area: plato
sprint: 
created: 2026-07-29
closed:
links: [submodules/Plato/conformance/Plato.ForwardConformanceTests/README.md, tools/regen-forward-conformance.ps1, submodules/Plato/stdlib-tests/foundation.laws.plato, tracker/issues/plato-306.md, tracker/issues/plato-294.md, tracker/issues/plato-291.md, tracker/issues/plato-323.md]
---

> **Merged 2026-07-29:** [plato-294](plato-294.md) was the same bug, filed independently the same
> day by a concurrent session (its 332 = this issue's 324 plus the 8 CS0736 already fixed here).
> 294 is closed as a duplicate; its unique content — the plato-291 provenance, the degraded-body
> regression box, the Stage-2 `-Test` flip, the per-shape measurement discipline, and the
> writer-collision warning — is folded in below.

## Issue

C# codegen of the forward stdlib now **succeeds** (1232 `.g.cs` files), but the generated code
does not **compile**: 324 errors in 25 files. This is the last thing standing between the
forward stdlib and an executing law runner.

The previously documented blocker — `CSharpTypeWriter.WriteBody` throwing
`No ground TIR for bodied AnimationTrack.ValueAt` and aborting all output — is already fixed by
[plato-291](plato-291.md) (Plato `61ad4a3`); the writer degrades to a throwing stub and keeps
writing. Every doc that still described that blocker was corrected on 2026-07-29.

Repro: `.\tools\regen-forward-conformance.ps1 -Codegen`, then
`dotnet build submodules\Plato\conformance\Plato.ForwardConformanceTests -c Release`.

## Impact

No law, witness, or runtime assertion can execute against the forward stdlib. The only forward
gates are `Plato.CLI lint` (structural) and `ForwardStdLibCheckerTests` (type-checker ratchet) —
neither runs a body. So every forward-stdlib behavioural claim is currently backed by inspection
only.

Concretely blocks [plato-306](plato-306.md): 11 affine orientation laws now exist in
`stdlib-tests/foundation.laws.plato` and type-check clean, but cannot run. The
`a.Add(a.Between(b)) == b` orientation for `Point2D`/`Point3D`/`PointN`/`UvCoordinate`/
`UvwCoordinate`/`Instant` is derived from one place now, so a sign slip would hit all six at
once — exactly the failure a law would catch and inspection would not.

## Evaluation 2026-07-29 (measured at HEAD, not read off this issue)

**NOT FINISHED. 2 of 7 `Done means` boxes hold, and both are ones that do not depend on the build.**

Regenerated `stdlib` + `stdlib-tests` at HEAD with the full recipe (1245 `.g.cs` files), then built
`Plato.ForwardConformanceTests`:

| Box | State | Evidence |
|---|---|---|
| build 0 errors, no quarantine | **NO** | **1,460 distinct errors** (2,920 log lines — MSBuild emits each twice; count distinct). Quarantine correctly absent (0 `Compile Remove`). |
| `-Test` runs the law runner, `KnownFailures` populated honestly | **NO** | Cannot run — the suite does not build. |
| Stage 2 flipped to `-Test` gating | **NO** | Still diagnostic: `STAGE 2 BLOCKED` / `exit 2` remain in the script. |
| 11 affine laws execute and pass | **NO** | Cannot run — the suite does not build. |
| Degraded bodies ≤ 44 | **YES** (provisional) | **35**, down from 44. Not final until the build is green. |
| `Ara3D.SDK.ConformanceTests` 0 fail | **YES** | 205/205 passing; both `Plato.Generated` projects compile; goldens byte-identical. |
| README / csproj / script status blocks accurate | **NO** | They describe the 324-error declaration state, which no longer exists. |

**The declaration-layer claim above is confirmed.** Not one CS0315, CS0305, CS0736, CS0535,
CS0557 or CS0562 remains — the entire original 324-error inventory is gone. Everything left is the
body layer:

Distinct, by shape:

- 1153 × CS1061 (no such member on the receiver)
- 145 × CS0030 (cannot convert)
- 54 × CS1503 / 48 × CS1929 (argument type / extension receiver)
- 24 × CS0029, 15 × CS0019, and a short tail

**Counting caveat, learned twice on this issue.** The raw figure is 2,920 *log lines*; MSBuild
emits each error twice, so the real count is **1,460**. The tracker note in Plato `b82ffd6` records
plato-323 at **1,204** — the residual gap is that session's in-flight fixes landing between the two
measurements. Same order, same shapes, same conclusion; the exact total is not a stable quantity
while two sessions are working. This is precisely the "compare per-shape counts, never just the
total" discipline merged in from plato-294, and the first draft of this evaluation tripped on it.

**[plato-323](plato-323.md) is the sole blocker and is moving fast** — ~7,510 at its filing, 1,204
at its latest note. It does not need a second pair of hands; it needs to be left alone to land.

## Affected code

> **STALE — kept for provenance.** The inventory below is the *original* declaration-layer
> measurement. Every error class in it is now fixed (see the evaluation above); do not use it as
> the current baseline. The live numbers are in the evaluation table.

Measured 2026-07-29, after the CS0736 class was fixed (that fix is landed, not pending):

- 166 × **CS0315** — `_CoonsPatch`, `_SweptSurface`, `_RuledSurface`, `_SweptSolid`, `_ExtrudedSurface`, `_TrimmedSurface`, `_SurfaceOfRevolution`, `_TubeSurface`: a concrete type passed where the concept's F-bounded `Self` is required (`Curve3D<Self>`); no boxing conversion.
- 134 × **CS0305** — `FieldsImplicitsShapes` / `FieldsImplicitsDistance` / `FieldsImplicitsCore` / `FieldsImplicitsFunction`, `ImplicitSdfTrees`, `FunctionalProcedural`, `_FunctionVolume3D`, `_FunctionRegion2D`: wrong generic arity.
- 14 × **CS0535** — `_Angle` (`Comparable.Compare`, `Hashable.Hash`), `_Matrix3x2` / `_Matrix4x4` (`MatrixLike.ColumnCount`, `ElementAt`), `_Quaternion` (`Interpolatable.Lerp`): the generated partial declares the interface, the handwritten `Plato.Intrinsics.V2` type lacks the member.
- 6 × **CS0557** — `Plato.Intrinsics.V2/Angle.cs:34,37,40`: generated conversions duplicate handwritten ones.

## Cause / analysis

**Update 2026-07-29: Root 1 was analyzed and split.** The CS0305 cluster is largely mechanical
emitter bugs (`Function1`-to-`System.Func` arity loss + constructor-call mangling), now
[plato-310](plato-310.md). The CS0315 cluster is a real design gap — concept in type position
(existential) has no defined C# lowering — now [plato-311](plato-311.md), with a decision recorded
there (dual-interface lowering, Option A). The "speculation, worth confirming" below is confirmed:
the emitter substitutes the *enclosing type* into `Self` for concept-typed fields
(`Curve3D<SweptSurface> Path`). This issue keeps Root 2 plus final wiring, and is blocked by the
two splits.

**Update 2026-07-29 (later): declaration layer fully resolved; a new body layer surfaced.**
plato-310 and plato-311 are closed (Plato `f859808` + `e2d8b83`), and Root 2 landed in `e2d8b83`
(Angle.Compare/Hash, Matrix3x2/4x4 ColumnCount/ElementAt, Quaternion.Lerp(Quaternion, float);
duplicate Angle conversions resolved by suppressing the generated field-type/Number pair for
primitive-backed types). All 324 original errors — the entire declaration layer — are gone,
verified in an isolated worktree: 402 declaration errors at HEAD, 0 after the fixes. Shared-writer
gates green: legacy goldens refreshed deliberately (views), both Generated projects compile,
`Ara3D.SDK.ConformanceTests` 205/205 (API snapshot re-baselined for the additive members),
frozen-V1 unchanged.

**But the build is still not green**: csc compiles in stages, and the declaration errors had been
suppressing body binding entirely. With declarations clean, ~7,510 pre-existing body-level errors
surfaced (proven pre-existing: two opposite CS0557 fixes unmask the identical set). That layer is
[plato-323](plato-323.md), which now solely blocks this issue's Done boxes.

Two independent roots, very different sizes.

**Root 1 (300 errors, deep): concept-as-generic-interface lowering.** CS0315 and CS0305 are one
cluster. A Plato concept becomes `interface C<Self> where Self : C<Self>`, and the forward stdlib
uses concepts in positions the legacy stdlib never did — concrete types flowing into `Self`
positions, and generic libraries over parameterized concepts. `ForwardStdLibCheckerTests` already
names this cluster (tuple → generic-interface returns) and says the right fix is a library
redesign returning a concrete type / `Self` rather than a checker patch. Speculation, worth
confirming: the surface/solid types fail because `Curve3D<Self>` is required where an
`ExtrudedSurface`'s *profile curve* is a different concrete type.

**Root 2 (20 errors, shallow): `Plato.Intrinsics.V2` gaps.** The generated partial asserts an
interface the handwritten half does not satisfy. Independent of Root 1 and fixable directly by
adding the four missing members and reconciling the `Angle` conversions.

**Build-level quarantine does not work — measured, not assumed.** Excluding the 16 failing files
cascades: `CurvesSampling.g.cs` and `GeometryTraits.g.cs` reference `SurfaceOfRevolution` /
`ExtrudedSurface` / `RuledSurface` / `SweptSurface` / `TubeSurface` / `CoonsPatch` / `SweptSolid`
directly, giving 148 CS0246; quarantining the referrers pulls in more. The forward stdlib is too
densely linked for file-level exclusion.

## Priority

**p2, effort L.** It gates all forward-stdlib runtime verification, which is a real hole. But it
is not gating shipping code — `stdlib-legacy` drives `Plato.Generated` and Studio, and its
conformance suite is green. Root 2 alone is a cheap, independently useful increment.

## Dependencies

- Blocked by: [plato-323](plato-323.md) (body-level errors — the sole remaining blocker).
  [plato-310](plato-310.md) and [plato-311](plato-311.md) are closed 2026-07-29.
- Blocks: [plato-306](plato-306.md) (final `Done means` box), and any future forward-stdlib law work.
- Touches: `Plato.CSharpWriter`, `Plato.Intrinsics.V2`, and the surface/solid + implicit-field library sources. `Plato.Intrinsics.V2` is shared with the legacy conformance suite — changes there must keep it green.

## Fix approaches

1. **Root 2 first, standalone.** Add `Angle.Compare`/`Hash`, `Matrix3x2`/`Matrix4x4` `ColumnCount`/`ElementAt`, `Quaternion.Lerp`; reconcile the duplicate `Angle` conversions. Clears 20 errors, ~4 types, no emitter change. Does not make the suite build on its own, but it is the cheap half and is useful regardless.
2. **Root 1 by library redesign** (the direction `ForwardStdLibCheckerTests` already recommends): change the offending signatures to return concrete types / `Self` instead of a bare concept. Touches real vocabulary, needs design review, but keeps the emitter simple.
3. **Root 1 by emitter change**: teach the writer to bridge concrete types into F-bounded `Self` positions. Avoids touching vocabulary but is emitter surgery with golden/byte-identity implications for the legacy suite.

## Bedrock

Strengthens the seam that the forward stdlib currently lacks entirely: an *executable* gate.
Today `stdlib` is verified only by structural lint and a type-checker ratchet, so the whole
forward vocabulary is asserted, never exercised. Closing this makes every future forward
refactor — plato-306 being the immediate one — checkable by law instead of by reading.

**Verdict: simplest-along-the-grain.** Take approach 1 then 2. The simple fix must NOT be
file-level quarantine (measured above: it cascades) and must NOT be relaxing
`BlockerGuardTests`, which is the only thing keeping an empty generation from reading as green.

## Done means

- [x] `dotnet build` of `Plato.ForwardConformanceTests` succeeds with 0 errors and no `<Compile Remove>` quarantine. (2026-07-31: 1307 `.g.cs`, 0 errors; the suite runs 41 pass / 3 fail / 3 skip.)
- [ ] `.\tools\regen-forward-conformance.ps1 -Test` runs the law runner; real failures quarantined in `KnownFailures.json` with an entry each, not by exclusion.
- [ ] Stage 2 of `regen-forward-conformance.ps1` flips from diagnostic (exit-code report) to **`-Test` gating** — merged from plato-294.
- [ ] The 11 affine laws in `stdlib-tests/foundation.laws.plato` execute and pass (this is plato-306's outstanding box).
- [ ] Degraded-body count (throwing stubs from non-ground TIR, `CSharpWriter.DegradedBodies`) unchanged or lower — **44 when plato-294 was filed**. Merged from plato-294: a fix that "compiles" by degrading more bodies to stubs is not a fix.
- [ ] `Ara3D.SDK.ConformanceTests` still 0 fail (any `Plato.Intrinsics.V2` change is shared with it).
- [ ] README / csproj / script status blocks updated to whatever is then true.

## Prevention

- **Compare per-shape error counts, never just the total** (merged from plato-294, and confirmed the hard way on this issue). The total is close to meaningless while the tree is in motion: this issue was measured at 324, then 364, then 398 within a few hours, with the class distribution *inverting* (CS0305/CS0315 dominant → gone; CS0535 14 → 392) as a concurrent session landed plato-310/311. Any "did my change help?" claim must be an A/B on identical sources, per shape.
- **Writer sessions collide in `Plato.CSharpWriter`** (merged from plato-294) — coordinate before editing it. Observed on this issue: three invalidated measurements, one stash of another session's file, one temp-merge-directory collision, and a period where the shared tree did not compile at all.
- **The CS0736 trap has no gate.** `CSharpFunctionInfo.IsStatic` is purely syntactic (`ParameterNames[0] == "_"`), so an obligation fill spelled `Zero(_: Color)` silently emits `static` and cannot satisfy the instance interface member it was written to discharge. Nothing catches the disagreement — it surfaced only as a C# compile error 1232 files later. A linter rule "implementation staticness must match the obligation it discharges" would have caught all 20 bodies at lint time. Worth filing.
- Wiring this suite into `check-all.ps1` once green would stop the forward stdlib silently drifting back out of compilability.

## Update 2026-07-30

Error count now 85 (was 324): plato-311 view lowering + plato-362 type-token members burned down the declaration and Zero-family classes. Biggest remaining class: CS1061 'Vector' on Direction3D/Vector3 receivers (62). PROMOTED TO P1 by the 2026-07-30 retirement decision (decisions/2026-07-30-retire-legacy-conformance-and-goldens.md): the legacy conformance suite is deleted, so this suite going green is the only path back to executable law coverage.

## Evaluation 2026-07-31 — compile blocker cleared

Measured at HEAD in this repo (not the stale studio submodule checkout, whose gate scripts still
point at pre-restructure paths and lint `stdlib` top-only, so `check-stdlib-fast.ps1` there fails
with "No .plato files found").

The 324-error CS0315/CS0305 concept-as-generic-interface cluster this issue was filed against is
GONE — the intervening concept/lowering work retired it. What remained was 16 duplicate-member
errors between the generated partials and `src/Plato.Intrinsics`, and behind them (masked, because
Roslyn skips body binding when declaration binding fails) ~108 body-level errors.

Root cause of the visible half: the primitive set shrank (plato-365) and the forward stdlib grew
reference bodies for the graduated types, but the handwritten runtime was never updated — so
`Angle.Compare/Hash`, the `Matrix4x4` Create*/Translation/Rotation/Determinant/Decompose surface,
`Matrix3x2.CreateRotation/Invert`, `Quaternion.CreateFrom*/Lerp/Length` and the whole
`AngleIntrinsics` trig class existed twice.

Fixes, by layer:

- **Runtime** — deleted every member the stdlib now bodies; added the ones it declares and the
  runtime lacked (`Number.Lerp/Inverse/ToNumber`, `Integer.ToInteger`, unary `-` and `%` on both
  matrices); `Intrinsics.MakeArray` returns `IReadOnlyList<T>` so array-literal fold seeds unify
  with `ReadOnlyList`/`IReadOnlyList` step results.
- **Writer** — sum-case constructors get receiver-style extension twins (`seg.Line()`); the
  inliner refuses a (callee, receiver-type) pair it already inlined on an earlier pass, which is
  the self-delegation signature that produced `self.Base.Base.Base…`; a non-wrapper primitive
  (`Type`, the `Function` arities) no longer gets `Value`-based equality scaffolding; the scalar
  re-home no longer duplicates a conversion the target's own single-field block emits.
- **stdlib** — explicit construction where a single Array-typed field cannot carry an implicit
  conversion (`PointN`/`VectorN` offsets, `TriangleArray3D`/`QuadArray3D` Deform); `DeCasteljau`
  spelled per control-value type, since an implicit `Interpolatable` bound on a library type
  variable does not survive concept-interface erasure.
- **Law packet** — `Law_SubtractIsReversedBetween2D` compared two displacements with `Distance`,
  which is declared between points.

Remaining on this issue: the three law failures (`AngleInterval.Law_ContainsCenter`,
`SuperEllipse.Law_SuperEllipseExponentTwoPerimeterIsEllipse`,
`VonMisesDistribution.Law_VonMisesCdfStartsAtZero`) need triage — quarantine in
`KnownFailures.json` or a content fix; the `-Test` gating flip; and the degraded-body box (48 now
vs the 44 recorded when plato-294 was filed — the drift predates this work).

Collateral, deliberately not fixed here: `generated/Plato.Generated.*` (legacy generation) went
from 498 to 710 error lines, because stdlib-legacy still declares as intrinsics the members the
runtime no longer carries. Those 19 names are recorded in `LegacyKnownMissing`, whose own doc
comment says the forward declaration wins where the corpora disagree.
