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

- [x] Generic constructor payloads do not require an abstract receiver dispatch license; unresolved or unlicensed calls nested in constructor arguments remain rejected.
- [x] The original concrete-first Integer/Boolean helper regressions pass on checker, interpreter, CSharp and differential; focused fallback safety controls pass.
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

## Verified constructor fix (2026-10-25)

The production change excludes EmissionKind.Constructor only from the receiver
license check. AllNodes still rejects unresolved arguments and nested unlicensed
dispatch. Regression tests are added to Plato.Interpreter.Tests; existing function
constraint controls remain unchanged in PlatoTests.

Plato-brep commit 638430f3 preserves private baseline04 and candidate04. The
baseline reproduced four specific failures with eight passing controls/tests;
the candidate passed all twelve across separate main and controls test hosts.
Inputs, packages, native runtime and loaded assembly identities remained fixed.
Candidate test receipt:
68af7ec76d172594487535ad2cc14aa3e5924fc2930da27258a4047116c79629.
Greenhouse inspected the changed predicate and writer fallback callers; optimizer
evidence/review and freshness checks are current, with six tooling tests passing.

The final acceptance remains open: independently build the committed production
compiler, verify the actual consumer helper, and move the plato-brep pin only
after regeneration/consumer verification. Private success is not that final gate.
The function-field half of TKT-398 remains separate and open.
