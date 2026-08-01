---
id: plato-376
title: Concept obligations on a GENERIC type can never be discharged
type: bug
status: done
priority: p2
effort: M
risk: low
area: plato
sprint: 
created: 2026-07-31
closed: 2026-08-01
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

- [x] A generic type's concept obligation is discharged by a library function
      over a type variable, with a `LinterTests` case pinning both the match and
      a non-match
- [x] Array2D/Array3D `ColumnCount`/`RowCount`/`LayerCount` no longer report
      LINT001, and the TODO in `intrinsics.library.plato` is removed
- [x] The `Row(0).Count` workarounds in `sampling-fields.library.plato` and
      `surfaces.library.plato` are replaced with the real members
- [x] The remaining track blocker is either fixed or split into its own issue
      (split 2026-08-01: plato-382, constrained type parameters + constraint-carrying emission)

## Simplest fix

Positional one-way unification in the pairing step (approach 1), scoped to the
Array2D/Array3D half, with the animation tracks split out.

## Resolution

**Compiler (this issue).** Pairing lives in one place now:
`ConcreteType.ImplementationFor` decides which implementation discharges an
obligation, and `ImplementedFunctions`/`UnimplementedFunctions`, LINT001, LINT012
and every writer read that one decision. Exact `SignatureId` equality is still
the primary key; the fallback is approach 1 — both signatures re-rendered with
their type variables (and, on the obligation side, the declaring type's own
parameters) renamed `#0, #1, ...` in order of first appearance, then compared.
First-occurrence renaming makes the variable NAME irrelevant while preserving its
REPETITION pattern, and only the obligation side may rename a type PARAMETER, so
the unification is one-way: a candidate naming a concrete type in that position
discharges nothing. `CheckDuplicateLibrarySignatures` (LINT004) is untouched —
duplicate detection stays exact. Covered by
`tests/PlatoTests/LinterGenericObligationTests.cs` (the match under an arbitrary
variable name; a concrete type argument; an inconsistent repetition pattern).

**Stdlib half — landed ahead of this, in plato-378 (`91a7f57`).** Giving
`Array2D`/`Array3D` an honest layout made the extents FIELDS, and a field
discharges an obligation directly, so the five grid findings were already gone
before the pairing fix: the `intrinsics.library.plato` TODO is removed and both
`Row(0).Count` workarounds now read `ColumnCount` / `RowCount`. The pairing fix
therefore changes no stdlib finding today — it removes the defect for the next
generic type rather than for these five members.

**Lint counts (three shipping tiers).** 8 LINT001 at filing → 1 before this
change (plato-378 cleared the five grid members; the two track types moved to
`stdlib/future` under stdlib-377) → 1 after. The survivor is
`Sample(Tween<T>, Duration):T`. Whole-corpus totals unchanged at 1535 findings,
ratchet 33 (0 Error + 33 Warning): no new finding of any code.

**Gates.** PlatoTests 202/202; `tools\check-stdlib-fast.ps1` both gates PASS
(lint --strict 0 errors, checker ratchet). Forward conformance does NOT build at
the time of writing — 6 x CS0102 (`Number.Zero/One/Tau/E`, `Integer.Zero/One`
declared on both the generated partial and `src/Plato.Intrinsics`). Measured with
this change stashed as well: identical errors, so it belongs to the in-flight
plato-378 constant migration, not to the pairing fix.

**Animation tracks are OUT of scope here.** `AnimationTrack`/`TangentTrack` moved
to `stdlib/future` (stdlib-377) and are neither linted nor emitted; `Tween<T>`
remains in `stdlib/graphics` and still reports LINT001, because its second
blocker is unrelated to pairing — `Sample` needs `Lerp` on a bare `T`, and a
library type variable carries no constraint that survives the shipping C# recipe
(see "The second blocker on the tracks" above). That needs its own issue; the
last box stays unticked until it is filed or fixed.

**Update 2026-08-01 — the three paragraphs above about `Tween<T>` were overtaken
by `plato-382`, which is now closed.** They stood for the few hours between the
pairing fix and the split issue landing, and are kept as the historical record of
why the split was made; every forward-looking claim in them is now false:

- `Tween<T>` no longer reports LINT001. It declares
  `type Tween<T> where T: Interpolatable`, and its `Sample` is a real body in the
  new `stdlib/graphics/motion-graphics.library.plato` — not a throwing stub.
- "A library type variable carries no constraint that survives the shipping C#
  recipe" is no longer true. Declared bounds on `type`, `concept` and (via
  `plato-393`) library-function declarations are verified by the checker and
  emitted as F-bounded C# `where` clauses. The decision is
  `tracker/decisions/2026-08-01-declared-type-parameter-bounds-are-verified-and-emitted.md`;
  the same change collapsed `DeCasteljau` from five hand-spelled overloads to one.
- LINT001 across the three shipping tiers is 0, and the lint ratchet dropped
  33 -> 32 (`ForwardStdLibLintTests.MaxLintRatchet`).

Nothing above this update changes: the pairing fix in
`ConcreteType.ImplementationFor` is what THIS issue did, and it stands as written.
The last `Done means` box was discharged by the split itself (plato-382 filed
2026-08-01); plato-382 has since been implemented and closed, so this issue closes
with no residue.
