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

## Conversions — type-named is implicit, `ToX` is explicit

A one-parameter function whose name is its own concrete return type **is an
implicit conversion**. Nothing marks it as one: the compiler infers it from the
spelling (`FunctionInstance.IsImplicitCast`), the type checker feeds it into
overload resolution as a cast relation (`TypeRelations.ComputeCasts`), and the
C# writer emits an `implicit operator` for every concrete implementer. Naming a
function after a type is therefore a decision about the type system, not about
taste.

**The bar is faithfulness.** A type-named conversion may only be written when
the result denotes the *same mathematical object* as its argument, re-expressed.
Nothing about the value may be invented and nothing may be discarded.

The practical test is: **did writing it force a choice the argument did not
already contain?** If the body had to pick a resolution, a cell size, a
tolerance, a sample count, a rounding mode, or which diagonal to split a quad
along, the conversion invents information and is not faithful. If it drops a
channel, a distance metric, a bound, a parameterisation, or any structure a
reader could expect to survive, it is not faithful either. Faithful conversions
are re-encodings: a rotation into the quaternion that represents it, a field
into a closure that evaluates it, a rigid pose into the affine transform that
performs the same map.

**Everything else is spelled `ToX` and stays explicit.** `ToX` is the honest
name for an approximation, a sampling, a projection, or a discard: the call site
shows the reader that something was decided or lost. A `ToX` function is
ordinary — it is never picked up as a cast — so it may take extra parameters and
carry the choice in its signature.

**Never both spellings for one (source, target) pair.** One pair, one function,
one name. Two names for the same conversion means one of them is wrong about
whether the conversion is faithful, and a reader has no way to tell which.

**Where a whole family converts to one canonical form, declare the obligation on
an interface** rather than repeating a library function per type. The transform
representations do this: `IAffine2D` / `IAffine3D` / `IRigid2D` / `IRigid3D` /
`IRotational3D` (`transforms.concepts.plato`) each name the single canonical
type their implementers convert into, which is what makes composing two
different representations well defined. Members named after a type follow this
same rule inside an interface, and the writer reifies one implicit conversion
per concrete implementer. A single conversion from an odd type stays an ordinary
library function.

**Because implicitness is inferred from the name, a return type can mint casts
by accident.** An interface member named `Length`, `Area` or `Volume` returning
`Number` is a plain accessor; change that return type to the quantity type of
the same name (`quantities.types.plato`) and every implementer silently gains an
implicit conversion. The guard is the cast-inventory pin
(`tests/PlatoTests/ImplicitCastInventoryTests.cs`, golden file
`tests/PlatoTests/implicit-cast-inventory.txt`): every implicit cast the
stdlib defines is listed there, so a conversion nobody meant to create fails the
test instead of shipping. Update the golden deliberately, in the commit that
earns the new casts.

That pin covers conversions **declared in Plato**. The C# writer additionally
mints an implicit operator in both directions between any single-field type and
its field's type, which no Plato declaration mentions and the pin does not see —
tracked as `compiler-399`.

**Families already decided.** Implicit, because each is a re-encoding: the
transform representations onto their common ground, and every field / SDF onto
its function-valued form (`fields-implicits.library.plato`). Explicit, and to
stay explicit: `IMesh3D.ToTriangleMesh` (triangulating a polygon or quad mesh
chooses diagonals; a tetrahedral mesh keeps only its boundary; a rich mesh drops
its attributes), `IPointCloud3D.ToPointCloud` (drops normals, colors,
intensities), `ToRegion` / `ToVolume` on a signed distance field (keep
membership, drop the distance), the curve-to-polyline samplers (a sample count is
invented), `ToPoint` on a vector (a displacement is not a position), and
`ToInteger` on a number (rounds). Where an interface declares the obligation,
the whole family shares one spelling: `IMesh3D` is explicit for everyone,
including the implementers whose own lift happens to be faithful.

*Owners:* the convention is stated only here. Conversion-bearing declarations
cite it — `transforms.concepts.plato` for the transform family,
`fields-implicits.library.plato` for the field and SDF lifts.

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
*Owners:* `matrices.types.plato` (Matrix2x2..4x4, Matrix3x2, Matrix4x3, MatrixN);
the transform representations in `transforms.types.plato` and their bodies in
`transforms.library.plato`.

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
*Owners:* `topology.types.plato` (WindingOrder), `meshes.types.plato` (face
normals), `planar.types.plato` (Triangle2D: CCW positive area) /
`spatial-primitives.types.plato` (Triangle3D: right-hand normal).

## Typed indices — `-1` means "none"

