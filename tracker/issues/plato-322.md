---
id: plato-322
title: Convert solids-polyhedra types to PolygonMesh3D / TriangleMesh3D
type: feature
status: done
priority: p1
effort: "?"
risk: "?"
area: plato
sprint: 
created: 2026-07-29
closed: 2026-07-30
links: [plato-301, plato-297, plato-313, tracker/decisions/2026-07-29-polyhedra-dual-kind-map.md, submodules/Plato/stdlib/solids-polyhedra.plato, submodules/Plato/stdlib/polyhedra-catalog.library.plato, submodules/Plato/stdlib/polyhedra-seeds.library.plato, submodules/Plato/stdlib/meshes.concepts.plato]
---

## Outcome (2026-07-30)

Shipped on `plato-stdlib-improvements` commit `faf0fbe`, merged to Plato main in `d0dc6c8` (2026-07-30). New `stdlib/solids-polyhedra.library.plato` (SolidsPolyhedra: placement helpers, Kind dispatch, parametric builders) + `Meshable3D` on all 7 solid types in `solids-polyhedra.plato`. Coverage: Platonic 5/5; Archimedean 7/13 (ambo/truncate families; snub/expand/bevel kinds await plato-297 operators); Catalan 4/13. Unimplemented kinds return a documented empty `PolygonMesh3D`, never a wrong mesh; Catalans placed by measured-circumradius uniform scale, not ProjectedToUnitSphere. RegularPrism/RegularPyramid/SquarePyramid/Antiprism build closed polygon meshes honoring SideCount/Radius/Height/BaseLength; all types triangulate via the fan path. Gates: lint 0 errors (warning ratchet 250 = baseline), checker ratchet 0/2402 at ceiling 0.

## Idea

Make every typed solid in `solids-polyhedra.plato` convertible to a mesh: `RegularPrism`, `RegularPyramid`, `SquarePyramid`, `Antiprism`, `PlatonicSolid`, `ArchimedeanSolid`, and `CatalanSolid`. Today those types carry `Kind` / `Frame` / `Radius` (or parametric size) but are inert for meshing — [plato-301](plato-301.md) shipped `PolygonMesh3D.*` catalog factories (unit-sphere seeds + Conway aliases), not methods on these value types. Closing this means Kind dispatch + Frame/Radius (or Height/SideCount) placement into `PolygonMesh3D`, then triangulation via the existing `Meshable3D` / fan path.

## Assumptions

- Catalog factories in `polyhedra-seeds.library.plato` / `polyhedra-catalog.library.plato` remain the combinatorial source; typed solids are a placement/dispatch layer over them, not a second vertex table.
- Circumscribed radius and frame placement are uniform for Platonic / Archimedean / Catalan wrappers; Catalans must not go through `ProjectedToUnitSphere` (catalog comment + Conway note).
- Parametric families (prism / pyramid / antiprism) may need new mesh builders; they already have parametric surface `Eval` in `solids.library.plato` but that is not a closed polyhedral mesh.
- Incomplete catalog coverage is OK initially: Archimedean/Catalan kinds still ahead of snub/expand/bevel operators — dispatch can fail or defer for missing aliases until operators land under [plato-297](plato-297.md).

## Design decisions

- **Canonical sink** — `ToPolygonMesh` then `ToTriangleMesh`, vs only `Meshable3D.ToTriangleMesh`. Prefer polygon mesh first (matches catalog + Conway domain); triangulation is a second hop.
- **Where bodies live** — new `solids-polyhedra.library.plato` vs extend `PolyhedraCatalog` with overloads on the typed solids. Prefer a dedicated library so catalog stays “named mesh factories” and solids stay “placed instances.”
- **Missing kinds** — soft fail / optional vs require all thirteen Archimedean/Catalan before shipping. Prefer progressive dispatch: implemented aliases work; unimplemented kinds are explicit gaps (laws or comments), not silent wrong meshes.
- **Prism/pyramid construction** — extrude/taper from regular polygon helpers vs hand-built CSR faces. Prefer reuse of polygon spatial builders if they exist; otherwise small CSR pack helpers shared with seeds.

## Related

- [plato-301](plato-301.md) — closed; explicitly listed these types as inert (no `ToTriangleMesh`, Kind dispatch, or Frame/Radius placement).
- [plato-297](plato-297.md) — umbrella catalog/operators; this issue is the typed-solid → mesh seam, not Johnson/stellation.
- [plato-313](plato-313.md) / [ADR dual kind map](../decisions/2026-07-29-polyhedra-dual-kind-map.md) — Catalan mesh can later be `mesh(Dual(kind)).Dual`; kind Dual map already exists.
- [solids-polyhedra.plato](../../submodules/Plato/stdlib/solids-polyhedra.plato) — type vocabulary.
- [polyhedra-catalog.library.plato](../../submodules/Plato/stdlib/polyhedra-catalog.library.plato) / [polyhedra-seeds.library.plato](../../submodules/Plato/stdlib/polyhedra-seeds.library.plato) — unit meshes to place.

## Approaches

Short term: (1) `ToPolygonMesh` for `PlatonicSolid` via Kind match → seed × Radius × Frame; (2) same for Archimedean/Catalan aliases that already exist in the catalog; (3) parametric prism/pyramid/antiprism CSR builders; (4) `Meshable3D` via fan triangulation of the polygon mesh.

Long term: full thirteen×two Kind coverage once snub/expand/bevel exist; Studio generators take typed solids instead of C# `Polyhedra` tables.

Adjacent: retire C# staging path once Stage 2 codegen runs ([plato-308](plato-308.md)); prism/antiprism as Conway seeds for infinite families.

## Bedrock

Strengthens the **typed solid → `PolygonMesh3D` → triangle mesh** seam that [plato-297](plato-297.md)/[plato-301](plato-301.md) left open: `solids-polyhedra.plato` kinds become usable geometry, not documentation. Catalog factories stay pure unit meshes; placement stays on the value types. **Verdict: simplest-along-the-grain** — dispatch + scale/frame over existing factories + parametric builders for prism/pyramid/antiprism; must NOT duplicate seed tables on the typed solids, must NOT require all thirteen Archimedean operators before first Platonic/`Cuboctahedron` path works.

## Done means

- [ ] `PlatonicSolid` (all five kinds) produces a `PolygonMesh3D` with correct topology at the given `Radius`, transformed by `Frame`
- [ ] At least the catalog-covered Archimedean and Catalan kinds dispatch the same way; unimplemented kinds are documented or law-gated, not wrong meshes
- [ ] `RegularPrism`, `RegularPyramid`, `SquarePyramid`, and `Antiprism` produce closed polygon meshes matching SideCount / Radius / Height (or BaseLength)
- [ ] Typed solids that implement `Meshable3D` (or equivalent) triangulate via the polygon-mesh path
- [ ] Forward stdlib lint / checker ratchet does not regress (`check-stdlib-fast`)

## Simplest possible implementation

`ToPolygonMesh(PlatonicSolid)` as a Kind match to `PolygonMesh3D.Cube` (etc.), scale vertices by `Radius`, apply `Frame`; add one Archimedean and one Catalan arm; defer full Kind coverage and parametric families to follow-up commits in the same issue.

Pros: unblocks typed API immediately; reuses seeds; small surface.
Cons: prism/pyramid still inert until next commit; Studio still on C# until codegen Stage 2.
