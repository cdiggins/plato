---
lesson: linear-interpolation
title: Linear Interpolation — Lerp Everywhere
domain: Foundations & vectors
v3-files: [02-interfaces-algebra.plato, 08-vectors.plato, 14-color.plato]
audience: High-school algebra and basic programming; no graphics API experience required
status: draft-v1
---

# Linear Interpolation — Lerp Everywhere

You have a character at one side of the room and you want them halfway to the door.
You have a UI slider at 30% and you want the background to sit 30% of the way between
dark gray and light gray. You have two keyframes in an animation track and you need the
value at 40% of the way from the first to the second. Every one of these is the same
mathematical move: pick a **blend factor** and walk along the straight line between two
values.

That move has a name programmers use constantly: **linear interpolation**, almost always
shortened to **lerp**. The idea is tiny; the reach is enormous. Once you recognize lerp,
you see it in motion, shading, camera paths, audio crossfades, physics blends, and
anywhere else two endpoints need a smooth family of in-between values.

## The formula

Given a start value `a`, an end value `b`, and a blend parameter `t`, linear
interpolation is:

$$
\text{lerp}(a, b, t) = a + (b - a)\,t
$$

Equivalently:

$$
\text{lerp}(a, b, t) = a(1 - t) + b\,t
$$

Both forms are the same algebra. The first reads naturally as "start at `a`, then move
along the displacement `(b - a)` by a fraction `t` of its length." The second reads as
"a weighted average of `a` and `b`."

Work through the anchor cases:

| `t` | Result | Meaning |
|-----|--------|---------|
| `0` | `a` | entirely the start |
| `1` | `b` | entirely the end |
| `0.5` | halfway | equal blend of `a` and `b` |
| `0.25` | nearer `a` | one quarter of the way toward `b` |

On the number line, `lerp(2, 10, 0.25)` is `2 + (10 - 2) * 0.25 = 4`. You moved one
quarter of the distance from 2 to 10.

ASCII picture of lerp on a 1D interval:

```
a=2                          b=10
 |----|----|----|----|----|----|----|----|
 0    t=.25              t=.5              t=1
      ^                   ^                 ^
      4                   6                10
```

Nothing in the formula requires `t` to stay between 0 and 1. That constraint is a
**usage choice**, not part of the definition.

## Why `t` is unitless

The parameter `t` is a **pure fraction**: how far you have traveled from `a` toward `b`
along the straight segment connecting them. It carries **no physical units**.

- `t = 0.3` means "30% of the way from `a` to `b`" — not "0.3 seconds", not "0.3
  meters", not "0.3 radians."
- If `a` and `b` are lengths in meters, the result is still in meters; `t` does not
  inherit those meters. It scales the **displacement** `(b - a)`, which already has the
  right units.

