---
lesson: euler-angles-and-gimbal-lock
title: Euler Angles and Gimbal Lock
domain: Rotations
v3-files: [10-rotations.plato]
audience: Familiar with 3D axes and the idea of rotating an object; no prior exposure to quaternions required beyond knowing they exist as an alternate encoding.
status: draft-v1
---

# Euler Angles and Gimbal Lock

Aircraft attitude is taught as three numbers you can feel: **yaw** (heading),
**pitch** (nose up/down), **roll** (bank). Animators think the same way — twist
the head, nod, then tilt. Three dials feel like the whole story of orientation.

They are not. Three chained elemental rotations always leave a singularity:
at certain pitches, two of the dials start turning the object about the *same*
axis. That degeneracy is **gimbal lock**. It is not a bug in your code; it is a
property of the parameterization. Plato makes the other hazard explicit too:
there are **six** distinct ways to order the three elemental spins, and they
are not interchangeable.

## The idea

An **elemental rotation** is a spin about one fixed coordinate axis — world $X$,
$Y$, or $Z$. An **Euler-angle triple** is three such spins composed in a chosen
order. If $R_x(\alpha)$, $R_y(\beta)$, $R_z(\gamma)$ are the elemental maps, one
common choice is:

$$
R = R_z(\gamma)\, R_x(\beta)\, R_y(\alpha)
$$

