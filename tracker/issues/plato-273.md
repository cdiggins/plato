---
id: plato-273
title: Move geometry libraries into Plato stdlib (Earcut, CSG, BREP, Noise, Models)
type: idea
status: idea
priority: "?"
effort: "?"
risk: "?"
area: plato
sprint: 
created: 2026-07-28
closed:
links: [submodules/Plato/earcut, submodules/Plato/csg, labs/Ara3D.Noise, ara3d-sdk/src/Ara3D.Models, ara3d-sdk/examples/Ara3D.Studio.Examples/Demos/Brep.cs, submodules/Plato/stdlib/noise.plato, submodules/Plato/stdlib/scene3d.plato, docs/plato-library-roadmap-ideas.md, plato-028, ara3d-056, studio-168]
---

## Idea

Fold proven geometry libraries into the forward Plato stdlib (`submodules/Plato/stdlib`) so triangulation, solid boolean, BREP, procedural noise, and instanced models are first-class portable types — not sidecar modules or C#-only SDK/labs code. Candidate inventory:

- **Earcut** — already Plato at `submodules/Plato/earcut/` (`earcut.plato` + tests); migrate into stdlib vocabulary.
- **CSG** — already Plato at `submodules/Plato/csg/` (fold-based port of evanw/csg.js); migrate into stdlib.
- **BREP** — parametric BREP face/edge topology today lives mainly in Studio demos (`Brep.cs` / `BrepSolids.cs`), not a shipped `Ara3D.Geometry` package; target a Plato BREP library (pairs with the capability lattice in ara3d-056).
- **Noise** — `labs/Ara3D.Noise` (Ashima webgl-noise → HPC#); stdlib already has declaration-heavy `noise.plato` — fill bodies / align with the labs port.
- **Models / instanced geometry** — `ara3d-sdk/src/Ara3D.Models` (`IModel3D`, meshes + instances + render buffers). Either port that shape, or design a better Models abstraction for shared mesh + instance transforms (stdlib `scene3d.plato` already sketches retained scenes).

## Assumptions

- Forward stdlib (`stdlib`) is the home for portable geometry algorithms; sidecar folders (`earcut/`, `csg/`) were spikes, not the long-term layout.
- Earcut/CSG ports are mature enough to absorb after vocabulary/naming adaptation (interface-era types).
- BREP in Plato need not mirror OpenCascade — start from the demo BREP + tessellation path.
- Noise declarations in `noise.plato` are the contract; Ashima/labs implementations are the body source (and GLSL-friendly).
- Models may deserve a redesign (instancing + materials + bounds) rather than a 1:1 C# port — `scene3d.plato` vs `IModel3D` is an open seam.

## Design decisions

- **In-tree vs package** — fold Earcut/CSG into `stdlib/*.plato` vs keep as optional packages under `stdlib/` subfolders that lint still sees. Flat stdlib matches current lint (`TopDirectoryOnly`); subpackages need tooling changes.
- **BREP fidelity** — faceted/demo BREP (faces + edge uses + tessellate) vs full NURBS/topology BREP. Prefer faceted + parametric faces first.
- **Noise source of truth** — implement `noise.plato` interfaces from Ashima vs keep labs C# and only declare in Plato. Prefer Plato bodies so all backends share one definition.
- **Models shape** — port `IModel3D`/`InstanceStruct`/`RenderModelData` vs invent Plato `Model3D` as mesh pool + instance list (lighter than full `Scene3D`). Decide whether Models is a stdlib type or a Studio/render concern that consumes stdlib meshes.
- **Migration order** — Earcut → CSG → Noise bodies → BREP → Models (increasing design ambiguity).

## Related

- [submodules/Plato/earcut](../../submodules/Plato/earcut) — working Plato Earcut + FINDINGS (language gaps).
- [submodules/Plato/csg](../../submodules/Plato/csg) — working Plato CSG + PROGRESS.
- [labs/Ara3D.Noise](../../labs/Ara3D.Noise) — Ashima webgl-noise C# port.
- [ara3d-sdk/src/Ara3D.Models](../../ara3d-sdk/src/Ara3D.Models) — current Models/instancing API.
- [Brep.cs](../../ara3d-sdk/examples/Ara3D.Studio.Examples/Demos/Brep.cs) — demo BREP types to lift.
- [noise.plato](../../submodules/Plato/stdlib/noise.plato) — stdlib noise declarations (bodies thin/missing).
- [scene3d.plato](../../submodules/Plato/stdlib/scene3d.plato) — retained scene; overlaps Models.
- [docs/plato-library-roadmap-ideas.md](../../docs/plato-library-roadmap-ideas.md) — noise/CSG-via-SDF roadmap notes.
- [plato-028](plato-028.md) — Earcut-exposed language/runtime gaps.
- [ara3d-056](ara3d-056.md) — geometry capability lattice including BREP/models → Plato interfaces.
- [studio-168](studio-168.md) — flowable types (brep among them).

## Approaches

Short term: (1) move Earcut into stdlib with v3 naming + keep existing tests as conformance; (2) same for CSG; (3) implement Perlin/Worley bodies behind `noise.plato` from labs/Ashima.

Long term: BREP as a stdlib module feeding tessellation/CSG; a clear Models/instancing type that Studio and codegen share; retire sidecar `earcut/`/`csg/` folders and eventually thin C# duplicates.

Adjacent ideas worth their own issue: GLSL noise backend parity; retire `labs/Ara3D.Noise` once stdlib lands; Models vs Scene3D ADR.

## Case against

- **Stdlib bloat.** Earcut/CSG/BREP are large algorithms; shipping them in every consumer increases compile/codegen surface. Optional packages may be healthier.
- **Dual-library tax.** Earcut/CSG already work beside stdlib-legacy; forcing a v3 port before vocabulary/settling (plato-268) risks a second rewrite.
- **BREP is not Geometry yet.** Demo BREP is not a productized `Ara3D.Geometry` API — promoting it to stdlib may lock in a half-baked topology model.
- **Models may be host-shaped.** `RenderModelData` / GPU buffers are Studio concerns; stuffing them into Plato fights purity and portability.
- **Noise already declared.** Filling bodies is a feature, not a “move labs in” story — conflating them muddies scope.

**Verdict: pursue** as a staged migration (Earcut/CSG first; Noise bodies next; BREP/Models only after a design spike). Park full Models port until Scene3D vs Model3D is decided. Drop nothing from the inventory — track order separately.

## Bedrock

Strengthens the **stdlib as the single portable geometry algorithm home**: spikes (`earcut/`, `csg/`) and C# labs/demos become stdlib modules with shared types, so C#/TS/Rust/GLSL writers and Studio stop maintaining parallel copies. **Verdict: simplest-along-the-grain** — migrate Earcut+CSG first as drop-in stdlib files; must NOT redesign Models/BREP topology in the same change, and must NOT delete working sidecar tests until stdlib equivalents are green.

## Done means

- [ ] Earcut lives under `stdlib` (or documented stdlib package), lint green, existing earcut tests still pass via regenerated path
- [x] CSG likewise migrated — first stdlib increment shipped 2026-07-30 (`solids-csg-*.plato`, polygon-soup Union/Intersection/Difference via plane clipping; BSP variant + executable tests deferred); old `csg/csg.plato` was inspiration only, retirement still open
- [ ] Noise: at least Perlin + Worley (2D/3D) have Plato bodies satisfying `noise.plato` interfaces; labs noted as superseded or kept as reference
- [x] Written decision: BREP scope — resolved by [plato-302](plato-302.md), shipped 2026-07-30: faceted demo lift (edge-use shell, `BrepCurve = Line` / `BrepSurface = Planar | Bilinear` sums, `brep*.plato` in stdlib); trims/booleans/NURBS explicitly out. Models vs Scene3D still open
- [ ] Sidecar folders / labs retirement plan recorded (delete, archive, or leave as conformance hosts)

## Simplest possible implementation

`git mv` / re-home `earcut.plato` and `csg.plato` into `stdlib/` with minimal renames to compile under v3 interfaces; wire regen scripts to stdlib; leave BREP/Noise/Models as follow-up issues once those two are green.

Pros: reuses proven ports; immediate consolidation signal; small design risk.
Cons: may need vocabulary patches; BREP/Models remain unresolved; lint/tooling may need path updates.
