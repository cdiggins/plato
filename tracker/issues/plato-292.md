---
id: plato-292
title: Decide assert-vs-validation policy: Debug and Release disagree about what compiles
type: problem
status: idea
priority: p2
effort: M
risk: low
area: plato
sprint: 
created: 2026-07-28
closed:
links: [submodules/Plato/PlatoCompiler/Analysis/FunctionInstance.cs, submodules/Plato/PlatoCompiler/Utilities, tracker/issues/plato-289.md]
---

## Issue
The compiler's internal invariants are enforced by two mechanisms with opposite Release-mode
behavior: `Debug.Assert` (elided in Release) and `Verifier.Assert*` (Ara3D.Utils, always
throws). No policy says which to use where, so Debug and Release builds disagree about what
compiles. plato-289 was one instance: `Debug.Assert(CountTypeVars(first.Type) == 0)` at
`FunctionInstance.cs:143` made the forward stdlib fail to complete compilation under Debug
while Release compiled it cleanly all along. 37 `Debug.Assert`/`Verifier.Assert` call sites
remain across 9 PlatoCompiler files; any of the `Debug.Assert` ones could be the next
configuration-dependent surprise (or the next over-strict assumption a broader stdlib
legitimately violates).

## Impact
- CI/local and Debug/Release can disagree silently: a Debug-only failure is invisible to any
  Release gate (`check-all.ps1` runs lint in Release), and vice versa an always-on Verifier
  throw can crash production paths for a condition that deserved a located diagnostic.
- Every future compiler-hardening effort inherits ambiguity: is a firing assert a compiler bug,
  a library bug, or an over-strict assumption? plato-289 took a full diagnosis cycle to answer.

## Affected code
- `PlatoCompiler/Analysis/FunctionInstance.cs` — 8 sites (one already removed by plato-289).
- `PlatoCompiler/Types/ReifiedType.cs` — 12 sites, the densest file.
- `PlatoCompiler/Analysis/InterfaceImplementation.cs` — 6; `ConcreteType.cs` — 3; `SymbolFactory.cs`, `VisualSyntaxGraphQueries.cs`, `FunctionArgAnalysis.cs` — 2 each; `Compilation.cs`, `Symbols/Definitions.cs` — 1 each.

## Cause / analysis
Organic growth: `Verifier.Assert*` came with Ara3D.Utils habits, `Debug.Assert` with BCL
habits. What makes the question hard: three genuinely different intents are collapsed into two
mechanisms — (a) true internal invariants (should throw always, caught by Compilation's now-
reporting catch-all), (b) assumptions about input shape that a library may legitimately violate
(should be located semantic errors, never asserts), (c) cheap sanity checks in hot paths where
Release elision is the point. plato-289 was a (b) miscoded as (c).

## Priority
p2. It compounds — every new stdlib shape (forward vocabulary growth, sum types, ports) walks
past all 37 sites — but nothing is on fire now that Compilation reports what it catches, which
converts silent divergence into a diagnosed one. Closing needs an ADR, not urgency.

## Dependencies
- Blocked by: nothing. plato-289 (closed) provides the reporting substrate.
- Touches: PlatoCompiler broadly — coordinate with concurrent compiler sessions.

## Fix approaches
1. **Policy ADR + mechanical sweep.** Decide: internal invariants use always-on `Verifier`
   (failures now reported via InternalErrors); input-shape assumptions become located semantic
   errors; keep `Debug.Assert` only for hot-path checks whose elision is deliberate, each with a
   comment saying so. Sweep all 37 sites into a bucket.
2. **Minimal:** ADR only, converting sites opportunistically as they are touched. Cheaper, but
   the Debug/Release divergence persists at unswept sites indefinitely.

## Bedrock
The invariant to establish: *a compilation's outcome is configuration-independent* — the same
input either compiles in both Debug and Release or fails in both with the same diagnostics.
The seam is the 37 assert sites; the enforcement is Compilation's failure-implies-diagnostic
guarantee from plato-289 plus a two-config lint comparison in CI. Verdict: **right** — option 1;
option 2 restates the problem per-site instead of retiring it.

## Done means
- [ ] ADR in tracker/decisions/ naming the three buckets and the rule for each
- [ ] all 37 sites classified and converted (or explicitly annotated as deliberate Debug-only)
- [ ] a gate compares Debug and Release lint output on both stdlibs and fails on divergence
- [ ] no remaining site where a library-shape assumption is enforced by an elidable assert

## Simplest fix
Option 2 (ADR only). Pro: an afternoon, immediately gives future sessions the rule. Con: the
existing 37 sites keep their divergence potential until organically touched; the plato-289
class of bug stays possible.

## Prevention
This IS the prevention issue for plato-289's class. The two-config lint-comparison gate (Done
means, third box) is the mechanical guard; candidate for check-all.ps1 alongside the strict
lint gates added by plato-290.
