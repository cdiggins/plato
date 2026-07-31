---
id: plato-302
title: Plato BREP: edge-use shell with geometry sum types
type: idea
status: done
priority: "?"
effort: "?"
risk: "?"
area: plato
sprint: 
created: 2026-07-29
closed: 2026-07-30
links: [ara3d-sdk/examples/Ara3D.Studio.Examples/Demos/Brep.cs, ara3d-sdk/examples/Ara3D.Studio.Examples/Demos/BrepSolids.cs, submodules/Plato/stdlib/topology-half-edges.plato, submodules/Plato/stdlib/topology-indices.plato, submodules/Plato/stdlib/surfaces-solids.concepts.plato, plato-273, ara3d-056, ara3d-032, studio-168, plato-298]
---

## Outcome (2026-07-30)

Shipped to Plato main (branch `plato-stdlib-improvements`, commit `4356c81`, merged in `1f55a6c`). Five stdlib files: `brep.concepts.plato` (BoundaryRepresentation marker + edge-use/closedness conventions), `brep.plato` (9 types: Brep3D, BrepEdgeUse, `BrepCurve = Line`, `BrepSurface = Planar | Bilinear`, loops/faces/validation), `brep.library.plato` (22 fns incl. IsWatertight, EulerCharacteristic, Validate, Deform), `brep-tessellate.library.plato` (full-UV-grid tessellation, loop clipping ignored as in demo), `brep-primitives.library.plato` (StraightEdge, QuadPatch, hand-verified watertight Box). Gates: lint 0 errors, checker ratchet 0 diagnostics at ceiling 0. All Done-means satisfied; plato-273 cross-link recorded there. Found in passing: qualified sum-type payload ctors (`BrepCurve.Line(x)`) fail CHK201 — filed as [plato-332](plato-332.md).

## Idea

Add a first-class parametric BREP to Plato `stdlib`: an immutable shell of vertices + undirected edges + faces, where faces bound themselves with oriented **edge-uses** (not half-edges), and edge/face *geometry* lives in **sum types** (`BrepCurve` / `BrepSurface`) rather than concept-typed fields. Lift the Studio demo (`BrepSolid` / `BrepEdge` / `BrepFace`) into Plato vocabulary; keep `HalfEdgeMesh` as the discrete tessellation target. Generative solids (`ExtrudedSolid`, …) stay separate and convert *to* BREP; `Deform` decays symbolic structure.

## Assumptions

- Plato remains the home for portable geometry vocabulary; C# demo is a sketch, not the product API ([plato-273](plato-273.md)).
- Concepts are constraints, not storage — you cannot put “any `Curve3D`” in a homogeneous array field; sum types are the representable path.
- Typed indices (`VertexIndex` / `EdgeIndex` / `FaceIndex` in `topology-indices.plato`) are the index convention ([ara3d-032](ara3d-032.md)).
- Markers (`ClosedShell`, `BrepSolid`) must be proofs, not hopes — lesson from the stale `IsBilinear` flag that motivated [ara3d-056](ara3d-056.md).
- Full CAD kernel (NURBS, pcurves, trims, BREP booleans) is out of scope; [studio-168](studio-168.md) already parks “real BREP” as XL.
- Earcut/CSG stdlib migration ([plato-273](plato-273.md) order) can land before or beside BREP; BREP booleans wait on CSG.

## Design decisions

- **Topology encoding — edge-use loops vs half-edges for BREP.** Edge-use: one undirected edge owns one curve; two faces cite it with opposite orientation. Half-edge: duplicates the undirected edge; awkward when geometry lives once. **Prefer edge-uses for BREP; keep `HalfEdgeMesh` for meshes.**
- **Geometry payload — sum types vs concept fields vs only-faceted.** Sums (`Line | Arc | Polyline`, `Planar | Bilinear | QuadGrid`) fit Plato and stay extensible by adding cases. Concept fields are not storable. Faceted-only is a valid v1 *subset* of the same sums.
- **Closedness — marker on `Brep3D` vs validated subtype.** Do not stamp `ClosedShell` on every `Brep3D`. Prefer `AsClosedShell` / `ClosedBrep3D` produced only after `IsWatertight` (or a builder that proves it).
- **Generative solids vs BREP.** `ExtrudedSolid.ToBrep` is exact/cheap/lossless (implicit-conversion candidate later). `Deform` on extrusion decays to generic BREP — tessellation hints go with the type change ([ara3d-056](ara3d-056.md)).
- **Trimming.** v1: topological holes via `Holes: Array<BrepLoop>` on faces; no UV-domain restriction / pcurves until needed.
- **File split.** `brep.concepts.plato`, `brep.plato`, `brep.library.plato`, `brep-tessellate.library.plato`, `brep-primitives.plato`, `brep-from-solids.library.plato` — match stdlib `*.library.plato` convention; do not overload half-edge `BoundaryLoop`.

## Related

