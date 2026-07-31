# Port candidates: `stdlib-legacy` → `stdlib`

> **STATUS 2026-07-28: EXECUTED.** Every item below was implemented across four waves
> (commits `ffb2976`, `eb6cdcc`, `b7fb18e`, `c356064`). The gate held throughout: `lint stdlib`
> at **0 parse errors, 0 symbol-resolution errors**; LINT001 **1382 → 332** (−76%),
> LINT003 2762 → 2317.
>
> **Read the "Execution evidence" section at the end before trusting any of this.** The lint gate
> checks name resolution only. A verification pass emitted and ran a slice: `library Polynomials`
> is exactly right over `Number`, but the emitted vector types are throwing stubs, whole-`stdlib`
> emission crashes, and roughly **2% of bodied functions have ever executed**.
>
> This document is now a record, not a plan. Four estimates in it were wrong, corrected inline
> below and summarised here:
>
> - **Item 4 (`unique` builders) is NOT deliverable** — compiler-blocked, not content-blocked.
>   `Freeze`/`Count`/`EmptyList` cannot be declared at all; a builder can be constructed and
>   mutated but never consumed. See the item for the source line.
> - **Items 3 and 6 were ~10× cheaper than estimated.** A library function with a concept-typed
>   first parameter discharges the obligation for *every* implementor, so item 6 went from ~490
>   hand-written bodies to one concept member plus 100 projections. That lever was the single
>   most valuable discovery of the port and is not mentioned anywhere in the original text.
> - **LINT003 is not a valid progress metric.** `CheckUnusedFields` (`Linter.cs:184-198`) does not
>   see field reads inside statement blocks or `var` initializers. Three agents hit this
>   independently. Judge this work by LINT001 and the 0/0 requirement only.
> - **Several item-8/10 type names in this doc collide with existing stdlib types.** The port
>   renamed them: `NPyramid`→`RegularPyramid`, `Pyramid`→`SquarePyramid` (vs existing `Pyramid3D`),
>   `Tube`→`CylindricalShell` (vs existing `TubeSurface`).
>
> Follow-ups the port opened rather than closed are listed at the end.

Survey date 2026-07-28 (revised same day: claims re-verified against both trees; item 1
redesigned around the `Vector` concept; constants promoted to Tier 0).
Method: full read of `stdlib-legacy/*.plato` (4,465 lines, 28 files), name-by-name presence
check against `stdlib/*.plato` (80 files, ~13K lines) plus the Plato navigation index.

**Framing.** `stdlib` is much broader in *vocabulary* (150 concepts, 1111 types) but far thinner in
*executable content*. `stdlib-legacy` is narrow but carries closed-form math, working combinators,
and several load-bearing language idioms that the new tree simply does not have. Everything below
is "legacy has a body / an idea, stdlib has at most a declaration".

Ordered by tier, then value-per-effort within tier.

---

## Tier 0 — day-one content (no dependencies; everything below reads them)

### 1. `library Constants`
`stdlib-legacy/constants.plato`. `stdlib` has **no constants file**; `GoldenRatio`, `SqrtTwo`,
`Ln2`, `Log10E`, `RadiansPerDegree`, `FeetPerMeter`, `PoundPerKilogram`, `GregorianYearDays`,
`XAxis3D`, `UnitInterval`, `UnitCircle` are all absent. Skip `Pi`/`Tau`/`E`/`Epsilon` and the
Number limits — already intrinsics (`stdlib/intrinsics.plato:80-88`).

This is not a Tier-3 content sweep: nearly every math item below consumes it. The axis vectors
feed item 12's `RotateX/Y/Z` and `TranslateX/Y/Z` conveniences (legacy `transforms.plato:421-429`
builds `RotateX` from `XAxis3D`); the unit-conversion constants pair with
`stdlib/quantities.plato`; `UnitInterval` is the natural default for `ParameterDomain.Domain`;
`GoldenRatio` feeds the platonic solids. Retype for the new tree: axis constants as
`Vector2D`/`Vector3D` (or `Direction2D/3D`), `UnitCircle(): Circle` (registry owner
`planar-shapes.plato`).

