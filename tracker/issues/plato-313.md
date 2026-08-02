---
id: plato-313
title: Represent known polyhedron Dual as an interface
type: problem
status: done
priority: p3
effort: M
risk: med
area: plato
sprint: 
created: 2026-07-29
closed: 2026-07-29
links: [plato-301, submodules/Plato/stdlib/polyhedra-catalog.library.plato, submodules/Plato/stdlib/polyhedra-conway.library.plato, submodules/Plato/stdlib/solids-polyhedra.plato, submodules/Plato/stdlib/algebra-metric.concepts.plato, submodules/Plato/docs/plato-language-semantics.md]
---

## Issue

Named dual pairs in the polyhedra catalog (e.g. cuboctahedron ↔ rhombic dodecahedron) are only comments plus Conway `Dual` on `PolygonMesh3D`. There is no type-level interface that says “this form’s known dual is that form.” Closing this problem means deciding whether (and how) to encode that pairing in Plato interfaces, then filing follow-up work / an ADR.

## Impact

Without a typed dual relationship, catalog aliases stay stringly documented mesh factories; generic code cannot constrain “Self has Dual TDual,” and Archimedean/Catalan kind pairs cannot be checked or reused as vocabulary. Safe to defer while Conway ops and mesh factories remain the only consumers; becomes blocking if we introduce per-solid types or kind-level dual maps for Studio/generators.

## Affected code

- `submodules/Plato/stdlib/polyhedra-catalog.library.plato` — Catalan aliases are `….Dual` of Archimedean mesh factories; pairing is comment-only.
- `submodules/Plato/stdlib/polyhedra-conway.library.plato` (~39–65) — geometric `Dual(PolygonMesh3D): PolygonMesh3D` (polar reciprocation); notes Platonic duals / self-dual tetrahedron.
- `submodules/Plato/stdlib/polyhedra-seeds.library.plato` — Platonic solids as static mesh factories, not types.
- `submodules/Plato/stdlib/solids-polyhedra.plato` — `ArchimedeanSolid` / `CatalanSolid` + kind sums; no Dual map between kinds.
- `submodules/Plato/stdlib/algebra-metric.concepts.plato` (`Difference<TDelta>`, `OriginBased<TDelta>`) and `Point2D implements OriginBased<Vector2D>` — existing “Self paired with another type” pattern.
- `submodules/Plato/docs/plato-language-semantics.md` §4 — interfaces with type parameters; satisfaction via members + `implements`.

## Cause / analysis

[plato-301](plato-301.md) deliberately shipped solids as `PolygonMesh3D` factories + Conway operators. That makes Dual an *operation* on meshes, not a *relationship* between named forms. An interface like `HasDual<TDual>` fits the language (same shape as `Difference<TDelta>`), but today there is no `Self` for Cuboctahedron / RhombicDodecahedron — they are library methods, not types. Kind wrappers (`ArchimedeanSolid` / `CatalanSolid`) could host a Dual map without new types, at coarser granularity.

## Priority

**p3** — design opportunity, not a defect; nothing currently fails. Cost of deferral is mainly vocabulary drift if more catalog aliases land before a dual model is chosen. Raise if typed solid forms become a near-term goal.

## Dependencies

- Blocked by: none for a decision; implementation may need per-solid types or kind Dual helpers.
- Blocks: future typed dual-aware APIs / Studio generators that want Dual as a capability.
- Touches: `solids-polyhedra.plato`, polyhedra `*.library.plato`, possibly new `*.concepts.plato`; concurrent Conway/catalog edits ([plato-301](plato-301.md) done) collide on those files.

## Fix approaches

1. **`HasDual<TDual>` on per-solid types** — e.g. `type Cuboctahedron implements HasDual<RhombicDodecahedron>`. Strongest typing; largest vocabulary change (each named solid becomes a type).
2. **Kind-level Dual map** — `Dual(ArchimedeanSolidKind): CatalanSolidKind` (and reverse / Platonic pairs). Cheap; no per-solid types; Dual stays coarse (kind, not mesh identity).
3. **Keep mesh Dual only** — document named pairs in catalog comments / laws; no interface. Zero design cost; typed pairing remains unavailable.

Closing should pick one (or a staged mix: 3→2→1) and record an ADR plus follow-up issues.

## Bedrock

The seam is **named polyhedral form ↔ partner form** as a compile-time capability, parallel to `OriginBased<TDelta>` / `Difference<TDelta>`, distinct from Conway `Dual` on arbitrary `PolygonMesh3D`. Strengthening that seam makes dual-aware generic code and catalog identity checkable; keep geometric Dual as the mesh construction. **Verdict: simplest-along-the-grain** — decide and ADR first; must NOT invent a second Conway Dual, and must NOT force per-solid types in the same change as a kind Dual map if kinds suffice for the chosen consumers.

## Done means

- [x] ADR in `tracker/decisions/` records the chosen Dual model (interface + types, kind map, or mesh-only) and rejected alternatives — `2026-07-29-polyhedra-dual-kind-map.md` (kind-level Dual map + Conway mesh Dual unchanged)
- [x] Follow-up issue(s) filed for any implementation work the ADR commits to (or explicit “no follow-up — mesh Dual only”) — no follow-up: maps + involution laws shipped with the ADR (`stdlib/polyhedra-duals.library.plato`, `stdlib-tests/polyhedra.laws.plato`)
- [x] If an interface is chosen: sketch signature (`HasDual<TDual>` or equivalent) and where `implements` would live — N/A: interface deliberately rejected (no per-solid types); see ADR

## Simplest fix

**ADR: kind-level Dual map + keep Conway Dual** (approach 2 + status quo mesh op). Gets typed Archimedean↔Catalan pairing without thirteen new solid types. Gives up per-instance Dual refinement until/unless per-solid types appear later.

## Prevention

- When adding catalog aliases, require either a Dual kind entry or an explicit “no known dual / self-dual” note so pairs do not stay comment-only by default.
- Optional later: law/witness that `Dual(Dual(kind)) == kind` for mapped pairs (file as its own issue if pursued).
