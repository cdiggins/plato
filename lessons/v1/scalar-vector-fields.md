---
lesson: scalar-vector-fields
title: Scalar and Vector Fields
domain: Fields, implicits & noise
v3-files: [26-fields.plato]
audience: High-school math and general programming background; basic derivatives helpful but not required
status: draft-v1
---

# Scalar and Vector Fields

A temperature map, a heightfield, a wind forecast, and a signed distance to a sculpture
are the same kind of object: a rule that assigns a value to every point of space. Once
you can evaluate that rule, you can contour it, advect particles through it, or take its
derivatives. Plato's v3 field layer makes that shared shape explicit — `Field` is just
`Procedural` over positions — then specializes it into scalar, vector, direction, color,
and tensor fields, with optional analytic derivatives and time.

## The idea

A **scalar field** $f: \mathbb{R}^n \to \mathbb{R}$ returns one number per point:
height, density, potential, temperature, signed distance.

A **vector field** $F: \mathbb{R}^n \to \mathbb{R}^n$ returns a vector per point:
velocity, force, displacement, gradient of a scalar.

```
  Scalar field f(x,y)                 Vector field F(x,y)
       3  4  5                           →  ↗  ↑
       2  3  4                           →  →  ↗
       1  2  3                           ↗  →  →
     (numbers on a grid)              (arrows on a grid)
```

Three derivative operators show up constantly:

**Gradient** $\nabla f$ — vector pointing toward steepest *increase* of a scalar. Magnitude
is the slope. For a signed distance field, $\nabla f$ (normalized) is the outward normal.

**Divergence** $\nabla \cdot F$ — scalar measuring net outflow. Positive = source,
negative = sink, zero = volume-preserving (incompressible) flow.

**Curl** $\nabla \times F$ — in 3D a vector measuring local rotation; in 2D a scalar
(rotation about the plane normal). Zero curl means locally irrotational.

The **Jacobian** of a vector field is the matrix of all first partials — the linear map
that best approximates how $F$ changes near a point. Divergence is its trace; curl is
built from its antisymmetric part.

Constant fields are the trivial case: the same value everywhere. They matter as defaults
and as leaves of expression graphs.

When fields are combined (add two heightmaps, threshold a mask, remap a range), you can
build an explicit expression DAG. Plato stores those DAGs as flat node arrays with
lower-index operands — same discipline as SDF trees.

## In Plato

Root concepts in `26-fields.plato`:

```plato
concept Field<TDomain, TValue>
    inherits Procedural<TDomain, TValue>
{ }

concept ScalarField3D
    inherits Field<Point3D, Number>
{ }

concept VectorField3D
    inherits Field<Point3D, Vector3D>
{ }

concept DirectionField3D
    inherits Field<Point3D, Direction3D>
{ }
```

Evaluation is `Eval` from `Procedural` — side-effect free and deterministic for a given
field value. Differentiable refinements add derivatives:

```plato
concept DifferentiableScalarField3D
    inherits ScalarField3D
{
    GradientAt(x: Self, point: Point3D): Vector3D;
}

concept DifferentiableVectorField3D
    inherits VectorField3D
{
    JacobianAt(x: Self, point: Point3D): Matrix3x3;
    DivergenceAt(x: Self, point: Point3D): Number;
    CurlAt(x: Self, point: Point3D): Vector3D;
}
```

In 2D, curl is a scalar:

```plato
concept DifferentiableVectorField2D
    inherits VectorField2D
{
    JacobianAt(x: Self, point: Point2D): Matrix2x2;
    DivergenceAt(x: Self, point: Point2D): Number;
    CurlAt(x: Self, point: Point2D): Number;
}
```

Time-varying fields take an `Instant` (not a raw `Number`):

```plato
concept TimeVaryingVectorField3D
{
    EvalAtTime(x: Self, point: Point3D, time: Instant): Vector3D;
}
```

Constants and graphs:

```plato
type ConstantScalarField3D
    implements Value, ScalarField3D
{
    Value: Number;
}

type ScalarFieldNode3D
    = Source(FieldIndex: ItemIndex)
    | Constant(Value: Number)
    | Add(Left: FieldNodeIndex, Right: FieldNodeIndex)
    | Subtract(...)
    | Multiply(...)
    | Divide(...)
    | Minimum(...)
    | Maximum(...)
    | Negate(Input: FieldNodeIndex)
    | AbsoluteValue(Input: FieldNodeIndex)
    | Lerp(A: FieldNodeIndex, B: FieldNodeIndex, Weight: FieldNodeIndex)
    | Clamp(Input: FieldNodeIndex, Low: FieldNodeIndex, High: FieldNodeIndex)
    | Remap(Input: FieldNodeIndex, FromLow: FieldNodeIndex, FromHigh: FieldNodeIndex)
    | Threshold(Input: FieldNodeIndex, Level: FieldNodeIndex);

type ScalarFieldGraph3D
{
    Nodes: Array<ScalarFieldNode3D>;
    Root: FieldNodeIndex;
}
```

