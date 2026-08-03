---
id: plato-233
title: v3 kind-pattern sweep: migrate remaining ~100 enum-style Kind types to sum-type enums
type: idea
status: idea
priority: "?"
effort: "?"
risk: "?"
area: plato
sprint: 
created: 2026-07-27
closed:
links: [plato-232, submodules/Plato/docs/plato-sum-types-v3-survey.md, submodules/Plato/docs/plato-sum-types-design-2026-07-27.md]
---

## Summary

Sum types + exhaustive `match` shipped in [plato-232](plato-232.md), and wave 3 migrated the
five flagship kind-pattern carriers (`PathSegment2D`, `Paint`, `MaskSource2D`,
`ScalarFieldNode2D/3D`, `WindowFunction`). The survey
([../../docs/reports/plato-sum-types-v3-survey.md](../../submodules/Plato/docs/plato-sum-types-v3-survey.md))
counted **115** `XxxKind` types; ~11–15 were true sums and the rest (~100) are **pure-enum**
selectors still in the legacy `type XxxKind { Value: Integer; }` kind pattern. Sweep them to
payload-free sum enums (`type LineCap = Butt | Round | Square;`) — a mechanical, behavior-
preserving collapse of `type XxxKind { Value: Integer; }` plus its selector field.

## Scope

- The ~100 pure enums (survey §4, blank rows), one file at a time, gated on `lint plato-src-v3`
  (0 parse / 0 symbol errors) staying green.
- The remaining verified/borderline true sums NOT taken in wave 3: `ThresholdKind`,
  `AlphaModeKind`, `MovingWindowKind`, `EmitterShapeKind`, `ErrorPropagationKind`, and the
  `SUM?` carriers `LightKind`, `SdfCombineKind`, `ColumnKind`, `BrdfKind`; plus `43-scene2d`'s
  `NodeContentKind`/`SceneNode2D` (the indexed/deferred outer-product variant).

## Deferred language features (separate follow-ups, out of scope here)

Bare (unqualified) case constructors; generic sum types (currently CHK306); default/wildcard
`match` arm; `match` guards; nested case patterns; recursive sum types; and GLSL/TypeScript/Rust
emission of sums (currently CHK320-rejected).

## Notes

- New `.plato` declarations should already use sums, not the kind pattern (per the updated
  `plato-src-v3/README.md`).
- Pure-enum collapse is behavior-preserving and low-risk; the true-sum carriers each need the
  same conditional-payload analysis the flagship five got.
