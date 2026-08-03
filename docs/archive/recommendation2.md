# Plato Type & Interface Surface Review (AI-generated)

> ⚠️ **NOT A SOURCE OF TRUTH — AI-GENERATED SUGGESTIONS.**
> This document is the output of a conversation with an AI model reviewing the
> exported type/interface surface. It is an idea bank, not authoritative
> direction. It mixes genuinely-verified defects (e.g. the `IBounds` /
> `IPrimitiveGeometry3D` broken constraints, which the linter independently
> confirms) with speculative type/interface additions that have not been
> decided. Weigh each suggestion; do not treat any of it as committed.
>
> **The actual sources of truth are:** the guiding vision in
> [`plato-overview.md`](../plato-overview.md) (author-written), the execution
> status and decisions in [`../../../docs/plato-roadmap.md`](plato-roadmap.md),
> the *verified* bug catalog in [`../../../docs/plato-library-review.md`](../reports/plato-library-review.md),
> and the Plato source itself (`stdlib-legacy/`). When this doc conflicts with any
> of those, they win.

---

I've reviewed the exported type/interface surface, cross-checked suspicious declarations against `stdlib-legacy`, and read the companion docs (`docs/reports/plato-library-roadmap-ideas.md` already covers *function-level* content like SDF catalogs and noise, so this review focuses on the **type and interface surface itself** — what the export actually shows). Here's my assessment.

## Overall

The foundation is genuinely good: the algebraic interface ladder (`IAdditive` → `IArithmetic` → `INumerical`), the affine point/vector split via `IDifference<T>`, distinct `Angle`/`Number`, and the `IProcedural<TIn,TOut>` unification of curves, surfaces, and fields are exactly the right bones for a graphics + scientific library. The main problems are (a) a handful of outright defects, (b) inconsistent application of the interfaces you already have, and (c) missing types that block whole categories of use.

## 1. Defects to fix first

These are bugs in the declarations themselves (verified in `stdlib-legacy`, not export artifacts):

- **`IBounds` constraint is broken** — the constraints reference a type parameter `T` that doesn't exist; the parameters are `TValue`/`TDelta`:

```176:178:submodules/Plato/stdlib-legacy/core.interfaces.plato
interface IBounds<TValue, TDelta>    
    where T: IVectorLike, T: IDifference<TDelta>
    inherits IValue
```

- **`IPrimitiveGeometry3D<PrimitiveT>`** has the same bug (`where T: IGeometricPrimitive3D` constrains a nonexistent `T`), and is inconsistently named versus the 2D version's `<T>`.
- **`IDistanceField2D/3D` domain mismatch**: they inherit `IProcedural<Vector2, Number>` but declare `Distance(x, p: Point2D)`. A distance field's domain is *points*, not vectors — it should be `IProcedural<Point2D, Number>` with `Distance` as the sole method (or an alias for `Eval`). This matters a lot because the roadmap's SDF catalog will freeze on top of it.
- **`IPolyLine2D/3D` and `ICurve1D` are dead interfaces**: the concrete `PolyLine2D`/`PolyLine3D` types implement bare `IGeometry2D/3D` instead of the polyline interfaces, so `IPolygon2D/3D` have almost no implementors and generic polyline code can't exist. Either wire the types up or delete the interfaces.
- **`IWholeNumber inherits IInterpolatable`** — `Lerp(a, b, t): Integer` is lossy and surprising. Integers should drop it (or get a `Lerp` returning `Number`).
- **`IMeasure` is not additive.** `IVectorLike` supplies scalar multiply and lerp but not `Add`, so `Angle + Angle` and `Time + Time` don't exist at the interface level. `IMeasure` should inherit `IAdditive` — measures form a vector space over `Number` by definition (the doc comment on it even says so).

## 2. Inconsistencies worth a consistency pass

