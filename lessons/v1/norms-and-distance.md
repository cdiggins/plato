---
lesson: norms-and-distance
title: Norms, Distance, and Normalization
domain: Foundations & vectors
v3-files: [02-concepts-algebra.plato, 08-vectors.plato]
audience: High-school algebra and basic programming; no prior 3D graphics required
status: draft-v1
---

# Norms, Distance, and Normalization

You have two markers on a map and want to know which one is closer to a treasure.
You have a velocity vector and need a *direction* for steering, not a speed.
You are culling ten thousand objects and only care whether one is nearer than another —
not the exact mile count.

All three problems share the same machinery: **length** (how big a displacement is),
**distance** (how far apart two things are), and **normalization** (scaling to unit
length while keeping direction). The Euclidean formulas look simple in 2D and 3D, but
the engineering choices around them — especially when to take a square root and when
not to — show up everywhere in geometry code.

## Length: the norm of a displacement

A **vector** is a displacement: "go 3 east, 4 north" is not a place on the map, it is
a step you could take from anywhere. Its **length** (also **magnitude** or **norm**)
answers: how far would that step carry you?

For a 2D displacement $(x, y)$ the Euclidean length is

$$
\| (x, y) \| = \sqrt{x^2 + y^2}.
$$

In 3D, with components $(x, y, z)$:

$$
\| (x, y, z) \| = \sqrt{x^2 + y^2 + z^2}.
$$

The pattern generalizes: square each component, add, square root. Geometrically this is
the straight-line distance from the origin to the tip of the arrow.

```
        tip (3, 4)
          *
         /|
        / |
   4   /  |
      /   |
     /    |
    *-----*
   origin   3

  length = sqrt(3² + 4²) = 5
```

Some useful identities:

- **Zero vector:** $\|(0, 0)\| = 0$. The displacement "go nowhere" has no length.
- **Scaling:** $\|s \cdot \mathbf{v}\| = |s| \cdot \|\mathbf{v}\|$. Doubling every
  component doubles the length.
- **Triangle inequality:** $\|\mathbf{a} + \mathbf{b}\| \le \|\mathbf{a}\| + \|\mathbf{b}\|$.
  The shortcut is never longer than going around the corner.

Length is always a non-negative **Number** — a plain scalar with no direction attached.

## Distance: length of the difference

**Distance** between two values is the length of the displacement from one to the other.

For two vectors $\mathbf{a}$ and $\mathbf{b}$ in the same space:

$$
d(\mathbf{a}, \mathbf{b}) = \| \mathbf{b} - \mathbf{a} \|
$$

Subtract first, then take length. Order matters for the displacement ($\mathbf{b} - \mathbf{a}$
points from $\mathbf{a}$ toward $\mathbf{b}$), but distance is symmetric:
$d(\mathbf{a}, \mathbf{b}) = d(\mathbf{b}, \mathbf{a})$ because negating a vector does not
change its length.

For **positions** (points), you cannot add two positions and get another position — but
you *can* subtract them and get a displacement. The distance between point $A$ and point $B$
is the length of the displacement from $A$ to $B$.

This "subtract, then measure length" pattern is the concrete meaning behind the abstract
**MetricSpace** concept: a type knows how to report `Distance(a, b)` as a non-negative scalar,
with distance from a value to itself equal to zero.

## Normalization: keeping direction, fixing length

A **unit vector** has length exactly 1. It encodes pure direction — "east" rather than
"300 km east."

**Normalization** divides a non-zero vector by its own length:

$$
\hat{\mathbf{v}} = \frac{\mathbf{v}}{\|\mathbf{v}\|}.
$$

The result points the same way but has unit length. Normalization is how you turn a speed
vector into a heading, a surface normal into a lighting direction, or an offset into a
pure compass bearing.

Precondition: $\|\mathbf{v}\| \neq 0$. Normalizing the zero vector requires dividing by
zero — undefined, and a common source of NaNs in real code.

