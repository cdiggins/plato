---
id: plato-421
title: 3D lattice structures: unit cells, tilings, and lattice operators
type: feature
status: in-progress
priority: p2
effort: M
risk: low
area: plato
sprint: 
created: 2026-08-03
closed:
links: []
---

## What and why

Plato has no vocabulary for periodic lattice structures — the strut-and-node
scaffolds used in additive manufacturing, metamaterials and infill. The pieces are
all present separately (`Grid3D`, `IntegerVector3`, `Line3D`, `Bounds3D`,
`PolygonMesh3D`, the implicit-SDF collection) and nothing names a unit cell or a
lattice operator over one.

Scope: **`stdlib/geometry`** (shipping tier — must pass lint strict, the checker
ratchet and index freshness). New `lattices.types.plato` +
`lattices.library.plato`, plus `lattices.concepts.plato` if an interface earns
its place.

Subject matter, roughly:

- **Strut lattices**: a unit cell as a set of nodes plus a connectivity list in
  normalized cell coordinates; the standard cells (simple/body-centred/face-centred
  cubic, octet truss, Kelvin/tetrakaidecahedron, diamond, re-entrant auxetic).
- **Tiling an instance**: a cell repeated over an `IntegerVector3` count inside a
  `Bounds3D`, producing struts as `Array<Line3D>` with the shared nodes welded, plus
  the derived readings (relative density, strut length, node valence, cell count).
- **Operators over a lattice**: uniform and graded scaling of strut radius or cell
  size from a field (`IScalarField3D`), trimming to a bounding solid or an SDF,
  conforming a lattice to a `Bounds3D` versus a deformation, and the dual lattice.
- **Triply-periodic minimal surfaces** as the implicit counterpart — gyroid,
  Schwarz P, Schwarz D, Neovius, primitive — as `IScalarField3D` /
  `ISignedDistanceField3D` implementations so the existing marching-cubes and SDF
  paths consume them with no new plumbing.

Reuse what exists. Struts become geometry through the existing `Line3D` /
`PolygonMesh3D` vocabulary; TPMS fields go through `implicit-sdf` and
`voxels.library.plato`'s marching cubes rather than a second extraction path.

## Design decisions

- **Two new files, no new interface.** `stdlib/geometry/lattices.types.plato` (nine
  declarations) and `stdlib/geometry/lattices.library.plato` (`library Lattices`).
  No `lattices.concepts.plato`: the only candidate was an `ILatticeCell` abstraction
  over `LatticeUnitCell`, which would have exactly one implementer. STYLE_GUIDE's
  "add a type only when two real uses exist" rules that out, and LINT013 punishes an
  interface nothing implements.

- **A unit cell is a graph in normalized cell coordinates**, not a world-sized
  object: `LatticeUnitCell { Nodes: Array<Point3D>; Struts: Array<LatticeStrut> }`
  with node positions in the unit cube. One cell description therefore serves every
  cell size, and the cell tables are pure data. Connectivity uses the existing
  `ItemIndex` (CONVENTIONS.md — typed indices) rather than a new index type; the
  precedent is `SdfNode2D.Leaf(Primitive: ItemIndex)`.

- **Welding is a local ownership rule, not a vertex search.** A strut with BOTH
  endpoints on the far face of an axis lies in the face shared with the next cell,
  which describes the same segment on its near face; the near-face copy wins, and
  the far-face copy is kept only when there is no next cell. That is one predicate
  over one strut — no hash grid, no tolerance on world positions, no second pass.
  Node positions weld by construction because every one is spelled
  `Min + (cellIndex + normalizedCoordinate) * cellSize`, so the far node of cell i
  and the near node of cell i+1 evaluate the identical expression `(i+1)*cellSize`
  and land bit-identically. Rejected: emitting every cell's struts and deduplicating
  afterwards, which needs a spatial hash and a tolerance the rest of the tree does
  not have.
  This makes **periodicity a stated precondition** on the cell (a node at
  coordinate 1 has a twin at 0), recorded in `lattices.types.plato`. All seven
  shipped cells satisfy it.

- **Seven named cells**, each literal data: `SimpleCubic`, `BodyCenteredCubic`,
  `FaceCenteredCubic`, `OctetTruss`, `DiamondCubic`, `TruncatedOctahedron` (the
  Kelvin cell) and `ReentrantHoneycomb(reentrancy)` / `ReentrantAuxetic`. Rejected:
  deriving struts by a nearest-neighbour distance threshold, which would have made
  the FCC cell inexpressible (the octet truss is exactly FCC's node set under that
  rule) and put a magic distance in every table.
  Two renames away from the obvious spelling, both to avoid joining an unrelated
  overload group: `TruncatedOctahedron` rather than `Kelvin` (`Temperature.Kelvin`
  exists) and `DiamondCubic` rather than `Diamond` (a `TpmsFamily` case).
  The re-entrant cell is **parameterized**, because re-entrancy is a family and the
  auxetic behaviour is a function of the fold depth; `ReentrantAuxetic` is the
  quarter-fold constant for callers that just want a named cell.