### 2. Angle unit constructors and accessors
`stdlib-legacy/angles.plato` (whole file, 22 lines): `Turns`/`Degrees`/`Gradians`/
`ArcMinutes`/`ArcSeconds` in both directions (Number→Angle constructors, Angle→Number
accessors) plus `Sec`/`Csc`/`Cot`. `stdlib` declares the "angles are `Angle`, never raw
`Number`" rule but has **no unit constructor at all** — the only way to make an `Angle` is the
radians intrinsic `Angle(x: Number)` (`intrinsics.plato:144`). Every formula in the legacy curve
zoo and solids library is written `t.Turns...`; items 8 and 10 cannot port until this exists.
Companion helper: `UnitCircle(t: Angle): Point2D` (`stdlib-legacy/curves.plato:161`), the
angle-to-point evaluator under `NGonPoint`, `CirclePoints`, and every polar curve.

---

## Tier 1 — structural ideas (change what stdlib can express)

### 3. Component-wise lifting on the `Vector` concept
The legacy engine is `IArrayLike<T>` (`stdlib-legacy/core.interfaces.plato:38`:
`NumComponents`/`Components`/`CreateFromComponents`/`CreateFromComponent`) plus the combinators
(`array.plato:42-61`: `MapComponents`, `ZipComponents` 2- and 3-ary, `AllComponents`,
`AnyComponent`, `AllZipComponents`, `AnyZipComponents`) plus `core.library.plato:32-94`, where
every `Number` intrinsic and helper (`Abs`, `Cbrt`, `Exp`, `Floor`, `Pow`, `Clamp`, `Lerp`,
`Fract`, `AlmostEqual`, …) is lifted to every vector type by a one-line call. ~50 functions
written once.

**Do not port `IArrayLike<T>` verbatim.** The right vehicle already exists: `concept Vector`
(`stdlib/vectors.plato:15`), which inherits `Indexable<Number>` and so already carries the whole
read side (`Count` = legacy `NumComponents`, `At` = component access). What it lacks is exactly
what `numeric-structures.library.plato:64-67` and `:139-144` already record as
`TODO(concept-gap)`: no construction, so no map/zip/fold is derivable, and `SumComponents`,
`MinComponent`, per-component `Abs`/`Floor`, and even generic `Zero`/`One` bodies are stuck
per-type. The port is therefore two members on `Vector`:

- `FromComponents(_: Self, xs: Array<Number>): Self` (legacy `CreateFromComponents`)
- `Broadcast(_: Self, x: Number): Self` (legacy `CreateFromComponent`; gives `Zero`/`One`/
  `MinValue`/`MaxValue` bodies for free, as `core.library.plato:32-35` shows)

with `MapComponents`/`ZipComponents`/the predicates/the folds and the ~30 lifted `Number`
functions landing in `numeric-structures.library.plato`, discharging its own TODOs.

Why this beats the verbatim port: the generic `T` in `IArrayLike<T>` is only ever instantiated
at `Number` (`IVectorLike inherits IArrayLike<Number>`); the read half is already inherited;
it adds zero new taxonomy (this survey already rejects the `IVectorLike`/`INumerical` stack
below); and it matches the new tree's idiom (bare concept names, `Self` first, fields like
`VectorN.Components` already named accordingly). All seven `Vector` implementors
(`Number2/3/4/8`, `Vector2D/3D`, `VectorN`) light up at once. The same recipe later extends
`MatrixLike` (its construction gap is the other half of `:139-144`), but that is a follow-on,
not part of this item.

### 4. `unique type` affine builders (`List<T>` / `Buffer<T>`)
`stdlib-legacy/unique.plato:29,34` — the whole file, including the effect table in its header
comment (observe `Count`/`At`; mutate `Add`/`AddRange`/`Set`; consume `Freeze`).

`unique type` appears **nowhere** in `stdlib`. Every collection in the new tree is
immutable-by-value with no construction story, so anything that builds a mesh, samples a curve
into an array, or accumulates points has no expressible implementation. Port the two
declarations and the doc-comment contract; the runtime already exists
(`Plato.Intrinsics/PlatoList.cs`, `PlatoBuffer.cs`). Note the bodiless intrinsic signatures
currently live in `stdlib-legacy-tests` (`library UniqueIntrinsics`), per the header comment.

