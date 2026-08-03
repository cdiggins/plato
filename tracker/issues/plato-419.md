---
id: plato-419
title: TypeScript writer: generated stdlib mesh/polygon/CSG paths do not run
type: bug
status: idea
priority: p1
effort: L
risk: med
area: plato
sprint: 
created: 2026-08-03
closed:
links: [writers/Plato.TypeScriptWriter/TypeScriptWriter.cs, demos/webgl/src/plato/array-ext.ts, tracker/issues/plato-418.md, tracker/issues/plato-276.md]
---

## Issue
`Plato.CLI --typescript` over `stdlib/foundation stdlib/geometry stdlib/graphics`
produces a file that compiles but throws on almost every mesh, polygon or CSG
member. The scalar and `Point*` paths run (that is all the SDF demo exercises,
which is why this was not visible before). Thirteen distinct writer defects,
umbrella issue — split when someone starts work.

Defects 1-7 were found by the first `demos/webgl` sweep (meshes, polygons, CSG,
deformers). Defects 8-12 were found by the second, which built seven more pages
over curves, surfaces, noise, colour, transforms, marching cubes and voxels —
the same file, a wider slice of the library. That sweep also produced new
instances of 3, 4, 5 and 6, recorded under each. Defect 13 came from a different
direction — writing new `stdlib/geometry` vocabulary (plato-422) rather than
consuming existing vocabulary — and is the first one that constrains what an
author may write rather than what a reader may call.

1. **`Array<T>` library functions are never emitted.** `IArray<T>` in the output
   declares only `At`/`Count`/`Map`/`Reduce`, but generated bodies call
   `Concatenate`, `SubArray`, `FlatMap`, `Append`, `AtModulo`, `First`, `Last`,
   `IsEmpty`, `Where`, `Any`, `All`, `PrefixSums`, `FromRows` — the
   `collections.library.plato` / `primitives.library.plato` surface, in
   extension-method position. Nothing installs them. Same shape as plato-418,
   wider blast radius.
2. **Library free functions over `Array<Point*>` are dropped the same way.**
   `PolygonMeshOfFaces` / `PolygonMeshOfVertexNumbers`
   (meshes-polygon.library.plato:113,118), `ShoelaceArea`, `ChainLength`,
   `PolygonAreaCentroid`, `PolygonContainsPoint`, `AverageOfPoints`
   (geometry.library.plato), `VectorArea`, `PlanarPolygonCentroid`,
   `PlanarPolygonContains`, `PlaneTangent`, `PlaneCoordinates`
   (polygons.library.plato), `CutByPlane` (solids-csg.library.plato:192). Every
   Platonic seed goes through `PolygonMeshOfVertexNumbers`, so
   `PolygonMesh3D.Cube()` alone is enough to reproduce.
3. **Overloads are dropped, and the survivor is picked by declaration order.**
   The output is littered with `// Skipped: overload or duplicate member
   'Transform'`. `Vector3D.Transform` keeps the `AffineTransform3D` body, so
   `v.Transform(quaternion)` — what every rotation-based deformation compiles to
   — reads `.Matrix.Row1` off a `Quaternion` and gets `undefined`. Silent wrong
   dispatch, not a missing-member error.
4. **`Integer` division is emitted as float division.** `TruncateFaceOfFace`
   (polyhedra.library.plato) indexes `FaceCorner(face, k / 2)`; in TypeScript
   `k / 2` is `1.5`, which indexes between slots and yields `undefined`. Any
   Plato body that divides an `Integer` is affected.
   Further fatal instances: `MarchingCubesCornerOffsetY/Z` (corner 1 lands at
   y = 0.5, so the whole isosurface kernel is wrong), `MarchingCubesLattice`'s
   cell enumeration, `MakeArray3D`, `WorleyNeighbour2D/3D`, and
   `BinningGrid2D/3D.BucketBounds` (`b / columns % rows` — bucket 0 is right and
   every bucket past the first row is a geometrically wrong box, which
   `CandidatesInBounds` inherits because it gates on `BucketBounds().Overlaps`).
   `GridTriangleFace` and the matrix `ElementAt` chains are the same shape and
   untested. This defect is the most pervasive of the twelve and the quietest:
   it produces plausible geometry, not exceptions.
