# stdlib global conventions

The single source of truth for **domain / world / API semantics** this vocabulary
assumes everywhere (coordinate frames, matrices, winding, units, sentinels, …).
Each convention is stated **once** here; the owning declaration files carry a
one-line citation (`// Convention: see CONVENTIONS.md - <section>`) instead of
restating it. When two files appear to disagree, this document wins — fix the
file, not this page.

**Authoring style** (how to write library bodies, comments, literals, formulas)
lives in [`STYLE_GUIDE.md`](STYLE_GUIDE.md) — not here. Read both before editing
stdlib sources.

Product authority for world frame and winding: **Ara 3D Studio**. Matrix
multiply / layout understanding matches **`System.Numerics.Matrix4x4`**.

Resolves the "coordination gates" of the plato-257 reviewer pass
(`../docs/plato-257-lessons-v1-recommendations-numbered.md`, section A + item 464).
Z-up / Studio alignment and the style split: tracker [plato-299](../../../tracker/issues/plato-299.md).

---

## Partial operations — no generic `Optional<T>` (A1)

There is **no** generic `Optional<T>` / `Maybe<T>` in this vocabulary, and there
cannot be: Plato sum types are non-generic (generic sums are rejected, CHK306).
An operation that may fail must land on exactly **one** of three sanctioned
concrete styles — any fourth style is rejected in review:

1. **Fallback parameter** for total-ish value ops: `NormalizeOr(v, fallback)`.
2. **Concrete result record with a validity field** for queries:
   `PlaneHit3D { Hit: Boolean; Point; Parameter }` — the reader checks the flag.
3. **Concrete (non-generic) sum where the classification IS the payload**:
   `SphereSphereIntersection = Separate | ExternalTouch | OverlapCircle(...) | ...`.

## Matrices — row-vector multiplication (`System.Numerics`)

Plato multiplies a **row vector on the left**: `v' = v M`. `RowN` holds row *N*,
so element `(r, c)` is component *c* of row *r*, and each row is the image of a
basis vector. Composition reads left to right: in `v (M1 * M2)`, `M1` applies
first. Textbook column-vector `M v` is **not** the convention here; determinant
and "column images" discussions must be read against this row-major layout.

This is the **same layout and multiplication model as `System.Numerics.Matrix4x4`**
(and the other System.Numerics matrix types): C# interop and mental models should
treat Plato `Matrix4x4` as Numerics-compatible, not as a column-major/OpenGL
`M v` textbook matrix.
*Owners:* `matrices.plato` (Matrix2x2..4x4, Matrix3x2, Matrix4x3, MatrixN);
the transform representations in `transforms-trs.plato` /
`transforms-affine.plato` / `transforms-frames.plato` and their bodies in the
`transforms-*.library.plato` files.

## World space — Z-up, right-handed

**World space is right-handed with `+Z` up** (`Up = (0, 0, 1)`), `+X` right,
`+Y` forward/depth as used by the product. Ara 3D Studio is the authority
(`CameraState.Up`, axis gnomon, viewport labels). Plato geometry, cameras, and
importers must match Studio — do not invent a Y-up world convention here.
*Owners:* spatial / camera / transform libraries; Studio viewport and gizmo code.

## Winding, handedness, and normals

Right-handed coordinate system throughout (world Z-up, above). A face/polygon
loop is **counter-clockwise (CCW) as seen from its front / outside**, so the
outward normal follows the right-hand rule over the CCW vertex order.
`CounterClockwise` is the `WindingOrder` default. Ara 3D Studio is the product
authority for this winding choice; Plato matches Studio. A mirroring transform
(negative determinant) inverts winding — the usual source of "suddenly inverted"
meshes after a mirror; re-orient on import rather than carrying a per-mesh flag.
*Owners:* `topology-classification.plato` (WindingOrder), `meshes.plato` (face
normals), `planar-triangles.plato` (Triangle2D: CCW positive area) /
`spatial-patches.plato` (Triangle3D: right-hand normal).

## Typed indices — `-1` means "none"

A cross-array reference is a typed index (implements `Index`, single
`Value: Integer`), never a raw `Integer`. A `Value` of `-1` (any negative) is the
**"none" / absent** sentinel; a non-negative `Value` is a valid zero-based
position. For multi-references, an **empty array** is the corresponding "none".
CSR/offset arrays, counts, and bitmasks are plain `Integer` and are *not*
governed by this rule. (The CSR packing itself is the `Jagged` concept in
`collections-jagged.concepts.plato`, which states that invariant once.) **Axis selectors are no longer plain `Integer`**: a
cardinal-axis choice is the typed `Axis3D` / `Axis2D` / `SignedAxis3D` sum
(`axes.plato`), whose `Ordinal` recovers the `Integer` component index when one
is genuinely needed — kd-tree split axes, `UpAxis` / `ForwardAxis` fields, and
longest-extent queries take an axis type, not a bare `0`/`1`/`2`.
*Owners:* `Index` concept (`collections-indexable.concepts.plato`); every typed
index type (`topology-indices.plato` VertexIndex/UndirectedEdgeIndex/..., domain files);
`ItemIndex` (`numbers.plato`); axis selectors (`axes.plato`
Axis3D/Axis2D/SignedAxis3D).