### 5. Function-valued fields and the combinator pattern
`stdlib-legacy/fields.plato` (`ScalarField3D { Function: Function1<Vector3, Number> }` +
`Eval`/`Combine`/`MapValue`/`Union`/`Intersection`/`Difference`/`Offset`, lines 9-42) and the
design essay in `procedurals.plato:1-76`.

`stdlib` has `concept Procedural<TDomain, TRange>` and a large `implicit-sdf.plato` node/tree
vocabulary, but **no type stores a function value** (every `Function1` in the tree is a
parameter) and no combinators — `MapValue` is absent, the only `Combine` is a sum-type tag.
Legacy proved this shape works end-to-end and monomorphizes. Port `ScalarField3D`-style
function-carrying types plus the combinator set; they are the compositional layer that
`implicit-sdf.plato`'s SDF trees currently only describe declaratively.

The `procedurals.plato` prose (domain/range dimension table, predicates as `→Boolean`, discrete
vs continuous forms) is the best written rationale in either tree and belongs in
`functional.concepts.plato` as doc comments. Note its library body is fully commented out — the
generic version blocked on in-function constraints; port the *monomorphic* form that works.

### 6. Obligation-filling library pattern for concrete quantity types
`stdlib-legacy/measures.plato` — 15 lines, and the header comment is the point: concepts that
inherit `Scalable`/`Comparable` have no generic component-wise implementation, so each concrete
measure must supply `Multiply`/`Divide`/`Modulo`/`LessThanOrEquals` explicitly or the compiler
emits throwing stubs.

`stdlib/quantities.plato` declares ~35 physical quantity types against `concept Quantity`.
Unless that pattern is followed, all ~35 will emit throwing stubs. Port the pattern and the
warning.

---

## Tier 2 — concrete math the new tree declares but does not compute

### 7. Closed-form polynomial / spline evaluation
`stdlib-legacy/algebra.plato` (153 lines) — `Linear`, `Quadratic`, `Cubic`, `Quartic`, each
with first and second derivatives; `CubicBezier`/`QuadraticBezier` with first and second
derivatives; `Hermite` + derivative; `CatmullRom` + derivative; `Barycentric`; `SmoothStep`
(`:148`); `SmootherStep`. All generic over `INumerical`, so one definition serves `Number`,
`Vector2`, `Vector3` — after item 3, the same genericity lands on `Vector`.

`stdlib` names `Hermite`/`CatmullRom` in `splines.plato` — as **types only**.
`curves-surfaces.library.plato` (~520 lines) is entirely derived traits (`SpeedAt`,
`TangentDirectionAt`, …) built on an assumed `Eval` that nothing implements. `SmoothStep` /
`SmootherStep` are absent from all 80 files (the closest is the `SmoothLerp` TODO in
`numeric-structures.library.plato:192-197`). Porting this file gives the whole curve/spline
layer its missing base case.

### 8. The named-curve zoo evaluators
`stdlib-legacy/curves.plato` (606 lines). `stdlib/curves-2d.plato` and `curves-3d.plato`
already declare `Spiral`, `Rose`, `Limacon`, `Cardioid`, `Lissajous`, `Epicycloid`,
`Hypotrochoid`, `Lemniscate`, `TorusKnot`, `Trefoil`, `FigureEightKnot` — legacy has the
**formula for each**, with the Wikipedia citation attached.

The polar half of the structural story is **already adopted**: `concept PolarCurve2D` with
`RadiusAt(x: Self, angle: Angle)` exists (`stdlib/curves-surfaces.concepts.plato:90`), and
`PolarPositionAt`/`CartesianPositionAt` are implemented generically
(`curves-surfaces.library.plato:332-345`). So the polar port is just the one-line `RadiusAt`
bodies — fourteen named curves in legacy (`curves.plato:299-381`).

Still worth importing as an idea: **`IAngularCurve2D`/`IAngularCurve3D`**
(`curves.plato:150`): curves whose natural parameter is an `Angle`, with a single bridge
`Eval(curve, t: Number) => curve.Eval(t.Turns)`. `stdlib` has no angular-curve concept; it
matches the `Angle`-not-`Number` rule and costs one function per family. Depends on item 2.

