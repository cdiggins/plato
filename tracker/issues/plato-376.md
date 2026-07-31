---
id: plato-376
title: Concept obligations on a GENERIC type can never be discharged
type: bug
status: ready
priority: p2
effort: M
risk: low
area: plato
sprint: 
created: 2026-07-31
closed:
links: [src/Plato.Compiler/Analysis/Linter.cs, stdlib/foundation/primitives-arrays.types.plato, stdlib/graphics/keyframes-tracks.types.plato]
---

## Issue

A concrete type that takes a type parameter cannot have its concept obligations
filled by any library function. The obligation's signature is keyed by the
TYPE's own parameter — `ColumnCount(Array2D<T>):Integer` — while a library
function can only be written over a type VARIABLE —
`ColumnCount(xs: Array2D<$T>): Integer`. The two signature ids never match, so
`FunctionInstance` pairing leaves the obligation unimplemented, LINT001 reports
it, and the C# writer emits a throwing member.

Found while burning down LINT001 (`plato-321`). A trial declaration of the
Array2D/Array3D extents as intrinsics changed nothing — the declaration is
inert — so it was reverted and replaced with a TODO in
`stdlib/foundation/intrinsics.library.plato`.

## Impact

Eight of the nine LINT001 findings left after the plato-321 burn-down are this
one defect:

- `Array2D<T>` — `ColumnCount`, `RowCount` (2)
- `Array3D<T>` — `ColumnCount`, `RowCount`, `LayerCount` (3)
- `AnimationTrack<T>`, `TangentTrack<T>`, `Tween<T>` — `Sample` (3)

For Array2D/Array3D the runtime members already exist (`GridExtensions` in
`src/Plato.Intrinsics`), so this is purely a pairing failure and the generated
throw is gratuitous. Consumers work around it: `sampling-fields.library.plato`
and `surfaces.library.plato` both read grid extents as `Row(0).Count` rather
than calling `ColumnCount`.

The three animation tracks have a SECOND blocker on top of this one, so fixing
the pairing alone will not clear them — see below.

## Affected code

- `src/Plato.Compiler/Analysis/Linter.cs` — `CheckUnimplementedInterfaceObligations`,
  matching by `FunctionInstance.SignatureId`
- `stdlib/foundation/primitives-arrays.types.plato` — `Array2D<T>`, `Array3D<T>`
- `stdlib/graphics/keyframes-tracks.types.plato`, `stdlib/graphics/motion-graphics.types.plato`
- `stdlib/foundation/intrinsics.library.plato` — the TODO left at the Array2D section

## Root-cause notes

The obligation is substituted with the concrete type's own type parameter as
the type argument; the candidate implementation carries a fresh type variable.
Signature-id equality is textual over those, so `T` and `$T` differ. Nothing
unifies them, because pairing does no unification at all — it is a dictionary
lookup keyed by the rendered signature.

## Fix approaches

1. **Unify during pairing.** Where an obligation's type argument is the
   declaring type's own parameter, treat a candidate's type variable in the same
   position as a match. Smallest change; the risk is admitting a genuinely
   unrelated overload, so the unification must be positional and one-way.
2. **Instantiate obligations per use.** Heavier, and it does not obviously
   terminate for a type that is generic in itself.
3. **Writer-side allowlist.** Add the five grid members to
   `Linter.MembersImplementedByWriter` and teach the writer to emit them from
   the runtime type. Clears the lint but leaves the pairing defect for the next
   generic type, so it is a patch, not a fix.

Approach 1 is the one to try, with `LinterTests` coverage for both a matching
and a deliberately non-matching generic candidate.

## The second blocker on the tracks

`Tween<T>` and the two track types need `Lerp` on a bare `T`. A library type
variable carries no constraint that survives the shipping C# recipe — the same
reason `DeCasteljau` in `splines-bezier.library.plato` is spelled once per
control-value type rather than once over `$T`. So even with pairing fixed,
their `Sample` bodies need either a constrained type parameter on the
declaration or one spelling per instantiated value type. Worth splitting out
if this issue is picked up for the Array2D half alone.

## Done means

- [ ] A generic type's concept obligation is discharged by a library function
      over a type variable, with a `LinterTests` case pinning both the match and
      a non-match
- [ ] Array2D/Array3D `ColumnCount`/`RowCount`/`LayerCount` no longer report
      LINT001, and the TODO in `intrinsics.library.plato` is removed
- [ ] The `Row(0).Count` workarounds in `sampling-fields.library.plato` and
      `surfaces.library.plato` are replaced with the real members
- [ ] The remaining track blocker is either fixed or split into its own issue

## Simplest fix

Positional one-way unification in the pairing step (approach 1), scoped to the
Array2D/Array3D half, with the animation tracks split out.
