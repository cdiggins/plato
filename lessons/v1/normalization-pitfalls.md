---
lesson: normalization-pitfalls
title: Normalization Pitfalls — Zero, Near-Zero, and Direction Types
domain: Foundations & vectors
v3-files: [08-vectors.plato]
audience: Comfortable with basic vectors; knows that a vector has length and direction
status: draft-v1
---

# Normalization Pitfalls — Zero, Near-Zero, and Direction Types

You aim a spotlight at a target. The direction from the lamp to the target is the
vector `target - lamp`, scaled to unit length. That scaling — **normalization** — is
one of the most common operations in geometry code. It also fails in the most boring
way imaginable: when the lamp and the target are the same point, the direction vector
is zero, division by its length is undefined, and every downstream calculation that
assumed a sensible direction starts producing infinities or NaNs.

The failure is not exotic. It appears whenever two positions coincide, a velocity
drops to rest, a mesh triangle collapses to a line, a user drags a handle onto itself,
or floating-point subtraction cancels two large nearly-equal coordinates. Normalization
looks like a one-liner; in production it is a policy decision.

## What normalization is

A **vector** is a displacement: it has a direction and a length (magnitude), but no
fixed location in space. In Plato, geometric displacements in 2D and 3D are
`Vector2D` and `Vector3D`. Both implement the `Vector` concept, which in turn
implements `Normed`:

```
concept Normed
{
    Magnitude(x: Self): Number;
    MagnitudeSquared(x: Self): Number;
}
```

For a vector **v** with components $(v_x, v_y, v_z)$, the magnitude is

$$\|v\| = \sqrt{v_x^2 + v_y^2 + v_z^2}$$

and **normalization** (when it succeeds) produces the **unit vector**

$$\hat{v} = \frac{v}{\|v\|}$$

with $\|\hat{v}\| = 1$ and the same direction as $v$ (except when $v = 0$).

The `Vector` concept library defines normalization as scaling by the reciprocal of
magnitude:

```
// The unit vector in the same direction. Precondition: non-zero magnitude.
Normalize(self: Vector): Vector
    => self / self.Magnitude;
```

That precondition is doing real work. There is no magic in the formula — if
`Magnitude` is zero, you are dividing by zero.

### Why squared magnitude matters

Comparing lengths or testing "is this vector tiny?" often does not need a square root.
`MagnitudeSquared` is cheaper and behaves well in comparisons:

```
// True when x has (near) zero length, within the given squared-magnitude tolerance.
IsZeroLength(x: Normed, tolerance: Number): Boolean
    => x.MagnitudeSquared <= tolerance;
```

Use `MagnitudeSquared` when you only need ordering or a zero test. Reserve `Magnitude`
for when you genuinely need the length in world units — normalization, projecting a
force onto a distance, converting speed to velocity.

## The zero vector

The **zero vector** is the additive identity: every component is zero. For any
`Vector2D` or `Vector3D` that implements `Numerical`, the concept library defines

```
IsZero(x: Numerical): Boolean
    => x == x.Zero;
```

Geometrically, the zero vector has no direction. Asking for "the direction of zero"
is like asking for the slope of a flat line at a single point — the question does not
have an answer.

Common situations that produce zero accidentally:

```
// Camera at P, look target also at P
var offset = target.Between(camera);   // Point3D.Between => Vector3D, b - a
var forward = offset.Normalize;          // PRECONDITION VIOLATION if offset is zero

// Segment collapsed to a point
var edge = segment.B.Between(segment.A);
var sideways = edge.Normalize;           // zero when A == B (see LineSegment3D doc)

// Catastrophic cancellation in float arithmetic
var a = Vector3D(1.0e8, 0.0, 0.0);
var b = Vector3D(1.0e8 + 1.0e-8, 1.0e-8, 0.0);
var diff = b - a;                        // may be far smaller than either operand
```

The last case is **near-zero**, not necessarily exactly zero. Exact equality tests
are brittle; length tests with tolerance are not.

## Near-zero vectors

Floating-point numbers are approximations. Subtraction, dot products, and cross
products routinely yield vectors that are *almost* zero but not exactly `Zero`.

Two complementary tools appear on `Normed` values in the core algebra library:

```
IsZeroLength(x: Normed, tolerance: Number): Boolean
    => x.MagnitudeSquared <= tolerance;

IsUnit(x: Normed, tolerance: Number): Boolean
    => x.MagnitudeSquared.AlmostEqual(1.0, tolerance);
```

Here `tolerance` is compared against **squared magnitude**, not length directly. That
matches `IsZeroLength`: if you want to treat vectors shorter than $\varepsilon$ as
zero, pass $\varepsilon^2$ (or think in squared units from the start).

Choosing $\varepsilon$ is domain-specific: world-space meters might use $10^{-6}$ as
noise floor; NDC coordinates need tighter thresholds; pixel-space compares to half a
pixel squared; accumulated simulation error may require widening tolerance over time.

A vector with magnitude $10^{-40}$ is not mathematically zero, but normalizing it
amplifies noise by $\sim 10^{40}$. The result is technically unit length and
technically useless. **Near-zero is often worse than exactly zero** because code
may skip the explicit zero check and still explode.