5. **Sum types cannot be emitted (CHK320) but their consumers still are.** The
   output carries `// CHK320: sum type 'PlaneRelation3D' cannot be emitted to
   the TypeScript target; sum types are C#-only in v1`, and 40 lines earlier
   `Polygon3D.RelationTo` returns `PlaneRelation3D.Spanning()`. The identifier is
   free, so it resolves to `globalThis` and throws at runtime. A body whose type
   cannot be emitted should not be emitted either — or CHK320 should be an error.
   The blast radius is wider than the unconstructible types: **every library
   function that dispatches on a skipped sum type vanishes with it.**
   `NoiseBasis` takes `BasisValue2D/3D`, `FbmOctave`, `TurbulenceOctave`,
   `RidgedOctave` and `WarpPoint` down with it, so eight noise types were
   unreachable rather than merely unconstructible. Other skipped sum types with
   live consumers: `WorleyDistance` / `WorleyFeature` (Worley noise),
   `RotationOrder` (`Quaternion.EulerAngles`, and every `EulerAngles`
   conversion), `TransferFunction` / `NamedColorSpace` (`RgbColorSpace.Default`
   and `ColorSpaceConversion.Default` throw on construction), `SdfNode3D` /
   `SdfCombine` (`SdfTree3D` exists and evaluates, but its `Nodes` array can
   never be populated, so the whole SDF-tree path is unreachable), and
   `Axis3D` / `SignedAxis3D`.
6. **Record returns written as tuple literals lose their field names.**
   `Intersect(r: Ray3D, pl: Plane): PlaneHit3D` (lines.library.plato:307) returns
   `(false, r.Origin, 0.0)`; the writer emits a bare `Tuple3`, so `RayHits`'
   `hit.Hit` and `hit.Point` are `undefined`. `PlaneHit3D` is emitted as a class
   right there in the same file — it is just never constructed.
   The same substitution happens in **argument** position, which is worse because
   the receiver then reads named fields off a `TupleN` that has `X0`/`X1`:
   `LatticeGradient2D(…).Dot((dx, dy))` emits `new Tuple2(dx, dy)` where
   `Vector2D` is declared; `Matrix4x3.Zero()`'s rows are `Tuple3` where `Number3`
   is declared; and the derived samples in `surfaces.library.plato`
   (`CenterPoint`, `CornerPoint00/10/01/11` on **every** `IParametricSurface`)
   emit `this.Eval(new Tuple2(0.5, 0.5))` where `UvCoordinate` is declared, so
   `uv.U` is `undefined`. Trig surfaces throw; the polynomial ones return NaN in
   silence.
7. **`IArithmetic` obligations missing on the native number mapping.**
   `Number.prototype.Zero`/`One`/`Half` are called by `IsOdd`, `Saturate`,
   `OneMinus`, `RoundedToNearest`; and `Number.Pi`/`Epsilon`/`MinValue`/`MaxValue`
   are emitted as `ThrowNotImplemented`. `Pi` is additionally called in *instance*
   position by `IsoperimetricQuotient`, so a static-only fix is not enough.
8. **The scalar overload on `NumberN` is dropped.** `Number2/3/4/8` keep the
   componentwise `Multiply`/`Divide`/`Modulo` and skip the `IScalable` scalar
   one. `Point3D.Transform` does `m.Row1.Multiply(this.X)` with `Row1: Number3`,
   lands in `Multiply(right: Number3)`, and reads `right.X` off a number — so
   **every affine transform returned NaN, including the identity**, with no
   error. A special case of defect 3, listed separately because it is the one
   that makes the transform tier unusable rather than merely incomplete.
9. **The commuted overload `Multiply(Number, IScalable)` is dropped**
   (algebra.library.plato, angles.library.plato). Only `Multiply(Number, Number)`
   survives, so `scalar * Angle` is NaN. Kills `Epicycloid2D.Eval`,
   `Hypocycloid2D.Eval`, `Epitrochoid2D.Eval`, `Hypotrochoid2D.Eval` — two by
   NaN and two by a throw one call further on.
10. **Extra arguments are dropped silently at emitted call sites.** Only the
    two-index `LatticeHash(seed, ix, iy)` is installed, but `LatticeGradient3D`,
    `FeatureOffset3D`, `WhiteNoise3D.Eval` and `ValueNoise3D.Eval` call it with
    three. JavaScript discards the extra argument without complaint, so **every
    spatial lattice noise was constant along z** — a plausible-looking field
    that is silently two-dimensional.