Not yet in stdlib at all: `ButterflyCurve`, `CycloidOfCeva`, `TschirnhausenCubic`,
`ConchoidOfDeSluze`, `SinusoidalSpiral`, `TrisectrixOfMaclaurin`.

### 9. Signed-distance primitive formulas
`stdlib-legacy/sdf3d.plato` (66 lines) — `SdSphere`, `SdBox`, `SdRoundBox`, `SdTorus`,
`SdVerticalCapsule`, `SdCappedCylinder`, `SdPlane`, `OpUnion`/`OpIntersect`/`OpSubtract`, and
the polynomial `OpSmoothUnion`. Iñigo Quílez formulas, deliberately array-free and lambda-free
so they port to GLSL/CUDA unchanged.

`stdlib/implicit-sdf.plato` declares the *modifiers* (`SdfRoundingModifier`,
`SdfOnionModifier`, `SdfTwistModifier3D`, …) and `MetaBall2D/3D`, but not one distance
function. Direct fit; the file was written with the GLSL backend in mind, which matches
stdlib's priority-1..4 backend policy.

### 10. Parametric surfaces for solids
`stdlib-legacy/solids.plato:99-246` — `library Solids` with `NGonPoint` (`:107`; walk a regular
N-gon at constant rate, linear between vertices) and `SquarePoint`, plus the UV convention
stated precisely (`:91-96`): origin-centred, Z up, U wraps around Z (`ClosedX` always true), V
runs up the shape, so a UV quad grid meshes any solid exactly.

`stdlib/solids.plato` + `surfaces.plato` declare the shapes; `NGonPoint` and the UV convention
are absent. The convention text alone prevents a class of bugs. Type coverage is better than
first surveyed: `ConeSegment` ≈ `ConicalFrustum` (`spatial-primitives.plato:104`) and
`NPrism` ≈ `RegularPrism` (`solids.plato:99`) already exist; genuinely missing are `NPyramid`,
`Pyramid` (square), and `Tube` (hollow cylinder). Depends on item 2 (`Turns`, `UnitCircle(Angle)`).

### 11. Quad-grid face-index generation
`stdlib-legacy/meshes.library.plato:116-133` — `QuadFaceIndices(col, row, nCols, nRows)` with
the `a`/`b`/`c`/`d` corner diagram, and `AllQuadFaceIndices(nCols, nRows, closedX, closedY)`
(`:125`) handling wrap-around in either direction via `%`. `QuadFaceIndices` appears nowhere in
`stdlib`. This is the routine everything grid-shaped needs (surface tessellation, heightfields,
revolutions) and it is easy to get wrong.

### 12. `Deform` as the primitive, `Transform` as the derivative
`stdlib-legacy/meshes.library.plato:49-100`: every geometric type implements
`Deform(x, f: Function1<Point3D, Point3D>)` (`:49-75`), then **one** line (`:84`) —
`Transform(self: IDeformable3D, t: Transform3D) => self.Deform(p => p.Vector3.Transform(t.Matrix))`
— gives every deformable type a full transform surface, from which `Scale`/`ScaleX/Y/Z`,
`Rotate`/`RotateX/Y/Z`, `Translate`/`TranslateX/Y/Z` follow as one-liners (`:86-100`).

`stdlib` is further along here than the rest of Tier 2: `Deformable2D/3D` already carry generic
`Translate`/`ScaleAbout` (`intervals-transforms.library.plato:189-227`). Still missing: the
concrete per-type `Deform` bodies, the Deform-implies-Transform bridge, and the rotation
conveniences (`RotateAbout` is explicitly blocked, `:209-211` — the bridge plus item 1's axis
constants unblock it). Cheap, high fan-out.

---

## Tier 3 — content and helpers

