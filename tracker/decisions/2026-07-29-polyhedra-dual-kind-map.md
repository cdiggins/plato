---
date: 2026-07-29
title: Polyhedral duals as kind-level maps, not per-solid types
status: accepted
superseded-by:
links: [plato-313, plato-301, plato-232]
---

## Context

The polyhedra catalog names dual pairs (cuboctahedron ↔ rhombic dodecahedron, …)
only in comments, plus the geometric Conway `Dual(PolygonMesh3D)` operation
(polar reciprocation). Nothing type-level records which named form the dual of a
named form *is*, so generic code cannot use the Archimedean/Catalan pairing as
vocabulary. plato-313 asked whether to encode the pairing, and how.

## Decision

Encode known duals as **total functions on the existing kind sums** in a new
`stdlib/polyhedra-duals.library.plato` (`library PolyhedraDuals`):

- `Dual(PlatonicSolidKind): PlatonicSolidKind` (tetrahedron self-dual)
- `Dual(ArchimedeanSolidKind): CatalanSolidKind` (13 arms)
- `Dual(CatalanSolidKind): ArchimedeanSolidKind` (13 arms, inverse)

Exhaustive `match` bodies (sum types, plato-232). The geometric Conway `Dual` on
`PolygonMesh3D` is unchanged and remains the only mesh-level dual. Involution
laws (`kind.Dual.Dual == kind`, all three sums) live in
`stdlib-tests/polyhedra.laws.plato`.

No new concept is introduced: this is deliberately *not* the `HasDual<TDual>`
concept the issue title suggested.

## Rationale

- The kind sums already enumerate all 5 + 13 + 13 forms, so the maps are total
  and exhaustiveness-checked — the compiler enforces that every kind names its
  dual, which comments never could.
- Costs ~30 lines and zero new types; landed the same day as the decision.
- The maps are ahead of the mesh catalog on purpose (catalog covers only the
  ambo/truncate families), matching the existing "enums ahead of this file"
  stance in `polyhedra-catalog.library.plato`.
- Overloading the name `Dual` is safe and desirable: different parameter types,
  one vocabulary word, no second mesh-dual invented.

## Alternatives rejected

- **`HasDual<TDual>` concept on per-solid types** — there are no per-solid types
  (solids are kind-tagged records + mesh factories, per plato-301), a concept
  needs members and a `Self` to attach to, and no consumer needs per-instance
  dual typing today. Revisit only if typed per-solid forms ever land; the kind
  maps would then be their obvious implementation substrate.
- **Mesh-only status quo** — free, but leaves the pairing uncheckable; rejected
  because the map now costs almost nothing (sum types shipped).

## Consequences

- Any future kind added to a solid sum must extend the matching `Dual` map —
  the exhaustiveness check makes forgetting impossible, which is the point.
- Catalan mesh construction can later dispatch through the map
  (mesh(catalan) = mesh(Dual(catalan)).Dual) once all 13 Archimedean factories
  exist; not committed here.
- No follow-up implementation issue: the maps and laws shipped with this ADR.