This distinction matters whenever time enters the story. An animation clock might say
"we are 0.4 of the way through this segment" — that 0.4 is a **normalized progress**
value you feed to lerp. The elapsed duration lives elsewhere (for example in a
`Duration` or a keyframe track's timing data). Conflating "40% through the blend" with
"40 milliseconds" is a common source of bugs: the lerp itself always wants a unitless
scalar in the `Number` slot.

In Plato's vocabulary, `t` is typed as `Number` — the generic unitless real scalar —
not as `Duration`, `Length`, or any other quantity type. That is deliberate: lerp is
**structural** ("how much of `b` versus `a`?"), not **dimensional**.

## Extrapolation

When `t` falls **outside** the closed interval `[0, 1]`, lerp **extrapolates**: it
continues along the same straight line past one endpoint or the other.

```
a=2                          b=10
 |----|----|----|----|----|----|----|----|----|----|
          0   1
 t=-0.5  ^                   t=1.5              ^
         0                                         14
   (half a unit before a)              (half a unit past b)
```

Examples on numbers:

- `lerp(2, 10, -0.5) = 2 + 8 * (-0.5) = -2` — extended backward from `a`
- `lerp(2, 10, 1.5) = 2 + 8 * 1.5 = 14` — extended forward past `b`

Extrapolation is not a bug. Camera dolly moves, springy UI overshoot, linear trend
prediction, and "continue at the same rate" all rely on `t` being **unclamped**. If you
need to stay inside the segment, clamp `t` to `[0, 1]` **before** calling lerp — that is
a separate policy decision.

Plato's `Interpolatable` interface documents this explicitly: `t` is unclamped; 0 yields
`a`, 1 yields `b`, and values outside `[0, 1]` extrapolate.

## Component-wise lerp

For types built from several `Number` components, lerp is applied **independently to
each component**. There is no cross-talk: `X` blends with `X`, `Y` with `Y`, and so on.

For a 2D displacement:

$$
\text{lerp}((a_x, a_y), (b_x, b_y), t) = (a_x + (b_x - a_x)t,\; a_y + (b_y - a_y)t)
$$

Geometrically, that is the straight segment in the plane from one arrow tip to the
other — not an arc, not a spline. In 3D, the same rule extends to three components. For
RGBA color, each of `R`, `G`, `B`, and `A` lerps on its own.

```
Vector2D a=(0,0)  -------- straight line --------  b=(10,4)
                      t=.5 -> (5, 2)
```

**Important geometric note:** lerping two **positions** (points) component-wise gives
the point on the straight segment in space between them — exactly what you want for
"mix these two world locations." Lerping two **displacements** (vectors) gives the
vector whose tip lies on the segment between the two tips when both are drawn from the
origin. Lerping two **directions** (`Direction2D`, `Direction3D`) component-wise does
**not** guarantee a unit-length result at intermediate `t`; normalization is a separate
step.

## In Plato: the `Interpolatable` interface

Plato centralizes linear interpolation in one algebraic interface:

```plato
// Supports linear interpolation. The parameter t is unclamped: 0 yields a,
// 1 yields b, values outside [0,1] extrapolate.
interface Interpolatable
{
    Lerp(a: Self, b: Self, t: Number): Self;
}
```

Every type that implements `Interpolatable` gets a uniform `Lerp` operation with the
semantics above. Call shape (interface functions take `Self` first):

```plato
start.Lerp(end, t)
```

### Numbers

`Number` implements `Real`, which inherits `Numerical`, which inherits `Interpolatable`.
Scalars are the ground case:

```plato
(2.0).Lerp(10.0, 0.5)      // 6.0
(2.0).Lerp(10.0, 1.5)      // 14.0 — extrapolation
```

`Integer` also implements `Interpolatable` (via `Whole`), but lerping whole numbers
produces real-valued blends unless you round afterward — usually you lerp `Number`
instead when you care about smooth motion.

### Vectors and numeric tuples

The `Vector` interface inherits `Numerical`, so every vector-like numeric tuple is
interpolatable with component-wise `Lerp`:

```plato
(0.0, 0.0, 0.0).Lerp((10.0, 4.0, -2.0), 0.25)           // Vector3D (2.5, 1.0, -0.5)
(1.0, 0.0, 0.0, 1.0).Lerp((0.0, 0.0, 1.0, 1.0), 0.5)   // Number4 (0.5, 0.0, 0.5, 1.0)
```

The low-level tuples `Number2`, `Number3`, `Number4`, and `Number8` share the same
behavior. The geometric types `Vector2D` and `Vector3D` share it too. `VectorN` (runtime
length) lerps each entry of `Components` the same way.

Because `Vector` also inherits `Additive` and `Scalable`, this identity always holds:

```plato
a.Lerp(b, t)  ==  a.Add(b.Subtract(a).Multiply(t))
```

That is the displacement form of the formula: subtract to get the delta, scale by `t`,
add back to the start.

### Points

Positions (`Point2D`, `Point3D`, `PointN`) implement `Coordinate`, which inherits
`Interpolatable`. Point lerp is component-wise on coordinates — geometrically, the point
that divides the segment between two locations in the ratio `t`:

```plato
(0.0, 0.0, 0.0).Lerp((10.0, 0.0, 10.0), 0.5)   // Point3D (5.0, 0.0, 5.0)
```

Points also implement `Difference<Vector3D>` (for `Point3D`), which exposes `Between` —
the displacement from one point to another. That ties lerp to affine geometry:

```plato
p0.Add(p0.Between(p1).Multiply(t))   // same as p0.Lerp(p1, t) for Point3D p0, p1
```

`Between(a, b)` is `b - a` in vector language; scaling that displacement and adding it
to `a` **is** linear interpolation along the segment.

### Colors

`Color` is linear-light RGBA with components nominally in `[0, 1]` (HDR values may
exceed 1). It implements `Numerical`, so interpolation is component-wise on `R`, `G`,
`B`, and `A`:

```plato
(1.0, 0.0, 0.0, 1.0).Lerp((0.0, 0.0, 1.0, 1.0), 0.5)   // Color (0.5, 0.0, 0.5, 1.0)
```

`ColorStop` pairs a `Position` in `[0, 1]` with a `Color`; a `ColorGradient` is an
ordered list of stops. Evaluating a gradient at some `t` is repeated lerping between
adjacent stops — the stop positions are **where** to lerp, not a different interpolation
rule.

```plato
// Between two stops: lerp their Color fields at local t within the segment
(1.0, 1.0, 1.0, 1.0).Lerp((0.0, 0.0, 0.0, 1.0), 0.3)   // 30% toward black
```

## What lerp is not

Linear interpolation is the **straight-line** blend. It is the right default for
`Number`, `Vector3D`, `Point3D`, and **linear** `Color` — but not for every quantity
you might want to "mix."

| Situation | Why plain `Lerp` misleads | What to do instead |
|-----------|---------------------------|-------------------|
| Rotations in 3D | Component lerp does not preserve orthogonality; speed varies | Spherical interpolation (`Slerp` on quaternions / rotors) |
| `Direction2D` / `Direction3D` | Intermediate vectors are not unit length | Lerp the underlying `Vector`, then normalize — or use angle-aware blending |
| `ColorHSV` / `ColorHSL` | Hue is circular (`Angle`); RGB lerp != hue arc lerp | Convert, lerp in a chosen space, or dedicated hue interpolation |
| `Color8` (8-bit storage) | Not `Numerical`; channels are integers, often sRGB-encoded | Convert to linear `Color`, lerp, convert back |
| Long paths in curved space | Straight chord != geodesic | Use curve evaluation (`Curve3D` at parameter `t`), not endpoint lerp |

None of this contradicts `Interpolatable`. It marks **where the interface stops** and
domain-specific interpolation begins.

## Pitfalls and fine print

**Clamping `t` is optional.** Plato does not clamp for you. UI code that must pin
results between endpoints should clamp explicitly (types implementing `Clampable` can
clamp `t` as a `Number` before the call). Animation easing functions reshape **time**
into a new `t` before lerping values — the lerp itself stays linear in the value space.

**Squared distance vs lerp parameter.** Moving with `t = 0.5` puts you halfway in
**parameter** along the segment. It also puts you halfway in **Euclidean distance** for
straight segments in Cartesian coordinates — but that coincidence fails for curved
paths and for weighted or non-uniform parameterizations.

**Alpha premultiplication.** Lerping `Color` with straight `(R,G,B,A)` components is
correct for **unpremultiplied** linear RGBA. If textures store premultiplied alpha,
lerp the premultiplied tuple, not the straight one — mixing conventions produces dark
fringe artifacts.

**Extrapolated colors.** Because `t` is unclamped, `red.Lerp(blue, 1.5)` yields channel
values outside `[0, 1]`. That is valid for linear HDR workflows; display encoding is a
separate step.

**Integer and discrete blends.** `Integer` implements `Interpolatable`, but
`a.Lerp(b, 0.3)` is not an integer. For pixel indices or voxel coordinates, round or
use a different rule.

**N-dimensional consistency.** `VectorN` and `PointN` lerp every element of
`Components`. If `a` and `b` have different lengths, that is a precondition violation —
not something lerp defines.

## Try it

Work these by hand, then check mentally against the formula.

**1.** `lerp(100, 200, 0.2) = ?`

**2.** `lerp((0, 10), (20, 0), 0.5)` as a `Vector2D` — what are `X` and `Y`?

**3.** A gradient stop at `Position = 0.0` is white `(1,1,1,1)` and a stop at
`Position = 1.0` is black `(0,0,0,1)`. What is the `Color` at `t = 0.75`?

**4.** `lerp(5, 15, 2.0) = ?` — interpolation or extrapolation?

**5.** You lerp `Direction3D` values by lerping their `Vector` fields and skipping
normalization. At `t = 0.5`, is the result necessarily length 1?

<details>
<summary>Answers</summary>

**1.** `100 + (200 - 100) * 0.2 = 120`

**2.** `X = 0 + (20 - 0) * 0.5 = 10`, `Y = 10 + (0 - 10) * 0.5 = 5` → `(10, 5)`

**3.** `0.25 * white + 0.75 * black` → `(0.25, 0.25, 0.25, 1.0)` — a dark gray

**4.** `5 + 10 * 2 = 25` — extrapolation (`t > 1`)

**5.** No. Only the endpoints are guaranteed unit length; the chord midpoint of two unit
vectors is generally shorter unless they are parallel.

</details>

## Library recommendations

- **missing-function** — `02-interfaces-algebra.plato`: add `InverseLerp(a: Self, b: Self,
  value: Self): Number` on `Interpolatable` (or a small companion interface). Given
  endpoints and a value on the line, return the `t` that produced it. Every
  "map this sensor reading into `[0,1]`" and "where on the segment is this point?"
  workflow needs inverse lerp; authors should not re-derive `(value - a) / (b - a)` ad
  hoc with divide-by-zero guards scattered through call sites.

- **missing-function** — `02-interfaces-algebra.plato`: add `Remap(value: Self, fromMin:
  Self, fromMax: Self, toMin: Self, toMax: Self): Self` (or a `Number`-valued overload
  when remapping scalars). Remap is two inverse lerps composed — from domain to unitless
  `t`, then into the target range — and appears in every shader, UI layout, and
  animation rigging lesson that touches normalized coordinates.

- **doc-comment** — `14-color.plato` `Color`: the doc says interpolation is
  component-wise (good) but should state explicitly that `Lerp` assumes **unpremultiplied
  linear** RGBA and that lerping `Color8` or sRGB-encoded bytes without conversion is
  incorrect. This lesson's main color pitfall is invisible from the type declaration
  alone.

- **missing-interface** — `14-color.plato`: `ColorHSV` and `ColorHSL` implement `Value`
  but not `Interpolatable`. Hue is an `Angle`; naive RGB lerp and hue-aware lerp diverge.
  Either document "convert to `Color` before `Lerp`" prominently or declare a separate
  `HueInterpolatable` (or `LerpHue` on `ColorHSV`) so hue-wheel blending has typed,
  reviewable semantics.

- **doc-comment** — `08-vectors.plato` `Direction2D` / `Direction3D`: note that
  `Vector`/`Numerical` inheritance does **not** apply to `Direction2D`/`Direction3D`
  (they only implement `Value`), but authors coming from other engines assume direction
  lerp exists. A one-line comment — "normalize after blending the underlying `Vector`,
  or use angular interpolation" — would prevent a class of rendering bugs.

- **pedagogy** — `02-interfaces-algebra.plato` `Interpolatable`: consider a doc-comment
  example block showing `t` unclamped with one extrapolation case. The comment already
  states the rule; a single numeric example (`Lerp(0, 10, 1.5) => 15`) would match how
  often extrapolation surprises newcomers who expect silent clamping.

> Resolved 2026-07-28: intervals-transforms.library.plato added NumberInterval At/ParameterOf/Remap (items 466, 209, 210) and wrap-aware AngleInterval Span/Contains; scalar InverseLerp/Remap already existed on Real in core-algebra.library.plato.
