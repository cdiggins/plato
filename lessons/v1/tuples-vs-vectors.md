---
lesson: tuples-vs-vectors
title: Tuples vs Vectors
domain: Foundations & vectors
v3-files: [08-vectors.plato]
audience: General programming background; some 2D/3D graphics exposure helpful.
status: draft-v1
---

# Tuples vs Vectors

Three floats walk into an API: `(1, 2, 3)`. Are they a point? A color? A scale factor? A direction?
The machine cannot tell. Programmers invent names — `float3`, `vec3`, `Vector3` — and then overload
every operation onto that one type until adding two "positions" compiles and means nothing.

Plato's v3 vocabulary draws a hard line. A bare number in the type name counts **components**. A
`D` suffix means the value lives in that-dimensional **space**. There is no `Vector3`. The rule
has teeth: it keeps intrinsic SIMD tuples apart from geometric displacements.

## The idea

### Two families, one concept

Both families implement the `Vector` concept (component-wise arithmetic, norms, dot product). They
are not interchangeable semantically:

| Family | Examples | Meaning |
|--------|----------|---------|
| Numeric tuples | `Number2`, `Number3`, `Number4`, `Number8` | Fixed-arity bags of `Number`s; backend intrinsics |
| Geometric vectors | `Vector2D`, `Vector3D`, `VectorN` | Displacements / directions in a Euclidean space |

$Number3$ is "three scalars glued together." $Vector3D$ is "an arrow in 3D space." The first is
ideal for RGB-linear triples, scale factors, and homogeneous weights before you interpret them.
The second participates in geometry: offsets between points, velocities, normals (before
normalization to `Direction3D`).

```
Number3          Vector3D
┌─┬─┬─┐          ──→
│X│Y│Z│          displacement in space
└─┴─┴─┘
 "three numbers"   "an arrow"
```

### Why "no Vector3" matters

If the library shipped `Vector3`, every call site would argue whether it means `Number3` or
`Vector3D`. C# `System.Numerics.Vector3` and GLSL `vec3` blur that line on purpose for performance
and familiarity. Plato regenerates toward those backends **from** distinct source types: map
`Number3` to the intrinsic lane type; map `Vector3D` to a geometric wrapper (or the same storage
with a different API surface). Naming at the Plato layer preserves intent even when the ABI
collapses.

### When three numbers are just three numbers

Use `Number3` when:

- Components are parallel channels (scales per axis, RGB without color-management yet, Bernstein
  weights).
- You are writing backend-shaped code that must match `float3` / `Vector3` intrinsics.
- There is no metric geometry yet — no length-as-distance-in-space story required.

Use `Vector3D` when:

- Subtracting two positions would yield this value.
- You need `Dot`, `Cross`, `Normalize`, `Reflect` as spatial operations.
- The quantity should transform as a displacement under rotations (w=0 in homogeneous terms).

`Number4` similarly doubles as homogeneous coordinates and RGBA storage math — still not a
`Vector4D` (and fixed 4D geometric vectors were deliberately removed from v3).

### Directions are stricter still

```plato
type Direction3D
    implements Value
{
    Vector: Vector3D;   // invariant: magnitude is one
}
```

A direction is not a third float triple type with the same fields — it wraps a `Vector3D` that is
unit length by construction. That is another place "just use Vector3" would erase an invariant.

## In Plato

File banner from `08-vectors.plato` (normative naming rule):

> Naming rule: a bare number counts components (`Number3`, `Tuple3`, `IntegerVector3`); a `D`
> suffix means the type lives in that-dimensional space (`Vector3D`, `Point3D`, `Ray3D`).

### The `Vector` concept

```plato
concept Vector
    inherits Numerical, Arithmetic, Indexable<Number>, Normed, Lattice, Hashable
{
    Dot(a: Self, b: Self): Number;
}
```

Both tuples and geometric vectors share `Dot`, component indexing, and lattice min/max. Shared
algebra does not imply shared meaning.

### Numeric tuples

```plato
type Number2  { X: Number; Y: Number; }
type Number3  { X: Number; Y: Number; Z: Number; }
type Number4  { X: Number; Y: Number; Z: Number; W: Number; }
type Number8  { X0: Number; ... X7: Number; }
```

`Number8` exists for SIMD-width lane math — a clear signal these types track hardware tuples, not
"points in 8D artistic space."

### Geometric displacements

```plato
type Vector2D { X: Number; Y: Number; }
type Vector3D { X: Number; Y: Number; Z: Number; }

type VectorN
{
    Components: Array<Number>;  // runtime arity
}
```

