---
lesson: angles-as-types
title: Angles as Types
domain: Foundations & vectors
v3-files: [06-quantities.plato, 12-intervals-bounds.plato]
audience: High-school math and general programming background
status: draft-v1
---

# Angles as Types

A game programmer stores the player's heading as a `Number`. The UI slider shows degrees;
the physics engine expects radians; the animation file stores "turns" (fractions of a full
revolution). Three functions each assume a different unit. Everything compiles. The character
spins at 57× the intended speed — because `1.0` degrees was passed where `1.0` radians was
required, and nobody noticed until playtest.

The bug is not arithmetic. It is **typing**: an angle is not a generic real number. It lives
on a circle, wraps every full turn, and carries an implicit unit convention even when the
number looks unitless. Treating it as `Number` hides all three facts until runtime.

## The idea

A planar angle measures rotation or direction in a plane. In calculus and physics the
standard unit is the **radian**: the angle subtended by an arc whose length equals the
radius. One full revolution is $2\pi$ radians. Degrees divide a revolution into 360 equal
parts; "turns" (or revolutions) divide it into 1.

These are **different scales on the same quantity**:

| Unit      | One full revolution |
|-----------|---------------------|
| Radians   | $2\pi \approx 6.283$ |
| Degrees   | $360$               |
| Turns     | $1$                 |

Converting between them is multiplication by a constant — easy to get wrong, impossible for
a compiler to catch if every angle is a bare `Number`.

Angles also **wrap**. On a circle, $0°$ and $360°$ aim the same direction. So do $1°$ and
$361°$. As raw numbers, $1$ and $361$ are far apart; as angles, they differ by one full
turn. Any code that compares, interpolates, or sorts headings must decide whether it cares
about the wrapped position on the circle or the accumulated rotation (a "wind-up" of many
turns). Those are different questions.

```
        0 rad / 2π rad  (same direction on the circle)
              ↑
    ─────────●─────────  +X axis
             / \
            /   \
     3π/2         π/2
```

For **orientations** (which way am I facing?), wrapping matters: two angles that differ
by $2\pi$ are equivalent. For **sweeps** (how much of the pie did I cut?), the span from
Start to End may cross the wrap point, and the interval is directed — clockwise vs
counter-clockwise sweeps over the same two boundary directions can enclose different regions.

## Why not Number?

A `Number` in Plato is a unitless real: it implements full `Arithmetic`, orders totally, and
interpolates linearly. Angles share some of that structure — you can add two rotations, scale
a rotation by a factor, interpolate between headings — but they are not a field and they are
not freely comparable without convention.

Three concrete failures of raw numbers:

1. **Unit confusion.** `Sin(90)` in a radians library is not sine of a right angle; it is
   sine of 90 radians. The literal `90` carries no hint that the caller meant degrees.

2. **Silent mixing.** Adding a heading in degrees to an angular velocity in radians per second
   is dimensionally absurd, yet both look like `Number`.

3. **Wrong equality and ordering.** Is $359°$ less than $1°$? Numerically yes. As headings on
   a compass, $359°$ is only $2°$ away from $1°$. Naive `<` on radians has the same problem
   near the $0 / 2\pi$ seam.

Plato's foundation convention is explicit: **angles are `Angle`, never raw `Number`**. Unit-
agnostic pure math (dot products, parametric $t$ along a segment) stays `Number`. Physical
and geometric angles get their own type.

## In Plato: the Angle quantity

`Angle` lives in `06-quantities.plato` with the other measured quantities. It implements
`Quantity` — a number with an intrinsic unit implied by the type name and field name:

```plato
// A one-dimensional measured amount with an implicit unit.
interface Quantity
    inherits Value, Comparable, Hashable, Additive, Scalable, Interpolatable
{
    Amount(x: Self): Number;
}

// A planar angle, stored in radians.
type Angle implements Quantity { Radians: Number; }
```

The field is named `Radians`, not `Value` or `X`. That is deliberate: the canonical storage
unit is visible at every construction site. Other units (degrees, turns) are conversions *to*
this representation, not alternate fields on the same struct.

