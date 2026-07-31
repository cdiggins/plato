---
id: plato-318
title: Plato mesh builders (TriangleMesh3DBuilder / QuadMesh3DBuilder)
type: idea
status: idea
priority: "?"
effort: "?"
risk: "?"
area: plato
sprint: 
created: 2026-07-29
closed:
links: [plato-353, plato-354, ara3d-sdk/src/Ara3D.Geometry/MeshConstruction/TriangleMesh3DBuilder.cs, ara3d-sdk/src/Ara3D.Geometry/MeshConstruction/QuadMesh3DBuilder.cs, submodules/Plato/docs/affine-types.md, submodules/Plato/stdlib/primitives-builders.plato, submodules/Plato/stdlib/meshes.plato, plato-277, plato-262]
---

## Idea

Give Plato a way to incrementally assemble `TriangleMesh3D` / `QuadMesh3D` the way Studio already does in C# via `TriangleMesh3DBuilder` / `QuadMesh3DBuilder`. Today Plato only constructs meshes from frozen arrays (`PolygonMeshOfFaces`, tuple constructors). Open question: whether that means new `unique` mesh-builder types, or helpers over the existing `List<T>` / `Buffer<T>` affine builders.

## Why new `unique` types are blocked today (not a language gap)

The parser already accepts `unique type Foo { }`. The ban is intentional Phase-6 scoping:

1. **Allow-list** — `PlatoCompiler/Symbols/UniqueTypes.cs` hard-codes `Names = { List, Buffer }`. `SymbolFactory` rejects any other name with a resolution error.
2. **No generated struct** — unique types are not emitted as C# structs; they must map to handwritten Intrinsics (`PlatoList` / `PlatoBuffer`).
3. **Name map** — `CSharpWriter.UniqueTypeCSharpNames` remaps Plato `List`→`PlatoList`, `Buffer`→`PlatoBuffer` (avoids shadowing `System.Collections.Generic.List` / `System.Buffer` in `Ara3D.Geometry`).
4. **Effect table** — observe / mutate / consume method sets live in `UniqueTypes` (for a future static affine pass). Today enforcement is runtime (frozen flag) plus LINT006/007 (no field storage, no unique-in-generic).
5. **Static affine checking is deferred** — `docs/affine-types.md`: occurrence counting, use-after-consume, lambda capture bans are not implemented yet. Widening the set of unique types without that pass multiplies "use after Freeze" footguns that only throw at runtime.

So you *can* write `unique type TriangleMesh3DBuilder` syntactically; the compiler refuses it because Phase 6 only shipped two host-backed builders.

### Compiler / runtime work to allow more unique types

| Slice | What changes | Size |
|-------|----------------|------|
| A. Allow-list + emit map | Add name(s) to `UniqueTypes.Names`, effect dictionaries, `UniqueTypeCSharpNames`; skip struct emit (already gated on `IsUnique`) | Small |
| B. Handwritten Intrinsics | New `PlatoTriangleMesh3DBuilder` (etc.) in `Plato.Intrinsics.V2` with freeze flag, mutators returning `this`, `Freeze()` → `TriangleMesh3D` | Medium |
| C. Bodiless host API | Declare `unique type` + library signatures in `stdlib` (like `primitives-builders*.plato`) | Small |
| D. Optional: generalize beyond allow-list | Let any empty `unique type` map via a convention / attribute instead of a hard-coded name set | Medium — design choice |
| E. Static affine pass | Occurrence counting on CFG using the effect table (`docs/affine-types.md` deferred items) | Large — separate workstream; not required to *run* new builders, required to make them *safe* |

A–C are enough for a mesh builder that works like `List`. E is the real language debt.

### Risk

