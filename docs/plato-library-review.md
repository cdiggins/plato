# Plato Standard Library & Ara3D.Geometry Review

*Reviewed: `submodules/Plato/stdlib-legacy`, `PlatoStandardLibrary/KitchenSink`, `ara3d-sdk/src/Ara3D.Geometry`,
`Ara3D.Models`, `Ara3D.F8`, and spot-verification against `ara3d-sdk/src/Plato.Generated`. July 2026.*

The architecture is right: pure value types, type-class interfaces, monomorphized output, a
formula-shaped source. The problem is that the library as it stands does not live up to the README's
pitch ("catch the bug classes geometry code actually has", "trivially testable") — because nothing is
testing it, and nothing is checking it. This review found roughly two dozen outright math errors in
~3,500 lines of source. Every one of them is exactly the kind of bug the language exists to prevent,
and every one is trivially catchable by the tooling recommended below.

---

## 1. Verified math bugs in stdlib-legacy

Ordered by blast radius.

### 1.1 `MagnitudeSquared` divides by component count — the worst bug in the library
`core.library.plato:38`:
```plato
MagnitudeSquared(v: IVectorLike): Number => v.SumSqrComponents / v.NumComponents;
```
This is generated into **every** IVectorLike type (verified in `_Vector3.g.cs:125`, `_Point3D.g.cs:86`,
`_Vector2/4/8`, `_Angle`, `_Time`). Consequences:
- `Vector3.Magnitude` = √(x²+y²+z² / 3) while `Vector3.Length` (intrinsic) is correct. **Same struct, two
  different answers for the same concept.**
- Generic `Normalize`, `Distance`, `DistanceSquared`, `Angle`, `IsParallel` in `vectors.plato` are all
  wrong by factors of √n wherever the intrinsic doesn't shadow them (Point2D/3D, and all generic paths).
- The C# comment in `GeometryUtil.cs:941` — "the old Angle function does not work" — is this bug being
  routed around in the host language instead of fixed at the source.

Fix: `=> v.SumSqrComponents;`. Then delete the C# workaround `AngleBetween` or make it call the fixed one.

### 1.2 Constants (`constants.plato`)
- `MinNumber() => 3.40282347E+38` and `MaxNumber() => -3.40282347E+38` — **swapped signs, both wrong.**
  Also redundant with the `Number.MinValue`/`MaxValue` intrinsics used elsewhere; delete them.
- `GoldenRatio() => 1.0 + 5.0.Sqrt / 2.0` — precedence error: computes 1 + (√5)/2 ≈ 2.118, not (1+√5)/2 ≈ 1.618.
- `Pi() => 3.1415926535897` — truncated (also `SqrtTwo`, `Ln10`, etc.). Harmless at float32, wrong the day
  `Number` becomes double. Write full-precision literals or compute (`2.0.Sqrt`).

### 1.3 Angles (`angles.plato:4`)
```plato
Degrees(x: Integer): Angle => x.Number.Turns;   // 90.Degrees == 90 FULL TURNS
```
Should be `x.Number.Degrees`. Note: the checked-in generated code (`_Integer.g.cs:34`) has the **correct**
body — meaning generated output and source have drifted (see §3.1).

### 1.4 Algebra (`algebra.plato`)
- `Barycentric` (line 4): `(v1 + (v2 - v1)) * uv.X + (v3 - v1) * uv.Y` — the parenthesization collapses the
  first term to `v2 * u` and drops `v1` entirely. Should be `v1 + (v2 - v1) * uv.X + (v3 - v1) * uv.Y`.