- **The graded, trimmed and warped variants consume the welded strut list**, not the
  lattice: `Trimmed(struts, …)`, `StrutRadii(struts, …)`, `Deformed(struts, mapping)`
  all take `Array<Line3D>`, so they compose in any order and read left to right
  (`lattice.Struts.Trimmed(solid).StrutRadii(field, range)`). There is exactly one
  tiling path, `Struts`. **Graded cell size is a deformation**, not a second tiling:
  `Deformed` warps the tiled struts, which is what conforming a lattice to something
  other than a box means. Trimming keeps or drops whole struts by their midpoint and
  never clips — clipping would need root finding against the trimming solid and would
  break the welding at the cut.

- **Relative density is declared as a first-order OVERESTIMATE.** Sum of
  `pi * r^2 * length` over the envelope volume double-counts material at the nodes and
  omits the joint fillets; it converges on the truth in the slender regime lattices
  are actually used in. Saying so is better than shipping a number that looks exact.

- **Node valence is the coordination number in the INFINITE tiling**, computed from
  the cell alone by identifying nodes modulo the cell period and counting endpoints of
  owned struts. That is the number an engineer means; the count of struts touching a
  node inside one cell is an artifact of where the cube was cut.

- **TPMS: one sum-type family, three types.** `TpmsFamily` (Gyroid, SchwarzPrimitive,
  SchwarzDiamond, Neovius, IwpSurface) mirrors the `NoiseBasis` precedent instead of
  five near-identical types. `TpmsField3D` is the raw nodal implicit and implements
  `IScalarField3D` only — a nodal implicit is NOT a distance and claiming otherwise
  would be the "not a bound at all" failure `implicit-sdf.concepts.plato` warns about.
  The two solids, `TpmsNetwork3D` (one labyrinth) and `TpmsSheet3D` (a wall of finite
  thickness), implement `ISignedDistanceField3D` by dividing the implicit by a
  per-family **Lipschitz bound**, which makes them an honest LOWER BOUND. Bounds are
  the summed per-term coefficients times the angular frequency; verified numerically
  to be upper bounds (see Verification).

- **Struts reach geometry through the existing SDF path.** `StrutSdf3D` /
  `GradedStrutSdf3D` are unions of capsules over `DistanceToCapsule`, already in
  `implicit-sdf.library.plato`, and are EXACT signed distance fields. Marching cubes
  and sphere tracing therefore consume a lattice with no new plumbing, which is what
  the issue asked for. Evaluation is linear in strut count with no acceleration
  structure, and the type comment says so.

- **Dropped: the dual lattice.** A correct periodic dual (the line graph of the cell,
  one node per strut midpoint) needs adjacencies that cross the cell boundary, and the
  normalized-unit-cube cell cannot name a node in the neighbouring cell. Identifying
  nodes modulo the period instead is too coarse — it would declare the simple cubic
  cell's twelve edges mutually adjacent, since all eight corners are one periodic
  node. Left out rather than shipped wrong; it is not one of the boxes below.

## Verification

Lint (strict) and the checker ratchet pass. No gate currently executes geometry
bodies (`plato-308`), so the algorithm was verified out-of-band, against the same
tables and index arithmetic the Plato source uses:

- For all seven cells, tiled at 3x3x3 and at a deliberately lopsided 3x1x2, the
  ownership rule emits **every distinct world-space strut exactly once**: no
  duplicates and nothing missing against the full undeduplicated set. The same holds
  for nodes. The lopsided count is the case that matters — it is where a cell has a
  successor on one axis and not another.
- The periodic valences the cell tables produce are the textbook coordination
  numbers: simple cubic 6, body-centred cubic 8, face-centred cubic 12 at a corner
  and 4 at a face node, octet truss 12 everywhere, diamond 4 everywhere, Kelvin 4
  (the truncated octahedron is 3-valent; the fourth strut is the neighbour's across
  the shared square face). Every cell's struts come out a single length, as each
  structure requires.
- Each TPMS family's Lipschitz constant was checked against 200k sampled gradient
  magnitudes and bounds them all, with slack (the gyroid's true maximum is half its
  bound). Every family is at volume fraction 0.5 at level 0 except I-WP, which the
  doc comments do not claim.

## Done means

- [x] A unit-cell type and the named standard cells, in normalized cell coordinates
- [x] A lattice instance tiles a cell over a count inside a bounds and yields welded struts
- [x] Graded / field-driven and trimmed variants, without a second tiling path
- [x] The TPMS family implements the existing scalar-field and SDF interfaces
- [x] Derived readings: relative density, strut count, total strut length, node valence
- [x] `.\tools\check-stdlib-fast.ps1 -SkipIndex` green (lint strict + checker ratchet)
- [x] Design decisions recorded above
- [ ] `stdlib/types-and-concepts.txt` regenerated — nine new types make it stale,
      and index freshness is the third gate in `check-stdlib-fast.ps1`
- [ ] A browser demo drives it: `demos/webgl/lattices.html` + `src/demos/lattices.ts`,
      green under `npm run typecheck` and `npm run scenes`
