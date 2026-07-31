---
id: plato-329
title: Writer emission-completeness assertion: every reachable library function must be emitted somewhere
type: debt
status: ready
priority: p3
effort: M
risk: low
area: plato
sprint: 
created: 2026-07-29
closed:
links: [tracker/issues/plato-323.md, submodules/Plato/Plato.CSharpWriter/CSharpWriter.cs]
---

## Issue

plato-323's biggest cluster (~350 CS1061) happened because library functions with an `Array<T>`
receiver were emitted NOWHERE — declared, type-checked, linted clean, then silently absent from
the output, discovered only as hundreds of downstream call-site errors. The specific hole is fixed
(Plato `e69a69d`), but the CLASS has no gate: nothing asserts that every reachable monomorphized
library function actually landed in some generated file.

## Fix approach

Post-`WriteAll` assertion in `Plato.CSharpWriter`: collect the set of library functions the
monomorphizer instantiated (or that call sites reference), subtract those written to any output,
and fail codegen naming each orphan (function, receiver type, declaring library). Body-less
intrinsics owned by the handwritten runtime need an explicit allowlist or marker so they don't
false-positive.

## Bedrock

Closes the compilation-to-emission seam — the one place (with the stdlib/runtime seam,
[plato-330](plato-330.md)) where errors still get through to the C# build. Any future
`IgnoredTypes`-shaped hole becomes a codegen error with a precise message instead of a CS1061
fan-out. Verdict: **right**.

## Done means

- [ ] Assertion in the writer, on by default for all recipes.
- [ ] The `Array<T>` hole, if reintroduced, fails codegen with the orphan named (regression test).
- [ ] Legacy + forward generations still succeed (allowlist covers legitimate body-less intrinsics).
