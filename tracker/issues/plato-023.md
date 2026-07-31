---
id: plato-023
title: TIR scalar lowering pass + type-checker completion
type: feature
status: done
priority: p1
effort: M
risk: med
area: plato
sprint: 
created: 2026-07-16
closed: 2026-07-31
links: [docs/plato-tir-scalar-lowering-plan-2026-07-12.md, plato-369, plato-370]
---

Post-C4 endgame (original): finish type checker, turn `--scalar` erasure into a TIR lowering
pass, reduce `TirCSharpBodyWriter` to a type-directed pretty-printer.

**Split 2026-07-31:** Mission 2 (scalar lowering S0–S3) is done in tree. Remaining work moved to:

- [plato-369](plato-369.md) — Mission 1: burn checker diagnostics to 0 (ceiling 25 today).
- [plato-370](plato-370.md) — S4: `--scalar=double` map + double intrinsics.

## Done means

- [x] `--scalar` erasure runs as `TirScalarLowerer` (type substitution + overload-aware coercions).
- [x] `TirCSharpBodyWriter` is type-directed under scalar erasure (no emit-time scalar analysis).
- [x] `ScalarEraseAnalysis` deleted; un-lowerable bodies fail loudly.
- [x] `TirTypeVerifier` hard violations at 0 (Mission 2 unblocked and stayed green).
- [ ] Type-checker diagnostics at 0 — **moved to plato-369**.
- [ ] `--scalar=double` — **moved to plato-370**.