`Quantity` gives `Angle` addition, subtraction, scaling by a unitless scalar, comparison,
hashing, and linear interpolation — but **not** full `Arithmetic`. You cannot multiply two
angles to get another angle (that would produce steradians — a different quantity, captured
by `SolidAngle`). Keeping quantities out of `Arithmetic` prevents `$ \text{angle} \times
\text{angle} $" from silently type-checking.

Related kinematic quantities inherit the same radian convention:

```plato
// Rate of rotation, stored in radians per second.
type AngularVelocity implements Quantity { RadiansPerSecond: Number; }

// Rate of change of angular velocity, stored in radians per second squared.
type AngularAcceleration implements Quantity { RadiansPerSecondSquared: Number; }
```

Downstream types store headings and rotations as `Angle`, not `Number`. Examples from the
v3 vocabulary:

```plato
// A rotation in the plane, stored as a single angle.
type Rotation2D
    implements Value, Multiplicative, Interpolatable
{
    Angle: Angle;
}

// A rotation of Angle about a unit Axis, following the right-hand rule.
type AxisAngle
    implements Value
{
    Axis: Direction3D;
    Angle: Angle;
}

// A planar position as distance from origin and angle from the positive X axis.
type PolarCoordinate
    implements Value
{
    Radius: Number;
    Angle: Angle;
}
```

Intrinsics (declared in `70-intrinsics.plato`) wire the numeric kernel through the type
boundary:

```plato
// The Number->Angle cast interprets the payload as radians.
Angle(x: Number): Angle;

Cos(self: Angle): Number;
Sin(self: Angle): Number;
Atan2(self: Number, x: Number): Angle;

Add(a: Angle, b: Angle): Angle;
Subtract(a: Angle, b: Angle): Angle;
Multiply(a: Angle, x: Number): Angle;
```

Usage-shaped expressions:

```plato
// Construct from a radian literal (field syntax makes the unit obvious)
let quarterTurn = Angle { Radians: 1.5707963267948966 };

// Or via the intrinsic cast — radians only
let rightAngle = Angle(1.5707963267948966);

// Trig expects Angle, returns unitless Number
let c = Cos(rightAngle);   // ~0
let s = Sin(rightAngle);   // ~1

// Heading from a 2D vector: Atan2 returns Angle
let heading = Atan2(v.Y, v.X);

// Compose plane rotations
let r1 = Rotation2D { Angle: Angle(0.5) };
let r2 = Rotation2D { Angle: Angle(1.0) };
let combined = Multiply(r1, r2);
```

Notice what the types reject: passing a bare `Number` to `Cos` where an `Angle` is required,
or storing `Yaw: Number` on a camera rig. The friction is the point.

## Wrapping and equivalence

`Add` and `Subtract` on `Angle` operate on the **stored radian value** — they do not
automatically reduce the result to $(-\pi, \pi]$ or $[0, 2\pi)$. After many integrations,
a heading can legitimately hold $7\pi$ radians (three full turns plus a bit). That is correct
for tracking **total rotation** (a joint winding up).

For **display** or **compass-style** comparison, you often want the representative in one
base period. v3 does not yet declare a named `Wrap` or `Normalize` on `Angle`; callers must
know whether they need raw accumulation or wrapped equivalence. This is the main fine-print
gap between "angle as number with a label" and "angle as circle position."

Conceptually, wrapping is modular arithmetic with period $2\pi$:

```
wrapped(θ) = θ mod 2π     (choose a canonical half-open interval)
```

Two angles $\theta_1$ and $\theta_2$ are **equivalent orientations** when their difference
is an integer multiple of $2\pi$. Raw `Compare(a, b) == 0` tests exact radian equality,
not equivalence modulo one turn. Treating $0$ and $2\pi$ as the same heading requires an
explicit comparison convention that v3 has not declared.

**Linear interpolation** inherits the same ambiguity. `Quantity` includes `Interpolatable`,
so `Lerp(a, b, t)` exists for `Angle`. Linear lerp walks the real line between the stored
values — it does not automatically take the **short arc** on the circle. From $350°$ to
$10°$, linear lerp in radians briefly swings the long way unless you normalize endpoints or
use a rotation-specific interpolator (quaternion slerp lives on a different type entirely).

```
Long path (naive Lerp):   350° ──→ 360° ──→ ... ──→ 10°   (340° of travel)
Short path (desired):     350° ──→ 10°                      (20° of travel)
```

The type tells you *what* is being interpolated; the algorithm must still encode *which path*
on the circle.

## AngleInterval: directed angular spans

A single `Angle` is a direction or rotation amount. An **angular range** — the sweep of a pie
slice, the parameter domain of a circular arc, the sector covered by a radar cone — needs
two endpoints. That is `AngleInterval` in `12-intervals-bounds.plato`:

```plato
// A one-dimensional directed span from Start to End. Start may exceed End,
// which encodes a reversed interval.
interface IntervalLike<T>
    inherits Value
{
    Start(x: Self): T;
    End(x: Self): T;
}

