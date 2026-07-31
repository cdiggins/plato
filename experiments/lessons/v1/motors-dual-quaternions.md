---
lesson: motors-dual-quaternions
title: Motors and Dual Quaternions
domain: Rotations
v3-files: [10-rotations.plato, 13-transforms.plato]
audience: Knows rigid poses (position + orientation) and unit quaternions at a practical level.
status: draft-v1
---

# Motors and Dual Quaternions

A unit quaternion rotates about the origin. A pose adds a translation afterward.
Skinning a character, though, wants **one** algebraic object that both rotates
and translates — then blends those objects so elbows do not collapse the way
linear blend skinning does with matrices.

That object is a **motor** in geometric algebra, and in engineering practice it
usually shows up as a **dual quaternion**: two ordinary quaternions, "real" and
"dual," obeying a compact product rule. Plato's `Motor3D` is exactly that
encoding, with conversions to and from `Pose3D`.

## The idea

### Rigid motions as one product

A rigid motion in 3D is rotation plus translation. You can always write:

$$
p' = R\,p + t
$$

with unit quaternion (or rotor) $R$ and vector $t$. Composition of two such
motions is another of the same form — the group $SE(3)$. Matrices represent it
with $4\times 4$ homogeneous blocks; dual quaternions represent it with 8
numbers and a multiplication that stays in the dual-quaternion algebra.

### Dual numbers, briefly

A dual number is $a + \varepsilon b$ with $\varepsilon^2 = 0$. Dual quaternions
use the same idea with quaternion coefficients:

$$
\hat{q} = q_r + \varepsilon q_d
$$

Plato stores that as:

- `Real` — the rotational quaternion $q_r$ (unit for a pure rigid motion)
- `Dual` — $q_d$ encoding translation mixed with rotation

The standard packing for translation $t$ (as a pure quaternion) is:

$$
q_d = \tfrac{1}{2}\, t\, q_r
$$

so that recovering translation is:

$$
t = 2\, q_d\, q_r^{*}
$$

(with $q_r^{*}$ the conjugate). This is the convention in Plato's doc comments
and conversion functions.

### Why skinning likes them

**Linear blend skinning (LBS)** with matrices: blend $4\times 4$ matrices by
vertex weights, then apply. Interpolating matrices does not preserve rigidity —
volumes collapse at bent joints ("candy wrapper").

**Dual quaternion skinning (DQS):** blend unit dual quaternions (with care to
stay on the manifold), then apply the rigid motion. Blends stay closer to rigid
motions, reducing collapse. Cost is modest: eight scalars per bone influence
instead of sixteen, with a normalization step.

Motors are not only for skinning — any time you interpolate or accumulate
**poses** (robot links, camera paths with parallax, prop placement) a single
multiplicative type is nicer than syncing a quaternion and a vector by hand.

### Motor vs pose vs TRS

| Type | Rotation | Translation | Scale | Algebraic product |
|------|----------|-------------|-------|-------------------|
| `Pose3D` | yes | yes | no | compose as pair |
| `Motor3D` | yes | yes | no | dual-quaternion $\times$ |
| `Transform3D` | yes | yes | yes | not a group |

Use `Motor3D` when you want rigid composition/interpolation in one object.
Use `Pose3D` when position and orientation are clearer as separate fields.
Use `Transform3D` when scale matters.

## In Plato

From `13-transforms.plato`:

```plato
// A rigid motion (rotation plus translation) as a geometric-algebra motor,
// stored in dual-quaternion form: Real is the rotation, Dual encodes the
// translation as Dual = 0.5 * t * Real.
type Motor3D
    implements Value, Multiplicative
{
    Real: Quaternion;
    Dual: Quaternion;
}
```

`10-rotations.plato` supplies `Quaternion`; the motor type lives with transforms
because it is a full rigid placement, not a pure rotation.

Construction:

```plato
let pose = Pose3D {
    Position: Point3D { X: 1, Y: 2, Z: 0 },
    Orientation: Quaternion.CreateFromAxisAngle(
        Vector3D { X: 0, Y: 1, Z: 0 }, (0.3490659).Angle)  // 20°
};

let m = Motor3D(pose);
// Real = orientation
// Dual = 0.5 * t * Real  (t pure quaternion from position)

let poseBack = Pose3D(m);
// t = 2 * Dual * conjugate(Real)
```

Pure pieces:

```plato
let rotOnly = Motor3D(someQuaternion);
// Dual = 0

let moveOnly = Motor3D.CreateTranslation(Vector3D { X: 0, Y: 5, Z: 0 });
// Real = identity, Dual = half translation
```

