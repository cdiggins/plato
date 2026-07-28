---
lesson: keyframes-and-tracks
title: Keyframes and Tracks
domain: Animation & motion
v3-files: [37-keyframes-tracks.plato]
audience: Familiar with time-based animation and the idea of interpolating values.
status: draft-v1
---

# Keyframes and Tracks

A tween says "go from A to B in two seconds." Real character and camera work says
something richer: "at 0 s be here, at 0.4 s there with a hold, at 1.2 s overshoot, then
settle." That authoring model is **keyframes on a track** — discrete samples on a
timeline, plus rules for filling the gaps and for what happens outside the keyed range.

Underneath, every track is the same idea: a function from time to a typed value. The
keyframes are how artists edit that function.

## The idea

A **keyframe** is a stamped sample $(t_i, v_i)$ plus a recipe for reaching the next
sample. A **track** is an ordered sequence of keys with policies for time before the
first key and after the last. Sampling the track at time $t$ means:

1. Find the segment $[t_i, t_{i+1}]$ that contains $t$ (or apply extrapolation).
2. Form a local parameter $u \in [0,1]$ (possibly reshaped by an easing).
3. Combine $v_i$ and $v_{i+1}$ with the segment's interpolation mode.

```
value
  ^
v2|          *---------*
  |         /           \
v1|   *----*             \
  |  /                    *  v3
  +--+----+----+----+------> time
     t0   t1   t2   t3
```

### Interpolation modes

Different properties want different fills:

| Mode | Behavior | Typical use |
|------|----------|-------------|
| Constant | hold $v_i$ until $t_{i+1}$ | visibility flags, discrete enums |
| Linear | lerp in the value space | positions, scales, floats |
| CubicSpline | smooth auto-tangents | organic motion |
| Bezier / Hermite | explicit tangent handles | hand-authored curves |
| Spring | approach next value with damping | reactive UI follow |

### Extrapolation

Outside the keyed span you choose a policy: clamp (Hold), repeat (Loop), mirror
(PingPong), continue the end slope (LinearContinue), or loop while accumulating the
net change (LoopWithOffset — the classic "keep walking" pattern).

## In Plato

`37-keyframes-tracks.plato` centers on the concept `TimeVarying<TValue>`:

```
concept TimeVarying<TValue>
{
    Sample(x: Self, time: Duration): TValue;
}
```

Time is a `Duration` offset from the track or clip start — not a bare `Number`, and not
an absolute `Instant`. That keeps tracks relocatable on a timeline.

### Keys

```
type KeyInterpolation = Constant | Linear | CubicSpline | Bezier | Hermite | Spring;

type Keyframe<T>
{
    Time: Duration;
    Value: T;
    Interpolation: KeyInterpolation;
    Easing: ClassicEasing;
}
```

`Easing` reshapes the segment parameter when interpolation is not already
tangent-based. For Bezier/Hermite authoring, use the companion record:

```
type TangentKeyframe<T>
{
    Time: Duration;
    Value: T;
    InTangent: T;
    OutTangent: T;
    InWeight: Proportion;
    OutWeight: Proportion;
}
```

Tangents are value-slopes per second; weights are fractions of the segment span
(classic default one third).

### Tracks

```
type AnimationTrack<T>
{
    Keys: Array<Keyframe<T>>;
    Before: Extrapolation;
    After: Extrapolation;
}

type Extrapolation = Hold | Loop | PingPong | LinearContinue | LoopWithOffset;
```

`TangentTrack<T>` is the same shape with `TangentKeyframe<T>`. Both implement
`TimeVarying<T>`.

Usage-shaped sampling:

```
track = AnimationTrack(
    Keys: [Keyframe(Duration(Seconds: 0), 0, KeyInterpolation.Linear, ClassicEasing.Linear),
           Keyframe(Duration(Seconds: 1), 10, KeyInterpolation.Linear, ClassicEasing.Linear)],
    Before: Extrapolation.Hold,
    After: Extrapolation.Hold)

x = Sample(track, Duration(Seconds: 0.5))   // ≈ 5
```

### Named tracks and clips

A clip bundles typed channels under string names the consumer resolves (property path,
bone name, …):

```
type NamedTrack<T> { Name: String; Track: AnimationTrack<T>; }

type AnimationClip
{
    Name: String;
    Duration: Duration;
    Events: Array<AnimationEvent>;
    NumberTracks: Array<NamedTrack<Number>>;
    Vector2DTracks: Array<NamedTrack<Vector2D>>;
    Vector3DTracks: Array<NamedTrack<Vector3D>>;
    QuaternionTracks: Array<NamedTrack<Quaternion>>;
    ColorTracks: Array<NamedTrack<Color>>;
    TransformTracks: Array<NamedTrack<Transform3D>>;
}
```