11. **`Array3D<T>` has no runtime.** `MakeArray3D` ends
    `return new IArray3D<_T0>(elements, this, rows, layers)`, but `IArray3D` is
    declared only as an `interface` — `new` on a type-only declaration is a
    `ReferenceError`. This is the only way to build the `Values` of
    `DensityGrid3D` / `SampledSdf3D` / `LevelSetGrid3D`, so the entire voxel tier
    was unconstructible. The interface also declares `ColumnCount()` /
    `RowCount()` / `LayerCount()` as methods while every consumer reads them as
    fields. `Buffer<T>` and `List<T>` have no runtime either.
12. **A property is emitted without its call parentheses.** The Plato bodies for
    `BezierPatch.Eval` and `BSplineSurface.Eval` (shared by `NurbsSurface.Eval`)
    read `ControlPoints.RowCount` / `.ColumnCount`; the writer emits them
    uncalled, so the body gets a function object where it wants a count
    (`this.ControlPoints.RowCount.MapRange is not a function`). The same trap
    appears in `Rotation2D.Identity()`, which builds `(0).Angle` uncalled and
    leaves a function in the `Angle` slot — which is why `Pose2D.Identity()` and
    `Pose2D.Lerp` fail.
13. **The affine builders `List<T>` and `Buffer<T>` have no TypeScript runtime at
    all.** They are two of the three sanctioned builder forms in the language and
    neither reaches this target, so any body that accumulates through one is
    unreachable rather than merely wrong. `demos/webgl/src/plato/array-ext.ts`
    already carries a hand-written `NodeBuffer` for the ear-clipping kernel,
    which is what a workaround for this costs. Unlike defects 1–12 this one
    changes what can be *written*, not just what survives emission: the
    `stdlib/geometry` sampling work (plato-422) rejected Bridson's Poisson-disk
    algorithm and discretized Lloyd relaxation outright because both need a table
    written once and read randomly, and designed around it with an immutable
    append chain at a known constant-factor cost. Found 2026-08-03 while
    implementing plato-422; recorded here because it is the same writer surface,
    but it is arguably its own issue.

Defect 1 is worse than "some helpers": it takes down the **entire ear-clipping
triangulation kernel**. `TriangulateRings`, `RingCount`, `RingStart`, `RingEnd`,
`RangeSignedArea`, `FilterRing`, `BridgeHoles`, `ClipEars` are all
`Array<…>`-first and none are emitted, while the `Integer`-first half of the same
kernel (`LinkRing`, `FirstLiveSlot`) is. Every `Triangulate` obligation on
`Polygon2D` / `PolygonWithHoles2D` / `PolygonSet2D`, and `Polygon3D.ToTriangleMesh`,
is therefore dead in unmodified output.

This paragraph previously said a prelude could not reasonably cover the
triangulation kernel. It has since been covered: `demos/webgl/src/plato/array-ext.ts`
hand-ports it, and the blocked scenes light up. That is a measure of how far the
prelude has had to go, not a reason to close this defect — the port is a second
copy of stdlib logic that has to be maintained in step with the first, which is
exactly the liability the issue is about.

`WindingOrder` is a second instance of defect 5, hit by `Polygon2D.Winding`.

Defect 3 (dropped overloads) is the widest of the seven. Beyond
`Transform(Quaternion)` it takes down, at least:

- `Vector2D.Transform(Rotation2D)` — ten skipped `Transform` overloads on
  `Vector2D` alone; blocks `Twist2D.Eval`.
- The componentwise `Scale` / `ScaleAbout`, so `ScaleX/Y/Z` multiply a vector by
  a `Number3` object and return **NaN with no error** on `PolygonMesh3D`,
  `RichMesh3D` and `TriangleArray3D` alike. There is no working non-uniform
  scale on a mesh.
- `Multiply(deformation, Number)` — the `Multiply` slot goes to the `Compose`
  alias, so constant-strength scaling of a deformation is unreachable.
- `Quaternion.Multiply(Quaternion)` and `Concatenate` — **NaN**, the scalar body
  ran. Quaternion composition, the reason the type exists, is unavailable.
  `Matrix3x3.Multiply(Matrix3x3)` is NaN the same way, taking `Transform2D.Compose`
  with it.
- `Matrix4x4.CreateScale(Number3)` and `Matrix3x2.CreateScale(Number2)` store the
  vector object in a matrix cell, so there is no non-uniform scale on either side.