### 13. Sampling helpers
`Fractions` (`stdlib-legacy/integers.plato:18`) and `LinearSpace`
(`stdlib-legacy/interval.plato:54`) — n evenly spaced parameters — drive
`Sample(curve, numPoints)`, `ToPolyLine2D/3D`, `CirclePoints`, `Sample(a, b, n)` for any
`Interpolatable`, and `RegularPolygon.At`. Neither name exists in `stdlib`, so
`curves-surfaces.library.plato`'s `Sample`/`ToPolyLine` have no discretization primitive under
them. Also missing: `Staircase{Floor,Ceiling,Round}`, `UnitQuad`/`UnitTriangle` constants.

### 14. Number helpers still missing from `core-algebra.library.plato`
Legacy `core.library.plato` vs stdlib's `core-algebra.library.plato`. Already covered under new
names (`Fract`→`FractionalPart`, `FromOne`→`OneMinus`, `Sqr`→`Square`, `Pow3`→`Cube`,
`ClampZeroOne`→`Saturate`). Genuinely absent: `Pow4`, `Pow5`, `InversePow`, `AlmostZeroOrOne`,
`Eight` (eighth), `Sixteenth`, `Million`, `Billion`, `Millionth`, `Billionth`, and the
left-scalar `Multiply(scalar, x)` overload on `Scalable`. (`MultiplyEpsilon` is a deliberate
drop, not a gap: folded into `AlmostEqual(x, y, tolerance)` —
`core-algebra.library.plato:12-13,209` and LIBRARIES.md rule 4.)

### 15. Named colors
`stdlib-legacy/colors.constants.plato` — 141 CSS/X11 named colors as `ByteRGB` literals.
`stdlib/color.plato` has `Color`, `Color8`, `ColorHSV`, `ColorHSL`, `ColorStop`,
`ColorGradient` and not one named color. Pure content, zero design risk, mechanical port to
`Color8`.

---

## Explicitly not worth porting

- `stdlib-legacy/core.interfaces.plato` as a *taxonomy* — stdlib's concept split (`Additive`,
  `Multiplicative`, `Scalable`, `Lattice`, `Clampable`, `Normed`, `MetricSpace`,
  `Difference<T>`) is strictly better factored than legacy's `IVectorLike`/`INumerical`/
  `IRealNumber` stack. Take the construction idea (item 3) and leave the rest.
- Legacy `IValue`/`IAny` — superseded by stdlib `core.concepts.plato`.
- Legacy `transforms.plato` — the representation types and conversion library are superseded by
  `stdlib/transforms.plato` + its inline `library Transforms`; `LookAt`/`Perspective`/
  `Orthographic` live in `cameras.plato` and the `Matrix4x4` Create* intrinsics. Only `Skew2D`
  and `PlaneProjection3D` (shadow projection) have no counterpart — add on demand.
- Legacy `coordinates.types.plato`, `bounds.plato`, `interval.plato`, `integers.plato`,
  `geometry.types.plato`, `colors.plato` — stdlib's `points.plato` / `intervals-bounds.plato` /
  `planar-shapes.plato` / `color-spaces.plato` already cover these and more, *except* the
  sampling helpers claimed by item 13 and the angle units claimed by item 2.
- Legacy V1 intrinsics not carried into `stdlib/intrinsics.plato` — verified all deliberate or
  already covered: the Vector8 SIMD surface (`Sum`, `ConditionalSelect`, `Lower`/`Upper`,
  `MinMagnitude`/`MaxMagnitude`), the IEEE exotica (`BitIncrement`, `IEEERemainder`, `ILogB`,
  `ScaleB`), the midpoint-rounding zoo, and the record-update/jagged-view helpers are all
  listed with rationale in that file's porting notes (`intrinsics.plato:436-455`);
  `CreateFromVertices` (`:386`) and `Square` (`:61`) already exist; `Distance`/
  `DistanceSquared` are implemented generically (`numeric-structures.library.plato:74-80`).
- The commented-out generic `library procedurals` body — port the idea (item 5), not the code;
  it is blocked on constraints inside function bodies.

---

## Suggested sequencing

1. Items 1, 2 — constants and angle units first. They have zero dependencies, are pure
   content, and items 8, 10, and 12 cannot even be transcribed without them (`Turns`,
   `UnitCircle`, axis vectors); items 6/13 read them too. Landing them first also lets every
   later port compile as written instead of inlining magic numbers to be cleaned up later.
