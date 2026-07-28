---
lesson: springs-and-procedural-motion
title: Springs and Procedural Motion
domain: Animation & motion
v3-files: [36-easing.plato, 39-motion-graphics.plato]
audience: Comfortable with basic animation timing; high-school physics of springs is enough.
status: draft-v1
---

# Springs and Procedural Motion

Tweens and keyframe tracks excel when you know the destination ahead of time. They fail
when the target *moves* while you are still in flight — a camera following a player, a
toggle that reverses mid-ease, a tooltip that tracks the cursor. Restarting a tween on
every target change feels sticky. A **spring** does not schedule an $e(t)$ curve; it
integrates a force toward the current target, so mid-flight retargeting is free.

Alongside springs, motion-graphics work leans on **procedural** signals: oscillators,
wiggles, camera shake — motion that is generated, not keyed.

## The idea

### Second-order spring

A mass-spring-damper toward target $x^*$:

$$
\ddot{x} + \frac{c}{m}\dot{x} + \frac{k}{m}(x - x^*) = 0
$$

with stiffness $k$, damping $c$, mass $m$. Critical damping (fastest settle without
oscillation) occurs at

$$
c = 2\sqrt{k m}.
$$

Underdamped ($c$ smaller) overshoots and rings. Overdamped ($c$ larger) creeps. Animation
springs almost always use **unitless** $k,c,m$ tuned for feel, not kilograms and
newtons — the ratios matter, not the absolute scale.

```
x
  |    * target jumps
  |   / \
  |  /   \___ underdamped
  | /         `---·
  |/________________\____ critically damped
  +-----------------------> time
```

### Why springs beat tweens for reactive motion

| Tween / ease | Spring |
|--------------|--------|
| Fixed duration | Duration emerges from parameters |
| Restart on retarget | Continuously chases new target |
| State = time along curve | State = position + velocity |
| Great for scripted beats | Great for follow, UI, cameras |

### Procedural companions

Not everything needs a spring. Periodic bobbing is an **oscillator**. Hand-held
nervousness is **wiggle** (layered noise). Impact feedback is **camera shake** with
exponential decay. These share a theme: `Sample(time) → value` without authored keys.

## In Plato

### Spring parameters (`36-easing.plato`)

```
type SpringParameters
{
    Stiffness: Number;
    Damping: Number;
    Mass: Number;
}
```

All three are unitless tuning knobs. The doc comment states the critical-damping rule
explicitly — use it when you want settle-without-wobble.

There is **no** declared spring state type and **no** `Step(spring, state, target, dt)`
in v3. The parameters alone configure a simulator that a later library must provide.
When keyframe tracks select `KeyInterpolation.Spring`, these are the natural knobs —
but the link is not typed yet.

### Tweens still matter (`39-motion-graphics.plato`)

```
type Tween<T>
{
    From: T;
    To: T;
    Duration: Duration;
    Delay: Duration;
    Easing: ClassicEasing;
}
```

`Tween<T>` implements `TimeVarying<T>`: before the delay you get `From`; after
`Delay + Duration` you get `To`; in between, ease then interpolate. Springs *replace*
tweens when the endpoint moves; they do not obsolete scripted transitions.

### Oscillators

```
type OscillatorWaveform = Sine | Square | Triangle | Sawtooth | Noise;

type Oscillator
{
    Waveform: OscillatorWaveform;
    Amplitude: Number;
    Frequency: Frequency;     // Hertz quantity
    Phase: Angle;
    Bias: Number;
}
```

Evaluates as $\mathrm{Bias} + \mathrm{Amplitude}\cdot\mathrm{wave}(f\,t + \phi)$.
`Frequency` is a quantity type (`Hertz`), not a bare number — that prevents mixing
"cycles per second" with "radians per second" by accident.

```
bob = Oscillator(OscillatorWaveform.Sine, 0.05, Frequency(Hertz: 2), Angle(Radians: 0), 0)
y   = Sample(bob, time)   // TimeVarying<Number>
```

### Wiggle and shake

```
type WiggleMotion
{
    Frequency: Frequency;
    Octaves: Integer;
    Amplitude: Number;
    Persistence: Number;
    Seed: Integer;
}