- **Low (compose `List` only, no new unique types):** No compiler change. Mesh assembly is `List<Point3D>` + `List<TriangleFace>` then construct the value type. Loses face-handle / soft-delete API; fine for most generators.
- **Med (new unique types, A–C only):** Same safety model as today's `List` — runtime use-after-freeze, LINT006/007. Risk is proliferation of handwritten Intrinsics and another special-case in the writer allow-list; every new unique type is another type that cannot live in fields or containers.
- **Med (unique face handle with ownership transfer):** Sound for single-face edit chains; multi-face APIs (`Bridge`, `GetFaces`) must be redesigned around indices. Opaque Intrinsics only — no Plato field nesting of unique types.
- **High (widen unique without static checking, or port C# shared-ref handles as-is):** Alias bugs that only show up as runtime exceptions; or pressure to weaken LINT006/007. Shipping many unique types before the static pass makes the eventual checker harder (more APIs to classify).

## Assumptions

- Incremental mesh assembly is a real stdlib need (polyhedra, extrusions, procedural generators), not only a Studio C# convenience.
- Immutable `TriangleMesh3D` / `QuadMesh3D` remain the interchange types; builders are consume-once.
- Rich Quad face-handle editing may stay in `Ara3D.Geometry` even if Plato gets a thin triangle builder.

## Design decisions

- **Compose `List`/`Buffer` vs new `unique` mesh builders** — compose needs no compiler work; new unique types need A–C (+ Intrinsics).
- **How much of `QuadMesh3DBuilder` to port** — thin AddPoint/AddFace/Freeze vs face-local edit ops (inset/extrude/subdivide) vs soft-delete / bridge.
- **Face handles: C# shared-ref vs affine ownership transfer** — see below.
- **Where the builder lives** — forward `stdlib/` only vs also Intrinsics + C# writer maps.
- **Whether to generalize the unique allow-list** — keep hard-coded names vs a convention for arbitrary unique host types.

### Face handles under uniqueness (2026-07-29)

C# `Quad3DFaceHandle` is `(Builder, Index)` with a *shared* builder reference — many handles may alias one builder. That pattern is illegal as Plato fields (LINT006) and breaks affine “one owner” semantics.

A coherent alternative: declare **`unique type Quad3DFaceHandle`** that *temporarily owns* the builder.

1. `GetFace(builder, i)` **consumes** the unique builder → unique handle (builder + index live in opaque Intrinsics state, not Plato fields).
2. Face ops (`Inset`, `Extrude`, …) take/return the unique handle (still sole owner).
3. `Release` / `ToBuilder(handle)` **consumes** the handle → unique builder again.

Fits single-face chains (e.g. Panel demo). Does **not** fit unmodified multi-handle APIs:

- `Bridge(faceA, faceB)` — two handles cannot both uniquely own one builder; need `Bridge(builder, indexA, indexB)` or “handle owns builder + other face is an `Integer` index.”
- `GetFaces()` → list of handles — LINT007 bans unique values in generic containers; iterate by index on the builder instead.

So handles are not ruled out — only the C# *shared-ref* shape is. Affine handles are a real option if we accept index-based multi-face APIs.

## Related

- [TriangleMesh3DBuilder.cs](../ara3d-sdk/src/Ara3D.Geometry/MeshConstruction/TriangleMesh3DBuilder.cs) — thin C# prior art (points + faces + `ToTriangleMesh3D`).
- [QuadMesh3DBuilder.cs](../ara3d-sdk/src/Ara3D.Geometry/MeshConstruction/QuadMesh3DBuilder.cs) — rich C# prior art (handles, groups, ops).
- [affine-types.md](../submodules/Plato/docs/affine-types.md) — Phase 6 scope + deferred static pass.
- [primitives-builders.plato](../submodules/Plato/stdlib/primitives-builders.plato) — only allowed unique types today.
- [meshes.plato](../submodules/Plato/stdlib/meshes.plato) — target immutable mesh types.
- [plato-277](plato-277.md) — landed affine builders / type-var work that made `List` usable.
- [plato-262](plato-262.md) — C++/CUDA path noted mesh builders as a remaining large-array consumer (orthogonal host).

## Approaches

Short term:

1. **Library helpers over `List`** — `AddTriangle` / `AddQuad` that take two lists (or return updated pairs), `ToTriangleMesh3D(positions, faces)` after `Freeze`. Zero compiler work.
2. **Thin `unique type TriangleMesh3DBuilder`** — Intrinsics wrapping two internal arrays (or composing two PlatoLists privately in C#), `AddPoint` / `AddFace` / `Freeze` → `TriangleMesh3D`. Compiler allow-list + one Intrinsics class.
3. **Unique Quad builder + ownership-transferring face handle** — opaque `unique` builder and `unique Quad3DFaceHandle`; multi-face ops take indices. More Intrinsics surface; preserves Panel-style single-face chains.
4. **Leave rich Quad builder in Ara3D.Geometry** — Plato consumers that need bridge/subdivide keep calling C#.

Long term: static affine pass (E) so more unique builders are cheap to add safely; optionally generalize allow-list (D).

Adjacent ideas:

- Static affine occurrence checker (spin off when promoting).
- `PolygonMesh3D` builder over jagged/`List<List<…>>` (blocked harder by LINT007).

## Bedrock

Strengthens the builder seam already named in `primitives-builders.plato`: immutable mesh values are assembled only through affine accumulation then consume. Prefer composing `List` so the unique-type surface stays tiny until the static pass exists.

Verdict: **simplest-along-the-grain** — ship List-composed mesh assembly first; do **not** weaken LINT006/007 or port shared-ref C# handles. Affine ownership-transferring handles remain a later option once a thin builder proves useful.

## Done means

- [ ] Documented choice: compose-`List` vs new unique type(s), recorded here or in an ADR
- [ ] Forward stdlib can build a non-trivial `TriangleMesh3D` incrementally (at least AddPoint + AddFace + freeze/construct)
- [ ] `.\tools\check-stdlib-fast.ps1` green; if new unique types: Intrinsics + writer map + conformance slice
- [ ] Explicit choice noted for Quad handles: out of scope / C# only / affine ownership-transfer (or a follow-up issue)

## Simplest possible implementation

Helper library in `stdlib`: functions that `Add` to `List<Point3D>` / `List<TriangleFace>`, then `TriangleMesh3D(positions.Freeze(), faces.Freeze())`.

Pros:

- No compiler / Intrinsics change
- Reuses proven `List` freeze semantics
- Matches thin `TriangleMesh3DBuilder`

Cons:

- Two builders to thread (or awkward tuple rebinding)
- No face handles / soft-delete / Quad edit ops
- Call sites slightly noisier than a dedicated builder type

## Note (2026-07-30)
User reaffirmed: definitely need QuadMeshBuilder and should use it. Keep this issue as the home for TriangleMesh3DBuilder / QuadMesh3DBuilder — do not file a duplicate.