Defensive patterns (expressed in Plato-shaped pseudocode; v3 does not yet declare
all of these helpers on `Vector`):

```
// Guard before normalize — preferred when zero is expected
if (!v.IsZeroLength(1.0e-12))
    return v.Normalize;
else
    return fallbackDirection;

// Avoid normalizing when you only need a comparison
if (a.DistanceSquared(b) < threshold)
    // treat as coincident — don't normalize (a - b)
```

Other `Vector` operations carry the same non-zero precondition, for the same reason:

```
ScalarProjection(self: Vector, onto: Vector): Number
    => self.Dot(onto) / onto.Magnitude;          // onto must be non-zero

ProjectOnto(self: Vector, onto: Vector): Vector
    => onto * (self.Dot(onto) / onto.Dot(onto)); // onto must be non-zero

CosineSimilarity(a: Vector, b: Vector): Number
    => a.Dot(b) / (a.Magnitude * b.Magnitude);  // a and b must be non-zero
```

If any operand might vanish, test length first or restructure the algorithm.

## Direction types — normalized by construction

Plato separates **displacement** from **direction** at the type level:

```
// A 2D direction: a Vector2D with the invariant that its magnitude is one.
type Direction2D
    implements Value
{
    Vector: Vector2D;
}

// A 3D direction: a Vector3D with the invariant that its magnitude is one.
type Direction3D
    implements Value
{
    Vector: Vector3D;
}
```

`Vector2D` and `Vector3D` are general displacements: they can represent velocity,
offsets, sums of forces, unscaled normals from cross products, anything with a length.
`Direction2D` and `Direction3D` promise something stronger: whoever built the value
already ensured unit length.

That promise shows up wherever only direction matters, not speed or distance:

```
type Ray3D
{
    Origin: Point3D;
    Direction: Direction3D;    // not Vector3D — a ray's direction must be unit
}

type Plane
{
    Normal: Direction3D;        // Hesse normal form assumes a unit normal
    Distance: Number;
}

type Frame3D
{
    Origin: Point3D;
    XAxis: Direction3D;
    YAxis: Direction3D;
    ZAxis: Direction3D;         // orthonormal axes
}

type AxisAngle
{
    Axis: Direction3D;
    Angle: Angle;
}
```

Using `Direction3D` on a `Ray3D` documents intent: parameter $t$ in
`Origin + Direction * t` is a true distance along the ray because `Direction` is
unit length. If `Direction` were a bare `Vector3D`, callers would not know whether
$t$ is in world units or scaled by an unknown factor.

### Building and maintaining directions

The honest way to obtain a direction from a displacement is normalize-then-wrap:

```
var delta = target.Between(camera);
var dir = Direction3D(delta.Normalize);   // PRECONDITION: delta non-zero
```

Library code in `Transforms` rotates directions without re-normalizing when the
transform is rigid:

```
Transform(d: Direction3D, q: Quaternion): Direction3D
    => Direction3D(d.Vector.Transform(q));
```

Rotation preserves length, so a unit input stays unit. **Uniform scale** does not.
Non-uniform scale or shear can turn a unit vector into a non-unit vector — another
reason `Direction3D` stores a `Vector` field rather than pretending the type is
immune to all operations.

### What Direction types do *not* fix

Wrapping does not automatically validate the invariant. This is still ill-formed:

```
var sloppy = Direction3D(Vector3D(3.0, 0.0, 0.0));  // magnitude 3, not 1
```

The type system records intent; it does not run a unit-length check on every
construction unless the library provides one. Today, validating constructors are a
gap (see recommendations).

Similarly, **lerping** two directions as vectors and normalizing the result is a
common hack for "almost facing" blends, but it is not uniform on the sphere and
speeds vary with the angle between endpoints. Interpolation of orientations belongs
on rotation types (`Quaternion`, `Rotation2D`), not on raw vector lerp.

## In Plato — putting it together

Relevant surface from `08-vectors.plato`:

```
concept Vector
    inherits Numerical, Arithmetic, Indexable<Number>, Normed, Lattice, Hashable
{
    Dot(a: Self, b: Self): Number;
}

type Vector2D implements Vector { X: Number; Y: Number; }
type Vector3D implements Vector { X: Number; Y: Number; Z: Number; }

type Direction2D implements Value { Vector: Vector2D; }
type Direction3D implements Value { Vector: Vector3D; }
```

Derived behavior (concept libraries under `stdlib/concept-library/`) that
every lesson about normalization eventually touches:

| Operation | Where | Precondition / note |
|-----------|-------|---------------------|
| `v.Normalize` | `NumericStructures` on `Vector` | `v` non-zero |
| `v.Magnitude`, `v.MagnitudeSquared` | `Normed` | always defined |
| `v.IsZeroLength(tol)` | `CoreAlgebra` on `Normed` | `tol` is squared-length |
| `v.IsUnit(tol)` | `CoreAlgebra` on `Normed` | checks $\|v\|^2 \approx 1$ |
| `a.Between(b)` on points | `Difference<Vector3D>` | can be zero |
| `Direction3D(v)` | tuple constructor | no validation yet |

