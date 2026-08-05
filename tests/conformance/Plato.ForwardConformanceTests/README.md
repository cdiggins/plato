# Plato.ForwardConformanceTests

Conformance suite for the **forward** stdlib (`stdlib/`, the v3 vocabulary),
the sibling of `Ara3D.SDK.ConformanceTests` (which covers `legacy/stdlib-legacy`).

It runs the same reflection law runner (`LawTests`) and manifest machinery
(`ConformanceSupport`) over C# generated from the forward stdlib merged with the forward law
packet (`stdlib/tests/`), against `Plato.Intrinsics`.

## Status — MEASURED 2026-07-29, NOT RE-MEASURED SINCE

> **Do not read the rest of this section as current.** Every number below is a snapshot taken on
> **2026-07-29** and has not been re-taken. It is known to have moved: `docs/gate-log.md` records
> a conformance row at commit `36369e5` (2026-07-31) reading **44 pass / 0 fail / 3 skip**, which
> the "neither builds nor runs" sentence below contradicts, and `plato-308`'s own title had
> already dropped from 324 errors to 85 by 2026-07-30. The error inventory here is kept as
> history — the shape of the four clusters is still the useful part — not as a status report.
>
> **To learn the current state, measure it**: `tools/regen-forward-conformance.ps1 -Test`, or read
> the last row of [`docs/gate-log.md`](../../../docs/gate-log.md). When this section and a gate
> disagree, the gate is right.

### Snapshot of 2026-07-29: codegen SUCCEEDS; blocked on generated-C# compile errors

**The 2026-07-28 codegen blocker is fixed.** `CSharpTypeWriter.WriteBody` no longer throws on a
bodied member with no ground TIR — it emits a throwing stub, records the member in
`Writer.DegradedBodies`, and keeps writing
(`Plato.CSharpWriter/CSharpTypeWriter.cs:282`). Stage 2 now generates **1232 `.g.cs` files**
from the merged forward stdlib.

The blocker moved one stage later: the generated C# does not yet **compile**. Measured
2026-07-29 after fixing the CS0736 class (see below): **324 errors in 25 of 1232 files**, in
four clusters.

| Errors | Code | Where | Cause |
|---|---|---|---|
| 166 | CS0315 | 8 concrete surface/solid types (`CoonsPatch`, `SweptSurface`, `RuledSurface`, `SweptSolid`, `ExtrudedSurface`, `TrimmedSurface`, `SurfaceOfRevolution`, `TubeSurface`) | a concrete type passed where the interface's F-bounded `Self` is required (`Curve3D<Self>`) |
| 134 | CS0305 | implicit-field / SDF libraries (`FieldsImplicits*`, `ImplicitSdfTrees`, `FunctionalProcedural`) and `FunctionVolume3D` / `FunctionRegion2D` | wrong generic arity, same interface-as-generic-interface root |
| 14 | CS0535 | `Angle`, `Matrix3x2`, `Matrix4x4`, `Quaternion` | generated partial declares the interface; the handwritten `Plato.Intrinsics` type lacks the member (`Angle.Compare`/`Hash`, `MatrixLike.ColumnCount`/`ElementAt`, `Quaternion.Lerp`) |
| 6 | CS0557 | `Plato.Intrinsics/Angle.cs:34,37,40` | generated conversions duplicate handwritten ones |

CS0315 + CS0305 are one cluster with one root — the interface-as-generic-interface lowering that
`ForwardStdLibCheckerTests` already names as needing a library redesign. CS0535 + CS0557 are
runtime gaps in `Plato.Intrinsics`, independent and much smaller.

**Build-level quarantine does not work, and this was measured rather than assumed.** Excluding
the 16 failing files cascades: `CurvesSampling.g.cs` and `GeometryTraits.g.cs` reference the
excluded surface types directly, producing 148 CS0246 errors; quarantining the referrers pulls
in more. The forward stdlib is too densely linked. The codegen defects have to be fixed. Tracked
as **plato-308**.

Until then the suite still neither builds nor runs, and Stage 1 remains the executable gate.
`BlockerGuardTests` keeps an empty/law-less generation an explicit **RED**, never a vacuous green.

### Fixed here (2026-07-29): the CS0736 class, 40 errors

`Additive` declares `Zero(x: Self): Self` — an *instance* member — but the implementations spelled
it `Zero(_: Color)`. `CSharpFunctionInfo.IsStatic` is purely syntactic
(`ParameterNames[0] == "_"`), so the writer emitted `public static Color Zero()`, which cannot
implement an instance interface member. The same applied to `One` / `MinValue` / `MaxValue` on
`Color`, `Complex`, `Proportion`, `Percent`, `Probability` — 20 bodies, 40 errors.

Fixed at the source by naming the receiver (`Zero(x: Color)`), which is forward-stdlib-only and so
cannot touch the `Generated/` goldens or the legacy suite. The underlying trap is worth a lint
rule: **an obligation fill whose staticness disagrees with the obligation it discharges is always
a bug**, and nothing currently catches it.

## What DOES pass today

`tools/regen-forward-conformance.ps1` (no args) — Stage 1 **type-check gate**: it merges
`stdlib` + `stdlib-tests`, lints the union, and asserts **0 symbol resolution errors**. This
proves the whole forward vocabulary plus the `Law_*` packet resolve against each other. That is
the honest, currently-green forward-stdlib gate.

## Activating execution (once the writer blocker is fixed)

1. Fix the "No ground TIR for bodied ..." writer gap so the full recipe generates the forward
   stdlib without aborting.
2. `tools/regen-forward-conformance.ps1 -Test` — regenerates `Generated/` and runs the suite.
3. Quarantine any real law failures in `KnownFailures.json`; remove each entry as its fix lands.
4. Once codegen is reliably green, relax/remove `BlockerGuardTests` and wire the suite into CI.

Deliberately **not** added to any `.sln`. `Generated/` is script-produced and gitignored.

## The law packet

The packet is `stdlib/tests/`, one `*.laws.plato` file per domain — it moved there from the old
top-level `stdlib-tests/`. `foundation.laws.plato` (`library FoundationLaws`) covers the interval
remap kit (`At`/`ParameterOf`/`Remap`), the generic `IntervalLike` containment surface, and the
concrete `Bounds2D`/`Bounds3D` AABB operations; siblings cover easing, polyhedra, implicit SDFs,
special numerics, shape surfaces, triangulation, sorting and the spatial structures. Every member
a law references must be verified against the forward library source first. See
[`stdlib/tests/README.md`](../../../stdlib/tests/README.md).
