---
lesson: time-is-not-a-number
title: Time Is Not a Number
domain: Foundations & vectors
v3-files: [07-time.plato, 11-points.plato]
audience: High-school math and general programming background
status: draft-v1
---

# Time Is Not a Number

"What time is it?" and "How long did it take?" both involve seconds, but they
are not the same kind of value. One is a position on a timeline. The other is
a span between two positions. Treat them as the same `double` and you will
eventually add two timestamps, compare a duration to a clock reading, or
lerp across midnight the wrong way.

This is the same distinction geometry makes between points and vectors —
played out on a one-dimensional line called time.

## The idea

An **instant** is a point on a continuous timeline. A **duration** is the
displacement between two instants. The legal operations mirror affine
geometry:

| Operation | Result | Meaning |
|-----------|--------|---------|
| Instant − Instant | Duration | How long between them |
| Instant + Duration | Instant | Move forward/back along the line |
| Duration + Duration | Duration | Combine spans |
| Instant + Instant | — | Undefined |

```
  timeline  ----*----------*----------*---->
                t0         t1         t2
                 \____ d ____/
                   Duration

  Instant is a location; Duration is a gap.
```

In space, `Point3D` minus `Point3D` yields `Vector3D`. In time, `Instant`
minus `Instant` yields `Duration`. The pattern is the `Difference` interface:
a position-like type whose delta is a separate type.

Wall-clock APIs muddy this by storing both as "seconds since epoch" floats.
The storage can look identical; the *meaning* must not. Frame indices, SMPTE
timecode, and musical beats are further coordinates on related — but not
identical — lines, each with their own conversion into continuous seconds.

## In Plato

From `07-time.plato`:

```plato
// A span of time, stored in seconds. The delta type of Instant.
type Duration
    implements Quantity
{
    Seconds: Number;
}

// A position on a continuous time line, in seconds relative to an arbitrary epoch.
// Subtracting two instants yields a Duration.
type Instant
    implements Value, Comparable, Difference<Duration>
{
    SecondsSinceEpoch: Number;
}
```

`Duration` is a `Quantity` (canonical field `Seconds`). `Instant` is not a
quantity — it is a position implementing `Difference<Duration>`, exactly as
`Point3D` implements `Difference<Vector3D>` in `11-points.plato`:

```plato
type Point3D
    implements Coordinate, Difference<Vector3D>, Hashable
{
    X: Number;
    Y: Number;
    Z: Number;
}
```

The affine analogy:

| Space (`11-points`) | Time (`07-time`) |
|---------------------|------------------|
| `Point3D` | `Instant` |
| `Vector3D` | `Duration` |
| `Between(a, b)` | displacement from `b` to `a` as `Duration` |
| `Add(p, v)` | `Add(instant, duration)` |

A half-open span of instants is its own type:

```plato
type TimeInterval
{
    Start: Instant;
    End: Instant;
}
```

Animation and media add discrete and musical coordinates:

```plato
type FrameRate { FramesPerSecond: Number; }

type FrameTime
    implements Value, Comparable
{
    Frame: Integer;
    Rate: FrameRate;
}

type Timecode
{
    Hours: Integer;
    Minutes: Integer;
    Seconds: Integer;
    Frames: Integer;
    Rate: FrameRate;
}

type Tempo { BeatsPerMinute: Number; }

type BeatTime
    implements Value, Comparable
{
    Beats: Number;
}
```

`FrameTime` is a discrete position under a rate — not a duration, not a wall
instant, until you convert. `BeatTime` plus `Tempo` reaches seconds; neither
alone is a wall-clock reading.

Usage-shaped snippets:

```plato
let t0 = Instant { SecondsSinceEpoch: 1000.0 };
let t1 = Instant { SecondsSinceEpoch: 1001.5 };

// Instant − Instant → Duration (via Difference)
let dt = Between(t1, t0);   // Duration { Seconds: 1.5 }

// Instant + Duration → Instant
let t2 = Add(t0, Duration { Seconds: 0.5 });

// Duration is a Quantity: scale and compare spans
let doubleSpan = dt.Multiply(2.0);

// Media coordinates stay distinct until converted
let rate = FrameRate { FramesPerSecond: 24.0 };
let frame = FrameTime { Frame: 48, Rate: rate };
let clip = TimeInterval {
    Start: t0,
    End: t1
};
```

What you cannot do at the type level: `Add(t0, t1)` — there is no
instant+instant. That is the whole point.

## Pitfalls / fine print

**Epoch is arbitrary.** `SecondsSinceEpoch` does not imply Unix epoch unless
your program says so. Two instants are only comparable inside one agreed
timeline.

**Duration is not Instant with a different name.** Both may be stored as
`Number` seconds. Mixing them in APIs ("pass time") recreates the Climate
Orbiter problem in temporal clothing.

**Frame rates are part of the value.** A bare frame index without `FrameRate`
is ambiguous (24 vs 30 vs 59.94). `FrameTime` bundles them for a reason.
Drop-frame timecode and NTSC quirks still need care beyond the type.

**Half-open intervals.** `TimeInterval` contains `Start` and excludes `End`.
Adjacent clips should abut without double-covering the boundary frame or
sample.

**Musical time ≠ wall time.** `BeatTime` needs `Tempo` (and often tempo
automation) to become `Duration`. Do not treat beats as seconds.

**Floating-point instants.** Long-running simulations that store absolute
`SecondsSinceEpoch` with huge magnitudes lose sub-millisecond precision. Prefer
a local epoch or fixed-point policies when accuracy matters.

**Comparable instants, quantity durations.** You can order instants on the
line. Durations compare as quantities (which span is longer). Do not compare
an `Instant` to a `Duration`.

## Try it

1. Instants $t_a = 10\,\mathrm{s}$ and $t_b = 13\,\mathrm{s}$ since epoch.
   What is `Between(t_b, t_a)` as a `Duration`?
2. Why is `Add(t_a, Between(t_b, t_a))` equal to $t_b$?
3. A `FrameTime` of frame 48 at 24 fps is how many seconds after frame 0
   (ignoring drop-frame)?

<details>
<summary>Answers</summary>

1. $13 - 10 = 3$ seconds as a `Duration`.
2. Same affine identity as points: adding the displacement from $t_a$ to $t_b$
   lands at $t_b$. `Difference` encodes that.
3. $48 / 24 = 2$ seconds of timeline under that rate.

</details>

## Library recommendations

- **missing-function** — `07-time.plato`: `Instant` implements
  `Difference<Duration>`, but unlike points in the `Transforms` library there
  is no local library block here showing `Add` / `Between` / `Lerp` bodies for
  instants. Declaring and documenting those operations next to the types (or
  a `library Time`) would make the affine story as concrete as `Point3D`.

- **missing-function** — `07-time.plato`: no conversions
  `ToDuration(frame: FrameTime): Duration`, `ToInstant(...)`, or
  `ToDuration(beats: BeatTime, tempo: Tempo): Duration`. The types exist; the
  bridges into continuous seconds are what every animation/audio lesson needs.

- **missing-function** — `07-time.plato`: `TimeInterval` has no
  `Duration(interval)`, `Contains(instant)`, or `Overlaps` helpers. Half-open
  semantics are stated in a comment but not operable.

- **doc-comment** — `Instant`: state the affine analogy in one line
  ("time-line point; delta type is Duration, cf. Point/Vector") so the
  parallel to `11-points.plato` is discoverable from the declaration alone.
