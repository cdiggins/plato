---
id: plato-388
title: Single verification policy manifest: one data file the gates, tests and plato_check all read
type: debt
status: ready
priority: p1
effort: M
risk: med
area: plato
sprint: 
created: 2026-08-01
closed:
links: [stdlib/VERIFICATION.md, docs/verification-inventory.md, tools/check-stdlib-fast.ps1, tools/stage-stdlib.ps1, tools/record-gates.py, tools/export-types-context.ps1, tests/PlatoTests/CheckerTestSupport.cs]
---

## Issue

Which stdlib tiers a given check covers is a policy decision that lives in source, once per
tool. Each of these carries its own copy of the shipping-tier list and its own expression of the
`future` exclusion rule (stdlib-377):

- `tools/check-stdlib-fast.ps1` — `$tiers` literal
- `tools/stage-stdlib.ps1` — `$tiers` literal
- `tools/export-types-context.ps1` — `$Tiers` literal
- `tools/record-gates.py` — `TIERS` / `FUTURE_TIER`
- `tests/PlatoTests/CheckerTestSupport.cs` — `ShippingTiers` / `AllTiers`
- studio `tools/check-all.ps1` — inline literal
- studio `tools/regen-forward-conformance.ps1` — expressed differently again, as a
  directory-name filter over the merged input rather than a tier list

Two further facts are also duplicated: the type-checker ratchet ceiling exists both as the
constant in `ForwardStdLibCheckerTests` and as the `--ratchet` launch argument of the navigation
MCP server (plato-389), and the codegen recipe flag list exists in both `tools/record-gates.py`
and studio `tools/regen-forward-conformance.ps1`.

Nothing checks that these copies agree. `stdlib/VERIFICATION.md` describes the intended policy
accurately but is prose — no tool reads it. The visible symptom is that the document needs a
section explaining why four tools legitimately report four different lint counts.

## Approach

Add one tracked data file — `stdlib/verification.json` — declaring the policy as a matrix of
tier against check, plus the ratchet ceilings and the codegen recipe. The `future` rule and any
cumulative "foundation only" scope both fall out of that matrix rather than being separate
features (see plato-390).

Every consumer reads it: PowerShell via `ConvertFrom-Json`, Python via `json.load`, the PlatoTests
fixtures by resolving it through the existing `CheckerTestSupport.FindFolder` walk-up, and the MCP
server from its root.

Moving the ratchet ceilings into the manifest contradicts the current rule in `VERIFICATION.md`
that the test constant is the only copy. That rule was written to stop a second copy drifting;
the manifest achieves the same end by removing the second copy instead, and it is what lets
plato_check stop carrying its own. Update the document in the same change.

## Simplest implementation

Manifest plus consumers, no code moves. Then a fixture asserting no tier list survives outside
the manifest, so the duplication cannot grow back.

## Done means

- [ ] `stdlib/verification.json` exists and declares tiers, per-tier checks, ratchet ceilings and the codegen recipe.
- [ ] Every consumer listed above reads it; no tier list literal remains in any of them.
- [ ] The type-checker ceiling and the codegen recipe each have exactly one copy.
- [ ] A test fails if a tier list or ceiling reappears outside the manifest.
- [ ] `stdlib/VERIFICATION.md` points at the manifest instead of restating the scope tables.