### Integer cousins

```plato
type IntegerVector2 { X: Integer; Y: Integer; }  // pixel offsets, grid steps
type IntegerVector3 { X: Integer; Y: Integer; Z: Integer; }
type IntegerVector4 { X: Integer; Y: Integer; Z: Integer; W: Integer; }
```

Whole-number offsets for grids and pixels — still component-count naming (`IntegerVector3`), not
`IntegerVector3D`. The `D` suffix is reserved for continuous spatial geometry.

### Usage-shaped contrasts

```
scale = Number3(1.5, 1.0, 1.0)           // per-axis scale factors
delta = Vector3D(1.5, 0.0, 0.0)          // move 1.5 along +X

// Non-uniform scale of a displacement (intrinsic on Vector3D):
stretched = delta.Multiply(scale)        // Vector3D × Number3 → Vector3D

rgb = Number3(r, g, b)                   // channels before Color wrapping
offset = Vector3D(dx, dy, dz)            // spatial displacement
```

The multiply overload that takes `Number3` factors on a `Vector3D` is exactly the pattern: tuples
as **parameters**, geometric vectors as **spatial values**.

## Pitfalls / fine print

**Same fields, different type.** `Number3` and `Vector3D` both have `X,Y,Z`. Convert explicitly at
boundaries; silent reinterpretation is how bugs travel.

**Dot product meaning.** `Dot` on `Number3` is a bilinear form on channels; on `Vector3D` it is
the Euclidean geometric product. The formula matches; the story does not.

**Homogeneous confusion.** `Number4` can hold $(x,y,z,w)$ before perspective divide. That is still
not a geometric `Vector3D`. Apply the divide, then decide point vs displacement.

**Integer vs float grids.** Pixel steps are `IntegerVector2`; subpixel motion is `Vector2D`. Mixing
them without conversion drops fractional motion.

**`VectorN` is not a tuple.** Runtime arity means no fixed backend intrinsic; use it for true
N-dimensional math, not as a substitute for `Number3`.

**Inventing `Vector3`.** If an API feels like it needs `Vector3`, pick `Number3` or `Vector3D`
(or `Point3D` / `Direction3D`) — the discomfort is the type system doing its job.

## Try it

<details>
<summary>Exercise 1 — Classify</summary>

Which type fits best? (a) RGB channels before color management (b) camera look direction after
normalize (c) voxel step `(1,0,0)` (d) velocity of a rigid body.

**Answer.** (a) `Number3` (b) `Direction3D` (c) `IntegerVector3` (d) `Vector3D`.
</details>

<details>
<summary>Exercise 2 — Naming rule</summary>

Why is `IntegerVector3` not called `IntegerVector3D` under Plato's rule?

**Answer.** Bare `3` counts components; these are grid steps, not continuous 3D geometric
displacements. The `D` suffix is reserved for space-embedded geometry types.
</details>

<details>
<summary>Exercise 3 — API smell</summary>

A function `Translate(entity, v: Number3)` compiles everywhere and confuses everyone. What
signature communicates intent?

**Answer.** `Translate(entity, displacement: Vector3D)` — or take a `Point3D` destination. Keep
`Number3` for scales/channels.
</details>

## Library recommendations

- **missing-function** — `08-vectors.plato`: no explicit `Vector3D(Number3)` / `Number3(Vector3D)`
  conversion constructors. The lesson needs a named, visible cast at the semantic boundary;
  without it, hosts invent ad hoc reinterpret casts that erase the rule.

- **doc-comment** — `Number3`: say aloud that it is **not** a geometric vector and list primary
  roles (scales, homogeneous pre-geometry, channel triples). The file banner states the rule; the
  type doc should repeat it where grep lands.

- **naming** — `IntegerVector2/3/4` vs geometric naming: consider documenting a forbidden list
  (`Vector3`, `Vec3`, `float3` as Plato source names) in the file banner so codegen authors do not
  reintroduce them as aliases in Plato text.

- **wrong-shape** — `Direction3D` wraps `Vector3D` but `Number3` has no unit-channel counterpart.
  That asymmetry is fine; a doc note under `Direction3D` ("normalize geometric vectors, not
  arbitrary Number3 channel triples") would stop RGB-normalization antipatterns.

- **pedagogy** — `Vector` concept name collides with everyday "vector" meaning `Vector3D`. A remark
  on the concept — "algebraic vector family; prefer concrete Vector3D/Number3 at APIs" — would
  reduce over-abstract call sites that accept any `Vector` and accidentally take `Number8`.
