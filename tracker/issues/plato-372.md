---
id: plato-372
title: Studio gate scripts are stale: check-stdlib-fast lints stdlib top-only and finds no files
type: bug
status: ready
priority: p2
effort: S
risk: low
area: plato
sprint: 
created: 2026-07-31
closed:
links: []
---

## Issue

`C:\Users\cdigg\git\studio\tools\check-stdlib-fast.ps1` runs

```
Plato.CLI lint <root>\submodules\Plato\stdlib --strict
```

and `Plato.CLI` enumerates each input root **top-directory-only by design**
(`src/Plato.CLI/Program.cs`, `GetPlatoFiles`). Since the tier reorg partitioned `stdlib/` into
`foundation/ geometry/ graphics/ future/`, that root holds no `.plato` file at all, so the gate
dies with `No .plato files found in ...\stdlib` and reports FAIL — 394s spent proving nothing.
`regen-forward-conformance.ps1` already learned this (it enumerates `-Recurse` and says so in a
comment); the fast gate did not.

Measured 2026-07-31: `lint --strict` over the four tier folders exits 0 with 0 parse and 0
resolution errors, 2792 findings (229 Warning / 2563 Info).

The same checkout is stale in a second way: `studio\submodules\Plato` is still pre-restructure
(`PlatoCompiler\`, `Plato.CLI\`, `Plato.Intrinsics.V2\`), so every gate path in those scripts
points at directories this repo renamed (`src/`, `writers/`, `tests/`, `legacy/`).

## Fix

**This repo's own copy is already correct** — `plato\tools\check-stdlib-fast.ps1` names the four
tier folders (fixed at the tier split, `36089e4`) and takes `-Folders` for a subset. It is the
gate to run. The stale one is `studio\tools\check-stdlib-fast.ps1`, which is a separate file that
was never updated and drives a separate, pre-restructure `submodules\Plato` checkout.

So: port this repo's version over the studio one (or delete the studio copy and have it call
through), and re-point every Plato path in `studio\tools\*.ps1` at the post-restructure layout
(`src/`, `writers/`, `tests/`, `legacy/`). Nothing to change here.

## Done means

- [ ] `studio\tools\check-stdlib-fast.ps1` passes both gates against a current Plato checkout.
- [ ] Every path in `studio\tools\*.ps1` that names a Plato project resolves after the restructure.
