---
lesson: easing-functions
title: Easing Functions
domain: Animation & motion
v3-files: [36-easing.plato]
audience: Comfortable with functions of one variable and basic animation (from/to over time).
status: draft-v1
---

# Easing Functions

A door that opens at constant angular speed looks mechanical. A UI panel that slides in at
constant velocity feels cheap. Real motion — and good-looking fake motion — spends more time
near the start and end of a move than in the middle. **Easing** is the name for reshaping
normalized time so that progress accelerates, decelerates, overshoots, or bounces instead of
ticking linearly from 0 to 1.

An easing is a pure map $e: [0,1] \to \mathbb{R}$ with $e(0)=0$ and $e(1)=1$. Intermediate
values usually stay in $[0,1]$, but the Back and Elastic families intentionally leave that
interval — anticipation and overshoot are features, not bugs.

## The idea

Animation almost always has the shape

$$
\text{value}(t) = (1 - u)\, a + u\, b, \qquad u = e\bigl(\tfrac{t - t_0}{T}\bigr)
$$

where $a$ and $b$ are endpoints, $T$ is duration, and $e$ is the easing. When $e(s)=s$,
progress is linear in time. Everything interesting lives in $e$.

### Phase: where the acceleration sits

Classic easings come in three **phases**:

| Phase | Behavior | Picture |
|-------|----------|---------|
| In | start slow, finish at full speed | `_/` |
| Out | start fast, settle to rest | `¯\` |
| InOut | accelerate then decelerate | `_/¯\` |

Out is the workhorse for UI: objects arrive and stop. In is useful when something *leaves*
(a menu that accelerates away). InOut is the default for camera moves and long transitions.

### Families: the polynomial ladder and friends

The **Quad / Cubic / Quart / Quint** ladder is successive powers. Roughly:

$$
e_{\text{In},n}(t) = t^{n+1}, \qquad
e_{\text{Out},n}(t) = 1 - (1-t)^{n+1}
$$

Higher $n$ means a sharper corner near the slow end. **Sine**, **Expo**, and **Circ** are
smooth alternatives with different curvature. **Back** pulls *past* 0 (or past 1) before
settling. **Elastic** oscillates while approaching the target. **Bounce** is a piecewise
quadratic that mimics a ball hitting the floor.

```
progress
  1 |           .----  Linear
    |         /
    |       /         EaseOutCubic
    |     /
    |   /.__
    |  /    `--·      EaseOutBack (overshoots past 1)
  0 +------------------> normalized time
    0                 1
```

### Why not just lerp?

Linear motion has discontinuous acceleration at the endpoints (an impulse). Human vision
is sensitive to that. Even a gentle cubic Out removes the jolt. Overshoot families trade
physical plausibility for emphasis — a bounce says "I arrived" louder than a smooth settle.

## In Plato

v3 puts the catalog in `36-easing.plato`. The abstract shape is the interface
`EasingFunction`, which inherits `Procedural<Number, Number>`:

```
interface EasingFunction
    inherits Procedural<Number, Number>
{ }
```

So every easing is evaluated as `Eval(easing, t)` with $t$ a unitless normalized time.

### Selecting a classic curve

The full Penner-style catalog is the sum type `ClassicEasing`:

```
type EasingFamily
    = Quad | Cubic | Quart | Quint | Sine | Expo | Circ | Back | Elastic | Bounce;

type EasingPhase = In | Out | InOut;

type ClassicEasing
    = Linear
    | Eased(Family: EasingFamily, Phase: EasingPhase);
```

Usage-shaped selection:

```
ease = ClassicEasing.Linear
ease = ClassicEasing.Eased(EasingFamily.Cubic, EasingPhase.Out)
ease = ClassicEasing.Eased(EasingFamily.Bounce, EasingPhase.Out)
```

`Linear` is a first-class case, not a fake family — there is no `EasingFamily.Linear`.

### Parameterized curves

Three records carry CSS / design-tool knobs that the classic catalog alone cannot express:

```
type CubicBezierEasing { P1: Number2; P2: Number2; }   // CSS cubic-bezier
type StepEasing        { Steps: Integer; Position: StepPosition; }
type SmoothstepEasing  { Order: Integer; }             // 0=linear, 1=cubic, 2=quintic
```

`CubicBezierEasing` pins endpoints at $(0,0)$ and $(1,1)$; `P1.X` and `P2.X` stay in
$[0,1]$ so the curve remains a *function* of time, while Y may overshoot. `StepPosition`
follows the CSS `steps()` model: `JumpStart`, `JumpEnd`, `JumpNone`, `JumpBoth`.

Shape knobs for the special classic families live as separate records:

```
type ElasticParameters { Amplitude: Number; Period: Number; }   // classic Period ≈ 0.3
type BackParameters    { Overshoot: Number; }                   // classic ≈ 1.70158
type BounceParameters  { Bounces: Integer; Restitution: Proportion; }
```

