---
id: plato-301
title: Implement Plato polyhedra library (seeds + Conway) in small-function style
type: feature
status: done
priority: "?"
effort: "?"
risk: "?"
area: plato
sprint: 
created: 2026-07-29
closed: 2026-07-29
links: [tracker/issues/plato-297.md, ara3d-sdk/src/Ara3D.Geometry/Primitives/ConwayOperators.cs, ara3d-sdk/src/Ara3D.Geometry/Primitives/Polyhedra.cs, ara3d-sdk/src/Ara3D.Geometry/Primitives/PolygonMesh3D.cs, submodules/Plato/stdlib/solids-polyhedra.plato, submodules/Plato/stdlib/meshes.plato, submodules/Plato/stdlib/LIBRARIES.md, docs/plato-library-roadmap-ideas.md, plato-298]
---

## Idea

Implement the hybrid polyhedra catalog **in Plato** (`stdlib/`): Platonic seed `PolygonMesh3D` tables, Conway `Dual` / `Ambo` / `Truncate`, named Archimedean/Catalan aliases, and triangle/polygon mesh export. The existing C# staging (`Polyhedra`, `ConwayOperators`, handwritten `PolygonMesh3D`) is **inspiration and a behavioral oracle only** — not a template to translate line-for-line. Author in Plato house style: many tiny pure functions, composed; prefer expression-bodied helpers over large imperative blocks with dictionaries/mutation.

## Assumptions

- [plato-297](plato-297.md) defines the product goal; this issue is the Plato-body work item.
- Forward `PolygonMesh3D` CSR + `Meshable3D` / `solids-polyhedra.plato` kinds (incl. `CatalanSolid`) are the vocabulary home.
- C# tests in `PolyhedraTests` can remain the count/Euler oracle while Plato gains its own laws/witnesses later.
- Affine `List` builders must not be captured in lambdas (`LIBRARIES.md`); algorithms should use `MapRange` / `FlatMap` / `Reduce` and small named steps like earcut did.

## Design decisions

- **Rewrite vs port** — **rewrite in Plato style** (chosen). C# may suggest edge cases and expected V/F/E counts; structure should be decomposed (face arity, directed edge, vertex ring, face centroid, CSR pack, …) rather than one `Dual`/`Ambo`/`Truncate` megafunction mirroring `ConwayOperators.cs`.
- **Library file split** — one `solids-polyhedra.library.plato` vs `meshes-polygon.library.plato` (CSR accessors) + `polyhedra-conway.library.plato` (operators) + seed file. Prefer split if any file would exceed the ~12-decl / readability cap (`LIBRARIES.md`).
- **Where seeds live** — literal tables in Plato vs generate from Frame+Radius only for named kinds. Literals for five Platonics match the hybrid plan; parametric prism/antiprism can stay separate.
- **Oracle** — keep C# until Plato conformance exists, then thin or retire staging (`PlatonicSolids` / `Polyhedra` cutover is a later step, not required to close this).

## Related

- [plato-297](plato-297.md) — umbrella catalog idea; this feature closes its Plato-body gap.
- [ConwayOperators.cs](../../ara3d-sdk/src/Ara3D.Geometry/Primitives/ConwayOperators.cs) / [Polyhedra.cs](../../ara3d-sdk/src/Ara3D.Geometry/Primitives/Polyhedra.cs) — C# staging / oracle (inspiration only).
- [PolygonMesh3D.cs](../../ara3d-sdk/src/Ara3D.Geometry/Primitives/PolygonMesh3D.cs) — CSR shape already mirrors `stdlib/meshes.plato`.
- [solids-polyhedra.plato](../../submodules/Plato/stdlib/solids-polyhedra.plato) / [meshes.plato](../../submodules/Plato/stdlib/meshes.plato) — vocabulary.
- [LIBRARIES.md](../../submodules/Plato/stdlib/LIBRARIES.md) — library file rules, small functions, no lambda-captured builders.
- [plato-298](plato-298.md) — manifold / holes; operators assume hole-free manifold seeds for v1.
- [docs/plato-library-roadmap-ideas.md](../../docs/plato-library-roadmap-ideas.md) §4 — Conway catalog sketch.

## Approaches

Short term: CSR helpers (`FaceCount`, `FaceArity`, `FaceVertexAt`, `FaceCentroid`, pack faces→CSR, fan `ToTriangleMesh`) as tiny functions; Platonic seeds; `Dual` composed from centroid + vertex-figure helpers; then `Ambo` / `Truncate`; aliases as one-liners.

Long term: more Conway ops (kis, gyro, …); Plato laws for V/F/E; Studio generators consume Plato-emitted meshes; retire duplicate C# tables.

Adjacent: half-edge builder from `PolygonMesh3D` (shared with repair/analysis); retire C# `Polyhedra` once Plato is golden.

## Case against

- **C# already works.** Porting risks a second maintenance surface before Studio needs Plato meshes.
- **Topology in pure Plato is hard.** Vertex rings / twin edges without mutation may force awkward `Reduce` folds; a blind port of dictionary-heavy C# would be worse than keeping C#.
- **stdlib still declaration-heavy.** Filling Conway before half-edge builders exist may invent a parallel adjacency story ([plato-298](plato-298.md)).

**Verdict: pursue** — user explicitly wants the Plato library, with style constraints that avoid a slavish port. Park extra Conway ops and Studio cutover until Dual/Ambo/Truncate + seeds lint and match oracle counts.

## Bedrock