When a value must *always* be a unit direction, it helps to store it in a type that carries
that invariant by construction rather than hoping every caller remembered to normalize.

## Squared distance: skip the square root

Define **squared magnitude**:

$$
\|\mathbf{v}\|^2 = x^2 + y^2 + z^2
$$

and **squared distance**:

$$
d^2(\mathbf{a}, \mathbf{b}) = \|\mathbf{b} - \mathbf{a}\|^2.
$$

Because square root is strictly increasing for non-negative inputs:

$$
d(\mathbf{a}, \mathbf{b}) < d(\mathbf{a}, \mathbf{c})
\quad\Longleftrightarrow\quad
d^2(\mathbf{a}, \mathbf{b}) < d^2(\mathbf{a}, \mathbf{c}).
$$

Any comparison "which is closer?" gives the same answer with squared distance — and
squared distance avoids a square root per comparison. In a loop that tests thousands of
candidates, that saving matters.

Use **squared distance** when:

- picking the nearest of many candidates;
- testing "within radius $r$" (compare $d^2 \le r^2$ instead of $d \le r$);
- checking whether a vector is unit length (compare $\|\mathbf{v}\|^2 \approx 1$).

Use **actual distance** when:

- the numeric value is shown to a human ("2.3 m away");
- the distance feeds another formula that genuinely needs $d$, not $d^2$;
- you accumulate many small distances and need the linear measure.

## In Plato

Plato separates the *declaration* of capabilities (concepts) from concrete vector types.
Two concepts from the algebra layer capture the ideas above.

### Normed — "this value has a length"

```plato
// Has a notion of length or magnitude. MagnitudeSquared avoids the square root
// when only relative comparisons are needed.
concept Normed
{
    Magnitude(x: Self): Number;
    MagnitudeSquared(x: Self): Number;
}
```

`Magnitude` is the length. `MagnitudeSquared` is the cheaper sibling — same ordering for
comparisons, no square root.

The **Vector** concept inherits **Normed** (among other structures):

```plato
concept Vector
    inherits Numerical, Arithmetic, Indexable<Number>, Normed, Lattice, Hashable
{
    Dot(a: Self, b: Self): Number;
}
```

Concrete types in the vector file implement **Vector**:

| Type | Role |
|------|------|
| `Number2`, `Number3`, `Number4`, `Number8` | Low-level numeric tuples; component-wise arithmetic |
| `Vector2D`, `Vector3D` | Geometric displacements in 2D/3D space |
| `VectorN` | Runtime-sized displacement |

Naming rule: a bare number counts components (`Number3` = three numbers); a `D` suffix
means the type lives in that-dimensional *space* (`Vector3D` = displacement in 3D).

### MetricSpace — "this type has a distance"

```plato
// A space with a distance between any two of its values.
concept MetricSpace
{
    Distance(a: Self, b: Self): Number;
}
```

**Normed** answers "how long is *this one* value?" **MetricSpace** answers "how far apart
are *these two*?" For vectors the standard Euclidean distance is $\|b - a\|$; the concept
does not prescribe the formula, only that `Distance` behaves like a metric.

### Usage-shaped examples

**Length of a 3D displacement:**

```plato
var v = Vector3D(3.0, 4.0, 0.0);
var len = v.Magnitude;           // 5.0
var lenSq = v.MagnitudeSquared;  // 25.0
```

**Distance between two displacements** (concept-library derives this from subtraction and
`Magnitude`):

```plato
var a = Vector3D(1.0, 0.0, 0.0);
var b = Vector3D(4.0, 0.0, 0.0);
var d = a.Distance(b);            // 3.0
var dSq = a.DistanceSquared(b);   // 9.0 — same nearest-neighbor ordering, no Sqrt
```

**Nearest-neighbor test without square roots:**

```plato
var origin = Vector2D(0.0, 0.0);
var p = Vector2D(3.0, 4.0);
var q = Vector2D(5.0, 1.0);
origin.IsNearerThan(p, q)         // true — p is closer to origin than q is
```

