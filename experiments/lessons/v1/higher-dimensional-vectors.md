---
lesson: higher-dimensional-vectors
title: Higher-Dimensional Vectors and PointN
domain: Foundations & N-D geometry
v3-files: [08-vectors.plato, 69-higher-dimensions.plato]
audience: Comfortable with Vector3D and arrays; curious about N-D math and when not to invent Vector4D.
status: draft-v1
---

# Higher-Dimensional Vectors and PointN

Two dimensions and three cover most interactive geometry. Sometimes you need **N**
components decided at runtime: a feature vector, a configuration space, a simplex in six
dimensions, or a hyperplane constraint. Plato splits that world cleanly:

- Fixed small arities: `Number2/3/4/8`, `Vector2D/3D`
- Runtime arity: `VectorN`, `PointN`
- N-D structures: `NSphere`, `HyperplaneN`, `SimplexN`, `SubspaceN`

Fixed **4D geometric** types (Point4D, Rotor4D, …) were removed from v3 — practical 4D
work uses `Number4` / `Quaternion` numerically, or `VectorN` when arity varies. This
lesson is how to navigate that design without reinventing a `Vector4D`.

## Naming rule (read this first)

From the vocabulary README:

- A **bare number** counts components: `Number3`, `IntegerVector3`, `Tuple3`
- A **`D` suffix** means the type lives in that-dimensional **space**: `Vector3D`,
  `Point3D`, `Ray3D`

There is **no** `Vector2`, `Vector3`, or `Vector4`. Geometric displacements in the plane
and in space are `Vector2D` and `Vector3D`. Raw numeric tuples are `Number2/3/4/8`.

```
  Number4          Vector3D           VectorN
  (4 floats,       (displacement      (Array<Number>,
   SIMD / RGBA /    in 3-space)        arity = Count)
   homogeneous)
```

## In Plato — vector families

```plato
interface Vector
    inherits Numerical, Arithmetic, Indexable<Number>, Normed, Lattice, Hashable
{
    Dot(a: Self, b: Self): Number;
}

type Number4
    implements Vector
{
    X: Number;
    Y: Number;
    Z: Number;
    W: Number;
}

type Number8
    implements Vector
{
    X0: Number;
    X1: Number;
    X2: Number;
    X3: Number;
    X4: Number;
    X5: Number;
    X6: Number;
    X7: Number;
}

type Vector3D
    implements Vector
{
    X: Number;
    Y: Number;
    Z: Number;
}

type VectorN
    implements Vector
{
    Components: Array<Number>;
}
```

All implement `Vector`: component arithmetic, `Dot`, norms, indexing. Choose by **meaning
and arity**:

| Need | Type |
|------|------|
| Displacement in 3D space | `Vector3D` |
| RGBA or homogeneous 4-tuple | `Number4` |
| SIMD 8-wide lane math | `Number8` |
| Dimension known only at runtime | `VectorN` |

### Points track the same split

```plato
type Point3D
    implements Coordinate, Difference<Vector3D>, Hashable
{
    X: Number;
    Y: Number;
    Z: Number;
}

type PointN
    implements Coordinate, Difference<VectorN>
{
    Components: Array<Number>;
}
```

`Between(p, q)` on `PointN` yields `VectorN`. Arity of the two points must match —
mixing a 3-component and 5-component `PointN` is a logic error (library should check).

### Directions stop at 3D

```plato
type Direction2D implements Value { Vector: Vector2D; }
type Direction3D implements Value { Vector: Vector3D; }
```

There is no `DirectionN`. For unit vectors in N-D, store a `VectorN` and document the
unit invariant, or normalize at use sites.

## In Plato — N-D structures

`69-higher-dimensions.plato` builds on `PointN` / `VectorN`:

```plato
type NSphere
    implements Value
{
    Center: PointN;
    Radius: Number;
}

type HyperplaneN
    implements Value
{
    Normal: VectorN;
    Distance: Number;
}

type SimplexN
{
    Vertices: Array<PointN>;
}

type SubspaceN
{
    Origin: PointN;
    BasisVectors: Array<VectorN>;
}
```

| Type | Picture |
|------|---------|
| `NSphere` | Points at distance `Radius` from `Center` in $N$ dimensions ($N =$ arity of center) |
| `HyperplaneN` | $\{ p : \mathrm{dot}(n,p) = d \}$ with unit `Normal` |
| `SimplexN` | $K+1$ vertices → $K$-simplex (triangle, tetrahedron, …) |
| `SubspaceN` | Affine flat: origin plus orthonormal basis spanning the subspace |

```
  R³ (everyday)              R^N
  Sphere  ↔                 NSphere (N=3)
  Plane   ↔                 HyperplaneN (N=3)
  Triangle / Tetrahedron ↔  SimplexN (3 or 4 vertices)
```

