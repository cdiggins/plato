---
lesson: dot-product
title: The Dot Product
domain: Foundations & vectors
v3-files: [08-vectors.plato]
audience: High-school trig and basic vectors; programming background assumed.
status: draft-v1
---

# The Dot Product

You are holding a flashlight along direction $\mathbf{f}$. A surface faces you with unit normal
$\mathbf{n}$. Is the surface lit from the front, edge-on, or from behind? One number answers:

$$
\mathbf{f} \cdot \mathbf{n} = \|\mathbf{f}\|\,\|\mathbf{n}\|\,\cos\theta
$$

Positive means the directions agree (front), near zero means grazing, negative means the light is
behind the surface. The same algebraic gadget measures "how much of this motion is uphill,"
projects shadows of vectors onto axes, and tests whether two headphones are aimed the same way.

The **dot product** is the workhorse scalar of Euclidean geometry. Plato puts it on the shared
`Vector` interface so every numeric tuple and geometric displacement speaks it.

## The idea

### Definition two ways

**Component form** (Cartesian orthonormal basis):

$$
\mathbf{a} \cdot \mathbf{b} = a_x b_x + a_y b_y + a_z b_z
$$

**Geometric form:**

$$
\mathbf{a} \cdot \mathbf{b} = \|\mathbf{a}\|\,\|\mathbf{b}\|\,\cos\theta
$$

where $\theta$ is the angle between them. Equating both gives the usual cosine formula:

$$
\cos\theta = \frac{\mathbf{a}\cdot\mathbf{b}}{\|\mathbf{a}\|\,\|\mathbf{b}\|}
$$

### Projection

The scalar projection of $\mathbf{a}$ onto a **unit** direction $\hat{\mathbf{u}}$ is the single
number $\mathbf{a}\cdot\hat{\mathbf{u}}$ — signed length along that axis. The vector projection is

$$
\mathrm{proj}_{\hat{\mathbf{u}}}\mathbf{a} = (\mathbf{a}\cdot\hat{\mathbf{u}})\,\hat{\mathbf{u}}
$$

```
        a
       /
      / θ
     /•••••----→ u-hat
     proj = |a| cos θ
```

"How much of my velocity is along the track?" is a projection. "Reject" the rest (slide along the
wall) by subtracting the projection from $\mathbf{a}$ — orthogonal decomposition.

### Facing and hemispheres

For unit vectors $\hat{\mathbf{a}}$, $\hat{\mathbf{b}}$:

| Dot product | Meaning |
|-------------|---------|
| $1$ | Same direction |
| $0$ | Perpendicular |
| $-1$ | Opposite |
| $>0$ | Acute angle — same open hemisphere |
| $<0$ | Obtuse — facing away |

Back-face decisions, "is the listener in front of the speaker," and Lambert shading
($\max(\mathbf{n}\cdot\mathbf{l}, 0)$) all read this sign/magnitude story.

### Work and power (physics cameo)

If force $\mathbf{F}$ and displacement $\mathbf{d}$ are vectors, work is $W = \mathbf{F}\cdot\mathbf{d}$.
Only the component of force along the motion counts. Same math, different nouns.

### Squared lengths

$$
\mathbf{a}\cdot\mathbf{a} = \|\mathbf{a}\|^2
$$

Comparing squared distances avoids square roots: $\mathbf{d}\cdot\mathbf{d}$ vs $r^2$ for
sphere tests.

## In Plato

### On the interface

From `08-vectors.plato`:

```plato
interface Vector
    inherits Numerical, Arithmetic, Indexable<Number>, Normed, Lattice, Hashable
{
    Dot(a: Self, b: Self): Number;
}
```

`Normed` supplies magnitude:

```plato
interface Normed
{
    Magnitude(x: Self): Number;
    MagnitudeSquared(x: Self): Number;
}
```

Geometric types also expose intrinsic `Length` / `LengthSquared` / `Normalize` (same role, backend
naming). Prefer one spelling consistently at a call site.

### Geometric displacements

```plato
type Vector3D
    implements Vector
{
    X: Number;
    Y: Number;
    Z: Number;
}
```

Usage-shaped snippets:

```
view = Vector3D(0, 0, 1)
normal = Vector3D(0, 0.5, 0.5).Normalize

facing = view.Dot(normal)              // > 0: front-facing-ish
cosTheta = facing / (view.Length * normal.Length)  // if not unit

// unit-unit facing check
lit = normal.Dot(lightDir)             // lightDir: Direction3D.Vector or unit Vector3D
diffuse = Max(lit, 0)

alongTrack = velocity.Dot(trackDir)    // scalar projection if trackDir unit
speedSqr = velocity.Dot(velocity)      // == LengthSquared
```

`Direction3D` stores a unit `Vector`:

```
forward = Direction3D(Vector = Vector3D(0, 0, 1))
alignment = forward.Vector.Dot(other.Normalize)
```

### Numeric tuples

`Number2/3/4/8` also implement `Vector`, so `Dot` works on channel triples:

```
weights = Number3(0.2, 0.5, 0.3)
channels = Number3(r, g, b)
lumaish = weights.Dot(channels)        // weighted sum — not a geometric angle
```

Same function name; interpret results according to the type's meaning.

### 2D is included

```
v = Vector2D(1, 0)
w = Vector2D(0, 1)
v.Dot(w)                               // 0 — perpendicular in the plane
```

There is no 2D cross product returning a vector; the planar signed area uses a scalar analogue
(often $v_x w_y - v_y w_x$) living elsewhere. Dot still measures angles in 2D.

## Pitfalls / fine print

**Unnormalized inputs.** $\mathbf{a}\cdot\mathbf{b}$ alone is not $\cos\theta$ unless both are unit
(or you divide by lengths). Facing checks that forget normalize flip at unexpected thresholds.

**Degenerate zero vectors.** `Normalize` on a near-zero vector blows up; `Dot` with zero is zero and
tells you nothing about angle. Guard lengths before taking acos.

**`acos` instability.** Computing $\theta = \arccos(\mathrm{clamp}(\hat a\cdot\hat b))$ is okay for
display; for small angles prefer `atan2`-style formulas with cross magnitude for stability.
Clamping the dot into $[-1,1]$ before `acos` is mandatory — numerical drift exceeds the domain.

**Sign of projection.** Scalar projection is signed. "Distance along axis" may need `Abs` if only
magnitude matters.

**Using Dot as equality.** `a.Dot(b) == 1` for unit vectors fails under float noise. Use a tolerance
on $1 - \dot$ or on angle.

**Homogeneous / Number4.** Dotting homogeneous 4-tuples is rarely the Euclidean 3D angle you meant.
Strip to `Vector3D` first.

## Try it

<details>
<summary>Exercise 1 — Mental compute</summary>

$\mathbf{a}=(1,0,0)$, $\mathbf{b}=(0.5, 0.5, 0)$. What is $\mathbf{a}\cdot\mathbf{b}$? What is
$\theta$?

**Answer.** Dot $= 0.5$. $\|\mathbf{a}\|=1$, $\|\mathbf{b}\|=\sqrt{0.5}$, so
$\cos\theta = 0.5/\sqrt{0.5} = \sqrt{0.5}$, $\theta = 45°$.
</details>

<details>
<summary>Exercise 2 — Facing</summary>

Unit normal $\mathbf{n}=(0,1,0)$, view $\mathbf{v}=(0,-1,0)$. Is the camera looking at the front
of a surface with that outward normal (view toward the surface)?

**Answer.** For "view direction toward the surface," often one uses $-\mathbf{v}\cdot\mathbf{n}$
depending on whether $\mathbf{v}$ points from camera to surface or vice versa. With
$\mathbf{v}=(0,-1,0)$ as direction *of gaze* hitting a ground plane from above,
$\mathbf{v}\cdot\mathbf{n} = -1$ — gaze opposes the outward normal, i.e. looking at the front.
Define your view vector convention once and stick to it.
</details>

<details>
<summary>Exercise 3 — Squared test</summary>

Rewrite $\|\mathbf{p} - \mathbf{c}\| < R$ using only dots / length-squared, with
$\mathbf{d} = \mathbf{p} - \mathbf{c}$.

**Answer.** $\mathbf{d}\cdot\mathbf{d} < R^2$ (i.e. `d.LengthSquared < R*R`).
</details>

## Library recommendations

- **missing-function** — `08-vectors.plato` / `Vector3D`: no `Project(Self, onto: Self)` or
  `Reject(Self, onto: Self)`. The projection story is half of every dot-product lesson; without
  both, every caller rewrites `(a.Dot(u))*u` and hopes `u` was normalized.

- **missing-function** — no `AngleBetween(Vector3D, Vector3D): Angle` (with documented stability
  guarantees). Teaching $\cos\theta$ immediately needs a typed `Angle` result, not a raw `Number`
  radians footgun.

- **naming** — `Normed.Magnitude` / `MagnitudeSquared` vs intrinsic `Length` / `LengthSquared` on
  `Vector3D`. The dual vocabulary is confusing in examples; pick one as canonical in docs and make
  the other an alias.

- **doc-comment** — `Vector.Dot`: state the geometric formula and that inputs need not be unit;
  mention $\mathbf{a}\cdot\mathbf{a} = \|\mathbf{a}\|^2$. The interface currently names the function
  with no semantic gloss.

- **missing-function** — `Direction3D`: no `Dot(Direction3D, Direction3D)` that bypasses `.Vector`
  noise. Facing checks are the primary use of directions; a first-class dot would encode unit-unit
  intent.