(application order: yaw about $Y$, then pitch about $X$, then roll about $Z$ —
the order Plato's default decomposition uses).

### Why order matters

Rotations in 3D do **not** commute. Spin $90°$ about $X$ then $90°$ about $Y$
is a different orientation from the reverse order. So "yaw = 30°, pitch = 10°,
roll = 5°" is meaningless until you fix the **rotation order**. Different DCC
tools, game engines, and robotics packages silently pick different defaults.
Exporting angles without the order is like exporting a matrix without saying
row-vector or column-vector.

There are $3! = 6$ permutations of three distinct axes. Plato names them all:

```
XYZ  XZY  YXZ  YZX  ZXY  ZYX
```

Each letter is an axis; the sequence is **application order** (first-listed
applies first) about **fixed world axes** — the extrinsic convention documented
in the transforms library.

### What gimbal lock is

Picture three nested gimbals, each contributing one angle. When the middle
gimbal aligns two of the outer axes, those two gimbals share an axis: you lost
a degree of freedom. In the yaw-pitch-roll picture, this happens at
**pitch $= \pm 90°$** for the common $ZXY$-style orders — looking straight up
or straight down. At that pole:

- many distinct (yaw, roll) pairs describe the *same* orientation;
- a small change in desired orientation can demand a huge jump in the stored
  angles;
- interpolating the three numbers independently produces wild spins.

```
        +Y (up)
         |
         |   pitch → ±90°
         |      \
         ●-------→ +X
        /          middle ring
       /           collapses onto
     +Z            another axis
```

The orientation space of 3D rotations is topologically a projective 3-sphere
(or $SO(3)$). Covering it with three angle charts always needs singularities —
the same reason you cannot give Earth a single latitude/longitude chart without
poles acting strangely.

### When Euler angles are still the right tool

- Authoring UIs: humans understand "tilt forward 15°."
- Interop with formats that only store yaw/pitch/roll.
- Constraints that truly are "only allow pitch in this range."

For composition, interpolation, and accumulation of many small updates, convert
to a singularity-free form (`Quaternion`, `AxisAngle`, `Rotor3D`) and convert
back only at the edges.

## In Plato

```plato
// The order in which elemental axis rotations compose.
type RotationOrder = XYZ | XZY | YXZ | YZX | ZXY | ZYX;

// Three chained elemental rotations under an explicit composition order.
type EulerAngles
    implements Value
{
    Yaw: Angle;
    Pitch: Angle;
    Roll: Angle;
    Order: RotationOrder;
}
```

Every `EulerAngles` value carries its `Order`. That is the load-bearing design
choice: the type refuses to be "three angles with a tribal-knowledge default."

Semantic field mapping (from the transforms library docs):

| Field | Axis | Role |
|-------|------|------|
| `Pitch` | $X$ | elevation / nod |
| `Yaw` | $Y$ | heading / turn |
| `Roll` | $Z$ | bank / tilt |

Angles are the `Angle` type — never raw `Number` — so degree/radian mistakes
are type errors rather than silent unit bugs.

Usage-shaped expressions:

```plato
let look = EulerAngles {
    Yaw: (0.5235988).Angle,    // 30°
    Pitch: (0.1745329).Angle,  // 10°
    Roll: (0.0).Angle,
    Order: RotationOrder.ZXY
};

// Convert through the quaternion hub
let q = Quaternion(look);

// Decompose back — always returns Order = ZXY in v3
let again = EulerAngles(q);
// At pitch ≈ ±90°, Yaw and Roll are not unique
```

Building from elementals with an explicit order:

```plato
let q = order.ChainRotations(
    Quaternion.CreateRotationX(pitch),
    Quaternion.CreateRotationY(yaw),
    Quaternion.CreateRotationZ(roll));
```

`CreateFromYawPitchRoll` on `Quaternion` / `Matrix4x4` exists as an intrinsic
for the fixed convention matching `EulerAngles(q)`'s $ZXY$ decomposition.
Prefer constructing `EulerAngles { … Order: … }` when the order might vary.

## Pitfalls / fine print

**Omitting `Order` in your head.** Two pipelines both saying "yaw, pitch, roll"
can disagree completely if one is $YXZ$ and the other $ZXY$. Always serialize
the order with the angles.

**Intrinsic vs extrinsic.** Plato documents **fixed world axes**, first-listed
applied first. Body-fixed (intrinsic) Tait–Bryan angles use the same six labels
with different composition meaning. Do not mix conventions when reading a paper.

**Interpolating Euler fields with `Lerp` on each angle.** Near a pole, the
shortest rotation in orientation space is not the straight line in angle space.
Convert to `Quaternion` and use `Slerp`, then decompose if you must show dials.

**Assuming `EulerAngles(q)` round-trips uniquely.** At the gimbal-lock poles the
doc comment states yaw and roll are not unique. Equality of orientations is
equality of quaternions (up to sign), not equality of Euler triples.

**Wrapping.** Angles that differ by a full turn are the same elemental spin, but
naive comparison of `Yaw` values will disagree. Normalize or compare in
quaternion space.

**Editing only one dial in a UI while pitch sits at 89°.** The remaining freedom
is ill-conditioned; small mouse moves on yaw can look like roll. Clamp pitch
away from the poles for authoring tools, or switch the UI to arcball/quaternion
drags when close.

## Try it

1. Why does `EulerAngles` store `Order` as a field instead of making six
   separate types?
2. At pitch $= +90°$ with order $ZXY$, you change yaw by $+10°$ and roll by
   $-10°$. What happens to the orientation, qualitatively?
3. You receive three numbers from a file with no order metadata. What can you
   safely do in Plato's type system?

<details>
<summary>Answers</summary>

1. The six orders share the same three angle meanings; a sum type for order
   plus one record avoids duplicating fields and forces every value to state
   its convention.
2. Those two adjustments largely cancel or combine into a single effective
   spin about the remaining free axis — the classic gimbal-lock coupling.
   The orientation barely changes (or changes about an unexpected axis).
3. Nothing safe without assuming a convention. Constructing `EulerAngles`
   requires picking an `Order`; document the assumption or refuse to load.

</details>

## Library recommendations

- **wrong-shape** — `10-rotations.plato` / `13-transforms.plato`:
  `EulerAngles(q: Quaternion)` always returns `RotationOrder.ZXY`, discarding
  any preferred order the caller might want. A second overload
  `EulerAngles(q, order: RotationOrder)` (with a documented singularity policy
  per order) is what interop and teaching both need.

- **doc-comment** — `10-rotations.plato`: `EulerAngles` fields document names
  but not the axis mapping (Yaw→Y, Pitch→X, Roll→Z). That mapping currently
  lives only in `13-transforms.plato` library banners; it should sit on the
  type so the declaration file teaches alone.

- **missing-function** — `10-rotations.plato`: no
  `IsNearGimbalLock(e: EulerAngles, tolerance: Angle): Boolean` (or on the
  quaternion before decompose). Authoring tools and this lesson's warnings
  need a shared predicate rather than re-deriving pole tests.

- **naming** — `70-intrinsics.plato`: `CreateFromYawPitchRoll` hides the fixed
  $ZXY$ convention in the name. Aligning the doc comment with
  `EulerAngles.Order` (or renaming toward `CreateFromEulerZXY`) would reduce
  the "which yaw-pitch-roll?" confusion this lesson exists to prevent.

- **missing-function** — `70-intrinsics.plato` / `06-quantities.plato`: only
  `Angle(x: Number)` (radians payload) is intrinsic. Authoring examples want
  `Degrees(x: Number): Angle` (and maybe `Turns`) so UI-shaped snippets are not
  forced to write raw radian literals for every yaw/pitch/roll dial.
