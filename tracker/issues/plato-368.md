---
id: plato-368
title: "Backend intrinsic override + repr table for the migrated reference bodies"
type: feature
status: ready
priority: p1
effort: L
risk: medium
area: plato
sprint:
created: 2026-07-30
closed:
links: [plato-367, plato-365, submodules/Plato/stdlib/foundation/intrinsics.library.plato, submodules/Plato/Plato.Intrinsics.V2]
---

## Task
Plato commit 709be97 made intrinsics.library.plato primitive-only: everything on
Angle, Number2/3/4/8, Vector2D/3D, Matrix3x2/4x4, Quaternion now has Plato
reference bodies (angle-trig / vectors-tuples-ops / vectors-geometric-ops /
matrices-ops / rotations-ops / hashing .library.plato). The stdlib gates are
green, but the C# backend has NOT been reconciled. This task is the backend half
of the B+C design (reference bodies define semantics; backend substitutes
verified native implementations):

1. ~~**Runtime members for the new scalar trig kernel**~~ — **DONE 2026-07-30.**
   `Plato.Intrinsics.V2/Number.cs` gained the 13 radians-kernel members
   (`Cos/Sin/Tan/Cosh/Sinh/Tanh(Number)` plus the `*Radians` inverse family).
   `IntrinsicObligationTests` is green and the full PlatoTests suite passes 190/190.
   What remains on the codegen side is running the writer end-to-end: no C# has been
   generated from the migrated stdlib yet, so the reference bodies are unproven as output.
2. **Override table**: per-backend mapping PlatoName -> native call (e.g.
   `Vector3D.Dot -> System.Numerics.Vector3.Dot`) so hot functions use
   System.Numerics instead of the reference bodies.
3. **Repr map**: formalize the existing implicit V2 representation mapping
   (Vector3D = SN.Vector3, Matrix4x4 = SN.Matrix4x4, Number8 = Vector256) as
   checked config rather than folklore.
4. **Conformance harness (light)**: sample-input comparison of reference body vs
   native override with ulp tolerance — Slerp/Atan2 edge conventions genuinely
   differ between libraries.

## Flagged discrepancies to reconcile (from the migration, commented in-file)
- **Matrix4x4.Decompose tuple order**: Plato declaration says (scale, rotation,
  translation, ok); V2 C# returns (translation, rotation, scale, ok). One of
  them must change — decide and align.
- Matrix3x2.CreateRotation: quarter-turn snapping dropped (plain cos/sin).
- CanInvert: NaN determinant now reported non-invertible (referencesource says
  invertible); Invert defined via CanInvert so they cannot disagree.
- Exception-throwing Create* paths became unchecked preconditions (no
  exceptions in Plato).
- Array2D/Array3D remain the one documented bodiless exception (opaque
  field-less types) — give them an honest layout or a repr contract to finish
  the rule.
- **Duplicate-member risk on the Angle-returning inverse trig.** `angle-trig.library.plato`
  now DEFINES `Acos(self: Number): Angle` (and Acosh/Asin/Asinh/Atan/Atan2/Atanh), while
  `Plato.Intrinsics.V2/Number.cs` still hand-writes the same seven extension methods — they
  were intrinsics until 709be97 and stdlib-legacy still declares them, so they cannot simply
  be deleted. When codegen runs against the forward stdlib, check whether the generated
  extension collides with the handwritten one; if it does, the override table (item 2) is the
  place to suppress emission rather than deleting the V2 member.

## Done means
- [ ] Generated C# from the current stdlib compiles and passes conformance
      against Plato.Intrinsics.V2 behavior.
- [ ] Override + repr tables exist as explicit per-backend config.
- [ ] Decompose order decision recorded and both sides aligned.
- [ ] Array2D/3D exception either removed or re-scoped with a decision note.
