---
lesson: quaternion-double-cover
title: Quaternion Double Cover of 3D Rotations
domain: Rotations
v3-files: [10-rotations.plato]
audience: Knows that a quaternion stores a 3D rotation; ready to confront sign ambiguity and 720° topology.
status: draft-v1
---

# Quaternion Double Cover of 3D Rotations

Store a rotation as a unit `Quaternion`. Negate every component. Apply both to the same
vector with the sandwich product. The results match — yet the four numbers differ. That
is the **double cover**: two unit quaternions for every ordinary 3D orientation.

Miss this fact and you will fail equality tests, take the long path in interpolation, and
wonder why a $360°$ spin left a minus sign on your pose. This lesson focuses on that
topology and how Plato's `Quaternion` type forces you to respect it.

## One rotation, two hemispheres

Unit quaternions are points on the 3-sphere $S^3$ in $\mathbb{R}^4$:

$$
X^2 + Y^2 + Z^2 + W^2 = 1
$$

Ordinary rotations form the group $SO(3)$. The map $S^3 \to SO(3)$ identifies antipodes:

$$
q \;\sim\; -q
$$

```
     S³ ⊂ ℝ⁴                    SO(3)
   unit quaternions           rotations
        q  ●                      ●  R
           |                     /
           | identify           /
        -q ●                   /
                              /
                    (same physical orientation)
```

**Why algebraically.** Rotating a vector uses $q\,v\,q^{-1}$ (for unit $q$, inverse is
the conjugate). Substituting $-q$:

$$
(-q)\,v\,(-q)^{-1} = (-q)\,v\,(-q)^{*} = q\,v\,q^{*}
$$

The two minus signs cancel. The induced linear map on $\mathbb{R}^3$ is identical.

### Axis-angle reading

A unit quaternion from axis $\mathbf{u}$ and angle $\theta$:

$$
q = \bigl(u_x\sin\tfrac{\theta}{2},\;
         u_y\sin\tfrac{\theta}{2},\;
         u_z\sin\tfrac{\theta}{2},\;
         \cos\tfrac{\theta}{2}\bigr)
$$

Negating $q$ is the same as replacing $\theta$ by $\theta + 2\pi$ in the half-angle
formulas (trig identities), or flipping the axis and adjusting the angle — still one
rotation in $SO(3)$.

| Quaternion | Axis-angle story | Same rotation as |
|------------|------------------|------------------|
| $q(\mathbf{u},\theta)$ | right-hand turn $\theta$ about $\mathbf{u}$ | — |
| $-q$ | $\theta+2\pi$ about $\mathbf{u}$, or equivalent | $q$ |
| conjugate $q^{*}$ | turn $-\theta$ about $\mathbf{u}$ | inverse of $q$ |

Conjugate is **not** the double-cover partner. Double cover is full negation
$(-X,-Y,-Z,-W)$, which also flips the scalar part.

### The $720°$ loop

Walk a continuous path of rotations that turns an object $360°$ about a fixed axis and
returns it to the starting pose. In $SO(3)$ you are home. Lift that path into $S^3$ and
you typically arrive at $-q$, not $q$. Only after another $360°$ (total $720°$) does the
lifted path close on the same quaternion.

```
 orientation in space:   ● ──────────────────────── ●  (back after 360°)
 quaternion lift:        q ──────────────────────→ -q
                         -q ─────────────────────→  q  (after another 360°)
```

Belt tricks and Dirac's plate trick demonstrate the same topology. In software you almost
always care about orientation mod sign; path memory on $S^3$ matters mainly for
spinorial or continuous-tracking applications.

## In Plato

```plato
// A unit quaternion encoding a 3D rotation. Invariant: X*X + Y*Y + Z*Z + W*W = 1.
type Quaternion
    implements Value, Multiplicative, Interpolatable
{
    X: Number;
    Y: Number;
    Z: Number;
    W: Number;
}
```

The doc comment states the unit-length invariant. It does **not** state $q \sim -q$.
That silence is why this lesson exists as its own topic.

Related types still talk about one geometric rotation each:

```plato
type AxisAngle implements Value
{
    Axis: Direction3D;
    Angle: Angle;
}

type EulerAngles implements Value
{
    Yaw: Angle;
    Pitch: Angle;
    Roll: Angle;
    Order: RotationOrder;
}
```

