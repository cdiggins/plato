---
id: plato-357
title: Unify colors and times under coordinate-space vocabulary
type: idea
status: idea
priority: "?"
effort: "?"
risk: "?"
area: plato
sprint: 
created: 2026-07-30
closed:
links: [plato-340, plato-335, plato-306, plato-356]
---

## Idea
Colors and times should be treated as coordinates — points in a coordinate space. Coordinate spaces share distance metrics, an offset (delta) type, and an origin. Broader than "re-parent colors" (plato-340): unify the *affine space* vocabulary across spatial points, colors, and time.

## Assumptions
- `Coordinate` today (`points.concepts.plato`) is a thin marker: `inherits Value, Equatable, Interpolatable` — spatial in practice.
- Instant already uses `OriginBased<Duration>`; Point uses OriginBased<Vector*>; colors lack this.
- Overloading spatial Coordinate for color/time may confuse CRS/geo (plato-335).

## Design decisions
- **One Coordinate vs families** — generic affine-space concept vs ColorCoordinate / TemporalCoordinate / SpatialCoordinate.
- **Obligations** — require OriginBased + metric Distance, or marker only.
- **Color delta** — what is the offset type for Color? (plato-340 ColorModel interaction).

## Related
- [plato-340](plato-340.md) — shared Color concept (narrower).
- [plato-335](plato-335.md) — CRS identity for spatial coordinates.
- [plato-306](plato-306.md) — Difference / delta conversion.
- `OriginBased` / metric concepts in algebra-metric.
- [plato-356](plato-356.md) — TimeInterval/Instant interval story.

## Approaches
Short term: write ADR distinguishing spatial Coordinate from affine-space pattern; extend Instant/Color docs to name origin+delta+metric.
Long term: shared concept `AffineSpace` (name TBD) with associated Delta; Spatial Coordinate inherits it.
Adjacent: perceptual color metrics vs Euclidean RGB (dangerous defaults).

## Bedrock
Makes the **origin + delta + metric** pattern an explicit lattice seam instead of tribal repetition. Verdict: **right** as an ADR-first idea. Simple version must NOT force RGB Euclidean distance as *the* color metric.

## Done means
- [ ] ADR describing affine-space pattern and whether Coordinate expands or splits
- [ ] Instant/Point/Color each mapped to origin, delta, metric (even if metric is TBD for color)
- [ ] No silent equation of WGS84 positions with Color channels

## Simplest possible implementation
ADR + glossary update; no code moves until ColorModel (plato-340) and Instant Additive land.
- Pros: prevents wrong reparenting.
- Cons: no immediate API win.

## Case against
- Coordinate is already spatial in agents' heads; expanding it creates taxonomy mush.
- Color metrics are contested; a forced Distance is worse than none.
- Times-as-coordinates without timezone/CRS-like caveats mislead.
- Verdict: **pursue** as design/ADR; **park** code reparenting until plato-340 and Instant constraints settle. Strong idea, easy to overreach.
