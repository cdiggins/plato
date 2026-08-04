---
id: plato-448
title: Extend stdlib comparison with geometry3Sharp and production environments
type: debt
status: in-progress
priority: p2
effort: S
risk: low
area: plato
sprint:
created: 2026-08-04
closed:
links:
  - docs/reports/plato-stdlib-comparative-study-2026-08-04-r2.md
---

## Problem

The comparative study does not cover geometry3Sharp's executable C# geometry workflows
or the scene, mesh-resource, and authoring models of Unity, Unreal Engine, Godot, and
Blender. Without those comparisons, its host-integration recommendations are weighted
toward browser rendering and omit important production consumers.

## Scope

Publish a second frozen 2026-08-04 revision and retain both earlier reports. Compare
geometry3Sharp directly with Plato's geometry and mesh library; treat the engines and
Blender as adjacent production environments; update the conclusions and documentation
index where these systems change the design recommendations.

## Done means

- [ ] geometry3Sharp is compared as an executable C# geometry and dynamic-mesh library.
- [ ] Unity, Unreal Engine, and Godot receive distinct scene, mesh, execution, and
      integration comparisons.
- [ ] Blender is compared as a dynamic-topology and procedural-authoring environment.
- [ ] The report distinguishes direct algorithm references, runtime consumers, and
      authoring hosts rather than presenting one generic engine category.
- [ ] New claims use official sources, local links resolve, and the index retains every
      frozen revision.

## Verification

- Audit repository-local links from the report directory.
- Open every new external citation from the official project source.
- Run `git diff --cached --check` before each commit.