`SpringParameters` also lives in this file (stiffness / damping / mass) — it is the
physical cousin of easing, used when motion should react continuously rather than follow
a fixed $e(t)$ curve. It does **not** implement `EasingFunction`; springs are a different
sampling model.

### Usage-shaped evaluation

```
u = Eval(CubicBezierEasing(Number2(0.42, 0), Number2(0.58, 1)), t)  // ease-in-out CSS
u = Eval(SmoothstepEasing(1), t)                                     // 3t² − 2t³
u = Eval(StepEasing(4, StepPosition.JumpEnd), t)
```

v3 does **not** yet declare `Eval` on `ClassicEasing` itself — the sum selects a curve,
but the evaluation function that turns `Eased(Cubic, Out)` into a number is a library
gap (see recommendations). In illustrative code one still writes the intended shape:

```
// intended, once ClassicEasing implements EasingFunction:
u = Eval(ClassicEasing.Eased(EasingFamily.Cubic, EasingPhase.Out), t)
value = Lerp(from, to, u)
```

## Pitfalls / fine print

**Normalized time is not seconds.** Feed $t \in [0,1]$ into an easing. Feeding a
`Duration` or frame index produces nonsense. Convert first: $t = \Delta / T$.

**Overshoot leaves $[0,1]$.** If you clamp the eased parameter before interpolating
colors or angles, you silently destroy Back/Elastic character. Clamp the *result* only
when the domain truly cannot leave the interval (e.g. opacity).

**In and Out are not mirrors of each other in every family.** Bounce In is especially
surprising: it "bounces" while still near zero, which rarely matches author intent. Prefer
Bounce Out for landings.

**Steps are discontinuous.** `StepEasing` jumps; pairing it with spatial interpolation
that assumes continuity (splines, motion blur) can flicker.

**Smoothstep order.** `Order: 0` is linear; `1` is the familiar cubic smoothstep; `2` is
Ken Perlin's smootherstep. Negative orders are undefined — the doc says order $N$ yields
degree $2N+1$.

**Bezier X monotonicity.** If `P1.X` or `P2.X` leave $[0,1]$, $x(t)$ is no longer
invertible and the easing ceases to be a function of time. Enforce the invariant at
construction time.

## Try it

<details>
<summary>Exercise 1 — Read the phase</summary>

Which `ClassicEasing` matches "start at full speed, decelerate into the end"?

**Answer.** `ClassicEasing.Eased(EasingFamily.Cubic, EasingPhase.Out)` (family optional;
any Out phase works — Cubic is the usual default).
</details>

<details>
<summary>Exercise 2 — Predict overshoot</summary>

For `BackParameters(Overshoot: 1.70158)` with EaseOutBack, is $e(0.9)$ greater than,
equal to, or less than 1?

**Answer.** Greater than 1. Out-Back overshoots past the target before settling to 1 at
$t=1$. The classic overshoot is about 10%.
</details>

<details>
<summary>Exercise 3 — Smoothstep at the midpoint</summary>

For cubic smoothstep $s(t) = 3t^2 - 2t^3$, compute $s(0.5)$.

**Answer.** $3(0.25) - 2(0.125) = 0.75 - 0.25 = 0.5$. The midpoint is fixed; the
derivatives at both ends are zero, which is why it feels soft.
</details>

## Library recommendations

- **missing-function** — `36-easing.plato`: `ClassicEasing` does not implement
  `EasingFunction`, so there is no declared `Eval(ClassicEasing, Number)`. The catalog
  sum is useless for sampling until a library function (or interface implementation) maps
  each `Eased(family, phase)` case to a curve. Teaching forces this gap into the open.

- **wrong-shape** — `36-easing.plato`: `ElasticParameters`, `BackParameters`, and
  `BounceParameters` are orphaned records — `ClassicEasing.Eased` carries only family and
  phase, with no slot for the classic amplitude/period/overshoot knobs. Either add
  parameterized sum cases (`ElasticEased(Phase, ElasticParameters)`, …) or document that
  those records are only for a future `EvalClassic(…, params)` overload.

- **missing-interface** — `36-easing.plato`: `SpringParameters` sits in the easing file but
  implements neither `EasingFunction` nor `TimeVarying`. A spring is not an $e(t)$ map;
  it needs state (position, velocity). Either move it beside motion-integration types or
  declare a `SpringMotion` / `TimeVarying` wrapper so the file's role is clear.

- **doc-comment** — `36-easing.plato`: `SmoothstepEasing.Order` should state the closed
  form for orders 0–2 explicitly (`t`, `3t²−2t³`, `6t⁵−15t⁴+10t³`) so implementers and
  teachers share one reference without hunting external sources.