Usage-shaped snippets:

```plato
temp = ConstantScalarField3D(20)
t = Eval(temp, p)                    // 20 everywhere

g = GradientAt(heightField, p)       // steepest ascent
len = Magnitude(g)                   // Normed; unit direction not yet a named Normalize

div = DivergenceAt(velocity, p)      // >0 expanding, <0 compressing
w = CurlAt(velocity, p)              // Vector3D vorticity in 3D
```

Building a soft mask from two sources via a graph (indices into an external field list
and into `Nodes`):

```plato
// nodes[0] = Source(0), nodes[1] = Source(1)
// nodes[2] = Maximum(0, 1)
// nodes[3] = Remap(2, lowNode, highNode)
// Root = 3
```

## Pitfalls / fine print

**Gradient direction.** $\nabla f$ points toward *increase*. For a heightfield used as
terrain, "downhill" is $-\nabla f$. For an SDF (negative inside), the outward normal is
$+\nabla f$ when the field increases toward the exterior — matching v3's doc comment on
`DifferentiableScalarField3D`.

**Not every field is differentiable.** `ScalarField3D` alone has no `GradientAt`. Numeric
finite differences are an implementation choice, not part of the concept. Prefer types
that implement `DifferentiableScalarField3D` when you need normals from an analytic field.

**Direction fields drop magnitude.** `DirectionField3D` stores unit directions. Wind *speed*
belongs in a `VectorField3D` or a parallel scalar field — not in the direction alone.

**Graph operand order.** Nodes may only reference lower-indexed nodes. Creating a cycle
or a forward reference makes the array an invalid topological order. `Root` need not be
the last node, but it must be reachable from lower operands correctly.

**Remap to unit range.** `Remap` maps $[FromLow, FromHigh]$ onto $[0, 1]$. It does not
clamp; values outside the from-interval extrapolate. Pair with `Clamp` when you need a
hard mask.

**Threshold is step-valued.** `Threshold` yields 1 where input ≥ level, else 0 — a
discontinuous field. Derivatives across the step are not meaningful.

**Time is `Instant`.** Passing a frame index as `Number` into `EvalAtTime` is a type
error by design. Convert through the time vocabulary (`07-time.plato`) rather than
smuggling integers.

**2D curl vs 3D curl.** Mixing them up in APIs is a classic bug. Plato's return types
enforce the distinction: `Number` in 2D, `Vector3D` in 3D.

## Try it

1. $f(x,y,z) = z$. What is $\nabla f$? What is the divergence of the constant field
   $F = (0,0,1)$?
2. Why can a non-zero curl field still have zero divergence everywhere?
3. In a `ScalarFieldGraph3D`, why must `Add`'s `Left` and `Right` indices be less than
   the `Add` node's own index?

<details>
<summary>Answers</summary>

1. $\nabla f = (0,0,1)$. Divergence of a constant field is 0 (all partials of components
   are zero).
2. Divergence measures expansion; curl measures rotation. A pure whirlpool can spin
   fluid without creating or destroying volume (classic example: $F = (-y, x, 0)$ in 3D
   has curl along $z$ and zero divergence).
3. The node array is required to be a valid topological order: operands are always
   lower-indexed, so evaluation can proceed in a single forward pass with no graph walk.

</details>

## Library recommendations

- **missing-concept** — `26-fields.plato`: scalar expression graphs exist
  (`ScalarFieldGraph2D/3D`) but there is no parallel `VectorFieldGraph3D` / node sum for
  composing vector fields (add flows, scale, project). Teaching advection pipelines hits
  this gap immediately after curl and divergence.

- **missing-function** — `26-fields.plato`: `ScalarFieldGraph3D` has no `Eval` /
  `implements ScalarField3D`. The graph is inert data until some undeclared interpreter
  exists. Declaring `ScalarFieldGraph3D implements ScalarField3D` (or a concept
  `FieldGraph`) would make graphs first-class fields like `ConstantScalarField3D`.

- **missing-function** — `26-fields.plato`: no `GradientField` wrapper that turns a
  `DifferentiableScalarField3D` into a `VectorField3D`. The lesson wants to say
  "the gradient *is* a vector field"; v3 only offers pointwise `GradientAt`, not a
  reified field value.

- **missing-function** — `08-vectors.plato` / `02-concepts-algebra.plato`: `Normed`
  declares `Magnitude` / `MagnitudeSquared` but there is no `Normalize` (or
  `Direction3D` factory from `Vector3D`). Gradient-as-normal teaching needs an explicit
  unitize step on the vocabulary surface.

- **pedagogy** — `26-fields.plato`: `TensorField2D/3D` and `ComplexField2D` are declared
  with no differentiable refinements and no constant/graph companions. They are hard to
  teach alongside the scalar/vector story until Jacobian-level operations or examples
  appear in doc comments.
