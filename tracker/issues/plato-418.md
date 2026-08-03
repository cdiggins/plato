---
id: plato-418
title: TypeScript writer: MakeArray2D/Array2D never emitted, generated mesh code crashes
type: bug
status: idea
priority: p2
effort: M
risk: low
area: plato
sprint: 
created: 2026-08-03
closed:
links: [writers/Plato.TypeScriptWriter/TypeScriptWriter.cs, stdlib/foundation/primitives.library.plato:250, tracker/issues/plato-276.md]
---

## Issue
The forward stdlib's mesh vocabulary constructs rank-2 arrays through `MakeArray2D`
(an ordinary Plato body since plato-378), but the TypeScript writer never emits it:
`Array2D`/`Array3D` sit in `TypeScriptWriter.IgnoredTypes` and `MakeArray2D` in
`IgnoredFunctions` with the comment "implemented in the intrinsics prelude" — which is
true for `Range`/`MapRange` (installed on Number.prototype since fad466bb) but was never
true for `MakeArray2D`. Generated output still contains the call site, so it throws
`TypeError: nx.MakeArray2D is not a function` the moment it runs.

Observed in `demos/typescript/sdf/src/plato/plato.g.ts` (regenerate to reproduce):
`return nx.MakeArray2D(ny, (col, row) => col.QuadFaceIndices(row, _var15, _var16));`
from `AllQuadFaceIndices` (stdlib/geometry/meshes.library.plato:356).

## Impact
Any TS-generated code path that builds grid meshes (quad-face index grids, matrix-like
construction per stdlib/foundation/numeric-structures.library.plato:527) crashes at
runtime. The SDF demo does not touch it, so nothing user-facing is broken today; a TS
mesh/marching-cubes demo would hit it immediately.

## Affected code
- writers/Plato.TypeScriptWriter/TypeScriptWriter.cs — `IgnoredTypes` (Array2D/Array3D), `IgnoredFunctions` (MakeArray2D), `WritePrelude` (has IArray2D/IArray3D interfaces but no classes and no MakeArray2D).
- stdlib/foundation/primitives.types.plato:35 — `Array2D<T>` now has an honest layout (Elements/ColumnCount/RowCount), so emitting it as a normal class is possible.
- stdlib/foundation/primitives.library.plato:250 — `MakeArray2D` is an ordinary Plato body.

## Cause / analysis
The ignore lists predate plato-378, when Array2D was opaque and its construction had to
be a host intrinsic. Now that the type has declared fields and library bodies, the
exclusions are stale — the C# writer already treats them as ordinary types.

## Priority
p2: hard runtime crash, but on a POC backend and outside every currently-shipping demo
path. Becomes p1 the moment a TS mesh demo is attempted.

## Dependencies
- Touches: writers/Plato.TypeScriptWriter (same files as [plato-276](plato-276.md); land sequentially).

## Fix approaches
1. Remove Array2D/Array3D from `IgnoredTypes` and MakeArray2D from `IgnoredFunctions`, letting the ordinary class + library-body pipeline emit them. Cleanest; needs a check that the generic class emission handles `Array2D<T>`.
2. Hand-write Array2D + MakeArray2D in the prelude (matching the comment that already claims this). Fast, but a second copy of the row-major layout to keep in sync with stdlib.

## Bedrock
Option 1 strengthens the invariant that the ignore lists contain only names genuinely
supplied by the prelude — the bug existed because an entry lied. Verdict: **right** —
option 1 unless generic-class emission has a blocker, in which case option 2 must keep
the stdlib row-major layout (index = row * ColumnCount + column) byte-compatible.

## Done means
- [ ] Regenerated plato.g.ts contains a working MakeArray2D (no bare `.MakeArray2D` call without an implementation)
- [ ] A vite-node snippet evaluating `AllQuadFaceIndices` (or any MakeArray2D caller) runs without TypeError
- [ ] TypeScriptEmitFlagOnTests still pass (TIR/legacy byte parity)

## Prevention
A smoke gate that imports the full generated plato.g.ts and touches one member per
concrete type would have caught this and the whole fad466bb bug family; that is worth
its own capture (offer: /track-idea "TS generated-library smoke gate").
