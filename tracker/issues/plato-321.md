---
id: plato-321
title: Burn down forward-stdlib LINT001 interface obligations (228 remaining)
type: debt
status: in-progress
priority: "?"
effort: "?"
risk: "?"
area: plato
sprint: 
created: 2026-07-29
closed:
links: [submodules/Plato/stdlib, .temp/plato-unimplemented-obligations.md]
---

## Issue

Forward stdlib (`submodules/Plato/stdlib/`) has **228** LINT001 warnings: concrete types claim concept members with no library body, so generated C# would throw `NotImplementedException`. Baseline before this session was **260**; **32** were fixed 2026-07-29 (images Width/Height, graphs, easing Eval, Hash batches). Full remaining inventory: `.temp/plato-unimplemented-obligations.md`.

## Impact

Largest clusters: `Procedural.Eval` (46), `Additive` (20), `MatrixLike` (18), `Scalable` (17), `Image` (0 after batch), spatial indexes, probability distributions, mesh counts. Blocks trustworthy forward-stdlib codegen (Plato.ForwardConformanceTests stage 2).

## Affected code

- `submodules/Plato/stdlib/**/*.library.plato` — missing per-type concept bodies
- `PlatoCompiler/Analysis/Linter.cs` — LINT001 definition
- Inventory: `.temp/plato-unimplemented-obligations.md`

## Priority

**Medium-high** — warnings only (lint `--strict` passes), but each obligation is a runtime throw if hit. Work in domain batches; ratchet gate must not regress.

## Done means

- [ ] LINT001 count on `lint submodules/Plato/stdlib` is 0 (or agreed ceiling documented)
- [ ] `check-stdlib-fast.ps1` green after each batch; lower checker ratchet ceiling when count drops
- [ ] No new LINT012 receiver-marker disagreements introduced

## Simplest fix

Continue domain-batch fills in sibling `*.library.plato` files (same pattern as session 2026-07-29): trivial field projections first, then shared generic fills where one body covers many types (e.g. `Hashable` on `Index` already in `collections-indexable.library.plato`).
