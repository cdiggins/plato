---
lesson: engineering-tolerance-fits
title: Engineering Tolerances and Fits
domain: Advanced & applied
v3-files: [63-uncertainty.plato, 66-engineering.plato]
audience: Comfortable with intervals and basic manufacturing vocabulary (shaft, hole, clearance); no metrology background assumed
status: draft-v1
---

# Engineering Tolerances and Fits

A drawing says a hole is 10 mm. A shop cannot make *exactly* 10 mm — every cut has
scatter. If the matching shaft is also “10 mm,” the pair might rattle freely, slide
with a light press, or seize forever, depending on which side of the scatter each
part landed on. Engineering **tolerance** is the deliberate band around a nominal
size. A **fit** is the designed relationship between two mating bands.

Measurement uncertainty is a related but different idea: how sure you are of a
reading. Mixing the two — treating a ±0.05 mm manufacturing allowance as if it were
a 1-σ lab error — is a common source of bad go/no-go decisions.

## The idea

### Nominal, plus, and minus

For a one-dimensional feature size (a diameter, a length), an asymmetric tolerance
asserts an acceptable closed interval around a **nominal** value $N$:

$$
[N - M,\; N + P]
$$

where $M \ge 0$ is the downward allowance (**minus**) and $P \ge 0$ is the upward
allowance (**plus**). Symmetric “±0.1” is just the special case $P = M = 0.1$.
Asymmetry matters: a hole often gets more plus than minus so drills tend to
oversized rather than undersized scrap.

```
  N - M          N          N + P
 ----●===========●===========●----
     |<-- minus -->|<-- plus -->|
           acceptable band
```

### Three classes of cylindrical fit

For a shaft inside a hole that share the same nominal diameter, compare the
**worst-case** hole and shaft extremes:

| Class | Worst-case relationship | Everyday feel |
|---|---|---|
| Clearance | smallest hole ≥ largest shaft | always a gap; free sliding or running |
| Transition | bands overlap | may clear or lightly interfere depending on luck |
| Interference | largest hole ≤ smallest shaft | always a press / shrink fit |

The engagement class is a design *intent*, not a measurement of one particular
pair. You compute it from the four extremes (hole max/min, shaft max/min).

### Worked numbers (millimetres)

Suppose nominal diameter is 20 mm.

- Hole: $20^{+0.021}_{0}$ — so $[20.000,\, 20.021]$
- Shaft: $20^{-0.020}_{-0.041}$ — so $[19.959,\, 19.980]$

Minimum clearance $= 20.000 - 19.980 = 0.020$ mm (always positive) → **clearance**
fit. Swap the shaft for $20^{+0.035}_{+0.022}$ and every pairing interferes.

### Tolerance is not standard uncertainty

A lab reports a measured diameter as $20.004$ mm with standard uncertainty
$u = 0.002$ mm (1-σ). That is a probabilistic statement about the measurement
process. The drawing’s $20^{+0.021}_{0}$ is a hard accept/reject window for the
*part*. Expanded uncertainty ($k \cdot u$, often $k = 2$ for ~95% coverage under
a normal model) is how metrologists widen a 1-σ figure into a reporting interval —
still not the same object as a manufacturing plus/minus band.

## In Plato

v3 separates the manufacturing band from the measurement story.

From `63-uncertainty.plato`:

```plato
// An asymmetric engineering tolerance about a nominal value. The acceptable
// range is [Nominal - Minus, Nominal + Plus]; Plus and Minus are
// non-negative magnitudes.
type Tolerance
    implements Value
{
    Nominal: Number;
    Plus: Number;
    Minus: Number;
}

type UncertainNumber
    implements Value
{
    Value: Number;
    StandardUncertainty: Number;
}

type ExpandedUncertainty
    implements Value
{
    StandardUncertainty: Number;
    CoverageFactor: Number;
}

type ConfidenceInterval
    implements Value
{
    Lower: Number;
    Upper: Number;
    Level: Probability;
}
```

From `66-engineering.plato`:

```plato
type FitClass = Clearance | Transition | Interference;

type ShaftHoleFit
    implements Value
{
    NominalDiameter: Length;
    HoleTolerance: Tolerance;
    ShaftTolerance: Tolerance;
    Engagement: FitClass;
}
```

Usage-shaped sketches (illustrative):

```plato
holeTol = Tolerance {
    Nominal: 20.0;
    Plus: 0.021;
    Minus: 0.0;
}

shaftTol = Tolerance {
    Nominal: 20.0;
    Plus: -0.020;   // WRONG — Plus/Minus must be non-negative magnitudes
    Minus: 0.041;
}

// Correct shaft band [19.959, 19.980] as magnitudes from nominal 20:
shaftTol = Tolerance {
    Nominal: 20.0;
    Plus: 0.0;      // still wrong for this example — see pitfalls
    Minus: 0.041;
}
```

The shaft example above is awkward on purpose: when *both* limits lie below the
nominal (ISO “fundamental deviation” below zero), the acceptable interval is
still $[N-M,\, N+P]$, but you need $P < 0$ in the *signed* ISO sense — which
v3’s non-negative `Plus`/`Minus` magnitudes cannot express without shifting
`Nominal` to the mid-band or encoding limits another way. Prefer:

```plato
// Represent the shaft band by setting Nominal to the mid-point of the zone,
// or keep Nominal = basic size and put the whole zone on Minus/Plus only when
// the zone straddles or sits above the basic size.
fit = ShaftHoleFit {
    NominalDiameter: Length { Meters: 0.02 };
    HoleTolerance: holeTol;
    ShaftTolerance: shaftTol;
    Engagement: Clearance;
}

// Measurement of one part (not a fit):
reading = UncertainNumber {
    Value: 20.004;
    StandardUncertainty: 0.002;
}

expanded = ExpandedUncertainty {
    StandardUncertainty: 0.002;
    CoverageFactor: 2.0;   // ~95% under a normal model
}
```

`ShaftHoleFit` stores `Engagement` as an explicit field: the vocabulary records
the designer’s classification rather than deriving it from the two `Tolerance`
records (no library body yet anyway).

## Pitfalls / fine print

- **Plus/Minus are magnitudes, not ISO deviations.** ISO hole/shaft letter codes
  encode signed fundamental deviations. Mapping “h6” or “H7” onto `Tolerance`
  requires converting catalog limits into non-negative offsets from whatever you
  choose as `Nominal`.
- **Both members share one `NominalDiameter`.** If hole and shaft basics differ
  (rare in simple cylindrical fits, common in special designs), `ShaftHoleFit`
  as declared is the wrong record.
- **Units.** `Tolerance` fields are bare `Number`; `ShaftHoleFit.NominalDiameter`
  is `Length`. Keep the tolerance numbers in the same unit as the diameter’s
  metres (or consistently millimetres in a local convention) — the type system
  will not catch a mm-vs-m mismatch inside `Tolerance`.
- **Do not confuse `ConfidenceInterval` with a tolerance zone.** A 95% CI is a
  statement about estimation; scrap bins care about the drawing zone.
- **Transition fits are statistical.** Worst-case classification can say
  Transition while a process capability study shows almost all assemblies clear.

## Try it

1. Hole $10^{+0.015}_{0}$, shaft $10_{-0.025}^{-0.010}$ (mm). What are the four
   extremes? Is the fit Clearance, Transition, or Interference?
2. A gauge reads $10.006$ mm with $u = 0.001$ mm. With coverage factor $k = 2$,
   what expanded half-width do you report? Does that prove the part is inside
   $10^{+0.015}_{0}$?
3. Why is `UncertainNumber` a poor substitute for `Tolerance` on a drawing?

<details>
<summary>Answers</summary>

1. Hole $[10.000, 10.015]$, shaft $[9.975, 9.990]$. Min clearance
   $10.000 - 9.990 = 0.010 > 0$ → Clearance.
2. Expanded half-width $= k u = 0.002$ mm, so roughly $[10.004, 10.008]$ as a
   reporting interval. That does **not** by itself prove conformance: you still
   need a decision rule that combines measurement uncertainty with the
   tolerance zone (GUM / ILAC policies). The reading alone looking “inside”
   the zone is necessary but not always sufficient.
3. `UncertainNumber` describes a measured value and its 1-σ error; `Tolerance`
   describes an allowed manufacturing band around a nominal. One is about
   knowledge of a specimen; the other is about acceptability of a design.

</details>

## Library recommendations

- **wrong-shape** — `63-uncertainty.plato`: `Tolerance` forces non-negative
  `Plus`/`Minus` about `Nominal`, but ISO shaft/hole limits often lie entirely
  above or below the basic size. Teaching fits immediately wants either signed
  deviations (`UpperDeviation`/`LowerDeviation`) or an explicit
  `Limits(Lower, Upper)` alternate form; the current shape pushes authors to
  lie about `Nominal`.
- **missing-function** — `66-engineering.plato`: `ShaftHoleFit` stores
  `Engagement` but nothing computes `FitClass` from the two `Tolerance` bands
  (min/max clearance). A pure function
  `ClassifyFit(hole: Tolerance, shaft: Tolerance): FitClass` would make the
  lesson’s table executable and keep `Engagement` from drifting out of sync.
- **naming** — `63-uncertainty.plato`: the type name `Tolerance` collides
  cognitively with floating-point epsilons and with fields like
  `PathSimplifyParameters.Tolerance: Number`. Prefer `EngineeringTolerance` or
  `DimensionalTolerance` so applied-engineering lessons can say the word
  “tolerance” without disambiguation paragraphs.
- **missing-type** — `66-engineering.plato`: no ISO fit designation record
  (hole basis letter/grade + shaft letter/grade). Lessons and CAD imports
  routinely start from “H7/g6”; mapping tables have nowhere typed to land.
- **doc-comment** — `63-uncertainty.plato`: `Tolerance` should state the unit
  convention when paired with `Length` (must match `NominalDiameter.Meters` for
  `ShaftHoleFit`) and warn that measurement types (`UncertainNumber`,
  `ExpandedUncertainty`) are not substitutes for manufacturing zones.
