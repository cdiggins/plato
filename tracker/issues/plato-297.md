---
id: plato-297
title: Polyhedra catalog with operators, duals, and mesh construction
type: idea
status: in-progress
priority: "?"
effort: "?"
risk: "?"
area: plato
sprint: 
created: 2026-07-29
closed:
links: [docs/plato-library-roadmap-ideas.md, submodules/Plato/stdlib/solids-polyhedra.plato, submodules/Plato/stdlib/meshes.plato, submodules/Plato/stdlib/meshes.concepts.plato, ara3d-sdk/src/Ara3D.Geometry/Primitives/PolygonMesh3D.cs, ara3d-sdk/src/Ara3D.Geometry/Primitives/ConwayOperators.cs, ara3d-sdk/src/Ara3D.Geometry/Primitives/Polyhedra.cs, ara3d-sdk/tests/Ara3D.SDK.GeometryTests/PolyhedraTests.cs, ara3d-sdk/examples/Ara3D.Studio.Examples/Generators/MeshGenerators.cs, plato-298, plato-273, ara3d-056, plato-301]
---

## Idea

Grow Plato's polyhedron story beyond named Platonic/Archimedean kinds into a **catalog + operator algebra + mesh export** path: common and exotic solids (Catalan duals, Johnson solids, Kepler–Poinsot stars, geodesic/Goldberg, zonohedra, prisms/antiprisms), the operators that produce them (Conway: truncate/ambo/kis/dual/…; optionally stellation and other classical ops), an explicit **dual** operation, and reliable construction of `TriangleMesh3D` / `PolygonMesh3D` from any solid in the catalog. Forward stdlib already declares `PlatonicSolid` / `ArchimedeanSolid` kinds and `Meshable3D.ToTriangleMesh`; C# Studio still seeds from handwritten `PlatonicSolids` tables. Interpretation: prefer compositional Conway programs over hand-authoring forty vertex tables, but keep a small seed table (five Platonics + parametric prism/antiprism) as the operator domain.

## Assumptions

- `Meshable3D` / `PolygonMesh3D` / topology helpers are the right sinks; polyhedra are solids that *become* meshes, not a parallel mesh format.
- A usable dual and Conway truncate/ambo need half-edge or equivalent adjacency (stdlib already sketches `HalfEdgeNavigable`, `Manifoldness`).
- Exotic non-convex / star polyhedra (Kepler–Poinsot, stellations) are in scope as catalog entries but may not satisfy `ConvexSolid` / `Manifold` markers — interfaces must allow that (ties to [plato-298](plato-298.md)).
- Studio generators (`PlatonicSolid` script) should eventually consume Plato/stdlib meshes rather than a permanent C#-only table.

## Design decisions

- **Catalog vs operators** — static vertex/face tables for every named solid vs seed Platonics + Conway programs (`tC`, `aD`, …). Tables are fast and exact for classics; operators scale and teach topology. Hybrid is likely: seeds + operator library + named aliases that expand to programs.
- **Dual representation** — dual as a pure function `Polyhedron → Polyhedron` (face centers → vertices) vs dual only after meshing. Prefer combinatorial dual on a manifold polygon mesh so Catalans fall out of Archimedeans.
- **Stellation scope** — full stellation series (many non-convex compounds) vs a few named Kepler–Poinsot + "stellate" as deferred. Stellation is geometrically heavier than Conway; treat as adjacent until Conway dual/truncate are solid.
- **Where kinds live** — extend `ArchimedeanSolidKind` / add `CatalanSolidKind` / `JohnsonSolidKind` enums vs one `NamedPolyhedron` + string/id table. Enums match current `solids-polyhedra.plato` style; a big Johnson enum is noisy — table + id may win past ~20.
- **Mesh arity** — always emit `PolygonMesh3D` then triangulate vs direct `TriangleMesh3D` for triangle-faced solids. Prefer polygon mesh as canonical polyhedron boundary; triangulation is `Meshable3D`.

## Related

- [docs/plato-library-roadmap-ideas.md](../../docs/plato-library-roadmap-ideas.md) §4 — already sketches Archimedean/Catalan/Johnson/Kepler–Poinsot + Conway operators; this issue owns that work item.
- [solids-polyhedra.plato](../../submodules/Plato/stdlib/solids-polyhedra.plato) — existing Platonic + Archimedean kind vocabulary (declarations).
- [meshes.plato](../../submodules/Plato/stdlib/meshes.plato) / [meshes.concepts.plato](../../submodules/Plato/stdlib/meshes.concepts.plato) — `PolygonMesh3D`, `TriangleMesh3D`, `Meshable3D`.
- [MeshGenerators.cs](../../ara3d-sdk/examples/Ara3D.Studio.Examples/Generators/MeshGenerators.cs) — Studio `PlatonicSolid` generator consuming C# tables today.
- [plato-298](plato-298.md) — polygon meshes with/without holes + manifold markers (dual/Conway preconditions).
- [plato-273](plato-273.md) — geometry libraries into stdlib (mesh/BREP adjacency context).
- [ara3d-056](ara3d-056.md) — capability lattice / solid tessellation interfaces.

## Approaches