Per-channel TRS helpers avoid packing everything into one `Transform3D` key when axes
should ease independently:

```
type TransformTrack3D
{
    Translation: AnimationTrack<Vector3D>;
    Rotation: AnimationTrack<Quaternion>;
    Scale: AnimationTrack<Number3>;
}
```

The doc comment states that rotation keys use spherical interpolation — important,
because component-wise lerp of `Quaternion` fields is wrong.

### Layering, machines, blend spaces

Beyond a single clip, v3 declares:

- `AnimationLayer` — weighted clip contribution, optional additive mode
- `AnimationPlayhead` — time, speed, direction, playing flag
- `AnimationStateMachine` / `AnimationTransition` — clip graph with cross-fades
- `BlendSpace1D` / `BlendSpace2D` — parameter-driven blends (walk speed, strafe)

```
layer = AnimationLayer(clip, Proportion(Value: 0.5), false)
play  = AnimationPlayhead(Duration(Seconds: 0.2), 1, PlaybackDirection.Forward, true)
```

## Pitfalls / fine print

**Keys must be time-ordered.** The type comment requires ascending `Time`. A sort is a
loader concern; a track with unsorted keys has undefined sample behavior.

**Easing vs tangents.** If `Interpolation` is `Bezier` or `Hermite`, the key's
`Easing` field is ignored for shaping — tangents own the curve. Mixing both mental
models in one segment is a common authoring bug.

**Quaternion tracks need slerp (or squad), not lerp.** `AnimationTrack<Quaternion>`
looks identical to a number track in the type system. The spherical rule lives in the
`TransformTrack3D` comment and in player semantics — it is not a distinct
`KeyInterpolation` case today.

**LoopWithOffset accumulates.** Use it for cyclic locomotion root motion. Using plain
`Loop` on a root-translation track teleports the character each cycle.

**Clip Duration vs last key.** Tracks whose keys extend past `AnimationClip.Duration`
are clipped by the player. Author duration to cover the last key, or accept truncation.

**Additive layers.** When `Additive` is true, values add atop lower layers instead of
lerping toward them. Scales and quaternions need careful additive conventions
(often "delta from bind"); the type does not encode that convention.

## Try it

<details>
<summary>Exercise 1 — Hold vs Linear</summary>

Keys at $t=0$ value 0 and $t=1$ value 10, both with `KeyInterpolation.Constant`. What
does `Sample` return at $t=0.9$?

**Answer.** 0 — Constant holds the left key's value until the next key's time.
</details>

<details>
<summary>Exercise 2 — Extrapolation</summary>

A track's last key is at $t=2$ with value 5. `After` is `Hold`. What is the sample at
$t=10$?

**Answer.** 5 — Hold clamps to the boundary key.
</details>

<details>
<summary>Exercise 3 — Which channel type</summary>

You need to animate a light's tint through orange → white → blue. Which
`AnimationClip` array field holds that track?

**Answer.** `ColorTracks` — a `NamedTrack<Color>`.
</details>

## Library recommendations

- **missing-function** — `37-keyframes-tracks.plato`: `KeyInterpolation` has a `Spring`
  case, but there is no link to `SpringParameters` from `36-easing.plato`. A spring
  segment needs stiffness/damping/mass (or a half-life); without parameters on the key
  or track, every player invents its own defaults.

- **doc-comment / naming** — `37-keyframes-tracks.plato`: `TransformTrack3D` documents
  spherical rotation interpolation, yet `AnimationTrack<Quaternion>` and
  `KeyInterpolation.Linear` still read as "lerp the value." Add an explicit
  `KeyInterpolation` case (`Spherical` / `Slerp`) or harden the Quaternion-track doc so
  `Linear` *means* slerp when `T = Quaternion`.

- **wrong-shape** — `37-keyframes-tracks.plato`: `Keyframe<T>.Easing` is always present,
  including when `Interpolation` is `Bezier`, `Hermite`, or `Constant`, where easing is
  irrelevant. A sum (`SimpleKey` vs `EasedKey` vs `TangentKey`) would make illegal
  combinations unrepresentable.

- **missing-function** — `37-keyframes-tracks.plato`: no declared
  `Evaluate(clip, time): …` or pose-sampling helper that resolves named tracks together.
  `TimeVarying.Sample` covers one track; clip-level evaluation is what every runtime
  actually calls.
