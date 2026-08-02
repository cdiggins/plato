---
lesson: spring-damping-critical
title: Critical Damping and Spring Motion
domain: Animation & easing
v3-files: [36-easing.plato, 39-motion-graphics.plato]
audience: Comfortable with second-order ODEs at a high level, or with "springy" UI animation; no physics engine required.
status: draft-v1
---

# Critical Damping and Spring Motion

UI motion often feels best when a value rushes toward a target quickly, settles without
a long bounce, and never quite oscillates forever. That feel is a **damped spring**. The
sweet spot with the fastest non-oscillating settle is **critical damping**.

Plato names the three tuning knobs as `SpringParameters` in `36-easing.plato`, and
related decay shows up in motion-graphics types such as `CameraShake`. This lesson is
about the critical-damping identity, how under- and over-damped regimes feel, and how
those parameters sit next to classic easings.

## The second-order model

A one-dimensional spring toward target $x^\star$ (take $x^\star = 0$ by shifting
coordinates):

$$
m\,\ddot{x} + c\,\dot{x} + k\,x = 0
$$

| Symbol | Plato field | Role |
|--------|-------------|------|
| $m$ | `Mass` | Inertia — larger mass slows response |
| $c$ | `Damping` | Energy loss — fights velocity |
| $k$ | `Stiffness` | Restoring force — higher $k$ snaps harder |

Plato's doc comment is explicit: these are **unitless animation tuning values**, not SI
kilograms and newtons. The same ratios still define the regimes.

Define the damping ratio:

$$
\zeta = \frac{c}{2\sqrt{km}}
$$

| Regime | Condition | Motion |
|--------|-----------|--------|
| Underdamped | $\zeta < 1$ | Oscillates while decaying |
| Critically damped | $\zeta = 1$ | Fastest settle **without** overshoot oscillation |
| Overdamped | $\zeta > 1$ | No oscillation, but slower settle |

Critical damping means:

$$
c = 2\sqrt{k m}
$$

That is exactly the sentence on `SpringParameters`:

> The spring is critically damped (no oscillation, fastest settle) when Damping equals
> twice the square root of Stiffness times Mass.

```
  x
  │   underdamped ~\/\/\/\____
  │
  │   critical    \_________
  │
  │   overdamped  \______
  └──────────────────────── t
```

### Worked numbers

`Stiffness = 100`, `Mass = 1` → critical `Damping = 2 * sqrt(100 * 1) = 20`.

| Damping | $\zeta$ | Feel |
|---------|---------|------|
| 10 | 0.5 | Bouncy, several wobbles |
| 20 | 1.0 | Snappy settle, no ring |
| 40 | 2.0 | Heavy, sluggish approach |

Doubling stiffness to $400$ with mass $1$ raises critical damping to $40$. Keep $\zeta$
fixed if you want the same *character* at a different stiffness.

## In Plato

```plato
type SpringParameters
    implements Value
{
    Stiffness: Number;
    Damping: Number;
    Mass: Number;
}
```

Construction:

```
var critical = SpringParameters(100.0, 20.0, 1.0);
var bouncy   = SpringParameters(100.0, 8.0, 1.0);
var heavy    = SpringParameters(100.0, 50.0, 1.0);
```

`SpringParameters` lives beside other parameterized easings — it is not itself an
`EasingFunction`. Classic easings map normalized time to progress; a spring is usually
integrated in time against a moving target (or solved in closed form for fixed targets).

```plato
interface EasingFunction
    inherits Procedural<Number, Number>
{ }

type ClassicEasing
    = Linear
    | Eased(Family: EasingFamily, Phase: EasingPhase);

type ElasticParameters
    implements Value
{
    Amplitude: Number;
    Period: Number;
}
```

`Elastic` easings *imitate* oscillation with amplitude and period — they are not the
same ODE. Use `ElasticParameters` when you want a fixed-duration ease that wiggles;
use `SpringParameters` when the motion should respond to a live target (gesture release,
follow cursor, settle after drag).