Short term: (1) implement combinatorial dual + truncate/ambo on manifold `PolygonMesh3D` seeded from five Platonics; (2) name Archimedean/Catalan aliases as Conway strings; (3) `ToTriangleMesh` / keep polygon faces for display wireframes.

Long term: Johnson subset, geodesic/Goldberg (`uu(I)`-style), zonohedra from vector stars; optional stellation; Studio generators call stdlib instead of `PlatonicSolids.cs`.

Adjacent ideas: stellation algebra as its own issue; icosphere/Goldberg as a primitive demo; retire C# `PlatonicSolids` once Plato path is green.

## Case against

- **Conway is a research surface.** Correct truncate/snub/gyro on non-triangular faces has edge cases; a wrong operator library teaches bad geometry.
- **Named tables already work.** Studio only needs five Platonics + a few Archimedeans; operator generality may not pay for product use.
- **Depends on topology maturity.** Dual/Conway need trustworthy manifold adjacency — if [plato-298](plato-298.md) / half-edge bodies lag, this stalls or forks a second topology stack.
- **Star polyhedra pollute convex APIs.** Mixing Kepler–Poinsot into `ConvexSolid` breaks volume/contains assumptions.

**Verdict: pursue** the hybrid (Platonic seeds + Conway dual/truncate/ambo + named Archimedean/Catalan aliases + mesh export). **Park** full Johnson enumeration and stellation until Conway core is proven. **Drop** nothing from the long-term catalog — keep exotic entries as named follow-ups.

## Bedrock

Strengthens the **polyhedron → polygon mesh → triangle mesh** seam in `solids-polyhedra.plato` + `meshes.plato`: one combinatorial boundary type (`PolygonMesh3D`) that operators rewrite, duals swap, and `Meshable3D` triangulates — so Studio primitives and topology demos share one path instead of parallel C# tables. **Verdict: simplest-along-the-grain** — seed meshes + dual + truncate/ambo + aliases; must NOT hand-author full Archimedean vertex tables in the first pass, and must NOT fold stellation into the same change.

## Done means

- [x] Five Platonics construct as `PolygonMesh3D` (and triangulate via fan `ToTriangleMesh`) — C# `Polyhedra` (Plato stdlib port still open)
- [x] Combinatorial dual works on those seeds; Catalan duals of chosen Archimedeans match known face/vertex counts (`RhombicDodecahedron` = dual of cuboctahedron)
- [x] Truncate + ambo produce named Archimedean aliases from Platonic seeds (`Cuboctahedron`, `TruncatedCube`, …)
- [ ] Operators state manifold/orientable preconditions; non-convex/star solids are typed so they do not claim `ConvexSolid`
- [ ] Plato stdlib bodies for seeds + Conway (port from C# `Polyhedra` / `ConwayOperators`); Studio generators can call that path without a second ad-hoc table

## Progress

2026-07-30: Conway expand, snub and bevel landed in Plato stdlib
(`polyhedra-conway-expansion.library.plato`, submodule commits f20ddf0 / 5133e3d /
b92d4c2): expand and snub share the corner-numbered shrink(+twist) construction,
bevel = ta. New catalog files `polyhedra-catalog-expanded` (six remaining
Archimedeans, with uniformity constants — rhombicuboctahedron shrink = sqrt(2)-1,
rhombicosidodecahedron = phi/3, snubs solved numerically) and
`polyhedra-catalog-catalans` (remaining nine Catalan duals); all 26
`ArchimedeanSolidKind`/`CatalanSolidKind` dispatch arms in
`solids-polyhedra.library.plato` now return real meshes (no EmptyPolygonMesh arms
left). `Dual` upgraded to true plane-foot polar reciprocation (Newell normal), which
keeps the snub/expand duals' faces planar. Verified via independent Python mirror
over all 26 solids: V/E/F, Euler, manifold twins, outward winding, planarity <=
5e-16, edge spread <= 7e-16 for expand/snub (snub cube V=24 E=60 F=38). Gates:
lint --strict 0 parse / 0 resolution, PlatoTests ForwardStdLib filter 7/7. Bevel's
vertex faces are rectangles, not squares (uniform bevel unreachable by literal
truncation) — documented in the operator file.

2026-07-29: Initial hybrid landed in C# (`PolygonMesh3D` CSR matching Plato, `ConwayOperators.Dual`/`Ambo`/`Truncate`, `Polyhedra` seeds + Archimedean/Catalan aliases). Tests: `PolyhedraTests` 6/6. Forward stdlib: `CatalanSolid`/`CatalanSolidKind` vocabulary + docs on `PolygonMesh3D` / `solids-polyhedra.plato`. Plato bodies tracked separately as [plato-301](plato-301.md) (rewrite in small-function Plato style; C# is oracle/inspiration only — not a blind port).

## Simplest possible implementation

Hardcode Platonic vertex/face tables in Plato (or lift from C#), add `Dual(PolygonMesh3D)` + `Truncate`/`Ambo` on manifold polygon meshes, register Archimedean names as operator programs, expose `ToTriangleMesh`.

Pros: matches roadmap §4.2; small seed surface; exercises topology; immediate Studio value.
Cons: operator correctness risk; Johnson/stars deferred; C# `PlatonicSolids` dual-maintained until cutover.
