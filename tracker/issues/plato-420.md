---
id: plato-420
title: Declared-but-unimplemented stdlib tiers: colour conversion, differential geometry, sampling patterns, voxelization
type: problem
status: idea
priority: p2
effort: L
risk: low
area: plato
sprint: 
created: 2026-08-03
closed:
links: [stdlib/graphics/color-spaces.library.plato, stdlib/geometry/differential-geometry.types.plato, stdlib/geometry/sampling.library.plato, stdlib/geometry/voxels.library.plato, demos/webgl/COORDINATION.md]
---

## Issue
Four areas of `stdlib/` declare a complete vocabulary of types and then define no
functions over them. The types compile, type-check, emit to every target and
carry constructors, `With*`, `Equals` and `Default` — and there is nothing to
call. A consumer discovers this only by reaching for a member that was never
written.

This is **not** plato-419. That issue is about bodies the writer fails to emit;
this one is about bodies the library never had. The two are easy to confuse from
the consuming side, because both present as "the member is not there", and
telling them apart costs a trip into the `.plato` source every time.

1. **Colour conversion.** `color-spaces.types.plato` declares `ColorSRGB`,
   `ColorHSL`, `ColorHSV`, `ColorHWB`, `ColorCMYK`, `ColorLab`, `ColorLCh`,
   `ColorOkLab`, `ColorOkLCh`, `ColorLuv`, `ColorXYZ`, `ColorXyY`, `ColorYCbCr`,
   `ColorYUV`, `Chromaticity`, `WhitePoint`, `RgbPrimaries`, `RgbColorSpace`,
   `ColorSpaceConversion`, `ColorLookupTable`, `Palette`, `ToneCurve` and seven
   adjustment types. `color-spaces.library.plato` defines `Chromaticity.Hash` and
   `ColorXYZ`'s vector arithmetic. **No member anywhere takes a `Color` and
   returns a `ColorLab`**, or applies an adjustment to anything. The types file
   says so in its own header — "Conversions are functions in a later pass; these
   types name the endpoints and the knobs" — so this is a known deferral rather
   than an oversight. It is filed here because the deferral is invisible from
   outside that header.
   `color.types.plato` also states that `Color` is linear-light and `Color8` is
   typically sRGB-encoded, and emits no member that decodes or encodes between
   them, so the two cannot be mixed at all.

2. **Differential geometry.** `differential-geometry.types.plato` declares
   `FrenetFrame2D`, `FrenetFrame3D`, `RotationMinimizingFrame3D`,
   `DarbouxFrame3D`, `CurveJet2D`, `CurveJet3D`, `OsculatingCircle2D`,
   `CurvatureComb2D`, `FirstFundamentalForm`, `SecondFundamentalForm`,
   `PrincipalCurvatures`, `SurfaceCurvature`, `TangentSpace3D`, `SurfacePoint3D`,
   `SurfaceJet3D`, `GeodesicPath3D`. **No library anywhere produces one of them
   from a curve or a surface** — they appear only in their own declaration file.
   The capability interfaces have the same shape from the other side:
   `curves.library.plato` derives a dozen helpers on `IDifferentiableCurve2D/3D`
   (`UnitTangentAt`, `CurvatureVectorAt`, `RadiusOfCurvatureAt`, the `FrameAt`
   accessors) and `surfaces.library.plato` derives `FirstFundamentalE/F/G`,
   `NormalAtCenter`, `TangentUAtCenter`, `IsOrthogonalAt` on
   `IDifferentiableSurface` — and **no concrete type implements the obligations**
   (`TangentAt`, `CurvatureAt`, `TorsionAt`, `FrameAt`). The derived half is
   therefore monomorphized onto nothing: it does not appear in generated output
   at all. `Clothoid2D` implementing `IArcLengthParameterized` is the only
   capability interface in this family with a real implementor.

3. **Sampling patterns.** `HaltonPattern2D`, `HammersleyPattern2D`,
   `SobolPattern2D` and `BlueNoisePattern2D` emit with an empty implemented-
   interface section: no `Points`, no `Eval`. `sampling.library.plato` defines
   bodies only for `SampledCurve2D/3D` and `SampledSurface3D` mapping. A
   quasi-random pattern type with no way to produce a point is inert.

4. **Voxelization.** `pointclouds-voxels.concepts.plato` declares
   `IVoxel3D.ToOccupancyGrid(cellSize)`, and `meshes.library.plato` writes
   `VoxelCellCount` / `VoxelOccupiedCount` against it — but **no stdlib type
   implements `IVoxel3D`**, so nothing can turn a solid or a mesh into an
   occupancy grid. `voxels.library.plato` gives bodies to four members
   (`DensityGrid3D` and `LevelSetGrid3D`'s `CellCenter` + `MarchingCubes`,
   `SampledSdf3D.NodePosition` + `MarchingCubes`, `Point3D.CellCenter`); the rest
   of the file is the marching-cubes kernel. `VoxelGrid3D<T>`, `OccupancyGrid3D`,
   `OccupancyGrid2D`, `DensityGrid2D`, `VoxelColorGrid3D`,
   `SparseVoxelGrid3D<T>`, `VoxelBrick3D`, `BrickMap3D`, `RegularGrid3D`,
   `GridCell3D` and `GridCell2D` have no library bodies at all.

