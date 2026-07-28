# Port candidates: `stdlib-legacy` → `stdlib`

Survey date 2026-07-28. Method: full read of `stdlib-legacy/*.plato` (4,465 lines, 28 files),
name-by-name presence check against `stdlib/*.plato` (80 files, ~13K lines) plus the Plato
navigation index over the legacy tree.

**Framing.** `stdlib` is much broader in *vocabulary* (150 concepts, 1111 types) but far thinner in
*executable content*. `stdlib-legacy` is narrow but carries closed-form math, working combinators,
and several load-bearing language idioms that the new tree simply does not have. Everything below
is "legacy has a body / an idea, stdlib has at most a declaration".

Ordered by value-per-effort.

---

## Tier 1 — structural ideas (change what stdlib can express)

### 1. The `ArrayLike` component-wise derivation engine
`stdlib-legacy/core.interfaces.plato:38` (`IArrayLike<T>`: `NumComponents` / `Components` /
`CreateFromComponents` / `CreateFromComponent`) plus `array.plato:31` (`MapComponents`,
`ZipComponents` 2- and 3-ary, `AllComponents`, `AnyComponent`, `AllZipComponents`,
`AnyZipComponents`) plus `core.library.plato:45-94`.

That machinery is what lets ~50 vector functions be written **once**: every `Number` intrinsic
(`Abs`, `Cbrt`, `Exp`, `Floor`, `Pow`, `Clamp`, `CopySign`, `FusedMultiplyAdd`, …) is lifted to
every vector type by a one-line `MapComponents`/`ZipComponents` call, and so is every derived
`Number` helper (`Lerp`, `Fract`, `Between01`, `AlmostEqual`, `Sqr`, …).

`stdlib` has **no equivalent concept at all** — `MapComponents`/`ZipComponents` appear zero times.
Its `Indexable<T>` is a read-only accessor with no reconstruction operation, so nothing can be
lifted generically. This is the single highest-leverage port: without it every vector function in
the new tree has to be written per type, per arity.

### 2. `unique type` affine builders (`List<T>` / `Buffer<T>`)
`stdlib-legacy/unique.plato` — the whole file, including the effect table in its header comment
(observe `Count`/`At`; mutate `Add`/`AddRange`/`Set`; consume `Freeze`).

`unique` appears **nowhere** in `stdlib`. Every collection in the new tree is immutable-by-value
with no construction story, so anything that builds a mesh, samples a curve into an array, or
accumulates points has no expressible implementation. Port the two declarations and the doc-comment
contract; the runtime already exists (`Plato.Intrinsics/PlatoList.cs`, `PlatoBuffer.cs`).

### 3. Function-valued fields and the combinator pattern
`stdlib-legacy/fields.plato` (`ScalarField3D { Function: Function1<Vector3, Number> }` +
`Eval`/`Combine`/`MapValue`/`Union`/`Intersection`/`Difference`/`Offset`) and the design essay in
`procedurals.plato:1-76`.

`stdlib` has `concept Procedural<TDomain, TRange>` and a large `implicit-sdf.plato` node/tree
vocabulary, but **no value that stores a function** and no combinators — `MapValue` is absent, the
only `Combine` is a sum-type tag. Legacy proved this shape works end-to-end and monomorphizes.
Port `ScalarField3D`-style function-carrying types plus the combinator set; they are the
compositional layer that `implicit-sdf.plato`'s SDF trees currently only describe declaratively.

The `procedurals.plato` prose (domain/range dimension table, predicates as `→Boolean`, discrete vs
continuous forms) is the best written rationale in either tree and belongs in
`functional.concepts.plato` as doc comments. Note its library body is fully commented out — the
generic version blocked on in-function constraints; port the *monomorphic* form that works.

### 4. Obligation-filling library pattern for concrete quantity types
`stdlib-legacy/measures.plato` — 15 lines, and the header comment is the point: concepts that
inherit `Scalable`/`Comparable` have no generic component-wise implementation, so each concrete
measure must supply `Multiply`/`Divide`/`Modulo`/`LessThanOrEquals` explicitly or the compiler emits
throwing stubs.

`stdlib/quantities.plato` declares ~35 physical quantity types against `concept Quantity`. Unless
that pattern is followed, all ~35 will emit throwing stubs. Port the pattern and the warning.

---

## Tier 2 — concrete math the new tree declares but does not compute

### 5. Closed-form polynomial / spline evaluation
`stdlib-legacy/algebra.plato` — `Linear`, `Quadratic`, `Cubic`, `Quartic`, each with first and
second derivatives; `CubicBezier`/`QuadraticBezier` with first and second derivatives; `Hermite` +
derivative; `CatmullRom` + derivative; `Barycentric`; `SmoothStep`; `SmootherStep`. All generic over
`INumerical`, so one definition serves `Number`, `Vector2`, `Vector3`.

