---
id: compiler-518
title: Open generic constructor payload is mistaken for a dispatch receiver
type: bug
status: in-progress
priority: p1
effort: S
risk: med
area: compiler
sprint:
created: 2026-10-25
closed:
links: [plato-brep TKT-398]
---

## Done means

- [ ] Generic constructor payloads do not require an abstract receiver dispatch license; unresolved or unlicensed calls nested in constructor arguments remain rejected.
- [ ] The original concrete-first Integer/Boolean helper regressions pass on checker, interpreter, CSharp and differential; focused fallback safety controls pass.
- [ ] The committed compiler and pinned plato-brep consumer are verified independently of the private candidate.

## Reproduction and scope

Plato-brep b41018a8 preserves TKT-398's current reproduction against actual fc4b5
production assemblies: checker and interpreter pass, CSharp and differential fail
with the no-ground-TIR CountedFlips stub. The first non-ground node is the
GenericCounted($T,Integer) constructor. Inputs and resolved packages are unchanged.
Test receipt53e0b30fb4dd7d21003f917676d99239beb63d88f0a49936db6d7d06ea9af100.

Source investigation finds IsOpenGenericEmittable interprets argument zero of
every TirCall as a receiver, including constructor payloads. The staged candidate
excludes only EmissionKind.Constructor from receiver licensing and preserves the
AllNodes unresolved/nested dispatch checks. No production fix is promoted yet.
This enables honest open-generic emission, not fabricated fully ground TIR.
The separate TKT-398 record-function-field dispatch issue is outside this patch.
