---
id: plato-363
title: Legacy golden drift + CS0557 conformance breakage from a72011e writer changes
type: bug
status: dropped
priority: p1
effort: S
risk: low
area: plato
created: 2026-07-30
closed: 2026-07-30
links: [plato-308, plato-362]
---

The wip commits a72011e/043a9cf changed the C# writer (scalar-on-left operator commute,
Point2D/Point3D <-> Vector2/Vector3 intrinsic bridges, capture-hoist fix) WITHOUT refreshing
the diff-gated goldens, violating the "intended emitter change refreshes goldens in the same
commit" rule (AGENTS.md).

Observed 2026-07-30 while verifying plato-362 (which contributes none of these diffs -
verified by a stash A-B):

- `tools/regen-generated.ps1` diff-gate RED: 3 files differ in Unoptimized (_Number,
  _Point2D, _Point3D), 12 in Optimized (adds capture-hoist temp renumbering in Integers /
  Core / Curves / Geometry / Meshes / ...). Both regenerated projects still BUILD clean.
- `tools/regen-conformance.ps1 -Test` now FAILS to build: the regenerated legacy suite hits
  CS0557 duplicate user-defined conversion in Point2D/Point3D - the new generated
  intrinsic-bridge implicit operators collide with conversions the legacy vocabulary already
  declares. The legacy conformance Generated/ folder is left in this broken state (it is
  script-produced and gitignored, so nothing is corrupted, but the suite cannot run until
  this is fixed).

Fix: guard the intrinsic-bridge emission against types that already declare the conversion
(or emit it only in the forward recipe), then `regen-generated.ps1 -Apply` +
`regen-conformance.ps1 -Test` green in the same commit.

## Resolution (2026-07-30)

Closed without fixing: the golden diff-gate and the legacy conformance suite were both
retired the same day (see decisions/2026-07-30-retire-legacy-conformance-and-goldens.md),
so neither reported breakage exists any more.