### Motion-graphics damping cousins

`39-motion-graphics.plato` uses "Damping" in a related but narrower sense:

```plato
type CameraShake
    implements Value
{
    Amplitude: Number;
    Frequency: Frequency;
    Seed: Integer;
    Damping: Number;
}
```

Here `Damping` is an **exponential decay rate per second** (0 shakes forever). It is not
$c$ in $m\ddot{x}+c\dot{x}+kx=0$, and there is no critical-damping formula on
`CameraShake`. Name collision across domains: read the field's doc comment.

Tweens still lean on classic easings:

```plato
type Tween<T>
    implements TimeVarying<T>
{
    From: T;
    To: T;
    Duration: Duration;
    Delay: Duration;
    Easing: ClassicEasing;
}
```

A spring-driven tween would substitute a spring solver for `Easing` — that composition
is application logic until a dedicated spring-tween type exists.

### Practical tuning recipe

1. Pick `Mass = 1` unless you have a reason not to (reduces free variables).
2. Choose `Stiffness` for response speed (higher = snappier natural frequency
   $\omega_0 = \sqrt{k/m}$).
3. Set `Damping = 2 * Sqrt(Stiffness * Mass)` for critical.
4. Nudge damping down 10–20% if you want a hint of overshoot; nudge up if you see ring.

Natural period of the undamped oscillator: $T = 2\pi\sqrt{m/k}$. Critical damping does
not oscillate, but that period still informs how "tight" the spring feels.

## Pitfalls and fine print

**Forgetting the factor 2.** Using $c = \sqrt{km}$ underdamps ($\zeta = 1/2$).

**Zero mass.** `Mass = 0` breaks the ODE and the critical formula. Keep mass positive.

**Unit theater.** Do not convert animation `Mass` from kilograms unless your integrator
and pixels share a real physical model — Plato marks them unitless for a reason.

**Elastic ≠ spring.** `EasingFamily.Elastic` with `ElasticParameters` is a canned curve
over $[0,1]$ time. It will not track a moving target the way a spring does.

**CameraShake.Damping.** Exponential envelope rate, not spring $c$. Do not apply
$2\sqrt{km}$ there.

**Frame-rate integration.** Explicit Euler on stiff springs ($k$ huge) goes unstable.
Prefer analytic critical/underdamped solutions or semi-implicit integrators when
`Stiffness` is large.

## Try it

<details>
<summary>Exercise 1 — Critical value</summary>

`Stiffness = 49`, `Mass = 1`. What `Damping` is critical?

**Answer.** $2\sqrt{49\cdot 1} = 14$.
</details>

<details>
<summary>Exercise 2 — Regime</summary>

`SpringParameters(81.0, 9.0, 1.0)`. Under, critical, or over?

**Answer.** $2\sqrt{81} = 18$, actual $c=9$ → $\zeta = 0.5$ → underdamped.
</details>

<details>
<summary>Exercise 3 — Same ζ, new k</summary>

You liked $\zeta = 1$ at $k=100$, $m=1$. You change to $k=400$. New critical damping?

**Answer.** $2\sqrt{400} = 40$.
</details>

## Library recommendations

- **missing-function** — `36-easing.plato`: no declared
  `CriticalDamping(stiffness, mass): Number` or
  `SpringParameters.Critical(stiffness, mass)` factory. The doc states the formula; the
  API should compute it so callers do not mistype the 2.

- **missing-function** — no `DampingRatio(params): Number` helper. Debugging feel is
  much easier in $\zeta$ space than raw $c$.

- **doc-comment** — `CameraShake.Damping` and `SpringParameters.Damping` share an English
  name with different meanings. Cross-file disambiguation in both comments would cut
  confusion (without requiring readers to open motion-graphics when tuning springs).

- **pedagogy** — `SpringParameters` does not implement `EasingFunction` and has no
  `Eval` story in the easing file. A short banner note — "integrate against a target;
  not a normalized ease" — would stop authors from plugging it into `Tween.Easing`.