// A directed interval of angles: arc sweeps, angular ranges.
type AngleInterval
    implements IntervalLike<Angle>
{
    Start: Angle;
    End: Angle;
}
```

`IntervalLike` is generic over the endpoint type; `NumberInterval` and `LengthInterval` share
the same shape for unitless spans and distances. `AngleInterval` is the angle-colored
instance.

The doc comment carries two teaching points:

1. **Directed.** `Start` and `End` are ordered. The interval describes the sweep *from*
   Start *toward* End, not the unordered set `{Start, End}`.

2. **Reversed allowed.** `Start` may exceed `End`. That encodes a sweep that runs the
   opposite direction around the circle — essential when winding convention matters.

Geometric types consume `AngleInterval` for exactly this reason:

```plato
// A portion of a circle: the points at distance Radius from Center, swept over
// the angle interval measured from the +X axis.
type CircularArc2D
    implements Curve2D
{
    Center: Point2D;
    Radius: Number;
    Angles: AngleInterval;
}
```

`CircularSector` and `CircularSegment` in `17-planar-shapes.plato` store their arc as
`Sweep: AngleInterval` with the same +X / counter-clockwise convention.

Usage-shaped construction:

```plato
// Quarter circle in the first quadrant: 0 to π/2, counter-clockwise from +X
let quarterArc = CircularArc2D {
    Center = Point2D { X: 0, Y: 0 },
    Radius = 1.0,
    Angles = AngleInterval {
        Start = Angle(0.0),
        End   = Angle(1.5707963267948966)
    }
};