- [plato-273](plato-273.md) — parent “geometry into stdlib”; this issue is the BREP design spike it calls for before promoting BREP.
- [ara3d-056](ara3d-056.md) — capability lattice (`IBrep` / shells / `IBrepSolid` / swept→BREP); this is the Plato-side encoding.
- [ara3d-032](ara3d-032.md) — typed indices; demo BREP still uses raw `int`.
- [studio-168](studio-168.md) — flowable brep; XL for a real kernel — this design intentionally stays below that bar.
- [plato-298](plato-298.md) — polygon faces with holes; BREP face loops should map cleanly onto that mesh story.
- [Brep.cs](../../ara3d-sdk/examples/Ara3D.Studio.Examples/Demos/Brep.cs) / [BrepSolids.cs](../../ara3d-sdk/examples/Ara3D.Studio.Examples/Demos/BrepSolids.cs) — source sketch to lift.
- [topology-half-edges.plato](../../submodules/Plato/stdlib/topology-half-edges.plato) — adjacent, not the BREP encoding.
- [surfaces-solids.concepts.plato](../../submodules/Plato/stdlib/surfaces-solids.concepts.plato) — `Solid` / `ParametricSurface` markers BREP concepts refine.

## Approaches

Short term: (1) declare `Brep3D` + `EdgeUse` + `Line`/`Planar`/`Bilinear` sums + `IsWatertight` / `Validate` / `Tessellate` / `Deform` / `Box`; (2) typed indices throughout; (3) no `ClosedShell` on the open type until a validated constructor exists.

Long term: Arc/Polyline/QuadGrid cases; `ExtrudedSolid.ToBrep`; closed proof type + conformance laws; optional flowable BREP in Studio once the type exists; BREP⊃CSG only after CSG is stdlib.

Adjacent ideas worth their own issue: BREP builder (`unique` List freeze vs pure append); law library `Law_Brep_*`; Studio `IFlowable` for `Brep3D` (subset of studio-168).

## Case against

- **Locks a half-baked topology.** Promoting the demo model into stdlib may ossify edge-uses before IFC/STEP interop teaches what we actually need (pcurves, non-manifold, shells with multiple voids).
- **Sum-type tax.** Every new curve/surface kind is a cross-cutting `match` in Eval/Deform/Tessellate; concept-dispatch would scale better if Plato ever got existentials / type-erased procedurals.
- **Overlap with `HalfEdgeMesh`.** Two topology stories confuse agents; a thinner “attributes on half-edges” design might be enough for faceted solids.
- **No Studio consumer yet.** [studio-168](studio-168.md) parks flowable brep; without a sink, stdlib BREP is vocabulary without demand.
- **Scope creep into OCCT.** Once “BREP” exists, pressure to add trims/booleans/NURBS will fight the faceted-first plan.

**Verdict: pursue** the poly/faceted BREP (Line + Planar/Bilinear, edge-uses, separate from half-edge) as the design that satisfies plato-273’s “written decision: BREP scope.” **Park** trimming, BREP booleans, and flowable Studio wiring until Earcut/CSG are in stdlib and a concrete consumer appears. **Drop** encoding BREP as half-edge attributes and any OCCT-parity goal for this issue.

## Bedrock

Strengthens the **exact-vs-discrete seam**: generative/`Brep3D` stay resolution-independent; `HalfEdgeMesh` / `TriangleMesh3D` are explicit tessellation products. Also hardens the **marker-as-proof** rule (`ClosedShell` only after watertightness) so stale geometric claims stay unrepresentable. **Verdict: simplest-along-the-grain** — ship Line + Planar/Bilinear `Brep3D` with validate/tessellate/deform/Box; must NOT add UV trimming, BREP booleans, or stamp `ClosedShell` on every shell in the same change.

## Done means

- [ ] `brep.concepts.plato` + `brep.plato` lint clean under `stdlib` with edge-use topology and `BrepCurve`/`BrepSurface` sums (at least Line + Planar/Bilinear)
- [ ] Library ops: `IsWatertight`, `Validate`, `Deform`, `Tessellate`, `Box` (or equivalent primitive)
- [ ] Documented convention: BREP uses edge-uses; `HalfEdgeMesh` is tessellation IR — no overload of half-edge `BoundaryLoop`
- [ ] Written call on closedness: validated subtype / Optional proof, not unconditional `ClosedShell` on `Brep3D`
- [ ] Cross-links from plato-273 Done-means “BREP scope” checkbox to this issue’s outcome

## Simplest possible implementation

Declare `Brep3D` with only `BrepCurve = Line` and `BrepSurface = Planar | Bilinear`, plus `IsWatertight` / `Validate` / `Tessellate` / `Deform` / `Box`, using existing typed indices. No closed marker type, no `ToBrep` from solids, no holes array required (empty ok).

Pros:
- Direct port of the working demo into Plato’s type system
- Exercises sum-type geometry without inventing existentials
- Unblocks plato-273’s BREP-scope decision

Cons:
- Faceted only — curved cylinders wait on Arc/CylinderPatch cases
- No proof-carrying closed type yet
- Tessellate may still ignore loop clipping (full UV grid) as the demo does