type CameraShake
{
    Amplitude: Number;
    Frequency: Frequency;
    Seed: Integer;
    Damping: Number;          // exponential decay per second; 0 = forever
}
```

Equal seeds replay identical motion — essential for deterministic takes. `CameraShake`
damping is a **decay rate**, not the spring's viscous coefficient; same English word,
different model.

### Paths, stagger, time remap

Procedural placement along curves:

```
type MotionPath2D { Path: Path2D; OrientToPath: Boolean; StartOffset: Proportion; }
type MotionPath3D { Poses: Array<Pose3D>; OrientToPath: Boolean; StartOffset: Proportion; }
```

Choreography helpers — `Stagger`, `Timeline`, `MotionRepeater2D`, `MotionEcho`,
`BeatSync`, `TimeRemap` — schedule and warp time without hand-keying every layer.
`PlaybackCycle` (`Once | Loop | PingPong | Hold`) describes what happens when a finite
animation finishes.

`LayerTransform2D` packs the After-Effects layer kit: anchor, position, rotation, scale,
opacity — the usual spring/wiggle targets in 2D motion design.

## Pitfalls / fine print

**Springs need state.** Storing only `SpringParameters` is not enough to resume motion.
You need at least current value and velocity (and the current target). v3 leaves that
state type undeclared — do not pretend `SpringParameters` is sampleable like an easing.

**Critical damping ≠ critically good.** UI often wants a *hint* of overshoot. Slightly
underdamped reads as lively; critical can feel soft-dead.

**Frame-rate dependence.** Naive integrators change feel when `dt` varies. Prefer a
fixed-step spring update or an analytic integrator for second-order linear springs.

**Wiggle is not physics.** Octaves of noise look organic but do not conserve momentum.
Do not mix wiggle displacement with rigid-body state without a clear ownership rule.

**Shake damping units.** `CameraShake.Damping` is per second. A value tuned at 60 fps
with a per-frame multiply is a different quantity — convert carefully.

**OrientToPath singularities.** At zero-length path segments the tangent vanishes;
orientation becomes undefined. Guard or fall back to the previous orientation.

## Try it

<details>
<summary>Exercise 1 — Critical damping</summary>

`Stiffness = 100`, `Mass = 1`. What `Damping` is critical?

**Answer.** $c = 2\sqrt{100\cdot 1} = 20$.
</details>

<details>
<summary>Exercise 2 — Tween vs spring</summary>

A button's hover target flips every 50 ms while the pointer jitters across the edge.
Which model tracks the intended state with less visual thrashing: restarting a 200 ms
cubic Out tween, or a critically damped spring?

**Answer.** The spring — it continuously pursues the latest target without restarting a
fixed-duration curve on every flip.
</details>

<details>
<summary>Exercise 3 — Oscillator bias</summary>

`Oscillator` with `Amplitude: 1`, `Bias: 2`, sine wave. What range does the signal span?

**Answer.** $[1, 3]$ — bias recenters the $\pm\mathrm{Amplitude}$ swing.
</details>

## Library recommendations

- **missing-type** — `36-easing.plato`: declare something like
  `SpringState<T> { Value: T; Velocity: T; }` (or a non-generic scalar/vector pair) plus
  `Step(params, state, target, dt)`. Parameters without state cannot teach or implement
  reactive springs.

- **missing-function** — `37-keyframes-tracks.plato` / `36-easing.plato`:
  `KeyInterpolation.Spring` has no typed association with `SpringParameters`. Add
  parameters on the key/track or a documented default constructor so players share one
  feel.

- **naming / doc-comment** — `39-motion-graphics.plato`: `CameraShake.Damping` vs
  `SpringParameters.Damping` share a name with different meanings (exponential decay rate
  vs viscous coefficient). Disambiguate in comments (`DecayRatePerSecond`) or rename one
  field to prevent copy-paste tuning bugs.

- **pedagogy** — `39-motion-graphics.plato`: `Oscillator` implements `TimeVarying<Number>`,
  but `WiggleMotion` and `CameraShake` do not. Either give them `Sample` via
  `TimeVarying` (possibly vector-valued) or document why they are inert parameter bags —
  the asymmetry confuses readers comparing procedural tools.