- **Centered vs. origin-anchored primitives.** `Circle` has a `Center`; `Sphere` is `{Radius}` only. `Rect2D` has a `Center`; `Box` is `{Extent}` at origin. Either convention is defensible (origin-anchored + `Pose3D` composes better), but pick one per dimension and document it — right now 2D and 3D disagree.
- **2D/3D parity gaps**: `Triangle2D`/`Quad2D` implement `IPolygon2D` but `Triangle3D`/`Quad3D` don't implement `IPolygon3D`; `Ray3D` is `IDeformable3D` but `Ray2D` isn't `IDeformable2D`; `Skew2D` exists with no `Skew3D`; `Reflection2D` exists (as an *empty* type — it can't even say which axis) with no `Reflection3D`. An audit table of "interface × type" would surface a dozen of these mechanically.
- **`Cylinder` is `ISurface` while `Cone`, `Capsule`, `Tube` are `ISolid`** — presumably because it's uncapped, but nothing in the names says so. Also `ISolid inherits IProceduralSurface` conflates "volume" with "closed parametric surface"; when the SDF work lands, `ISolid` is the natural home for `IDistanceField3D` too, so I'd reserve the name for that and call the current interface `IClosedSurface`.
- **`Chord` and `Segment` are structurally identical** (`{Arc: Arc}`) and a chord is a *line*, not a region. One of these is misnamed or redundant.
- **`IRealFunction` duplicates `IProcedural<Number, Number>`** — make it inherit, so `Quadratic`, `SineWave`, etc. compose with the curve/field machinery for free.
- Export nit: placeholder types like `ArrayHelpers{}` (library-only carriers) leak into the context doc; filter them in `Plato.ContextExport`.

## 3. Missing interfaces that unlock generic algorithms

The biggest structural gap: **there is no inner-product or norm interface.** `Dot`, `Length`, `Normalize`, `Distance` presumably exist as per-type library functions, but without `IInnerProduct { Dot(a,b): Number }` (and a derived `Length`/`Normalize`), you can't write projection, Gram–Schmidt, closest-point, or least-squares code once. This is the single highest-leverage interface addition for both graphics and scientific computing.

Others, roughly in priority order:

- **`IBounded2D/3D { Bounds(x): Bounds2D/3D }`** — already argued in the roadmap doc (§12); generating implementations will also expose today's gaps. Companion types: `HalfSpace`, `InfiniteLine2D/3D`, `Slab`, and eventually renaming `Line3D` → `Segment3D`.
- **An option story.** There is no `Optional<T>`, so intersection/query APIs (`RayIntersect`, `ClosestPoint` on empty geometry) have no honest return type. Given monomorphization, a simple `Optional<T> { HasValue: Boolean; Value: T }` value type works, or dedicated hit types (`RayHit3D { Hit: Boolean; Distance: Number; Position: Point3D; Normal: Vector3 }`). Without this, the whole query layer will grow `-1.0` sentinels.
- **Colors should be interpolatable.** `Color` and friends implement only `ICoordinate`, so no generic `Lerp` — yet gradients/ramps are the most common color operation in graphics. `Color` (and probably `ColorHSL`/`ColorLAB`) should implement `IVectorLike` or at minimum `IInterpolatable`; LAB/LUV exist *precisely because* lerping in them is perceptually right.
- **`IBoolean` lacks `Xor`** (cheap, and needed for implicit-geometry CSG symmetric difference later).
- The `IArrayLike` "dummy self as type witness" pattern (`CreateFromComponents(_: Self, xs: ...)`) is awkward; if interface-level static constructors ever land in the compiler, this is the first customer.

## 4. Missing types — graphics

- **Mesh vertex attributes.** `TriangleMesh3D` is points + indices only: no normals, UVs, colors, or tangents. This is the largest graphics gap in the surface. Parallel attribute arrays fit Plato better than a fat `Vertex` struct: e.g. `TriangleMesh3D` variants or an interface `IVertexAttributes { Normals: IArray<Vector3>; Uvs: IArray<Vector2>; ... }`. Right now every renderer-side consumer must reconstruct these outside Plato.
- **Unit-vector types** (`UnitVector2/3`, or `Direction2D/3D`). `Ray3D.Direction`, `Plane.Normal`, `LookDirection3D.Direction` are all plain `Vector3`, so "is it normalized?" is a runtime convention. Distinct direction types catch the same bug class that `Angle` vs `Number` already catches — very on-brand for Plato.
- **`Frame3D`** (origin + orthonormal basis) with `ToWorld`/`ToLocal`. `FrenetFrame` exists but is curve-specific and has zero functions; a general frame type also gives you named Y-up/Z-up conversion constants (roadmap §6).
- **Camera/culling types**: `Frustum` (naturally `IArray<HalfSpace>` or six named planes), and a `Camera3D` = pose + projection. `Perspective3D`/`Orthographic3D` exist but have nowhere to live.
- **`OrientedBox2D/3D`** — PCA/fitting output needs a home; `Bounds3D` is axis-aligned only.
- **`Matrix2x2` and `Matrix3x3`.** Only `Matrix3x2` and `Matrix4x4` exist. 3×3 is needed for normal matrices, inertia tensors, covariance, and rotation-without-translation; 2×2 for 2D linear maps and eigen-decomposition of 2D covariance.

## 5. Missing types — scientific computing

- **`Complex`** — the most conspicuous absence for the "scientific" half: oscillations, FFT, conformal maps, 2D rotors. It slots straight into the existing hierarchy (`INumerical` + `IInvertible`) and `Quaternion` already proves the pattern.
- **Interval arithmetic on `NumberInterval`** (roadmap §11) — the *type* exists; adding `IArithmetic` to it (with correct min/max-of-4 semantics) turns it into a scientific tool and unifies vocabulary with `Bounds2D/3D`, which are just vector intervals.
- **Grid/raster types with world mapping**: `Array2D`/`Array3D` exist, but a `Grid2D { Values: IArray2D<T>; Bounds: Bounds2D }` (and 3D voxel equivalent) that implements `IProcedural` via bilinear/trilinear interpolation is the bridge between discrete data (images, height fields, simulation grids, voxel volumes) and the continuous field machinery. Cheap to add, huge payoff.
- **More measures, cautiously.** Only `Time` and `Angle` exist; a full units system would be a breaking redesign (all distances are `Number`), so I would *not* go there now — but `Area` and `Volume` as return types for the roadmap's mass-properties work is a contained, safe step.
- **`Vector8` is oddly alone** — if it's a SIMD lane type, consider whether it belongs in this "interfaces for users" surface at all, or whether `VectorN { Components: IArray<Number> }` (arbitrary-dimension, for statistics/PCA/least-squares) is the type scientific users actually need.

## 6. Suggested order of attack

1. Fix the outright defects (§1) — especially `IBounds`, `IPrimitiveGeometry3D`, and the `IDistanceField` domain, since the SDF catalog will freeze on top of them.
2. Add `IInnerProduct`/norm interfaces and `Optional<T>` — they change what generic code is *possible* and should precede the library build-out in the roadmap doc.
3. `IBounded` + unbounded primitives (`HalfSpace` etc.) — the roadmap already ranks this "before the SDF catalog freezes names," and I agree.
4. Mesh attributes, unit-vector types, `Matrix3x3`, `Complex`, grid types — each unlocks a user-visible category (rendering, robust APIs, physics/statistics, images/voxels).
5. The consistency audit (§2) can proceed incrementally; it's mostly mechanical once conventions are decided.

One caveat from the repo rules: `stdlib-legacy/` is frozen until the Phase 4 bug-fix wave except for additive new files, so §1 items should be queued into that wave, while most of §3–§5 can land as new files any time. If you'd like, I can write this up as a durable doc in `docs/` (e.g. `plato-type-surface-review.md`) alongside the existing roadmap-ideas doc.