Strengthens the **Plato polyhedron → `PolygonMesh3D` → triangle mesh** path in `stdlib/` as the source of truth for catalog solids, with C# demoted to oracle/consumer. Small-function decomposition makes each adjacency step reusable for later half-edge / repair work. **Verdict: simplest-along-the-grain** — seeds + Dual/Ambo/Truncate + aliases as composed helpers; must NOT paste `ConwayOperators.cs` into one Plato function, must NOT add Johnson/stellation in the same change, must NOT delete working C# until Plato counts match.

## Done means

- [x] Platonic seeds construct as `PolygonMesh3D` in Plato stdlib library bodies
- [x] `Dual`, `Ambo`, `Truncate` exist as compositions of small named helpers (not monolithic ports); lint 0 parse / 0 resolution on `stdlib`
- [x] Named aliases (at least cuboctahedron / truncated cube / one Catalan dual) match known V/F counts (oracle: independent mirror, see below — NOT `PolyhedraTests`, which encodes two bugs this work found)
- [x] Fan (or documented) `ToTriangleMesh` for `PolygonMesh3D` / relevant solids
- [x] Issue notes which C# staging files remain temporary vs scheduled for retire

## Outcome (2026-07-29)

Six library files, submodule commit `af58463`:

| File | Holds |
|---|---|
| `meshes-polygon.library.plato` | face / corner accessors over the Jagged rows |
| `meshes-polygon-corners.library.plato` | corner-as-half-edge navigation, vertex rings, edge numbering |
| `meshes-polygon-building.library.plato` | pack via `FromRows`, position maps, fan `ToTriangleMesh` |
| `polyhedra-seeds.library.plato` | the five Platonic tables |
| `polyhedra-conway.library.plato` | `Dual` / `Ambo` / `Truncate` |
| `polyhedra-catalog.library.plato` | 11 named Archimedean + Catalan aliases |

**The idea that made it small.** A face corner's global index in the CSR packing already
IS a half-edge id: corner `c` runs from its own vertex to the next corner of the same face.
Twins are a search, vertex rings a rotation (`NextCorner(TwinCorner(c))`), edges numbered by
the rank of the lower corner of each twin pair. So no operator builds a lookup table — the
dictionaries in `ConwayOperators.cs` have no counterpart here — and the new vertex numbering
of each operator falls out for free (dual = per face, ambo = per edge, truncate = per corner).

**CSR came from [plato-303](plato-303.md), not from a private clone.** The `PackFaces` seam
this issue planned for became a direct call to that library's `FromRows`; row access is its
`RowCount` / `RowLength` / `Row`.

**Two bugs found in the C# oracle** (both live in `ConwayOperators.cs`, both corrected here,
neither fixed in C# yet):

1. `Dual` takes the vertex ring unreversed, so every dual face winds inward.
2. All three operators call `WithNormalizedPoints`. Normalizing onto a common radius is only
   meaning-preserving for a **vertex-transitive** solid; a Catalan dual is face-transitive, so
   normalizing bends every face out of plane (rhombic dodecahedron faces were off-plane by
   0.21). The Plato `Dual` places vertices by **polar reciprocation** (`c / |c|^2`) instead,
   which makes dual faces planar by construction; no operator normalizes.

**Verification.** `.\tools\check-stdlib-fast.ps1` — **both gates PASS**: `lint --strict` over
`stdlib` (0 parse, 0 resolution errors; findings 2666 → 2665) and the checker ratchet
(`ForwardStdLibDiagnosticCountDoesNotRegress`, i.e. these six files add no checker diagnostics).
The gate was blocked for most of this work by a concurrent session's in-flight compiler edits and
was run once the tree built again.

Behaviour was checked separately, since no Plato path executes these bodies yet (Stage 2 codegen,
[plato-308](plato-308.md)): a line-for-line mirror of the six library files over 19 cases —
V/F/E, Euler characteristic, outward winding, face planarity, manifoldness, and face-arity
histograms (cuboctahedron 8 triangles + 6 squares, truncated icosahedron 12 pentagons + 20
hexagons, rhombic dodecahedron 12 rhombi, icosidodecahedron 20 + 12, ...). All 19 pass; the two
oracle bugs above are what that mirror caught. A mirror is not the library — Plato laws replace
it when the forward suite can run.

**C# staging status.** Nothing retired. `Polyhedra.cs`, `ConwayOperators.cs` and
`PolygonMesh3D.cs` all remain live and are the only path Studio has today; they are now
*divergent* rather than merely duplicated, because of the two corrections above. Retirement
waits on the Plato path being executable (Stage 2 codegen, [plato-308](plato-308.md)).

## Follow-ups

- Fix the two oracle bugs in `ConwayOperators.cs`, or mark it superseded — right now
  `PolyhedraTests` would ratify inside-out duals.
- `PlatonicSolid` / `ArchimedeanSolid` / `CatalanSolid` in `solids-polyhedra.plato` are still
  inert: no `ToTriangleMesh`, no `Kind` dispatch, no `Frame`/`Radius` placement. The catalog
  library names meshes, not those types.
- Remaining Archimedeans need `snub` / `expand` / `bevel`; the `*Kind` sums enumerate all
  thirteen and are ahead of the operators on purpose.
- The twin search is linear, so the walks are quadratic in corner count. Fine for catalog
  solids (180 corners at the largest), wrong for a general mesh — that is the half-edge builder
  in [plato-298](plato-298.md).

## Simplest possible implementation

CSR accessors + five seed tables + `Dual` only, validated against cube↔octahedron counts; add Ambo/Truncate in a follow-up commit in the same issue.

Pros: proves Plato style early; smallest adjacency surface.
Cons: Archimedean aliases wait; C# stays primary for Studio longer.
