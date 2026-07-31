---
date: 2026-07-31
title: Delete the V1 runtime and its freeze; Plato.Intrinsics.V2 becomes Plato.Intrinsics
status: accepted
superseded-by:
links: [./2026-07-30-retire-legacy-conformance-and-goldens.md, ../../docs/plato-library-map.md]
---

## Context

The repo carried two handwritten runtimes side by side. `Plato.Intrinsics.Legacy` was the **V1**
shape — wrapper scalars (`Number` wrapping a `float`), members emitted as properties — kept
byte-frozen because Ara3D.Studio ships a copy of it. `Plato.Intrinsics.V2` was the live one:
native `float`, extension methods, `System.Numerics`-backed.

The freeze was enforced from *another repository*: `studio/tools/check-frozen-v1.ps1` against
`studio/tools/frozen-v1.sha256`, wired into `check-all.ps1`. That machinery had already gone stale —
its manifest paths still read `submodules/Plato/Plato.Intrinsics.Legacy/...`, which the July
directory restructure invalidated, so the gate would fail on paths before it ever hashed anything.
The `.gitattributes` `-text` rule that existed only to keep those bytes stable had a known LF/CRLF
hazard recorded in `CURRENT_STATUS.md`.

Carrying a frozen second copy cost a `.gitattributes` rule, a cross-repo checksum gate, a hard rule
in `AGENTS.md`, a whole axis in `docs/plato-library-map.md`, and a "V2" suffix on every live name.

## Decision

Delete V1 from this repo and drop the version suffix from the survivor.

- `legacy/Plato.Intrinsics.Legacy/` — deleted (24 files).
- `src/Plato.Intrinsics.V2/` — renamed `src/Plato.Intrinsics/`, along with its `.shproj` and
  `.projitems`. Root namespace was already `Plato.Intrinsics`; code namespace `Ara3D.Geometry` is
  unchanged.
- `IntrinsicsV2SurfaceTests` / `IntrinsicsV2TestShims` — renamed without the `V2`.
- The three projects that imported the V1 `.projitems` — `experiments/csg`, `experiments/earcut`,
  `tests/optimizer-smoke/smoke.props` — repointed at `src/Plato.Intrinsics`. All three already
  generated with the V2 recipe (`--csharp-style=extensions --scalar=float --methods`), so they were
  importing a runtime that did not match their generated code.
- `.gitattributes` — deleted; its only content was the freeze rule.
- Freeze guidance removed from `AGENTS.md` (hard rule 2, the gate list, the layout entry),
  `docs/plato-for-agents.md`, `docs/plato-library-map.md` (rewritten), `CURRENT_STATUS.md`.

Dated plan documents keep their V1/V2 language: they are historical records of the migration.

## Consequences

**Studio still ships V1.** `ara3d-sdk/src/Plato.Generated`, `ara3d-sdk/src/Plato.Intrinsics` and the
`Ara3D.Geometry` library that references them were deliberately not touched — they live in another
repository. This decision removes the *duplicate* and the guard, not the shipping artifact. Retiring
those is an adoption project on the ara3d-sdk side.

**The studio gate will fail until it is removed there.** `check-frozen-v1.ps1` now hashes files that
do not exist. Deleting the script, the manifest and its `check-all.ps1` row is follow-up work in the
studio repo.

**Not verified by a build.** The rename and the import repointing are mechanical but were not
compiled; the solution has separate pre-existing breakage (stale cross-folder `ProjectReference`
paths from the restructure) that makes a green build impossible right now.