A cross-array reference is a typed index (implements `Index`, single
`Value: Integer`), never a raw `Integer`. A `Value` of `-1` (any negative) is the
**"none" / absent** sentinel; a non-negative `Value` is a valid zero-based
position. For multi-references, an **empty array** is the corresponding "none".
CSR/offset arrays, counts, and bitmasks are plain `Integer` and are *not*
governed by this rule. (The CSR packing itself is the `Jagged` interface in
`collections.concepts.plato`, which states that invariant once.) **Axis selectors are no longer plain `Integer`**: a
cardinal-axis choice is the typed `Axis3D` / `Axis2D` / `SignedAxis3D` sum
(`axes.types.plato`), whose `Ordinal` recovers the `Integer` component index when one
is genuinely needed — kd-tree split axes, `UpAxis` / `ForwardAxis` fields, and
longest-extent queries take an axis type, not a bare `0`/`1`/`2`.
*Owners:* `Index` interface (`collections.concepts.plato`); every typed
index type (`topology.types.plato` VertexIndex/UndirectedEdgeIndex/..., domain files);
`ItemIndex` (`numbers.types.plato`); axis selectors (`axes.types.plato`
Axis3D/Axis2D/SignedAxis3D).

## Angles — `Angle`-typed, radians-canonical

An angle is always the `Angle` type, **never a raw `Number`**. `Angle` stores
**radians** (`quantities.types.plato`); radians is the storage unit, not the
interchange type. Build angles through the unit constructors
`n.Degrees` / `n.Turns` / `n.Gradians` / `n.ArcMinutes` / `n.ArcSeconds`
(`angles.library.plato`); the raw path is the radians cast `Angle(x)`
(`angles.library.plato`). Read them back with the matching accessors
(`a.Degrees`, `a.Turns`, ...).
*Owners:* `Angle` (`quantities.types.plato`), `angles.library.plato`.

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
*Owners:* `intervals.types.plato` (NumberInterval, AngleInterval, LengthInterval,
IntegerInterval), `intervals.types.plato` (Bounds2D/3D, IntegerBounds2D/3D,
Rect2D); interface in `intervals-bounds.concepts.plato` (IInterval, IBounds).

## Color — linear-light, straight alpha

`Color` is **linear-light RGBA**, components nominally in `[0, 1]` (wide-gamut /
HDR values may exceed 1). It is the canonical color type for computation;
interpolation and arithmetic are component-wise and assume **unpremultiplied
(straight)** alpha — `Color.A` is straight, not premultiplied. Do **not**
construct a `Color` from sRGB hex/bytes without decoding, and do not `Lerp`
`Color8` or sRGB-encoded bytes without converting to linear first — both are
common and silent errors. `Color8` is the 8-bit, typically sRGB-encoded interop
form.
*Owners:* `color.types.plato` (Color, Color8).

## View space — camera-local, forward is `-Z`

**View space is not world space.** World remains Z-up (above). After a look-at /
view transform, **camera-local** coordinates are right-handed with the camera
looking down local **`-Z`**, local **`+Y` up**, and local **`+X` right**
(OpenGL-style view frame). A look-at basis takes
`right = normalize(forward × up)` in that right-handed order, where `up` is the
world up axis (`+Z`). Projection matrices and per-backend look-at lowerings must
honour this so handedness bugs are not rediscovered per backend.
*Owner:* `cameras.concepts.plato` (the `Camera` interface), `cameras.types.plato`
(LookAtCamera and the concrete camera types).

## UV origin — top-left, V increases downward

The texture-coordinate origin `(u, v) = (0, 0)` is at the **top-left**; `U`
increases to the right and `V` increases **downward**, over the unit square
`[0,1] x [0,1]`. This matches the default image storage (`images.types.plato`:
row-major from the top-left pixel, `ImageOrigin.TopLeft`) and the `UvChart`
mapping (`meshes.types.plato`: `Bounds2D` with `X = U, Y = V`). The
OpenGL-style bottom-left origin is opt-in via `ImageOrigin.BottomLeft`.
Top-left is chosen because the existing images / mesh-attributes / texturing
files already imply it.
*Owners:* `points.types.plato` (UvCoordinate, UvwCoordinate); cited from
`meshes.types.plato`, `images.types.plato`, `texturing.types.plato`.

## Floating-point comparison — one epsilon policy

Numerical near-equality uses a single record, **`ComparisonTolerance`**
(`numbers.types.plato`) with `{ Absolute: Number, Relative: Number }`. The test is
`|x - y| <= Absolute + Relative * max(|x|, |y|)`. `AlmostEqual` overloads accept
it (`algebra.library.plato`, beside the scalar-tolerance `AlmostEqual`;
the component-wise vector lift is in `numeric-structures.library.plato`).

Engineering `Tolerance` (`uncertainty.types.plato`) is **explicitly NOT** this type:
its `Plus` / `Minus` are asymmetric **acceptance allowances** about a `Nominal`
manufacturing value, not comparison epsilons — do not stuff floating-point
epsilons into it (and use `UncertainNumber` for 1-sigma uncertainties).
*Owners:* `ComparisonTolerance` (`numbers.types.plato`), `AlmostEqual`
(`algebra.library.plato`), `Tolerance` (`uncertainty.types.plato`).