A minimal "look along offset" pipeline, with the guard explicit:

```
function LookDirection(from: Point3D, to: Point3D, fallback: Direction3D): Direction3D
{
    var delta = to.Between(from);
    return delta.IsZeroLength(1.0e-16)
        ? fallback
        : Direction3D(delta.Normalize);
}
```

Compare with the unsafe version — fine in a demo, wrong in a tool:

```
// Unsafe: identical points, rest velocity, degenerate geometry
var dir = Direction3D(to.Between(from).Normalize);
```

## Pitfalls and fine print

**1. Normalizing twice.** If `v` is already unit length, `v.Normalize` is a no-op
in real arithmetic. In floats, it is a needless extra square root and division that
can nudge components off exact unity. If you know you have a `Direction3D`, use
`d.Vector` when a displacement is required; do not normalize again without reason.

**2. Confusing zero vector with zero scalar.** `Vector3D(0,0,0)` is a valid value
and the additive identity. It is not "falsy". Test with `IsZero` or `IsZeroLength`,
not with ad hoc component checks unless you have a reason.

**3. Using normalize where projection suffices.** If you only need "how much of `a`
lies along `b`?", use `ScalarProjection` or `ProjectOnto`. Normalizing both sides
first throws away magnitude information you may still need.

**4. Plane and line normals from cross products.** For a triangle with edges `e1`,
`e2`, the normal direction is `e1` crossed with `e2` (declared in later geometry
files). That cross product can be zero for degenerate triangles. Normalize **after**
confirming the cross product is not near-zero, not before.

**5. Equality vs. geometric sameness.** Two unit vectors with opposite sign point
different ways even if they are collinear negatives. `Direction3D` does not by itself
resolve "same line, opposite ray" — ray and line types encode which end matters.

**6. Integer grids are a different story.** `IntegerVector2` and `IntegerVector3`
are grid steps and pixel offsets; they do not implement `Vector` or carry a
normalization story. Do not call `Normalize` on them — the concept does not apply.

**7. Documented degeneracy elsewhere.** `LineSegment3D` notes it is degenerate when
`A` equals `B`. Any code that turns a segment into a direction via `B.Between(A)`
must handle that case before normalization.

## Try it

<details>
<summary>Exercise 1 — Predict the problem</summary>

`Point3D(1, 2, 3).Between(Point3D(1, 2, 3))` yields `Vector3D(0, 0, 0)`.
Calling `.Normalize` on it violates the stated precondition. What should the caller
return instead if this is a camera forward vector?

**Answer:** A chosen fallback direction (world +Z, previous frame forward, user-picked
reference) — anything valid except normalization. The correct behavior is policy, not
math.

</details>

<details>
<summary>Exercise 2 — Squared tolerance</summary>

You want to reject vectors shorter than $10^{-5}$ world units. Should you pass
`1.0e-5` or `1.0e-10` to `IsZeroLength`?

**Answer:** `1.0e-10`, because `IsZeroLength` compares against **squared** magnitude.
$(10^{-5})^2 = 10^{-10}$.

</details>

<details>
<summary>Exercise 3 — Type choice</summary>

You store a bullet's velocity as `Vector3D` and its facing for rendering as
`Direction3D`. After a collision, speed drops to zero but you still need a facing.
Which value broke, and which still makes sense?

**Answer:** Velocity becomes zero — normalization of velocity is meaningless. Facing
can remain the last valid `Direction3D` (or spin in place using angular state).
Speed and heading decouple precisely so this case is representable.

</details>

## Library recommendations

- **missing-function** — `08-vectors.plato` / `Vector`: add `TryNormalize(self: Vector, fallback: Vector): Vector` or `NormalizeOr(self: Vector, fallback: Vector): Vector` beside the existing preconditioned `Normalize`. Teaching this lesson makes clear that every call site repeats the same `IsZeroLength` guard; the concept library already has `SafeDivide` on `Real` for the analogous scalar case.

- **missing-function** — `08-vectors.plato` / `Direction2D`, `Direction3D`: declare validated factories, e.g. `FromVector(v: Vector2D): Direction2D` (precondition: non-zero) and `TryFromVector(v: Vector2D, fallback: Direction2D): Direction2D`, plus `FromVectorUnchecked` explicitly documented as unsafe. Today tuple construction `Direction3D(v)` appears in `13-transforms.plato` but nothing in `08-vectors.plato` states when wrapping is legal or enforces `IsUnit`.

- **doc-comment** — `08-vectors.plato` / `Direction2D`, `Direction3D`: the invariant comment should warn that arbitrary `Vector` values may violate unit length after non-rigid transforms or manual construction, and point callers at `IsUnit` for diagnostics. The type promises intent, not runtime proof.

- **pedagogy** — `concept-library/06-numeric-structures.library.plato` / `Normalize`: pair `IsZeroLength` guidance with `Normalize` in doc comments, or add a guarded helper, so callers do not rediscover near-zero policy independently.