### Escape-time fractals (numeric 2D / 4D)

The same file hosts parameter records that live naturally in complex and quaternion
number systems — not as geometric `Point4D`:

```plato
type JuliaSetParameters
    implements Value
{
    C: Complex;
    MaxIterations: Integer;
    EscapeRadius: Number;
}

type MandelbrotView
    implements Value
{
    Center: Complex;
    Zoom: Number;
    MaxIterations: Integer;
}

type QuaternionJuliaParameters
    implements Value
{
    C: Quaternion;
    MaxIterations: Integer;
    EscapeRadius: Number;
}
```

A quaternion Julia set is a 4D fractal usually *displayed* as a 3D slice. The state is
`Quaternion` arithmetic, not a missing `Point4D` mesh type.

### Worked example — configuration distance

A robot arm state as six joint offsets:

```
var a = PointN(Array(0.0, 0.1, -0.2, 0.0, 0.5, 0.0));
var b = PointN(Array(0.0, 0.2, -0.2, 0.1, 0.5, 0.0));
var d = Between(a, b);           // VectorN
var dist = Magnitude(d);         // Normed on Vector
```

An `NSphere` around `a` with radius `0.15` contains configurations within that joint-space
distance — same API shape as a 3D sphere, different dimension.

### When to prefer Number4 over VectorN

- You always have exactly four components.
- You map to a backend SIMD/`float4` type.
- Field names `X,Y,Z,W` matter (homogeneous divide, plane equations in 3D).

Use `VectorN` when:

- Arity varies (5 today, 12 tomorrow).
- You write dimension-generic algorithms once.
- You build `SimplexN` / `SubspaceN` that must not hardcode 3.

## Pitfalls and fine print

**Inventing Vector4D.** Resist. For spatial 3D with homogeneous coords, `Number4` or
`HomogeneousPoint3D` is the vocabulary. For true N-D, `VectorN`.

**Arity mismatch.** `Dot` on two `VectorN` values requires equal `Count`. Fail loudly.

**Orthonormal claims.** `SubspaceN` docs say basis vectors are orthonormal — callers must
uphold that; the type does not prove it.

**Hyperplane normal.** Must be unit length; `Distance` is signed distance from the origin
in world units.

**Quaternion ≠ Point4D.** `Quaternion` is a rotation (or fractal state), with unit
invariants for orientations. Do not treat it as a position in $\mathbb{R}^4$ without a
clear numeric interpretation.

**Integer vectors.** `IntegerVector2/3/4` are grid steps, not substitutes for `VectorN`
of integers — there is no `IntegerVectorN` in the foundation file.

## Try it

<details>
<summary>Exercise 1 — Pick a type</summary>

You need a displacement between two positions in 3D CAD space. `Number3`, `Vector3D`, or
`VectorN`?

**Answer.** `Vector3D` — geometric displacement in 3-space. (`Number3` is a raw tuple;
`VectorN` works but throws away fixed-arity clarity.)
</details>

<details>
<summary>Exercise 2 — Sphere in 5D</summary>

`Center` has five components, `Radius = 2`. What type wraps this, and what is $N$?

**Answer.** `NSphere` with $N = 5$.
</details>

<details>
<summary>Exercise 3 — Simplex count</summary>

`SimplexN` with 4 vertices. What dimension $K$ is the simplex?

**Answer.** $K + 1 = 4$ → $K = 3$ (tetrahedron in whatever ambient $N$ the points live
in; ambient $N$ is the component count of each vertex).
</details>

## Library recommendations

- **missing-function** — `08-vectors.plato`: no declared `Arity`/`Count` alias on
  `VectorN` beyond `Indexable`/`Countable` inheritance details. A obvious
  `Dimension(v: VectorN): Integer` (or documented `Count`) helps N-D tutorials.

- **missing-function** — no `Normalize(VectorN): VectorN` or `DirectionN` type. Unit
  normals for `HyperplaneN` need a sanctioned construction path.

- **doc-comment** — `69-higher-dimensions.plato` already notes removal of fixed 4D
  geometry; `08-vectors.plato` should echo "use Number4/Quaternion/VectorN, not
  Vector4D" so readers of the vectors file alone get the policy.

- **pedagogy** — `SimplexN` and `SubspaceN` omit `implements Value` unlike neighbors.
  Either align them or document why — inconsistency distracts when teaching the N-D
  kit as one family.

> Resolved 2026-07-28: `Normalize(VectorN)` is covered by the generic `Normalize(Vector)` in numeric-structures.library.plato (VectorN implements Vector); `NormalizeOr` applies too. A dedicated `DirectionN` type was deferred (new type + Value obligations; VectorN's emitted runtime is still incomplete) — noted as a type gap (item 158 partial, stdlib commit pending).
