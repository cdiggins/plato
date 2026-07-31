---
lesson: units-in-types
title: Units in Types
domain: Foundations & vectors
v3-files: [06-quantities.plato]
audience: High-school physics and general programming background
status: draft-v1
---

# Units in Types

The Mars Climate Orbiter was lost in 1999 because one team sent thruster data
in pound-seconds and another expected newton-seconds. The numbers were fine;
the units were not. Every programmer who has mixed degrees with radians, or
added a length to a time, has a smaller version of that story.

Dimensional analysis is the discipline of tracking units so illegal mixes are
impossible. Plato's quantity types put that discipline in the type checker:
`Length` is not `Mass`, `Angle` is not `Number`, and adding them is not a
thing you can write by accident.

## The idea

A physical quantity is a number *plus* a dimension (length, mass, time, …) and
a chosen unit (meters, kilograms, seconds). You may:

- **Add / subtract** quantities of the *same* dimension (3 m + 2 m = 5 m)
- **Scale** a quantity by a unitless number (2 × 3 m = 6 m)
- **Multiply / divide** quantities and get a *different* dimension
  (3 m / 2 s = 1.5 m/s)

You may **not**:

- Add meters to kilograms
- Pass radians where a raw float was expected and hope the next function
  assumed degrees
- Compare a temperature in kelvin to a temperature *difference* without
  noticing the distinction

The SI base dimensions are length, mass, time, electric current, temperature,
amount of substance, and luminous intensity. Derived quantities (force, energy,
pressure, …) are products of powers of those bases. In software, each derived
quantity can be its own named type with a canonical storage unit.

```
  Length  ×  Length     →  Area
  Length  /  Duration   →  Speed   (in Plato: Speed, not a Velocity type)
  Mass    ×  Acceleration → Force
  Force   ×  Length     →  Energy (as work)
```

Runtime unit systems still matter for file import and UI ("show me feet").
Compile-time types catch the bugs *inside* your algorithms; dynamic units
catch mismatches at the boundary.

## In Plato

The `Quantity` concept in `06-quantities.plato` is the abstract shape:

```plato
// A one-dimensional measured amount with an implicit unit.
concept Quantity
    inherits Value, Comparable, Hashable, Additive, Scalable, Interpolatable
{
    Amount(x: Self): Number;
}
```

Important: `Quantity` is deliberately **not** `Arithmetic`. You can add two
lengths and scale a length by a `Number`, but multiplying two quantities is
*not* declared as returning `Self` — because the product is usually a
different type. That omission is the type system's teeth.

Each concrete type names its canonical field in natural units:

```plato
type Angle implements Quantity { Radians: Number; }
type Length implements Quantity { Meters: Number; }
type Area implements Quantity { SquareMeters: Number; }
type Volume implements Quantity { CubicMeters: Number; }
type Speed implements Quantity { MetersPerSecond: Number; }
type Acceleration implements Quantity { MetersPerSecondSquared: Number; }
type Mass implements Quantity { Kilograms: Number; }
type Force implements Quantity { Newtons: Number; }
type Energy implements Quantity { Joules: Number; }
type Temperature implements Quantity { Kelvin: Number; }
type TemperatureDelta implements Quantity { Kelvin: Number; }
```

Field names are the documentation: `Meters`, not `Value`. Reading a
`Length` always means SI meters at rest; conversion happens at I/O.

For runtime-tagged units (import, UI), v3 provides the dynamic trio:

```plato
type Dimension
{
    LengthPower: Integer;
    MassPower: Integer;
    TimePower: Integer;
    CurrentPower: Integer;
    TemperaturePower: Integer;
    AmountPower: Integer;
    LuminosityPower: Integer;
}

type UnitOfMeasure
{
    Name: String;
    Symbol: String;
    Dimension: Dimension;
    ScaleToSI: Number;
    OffsetToSI: Number;
}

type DynamicQuantity
{
    Amount: Number;
    Unit: UnitOfMeasure;
}
```

`OffsetToSI` exists because temperature scales are affine (°C → K adds 273.15),
not purely multiplicative. `Temperature` vs `TemperatureDelta` splits absolute
temperature from a difference — you can add a delta to a temperature, but
adding two absolute temperatures is almost always a mistake.

Usage-shaped snippets:

```plato
let width = Length { Meters: 3.0 };
let height = Length { Meters: 2.0 };

// Same dimension: Additive applies
let perimeterPart = width.Add(height);

// Scale by a unitless Number (Scalable)
let doubleWidth = width.Multiply(2.0);

// Amount extracts the raw SI number when you truly need it
let meters = width.Amount;   // 3.0

// Different types do not add
// width.Add(Mass { Kilograms: 1.0 })  — not well-typed

let speed = Speed { MetersPerSecond: 10.0 };
let accel = Acceleration { MetersPerSecondSquared: 2.0 };
```

Angles are quantities too — stored in radians, never as a bare `Number` in
APIs that mean "turn":

```plato
let quarterTurn = Angle { Radians: 1.5707963 };
```

That single rule eliminates an entire class of degrees/radians bugs at API
boundaries.

## Pitfalls / fine print

**Canonical unit ≠ display unit.** `Length.Meters` is storage. Showing feet in
a UI means converting at the edge via `UnitOfMeasure` / `DynamicQuantity`, not
storing feet inside `Length`.

**Affine temperatures.** Converting °C to K uses an offset. Scaling a
temperature in °C by 2 is meaningless in a way that scaling a
`TemperatureDelta` is not. Keep absolute and delta types distinct.

**Dimensionless traps.** `Strain` is a ratio (`Ratio: Number`) that still
implements `Quantity`. Unitless does not mean "just use `Number`" when the
value has a physical meaning and a conventional range.

**No typed multiply yet.** v3 does not declare `Multiply(Length, Length): Area`
or `Divide(Length, /* time */): Speed`. The concept comment promises that
multiplying yields a different type, but the cross-type operators are not on
the surface — algorithms that need them must document the gap or drop to
`Amount` and rebuild by hand.

**Speed vs velocity.** The scalar rate of motion is `Speed`. Directional
velocity in simulation code is typically a `Vector2D` / `Vector3D` field named
`Velocity`, not a quantity type in `06-quantities.plato`. Do not invent a
`Velocity` quantity.

**Mixing `Number` geometry with `Length`.** Pure math APIs often use `Number`
for coordinates (unit-agnostic). Physical APIs use `Length`. Crossing the
boundary requires an explicit policy for "what is one unit in this scene."

## Try it

1. Which of these should type-check as addition of quantities: `Length + Length`,
   `Length + Mass`, `Angle + Angle`?
2. Why does `Temperature` share the field name `Kelvin` with
   `TemperatureDelta` but remain a separate type?
3. A `UnitOfMeasure` for inches might use `ScaleToSI = 0.0254` and
   `OffsetToSI = 0`. Why is the offset zero here but nonzero for °C?

<details>
<summary>Answers</summary>

1. `Length + Length` and `Angle + Angle` are same-dimension adds.
   `Length + Mass` is illegal.
2. Absolute temperature and temperature *difference* share a storage unit but
   different affine meaning; adding two room temperatures is not a physical
   temperature in the same sense as adding two deltas.
3. Inch ↔ meter is a pure scale. Celsius ↔ kelvin needs an additive offset
   (273.15), which is why `UnitOfMeasure` carries `OffsetToSI`.

</details>

## Library recommendations

- **missing-function** — `06-quantities.plato`: no cross-quantity operators such
  as `Multiply(a: Length, b: Length): Area`, `Divide(a: Length, b: Length): /* ratio */`,
  or `Divide(distance: Length, time: /* Duration */): Speed`. The file's own
  banner says multiplication yields a different type, but nothing declares
  those maps — the lesson cannot show typed dimensional arithmetic end-to-end.

- **missing-type** — `06-quantities.plato`: there is `Speed` (scalar) but no
  quantity-level companion for "duration as a quantity product partner"
  inside this file (`Duration` lives in `07-time.plato`). A documented
  `Divide(Length, Duration): Speed` (wherever it lives) would make the
  Length/Speed story teachable without dropping to raw `Number`.

- **naming** — `06-quantities.plato`: `Amount(x: Self): Number` on `Quantity`
  vs per-type fields (`Meters`, `Kilograms`). Callers will wonder whether to
  read `.Meters` or call `.Amount`. A doc comment stating they are equivalent
  accessors for the canonical SI value would remove the ambiguity.

- **doc-comment** — `Temperature` / `TemperatureDelta`: the split is correct
  and subtle; the comments should state explicitly that you must not add two
  `Temperature` values as if they were deltas, and that `UnitOfMeasure.OffsetToSI`
  exists primarily for temperature scales.
