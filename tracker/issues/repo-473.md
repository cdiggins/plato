---
id: repo-473
title: Resolve external SDK frozen-artifact gate drift
type: problem
status: idea
priority: p2
effort: S
risk: med
area: repo
sprint: 
created: 2026-10-23
closed:
links: []
---

The required studio integration battery fails its frozen-V1 gate independently of Plato source organization. `C:/Users/cdigg/git/studio/tools/check-frozen-v1.ps1` reports changed SDK runtime files `PlatoList.cs`, `PlatoBuffer.cs` and `Number.cs` under `studio/ara3d-sdk/src/Plato.Intrinsics`. The repo-468 mission changed none of those paths. Both standard-library lints, the external SDK build and its geometry tests pass in the same battery.

Determine whether the external SDK changes are intentional and reconcile them with the studio manifest through that repository's review process. Do not silently refresh the manifest to make this checkout's structural cleanup green. This issue tracks a cross-checkout validation blocker, not a regression in Plato's compiler.
