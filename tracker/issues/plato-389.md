---
id: plato-389
title: plato_check has no tier awareness: future and stdlib-tests count toward its verdict
type: bug
status: ready
priority: p2
effort: S
risk: low
area: plato
sprint: 
created: 2026-08-01
closed:
links: [docs/verification-inventory.md, tracker/issues/plato-388.md, studio labs/PlatoNavigationMcp/CheckMcpTools.cs, studio labs/PlatoNavigationMcp/Program.cs]
---

## Issue

`plato_check` (studio `labs/PlatoNavigationMcp/CheckMcpTools.cs`) is the warm inner-loop gate
agents run after every edit. It has no concept of a tier: it lints and style-checks whatever the
server was given as `--root`, which in practice is all four `stdlib/` tiers plus
`tests/stdlib-tests/`, and folds every finding into the single `ok` verdict it returns.

Two consequences.

The inner loop holds `stdlib/future` to a bar no gate enforces. Under stdlib-377 that tier only
has to parse and type-check; it is not linted and not converted to C#. An agent editing a
shipping tier therefore sees findings it must not act on, mixed in with findings it must, and
nothing in the response distinguishes them. `stdlib/VERIFICATION.md` currently documents this as
intended ("its numbers will not match the ratchet ceilings, by design") — that is a description
of the defect, not a design.

Separately, the type-checker ratchet ceiling reaches the server as the `--ratchet` launch
argument in `Program.cs`, making it a second copy of
`ForwardStdLibCheckerTests.MaxFunctionsWithDiagnostics`. The comment there notes the default
happens to match. Nothing enforces that it keeps matching.

## Approach

Tag every finding with the tier its file came from, resolved against the manifest in plato-388.
Exclude non-shipping tiers and the law packet from the `ok` verdict and report them in a separate
informational block, so the inner-loop verdict means what the gates mean. Take the ceiling from
the manifest and drop the `--ratchet` argument.

## Simplest implementation

Tier resolution is a path prefix match against the manifest. It does not require the server to
change how it indexes or compiles — the whole corpus still compiles as one program, which is
correct and is what catches a shipping tier reaching into `future`.

## Done means

- [ ] `plato_check` findings carry the tier they came from.
- [ ] Its verdict covers the shipping tiers only; other tiers report as informational.
- [ ] The ratchet ceiling comes from the manifest; `--ratchet` is gone.
- [ ] `stdlib/VERIFICATION.md` no longer has to explain away a corpus mismatch here.