Product conventions match the rest of the library:

```plato
// a.Multiply(b) = motion b followed by a  (Hamilton-like)
let product = a.Multiply(b);

// Compose(first, second) = apply first, then second
let chained = Compose(first, second);
// == second.Multiply(first)
```

Always prefer `Compose` when you think in "then" language — same rule as
`Pose3D` and matrices.

Inverse and normalize:

```plato
let undo = Inverse(m);
// dual-quaternion conjugate; inverse for unit motors

let unit = Normalize(m);
// rescale so Real has unit length; Dual scaled the same
```

Apply to points and vectors:

```plato
let world = localPoint.Transform(m);
// via Pose3D under the hood in v3

let dir = localVector.Transform(m);
// rotation only — translation skips vectors
```

Matrix escape hatch:

```plato
let mat = Matrix4x4(m);
// homogeneous rigid matrix of the same motion
```

## Pitfalls / fine print

**Blending dual quaternions like vectors without a sign check.** As with
ordinary quaternions, $\hat{q}$ and $-\hat{q}$ are the same rigid motion.
Weighted sums should flip signs so dots with a reference stay non-negative,
then normalize. v3 does not yet declare a `ScLERP` / dual-quaternion blend
helper — call that out before inventing one silently.

**Treating `Dual` as "the translation quaternion."** Translation is recovered
only through $2\,q_d q_r^{*}$. Reading `Dual.X/Y/Z` as meters is wrong whenever
`Real` is not identity.

**Mixing `Multiply` order with matrix mental models.** Row-vector matrices use
`Compose` = first then second = $M_1 M_2$ in Plato. Motor `Multiply` follows
quaternion order (second factor applies first). Use `Compose` to stay sane.

**Non-unit motors.** After many products, `Normalize`. Applying a non-unit motor
drifts from rigidity.

**Expecting scale.** Motors are rigid. Non-uniform scale belongs in
`Transform3D` / affine maps; DQS assumes bone matrices were rigid (or
rigidified) first.

**GA vocabulary vs dual-quaternion vocabulary.** Papers may say "motor,"
"screw," or "dual quaternion" for related ideas. Plato's `Motor3D` is
explicitly the dual-quaternion storage with the $q_d = \frac12 t q_r$ packing.

## Try it

1. For a pure translation by $(2,0,0)$ with identity rotation, what are `Real`
   and `Dual`?
2. Why does `Transform` on a `Vector3D` ignore translation for a motor?
3. You have two poses and want a halfway rigid motion. What is missing from v3
   that `Pose3D.Lerp` already approximates another way?

<details>
<summary>Answers</summary>

1. `Real = Quaternion.Identity`, `Dual = (1, 0, 0, 0)` because half of $(2,0,0)$
   is $(1,0,0)$ as a pure quaternion (`CreateTranslation` uses this).
2. Free vectors represent displacements; translating every copy of a
   displacement would change its meaning. Only the linear (here rotational)
   part acts.
3. A dual-quaternion `Slerp`/`ScLERP` on `Motor3D`. Today `Pose3D.Lerp`
   linearly blends positions and `Slerp`s orientations — a practical stand-in,
   not the same as blending in $SE(3)$ via motors.

</details>

## Library recommendations

- **missing-function** — `13-transforms.plato`: no
  `Slerp(a: Motor3D, b: Motor3D, t: Number): Motor3D` (or `ScLERP`). Skinning
  and this lesson's motivation need a declared blend; `Pose3D.Lerp` is the only
  rigid interpolation helper today.

- **missing-function** — `13-transforms.plato`: `Motor3D` has `Multiply` /
  `Compose` / `Inverse` / `Normalize` but no `Dot` or `Antipodal` helper for
  the sign-alignment step DQS requires. Without it every skinning
  implementation reinvents the same check.

- **doc-comment** — `13-transforms.plato`: `type Motor3D` should mention the
  unit dual-quaternion invariant ($|q_r|=1$ and the dual orthogonality
  condition $q_r\cdot q_d = 0$) explicitly. The packing formula is there; the
  invariant pair is what `Normalize` is trying to restore.

- **naming** — `13-transforms.plato`: `CreateTranslation(_: Motor3D, v)` is
  easy to confuse with `Matrix4x4.CreateTranslation`. A name like
  `FromTranslation` on the motor static side would mirror `Motor3D(pose)` and
  read clearer in teaching snippets.