Converting `AxisAngle` → `Quaternion` picks one of the two covers (usually $W \ge 0$ for
angles in a conventional range). Converting back may not round-trip component-wise to the
quaternion you started with — only to an equivalent rotation.

### Comparing orientations

Component equality is wrong:

```
var q  = Quaternion(0.0, 0.7071, 0.0, 0.7071);   // 90° about +Y
var nq = Quaternion(0.0, -0.7071, 0.0, -0.7071); // -q

// q == nq  → false as tuples
// Transform(v, q) and Transform(v, nq) → same Vector3D for every v
```

A robust sameness test uses the absolute dot product on $S^3$:

$$
\bigl|\, q_a \cdot q_b \,\bigr| \approx 1
\quad\text{(within a small tolerance)}
$$

If the signed dot product is negative, the representatives lie in opposite hemispheres;
flip one before measuring angular separation for interpolation.

### Interpolation and the long arc

`Quaternion` implements `Interpolatable`. Spherical interpolation (`Slerp`) along $S^3$
has two arcs between $q_a$ and $q_b$. The short arc in rotation space corresponds to the
hemisphere choice where $q_a \cdot q_b \ge 0$.

```
var a = Quaternion(...);
var b = Quaternion(...);
// Before Slerp: if Dot(a, b) < 0, replace b with -b (or a with -a)
var mid = Slerp(a, b, 0.5);
```

Without the flip you can spin nearly $360°$ when the poses were only a few degrees apart
in $SO(3)$.

### `Rotor3D` shares the cover

```plato
type Rotor3D
    implements Value, Multiplicative
{
    Scalar: Number;
    Bivector: Bivector3D;
}
```

`Rotor3D` is structurally a quaternion in plane-based components. Negating scalar and
bivector together is the same double cover. Any API that treats rotors as orientations
must use the same sign-aware comparison.

## Pitfalls and fine print

**`==` on fields.** Never use component equality as orientation equality.

**Normalization vs negation.** `Normalize` fixes length drift; it does not choose a
canonical hemisphere. Many codebases also enforce $W \ge 0$ after normalize — that is a
separate convention on top of unit length.

**Logging poses.** Dumping raw $X,Y,Z,W$ in a diff tool will show "changes" that are pure
sign flips. Diff rotations via axis-angle or matrices if you care about user-visible pose.

**Inverse vs antipode.** `Conjugate` (inverse for unit quaternions) undoes a rotation.
Negation does not undo it; negation is the same rotation.

**Euler extraction.** Ambiguous Euler triples at gimbal poles are a different problem
from double cover. You can hit both at once when converting for UI readouts.

## Try it

<details>
<summary>Exercise 1 — Same action?</summary>

`q1 = (0, 0, 0, 1)` and `q2 = (0, 0, 0, -1)`. Do they rotate vectors the same way?

**Answer.** Yes. `q2 = -q1`; both are the identity rotation in $SO(3)$ (identity often
stored as $W = +1$ by convention).
</details>

<details>
<summary>Exercise 2 — Dot product test</summary>

`a = (0.0, 0.707, 0.0, 0.707)`, `b = (0.0, -0.707, 0.0, -0.707)`. What is `Dot(a,b)`?
Are they the same orientation?

**Answer.** Dot $= -1$. Absolute value $1$ → same orientation, opposite hemisphere.
</details>

<details>
<summary>Exercise 3 — Slerp prep</summary>

You will `Slerp` from `a` to `b` in Exercise 2. Should you negate one operand first?

**Answer.** Yes — signed dot is negative. Negate `b` (or `a`) so the short arc is used;
otherwise you traverse the long way around $S^3$.
</details>

## Library recommendations

- **doc-comment** — `10-rotations.plato`: `Quaternion` should state that `q` and
  `Negative(q)` encode the same rotation. The unit-length invariant alone is not enough;
  double cover is the #1 consumer footgun.

- **missing-function** — no declared
  `ApproximatelySameRotation(a, b, tolerance)` or `SameOrientation(a, b)` using
  $|a\cdot b|$. Without it every engine reimplements fragile ad-hoc checks.

- **missing-function** — no declared `Canonicalize(q)` (e.g. force $W \ge 0$) for
  deterministic serialization. Optional, but useful beside `Normalize`.

- **pedagogy** — `Interpolatable` on `Quaternion` does not document the hemisphere flip
  required for shortest-path blends. Either `Slerp` docs (when bodies exist) or the type
  comment should mention `Dot < 0` negation.