2. Items 3, 4, 6 — the enabling idioms; nothing in Tier 2 lands cleanly before them.
3. Items 7, 11, 13, 14 — pure math and helpers, no new vocabulary needed.
4. Items 5, 9, 8, 10, 12 — the domain bodies, each now resting on 1/2/3/7/13.
5. Item 15 — content sweep, parallelizable with step 4.

Gate each step with `dotnet run --project Plato.CLI -c Release -- lint stdlib` (0 parse errors,
0 resolution errors); LINT001 counts should **fall** as libraries implement declared members.

---

## Follow-ups the port opened (2026-07-28)

Ordered by how much they block.

1. **`unique` builders are unusable** (item 4). `Freeze`, `Count`, `EmptyList` cannot be declared:
   a generic function with one or fewer parameters throws at
   `Plato.Compiler/Analysis/FunctionInstance.cs:162-165` and aborts the whole compilation.
   `stdlib-legacy-tests` (`library UniqueIntrinsics`) declares the same signatures and *does*
   compile, so the two trees reach different paths — that is the thread to pull. Until it is
   fixed no affine algorithm is writable in stdlib.
2. **The emitted vector types are throwing stubs — the concept-generic bodies compute only at
   `Number`.** A 115-file minimal slice emits **713 `NotImplementedException` throws**:
   `_Vector3D.g.cs` stubs `Add`, `Subtract`, `Multiply(float)`, `Magnitude`, `Zero`. So
   "one definition serves Number, Vector2D, Vector3D and every quantity type" — the claim
   item 3 and item 7 both rest on — is true at the type level and **false at runtime today**.
   `CubicBezier` over `Vector3D` throws. `DistanceToSphere` emits as
   `p.PositionVector().Magnitude().Subtract(radius)` and would throw rather than return `d-r`.
   This is the single largest gap between "lints clean" and "works", and it is invisible to lint.

   *(On the `VectorN` `At`/`Count` question, which this list got wrong twice in opposite
   directions: the writer DID have a real bug of exactly that shape, fixed in `099c447`. The
   single-collection-field special case was gated on a type-NAME test against
   `"Array"`/`"Array2D"`/`"Array3D"`. stdlib's `VectorN` declares `Components: Array<Number>`,
   matched that name, and emitted correctly — which is why emitting and running it showed
   `Count == 5`. But `IArray<T>`, the spelling `stdlib-legacy` uses throughout, missed the case
   and emitted `Count => 1` with an `At` returning the list where a `Number` was declared; and
   `Array2D`/`Array3D` matched when they should not, having no linear `Count`. The fix keys off
   the field type's concept instead. Latent, not live — no type in the shipping generation
   reaches the synthesis. Lesson: the inference identified a real defect but the wrong type, and
   the execution check tested the one spelling that worked and over-generalised from it. Neither
   alone was sufficient.)*
3. **LINT003 statement-body blindness** (`Linter.cs:184-198`). Fields read only inside a `var`
   initializer or a statement block are reported unread. Real in-tree false positives today:
   `Frame3D.Origin/XAxis/YAxis/ZAxis`, `AffineTransform3D.Matrix`, `CylindricalShell.*`,
   `RegularPyramid.SideCount/Radius`. Fixing it would make LINT003 usable as a metric again.
4. **`concept AngularCurve2D/3D` was declined, not rejected.** Item 8's agent fell back to
   per-type `t.Turns` bodies because `LIBRARIES.md` rule 6 then forbade new concepts. Rule 6 has
   since been rewritten to permit deliberate concept extension, so the collapse is now available;
   `TODO(concept-gap)` notes in `curves-2d.plato` / `curves-3d.plato` say exactly what it buys.
5. **A generic `Hash(self: Index) => self.Value.Hash`** would likely discharge the LINT001 `Hash`
   obligation for the dozens of typed index types at once. Belongs in P2
   (`collections-functional.library.plato`). Not attempted during the port to avoid a cross-agent
   collision.