## Angles — `Angle`-typed, radians-canonical

An angle is always the `Angle` type, **never a raw `Number`**. `Angle` stores
**radians** (`quantities-geometric.plato`); radians is the storage unit, not the
interchange type. Build angles through the unit constructors
`n.Degrees` / `n.Turns` / `n.Gradians` / `n.ArcMinutes` / `n.ArcSeconds`
(`angles.library.plato`); the sole intrinsic path is the radians cast `Angle(x)`
(`intrinsics-scalars.library.plato`). Read them back with the matching accessors
(`a.Degrees`, `a.Turns`, ...).
*Owners:* `Angle` (`quantities-geometric.plato`), `angles.library.plato`.

### Canonical angle interval

`Normalize(a: Angle)` reduces to the canonical **half-open interval `(-pi, pi]`**
— equivalently `(-180 deg, 180 deg]`. This matches the range of `atan2`: `+pi`
is included, `-pi` is not. `Wrap(a, period)` reduces to `[0, period)`.
`LerpShortest` interpolates along the shortest signed arc (the normalized
delta), and `EquivalentAngle` compares canonical (normalized) forms.
*Owner:* `angles.library.plato`.

## Bounds — inclusive, with an empty encoding

Axis-aligned bounds and intervals are **inclusive on both `Min` and `Max`**
(inclusive on `Max` unless a doc comment says otherwise). The **empty** region is
encoded by inversion: `Start > End` for a `NumberInterval`, or `Min` component-
wise greater than `Max` for `Bounds`. An empty region contains no points and is
the identity for `Union` — this is what makes "grow from empty" correct.
*Owners:* `intervals.plato` (NumberInterval, AngleInterval, LengthInterval,
IntegerInterval), `intervals-bounds.plato` (Bounds2D/3D, IntegerBounds2D/3D,
Rect2D); concept in `intervals-bounds.concepts.plato` (IntervalLike, BoundsLike).

## Color — linear-light, straight alpha

`Color` is **linear-light RGBA**, components nominally in `[0, 1]` (wide-gamut /
HDR values may exceed 1). It is the canonical color type for computation;
interpolation and arithmetic are component-wise and assume **unpremultiplied
(straight)** alpha — `Color.A` is straight, not premultiplied. Do **not**
construct a `Color` from sRGB hex/bytes without decoding, and do not `Lerp`
`Color8` or sRGB-encoded bytes without converting to linear first — both are
common and silent errors. `Color8` is the 8-bit, typically sRGB-encoded interop
form.
*Owners:* `color.plato` (Color, Color8).

## View space — camera-local, forward is `-Z`

**View space is not world space.** World remains Z-up (above). After a look-at /
view transform, **camera-local** coordinates are right-handed with the camera
looking down local **`-Z`**, local **`+Y` up**, and local **`+X` right**
(OpenGL-style view frame). A look-at basis takes
`right = normalize(forward × up)` in that right-handed order, where `up` is the
world up axis (`+Z`). Projection matrices and per-backend look-at lowerings must
honour this so handedness bugs are not rediscovered per backend.
*Owner:* `cameras.concepts.plato` (the `Camera` concept), `cameras.plato`
(LookAtCamera and the concrete camera types).

## UV origin — top-left, V increases downward

The texture-coordinate origin `(u, v) = (0, 0)` is at the **top-left**; `U`
increases to the right and `V` increases **downward**, over the unit square
`[0,1] x [0,1]`. This matches the default image storage (`images.plato`:
row-major from the top-left pixel, `ImageOrigin.TopLeft`) and the `UvChart`
mapping (`mesh-attributes.plato`: `Bounds2D` with `X = U, Y = V`). The
OpenGL-style bottom-left origin is opt-in via `ImageOrigin.BottomLeft`.
*(Decision: the reviewer pass (item 464) left the origin unstated; top-left is
chosen because the existing images / mesh-attributes / texturing files already
imply it.)*
*Owners:* `points-parametric.plato` (UvCoordinate, UvwCoordinate); cited from
`mesh-attributes.plato`, `images.plato`, `texturing.plato`.

## Floating-point comparison — one epsilon policy

Numerical near-equality uses a single record, **`ComparisonTolerance`**
(`numbers.plato`) with `{ Absolute: Number, Relative: Number }`. The test is
`|x - y| <= Absolute + Relative * max(|x|, |y|)`. `AlmostEqual` overloads accept
it (`algebra-numeric.library.plato`, beside the scalar-tolerance `AlmostEqual`;
the component-wise vector lift is in `numeric-structures-algebra.library.plato`).

Engineering `Tolerance` (`uncertainty.plato`) is **explicitly NOT** this type:
its `Plus` / `Minus` are asymmetric **acceptance allowances** about a `Nominal`
manufacturing value, not comparison epsilons — do not stuff floating-point
epsilons into it (and use `UncertainNumber` for 1-sigma uncertainties).
*Owners:* `ComparisonTolerance` (`numbers.plato`), `AlmostEqual`
(`algebra-numeric.library.plato`), `Tolerance` (`uncertainty.plato`).
