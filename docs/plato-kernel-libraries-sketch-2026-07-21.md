# Plato kernel libraries: motion, effects, style, layout

*Design sketch, 2026-07-21. Companion to plato-076 (Gratify→Plato kernel split),
studio-074 (Gratify→C# port), and the 2026-07-21 notes on both. Sketches a family
of small, pure Plato libraries that would sit BENEATH Gratify (and any other 2D/3D
consumer), authored once and emitted to both TypeScript and C#. Syntax follows
stdlib-legacy house style (verified against curves.plato, colors.plato,
core.interfaces.plato). Function bodies are indicative, not final.*

## Design principle

Define everything over **time, unit intervals, and interpolatable values** — never
over widgets. The boundary rule: Plato takes anything expressible as *"values in,
values out, no names as strings."* Gratify's channel stepper, theme fade, and
layout containers become thin consumers; Studio's heat-maps, camera moves, and
gizmo animation consume the same source.

What stays OUT (host-side, per plato-076): channels-by-name, parts/facets,
Element trees, Tokens-with-mix-method, event routing, the reconciler.

The generality lever: the stdlib's existing `IInterpolatable` /
`IScalarArithmetic` / `IAdditive` interfaces (core.interfaces.plato) already carve
the right seam — scalar cores lift to `Vector2`, `Vector3`, `Color`, `Angle` for
free. Same three motion functions animate a hover glow, a 2D knob, a 3D camera,
a gizmo handle.

## 1. motion.plato — time-domain math over interpolatable values

```plato
// A spring is a VALUE — stepping returns a new spring. No mutation.
type Spring
{
    Value: Number;
    Velocity: Number;
    Stiffness: Number;
    Damping: Number;
}

library Motion
{
    // Semi-implicit Euler; matches gratify core/spring.ts semantics.
    Step(s: Spring, target: Number, dt: Number): Spring => ...;

    // Exponential approach — no overshoot (Gratify "rate" channels, camera glides)
    Approach(current: Number, target: Number, rate: Number, dt: Number): Number
        => target + (current - target) * (-rate * dt).Exp;

    // Impulse decay (Gratify "kick" channels, hit flashes, shake energy)
    Decay(value: Number, rate: Number, dt: Number): Number
        => value * (-rate * dt).Exp;
}
```

`Approach`/`Decay` lift to any `IInterpolatable + IScalarArithmetic` type
(Vector2/3, Color, Angle). Spring lifts per-component or via a generic
`Spring<T>` if the monomorphizer cooperates (spike question, shared with
plato-076's delegate-fields spike).

```plato
// Easing: unit interval in, unit interval out. Slots in beside curves.plato
// (which already has SineWave, Staircase*, etc.).
library Easing
{
    SmoothStep(t: Number): Number => t * t * (3.0 - 2.0 * t);
    InOutCubic(t: Number): Number => ...;
    OutBack(t: Number, overshoot: Number): Number => ...;
    // Periodic motion as pure functions of a clock (Gratify node.time, idle anims)
    Pulse(t: Number, frequency: Number): Number => ...;
    PingPong(t: Number, period: Number): Number => ...;
}

// Keyframes: ease stored as a function VALUE (fields.plato precedent —
// no sum type needed for "which easing").
type Keyframe { Time: Number; Value: Number; Ease: Function1<Number, Number>; }
library Timeline { Eval(frames: IArray<Keyframe>, t: Number): Number => ...; }
```

## 2. effects.plato — particles and juice as pure step functions

```plato
type Particle
{
    Position: Vector3;    // 3D native; 2D = z zero. One library, both.
    Velocity: Vector3;
    Age: Number;
    Life: Number;
}

library Effects
{
    Step(p: Particle, gravity: Vector3, drag: Number, dt: Number): Particle => ...;
    Alive(p: Particle): Boolean => p.Age < p.Life;
    Fade(p: Particle): Number => 1.0 - (p.Age / p.Life).SmoothStep;

    // Procedural juice: pure functions of time; host adds to any transform.
    Shake(t: Number, amplitude: Number, frequency: Number, seed: Number): Vector2 => ...;
    Bounce(t: Number, height: Number, squash: Number): Vector2 => ...;
}
```

Host owns spawn lists and rendering; Plato owns every per-frame formula.

## 3. style.plato — color arithmetic, palettes, ramps

```plato
library ColorOps
{
    // Perceptual mix (OKLab) — better than RGB lerp for UI AND heat-maps.
    Mix(a: Color, b: Color, t: Number): Color => ...;
    // The gratify style.ts `surface` recipe formula, widget-free:
    Emphasis(base: Color, tint: Color, hover: Number, press: Number): Color
        => base.Mix(tint, 0.18 + 0.32 * hover + 0.4 * press);
    WithAlpha(c: Color, a: Number): Color => ...;
    Contrast(c: Color): Color => ...;    // readable text on c
}

// Gradient = color stops; UI fills, data-viz ramps, 3D vertex color.
type ColorStop { T: Number; Value: Color; }
library Gradients { Eval(stops: IArray<ColorStop>, t: Number): Color => ...; }

// KEY FINDING: a palette is a FIXED-FIELD record — Plato handles it fine.
// Gratify's Tokens problem was the stored mix function + string lookup,
// not the palette itself.
type Palette
{
    Bg: Color; Surface: Color; SurfaceHi: Color; Muted: Color;
    Text: Color; TextDim: Color; TextBright: Color;
    Accent: Color; Accent2: Color; Danger: Color;
}
library Palettes
{
    Lerp(a: Palette, b: Palette, t: Number): Palette => ...;  // theme cross-fade
    Derive(base: Palette, accent: Color): Palette => ...;     // theme from seed color
}
```

Gratify's `tickTheme` fade becomes `Approach` over `Palette` — the whole
choreographed theme swap is two library calls.

## 4. layout.plato — arranging boxes and points

```plato
type PackResult { Offsets: IArray<Vector2>; Size: Vector2; }

library Packing
{
    // Gratify Flow (containers.ts packRows) — also texture atlases, sprite
    // sheets, tag clouds.
    PackRows(sizes: IArray<Vector2>, width: Number, gap: Number): PackResult => ...;
    // Stack/Row cores. Trick: align/justify as Number 0..1 (0=start, 0.5=center,
    // 1=end) — kills the enum, dodges the sum-type gap (plato-077), and is
    // strictly richer than the TS union (0.3 works).
    PackAxis(sizes: IArray<Vector2>, axis: Integer, gap: Number, align: Number): PackResult => ...;
    PackGrid(count: Integer, cell: Vector2, columns: Integer, gap: Number): PackResult => ...;
    // Points on shapes — radial menus, node-editor auto-layout, billboard rings.
    OnCircle(count: Integer, radius: Number): IArray<Vector2> => ...;
    Distribute(sizes: IArray<Number>, avail: Number, gap: Number): IArray<Number> => ...;
}

library RectOps   // extends bounds.plato vocabulary
{
    Inset(r: Bounds2D, pad: Number): Bounds2D => ...;
    AlignIn(inner: Vector2, outer: Bounds2D, ax: Number, ay: Number): Bounds2D => ...;
    SplitX(r: Bounds2D, fraction: Number, gap: Number): IArray<Bounds2D> => ...;  // split-pane
}
```

## Supporting libraries (gaps to fill)

- **unit.plato** (or grow interval.plato): `Remap`, `Clamp01`, `SmoothStep`,
  `Fraction(value, lo, hi)` — connective tissue for everything above; half
  exists already.
- **random.plato** — deterministic hash-based noise (`Hash(seed): Number`,
  `Jitter(i, seed): Vector2`) for particles, shake, scatter layouts. Likely the
  biggest true stdlib gap; pure-function form (hash of inputs, no state) fits
  Plato exactly.
- **colors.plato additions** — OKLab conversion (perceptual Mix needs it);
  colors.plato already has LUV/other spaces, so precedent exists.
- curves.plato — easing belongs beside the existing wave/staircase functions.

## Downstream payoff

- Gratify TS deletes `core/` (spring/curve/vec/rect/color) + the recipe formulas
  + `packRows`, imports emitted TS instead.
- The studio-074 C# port starts with its entire math layer already written and
  reviewed.
- Studio gets gradients/motion for heat-maps, camera moves, and gizmo/L2 element
  animation from the same source.
- Plato gains a non-geometry consumer for the TS writer (plato-078's missing
  second customer).

## Prerequisites / risks

- TS writer revival (plato-078) gates all TS-side payoff.
- Generic `Spring<T>` / function-valued `Keyframe.Ease` must survive the
  TIR/monomorphizer pipeline — same spike plato-076 already calls for.
- OKLab + hash/noise are new stdlib content — small but real scope beyond
  transcription.
