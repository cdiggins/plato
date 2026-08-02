---
id: plato-303
title: CompressedSparseRow type/interface and library
type: idea
status: done
priority: "?"
effort: "?"
risk: "?"
area: plato
sprint: 
created: 2026-07-29
closed: 2026-07-29
links: [submodules/Plato/stdlib/meshes.plato, submodules/Plato/stdlib/topology-adjacency.plato, submodules/Plato/stdlib/CONVENTIONS.md, submodules/Plato/stdlib/README.md, tracker/issues/plato-301.md, tracker/issues/plato-298.md, ara3d-sdk/src/Ara3D.Geometry/Primitives/PolygonMesh3D.cs]
---

## Idea

Add a first-class Plato **Compressed Sparse Row (CSR)** abstraction — a `type` and/or `interface`, plus a small library of accessors and builders — so jagged collections (variable-length rows packed into one flat array + offsets) are not re-invented ad hoc. Today CSR appears informally: `PolygonMesh3D` stores `FaceOffsets` + `FaceVertices` with half-open slices documented in comments; CONVENTIONS/README already carve out “CSR/offset arrays stay `Array<Integer>`” (boundaries, not typed element refs). Goal: one reusable seam for meshes, adjacency, and any other ragged lists, with tiny helpers (`RowCount`, `RowArity`, `At`, `Slice`, pack from `Array<Array<T>>`, map over rows).

## Assumptions

- Half-open `[Offsets[i], Offsets[i+1])` with `Offsets.Count = RowCount + 1` and `Offsets[RowCount] = Values.Count` is the invariant (matches `PolygonMesh3D` and classic CSR).
- Offset entries are plain `Integer` (not `Index` types) — already policy in CONVENTIONS.md; element values may be typed (`VertexIndex`, etc.) or generic `$T`.
- [plato-301](plato-301.md) polyhedra / polygon-mesh helpers should **consume** this library rather than grow private CSR clones.
- Generic `CompressedSparseRow<$T>` (or interface over Offsets+Values) is preferable to mesh-only helpers if the compiler/stdlib pattern for generics allows it cleanly.

## Design decisions

- **Type vs interface** — concrete `type CompressedSparseRow<T> { Offsets: Array<Integer>; Values: Array<T> }` vs interface `CompressedSparse` with `Offsets`/`Values`/`RowCount` so `PolygonMesh3D` can implement without wrapping. Hybrid: interface for algorithms + optional struct for standalone jagged data. Prefer interface + thin type alias/wrapper if fields would duplicate mesh storage.
- **Owns positions?** — CSR alone is indices/values; meshes keep `Positions` beside a CSR of face corners. Do not force geometry into the CSR type.
- **Builder API** — `FromRows(Array<Array<T>>)` / fold-append vs only accessors on existing pairs. Builders needed for Conway pack; accessors needed everywhere.
- **Empty / degenerate** — empty CSR = `Offsets = [0]`, `Values = []`; forbid negative lengths; document whether unsorted/overlapping offsets are representable (illegal) vs validated constructors.
- **2D CSR / matrix CSR** — classic numeric sparse matrices (col indices + values) are a different shape; out of scope unless named distinctly (`SparseMatrixCsr`). This issue is **ragged row packing**, not scientific sparse linear algebra.

## Related

- [meshes.plato](../../submodules/Plato/stdlib/meshes.plato) — `PolygonMesh3D` FaceOffsets/FaceVertices (CSR by comment).
- [CONVENTIONS.md](../../submodules/Plato/stdlib/CONVENTIONS.md) / [README.md](../../submodules/Plato/stdlib/README.md) — offset arrays stay `Integer`.
- [topology-adjacency.plato](../../submodules/Plato/stdlib/topology-adjacency.plato) — already documents CSR and uses it for `VertexAdjacency` / `FaceAdjacency` / `VertexFaceAdjacency` (Offsets + payload); prime refactor target to share the interface.
- [plato-301](plato-301.md) — polyhedra library should use shared CSR helpers.
- [plato-298](plato-298.md) — notes CSR as packing view vs face-with-holes records.
- [PolygonMesh3D.cs](../../ara3d-sdk/src/Ara3D.Geometry/Primitives/PolygonMesh3D.cs) — C# CSR mirror (oracle for pack/slice).