- `Matrix3x2.CreateRotation(angle, centre)` — the one-argument body runs and **the
  centre is silently ignored**, so `RotationAbout2D.AffineTransform2D` returns a
  zero translation row instead of the pivot correction.
- `Point2D.Transform(AffineTransform2D)` — the dropped overload is one level down
  (`Vector2D.Transform(Matrix3x2)`), and it cascades through
  `AffineTransform2D.Multiply`, `Transform2D.Multiply`, `Pose2D.Compose`,
  `RotationAbout2D.Multiply` and `Polygon2D.Deform` over an affine.
- `Rotate(Angle)` and `RotateAbout(centre, Angle)` on every 2D deformable — only
  the `Rotation2D` overload survives, so the generated `Rotate(angle)` body puts
  an `Angle` in the `Rotation2D` slot.
- `ToSdf(Triangle3D, thickness)` / `ToSdf(Quad3D, thickness)` — this one is the
  sharpest illustration of why dropped overloads are worse than missing members:
  `triangle.ToSdf(0.3)` **typechecks**, the argument is discarded, the
  zero-thickness body runs, and marching cubes returns an **empty surface**. No
  exception, no NaN, just nothing.
- `MarchingCubes(bounds, nodeCounts, isoLevel)` is dropped on every SDF type
  (`FunctionSdf3D`, `BoundedSdf3D`, `PlacedSdf3D`, `SampledSdf3D`); only the
  scalar-field types keep the iso-level form.

Also unemitted, and not overload-related: the `Deform` **apply lifts** over
`IDeformable3D` (plain and weighted) — only `Deform(mapping)` survives, so the
library's weighted-apply design has to be spelled out by the caller. And
`Compose` / `Multiply` are emitted monomorphically per type
(`Twist3D.Compose(second: Twist3D)`) rather than over `IDeformation3D`.

A performance note for whoever fixes this, found in the same demo: generated
members return lazily mapped collections, so a `Positions` chain after three
`Truncate` rounds walks a deep stack of Map/Concatenate views on every element
read — 8.4 s per rebuild until the caller flattens it once. `Truncate` itself is
quadratic in vertex count and runs ~15x slower under a browser JIT than under
tsx. Neither is a correctness bug, but both shape what generated TypeScript can
be used for.

## Impact
Everything the stdlib says about meshes, polyhedra, polygons, triangulation and
CSG is unreachable from TypeScript — roughly the whole `stdlib/geometry` surface
above `Point3D`. The second sweep extended that to curves, surfaces, noise,
transforms, isosurfaces and voxels: the transform tier returned NaN for every
affine including the identity, spatial noise was silently flat in z, and the
voxel grids could not be constructed at all. The "write once, compile to
idiomatic libraries" claim only holds for the scalar/vector tier on this target.

Defects 3, 4, 6, 8, 9 and 10 are the dangerous ones, and they now outnumber the
loud ones: they produce wrong answers rather than exceptions, so a downstream
user would ship them. Three failure shapes recur, in ascending order of how long
they take to notice — a missing member throws immediately; a dropped overload
runs the wrong body and returns NaN; a dropped **argument** runs the wrong body
and returns something that looks right. `ToSdf(triangle, thickness)` marching to
an empty surface is the whole family in one call.

## Affected code
- writers/Plato.TypeScriptWriter/TypeScriptWriter.cs — `IgnoredTypes` /
  `IgnoredFunctions`, library-function emission, overload handling.
- writers/Plato.TypeScriptWriter/TypeScriptConcreteTypeWriter.cs — `At`/`Count`
  synthesis (plato-276 territory), record construction from tuple literals.
- demos/webgl/src/plato/array-ext.ts — the hand-written prelude that works
  around all seven, one section per defect, each body mirroring its `.plato`
  source. Delete it as the writer catches up; it is the shortest spec of what is
  missing.
- demos/webgl/scripts/smoke.mts — value checks over the affected members, each
  pinned to a number the Plato source determines (Conway face counts, the area of
  a unit square, a de Casteljau evaluation at its endpoints, a marched sphere's
  radius). Run it after any writer change to see which side moved.
- demos/webgl/scripts/probe.mts — the triage tool: calls each candidate member and
  reports `ok`, `FAIL <message>` or `NaN`. The NaN column is the one that matters
  here, because most of the defects above fail silently.