6. **`Deform` bodies for the 13 `Deformable2D/3D` implementors** — they live in `lines.plato`,
   `planar-shapes.plato`, `polygons.plato`, `spatial-primitives.plato`, all outside the transform
   agent's allowlist. The `Deform`-implies-`Transform` bridge is landed and generalized to 13
   transform representations, but discharges nothing until a type declares both `Deformable3D`
   and `Transformable<T>` — today none does.
7. **Content gaps left with explicit TODOs**, each needing a primitive the tree lacks:
   `Clothoid2D.Eval` (Fresnel integrals / series summation), B-spline and NURBS `Eval`
   (Cox–de Boor knot-span fold), `NaturalCubicSpline1D` (tridiagonal solve), Bezier/BSpline/
   Nurbs/Coons patch `Eval`, `SurfaceOfRevolution`/`SweptSurface`/`LoftedSurface` (rotation-
   minimizing frame or a surface normal `ParametricSurface` does not expose), dense/sparse
   polynomial `Eval` (Horner needs a reachable fold).
8. **`PointOnUnitCircle` is duplicated.** Defined locally in `library Solids` with a `NOTE(dedupe)`
   because the canonical `UnitCircle(t: Angle): Point2D` still does not exist in stdlib. Pick one
   home and delete the other.

---

## Execution evidence (2026-07-28)

A verification pass emitted a slice of `stdlib` to C#, compiled it, and ran value checks — the
first time any of this code has executed. Method:

```
dotnet build Plato.CLI/Plato.CLI.csproj -c Release
dotnet Plato.CLI.dll <slice> <out> --csharp-style=extensions --scalar=float --no-properties
# then build and run a checker against <out>
```

**What passed.** All 29 functions in `library Polynomials` over `Number`, exact to float
precision: Bezier and Hermite return their endpoints at t=0/1, `CatmullRom` midpoint is exact,
all 12 derivative functions agree with central differences of their own base function,
`SmoothStep`/`SmootherStep` exact at 0/0.5/1, and `Eval`/`Derivative` on all four polynomial
types. `NGonPoint` re-hosted verbatim: vertices at exactly r=1, edge midpoints at exactly
`cos(pi/n)`, and `NGonPoint(0) == NGonPoint(1)`.

Separately verified outside the tree by numerical integration: the Zelen & Severo normal-CDF
coefficients (max abs error 6.9e-8 against a reference `erf`, inside the published 7.5e-8), and
the spherical-cap, conical-frustum and circular-segment centroids (agreement to ~1e-14 at
intermediate, non-degenerate values).

**Scorecard: ~35 of 1,692 bodied functions verified — about 2%**, and 0% of the vector or
quantity instantiations of even those.

**Defects the pass found**, beyond the throwing stubs in follow-up 2 above:

- Whole-`stdlib` emission **crashes**: `No ground TIR for bodied AnimationTrack.ValueAt`
  (`keyframes-tracks.plato`), and again for every generic body monomorphized against `Array4D`
  (`collections-functional.library.plato` vs `primitives.plato`). It does not reach output.
- `Angle` is declared here with a `Radians` field but is fieldless/intrinsic in `stdlib-legacy`,
  so the emitted `_Angle.g.cs` duplicates conversions the handwritten `Plato.Intrinsics/Angle.cs`
  already has (CS0557) and emits `Amount()` against a symbol that does not exist.
- Function-typed parameters lower to bare `System.Func` with no type arguments (CS0305) —
  affects every function-valued field type added for item 5.
- `--no-properties` emits `((int)self.Count)` against a method in 16 places (CS0030).
- `Tuple9`/`Tuple10` call a `CombineHashCodes` overload that does not exist.
- `MatrixLike` obligations (`RowCount`/`ColumnCount`/`ElementAt`), `Quaternion.Lerp` and
  `Plane.ClosestPoint` are unsatisfied (CS0535).
- Reverse direction: `Plato.Intrinsics` depends on members `stdlib` never declares —
  `Number.Pow2`/`Pow3`, a no-arg `Vector3.AlmostZero()`, `Integer.ToNumber`.

**The cheapest fix for all of it**: a gate in `tools/check-all.ps1` that merely emits `stdlib`
and compiles the result. The two crashes above fail it in under ten seconds today. Every defect
in this section was found because someone went looking; a gate turns that into every run.