## Approaches

Short term: interface + library (`RowCount`, `RowLength`, `RowSlice`/`At`, `FromRows`) and refactor docs so `PolygonMesh3D` cites the interface; optional `CompressedSparseRow<T>` value type for standalone use.

Long term: adjacency/half-edge builders emit CSR; mesh ops take `CompressedSparse<VertexIndex>`; laws for offset monotonicity.

Adjacent: sparse matrix CSR (numeric); retire duplicated offset logic in C# `PolygonMesh3DExtensions`.

## Case against

- **Two fields are enough.** A named type may be ceremony if every consumer already has domain-specific field names (`FaceOffsets`).
- **Generic friction.** Plato generics + interface libraries may make `CompressedSparseRow<$T>` clumsier than mesh-local helpers for [plato-301](plato-301.md).
- **Wrong CSR.** Pulling in matrix-market CSR vocabulary confuses geometry authors.

**Verdict: pursue** a small interface (+ library), with an optional concrete type if standalone jagged arrays need a noun. Park scientific sparse-matrix CSR. Do not block plato-301 on a perfect abstraction — extract once two call sites exist (polygon mesh + adjacency or Conway pack).

## Bedrock

Strengthens the **ragged-array packing seam** used by `PolygonMesh3D` and future adjacency: one invariant and one helper library instead of copy-pasted offset arithmetic in every mesh/topology file. **Verdict: simplest-along-the-grain** — interface + tiny accessors/builders; must NOT redesign `PolygonMesh3D` field names in the same change unless the interface is implemented by projection, and must NOT scope-creep into sparse linear algebra.

## Done means

- [x] `CompressedSparseRow` type and/or interface declared in forward stdlib with documented half-open offset invariant
- [x] Library: row count, row length, element/row slice access, pack from rows (or equivalent small builders)
- [x] `PolygonMesh3D` (docs and/or implementation) references the shared abstraction rather than explaining CSR only in a comment
- [x] Lint: 0 parse / 0 resolution on touched stdlib files
- [x] At least one consumer path noted (polyhedra pack or adjacency) or a follow-up issue filed if deferred

## Outcome (2026-07-29, ec84f2b)

Shipped as **`Jagged<T>`**, not `CompressedSparseRow`. The scientific-sparse-matrix
vocabulary was the named risk in "Case against"; `Jagged` says *ragged row packing* and
leaves the `SparseMatrixCsr` name free. CSR is still the documented encoding — the
invariant is stated once, in full, on the interface.

Files (`submodules/Plato/stdlib/`):

- `collections-jagged.concepts.plato` — `interface Jagged<T>`, exactly two obligations
  (`Offsets: Array<Integer>`, `Values: Array<T>`). Doc comment carries the whole CSR
  invariant plus the Wikipedia link, and cites CONVENTIONS "Typed indices" for why
  offsets stay plain `Integer`.
- `collections-jagged.plato` — `type JaggedArray<T> implements Value, Jagged<T>`, the
  standalone noun builders return. Its two fields satisfy the interface directly, so it
  needs no projection.
- `collections-jagged.library.plato` — `RowCount`, `RowLength(i)`, `Row(i): Array<T>`
  (via the `SubArray` intrinsic), `RowElement(row, k)`, `HasNoRows`, and the `FromRows`
  builder. All interface-receiver form, as in `collections-containers.library.plato`.
- `meshes.library.plato` (new) — `PolygonMesh3D` projections: `Offsets => FaceOffsets`,
  `Values => FaceVertices`.