**Normalization** (intrinsic on concrete geometric vectors; concept-library also defines
`Normalize` for any `Vector`):

```plato
var v = Vector3D(3.0, 4.0, 0.0);
var u = v.Normalize;              // Vector3D(0.6, 0.8, 0.0) — unit vector, same direction
```

**Unit-length by construction** — when direction must stay normalized:

```plato
type Direction3D
    implements Value
{
    Vector: Vector3D;   // invariant: Magnitude is 1
}
```

Store a `Direction3D` when the value *is* a heading; store a `Vector3D` when length carries
information (velocity, force, offset).

**Distance between positions** — points are not vectors, but subtracting two points yields
a displacement whose length is the distance:

```plato
var treasure = Point2D(10.0, 5.0);
var markerA = Point2D(7.0, 1.0);
var markerB = Point2D(12.0, 6.0);

var distA = treasure.Between(markerA).Magnitude;
var distB = treasure.Between(markerB).Magnitude;
// compare distA vs distB, or compare squared magnitudes to avoid Sqrt
```

**Within a radius** — compare squared values:

```plato
var center = Vector2D(0.0, 0.0);
var probe = Vector2D(3.0, 4.0);
center.WithinDistance(probe, 5.0)   // true — exactly on the circle of radius 5
center.WithinDistance(probe, 4.9)   // false — uses DistanceSquared internally
```

### How the pieces fit

```
  Vector3D  ──implements──▶  Vector  ──inherits──▶  Normed
       │                         │
       │                         └── Dot, Add, Subtract, Scalable ...
       │
       └── Normalize (intrinsic) — unit vector, same direction

  Point3D  ──Between(other)──▶  Vector3D  ──Magnitude──▶  distance as Number

  MetricSpace.Distance  — generic "far apart?" for any implementing type
  CoreAlgebra.IsNear / IsNearerThan / Nearest — comparison helpers on MetricSpace
```

## Pitfalls and fine print

**The zero vector.** `Magnitude` of zero is 0. `Normalize` divides by that magnitude —
undefined. Guard with `IsZeroLength` (compares `MagnitudeSquared` against a tolerance) before
normalizing, or use `Direction2D`/`Direction3D` when the type system should enforce unit length.

**Near-zero, not exactly zero.** Floating-point arithmetic leaves vectors with tiny but
non-zero length after long chains of operations. A threshold on **squared** magnitude
(`MagnitudeSquared <= tolerance`) is cheaper and numerically well-behaved than comparing
`Magnitude` directly.

**Normalization is not free.** It costs a length (square root) plus a division. In hot
loops — ray directions, gradient descent steps, particle headings — ask whether you need a
unit vector at all. Often `Dot` products, squared distances, or comparing `MagnitudeSquared`
to a threshold suffice.

**Do not confuse displacement length with coordinate size.** `Vector3D(1, 1, 1)` has
length $\sqrt{3}$, not 3. Component values and vector length are different questions.

**Metric vs convenience distance.** `Distance` on `Vector` means Euclidean distance between
two displacements treated as positions-from-origin. Distance between *points* goes through
`Between` first. Mixing the two shapes (comparing a point to a vector directly) is a type
error Plato prevents.

**Integer grid steps are different.** `IntegerVector2` and `IntegerVector3` store whole-number
components for pixel offsets and voxel steps; they do not implement **Vector** or **Normed**.
Grid distance (Manhattan, Chebyshev) is a separate story — the Euclidean **Vector** family
is for continuous space.

## Try it

Work these by hand or with a calculator. Answers are in the block below.

1. **Length.** What is `Vector2D(5.0, 12.0).Magnitude`?

2. **Squared vs unsquared.** Points $A = (0, 0)$, $B = (3, 4)$, $C = (6, 0)$. Which is
   nearer to $A$? Confirm using both `Magnitude` and `MagnitudeSquared` of the displacement
   vectors — do you get the same winner?