`stdlib` names `Hermite`/`CatmullRom` in `splines.plato`, `polynomials.plato`,
`keyframes-tracks.plato` — as **types only**. `curves-surfaces.library.plato` (527 lines) is
entirely derived traits (`SpeedAt`, `IsPlanarAt`, `TangentDirectionAt`, …) built on an assumed
`Eval` that nothing implements. `SmoothStep` / `SmootherStep` are absent from all 80 files.
Porting this file gives the whole curve/spline layer its missing base case.

### 6. The named-curve zoo evaluators
`stdlib-legacy/curves.plato` (606 lines). `stdlib/curves-2d.plato` and `curves-3d.plato` already
declare `Spiral`, `Rose`, `Limacon`, `Cardioid`, `Lissajous`, `Epicycloid`, `Hypotrochoid`,
`Lemniscate`, `TorusKnot`, `Trefoil` — legacy has the **formula for each**, with the Wikipedia
citation attached.

Two structural ideas ride along and are worth more than the formulas:
- **`IAngularCurve2D`/`IAngularCurve3D`**: curves whose natural parameter is an `Angle`, with a
  single bridge `Eval(curve, t: Number) => curve.Eval(t.Turns)`. Matches stdlib's "angles are
  `Angle`, never raw `Number`" rule and costs one function per family.
- **`IPolarCurve`**: declare only `GetRadius(t: Angle): Number`; `EvalPolar` and Cartesian `Eval`
  come free. Fourteen named curves in legacy are one line each because of it. `stdlib/points.plato`
  has `PolarCoordinate` but no polar-curve concept.

Not yet in stdlib at all: `ButterflyCurve`, `CycloidOfCeva`, `TschirnhausenCubic`,
`ConchoidOfDeSluze`, `SinusoidalSpiral`, `TrisectrixOfMaclaurin`, `FigureEightKnot`.

### 7. Signed-distance primitive formulas
`stdlib-legacy/sdf3d.plato` — `SdSphere`, `SdBox`, `SdRoundBox`, `SdTorus`, `SdVerticalCapsule`,
`SdCappedCylinder`, `SdPlane`, `OpUnion`/`OpIntersect`/`OpSubtract`, and the polynomial
`OpSmoothUnion`. Iñigo Quílez formulas, deliberately array-free and lambda-free so they port to
GLSL/CUDA unchanged.

`stdlib/implicit-sdf.plato` declares the *modifiers* (`SdfRoundingModifier`, `SdfOnionModifier`,
`SdfTwistModifier3D`, …) and `MetaBall2D/3D`, but not one distance function. Direct fit; the file
was written with the GLSL backend in mind, which matches stdlib's priority-1..4 backend policy.

### 8. Parametric surfaces for solids
`stdlib-legacy/solids.plato:99+` — `library Solids` with `NGonPoint` (walk a regular N-gon at
constant rate, linear between vertices) and `SquarePoint`, plus the UV convention stated precisely:
origin-centred, Z up, U wraps around Z (`ClosedX` always true), V runs up the shape, so a UV quad
grid meshes any solid exactly.

`stdlib/solids.plato` + `surfaces.plato` declare the shapes; `NGonPoint` and the UV convention are
absent. The convention text alone prevents a class of bugs. Also missing as types: `ConeSegment`,
`NPrism`, `NPyramid`, `Tube`, `Pyramid`.

### 9. Quad-grid face-index generation
`stdlib-legacy/meshes.library.plato:107-133` — `QuadFaceIndices(col, row, nCols, nRows)` with the
`a`/`b`/`c`/`d` corner diagram, and `AllQuadFaceIndices(nCols, nRows, closedX, closedY)` handling
wrap-around in either direction via `%`. `QuadFaceIndices` appears nowhere in `stdlib`. This is the
routine everything grid-shaped needs (surface tessellation, heightfields, revolutions) and it is
easy to get wrong.

### 10. `Deform` as the primitive, `Transform` as the derivative
`stdlib-legacy/meshes.library.plato:47-100`: every geometric type implements
`Deform(x, f: Function1<Point3D, Point3D>)`, then **one** line —
`Transform(self: IDeformable3D, t: Transform3D) => self.Deform(p => p.Vector3.Transform(t.Matrix))`
— gives every deformable type a full transform surface, from which `Scale`/`ScaleX/Y/Z`,
`Rotate`/`RotateX/Y/Z`, `Translate`/`TranslateX/Y/Z` follow as one-liners.

`stdlib` has `Deformable`/`Transformable` concepts and mentions `Deform` in nine files, but the
Deform-implies-Transform bridge and the convenience surface are not there. Cheap, high fan-out.