Smaller instances of the same shape, found alongside: `TrimmedSurface`,
`HeightField` and `SubdivisionSurface` are declarations without an `Eval` (their
types file says so); `FunctionSdf2D` has no `MapDomain` although `FunctionSdf3D`
and both `ScalarFunctionField`s do; `Prism3D` and `ExtrudedSolid` generate fields
but no mesh, so only `RegularPrism`, `RegularPyramid`, `SquarePyramid` and
`Antiprism` carry `ToPolygonMesh`.

## Impact
A consumer cannot tell a deliberate deferral from a defect. Every one of these
cost a demo agent real time: the colour page was scoped around conversions that
do not exist and had to be rebuilt around what does; the curves and surfaces
pages each planned a Frenet-frame scene and had to substitute a finite-difference
frame; the voxels page planned to voxelize a mesh and had to fill from a field's
own `Eval` instead. The work was not wasted — each page now reports the gap
live — but the discovery cost is paid again by every consumer.

The second-order cost is worse: a type that cannot be produced is
indistinguishable, from the outside, from a type the writer failed to emit. That
ambiguity is what makes plato-419 expensive to triage.

## Affected code
- stdlib/graphics/color-spaces.library.plato — the whole conversion pass.
- stdlib/geometry/differential-geometry.types.plato — inert records; no producer.
- stdlib/geometry/curves.library.plato, surfaces.library.plato — derived members
  on capability interfaces with no implementors.
- stdlib/geometry/sampling.library.plato — the pattern types.
- stdlib/geometry/voxels.library.plato, pointclouds-voxels.concepts.plato — the
  missing `IVoxel3D` implementors.
- demos/webgl/src/demos/{colors,curves,surfaces,voxels}.ts — each page's status
  lines are the live evidence: they probe the missing members by name on every
  rebuild, so they will start reporting values the day a body lands.

## Cause / analysis
The `stdlib/` partition separates declarations from bodies by design
(`<stem>.types.plato` / `<stem>.library.plato`), which makes it cheap and correct
to land a vocabulary before its implementation. Nothing then records that the
second half is outstanding: a types file with no library file, or a capability
interface with no implementor, is indistinguishable from a finished one. The
`stdlib/types-and-concepts.txt` index counts declarations, not bodies.

Inferred, not measured: the four areas above are unlikely to be the only ones.
A mechanical answer would find the rest.

## Fix approaches
1. **A coverage report, not a fix.** Walk the declarations and report each type
   with no library function taking or returning it, and each concept interface
   with no implementor. Cheap, mechanical, and it converts an unknown into a
   list. Everything else depends on having that list.
2. **Write the bodies**, area by area, priced separately — colour conversion is a
   well-specified matrix-and-transfer-function job; differential geometry needs a
   design decision first (analytic derivative obligations on every curve and
   surface, versus one finite-difference default in the library).
3. **Mark the deferral in the source.** A convention — a marker in the types file
   that the lint gate reads — so "declared, not implemented" is a fact the
   toolchain carries rather than a comment in a header a consumer never opens.

## Bedrock
The invariant worth having is *a declared type is inhabitable and a derived
member is reachable*: if the library declares it, something can produce it and
something can call it. Approach 1 measures the violation and approach 3 makes it
declarable; together they turn a silent gap into a checkable property, which is
the same structural move plato-419 needs on the writer side (closure of the
generated file). Verdict: **right** — 1 first, since neither 2 nor 3 can be
scoped without it.

## Done means
- [ ] A report exists listing every declared type with no producing function and
      every concept with no implementor, runnable from `tools/`.
- [ ] Each area above is either implemented or carries an explicit
      declared-not-implemented marker the lint gate understands.
- [ ] The demo pages that probe these members by name start printing values
      rather than `UNAVAILABLE`, with no change to the demo files.

## Simplest fix
Approach 1 alone, run once, pasted into this issue as the list. It fixes nothing,
but it is the difference between four known gaps and an unknown number of them,
and it costs a script.

## Prevention
Approach 3 — the marker plus the lint rule — is the prevention. Without it the
next vocabulary lands the same way, and the next consumer pays the same discovery
cost.

## Dependencies
- Related: plato-419 (the writer-side gaps that present identically from the
  consuming side).
- The `demos/webgl` pages are the working reproduction for all four areas.
