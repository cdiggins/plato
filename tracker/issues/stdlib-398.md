---
id: stdlib-398
title: Law packet moved into stdlib/ breaks the checker ratchet
type: bug
status: ready
priority: p2
effort: S
risk: low
area: stdlib
sprint: 
created: 2026-08-02
links: []
---

## Symptom

`tools/check-stdlib-fast.ps1` fails its second gate:

```
Forward stdlib checker completeness regressed.
  Expected: less than or equal to 0
  But was:  3
```

The three diagnostics are all in `stdlib/tests/polyhedra.laws.plato`:

```
CHK201  Law_PlatonicDualIsInvolution     No overload of 'Equals' matches (PlatonicSolidKind, PlatonicSolidKind)
CHK201  Law_ArchimedeanDualIsInvolution  No overload of 'Equals' matches (ArchimedeanSolidKind, ArchimedeanSolidKind)
CHK201  Law_CatalanDualIsInvolution      No overload of 'Equals' matches (CatalanSolidKind, CatalanSolidKind)
```

## Cause

The forward law packet was relocated from `tests/stdlib-tests/` to `stdlib/tests/`.
`CheckerTestSupport.PlatoFiles` enumerates with `SearchOption.AllDirectories`
(`tests/PlatoTests/CheckerTestSupport.cs`), so anything placed under `stdlib/` is now part
of the ratchet's corpus. The three law diagnostics pre-date the move — they were simply
outside the gate's scope before it.

The move also contradicts what the packet's own README states: laws are "never merged into
`stdlib/`", they are merged into a temporary folder by `tools/regen-forward-conformance.ps1`.
A folder physically inside `stdlib/` is merged by every recursive consumer whether it wants
to be or not.

## Also left behind by the same move

- `tests/Plato.Navigation.Tests/Corpus.cs` still lists `tests/stdlib-tests` as an index root.
  That folder now holds no `.plato` files, and git does not track empty directories, so a
  fresh clone has no such path at all.
- `tools/record-gates.py` passes `tests/stdlib-tests` to the codegen step, so the law packet
  is no longer handed to the conformance generation.
- The Plato navigation MCP server's default roots are `stdlib` and `tests/stdlib-tests`; the
  second root now fails to resolve on a fresh checkout.

## Fix options

1. Move the packet back out of `stdlib/` (to `tests/stdlib-tests/` or a sibling of `stdlib/`)
   and update the three consumers above to whichever path wins. Restores the scope split the
   README describes and the ratchet ceiling of 0 with no library change.
2. Keep it inside `stdlib/` and give the sum-kind types an `Equals`, so the three laws
   type-check. This is the larger fix: `PlatonicSolidKind` / `ArchimedeanSolidKind` /
   `CatalanSolidKind` are sum types and the checker finds no `Equals` overload for them, which
   is a gap that affects any law comparing two sum values, not just these three.
3. Keep it inside `stdlib/` and teach `CheckerTestSupport` to skip `stdlib/tests`. Cheapest,
   but it hides the packet from every recursive consumer by special case rather than by layout.

Option 1 matches the documented design.

## Done means

- [ ] `tools/check-stdlib-fast.ps1` passes all three gates with the ratchet ceiling still at 0
- [ ] `Corpus.cs`, `record-gates.py` and the MCP default roots name a path that exists
- [ ] The law packet's README and the location it describes agree