- `SmootherStep` (line 151): `x.Pow3 * (x * 6.0 - 15.0) + 10.0` = 6x⁴−15x³+10 (doesn't even pass through 0 at 0).
  Correct: `x.Pow3 * (x * (x * 6.0 - 15.0) + 10.0)` = 6x⁵−15x⁴+10x³. (`SmoothStep` above it is correct.)

### 1.5 Curves (`curves.plato`) — the formula file has broken formulas
- `SineWave` (line 29): `amplitude * (frequency * x.Turns.Sin + phase)` — frequency scales the output and
  phase is added to the output. Should be `amplitude * (x * frequency + phase).Turns.Sin` (or similar).
- `Lissajous` (line 219): `((t + d).Sin, b.Turns.Sin)` — the y-component is **constant in t**, and field `A`
  is unused. Correct: `((t * a + d).Sin, (t * b).Sin)`.
- `Epicycloid` / `Hypocycloid` / `Epitrochoid` / `Hypotrochoid` (lines 176–205): in all four, the frequency
  ratio was folded into the amplitude. E.g. epicycloid should be
  `(R+r)·cos t − r·cos((R+r)/r · t)` but is written `(R+r)·cos t − r(R+r)·cos(t/r)`.
  (The KitchenSink `std.parametric.curves` versions are structurally correct — port those.)
- `Rose` (line 343): `k * t.Cos` is a circle of radius k. A rose is `(t * k).Cos`. (Also `K: Integer` in the
  type but `k: Number` in the function.)
- `FermatsSpiral` (line 392): `(a * t.Turns.Sqr).Sqrt` = √a·|θ| — linear in θ, i.e. an Archimedean spiral.
  Fermat is r = a·√θ.

### 1.6 Geometry library (`geometry.library.plato`)
- `Corners(x: Bounds2D)` (line 152): returns **8 points** — the 4 corners duplicated, copy-pasted from the 3D
  version.
- Line 101: `Points(t: Triangle2D)` duplicated inside the Triangle3D section — should be `Triangle3D` (which,
  in this library, has no `Points`; `meshes.library.plato` defines another one — dedupe).

### 1.7 Transforms (`transforms.plato`)
- `LookAt3D.Matrix` (line 367): `Matrix4x4.CreateWorld(0.0, r.Forward, ZAxis3D)` — the origin is hardcoded to
  zero; `r.Origin` is ignored. A look-at that doesn't look *from* anywhere.
- `LookAt3D` and `LookDirection3D` implement `IRotation3D`, which requires `Quaternion(Self)` — **never
  defined**. Unfulfilled interface obligation, silently accepted (no checker).
- Composition order needs a decision and a property test. With System.Numerics row-vector convention
  (`v·M`, so `v·(A·B)` applies A first):
  - `Matrix(TRSTransform3D) => (T * R) * S.Matrix` means translate → rotate → scale. Every other engine's
    "TRS" means scale → rotate → translate (`S·R·T`). The doc comment matches the code, but the *name*
    promises the opposite.
  - `Matrix(Pose3D) => T.Matrix * R.Matrix` rotates *after* translating, which orbits the position around
    the origin — almost certainly not what "pose" means anywhere.

### 1.8 Interface declarations with unchecked/wrong constraints
- `core.interfaces.plato:177` — `IBounds<TValue, TDelta>` constraint reads `where T: IVectorLike, T: IDifference<TDelta>`;
  there is no `T`. Dead text.
- `geometry.interfaces.plato:215` — `IPrimitiveGeometry3D<PrimitiveT>` constrains `T`, not `PrimitiveT`. Same.
- Every `ISolid` in `solids.plato` inherits `IProceduralSurface`, which requires `Eval(Vector2): Point3D`,
  `ClosedX`, `ClosedY` — **no solid implements any of them**, anywhere. The obligations are decorative.
  Worse: `Sphere` and `Cylinder` are missing from `Plato.Generated` entirely, because handwritten C# structs
  with the *same names and different definitions* exist in `Ara3D.Geometry` (`Cylinder.cs` = line+radius vs.
  Plato's height+radius). Two `Cylinder`s, one namespace family, silently resolved by exclusion.

### 1.9 Intrinsics signature errors (`intrinsics.plato`)
- `Repeat(x: IAny, n: Integer): IArray<IAny>` — type-erasing; should be generic over `$T`.
- `WithNext(self: IArray<$T>, f: Function2<$T, $T, $TR>, first: Boolean): IArray<$T>` — takes a function
  returning `$TR` but declares a return of `IArray<$T>`. Also `first` actually means "wrap around/closed";
  rename it.
- `Indices` vs `MapIndices` — duplicates.

### 1.10 The Angle firewall is open at the seam
`Plato.Intrinsics/Angle.cs:34-40` defines **implicit** `float → Angle` and `Number → Angle`. The README's
claim that "degree/radian confusion is impossible" is void while any bare number silently becomes an angle
(as radians). Library code itself leans on it (`Identity(_: AxisAngle) => (ZAxis3D, 0.0)`).
Recommendation: make the conversion explicit (`Angle.FromRadians(x)` / `x.Radians` as a Number→Angle
constructor — which, notably, doesn't exist in stdlib-legacy: you can write `x.Turns` and `x.Degrees` but not
`x.Radians`). Keep `Angle → Number` implicit if needed for interop, but not the inbound direction.

### 1.11 Handwritten C# bugs found in passing
- `GeometryUtil.cs:896-904` — all three `Within` overloads test `v.Y <= b.X` (should be `v.X <= b.X`).
- `GeometryUtil.cs:115` — `u = float.Clamp(t, 0, 1)` clamps `t` into `u`.
- `GeometryUtil.cs:930` — `Epsilon = 1e-15` used in `SafeDivide(float, ...)`: below float32 resolution,
  so the guard never fires for subnormal-ish denominators that still explode. Use ~1e-6f for float paths.
- Also note three different epsilons across the codebase: Plato `Constants.Epsilon = 1e-7`, KitchenSink
  `1e-15`, GeometryUtil `1e-15` + `DefaultFloatEpsilon 1e-6`. Unify with named, documented tolerances.

---

## 2. What these bugs have in common (and what to build)

None of these are exotic. They are: precedence slips, copy-paste between dimensions, frequency/amplitude
transposition, unfulfilled interface promises, and drift between source and generated output. All four
categories are mechanically detectable.

### 2.1 A checker, even a dumb one, before the type checker
The full native type checker is the roadmap item; you don't have to wait for it. A lint pass over the AST
(you already have the parser) that reports:
1. interface obligations not implemented by any library function for a given type (`ISolid.Eval`, `IRotation3D.Quaternion`);
2. `where` clauses referencing undeclared type variables (`IBounds`, `IPrimitiveGeometry3D`);
3. declared-but-unused type fields (`Lissajous.A`, `LookAt3D.Origin` — both actual bugs);
4. duplicate signatures (`Points(Triangle2D)` ×2, `Indices`/`MapIndices`);
5. mismatched generic return types (`WithNext`).
Five checks, five classes of real bugs found in this review.

### 2.2 Law-based property tests, generated from the interfaces
The interfaces are already documented as algebraic structures ("Abelian group", "field", "monoid"). Encode
the laws once per interface and let the compiler stamp out NUnit tests for every implementing type —
exactly the same amplification the language already does for code:
- `IAdditive`: `a+b == b+a`, `a + a.Negative` ≈ zero.
- `IInterpolatable`: `Lerp(a,b,0)==a`, `Lerp(a,b,1)==b`.
- `IVectorLike`: `Magnitude(Normalize(v)) ≈ 1`, `Magnitude == Length` where both exist (catches §1.1 instantly).
- `ICurve` + closure: `Eval(0) ≈ Eval(1)` for every `IClosedCurve` (catches Lissajous, Rose).
- Analytic derivatives vs. central finite differences for the entire `Algebra` library (catches SmootherStep, Barycentric).
- Round-trips: `Degrees(Degrees(x)) == x`-style unit inverses (catches §1.3), matrix/quaternion round-trips,
  `Invert(Invert(m)) ≈ m` (would pin down the TRS convention).
This is the highest-leverage robustness investment available, and it is *cheap* because the semantic
surface is 3.5k lines and fully known to the compiler.

### 2.3 Regeneration must be enforced, not hoped for
`angles.plato` (wrong) vs `_Integer.g.cs` (right) proves the source and generated output are edited/built
out of sync. Whichever direction the drift ran, the "single source of truth" property is currently violated
in the flagship repo. Add a CI step: regenerate, `git diff --exit-code` on `Plato.Generated`. And resolve
the `Sphere`/`Cylinder` name collisions explicitly rather than by omission.

---

## 3. Bloat: what to remove

The README's honest cost is that one declaration fans out into hundreds of members. That's only a win when
each declaration deserves the fanout. Cuts, in order of confidence:

1. **KitchenSink and legacy, after harvesting.** Two divergent copies of the standard library in one repo is
   the exact "which copy is true" failure Plato exists to kill. Port the good parts (below), delete the rest.
2. **Numeric magnitude helpers on `IScalarArithmetic`** — `Hundred`, `Thousand`, `Million`, `Billion`,
   `Hundredth`, `Thousandth`, `Millionth`, `Billionth`, `Sixteenth`, `Tenth`, `Eight` (sic — it computes an
   eighth). ~11 functions × every vector-like/measure type ≈ hundreds of generated members that answer no
   real geometric need. Keep `Half`, `Twice`, maybe `Quarter`. (If `Million` survives anywhere, it belongs on
   Number only.)
3. **The float-intrinsic fanout onto all of `IVectorLike`** (`core.library.plato:43-68`): `BitDecrement`,
   `BitIncrement`, `ILogB`, `ScaleB`, `IEEERemainder`, `ReciprocalSqrtEstimate`… on `Angle`, `Time`, `Color`,
   `Point3D`. `Time.ILogB` is API noise; noise is the enemy of the discoverability story (the useful 60
   members of Vector3 are buried in 340). Restrict this block to `IVector`, and curate even there.
4. **Functionless types.** `Lens`, `Ring`, `Sector`, `Chord`, `Segment` (three of which are identical
   one-field wrappers around `Arc`), `LogPolarCoordinate`, `HorizontalCoordinate`, `GeoCoordinate`,
   `GeoCoordinateWithAltitude` — no library function consumes or produces most of them. A type with no
   functions is a liability in a library whose pitch is "everything composes". Cut now, re-add with
   functions when needed.
5. **Color spaces without conversions.** `colors.plato` declares 8 color types and `colors.constants.plato`
   140 named colors, but stdlib-legacy contains **zero conversion functions** — not even Color↔ByteRGB, let
   alone HSV→RGB. Either port a conversion library (this is a great Plato showcase: pure formulas with
   citations) or trim to `Color` + `ByteRGB(A)`.
6. **Unit-conversion trivia in `Constants`** (`PoundPerTon`, `TroyOuncePerGram`, `GregorianYearDays`,
   `FeetPerMile`): orphaned without the measures library. Move into the measures work (§4.5) or delete.
7. **Name synonyms.** `Skip`/`Drop`, `SubArray`/`Slice`, `Indices`/`MapIndices`, `Lesser`,`Greater` vs
   `Min`/`Max`, `Magnitude` vs `Length`, `Sqr` vs `Pow2` vs `Square` (KitchenSink adds `SquareRoot` vs
   `Sqrt`). Every alias doubles surface and halves greppability. Pick one of each; the compiler makes
   renames global and safe.
8. **`Vector8` in stdlib-legacy** (or commit to it). Nothing in the standard library uses it, `Ara3D.F8`
   hand-writes the real SIMD kernels, and an 8-float Plato type compiled through the `IArrayLike` lambda
   machinery will never compete with hand-written AVX. Either document it as a .NET-intrinsic-only type
   with polyfill semantics for other targets, or remove it from the portable surface. SIMD width is a
   target property, not a domain property, and it sits oddly in a "write once, retarget" language.

---

## 4. Additions that earn their keep

### 4.1 A `Surfaces` library in Plato (highest value)
Implement `Eval(solid, uv)` for Sphere, Cylinder, Cone, ConeSegment, Torus, Capsule, Box, Ellipsoid, Tube,
NPrism — port of `Ara3D.Geometry/SurfaceFunctions.cs`, whose own header says "Created raw by ChatGPT,
to-be-reviewed". This one move: fulfills the `ISolid` interface contract, deletes ~600 lines of
acknowledged-unreviewed C#, resolves the duplicate Sphere/Cylinder definitions, and gives every solid free
`ToQuadGrid`/mesh discretization through the existing procedural machinery.

### 4.2 Harvest KitchenSink into stdlib-legacy
Worth porting, roughly in order: **easings** (pure, popular, great demo), **InverseLerp/Remap/Nearest**
(currently reinvented in C# as `Unlerp` *and* `InverseLerp` in the same file), the **correct** cycloid
family from `std.parametric.curves`, catenary, sinc/step/rect signal functions, and — if you want the units
story — the measure types with dimensional `Multiply`/`Divide` (Length×Length→Area is a genuinely good
Plato showcase). Then delete KitchenSink (§3.1).

### 4.3 Vector2 parity and 2D basics
2D is a first-class citizen for CAD workflows and currently the poor cousin: no `Cross`/perp-dot for
Vector2, no `Perpendicular`, no `Rotate(Vector2, Angle)`, no `SignedAngle`, no Vector2 swizzles (Vector3
has nine), `Line2D` has no `Bounds2D` (Line3D has Bounds3D), `Ray2D` isn't deformable (Ray3D is),
`Triangle3D` isn't `IPolygon3D` while `Triangle2D` is `IPolygon2D`, and `PolyLine2D/3D` don't implement the
`IPolyLine2D/3D` interfaces that exist for them (those interfaces currently have **no** implementors).
Do a systematic 2D/3D parity audit — mechanically derivable from the compiler, and a great generated-docs
artifact ("capability matrix: type × interface").

### 4.4 Pure functions currently stranded in C#
These are total functions over values with no mutation — they are Plato code written in the wrong language:
- Angle utilities: `Normalize`, `AngularDistance`, `AngularLerp` (`AngleUtils.cs`).
- `RotateTo(from, to): Quaternion`, `AlignZAxisWith` (`GeometryUtil.cs` — including the commented-out
  first attempt that should just be deleted).
- Axis machinery: `GetLongestAxis`/`GetComponent`/`WithComponent`/ordered-axis-indices.
- Point/line queries: `Distance(Point3D, Line3D)`, `ProjectOntoLine`, `Reject`, `SignedDistanceAlongLine`.
- `Bounds3D.Intersects`, `Expand`, `FastTransform` (the abs-matrix AABB transform), interval intersection.
- Bilinear quad evaluation, `Inset`, barycentric (once fixed).
- `SafeNormalize` — and make generic `Normalize`'s zero-vector policy explicit and documented while at it.
Migrating them shrinks `GeometryUtil.cs` (1,362 lines, self-described "TODO: many of these functions should
live in other places") toward the genuinely-imperative core, and every migrated function becomes available
to all future targets.

### 4.5 Where C# should stay C# — write the policy down
A one-page CONTRIBUTING rule: *pure total functions over values → Plato; algorithms with mutation and data
structures (Topology, AABB tree, Delaunay, remesher, welder, marching cubes), I/O (OBJ/STL/BFAST), SIMD
kernels (F8), and lambda-holding types (Curve3D, Sdf3D, ParametricSurface — until Plato has function-typed
fields, which `Procedural` already sketches) → C#.* The current split is accidental, and it shows: curves,
surfaces, transforms, and bounds ops each exist in both layers with different names and subtly different
semantics.

### 4.6 Un-comment the `procedurals` library
`procedurals.plato` — the entire library body (Combine/Map/MapDomain/Compose/Union/Intersection/Difference)
is commented out, presumably blocked on compiler features (generic constraints in functions). This is the
compositional heart of the procedural-geometry story, and the C# `Sdf3D`/`SdfExtensions`/`Curve3D` classes
are hand-written stand-ins for it. Whatever compiler work unblocks it is worth prioritizing; it converts
three C# files into ~30 lines of Plato and delivers the "combine procedurals into complex shapes" promise
in the file's own doc comment.

### 4.7 Smaller, worthwhile additions
- `Radians(x: Number): Angle` — the missing explicit constructor (then close the implicit hole, §1.10).
- `Sum`, `Average`, `Filter`, `Contains`, `IndexOf`, `MinBy`/`MaxBy`, `Sort` for `IArray` — `Filter`'s
  absence in particular forces algorithms into C#.
- `Option<T>` (or totalization policy). `Invert → Tuple2<Matrix3x2, Boolean>` and
  `Decompose → Tuple4<...,Boolean>` are C-style success flags leaking through the portable surface. A
  library-level `Option` (a 2-field struct + functions; no language change needed) is more idiomatic, more
  self-documenting, and ports cleanly.
- Named result types instead of anonymous tuples where domain meaning exists (`DecomposedTransform`).
- Deterministic noise (Perlin/Worley are already pure in C#; as Plato they'd reach GLSL when it lands).
- `Angle`-typed `SinCos` destructuring already exists — use it in the curve library for speed and clarity.

---

## 5. Discoverability

1. **Generate the docs from the compiler.** One page per interface: laws, functions, implementing types;
   one page per type: fields, interfaces, members grouped by source library, *with the stdlib-legacy line the
   member came from*. The compiler knows all of it; `docs.html` in Plato.Generated is the seed. This is the
   discoverability superpower no hand-written library has — currently unrealized.
2. **A NAMING.md with the naming laws**, enforced by the linter: unit constructors read as `x.Degrees`;
   conversions as `.Vector3`/`To3D`; predicates as `Is*`; no synonyms (§3.7); `With*` for field updates;
   `Create*` reserved for statics. Also rename the cryptic ones: `FromOne` (reads like a constructor; it's
   `1 - x` — `OneMinus` says what it is), `Eight` → `Eighth` (if it survives), `WithNext(..., first)` →
   `wrapAround`.
3. **Document the conventions that carry semantic weight** in one place: row-vector matrices (`v·M`),
   composition order (after fixing §1.7), handedness, Z-up (the C# code assumes Z-up in `CreateBasisFromZ`,
   `LookAtMatrix` uses UnitZ up — never stated), winding order for `Flip`/normals, the parameterization of
   `t` (turns, not radians — `Eval(curve, t)` multiplies by `Turns`, which is a good convention that is
   documented nowhere).

---

## 6. Notes on Ara3D.Models and Ara3D.F8

Both are appropriately host-side and in decent shape; nothing here changes the recommendations above.
- `InstanceStruct` (64-byte, 3×4 transform + packed material) is a good renderer-facing design. The triple
  representation of material (Material / PackedMaterial / raw bytes in InstanceStruct) could collapse to two.
- `IModel3D : ITransformable3D<IModel3D>` plus `Model3D : ITransformable3D<Model3D>` forces the awkward
  explicit-interface double implementation; consider dropping the interface-typed variant and letting
  extension methods operate on the concrete self-type.
- `Model3DExtensions.ToColoredMesh` allocates per-vertex colors even for uncolored paths (its own TODO);
  split the paths.
- F8 is fine as the .NET SIMD kernel layer. The open question is Plato's `Vector8` (§3.8) — decide whether
  it's portable surface or .NET intrinsic, and document the decision.

---

## 7. Suggested sequencing

1. **Fix the verified bugs** (§1) — a day of edits, mostly one-liners; regenerate and recommit.
2. **Regen-diff CI** (§2.3) — prevents drift permanently.
3. **Law-based property tests** (§2.2) — locks the fixes in and catches the next Rose/Lissajous at commit time.
4. **The 5-check linter** (§2.1) — cheap, catches the structural class.
5. **Prune** (§3) before adding — every removal shrinks the generated surface and the docs.
6. **Surfaces library + KitchenSink harvest + C#-to-Plato migration** (§4.1, 4.2, 4.4).
7. **Generated docs + naming/convention pages** (§5).

The through-line: Plato's premise is that determinism and purity make a library verifiable. The premise is
sound — this review *was* that verification, done by hand. Build the tooling so it never has to be done by
hand again.

---

## 8. Addendum (2026-07-07): bugs found by the Phase 1 conformance harness

The law/witness suite (`stdlib-legacy-tests`, `Ara3D.SDK.ConformanceTests`) found four defects beyond §1,
proving the harness earns its keep on day one:

1. **COMPILER BUG — additive chains are emitted right-associatively.** `2t³ − 3t² + 1` (Hermite,
   `algebra.plato`, *source correct*) generates as `2t³ − (3t² + 1)`; `-x.Twice + 3.0` (SmoothStep) as
   `(2x + 3).Negative` (verified in shipped `_Vector2.g.cs:97`). Consequence: shipped `SmoothStep`,
   `Hermite`, `HermiteDerivative`, `CatmullRom` (t≠0), and `CatmullRomDerivative` are all wrong even
   though §1 rated their source correct. Any generated formula mixing `+`/`−` in a chain is suspect
   until the compiler is fixed. **This must be fixed before the §1 bug-fix wave** — until then,
   "fix the source and regenerate" does not guarantee correct output.
2. **`ArcMinutes`/`ArcSeconds` inverted** (`angles.plato:12-13`): multiply by 60 where they should
   divide — 60 arcminutes currently equals 3,600 degrees.
3. **`Time` ships four throwing members** (`LessThanOrEquals`, `Multiply`, `Divide`, `Modulo`) —
   unfulfilled `IMeasure` obligations generated as `throw new NotImplementedException()`. Same class as
   §1.8 / the `Point2D.Subtract(Vector2)` stub; lint check §2.1(1) catches all of these.
4. **`Area(Triangle2D)` negates the signed area** (`geometry.library.plato:80`, shoelace terms
   transposed): a CCW unit right triangle yields −0.5.

Ergonomics findings from writing the laws: `Number` does not implement `INumerical`
(`IRealNumber` doesn't inherit it), so `SmoothStep`/Bezier/`Barycentric`/Hermite are unavailable on
scalars; `IAdditive` has no zero element (identity laws must be phrased subtractively); and the
library's `AlmostEqual` is relative-only, admitting zero error against an exact 0 — unusable for
tolerance checks near zero (see roadmap 1.4).