3. **Normalize.** `Vector3D(0.0, 0.0, 9.0).Normalize` — what are the components? What is
   `Magnitude` of the result?

4. **Within radius.** Is `Vector2D(3.0, 4.0)` within distance 5.0 of the origin? Within
   4.999?

<details>
<summary>Answers</summary>

1. $\sqrt{5^2 + 12^2} = \sqrt{169} = 13.0$.

2. $B$ is nearer. $\|B - A\| = 5$, $\|C - A\| = 6$. Squared: 25 vs 36 — same winner without
   a square root.

3. Components $(0, 0, 1)$. Magnitude is 1.0 — a unit vector along +Z.

4. Distance from origin is exactly 5.0, so `WithinDistance(..., 5.0)` is **true** (inclusive
   `<=`). With radius 4.999, **false**.

</details>

## Library recommendations

- **missing-function** — `02-concepts-algebra.plato`: **Normed** declares `Magnitude` and
  `MagnitudeSquared` but not `Normalize`. Normalization is the third leg of the lesson triad
  (length, distance, normalize); it currently lives only on concrete types via intrinsics
  (`70-intrinsics.plato`) and as a derived helper on `Vector` in concept-library. Adding
  `Normalize(x: Self): Self` to **Normed** (with a documented zero-length precondition)
  would make the concept self-contained and let `Direction2D`/`Direction3D` factories read
  as `Normed`-preserving operations.

- **missing-concept** — `08-vectors.plato`: `Vector2D`/`Vector3D` implement **Normed** but
  not **MetricSpace**, even though Euclidean vector distance is standard. **`Point2D`/`Point3D`**
  (file 11) likewise lack **MetricSpace** despite being the primary "how far apart are two
  positions?" types. Implementing **MetricSpace** on geometric points and vectors — with
  `Distance(a, b) => a.Between(b).Magnitude` for points and `(a - b).Magnitude` for vectors —
  would let `IsNear`, `IsNearerThan`, and `Nearest` from CoreAlgebra apply directly without
  the manual `Between(...).Magnitude` chain.

- **missing-function** — `02-concepts-algebra.plato`: **MetricSpace** exposes only `Distance`,
  not `DistanceSquared`. The **Normed** doc comment already motivates squared magnitude for
  comparisons; the metric counterpart (`DistanceSquared(a, b)`) appears in concept-library on
  **Vector** but not on the concept. Declaring it on **MetricSpace** (defaulting to
  `Distance(a, b).Square` or, for Euclidean types, `Between`/`Subtract` then `MagnitudeSquared`)
  would make radius and nearest-neighbor tests discoverable at the concept level.

- **doc-comment** — `08-vectors.plato`: **Direction2D** and **Direction3D** doc comments
  state the unit-length invariant but do not show how to construct one from a `Vector2D`/`Vector3D`
  safely. A one-line note ("construct via normalization of a non-zero displacement; zero input
  is undefined") would close the loop between normalization pitfalls and the direction types.

- **naming** — `08-vectors.plato` vs `02-concepts-algebra.plato`: **Difference**.`Between(a, b)`
  (displacement from `a` to `b`) shares the name `Between` with **Orderable** interval membership
  (`Between(x, lower, upper)`). Teaching distance between points forces both names into one
  lesson. Consider renaming one operation (e.g. `DisplacementTo` on **Difference**, or
  `InRange` on **Orderable**) to reduce overload confusion in pedagogical material and in
  API search.

> Resolved 2026-07-28: `Normalize` already exists generically on `Vector` (numeric-structures.library.plato) + as Vector2D/3D intrinsics; added concrete `Distance`/`DistanceSquared` for Point2D/3D/N (vectors already had them). Concept-level `Normalize` on Normed / `DistanceSquared` on MetricSpace were NOT added: algebra.concepts.plato is out of this packet's file scope, MetricSpace has no implementors, and Normalize needs Scalable which Normed lacks (items 256/257/258/259, stdlib commit pending).