// Reversed sweep: Start > End means "sweep clockwise from Start down to End"
let clockwiseSlice = AngleInterval {
    Start = Angle(1.5707963267948966),   // π/2
    End   = Angle(0.0)
};
```

An `AngleInterval` is **not** a pair of headings to compare for "closeness." It is a
parametric domain: the directed trace from Start toward End. A radar field-of-view or a
light cone is naturally an angular interval; a compass bearing is a single `Angle`.

When the sweep crosses the $0 / 2\pi$ seam — Start $= 3\pi/4$, End $= 5\pi/4$ — the interval
may need to pass through zero. Whether the geometry implementation splits the arc or treats
the interval as monotonic in stored radians is an implementation detail; the type tells you
*which two bounds* and *which direction*, not the numeric trickery.

## Pitfalls / fine print

**Radians as the only declared storage unit.** v3 stores `Angle.Radians`. Degrees and turns
are not alternate fields. Every degree-to-radian conversion is a explicit multiplication at
the boundary — ideally one named constructor per unit once the library lands. Until then,
`Angle(3.141592653589793)` and `Angle(180)` mean very different things.

**Cast vs construction.** `Angle(x: Number)` interprets `x` as radians. There is no
declared `Angle.FromDegrees`. Feeding UI slider values directly into the cast is the classic
bug this type exists to prevent — you need a conversion layer the types can enforce.

**Do not confuse angle with unitless phase.** Parametric curve parameter $t \in [0,1]$ is a
`Number`. The angle parameter on a circle is an `Angle`. `Frequency` (hertz) and
`AngularVelocity` (radians per second) are different quantity types — mixing them with `Angle`
should fail at compile time once quantities are fully checked.

**`Amount` strips the unit.** `Quantity.Amount(a)` returns the raw `Number` payload. That
escape hatch is for serialization and interop; geometry code should keep values as `Angle`
until the last moment.

**SolidAngle is separate** (steradians on a sphere, not a planar rotation). **Interval
endpoints are not normalized** — Start and End need not lie in $[0, 2\pi)$; arc code must
agree on conventions.

## Try it

1. A UI slider stores `45` meaning degrees. The engine calls `Angle(45)`. How far off is
   the resulting rotation from a true $45°$ turn?

2. `Start = Angle(0)`, `End = Angle(6.283185307179586)` ($2\pi$). Does this interval
   describe a full circle sweep, or a zero-length arc? (Think: are Start and End equivalent
   orientations, or identical stored values?)

3. Two headings: `a = Angle(6.178)` (~$354°$) and `b = Angle(0.175)` (~$10°$). Naive
   `Lerp(a, b, 0.5)` walks the long way. What approximate radian value would the midpoint
   on the **short** arc have?

<details>
<summary>Answers</summary>

1. `Angle(45)` treats 45 as **radians** (~2578°). A true $45°$ turn is
   $\pi/4 \approx 0.785$ radians — roughly 57× too large. This is the canonical
   degrees-vs-radians disaster.

2. As **stored values**, Start $\neq$ End, so the interval has numeric extent $2\pi$.
   As **orientations**, $0$ and $2\pi$ aim the same direction; some algorithms would treat
   this as an empty or full sweep depending on convention. The type does not resolve that —
   the caller must.

3. Short-arc midpoint: average the **wrapped** positions. $354°$ and $10°$ differ by $16°$
   going the short way; midpoint is $2°$ past $0°$, about $0.035$ radians. Linear lerp in
   raw radians would land near $3.18$ rad (~$182°$) — the long path.

</details>

## Library recommendations

- **missing-function** — `06-quantities.plato` / `70-intrinsics.plato`: `Angle` has `Add`,
  `Subtract`, and `Compare`, but no declared `Wrap(self, period: Angle): Angle` or
  `Normalize(self): Angle` to reduce to a canonical period (e.g. $(-\pi, \pi]$). Wrapping
  and compass comparisons need this on day one.

- **missing-function** — `06-quantities.plato`: no `FromDegrees(n: Number): Angle` or
  `FromTurns(n: Number): Angle` constructors. Without them, the type prevents conflating
  units at call sites but pushes every caller to hand-roll conversion constants — the exact
  bug farm the type is meant to eliminate.

- **missing-function** — `12-intervals-bounds.plato`: `AngleInterval` declares only
  `Start`/`End` fields via `IntervalLike`. There is no `Contains(interval, angle: Angle):
  Boolean`, `Span(interval): Angle`, or `Union`/`Intersection` for overlapping angular ranges.
  `CircularSector` and `CircularArc2D` need these operations; teaching intervals without them
  stops at data shape.

- **missing-function** — `06-quantities.plato`: `Angle` implements `Interpolatable` through
  `Quantity`, but no `LerpShortest(a, b, t: Number): Angle` (or doc comment on `Lerp`
  stating it uses the long path). Heading interpolation is a daily operation; the default lerp
  semantics are actively wrong for many inputs.

- **missing-interface** — no periodic equality (e.g. `EquivalentModPeriod(a, b, period)`).
  `Compare` on raw radians cannot express "same heading" near the $0/2\pi$ seam.

- **doc-comment** — `12-intervals-bounds.plato`: `AngleInterval` should document inclusive
  endpoints and wrap-crossing behavior for arc consumers (`CircularArc2D`, `CircularSector`).

> Resolved 2026-07-28: intervals-transforms.library.plato added wrap-aware AngleInterval Span/Contains (item 11, partial: circular Union/Intersection left undefined as unsound); AngleInterval type comment documents inclusive endpoints + seam-crossing (item 14).