- demos/webgl/scripts/scenes.mts — builds every scene of every demo page off the
  page, so a member that throws only under real inputs is caught by a gate rather
  than by a blank stage.

## Cause / analysis
The writer's model is type-centric: it emits classes for concrete types and
installs intrinsics on native prototypes, but has no story for a Plato *library*
whose functions extend a generic interface (`Array<$T>`) rather than a concrete
type. C# gets these as extension methods for free; TypeScript has no equivalent,
so the call sites are emitted and the definitions are not. Defects 3, 5 and 6 are
a different family: the writer emits a call site whose target it has already
decided it cannot represent, instead of failing the compile. Speculative but
likely: a "did I emit a definition for everything I called?" pass over the output
would have caught 1, 2, 5 and 6 at build time.

## Priority
p1. Not because anything is on fire — nothing downstream consumes the TS mesh
tier yet — but because the cost of deferral is that every new TS consumer either
rediscovers this or copies the prelude, and because defects 3 and 4 are silent
wrong answers rather than crashes. The prelude buys time; it does not remove the
liability, and it now has to be maintained in step with `stdlib/`.

## Dependencies
- Related: plato-418 (`MakeArray2D` — the same defect family, narrower case),
  plato-276 (`At`/`Count` synthesis in the same writers).
- Touches: `writers/Plato.TypeScriptWriter/**`, which the SDF demo track also
  edits — coordinate before starting.

## Fix approaches
1. **Emit array libraries as prototype installs.** Give `Arr<T>` (or `IArray`) the
   same `Intrinsics.Install` treatment `Number`/`Boolean` already get, driven by
   the same library-function walk. Fixes 1 and 2 together; the prelude's bodies
   are the reference. Largest payoff, most writer work.
2. **Name-mangle overloads** (`Transform_Quaternion`) and resolve at the call
   site from the static argument type, or emit one dispatching body. Fixes 3.
   Independent of 1.
3. **Make unemittable-target call sites a hard error.** If CHK320 fires for a
   type, refuse to emit the bodies that mention it rather than emitting a
   dangling reference. Fixes 5, and turns future gaps into build failures instead
   of runtime surprises. Cheapest of the three and the one with the most leverage.

## Bedrock
The invariant this violates is *the generated file is closed*: every identifier it
mentions is defined in it or in its prelude. Six unrelated-looking bugs are one
missing check. Enforcing closure as a post-emit pass over the writer's output —
walk every call site, assert a definition exists — turns each of these from a
runtime `is not a function` into a build failure naming the missing library
function, and stops the next gap from reaching a demo at all. It strengthens the
seam between `TypeScriptWriter` and its output rather than any single member.
Verdict: **right** — approach 3 first (it makes the rest self-reporting), then 1,
then 2.

## Done means
- [ ] `PolygonMesh3D.Cube().Truncate().FaceCount()` returns 14 from unmodified
      generated output, with no prelude imported.
- [ ] `demos/webgl/scripts/smoke.mts` passes with `src/plato/array-ext.ts`
      removed from the import graph.
- [ ] The writer fails the build, naming the symbol, when a body references a
      type or function it did not emit.
- [ ] `demos/webgl/src/plato/array-ext.ts` is deleted and its README section
      removed.
- [ ] `Polygon2D.Triangulate` returns a face per ear from unmodified generated
      output, and the blocked scenes in `demos/webgl/src/demos/polygons.ts`
      light up.

## Simplest fix
Keep the prelude, and make it generated: teach the writer to emit exactly the
hand-written file `demos/webgl/src/plato/array-ext.ts` already contains, as a
second output alongside `plato.g.ts`. Gets running code on every target
regeneration for a fraction of the work. Gives up the closure invariant — the
list of what to emit stays hand-maintained, so the next stdlib addition breaks it
again silently. Acceptable as a stopgap only if the closure check (approach 3)
lands with it.

## Prevention
- The generated-output closure check above is the structural prevention.
- Tests: no gate currently runs generated TypeScript over the geometry tier.
  `demos/webgl/scripts/smoke.mts` is that gate for now but lives in a demo —
  worth promoting into the writer's own test suite. Separate issue.
- The `// Skipped: overload or duplicate member` and `// CHK320:` comments are
  the writer telling us it dropped something. Nothing reads them. A build-time
  count of skip comments, asserted against a checked-in baseline, would have
  surfaced defects 3 and 5 the day they appeared. Worth filing.