---

## Tier 3 — content and helpers

### 11. `library Constants`
`stdlib-legacy/constants.plato`. `stdlib` has **no constants file**; `GoldenRatio`, `SqrtTwo`,
`Ln2`, `Log10E`, `RadiansPerDegree`, `FeetPerMeter`, `PoundPerKilogram`, `GregorianYearDays`,
`XAxis3D`, `UnitInterval`, `UnitCircle` are all absent (only `Pi`/`Tau`/`E`/`Epsilon` exist, in
`intrinsics.plato`). The unit-conversion constants pair naturally with
`stdlib/quantities.plato`.

### 12. Named colors
`stdlib-legacy/colors.constants.plato` — the ~140 CSS/X11 named colors as `ByteRGB` literals.
`stdlib/color.plato` (62 lines) has `Color`, `Color8`, `ColorHSV`, `ColorHSL`, `ColorStop`,
`ColorGradient` and not one named color. Pure content, zero design risk, mechanical port to
`Color8`.

### 13. Number helpers still missing from `core-algebra.library.plato`
Legacy `core.library.plato` vs stdlib's 51-function `core-algebra.library.plato`. Already covered
under new names (`Fract`→`FractionalPart`, `FromOne`→`OneMinus`, `Sqr`→`Square`, `Pow3`→`Cube`,
`ClampZeroOne`→`Saturate`). Genuinely absent: `Pow4`, `Pow5`, `InversePow`, `MultiplyEpsilon`
(the relative-epsilon basis for `AlmostEqual`), `AlmostZeroOrOne`, `Eight`, `Sixteenth`, `Million`,
`Billion`, `Millionth`, `Billionth`, and the left-scalar `Multiply(scalar, x)` overload.

### 14. Array/collection intrinsics not carried over
Present in `stdlib-legacy/intrinsics.plato`, absent from `stdlib/intrinsics.plato`:
`Sum`, `ConditionalSelect`, `MinMagnitude`/`MaxMagnitude`, `Middle`, `PrependAndAppend`,
`CreateFromVertices`, `Sqr`, `Distance`/`DistanceSquared` (declared as a concept in stdlib but not
as an intrinsic), and the matrix row accessors `Rows`/`Columns`/`Row1..4`/`WithRow1..4`,
`Lower`/`Upper`/`WithLower`/`WithUpper`.

Deliberate exclusions — do **not** port, they violate the stated intrinsics policy in
`stdlib/README.md`: `BitIncrement`/`BitDecrement`, `ReciprocalEstimate`,
`ReciprocalSquareRootEstimate`, `RoundToZero`/`RoundAwayFromZero` (MidpointRounding variants),
`IEEERemainder`, `ILogB`, `ScaleB`.

### 15. Sampling helpers
`LinearSpace` and `Fractions` (n evenly spaced parameters) drive
`Sample(curve, numPoints)`, `ToPolyLine2D/3D`, `CirclePoints`, `Sample(a, b, n)` for any
`Interpolatable`, and `RegularPolygon.At`. Neither name exists in `stdlib`, so
`curves-surfaces.library.plato`'s `Sample`/`ToPolyLine` have no discretization primitive under them.
Also missing: `Staircase{Floor,Ceiling,Round}`, `UnitQuad`/`UnitTriangle` constants.

---

## Explicitly not worth porting

- `stdlib-legacy/core.interfaces.plato` as a *taxonomy* — stdlib's concept split (`Additive`,
  `Multiplicative`, `Scalable`, `Lattice`, `Clampable`, `Normed`, `MetricSpace`, `Difference<T>`) is
  strictly better factored than legacy's `IVectorLike`/`INumerical`/`IRealNumber` stack. Take
  `IArrayLike` (item 1) and leave the rest.
- Legacy `IValue`/`IAny` — superseded by stdlib `core.concepts.plato`.
- Legacy `coordinates.types.plato`, `bounds.plato`, `interval.plato`, `angles.plato`,
  `integers.plato` — stdlib's `points.plato` / `intervals-bounds.plato` / `quantities.plato` already
  cover these and more.
- The commented-out generic `library procedurals` body — port the idea (item 3), not the code; it
  is blocked on constraints inside function bodies.

---

## Suggested sequencing

1. Items 1, 2, 4 — the enabling idioms; nothing else lands cleanly first.
2. Items 5, 9, 13, 15 — pure math and helpers, no new vocabulary needed.
3. Items 3, 7, 6, 8, 10 — the domain bodies, each now resting on 1/5/15.
4. Items 11, 12, 14 — content sweeps, parallelizable.

Gate each step with `dotnet run --project Plato.CLI -c Release -- lint stdlib` (0 parse errors,
0 resolution errors); LINT001 counts should **fall** as libraries implement declared members.
