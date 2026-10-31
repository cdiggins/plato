# Quantities and time

A quantity record separates a physical meaning from an unlabelled `Number`.

## A quantity is an amount with a meaning

`Length`, `Area`, `Volume`, and the other quantity records identify different meanings. `IQuantity.Amount` returns the canonical numeric amount. `FromAmount` constructs the corresponding quantity. Read each field's unit rather than assuming every stored number has the same unit.

Within one quantity type, addition and scalar multiplication preserve its meaning. Two lengths can be added. Scaling a length by a unitless number returns a length. Do not infer a complete dimensional-analysis calculus from `IQuantity`. An available length-times-length overload must be established from its signature, not from mathematical plausibility. A raw constructor can represent a physically invalid value, such as negative mass, unless a separate validator prevents it. A wrapper around a numeric amount is a construction, not proof that its invariant holds.

Conversions follow the [faithfulness rule](../../../../stdlib/CONVENTIONS.md#conversions--type-named-is-implicit-tox-is-explicit). Type-named single-argument conversions can become implicit casts. Explicit `ToX` calls expose rounding, sampling, or discarded information. [System.Numerics](https://learn.microsoft.com/en-us/dotnet/api/system.numerics.matrix4x4) supplies numerical storage used elsewhere in Plato and does not supply these quantity meanings. A numeric light parameter keeps its declared interpretation rather than becoming an arbitrary RGB multiplier ([PBRT's radiometry chapters](https://pbr-book.org/4ed/contents)).

## Instants are positions; durations are differences

`Instant` stores seconds relative to an arbitrary epoch. `Duration` stores a span in seconds. Given instants `a` and `b`, `a.Between(b)` is the duration from `a` to `b`. Adding that duration to `a` recovers `b` in exact arithmetic. Adding two instants has no corresponding affine interpretation.

`TimeInterval` is explicitly half-open. It includes `Start` and excludes `End`. This is an exception to the general inclusive geometric-bound convention. Adjacent time intervals can meet without assigning one instant to both spans.

`FrameRate` stores frames per second. `FrameTime` pairs a discrete frame with its rate. A frame number alone does not identify a time. The comparison body compares frame numbers and requires equal rates. It does not compare physical times across rates. `Tempo` and `BeatTime` express musical beats. Converting beats to seconds needs a tempo. A timecode record does not by itself implement every broadcast drop-frame rule, timezone, leap-second system, or civil calendar.

## Where to look

- [Quantity types](../../api/foundation/quantities.types.md), [their interface](../../api/foundation/quantities.concepts.md), and [quantity arithmetic](../../api/foundation/quantities.library.md)
- [Time records](../../api/foundation/time.types.md) and [time operations](../../api/foundation/time.library.md)
