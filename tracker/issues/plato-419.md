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
which is why this was not visible before). Seven distinct writer defects,
umbrella issue — split when someone starts work:

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
5. **Sum types cannot be emitted (CHK320) but their consumers still are.** The
   output carries `// CHK320: sum type 'PlaneRelation3D' cannot be emitted to
   the TypeScript target; sum types are C#-only in v1`, and 40 lines earlier
   `Polygon3D.RelationTo` returns `PlaneRelation3D.Spanning()`. The identifier is
   free, so it resolves to `globalThis` and throws at runtime. A body whose type
   cannot be emitted should not be emitted either — or CHK320 should be an error.
6. **Record returns written as tuple literals lose their field names.**
   `Intersect(r: Ray3D, pl: Plane): PlaneHit3D` (lines.library.plato:307) returns
   `(false, r.Origin, 0.0)`; the writer emits a bare `Tuple3`, so `RayHits`'
   `hit.Hit` and `hit.Point` are `undefined`. `PlaneHit3D` is emitted as a class
   right there in the same file — it is just never constructed.
7. **`IArithmetic` obligations missing on the native number mapping.**
   `Number.prototype.Zero`/`One`/`Half` are called by `IsOdd`, `Saturate`,
   `OneMinus`, `RoundedToNearest`; and `Number.Pi`/`Epsilon`/`MinValue`/`MaxValue`
   are emitted as `ThrowNotImplemented`. `Pi` is additionally called in *instance*
   position by `IsoperimetricQuotient`, so a static-only fix is not enough.

Defect 1 is worse than "some helpers": it takes down the **entire ear-clipping
triangulation kernel**. `TriangulateRings`, `RingCount`, `RingStart`, `RingEnd`,
`RangeSignedArea`, `FilterRing`, `BridgeHoles`, `ClipEars` are all
`Array<…>`-first and none are emitted, while the `Integer`-first half of the same
kernel (`LinkRing`, `FirstLiveSlot`) is. Every `Triangulate` obligation on
`Polygon2D` / `PolygonWithHoles2D` / `PolygonSet2D`, and `Polygon3D.ToTriangleMesh`,
is therefore dead. A prelude cannot reasonably cover this one — it would mean
hand-porting the kernel, which is reimplementing the stdlib rather than patching
the writer, so `demos/webgl` leaves those scenes visibly blocked instead.

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
above `Point3D`. The "write once, compile to idiomatic libraries" claim only
holds for the scalar/vector tier on this target. Defects 3 and 4 are the
dangerous ones: they produce wrong answers rather than exceptions, so a
downstream user would ship them.

## Affected code
- writers/Plato.TypeScriptWriter/TypeScriptWriter.cs — `IgnoredTypes` /
  `IgnoredFunctions`, library-function emission, overload handling.
- writers/Plato.TypeScriptWriter/TypeScriptConcreteTypeWriter.cs — `At`/`Count`
  synthesis (plato-276 territory), record construction from tuple literals.
- demos/webgl/src/plato/array-ext.ts — the hand-written prelude that works
  around all seven, one section per defect, each body mirroring its `.plato`
  source. Delete it as the writer catches up; it is the shortest spec of what is
  missing.
- demos/webgl/scripts/smoke.mts — 30 value checks over the affected members
  (Conway face counts, square area, `Taper3D` scaling). Run it after any writer
  change to see which side moved.

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