- `topology-adjacency.library.plato` (new) — payload projections for `VertexAdjacency`,
  `FaceAdjacency`, `VertexFaceAdjacency`.

Implementation by projection, no field renames: `PolygonMesh3D` now
`implements … Jagged<VertexIndex>`, `VertexAdjacency` `Jagged<VertexIndex>`,
`FaceAdjacency` / `VertexFaceAdjacency` `Jagged<FaceIndex>`. The three adjacency tables
already store a field literally named `Offsets`, so only the payload projection is
written — a redundant `Offsets(self: VertexAdjacency) => self.Offsets` would duplicate
the field accessor.

**Consumer path:** the adjacency tables (`topology-adjacency.plato`) and `PolygonMesh3D`
(`meshes.plato`) — four implementors on day one, which is the two-call-site bar the
Bedrock section set. Their duplicated CSR paragraphs are now one-line citations of the
interface; only type-specific facts remain (CCW winding, sorted-ascending neighbour runs,
per-table entry counts). [plato-301](plato-301.md)'s `MeshesPolygon` accessors landed in
parallel and still restate the offset arithmetic; forwarding them to `Jagged` is a
follow-up, not a blocker.

`FromRows` is a plain library function, not an interface member: static-abstract interface
members are [plato-312](plato-312.md) and were not shipped when this landed.

Two notes for whoever writes the next fold over a collection-of-collections:

- `rows.Take(i).Reduce(0, (acc, r) => acc + r.Count)` does **not** type-check — a lambda
  parameter that is itself a collection binds `Count` ambiguously (`Array` vs the
  `unique List` builder) and the checker rejects it with `CHK201`. The prefix sum folds
  over integer row lengths instead. Comment in the body says so.
- The prefix sum is therefore O(n^2) in row count. Correct and composable first
  (STYLE_GUIDE); a linear-scan variant is separate, later work if a profile asks.

Parked as designed: scientific sparse-matrix CSR (`SparseMatrixCsr`), and map-over-rows.

### Addendum (2026-07-29, Plato 28599cc): PolygonMesh3D moved to composition

The projection approach for `PolygonMesh3D` was superseded the same day: the mesh now
holds `Faces: JaggedArray<VertexIndex>` and no longer implements `Jagged<VertexIndex>`.
Rationale: the mesh HAS a face table rather than IS one — the interface's generic
`Offsets`/`Values` names were wrong vocabulary on a mesh receiver, generic jagged
algorithms applied to the whole mesh read as nonsense, and a second jagged member
(vertex-to-face adjacency) could never implement the interface twice; a field scales.
`meshes.library.plato` (the projection file listed above) is deleted; row queries go
through `mesh.Faces`. The "forward `MeshesPolygon` accessors to `Jagged`" follow-up
noted above landed as part of this — they now delegate through `self.Faces`. The three
adjacency tables are unchanged: their own storage already names `Offsets`, so direct
implementation remains correct there, and `collections-jagged` doc comments now point
only at them as the direct-implementer example. Gates re-run PASS (lint --strict +
ratchet 0 diagnostics).

Gates: `check-stdlib-fast.ps1` PASS — `lint --strict` 0 parse / 0 resolution errors;
checker ratchet 0 / 2189 with ceiling 0. `ForwardStdLibCheckerTests` 5 / 5 passing.

## Simplest possible implementation

```plato
interface CompressedSparseRow<T> {
    Offsets(x: Self): Array<Integer>;
    Values(x: Self): Array<T>;
}
```
plus library functions `RowCount`, `RowLength`, `At(row, k)`, `FromRows`. Keep `PolygonMesh3D` fields as-is; add interface members as projections or a thin wrapper type used by helpers.

Pros: unblocks shared helpers for plato-301; matches existing conventions.
Cons: interface-on-mesh may need boilerplate projections; builders still need careful `MapRange`/`FlatMap` style.
